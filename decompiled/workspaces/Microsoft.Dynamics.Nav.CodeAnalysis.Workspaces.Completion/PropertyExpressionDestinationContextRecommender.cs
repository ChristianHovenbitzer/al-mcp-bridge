using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Completion;

internal class PropertyExpressionDestinationContextRecommender : ContextAwareSymbolRecommender
{
	internal PropertyExpressionDestinationContextRecommender(MemberSyntaxContext context)
		: base(context)
	{
	}

	protected internal override async Task<IEnumerable<ISymbol>> RecommendSymbolsAsync(CancellationToken cancellationToken)
	{
		if (base.Context.PropertyExpressionContexts == PropertyExpressionContexts.None)
		{
			return await base.RecommendSymbolsAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		}
		return base.Context.LookupMemberSymbols(SymbolKind.Table, SymbolKind.Field, cancellationToken);
	}
}
