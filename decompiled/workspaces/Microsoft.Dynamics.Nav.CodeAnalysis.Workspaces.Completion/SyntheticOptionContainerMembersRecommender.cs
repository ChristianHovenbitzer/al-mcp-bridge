using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Completion;

internal class SyntheticOptionContainerMembersRecommender : ContextAwareSymbolRecommender
{
	internal SyntheticOptionContainerMembersRecommender(MemberSyntaxContext context)
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
		if (parent.Kind != SyntaxKind.OptionAccessExpression)
		{
			return await base.RecommendSymbolsAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		}
		CodeExpressionSyntax expression = ((OptionAccessExpressionSyntax)parent).Expression;
		if (!TryGetOptionValuesFromSyntheticOptionSymbol(expression, cancellationToken, out IEnumerable<ISymbol> symbols))
		{
			return await base.RecommendSymbolsAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		}
		return symbols;
	}

	private bool TryGetOptionValuesFromSyntheticOptionSymbol(CodeExpressionSyntax optionExpression, CancellationToken cancellationToken, out IEnumerable<ISymbol> symbols)
	{
		symbols = SpecializedCollections.EmptyEnumerable<ISymbol>();
		if (optionExpression.Kind == SyntaxKind.IdentifierName)
		{
			return TryGetOptionValuesFromIdentifier((IdentifierNameSyntax)optionExpression, cancellationToken, ref symbols);
		}
		if (optionExpression.Kind == SyntaxKind.MemberAccessExpression)
		{
			return TryGetSymbolsFromQueryFilterSymbol((MemberAccessExpressionSyntax)optionExpression, cancellationToken, ref symbols);
		}
		return false;
	}

	private bool TryGetOptionValuesFromIdentifier(IdentifierNameSyntax originalExpression, CancellationToken cancellationToken, ref IEnumerable<ISymbol> symbols)
	{
		TypeInfo typeInfo = base.Context.SemanticModel.GetTypeInfo(originalExpression, cancellationToken);
		if (typeInfo.Type == null)
		{
			return false;
		}
		if (typeInfo.Type.IsArray())
		{
			return TryGetOptionValuesFromArrayOfOptions(originalExpression, typeInfo.Type, cancellationToken, ref symbols);
		}
		return TryGetOptionValuesFromQueryFilterAccessedDirectlyInQuery(originalExpression, cancellationToken, ref symbols);
	}

	private bool TryGetOptionValuesFromArrayOfOptions(IdentifierNameSyntax originalExpression, ITypeSymbol expressionType, CancellationToken cancellationToken, ref IEnumerable<ISymbol> symbols)
	{
		TypeSymbol elementType = ((ArrayTypeSymbol)expressionType).ElementType;
		if (elementType.Kind != SymbolKind.OptionType)
		{
			return false;
		}
		LookupOptions lookupOptions = elementType.GetLookupOptions() | LookupOptions.MustBeOption;
		SemanticModel semanticModel = base.Context.SemanticModel;
		int spanStart = originalExpression.SpanStart;
		IContainerSymbol container = elementType;
		symbols = semanticModel.LookupSymbols(spanStart, lookupOptions, container, null, SymbolKind.Undefined, cancellationToken);
		return true;
	}

	private bool TryGetOptionValuesFromQueryFilterAccessedDirectlyInQuery(IdentifierNameSyntax expression, CancellationToken cancellationToken, ref IEnumerable<ISymbol> symbols)
	{
		if (base.Context.SemanticModel.GetTypeInfo(expression, cancellationToken).Type != NavCorLib.QueryFilterType)
		{
			return false;
		}
		QueryTypeSymbol queryTypeSymbol = base.Context.SemanticModel.GetEnclosingSymbol(expression.SpanStart, cancellationToken)?.GetContainingSymbolOfType<QueryTypeSymbol>();
		if (queryTypeSymbol == null)
		{
			return false;
		}
		return TryGetOptionValuesFromQueryFilterSymbol(expression, expression.Unquoted(), queryTypeSymbol, cancellationToken, ref symbols);
	}

	private bool TryGetSymbolsFromQueryFilterSymbol(MemberAccessExpressionSyntax originalExpression, CancellationToken cancellationToken, ref IEnumerable<ISymbol> symbols)
	{
		ExpressionSyntax node = originalExpression.WalkDownParentheses();
		if (base.Context.SemanticModel.GetTypeInfo(node, cancellationToken).Type != NavCorLib.QueryFilterType)
		{
			return false;
		}
		ExpressionSyntax node2 = originalExpression.Expression.WalkDownParentheses();
		TypeInfo typeInfo = base.Context.SemanticModel.GetTypeInfo(node2, cancellationToken);
		ITypeSymbol type = typeInfo.Type;
		if (type == null || type.NavTypeKind != NavTypeKind.Query)
		{
			ITypeSymbol type2 = typeInfo.Type;
			if (type2 == null || type2.NavTypeKind != NavTypeKind.CurrQuery)
			{
				return false;
			}
		}
		return TryGetOptionValuesFromQueryFilterSymbol(originalExpression, originalExpression.Name.Unquoted(), typeInfo.Type, cancellationToken, ref symbols);
	}

	private bool TryGetOptionValuesFromQueryFilterSymbol(SyntaxNode originalExpression, string filterName, ITypeSymbol querySymbol, CancellationToken cancellationToken, ref IEnumerable<ISymbol> symbols)
	{
		LookupOptions lookupOptions = querySymbol.GetLookupOptions();
		SemanticModel semanticModel = base.Context.SemanticModel;
		int spanStart = originalExpression.SpanStart;
		IContainerSymbol container = querySymbol;
		CancellationToken token = cancellationToken;
		QueryFilterSymbol queryFilterSymbol = semanticModel.LookupSymbols(spanStart, lookupOptions, container, filterName, SymbolKind.Undefined, token).FirstOrDefault((ISymbol s) => s.Kind == SymbolKind.QueryFilter) as QueryFilterSymbol;
		if (!(queryFilterSymbol == null))
		{
			FieldSymbol? relatedField = queryFilterSymbol.RelatedField;
			if ((object)relatedField != null && (relatedField.Type?.Kind).GetValueOrDefault() == SymbolKind.OptionType)
			{
				lookupOptions |= LookupOptions.MustBeOption;
				SemanticModel semanticModel2 = base.Context.SemanticModel;
				int spanStart2 = originalExpression.SpanStart;
				container = queryFilterSymbol.RelatedField.Type;
				token = cancellationToken;
				symbols = semanticModel2.LookupSymbols(spanStart2, lookupOptions, container, null, SymbolKind.Undefined, token);
				return true;
			}
		}
		return false;
	}
}
