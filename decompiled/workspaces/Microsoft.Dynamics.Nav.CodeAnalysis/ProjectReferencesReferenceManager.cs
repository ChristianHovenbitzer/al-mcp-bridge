using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

internal class ProjectReferencesReferenceManager : IReferenceManager, IReferenceResolver
{
	private readonly Solution solution;

	private readonly IReferenceManager delegatingResolver;

	private readonly ProjectId projectId;

	private readonly ReferenceManager delegatingReferenceManager;

	private Project Project => solution.GetProject(projectId);

	public IReferenceManager DelegatingResolver => delegatingResolver;

	public ReferenceManagerKind Kind => ReferenceManagerKind.Project;

	internal ProjectReferencesReferenceManager(Solution solution, ProjectId projectId, IReferenceManager delegatingResolver)
	{
		DebugAssertHelper.Assert(delegatingResolver != null, "A ProjectReferencesReferenceManager should always proxy a reference manager from an existing compilationHow could an existing compilation be created without a reference manager");
		this.delegatingResolver = GetDelegatingResolver(delegatingResolver);
		DebugAssertHelper.Assert(this.delegatingResolver.Kind == ReferenceManagerKind.SymbolFile, "The delegating resolver should always be a ReferenceManager");
		this.solution = solution;
		this.projectId = projectId;
		delegatingReferenceManager = (ReferenceManager)this.delegatingResolver;
	}

	public ImmutableArray<Diagnostic> GetDiagnostics(CancellationToken cancellationToken)
	{
		return DelegatingResolver.GetDiagnostics(cancellationToken);
	}

	public ImmutableArray<IModuleSymbol> GetLoadedModules()
	{
		PooledDictionary<SymbolReferenceSpecification, IModuleSymbol> instance = PooledDictionary<SymbolReferenceSpecification, IModuleSymbol>.GetInstance();
		PooledHashSet<IModuleSymbol> instance2 = PooledHashSet<IModuleSymbol>.GetInstance();
		try
		{
			AddOrUpdateModuleSymbolsFromDependentProjects(instance, instance2);
			ImmutableArray<ModuleSymbol> loadedModules = delegatingReferenceManager.GetLoadedModules();
			AddNewModuleSymbols(instance, loadedModules, instance2);
			return instance.Values.ToImmutableArray();
		}
		finally
		{
			instance2.Free();
			instance.Free();
		}
	}

	public ImmutableArray<ISymbolWithId> GetObjectSymbolsByIdAcrossModules(IModuleSymbol referencingModule, SymbolKind kind, int id)
	{
		ArrayBuilder<ISymbolWithId> instance = ArrayBuilder<ISymbolWithId>.GetInstance();
		try
		{
			ISymbolWithId objectSymbolById = referencingModule.GetObjectSymbolById(kind, id);
			if (objectSymbolById != null)
			{
				instance.Add(objectSymbolById);
			}
			(IList<ISymbol>, ISet<SymbolReferenceSpecification>) tuple = TryResolveObjectSymbolsFromDependentProjectsByIdOrName(kind, id, string.Empty);
			instance.AddRange(tuple.Item1.Cast<ISymbolWithId>());
			ImmutableArray<IModuleSymbol>.Enumerator enumerator = ((ModuleSymbol)referencingModule).ReferenceModules.GetEnumerator();
			while (enumerator.MoveNext())
			{
				IModuleSymbol current = enumerator.Current;
				SymbolReferenceSpecification item = new SymbolReferenceSpecification(current.Publisher, current.Name, current.Version, exact: false, current.AppId, isPropagated: false, current.AlternateIds);
				if (!tuple.Item2.Contains(item))
				{
					objectSymbolById = current.GetObjectSymbolById(kind, id);
					if (objectSymbolById != null)
					{
						instance.Add(objectSymbolById);
					}
				}
			}
			return instance.ToImmutable();
		}
		finally
		{
			instance.Free();
		}
	}

	public IEnumerable<ISymbol> GetObjectSymbolsByKindAcrossModules(IModuleSymbol referencingModule, SymbolKind kind)
	{
		ImmutableArray<ISymbol>.Enumerator enumerator = referencingModule.GetObjectSymbols(kind).GetEnumerator();
		while (enumerator.MoveNext())
		{
			yield return enumerator.Current;
		}
		(IList<ISymbol> Symbols, ISet<SymbolReferenceSpecification> Definitions) context = TryResolveObjectSymbolsByKindAcrossModulesFromDependentProjects(kind);
		foreach (ISymbol item2 in context.Symbols)
		{
			yield return item2;
		}
		ImmutableArray<IModuleSymbol>.Enumerator enumerator3 = ((ModuleSymbol)referencingModule).ReferenceModules.GetEnumerator();
		while (enumerator3.MoveNext())
		{
			IModuleSymbol current = enumerator3.Current;
			SymbolReferenceSpecification item = new SymbolReferenceSpecification(current.Publisher, current.Name, current.Version, exact: false, current.AppId, isPropagated: false, current.AlternateIds);
			if (!context.Definitions.Contains(item))
			{
				enumerator = current.GetObjectSymbols(kind).GetEnumerator();
				while (enumerator.MoveNext())
				{
					yield return enumerator.Current;
				}
			}
		}
	}

