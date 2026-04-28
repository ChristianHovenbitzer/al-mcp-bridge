using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Completion;

internal class ContainerContextSymbolRecommender : ContextAwareSymbolRecommender
{
	internal ContainerContextSymbolRecommender(MemberSyntaxContext context)
		: base(context)
	{
	}

	protected internal override async Task<IEnumerable<ISymbol>> RecommendSymbolsAsync(CancellationToken cancellationToken)
	{
		if (!base.Context.IsRightOfNameSeparator || base.Context.General.HasFlag(GeneralContexts.AnyComplexPropertyExpression))
		{
			return await base.RecommendSymbolsAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		}
		SyntaxNode parent = base.Context.TargetToken.Parent;
		OptionAccessExpressionSyntax ancestor = null;
		ExpressionSyntax expressionSyntax = null;
		switch (parent.Kind)
		{
		case SyntaxKind.MemberAccessExpression:
			expressionSyntax = ((MemberAccessExpressionSyntax)parent).Expression;
			break;
		case SyntaxKind.QualifiedName:
			if (parent.TryGetAncestorOfKind<OptionAccessExpressionSyntax>(SyntaxKind.OptionAccessExpression, out ancestor))
			{
				expressionSyntax = ((QualifiedNameSyntax)parent).Left;
			}
			break;
		case SyntaxKind.IdentifierName:
			if (parent.GetFirstToken().HasDotDotAsFirstTrivia())
			{
				expressionSyntax = (IdentifierNameSyntax)parent;
			}
			break;
		case SyntaxKind.OptionAccessExpression:
			ancestor = (OptionAccessExpressionSyntax)parent;
			expressionSyntax = ancestor.Expression;
			break;
		}
		if (expressionSyntax != null)
		{
			return GetSymbolsOffOfExpression(expressionSyntax, ancestor, cancellationToken);
		}
		return SpecializedCollections.EmptyEnumerable<ISymbol>();
	}

	private IEnumerable<ISymbol> GetSymbolsOffOfExpression(ExpressionSyntax originalExpression, OptionAccessExpressionSyntax? optionAccess, CancellationToken cancellationToken)
	{
		ExpressionSyntax expressionSyntax = originalExpression.WalkDownParentheses();
		SymbolInfo symbolInfo = base.Context.SemanticModel.GetSymbolInfo(expressionSyntax, cancellationToken);
		ISymbol? symbol = symbolInfo.Symbol;
		IContainerSymbol container;
		if (symbol != null && symbol.Kind == SymbolKind.Namespace)
		{
			container = (ContainerSymbol)symbolInfo.Symbol;
		}
		else
		{
			TypeInfo typeInfo = base.Context.SemanticModel.GetTypeInfo(expressionSyntax, cancellationToken);
			container = ((typeInfo != TypeInfo.None) ? typeInfo.Type : null);
		}
		SymbolKind symbolKind = SymbolKind.Undefined;
		if (optionAccess != null)
		{
			TypeInfo typeInfo2 = base.Context.SemanticModel.GetTypeInfo(optionAccess.Expression, cancellationToken);
			if (typeInfo2.Type.Kind == SymbolKind.Class)
			{
				IContainerSymbol containerSymbol = container;
				if (containerSymbol == null || containerSymbol.Kind != SymbolKind.Enum)
				{
					symbolKind = typeInfo2.Type.NavTypeKind.ToSymbolKind();
				}
			}
		}
		IEnumerable<ISymbol> symbolsOffOfBoundExpression = GetSymbolsOffOfBoundExpression(originalExpression, symbolInfo, container, symbolKind, cancellationToken);
		foreach (ISymbol item in symbolsOffOfBoundExpression)
		{
			if (!FilterBasedOnDependentProperties(container?.OriginalDefinition, item))
			{
				yield return item;
			}
		}
	}

	private IEnumerable<ISymbol> GetSymbolsOffOfBoundExpression(ExpressionSyntax originalExpression, SymbolInfo leftHandBinding, IContainerSymbol? container, SymbolKind symbolKind, CancellationToken cancellationToken)
	{
		if (leftHandBinding.CandidateReason == CandidateReason.NotReferencable)
		{
			return SpecializedCollections.EmptyEnumerable<ISymbol>();
		}
		ISymbol symbol = leftHandBinding.GetBestOrAllSymbols().FirstOrDefault();
		if (symbol != null && originalExpression.IsKind(SyntaxKind.ParenthesizedExpression) && symbol.IsKind(SymbolKind.NamedType))
		{
			return SpecializedCollections.EmptyEnumerable<ISymbol>();
		}
		if (container == null)
		{
			DebugAssertHelper.Assert(symbol == null, "Unexpected null container in " + originalExpression.ToReportingString());
			return SpecializedCollections.EmptyEnumerable<ISymbol>();
		}
		LookupOptions lookupOptions = symbol?.GetLookupOptions() ?? container.GetLookupOptions();
		int spanStart = originalExpression.SpanStart;
		if (base.Context.IsRightOfOptionAccess)
		{
			lookupOptions = LookupOptions.MustBeOption | LookupOptions.MemberAccess;
			if (container.Kind == SymbolKind.Class)
			{
				TypeSymbol typeSymbol = (TypeSymbol)container;
				IEnumerable<ISymbol> symbols;
				if (typeSymbol.TypeCategory == TypeCategoryKind.ApplicationObject && typeSymbol.OriginalDefinition != null)
				{
					symbolKind = ((TypeSymbol)container).NavTypeKind.ToSymbolKind();
					container = null;
				}
				else if (container.TryGetSymbolsFromOptionType(base.Context.SemanticModel.Compilation, spanStart, cancellationToken, out symbols))
				{
					return symbols;
				}
			}
			else if (container.Kind == SymbolKind.Enum && EnumContextSymbolRecommender.IsNestedEnumValue(base.Context))
			{
				return SpecializedCollections.EmptyEnumerable<ISymbol>();
			}
		}
		if (symbolKind != SymbolKind.Undefined)
		{
			lookupOptions = LookupOptions.MustBeObjectTypeOrNamespaceSymbol | LookupOptions.CompilationUnit;
		}
		else if (originalExpression.Kind == SyntaxKind.ThisExpression)
		{
			lookupOptions |= LookupOptions.ThisAccess;
		}
		SemanticModel semanticModel = base.Context.SemanticModel;
		IContainerSymbol container2 = container;
		return semanticModel.LookupSymbols(spanStart, lookupOptions, container2, null, symbolKind, cancellationToken);
	}

	private static bool FilterBasedOnDependentProperties(ISymbol? container, ISymbol symbol)
	{
		if (container == null)
		{
			return false;
		}
		if (!symbol.IsBuiltInMethodSymbol())
		{
			return false;
		}
		foreach (DependentProperty dependentProperty in ((BuiltInMethodTypeSymbol)symbol).DependentProperties)
		{
			if (!SymbolWithPropertiesVisitor.IsPropertyDependencySatisfied((Symbol)container, dependentProperty))
			{
				return true;
			}
		}
		return false;
	}
}
