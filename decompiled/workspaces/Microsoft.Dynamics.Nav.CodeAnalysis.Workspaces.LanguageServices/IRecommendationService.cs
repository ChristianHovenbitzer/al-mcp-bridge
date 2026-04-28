using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Completion;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Host;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Options;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.LanguageServices;

internal interface IRecommendationService : ILanguageService
{
	Task<IEnumerable<ISymbol>> GetRecommendedSymbolsAtPositionAsync(AbstractSyntaxContext context, OptionSet options, CancellationToken cancellationToken);
}
