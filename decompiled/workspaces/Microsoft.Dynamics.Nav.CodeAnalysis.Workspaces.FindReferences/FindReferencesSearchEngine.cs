using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.CodeAnalysis.Collections;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.FindSymbols;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Log;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.FindReferences;

internal class FindReferencesSearchEngine
{
	private readonly CancellationToken cancellationToken;

	private readonly ProjectDependencyGraph dependencyGraph;

	private readonly IImmutableSet<Document> documents;

	private readonly ImmutableArray<IReferenceFinder> finders;

	private readonly IStreamingFindReferencesProgress progress;

	private readonly StreamingProgressTracker progressTracker;

	private readonly Solution solution;

	private static readonly Func<Document, ISymbol, string> logDocument = (Document d, ISymbol s) => (d.Name == null || s.Name == null) ? string.Empty : string.Format(CultureInfo.InvariantCulture, "{0} - {1}", d.Name, s.Name);

	public FindReferencesSearchEngine(Solution solution, IImmutableSet<Document> documents, ImmutableArray<IReferenceFinder> finders, IStreamingFindReferencesProgress progress, CancellationToken cancellationToken)
	{
		this.documents = documents;
		this.solution = solution;
		this.finders = finders;
		this.progress = progress;
		this.cancellationToken = cancellationToken;
		dependencyGraph = solution.GetProjectDependencyGraph();
		progressTracker = new StreamingProgressTracker(progress.ReportProgressAsync);
	}

	public async Task FindReferencesAsync(SymbolAndProjectId symbolAndProjectId)
	{
		await progress.OnStartedAsync().ConfigureAwait(continueOnCapturedContext: false);
		await progressTracker.AddItemsAsync(1).ConfigureAwait(continueOnCapturedContext: false);
		try
		{
			await ProcessAsync(await CreateProjectToDocumentMapAsync(await CreateProjectMapAsync(await DetermineAllSymbolsAsync(symbolAndProjectId).ConfigureAwait(continueOnCapturedContext: false)).ConfigureAwait(continueOnCapturedContext: false)).ConfigureAwait(continueOnCapturedContext: false)).ConfigureAwait(continueOnCapturedContext: false);
		}
		finally
		{
			await progressTracker.ItemCompletedAsync().ConfigureAwait(continueOnCapturedContext: false);
			await progress.OnCompletedAsync().ConfigureAwait(continueOnCapturedContext: false);
		}
	}

	private async Task ProcessAsync(Dictionary<Project, MultiDictionary<Document, Tuple<SymbolAndProjectId, IReferenceFinder>>> projectToDocumentMap)
	{
		using (Logger.LogBlock(FunctionId.FindReference_ProcessAsync, cancellationToken))
		{
			if (projectToDocumentMap.Count == 0)
			{
				return;
			}
			IEnumerable<IEnumerable<ProjectId>> connectedProjects = dependencyGraph.GetDependencySets(cancellationToken);
			int count = projectToDocumentMap.Sum<KeyValuePair<Project, MultiDictionary<Document, Tuple<SymbolAndProjectId, IReferenceFinder>>>>((KeyValuePair<Project, MultiDictionary<Document, Tuple<SymbolAndProjectId, IReferenceFinder>>> kvp1) => kvp1.Value.Sum((KeyValuePair<Document, MultiDictionary<Document, Tuple<SymbolAndProjectId, IReferenceFinder>>.ValueSet> kvp2) => kvp2.Value.Count));
			await progressTracker.AddItemsAsync(count).ConfigureAwait(continueOnCapturedContext: false);
			foreach (IEnumerable<ProjectId> item in connectedProjects)
			{
				cancellationToken.ThrowIfCancellationRequested();
				await ProcessProjectsAsync(item, projectToDocumentMap).ConfigureAwait(continueOnCapturedContext: false);
			}
		}
	}

	[Conditional("DEBUG")]
	private static void ValidateProjectToDocumentMap(Dictionary<Project, MultiDictionary<Document, Tuple<SymbolAndProjectId, IReferenceFinder>>> projectToDocumentMap)
	{
		HashSet<Tuple<SymbolAndProjectId, IReferenceFinder>> hashSet = new HashSet<Tuple<SymbolAndProjectId, IReferenceFinder>>();
		foreach (MultiDictionary<Document, Tuple<SymbolAndProjectId, IReferenceFinder>> value in projectToDocumentMap.Values)
		{
			foreach (KeyValuePair<Document, MultiDictionary<Document, Tuple<SymbolAndProjectId, IReferenceFinder>>.ValueSet> item in value)
			{
				hashSet.Clear();
				foreach (Tuple<SymbolAndProjectId, IReferenceFinder> item2 in item.Value)
				{
					_ = item2;
				}
			}
		}
	}

