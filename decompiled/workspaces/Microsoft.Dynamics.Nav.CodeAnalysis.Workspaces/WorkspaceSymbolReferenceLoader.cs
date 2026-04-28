using System.Collections.Generic;
using System.Linq;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.SymbolReference;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces;

internal sealed class WorkspaceSymbolReferenceLoader : ISymbolReferenceLoader
{
	private readonly MemoryCachedSymbolReferenceLoader loader = new MemoryCachedSymbolReferenceLoader();

	private readonly object mutex = new object();

	internal ISymbolReferenceLoader? NextLoader { get; private set; }

	internal ProjectId? ActiveProjectId { get; set; }

	internal WorkspaceSymbolReferenceLoader()
	{
	}

	internal void SetNextLoaderForProject(Workspace workspace, ProjectId id, ISymbolReferenceLoader? nextLoader)
	{
		Project project = workspace.CurrentSolution.GetProject(id);
		if (project == null)
		{
			return;
		}
		LocalCacheSymbolReferenceLoader nextLoader2 = new LocalCacheSymbolReferenceLoader(project.PackageCachePaths, DocumentationProviderFactory.Instance);
		lock (mutex)
		{
			ActiveProjectId = id;
			loader.NextLoader = nextLoader2;
			NextLoader = nextLoader;
		}
	}

	public ModuleDefinition LoadModule(SymbolReferenceSpecification reference, IList<Diagnostic> diagnostics)
	{
		return LoadModuleInfo(reference, diagnostics)?.ModuleMetadata;
	}

	public IEnumerable<SymbolReferenceSpecification> GetDependencies(SymbolReferenceSpecification reference, IList<Diagnostic> diagnostics)
	{
		IEnumerable<SymbolReferenceSpecification> dependencies = loader.GetDependencies(reference, diagnostics);
		if (!dependencies.Any() && NextLoader != null)
		{
			return NextLoader.GetDependencies(reference, diagnostics);
		}
		return dependencies;
	}

	public ModuleInfo LoadModuleInfo(SymbolReferenceSpecification reference, IList<Diagnostic> diagnostics, LoadModuleInfoFlags loadOptions = LoadModuleInfoFlags.Symbols)
	{
		ModuleInfo moduleInfo = loader.LoadModuleInfo(reference, diagnostics);
		if (moduleInfo == null && NextLoader != null)
		{
			return NextLoader.LoadModuleInfo(reference, diagnostics);
		}
		return moduleInfo;
	}

	internal void InvalidateModules(IEnumerable<IModuleSpecification> modules)
	{
		loader.InvalidateModules(modules);
	}

	internal void InvalidateSymbol(SymbolReferenceSpecification reference)
	{
		loader.InvalidateSymbol(reference);
	}

	internal void InvalidateDependencies(IEnumerable<SymbolReferenceSpecification> references)
	{
		loader.InvalidateDependencies(references);
	}
}
