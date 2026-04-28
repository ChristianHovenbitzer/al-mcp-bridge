using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.LanguageServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Options;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Completion;

public static class Recommender
{
	public static Task<IEnumerable<ISymbol>> GetRecommendedSymbolsAtPositionAsync(AbstractSyntaxContext context, OptionSet options = null, CancellationToken cancellationToken = default(CancellationToken))
	{
		options = options ?? context.Workspace.Options;
		return context.Workspace.Services.GetLanguageServices("AL").GetService<IRecommendationService>().GetRecommendedSymbolsAtPositionAsync(context, options, cancellationToken);
	}
}