	private Task HandleLocationAsync(SymbolAndProjectId symbolAndProjectId, ReferenceLocation location)
	{
		return progress.OnReferenceFoundAsync(symbolAndProjectId, location);
	}

	private async Task ProcessDocumentQueueAsync(Document document, MultiDictionary<Document, Tuple<SymbolAndProjectId, IReferenceFinder>>.ValueSet documentQueue)
	{
		await progress.OnFindInDocumentStartedAsync(document).ConfigureAwait(continueOnCapturedContext: false);
		SemanticModel model = null;
		try
		{
			model = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
			FindReferenceCache.Start(model);
			foreach (Tuple<SymbolAndProjectId, IReferenceFinder> item3 in documentQueue)
			{
				SymbolAndProjectId item = item3.Item1;
				IReferenceFinder item2 = item3.Item2;
				await ProcessDocumentAsync(document, item, item2).ConfigureAwait(continueOnCapturedContext: false);
			}
		}
		finally
		{
			FindReferenceCache.Stop(model);
			await progress.OnFindInDocumentCompletedAsync(document).ConfigureAwait(continueOnCapturedContext: false);
		}
	}

	private async Task ProcessDocumentAsync(Document document, SymbolAndProjectId symbolAndProjectId, IReferenceFinder finder)
	{
		using (Logger.LogBlock(FunctionId.FindReference_ProcessDocumentAsync, logDocument, document, symbolAndProjectId.Symbol, cancellationToken))
		{
			try
			{
				ImmutableArray<ReferenceLocation>.Enumerator enumerator = (await finder.FindReferencesInDocumentAsync(symbolAndProjectId, document, cancellationToken).ConfigureAwait(continueOnCapturedContext: false)).GetEnumerator();
				while (enumerator.MoveNext())
				{
					ReferenceLocation current = enumerator.Current;
					await HandleLocationAsync(symbolAndProjectId, current).ConfigureAwait(continueOnCapturedContext: false);
				}
			}
			finally
			{
				await progressTracker.ItemCompletedAsync().ConfigureAwait(continueOnCapturedContext: false);
			}
		}
	}

	private async Task<Dictionary<Project, MultiDictionary<Document, Tuple<SymbolAndProjectId, IReferenceFinder>>>> CreateProjectToDocumentMapAsync(MultiDictionary<Project, Tuple<SymbolAndProjectId, IReferenceFinder>> projectMap)
	{
		using (Logger.LogBlock(FunctionId.FindReference_CreateDocumentMapAsync, cancellationToken))
		{
			Dictionary<Project, MultiDictionary<Document, Tuple<SymbolAndProjectId, IReferenceFinder>>> finalMap = new Dictionary<Project, MultiDictionary<Document, Tuple<SymbolAndProjectId, IReferenceFinder>>>();
			foreach (KeyValuePair<Project, MultiDictionary<Project, Tuple<SymbolAndProjectId, IReferenceFinder>>.ValueSet> item in projectMap)
			{
				Project project = item.Key;
				MultiDictionary<Project, Tuple<SymbolAndProjectId, IReferenceFinder>>.ValueSet value = item.Value;
				MultiDictionary<Document, Tuple<SymbolAndProjectId, IReferenceFinder>> documentMap = new MultiDictionary<Document, Tuple<SymbolAndProjectId, IReferenceFinder>>();
				foreach (Tuple<SymbolAndProjectId, IReferenceFinder> symbolAndFinder in value)
				{
					cancellationToken.ThrowIfCancellationRequested();
					ISymbol symbol = symbolAndFinder.Item1.Symbol;
					foreach (Document item2 in Enumerable.Distinct(await symbolAndFinder.Item2.DetermineDocumentsToSearchAsync(symbol, project, documents, cancellationToken).ConfigureAwait(continueOnCapturedContext: false)).WhereNotNull())
					{
						if (documents == null || documents.Contains(item2))
						{
							documentMap.Add(item2, symbolAndFinder);
						}
					}
				}
				if (documentMap.Count > 0)
				{
					finalMap.Add(project, documentMap);
				}
			}
			return finalMap;
		}
	}

