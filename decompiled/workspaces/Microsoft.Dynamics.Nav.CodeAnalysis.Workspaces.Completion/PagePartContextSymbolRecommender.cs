using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Completion;

internal class PagePartContextSymbolRecommender : ContextAwareSymbolRecommender
{
	internal PagePartContextSymbolRecommender(MemberSyntaxContext context)
		: base(context)
	{
	}

	protected internal override async Task<IEnumerable<ISymbol>> RecommendSymbolsAsync(CancellationToken cancellationToken)
	{
		if (!base.Context.Page.HasFlag(PageContexts.PartPage))
		{
			return await base.RecommendSymbolsAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		}
		return from s in base.Context.LookupSymbols(SymbolKind.Page, cancellationToken)
			where s.IsKind(SymbolKind.Namespace) || (s.IsKind(SymbolKind.Page) && ((PageTypeSymbol)s).PageType.IsPartPageType())
			select s;
	}
}
