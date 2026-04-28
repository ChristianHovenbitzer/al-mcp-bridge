using System.Collections.Generic;
using Microsoft.Dynamics.Nav.CodeAnalysis.SymbolUsage;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Host;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.LanguageServices;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces;

public class SymbolUsageLoaderFactoryService : ISymbolUsageLoaderFactoryService, ILanguageService
{
	public ISymbolUsageLoader GetSymbolUsageLoader(IReadOnlyList<string> cachePaths)
	{
		return new MemoryCachedSymbolUsageLoader(new LocalCacheSymbolUsageLoader(cachePaths));
	}
}