	private async Task<MultiDictionary<Project, Tuple<SymbolAndProjectId, IReferenceFinder>>> CreateProjectMapAsync(ConcurrentSet<SymbolAndProjectId> symbols)
	{
		using (Logger.LogBlock(FunctionId.FindReference_CreateProjectMapAsync, cancellationToken))
		{
			MultiDictionary<Project, Tuple<SymbolAndProjectId, IReferenceFinder>> projectMap = new MultiDictionary<Project, Tuple<SymbolAndProjectId, IReferenceFinder>>();
			ImmutableHashSet<Project> scope = documents?.Select((Document d) => d.Project).ToImmutableHashSet();
			ConcurrentSet<SymbolAndProjectId>.KeyEnumerator enumerator = symbols.GetEnumerator();
			while (enumerator.MoveNext())
			{
				SymbolAndProjectId symbolAndProjectId = enumerator.Current;
				ImmutableArray<IReferenceFinder>.Enumerator enumerator2 = finders.GetEnumerator();
				while (enumerator2.MoveNext())
				{
					IReferenceFinder finder = enumerator2.Current;
					cancellationToken.ThrowIfCancellationRequested();
					foreach (Project item in Enumerable.Distinct(await finder.DetermineProjectsToSearchAsync(symbolAndProjectId.Symbol, solution, scope, cancellationToken).ConfigureAwait(continueOnCapturedContext: false)).WhereNotNull())
					{
						if (scope == null || scope.Contains(item))
						{
							projectMap.Add(item, Tuple.Create(symbolAndProjectId, finder));
						}
					}
				}
			}
			return projectMap;
		}
	}

	private async Task<ConcurrentSet<SymbolAndProjectId>> DetermineAllSymbolsAsync(SymbolAndProjectId symbolAndProjectId)
	{
		using (Logger.LogBlock(FunctionId.FindReference_DetermineAllSymbolsAsync, cancellationToken))
		{
			ConcurrentSet<SymbolAndProjectId> result = new ConcurrentSet<SymbolAndProjectId>(new SymbolAndProjectIdComparer(MetadataUnifyingEquivalenceComparer.Instance));
			await DetermineAllSymbolsCoreAsync(symbolAndProjectId, result).ConfigureAwait(continueOnCapturedContext: false);
			return result;
		}
	}