	public ImmutableArray<ISymbol> GetObjectSymbolsByNameAcrossModules(IModuleSymbol referencingModule, SymbolKind kind, string name)
	{
		if (kind == SymbolKind.DotNet)
		{
			return DelegatingResolver.GetObjectSymbolsByNameAcrossModules(referencingModule, kind, name);
		}
		ArrayBuilder<ISymbol> instance = ArrayBuilder<ISymbol>.GetInstance();
		try
		{
			ISymbol objectSymbolByName = referencingModule.GetObjectSymbolByName(kind, name);
			if (objectSymbolByName != null)
			{
				instance.Add(objectSymbolByName);
			}
			(IList<ISymbol>, ISet<SymbolReferenceSpecification>) tuple = TryResolveObjectSymbolsFromDependentProjectsByIdOrName(kind, null, name);
			instance.AddRange(tuple.Item1);
			ImmutableArray<IModuleSymbol>.Enumerator enumerator = ((ModuleSymbol)referencingModule).ReferenceModules.GetEnumerator();
			while (enumerator.MoveNext())
			{
				IModuleSymbol current = enumerator.Current;
				SymbolReferenceSpecification item = new SymbolReferenceSpecification(current.Publisher, current.Name, current.Version, exact: false, current.AppId, isPropagated: false, current.AlternateIds);
				if (!tuple.Item2.Contains(item))
				{
					objectSymbolByName = current.GetObjectSymbolByName(kind, name);
					if (objectSymbolByName != null)
					{
						instance.Add(objectSymbolByName);
					}
				}
			}
			return instance.ToImmutable();
		}
		finally
		{
			instance.Free();
		}
	}

	public ISymbol? GetObjectSymbolByName(IModuleSymbol referencingModule, Guid moduleId, SymbolKind kind, string name)
	{
		if (kind == SymbolKind.DotNet)
		{
			return DelegatingResolver.GetObjectSymbolsByNameAcrossModules(referencingModule, kind, name).FirstOrDefault();
		}
		foreach (ProjectId dependencyProject in GetDependencyProjects())
		{
			Project project = solution.GetProject(dependencyProject);
			if (project != null && project.Id.Id == moduleId)
			{
				return project.GetCompilationAsync().GetAwaiter().GetResult()
					.CompiledModule.GetObjectSymbolByName(kind, name);
			}
		}
		return null;
	}

	private static void AddNewModuleSymbols(PooledDictionary<SymbolReferenceSpecification, IModuleSymbol> moduleSymbols, IEnumerable<IModuleSymbol> symbols, PooledHashSet<IModuleSymbol> toBePropagatedSymbols)
	{
		foreach (IModuleSymbol symbol in symbols)
		{
			SymbolReferenceSpecification key = new SymbolReferenceSpecification(symbol.Publisher, symbol.Name, symbol.Version, exact: false, symbol.AppId, isPropagated: false, symbol.AlternateIds);
			if (!moduleSymbols.ContainsKey(key))
			{
				moduleSymbols.Add(key, symbol);
			}
		}
		foreach (IModuleSymbol toBePropagatedSymbol in toBePropagatedSymbols)
		{
			SymbolReferenceSpecification key2 = new SymbolReferenceSpecification(toBePropagatedSymbol.Publisher, toBePropagatedSymbol.Name, toBePropagatedSymbol.Version, exact: false, toBePropagatedSymbol.AppId, isPropagated: false, toBePropagatedSymbol.AlternateIds);
			if (!moduleSymbols.ContainsKey(key2))
			{
				moduleSymbols.Add(key2, toBePropagatedSymbol);
			}
		}
	}

	private (IList<ISymbol>, ISet<SymbolReferenceSpecification>) TryResolveObjectSymbolsByKindAcrossModulesFromDependentProjects(SymbolKind kind)
	{
		PooledList<ISymbol> instance = PooledList<ISymbol>.GetInstance();
		PooledHashSet<SymbolReferenceSpecification> instance2 = PooledHashSet<SymbolReferenceSpecification>.GetInstance();
		try
		{
			foreach (ProjectId dependencyProject in GetDependencyProjects())
			{
				Project project = solution.GetProject(dependencyProject);
				if (project != null)
				{
					ImmutableArray<ISymbol>.Enumerator enumerator2 = project.GetCompilationAsync().GetAwaiter().GetResult()
						.CompiledModule.GetObjectSymbols(kind).GetEnumerator();
					while (enumerator2.MoveNext())
					{
						ISymbol current2 = enumerator2.Current;
						instance.Add(current2);
					}
					AddProjectDefinitionAsSymbolReferenceSpecification(project, instance2);
				}
			}
			return (instance.ToArray(), instance2.ToSet(SymbolReferenceSpecification.VersionLessEqualityComparer));
		}
		finally
		{
			instance.Free();
			instance2.Free();
		}
	}

