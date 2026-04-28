using System.Collections.Generic;
using Microsoft.Dynamics.Nav.CodeAnalysis.SymbolUsage;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Host;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.LanguageServices;

public interface ISymbolUsageLoaderFactoryService : ILanguageService
{
	ISymbolUsageLoader GetSymbolUsageLoader(IReadOnlyList<string> cachePaths);
}
