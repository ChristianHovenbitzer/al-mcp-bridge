using System.Collections.Generic;
using Microsoft.Dynamics.Nav.CodeAnalysis.DotNet;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Host;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.LanguageServices;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces;

internal class DotNetResolverFactoryService : IDotNetResolverFactoryService, ILanguageService
{
	public IDotNetResolverFactory GetDotNetResolverFactory(IReadOnlyList<string> assemblyProbingPaths)
	{
		return DotNetResolverFactoryHelper.CreateLocalDotNetResolverFactory(assemblyProbingPaths);
	}
}
