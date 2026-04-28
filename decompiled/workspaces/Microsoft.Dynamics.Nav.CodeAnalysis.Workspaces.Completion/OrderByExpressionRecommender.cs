using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Completion;

internal class OrderByExpressionRecommender : ContextAwareSymbolRecommender
{
	private readonly OrderByExpressionSyntax orderByExpression;

	internal OrderByExpressionRecommender(MemberSyntaxContext context)
		: base(context)
	{
		orderByExpression = Microsoft.Dynamics.Nav.CodeAnalysis.Syntax.SyntaxTokenExtensions.GetAncestor<OrderByExpressionSyntax>(base.Context.TargetToken);
	}

	protected internal override Task<IEnumerable<ISymbol>> RecommendSymbolsAsync(CancellationToken cancellationToken)
	{
		if (orderByExpression == null)
		{
			return Task.FromResult(SpecializedCollections.EmptyEnumerable<ISymbol>());
		}
		return Task.FromResult(GetAvailableColumnsForSorting(cancellationToken));
	}

	private void GetNamesOfColumnsInExpression(PooledNameComparisonHashSet result)
	{
		if (!(orderByExpression.Parent is OrderByPropertyValueSyntax { Order: var order }))
		{
			return;
		}
		SeparatedSyntaxList<OrderByExpressionSyntax>.Enumerator enumerator = order.GetEnumerator();
		while (enumerator.MoveNext())
		{
			SeparatedSyntaxList<IdentifierNameSyntax>.Enumerator enumerator2 = enumerator.Current.SortingFields.GetEnumerator();
			while (enumerator2.MoveNext())
			{
				IdentifierNameSyntax current = enumerator2.Current;
				result.Add(current.GetIdentifierOrLiteralValue());
			}
		}
	}

	private IEnumerable<ISymbol> GetAvailableColumnsForSorting(CancellationToken cancellationToken)
	{
		ArrayBuilder<ISymbol> instance = ArrayBuilder<ISymbol>.GetInstance();
		PooledNameComparisonHashSet instance2 = PooledNameComparisonHashSet.GetInstance();
		try
		{
			GetNamesOfColumnsInExpression(instance2);
			foreach (ISymbol item in (IEnumerable<ISymbol>)base.Context.SemanticModel.LookupSymbols(base.Context.LeftToken.SpanStart, LookupOptions.MemberAccess, null, null, SymbolKind.Undefined, cancellationToken))
			{
				if (item.Kind == SymbolKind.QueryColumn && !instance2.Contains(item.Name))
				{
					instance.Add(item);
				}
			}
			return instance.ToArray();
		}
		finally
		{
			instance.Free();
			instance2.Free();
		}
	}
}
