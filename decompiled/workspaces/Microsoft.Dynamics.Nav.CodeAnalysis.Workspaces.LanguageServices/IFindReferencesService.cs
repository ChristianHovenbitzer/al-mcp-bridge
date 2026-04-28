using System.Collections.Generic;
using System.Threading;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.FindSymbols;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Host;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.LanguageServices;

internal interface IFindReferencesService : ILanguageService
{
	IEnumerable<ReferencedSymbol> FindReferencedSymbolsAsync(Document document, int position, CancellationToken cancellationToken = default(CancellationToken));
}
