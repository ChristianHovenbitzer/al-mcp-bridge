using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.SymbolReference;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces;

public class ProjectBasedSymbolReferenceLoader : ISymbolReferenceLoader
{
	private readonly ISymbolReferenceLoader nextLoader;

	private readonly Workspace workspace;

	private readonly ProjectId projectId;

	private Solution CurrentSolution => workspace.CurrentSolution;

	public ProjectBasedSymbolReferenceLoader(ISymbolReferenceLoader nextLoader, Workspace workspace, ProjectId projectId)
	{
		if (nextLoader == null)
		{
			throw new ArgumentNullException("nextLoader");
		}
		if (workspace == null)
		{
			throw new ArgumentNullException("workspace");
		}
		this.nextLoader = nextLoader;
		this.workspace = workspace;
		this.projectId = projectId;
	}

	public ModuleDefinition LoadModule(SymbolReferenceSpecification reference, IList<Diagnostic> diagnostics)
	{
		return LoadModuleInfo(reference, diagnostics, LoadModuleInfoFlags.Symbols)?.ModuleMetadata;
	}

	public IEnumerable<SymbolReferenceSpecification> GetDependencies(SymbolReferenceSpecification reference, IList<Diagnostic> diagnostics)
	{
		IEnumerable<SymbolReferenceSpecification> dependencies = Enumerable.Empty<SymbolReferenceSpecification>();
		if (WalkDependencyGraphAndApplyAction(reference, delegate(Project referencedProject)
		{
			dependencies = referencedProject.SymbolReferences;
		}))
		{
			return dependencies;
		}
		SetNextLoaderIfNeeded();
		return nextLoader.GetDependencies(reference, diagnostics);
	}

	private bool WalkDependencyGraphAndApplyAction(SymbolReferenceSpecification reference, Action<Project> referencedProject)
	{
		ProjectDependencyGraph projectDependencyGraph = CurrentSolution.GetProjectDependencyGraph();
		if (projectDependencyGraph != null)
		{
			ProjectDefinition projectDefinition = (ProjectDefinition)reference;
			IImmutableSet<ProjectId> projectsThatThisProjectDirectlyDependsOn = projectDependencyGraph.GetProjectsThatThisProjectDirectlyDependsOn(projectId);
			IList<ProjectId> projectsThatThisProjectTransitivelyDependsOnTopologicallySorted = projectDependencyGraph.GetProjectsThatThisProjectTransitivelyDependsOnTopologicallySorted(projectId);
			for (int num = projectsThatThisProjectTransitivelyDependsOnTopologicallySorted.Count - 1; num >= 0; num--)
			{
				ProjectId value = projectsThatThisProjectTransitivelyDependsOnTopologicallySorted[num];
				Project project = CurrentSolution.GetProject(value);
				if (project != null && projectDefinition.IsVersionGreaterOrEqual(project.ProjectDefinition))
				{
					if (!projectsThatThisProjectDirectlyDependsOn.Contains(value))
					{
						foreach (ProjectId item in projectDependencyGraph.GetProjectsThatDirectlyDependOnThisProject(value))
						{
							Project project2 = CurrentSolution.GetProject(item);
							if (project2 != null && project2.ProjectDefinition.PropagateDependencies && projectsThatThisProjectDirectlyDependsOn.Contains(item))
							{
								referencedProject(project);
								return true;
							}
						}
						return false;
					}
					referencedProject(project);
					return true;
				}
			}
		}
		return false;
	}

	public ModuleInfo LoadModuleInfo(SymbolReferenceSpecification reference, IList<Diagnostic> diagnostics, LoadModuleInfoFlags loadOptions)
	{
		IList<Diagnostic> diagnostics2 = diagnostics;
		ModuleInfo module = null;
		if (reference.IsPlatformSymbolReference())
		{
			SetNextLoaderIfNeeded();
			return nextLoader.LoadModuleInfo(reference, diagnostics2, loadOptions);
		}
		if (WalkDependencyGraphAndApplyAction(reference, delegate(Project referencedProject)
		{
			Compilation result = referencedProject.GetCompilationAsync().GetAwaiter().GetResult();
			if (result != null && result.CompiledModule != null)
			{
				module = new ModuleInfo(SerializableSymbolModelConverter.ConvertModuleToSerializableSymbolModel(result), XmlDocumentationProvider.CreateFromStream(result.GetDocumentationComments(diagnostics2)));
			}
		}))
		{
			return module;
		}
		if (WalkDependencyGraphAndSearchExternalReferences(reference, diagnostics2, out module))
		{
			return module;
		}
		SetNextLoaderIfNeeded();
		return nextLoader.LoadModuleInfo(reference, diagnostics2, loadOptions);
	}

	private bool WalkDependencyGraphAndSearchExternalReferences(SymbolReferenceSpecification reference, IList<Diagnostic> diagnostics, out ModuleInfo moduleInfo)
	{
		moduleInfo = null;
		ProjectDependencyGraph projectDependencyGraph = CurrentSolution.GetProjectDependencyGraph();
		if (projectDependencyGraph == null)
		{
			return false;
		}
		foreach (ProjectId item in projectDependencyGraph.GetProjectsThatThisProjectDirectlyDependsOn(projectId))
		{
			Project project = CurrentSolution.GetProject(item);
			foreach (SymbolReferenceSpecification symbolReference in project.SymbolReferences)
			{
				if (symbolReference.IsPropagated && reference.IsSatisfiedBy(symbolReference.Publisher, symbolReference.Name, symbolReference.AppId, symbolReference.Version))
				{
					Compilation result = project.GetCompilationAsync().GetAwaiter().GetResult();
					if (result?.ReferenceLoader != null)
					{
						moduleInfo = result.ReferenceLoader.LoadModuleInfo(reference, diagnostics);
						return true;
					}
				}
			}
		}
		return false;
	}

	private void SetNextLoaderIfNeeded()
	{
		if (nextLoader is WorkspaceSymbolReferenceLoader workspaceSymbolReferenceLoader && !(workspaceSymbolReferenceLoader.ActiveProjectId == projectId))
		{
			workspace.SymbolReferenceLoader.SetNextLoaderForProject(workspace, projectId, null);
		}
	}
}
