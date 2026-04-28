using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Completion;

internal class TableSymbolRecommender : ContextAwareSymbolRecommender
{
	internal TableSymbolRecommender(MemberSyntaxContext context)
		: base(context)
	{
	}

	protected internal override async Task<IEnumerable<ISymbol>> RecommendSymbolsAsync(CancellationToken cancellationToken)
	{
		if (IsNotTableContext())
		{
			return await base.RecommendSymbolsAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		}
		return base.Context.LookupSymbols(SymbolKind.Table, cancellationToken);
	}

	private bool IsNotTableContext()
	{
		if (!base.Context.Report.HasFlag(ReportContexts.DataItemSource) && !base.Context.XmlPort.HasFlag(XmlPortContexts.TableNodeSource))
		{
			return !base.Context.Query.HasFlag(QueryContexts.DataItemSource);
		}
		return false;
	}
}