	private (IList<ISymbol>, ISet<SymbolReferenceSpecification>) TryResolveObjectSymbolsFromDependentProjectsByIdOrName(SymbolKind kind, int? id, string name)
	{
		PooledList<ISymbol> instance = PooledList<ISymbol>.GetInstance();
		PooledHashSet<SymbolReferenceSpecification> instance2 = PooledHashSet<SymbolReferenceSpecification>.GetInstance();
		try
		{
			foreach (ProjectId dependencyProject in GetDependencyProjects())
			{
				Project project = solution.GetProject(dependencyProject);
				if (project != null)
				{
					Compilation result = project.GetCompilationAsync().GetAwaiter().GetResult();
					ISymbol symbol;
					if (!id.HasValue)
					{
						symbol = result.CompiledModule.GetObjectSymbolByName(kind, name);
					}
					else
					{
						ISymbol objectSymbolById = result.CompiledModule.GetObjectSymbolById(kind, id.Value);
						symbol = objectSymbolById;
					}
					ISymbol symbol2 = symbol;
					if (symbol2 != null)
					{
						instance.Add(symbol2);
					}
					AddProjectDefinitionAsSymbolReferenceSpecification(project, instance2);
				}
			}
			return (instance.ToArray(), instance2.ToSet(SymbolReferenceSpecification.VersionLessEqualityComparer));
		}
		finally
		{
			instance.Free();
			instance2.Free();
		}
	}

	private static void AddProjectDefinitionAsSymbolReferenceSpecification(Project project, PooledHashSet<SymbolReferenceSpecification> projectDefinitions)
	{
		ProjectDefinition projectDefinition = project.ProjectDefinition;
		SymbolReferenceSpecification item = new SymbolReferenceSpecification(projectDefinition.Publisher, projectDefinition.Name, projectDefinition.Version, exact: false, projectDefinition.AppId, isPropagated: false, projectDefinition.AlternateIds);
		try
		{
			projectDefinitions.Add(item);
		}
		catch (AggregateException ex)
		{
			LocalMachineLogger.LogException(ex.InnerException ?? ex);
		}
	}

	private void AddOrUpdateModuleSymbolsFromDependentProjects(PooledDictionary<SymbolReferenceSpecification, IModuleSymbol> moduleSymbols, HashSet<IModuleSymbol> toBePropagatedSymbols)
	{
		ProjectDependencyGraph projectDependencyGraph = solution.GetProjectDependencyGraph();
		if (projectDependencyGraph == null)
		{
			return;
		}
		IImmutableSet<ProjectId> projectsThatThisProjectDirectlyDependsOn = projectDependencyGraph.GetProjectsThatThisProjectDirectlyDependsOn(projectId);
		foreach (ProjectId dependencyProject in GetDependencyProjects())
		{
			Project project = solution.GetProject(dependencyProject);
			if (project == null)
			{
				continue;
			}
			ProjectDefinition projectDefinition = project.ProjectDefinition;
			SymbolReferenceSpecification key = new SymbolReferenceSpecification(projectDefinition.Publisher, projectDefinition.Name, projectDefinition.Version, exact: false, projectDefinition.AppId, projectDefinition.PropagateDependencies, projectDefinition.AlternateIds);
			Compilation result = project.GetCompilationAsync().GetAwaiter().GetResult();
			if (result == null || !(result.CompiledModule != null))
			{
				continue;
			}
			moduleSymbols[key] = result.CompiledModule;
			if (projectDefinition.PropagateDependencies && projectsThatThisProjectDirectlyDependsOn.Contains(project.Id))
			{
				ImmutableArray<IModuleSymbol>.Enumerator enumerator2 = result.CompiledModule.ReferenceModules.GetEnumerator();
				while (enumerator2.MoveNext())
				{
					IModuleSymbol current2 = enumerator2.Current;
					toBePropagatedSymbols.Add(current2);
				}
			}
		}
	}

	private IReferenceManager GetDelegatingResolver(IReferenceManager startup)
	{
		IReferenceManager referenceManager = startup;
		while (referenceManager.DelegatingResolver != referenceManager)
		{
			referenceManager = referenceManager.DelegatingResolver;
		}
		return referenceManager;
	}

	private IList<ProjectId> GetDependencyProjects()
	{
		PooledHashSet<ProjectId> instance = PooledHashSet<ProjectId>.GetInstance();
		try
		{
			foreach (ProjectReference projectReference in Project.ProjectReferences)
			{
				instance.Add(projectReference.ProjectId);
				Project project = solution.GetProject(projectReference.ProjectId);
				if (!project.ProjectDefinition.PropagateDependencies || project.ProjectReferences == null)
				{
					continue;
				}
				foreach (ProjectReference projectReference2 in project.ProjectReferences)
				{
					instance.Add(projectReference2.ProjectId);
				}
			}
			return instance.ToList();
		}
		finally
		{
			instance.Free();
		}
	}
}
