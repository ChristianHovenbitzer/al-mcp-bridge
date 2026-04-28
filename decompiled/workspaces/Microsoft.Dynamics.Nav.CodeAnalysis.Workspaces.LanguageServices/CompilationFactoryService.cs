using System;
using System.Collections.Immutable;
using Microsoft.Dynamics.Nav.CodeAnalysis.DotNet;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Host;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.LanguageServices;

internal class CompilationFactoryService : ICompilationFactoryService, ILanguageService
{
	private static readonly CompilationOptions DefaultOptions = new CompilationOptions(concurrentBuild: false);

	private readonly IDotNetResolverFactory dotNetResolverFactory = DotNetResolverFactoryHelper.CreateLocalDotNetResolverFactory();

	Compilation ICompilationFactoryService.CreateCompilation(string assemblyName, CompilationOptions options, ProjectDefinition definition)
	{
		return Compilation.Create(definition?.Name ?? assemblyName, definition?.Publisher, definition?.Version, definition?.AppId, null, null, alternateIds: definition?.AlternateIds ?? ImmutableArray<Guid>.Empty, options: options ?? DefaultOptions, fileSystem: null, dotNetResolverFactory: dotNetResolverFactory);
	}

	CompilationOptions ICompilationFactoryService.GetDefaultCompilationOptions()
	{
		return DefaultOptions;
	}
}
