using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Completion;

internal class ActionRefContextSymbolRecommender : ContextAwareSymbolRecommender
{
	internal ActionRefContextSymbolRecommender(MemberSyntaxContext context)
		: base(context)
	{
	}

	protected internal override async Task<IEnumerable<ISymbol>> RecommendSymbolsAsync(CancellationToken cancellationToken)
	{
		if (!base.Context.Page.HasFlag(PageContexts.ActionRef))
		{
			return await base.RecommendSymbolsAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		}
		if (base.DeclaringObject == null || base.Context.LeftToken.Kind != SyntaxKind.SemicolonToken)
		{
			return SpecializedCollections.EmptyEnumerable<ISymbol>();
		}
		SyntaxToken leftToken = base.Context.LeftToken;
		ImmutableArray<ISymbol> immutableArray = base.Context.SemanticModel.LookupSymbols(leftToken.SpanStart, LookupOptions.MustBePageElement, base.DeclaringObject, null, SymbolKind.Undefined, cancellationToken);
		ArrayBuilder<ISymbol> instance = ArrayBuilder<ISymbol>.GetInstance();
		try
		{
			ImmutableArray<ISymbol>.Enumerator enumerator = immutableArray.GetEnumerator();
			while (enumerator.MoveNext())
			{
				ISymbol current = enumerator.Current;
				if (current.Kind == SymbolKind.Action)
				{
					IActionSymbol actionSymbol = (IActionSymbol)current;
					if (actionSymbol.ActionKind.IsAllowedActionRefTarget() && actionSymbol.GetContainingActionAreaKind() != ActionAreaKind.Promoted)
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
