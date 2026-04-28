using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Host;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.LanguageServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Options;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Completion;

internal abstract class AbstractRecommendationService : IRecommendationService, ILanguageService
{
	protected abstract Task<IEnumerable<ISymbol>> GetRecommendedSymbolsAtPositionWorkerAsync(AbstractSyntaxContext context, OptionSet options, CancellationToken cancellationToken);

	public async Task<IEnumerable<ISymbol>> GetRecommendedSymbolsAtPositionAsync(AbstractSyntaxContext context, OptionSet options, CancellationToken cancellationToken)
	{
		return await GetRecommendedSymbolsAtPositionWorkerAsync(context, options, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
	}
}
