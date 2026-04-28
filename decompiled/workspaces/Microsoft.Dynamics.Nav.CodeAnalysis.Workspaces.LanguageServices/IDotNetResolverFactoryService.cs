using System.Collections.Generic;
using Microsoft.Dynamics.Nav.CodeAnalysis.DotNet;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Host;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.LanguageServices;

internal interface IDotNetResolverFactoryService : ILanguageService
{
	IDotNetResolverFactory GetDotNetResolverFactory(IReadOnlyList<string> assemblyProbingPaths);
}
