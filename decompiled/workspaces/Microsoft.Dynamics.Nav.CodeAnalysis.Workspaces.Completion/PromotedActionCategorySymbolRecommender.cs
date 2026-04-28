using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Completion;

internal class PromotedActionCategorySymbolRecommender : ContextAwareSymbolRecommender
{
	internal PromotedActionCategorySymbolRecommender(MemberSyntaxContext context)
		: base(context)
	{
	}

	protected internal override async Task<IEnumerable<ISymbol>> RecommendSymbolsAsync(CancellationToken cancellationToken)
	{
		if (base.DeclaringObject == null || !base.DeclaringObject.Kind.IsKind(SymbolKind.Page))
		{
			return SpecializedCollections.EmptyEnumerable<ISymbol>();
		}
		if (base.Context.TargetToken.Parent.Kind != SyntaxKind.PageActionGroup)
		{
			return SpecializedCollections.EmptyEnumerable<ISymbol>();
		}
		SyntaxNode firstParent = base.Context.TargetToken.Parent.GetFirstParent(SyntaxKind.PageActionArea);
		if (firstParent == null || !SemanticFacts.IsSameName(firstParent.GetNameStringValue(), ActionAreaKind.Promoted.ToString()))
		{
			return SpecializedCollections.EmptyEnumerable<ISymbol>();
		}
		ImmutableArray<ISymbol> immutableArray = base.Context.SemanticModel.LookupSymbols(base.Context.LeftToken.SpanStart, LookupOptions.MustBePageElement, base.DeclaringObject, null, SymbolKind.Undefined, cancellationToken);
		ArrayBuilder<ISymbol> instance = ArrayBuilder<ISymbol>.GetInstance(SyntaxFacts.PromotedCategoriesSynthesizedSymbolNames.Count);
		try
		{
			ImmutableArray<ISymbol>.Enumerator enumerator = immutableArray.GetEnumerator();
			while (enumerator.MoveNext())
			{
				ISymbol current = enumerator.Current;
				if (current.Kind == SymbolKind.Action)
				{
					IActionSymbol actionSymbol = (IActionSymbol)current;
					if (actionSymbol.ActionKind == ActionKind.Group && actionSymbol.IsSynthesized && SyntaxFacts.PromotedCategoriesSynthesizedSymbolNames.Contains(actionSymbol.Name))
					{
						instance.Add(actionSymbol);
					}
				}
			}
			return instance.ToImmutableArray();
		}
		finally
		{
			instance.Free();
		}
	}
}
