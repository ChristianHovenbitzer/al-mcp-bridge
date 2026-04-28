using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Completion;

internal class ForEachContextSymbolRecommender : ContextAwareSymbolRecommender
{
	internal ForEachContextSymbolRecommender(MemberSyntaxContext context)
		: base(context)
	{
	}

	protected internal override async Task<IEnumerable<ISymbol>> RecommendSymbolsAsync(CancellationToken cancellationToken)
	{
		if (!(base.Context.EnclosingBinder.ContainingMember is IMethodSymbol))
		{
			return await base.RecommendSymbolsAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		}
		SyntaxToken leftToken = base.Context.LeftToken;
		if (!leftToken.TryGetAncestorOfKind<ForEachStatementSyntax>(SyntaxKind.ForEachStatement, out ForEachStatementSyntax ancestor))
		{
			return await base.RecommendSymbolsAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		}
		IdentifierNameSyntax iterationVariable = ancestor.IterationVariable;
		TypeSymbol variableType = null;
		if (iterationVariable != null)
		{
			BoundExpression boundExpression = base.Context.EnclosingBinder.BindExpression(iterationVariable, DiagnosticBag.GetNullInstance());
			if (!boundExpression.HasErrors)
			{
				variableType = boundExpression?.Type;
			}
		}
		BoundExpression boundExpression2 = base.Context.EnclosingBinder.BindExpression(ancestor.Expression, DiagnosticBag.GetNullInstance());
		TypeSymbol elementType = null;
		if (!boundExpression2.HasErrors && boundExpression2.Type.IsEnumerableType(base.Context.RuntimeVersion))
		{
			elementType = boundExpression2.Type.GetEnumeratorElementType_Internal();
		}
		if (leftToken.Kind == SyntaxKind.ForEachKeyword)
		{
			return GetIterationVariables(elementType, cancellationToken);
		}
		if (leftToken.Kind == SyntaxKind.InKeyword)
		{
			return GetEnumerableVariables(variableType, cancellationToken);
		}
		return await base.RecommendSymbolsAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
	}

	private IEnumerable<ISymbol> GetIterationVariables(ITypeSymbol? elementType, CancellationToken token)
	{
		ImmutableArray<ISymbol>.Enumerator enumerator = base.Context.SemanticModel.LookupSymbols(base.Context.LeftToken.EndPosition, LookupOptions.Default, null, null, SymbolKind.Undefined, token).GetEnumerator();
		while (enumerator.MoveNext())
		{
			ISymbol current = enumerator.Current;
			if (!current.IsImplicitlyDeclared && !current.IsSynthesized && (current.Kind == SymbolKind.GlobalVariable || current.Kind == SymbolKind.LocalVariable))
			{
				IVariableSymbol variableSymbol = (IVariableSymbol)current;
				if (elementType == null)
				{
					yield return variableSymbol;
				}
				else if (IsAssignable(elementType, variableSymbol.Type))
				{
					yield return variableSymbol;
				}
			}
		}
	}

	private IEnumerable<ISymbol> GetEnumerableVariables(ITypeSymbol? variableType, CancellationToken cancellationToken)
	{
		ImmutableArray<ISymbol>.Enumerator enumerator = base.Context.SemanticModel.LookupSymbols(base.Context.LeftToken.EndPosition, LookupOptions.Default, null, null, SymbolKind.Undefined, cancellationToken).GetEnumerator();
		while (enumerator.MoveNext())
		{
			ISymbol current = enumerator.Current;
			ITypeSymbol possibleCollectionType = GetPossibleCollectionType(current);
			if (possibleCollectionType == null || !possibleCollectionType.IsEnumerableType_Internal(base.Context.RuntimeVersion))
			{
				continue;
			}
			if (variableType == null)
			{
				yield return current;
				continue;
			}
			TypeSymbol enumeratorElementType_Internal = possibleCollectionType.GetEnumeratorElementType_Internal();
			if (enumeratorElementType_Internal != null && IsAssignable(enumeratorElementType_Internal, variableType))
			{
				yield return current;
			}
		}
	}

	private static ITypeSymbol? GetPossibleCollectionType(ISymbol symbol)
	{
		if (symbol.Kind == SymbolKind.GlobalVariable || symbol.Kind == SymbolKind.LocalVariable)
		{
			return ((IVariableSymbol)symbol).Type;
		}
		if (symbol.Kind == SymbolKind.Method)
		{
			return ((IMethodSymbol)symbol).ReturnValueSymbol?.ReturnType;
		}
		if (symbol.Kind == SymbolKind.Parameter)
		{
			return ((IParameterSymbol)symbol).ParameterType;
		}
		return null;
	}

	private bool IsAssignable(ITypeSymbol source, ITypeSymbol destination)
	{
		return base.Context.EnclosingBinder.Conversions.ClassifyConversionFromType((TypeSymbol)source, (TypeSymbol)destination).Exists;
	}
}