	private async Task DetermineAllSymbolsCoreAsync(SymbolAndProjectId symbolAndProjectId, ConcurrentSet<SymbolAndProjectId> result)
	{
		ConcurrentSet<SymbolAndProjectId> result2 = result;
		cancellationToken.ThrowIfCancellationRequested();
		SymbolAndProjectId searchSymbolAndProjectId = MapToAppropriateSymbol(symbolAndProjectId);
		if (searchSymbolAndProjectId.Symbol == null || !result2.Add(searchSymbolAndProjectId))
		{
			return;
		}
		await progress.OnDefinitionFoundAsync(searchSymbolAndProjectId).ConfigureAwait(continueOnCapturedContext: false);
		ImmutableHashSet<Project> projects = GetProjectScope();
		cancellationToken.ThrowIfCancellationRequested();
		List<Task> list = new List<Task>();
		ImmutableArray<IReferenceFinder>.Enumerator enumerator = finders.GetEnumerator();
		while (enumerator.MoveNext())
		{
			IReferenceFinder f = enumerator.Current;
			list.Add(Task.Run(async delegate
			{
				List<Task> symbolTasks = new List<Task>();
				ImmutableArray<SymbolAndProjectId> immutableArray = await f.DetermineCascadedSymbolsAsync(searchSymbolAndProjectId, solution, projects, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
				AddSymbolTasks(result2, immutableArray, symbolTasks);
				cancellationToken.ThrowIfCancellationRequested();
				await Task.WhenAll(symbolTasks).ConfigureAwait(continueOnCapturedContext: false);
			}, cancellationToken));
		}
		await Task.WhenAll(list).ConfigureAwait(continueOnCapturedContext: false);
	}

	private void AddSymbolTasks(ConcurrentSet<SymbolAndProjectId> result, IEnumerable<SymbolAndProjectId> symbols, List<Task> symbolTasks)
	{
		ConcurrentSet<SymbolAndProjectId> result2 = result;
		if (symbols == null)
		{
			return;
		}
		foreach (SymbolAndProjectId child in symbols)
		{
			cancellationToken.ThrowIfCancellationRequested();
			symbolTasks.Add(Task.Run(() => DetermineAllSymbolsCoreAsync(child, result2), cancellationToken));
		}
	}

	private ImmutableHashSet<Project> GetProjectScope()
	{
		if (documents == null)
		{
			return null;
		}
		ImmutableHashSet<Project>.Builder builder = ImmutableHashSet.CreateBuilder<Project>();
		foreach (Document document in documents)
		{
			builder.Add(document.Project);
			foreach (ProjectReference projectReference in document.Project.ProjectReferences)
			{
				Project project = document.Project.Solution.GetProject(projectReference.ProjectId);
				if (project != null)
				{
					builder.Add(project);
				}
			}
		}
		return builder.ToImmutable();
	}

	private static SymbolAndProjectId MapToAppropriateSymbol(SymbolAndProjectId symbolAndProjectId)
	{
		ISymbol symbol = symbolAndProjectId.Symbol;
		return symbolAndProjectId.WithSymbol(symbol);
	}

	private async Task ProcessProjectsAsync(IEnumerable<ProjectId> connectedProjectSet, Dictionary<Project, MultiDictionary<Document, Tuple<SymbolAndProjectId, IReferenceFinder>>> projectToDocumentMap)
	{
		HashSet<ProjectId> visitedProjects = new HashSet<ProjectId>();
		foreach (ProjectId item in connectedProjectSet)
		{
			cancellationToken.ThrowIfCancellationRequested();
			await ProcessProjectAsync(item, projectToDocumentMap, visitedProjects).ConfigureAwait(continueOnCapturedContext: false);
		}
	}

	private async Task ProcessProjectAsync(ProjectId projectId, Dictionary<Project, MultiDictionary<Document, Tuple<SymbolAndProjectId, IReferenceFinder>>> projectToDocumentMap, HashSet<ProjectId> visitedProjects)
	{
		if (!visitedProjects.Add(projectId))
		{
			return;
		}
		Project project = solution.GetProject(projectId);
		foreach (ProjectReference projectReference in project.ProjectReferences)
		{
			cancellationToken.ThrowIfCancellationRequested();
			await ProcessProjectAsync(projectReference.ProjectId, projectToDocumentMap, visitedProjects).ConfigureAwait(continueOnCapturedContext: false);
		}
		await ProcessProjectAsync(project, projectToDocumentMap).ConfigureAwait(continueOnCapturedContext: false);
	}

	private async Task ProcessProjectAsync(Project project, Dictionary<Project, MultiDictionary<Document, Tuple<SymbolAndProjectId, IReferenceFinder>>> projectToDocumentMap)
	{
		if (projectToDocumentMap.TryGetValue(project, out MultiDictionary<Document, Tuple<SymbolAndProjectId, IReferenceFinder>> value))
		{
			projectToDocumentMap.Remove(project);
			await ProcessProjectAsync(project, value).ConfigureAwait(continueOnCapturedContext: false);
		}
	}

	private async Task ProcessProjectAsync(Project project, MultiDictionary<Document, Tuple<SymbolAndProjectId, IReferenceFinder>> documentMap)
	{
		using (Logger.LogBlock(FunctionId.FindReference_ProcessProjectAsync, project.Name, cancellationToken))
		{
			Compilation compilation = await project.GetCompilationAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
			List<Task> list = new List<Task>();
			foreach (KeyValuePair<Document, MultiDictionary<Document, Tuple<SymbolAndProjectId, IReferenceFinder>>.ValueSet> item in documentMap)
			{
				Document document = item.Key;
				if (document.Project == project)
				{
					MultiDictionary<Document, Tuple<SymbolAndProjectId, IReferenceFinder>>.ValueSet documentQueue = item.Value;
					list.Add(Task.Run(() => ProcessDocumentQueueAsync(document, documentQueue), cancellationToken));
				}
			}
			await Task.WhenAll(list).ConfigureAwait(continueOnCapturedContext: false);
			GC.KeepAlive(compilation);
		}
	}
}
