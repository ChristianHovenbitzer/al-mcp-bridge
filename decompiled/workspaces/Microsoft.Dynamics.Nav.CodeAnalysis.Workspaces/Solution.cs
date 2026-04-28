using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.CodeAnalysis.Analyzers;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.DotNet;
using Microsoft.Dynamics.Nav.CodeAnalysis.SymbolUsage;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Host;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.LanguageServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Log;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces;

public class Solution
{
	private class CompilationTracker : IDisposable
	{
		private class State
		{
			public static readonly State Empty = new State();

			public Compilation DeclarationOnlyCompilation { get; }

			public ValueSource<Compilation> Compilation { get; }

			public virtual ValueSource<Compilation> FinalCompilation => ValueSource<Microsoft.Dynamics.Nav.CodeAnalysis.Compilation>.Empty;

			private State()
				: this(ValueSource<Microsoft.Dynamics.Nav.CodeAnalysis.Compilation>.Empty, null)
			{
			}

			protected State(ValueSource<Compilation> compilation)
				: this(compilation, null)
			{
			}

			protected State(Compilation declarationOnlyCompilation)
				: this(ValueSource<Microsoft.Dynamics.Nav.CodeAnalysis.Compilation>.Empty, declarationOnlyCompilation)
			{
			}

			protected State(ValueSource<Compilation> compilation, Compilation declarationOnlyCompilation)
			{
				Compilation = compilation;
				DeclarationOnlyCompilation = declarationOnlyCompilation;
			}

			public static State Create(Compilation compilation, ImmutableArray<(ProjectState, CompilationTranslationAction)> intermediateProjects)
			{
				Contract.ThrowIfNull(compilation);
				Contract.ThrowIfTrue(intermediateProjects.IsDefault);
				if (intermediateProjects.Length != 0)
				{
					return new InProgressState(compilation, intermediateProjects);
				}
				return new FullDeclarationState(compilation);
			}

			public static ValueSource<Compilation> CreateValueSource(Compilation compilation, SolutionServices services)
			{
				if (!services.SupportsCachingRecoverableObjects)
				{
					return new ConstantValueSource<Compilation>(compilation);
				}
				return new WeakConstantValueSource<Compilation>(compilation);
			}
		}

		private sealed class InProgressState : State
		{
			public ImmutableArray<(ProjectState, CompilationTranslationAction)> IntermediateProjects { get; }

			public InProgressState(Compilation inProgressCompilation, ImmutableArray<(ProjectState, CompilationTranslationAction)> intermediateProjects)
				: base(new ConstantValueSource<Compilation>(inProgressCompilation))
			{
				Contract.ThrowIfNull(inProgressCompilation);
				Contract.ThrowIfTrue(intermediateProjects.IsDefault);
				Contract.ThrowIfFalse(intermediateProjects.Length > 0);
				IntermediateProjects = intermediateProjects;
			}
		}

		private sealed class LightDeclarationState : State
		{
			public LightDeclarationState(Compilation declarationOnlyCompilation)
				: base(declarationOnlyCompilation)
			{
			}
		}

		private sealed class FullDeclarationState : State
		{
			public FullDeclarationState(Compilation declarationCompilation)
				: base(new WeakConstantValueSource<Compilation>(declarationCompilation), declarationCompilation.Clone().RemoveAllReferences())
			{
			}
		}

		private sealed class FinalState : State
		{
			public override ValueSource<Compilation> FinalCompilation => base.Compilation;

			public FinalState(ValueSource<Compilation> finalCompilationSource)
				: base(finalCompilationSource, finalCompilationSource.GetValue().Clone().RemoveAllReferences())
			{
			}
		}

		private static readonly Func<ProjectState, string> logBuildCompilationAsync = LogBuildCompilationAsync;

		private State stateDoNotAccessDirectly;

		private AsyncLazy<VersionStamp> lazyDependentVersion;

		private AsyncLazy<VersionStamp> lazyDependentSemanticVersion;

		private readonly SemaphoreSlim buildLock = new SemaphoreSlim(1);

		private bool disposedValue;

		public ProjectState ProjectState { get; }

		public bool HasCompilation
		{
			get
			{
				State state = ReadState();
				if (!state.Compilation.HasValue)
				{
					return state.DeclarationOnlyCompilation != null;
				}
				return true;
			}
		}

		private CompilationTracker(ProjectState project, State state)
		{
			Contract.ThrowIfNull(project);
			ProjectState = project;
			stateDoNotAccessDirectly = state;
		}

		public CompilationTracker(ProjectState project)
			: this(project, State.Empty)
		{
		}

		private State ReadState()
		{
			return Volatile.Read(ref stateDoNotAccessDirectly);
		}

		private void WriteState(State state, Solution solution)
		{
			if (solution.Services.SupportsCachingRecoverableObjects)
			{
				solution.Services.CacheService.CacheObjectIfCachingEnabledForKey(ProjectState.Id, state, state.Compilation.GetValue());
			}
			Volatile.Write(ref stateDoNotAccessDirectly, state);
		}

		public CompilationTracker Fork(ProjectState newProject, CompilationTranslationAction translate = null, bool clone = false, CancellationToken cancellationToken = default(CancellationToken))
		{
			State state = ReadState();
			Compilation value = state.Compilation.GetValue(cancellationToken);
			if (value != null)
			{
				Compilation compilation = (clone ? value.Clone() : value);
				ImmutableArray<(ProjectState, CompilationTranslationAction)> immutableArray = ((state is InProgressState inProgressState) ? inProgressState.IntermediateProjects : ImmutableArray.Create<(ProjectState, CompilationTranslationAction)>());
				ImmutableArray<(ProjectState, CompilationTranslationAction)> intermediateProjects = ((translate == null) ? immutableArray : immutableArray.Add(ValueTuple.Create(ProjectState, translate)));
				State state2 = State.Create(compilation, intermediateProjects);
				return new CompilationTracker(newProject, state2);
			}
			Compilation declarationOnlyCompilation = state.DeclarationOnlyCompilation;
			if (declarationOnlyCompilation != null)
			{
				if (translate != null)
				{
					ImmutableArray<(ProjectState, CompilationTranslationAction)> intermediateProjects2 = ImmutableArray.Create(ValueTuple.Create(ProjectState, translate));
					return new CompilationTracker(newProject, new InProgressState(declarationOnlyCompilation, intermediateProjects2));
				}
				return new CompilationTracker(newProject, new LightDeclarationState(declarationOnlyCompilation));
			}
			return new CompilationTracker(newProject);
		}

		public CompilationTracker Clone()
		{
			return Fork(ProjectState, null, clone: true);
		}

		public CompilationTracker FreezePartialStateWithTree(Solution solution, DocumentState docState, SyntaxTree tree, CancellationToken cancellationToken)
		{
			SyntaxTree tree2 = tree;
			GetPartialCompilationState(solution, docState.Id, out ProjectState inProgressProject, out Compilation inProgressCompilation, cancellationToken);
			if (!inProgressCompilation.SyntaxTrees.Contains(tree2))
			{
				SyntaxTree syntaxTree = inProgressCompilation.SyntaxTrees.FirstOrDefault((SyntaxTree t) => t.FilePath == tree2.FilePath);
				if (syntaxTree != null)
				{
					inProgressCompilation = inProgressCompilation.ReplaceSyntaxTree(syntaxTree, tree2);
					inProgressProject = inProgressProject.UpdateDocument(docState, textChanged: false, recalculateDependentVersions: false);
				}
				else
				{
					inProgressCompilation = inProgressCompilation.AddSyntaxTrees(tree2);
					inProgressProject = inProgressProject.AddDocument(docState);
				}
			}
			return new CompilationTracker(inProgressProject, new FinalState(new ConstantValueSource<Compilation>(inProgressCompilation)));
		}

		private void GetPartialCompilationState(Solution solution, DocumentId id, out ProjectState inProgressProject, out Compilation inProgressCompilation, CancellationToken cancellationToken)
		{
			DocumentId id2 = id;
			State state = ReadState();
			inProgressCompilation = state.Compilation.GetValue(cancellationToken);
			InProgressState inProgressState = state as InProgressState;
			if (inProgressState != null && inProgressCompilation != null && inProgressState.IntermediateProjects.All<(ProjectState, CompilationTranslationAction)>(((ProjectState, CompilationTranslationAction) t) => TouchDocumentActionForDocument(t, id2)))
			{
				inProgressProject = ProjectState;
				return;
			}
			inProgressProject = ((inProgressState != null) ? inProgressState.IntermediateProjects.First().Item1 : ProjectState);
			if (inProgressCompilation != null && state is FinalState)
			{
				return;
			}
			if (inProgressCompilation == null)
			{
				inProgressProject = inProgressProject.RemoveAllDocuments();
				inProgressCompilation = CreateEmptyCompilation();
				inProgressCompilation = WithProjectReferenceResolver(solution, inProgressCompilation);
			}
			inProgressProject = inProgressProject.WithProjectReferences(ImmutableArray.Create<ProjectReference>());
			throw ExceptionUtilities.SkippedForNow;
		}

		private static bool TouchDocumentActionForDocument((ProjectState, CompilationTranslationAction) tuple, DocumentId id)
		{
			if (tuple.Item2 is CompilationTranslationAction.TouchDocumentAction touchDocumentAction)
			{
				return touchDocumentAction.DocumentId == id;
			}
			return false;
		}

		public bool TryGetCompilation(out Compilation compilation)
		{
			if (ReadState().FinalCompilation.TryGetValue(out compilation))
			{
				return compilation != null;
			}
			return false;
		}

		public async ValueTask<Compilation> GetCompilationAsync(Solution solution, CancellationToken cancellationToken)
		{
			if (TryGetCompilation(out Compilation compilation))
			{
				return compilation;
			}
			return await GetOrBuildCompilationAsync(solution, lockGate: true, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		}

		private static string LogBuildCompilationAsync(ProjectState state)
		{
			return string.Join(",", state.AssemblyName, state.DocumentIds.Count);
		}

		private async Task<Compilation> GetOrBuildCompilationAsync(Solution solution, bool lockGate, CancellationToken cancellationToken)
		{
			_ = 2;
			try
			{
				using (Logger.LogBlock(FunctionId.Workspace_Project_CompilationTracker_BuildCompilationAsync, logBuildCompilationAsync, ProjectState, cancellationToken))
				{
					cancellationToken.ThrowIfCancellationRequested();
					Compilation value = ReadState().FinalCompilation.GetValue(cancellationToken);
					if (value != null)
					{
						return value;
					}
					if (lockGate)
					{
						using (await buildLock.DisposableWaitAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
						{
							return await BuildCompilationAsync(solution, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
						}
					}
					return await BuildCompilationAsync(solution, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
				}
			}
			catch (Exception exception) when (FatalError.ReportUnlessCanceled(exception))
			{
				throw ExceptionUtilities.Unreachable;
			}
		}

		private Task<Compilation> BuildCompilationAsync(Solution solution, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();
			State state = ReadState();
			Compilation value = state.FinalCompilation.GetValue(cancellationToken);
			if (value != null)
			{
				return SpecializedTasks.FromResult(value);
			}
			value = state.Compilation.GetValue(cancellationToken);
			if (value == null)
			{
				if (state.DeclarationOnlyCompilation != null)
				{
					value = FinalizeCompilation(solution, state.DeclarationOnlyCompilation);
					return SpecializedTasks.FromResult(value);
				}
				return BuildCompilationFromScratchAsync(solution, state, cancellationToken);
			}
			if (state is FullDeclarationState)
			{
				value = FinalizeCompilation(solution, value);
				return SpecializedTasks.FromResult(value);
			}
			if (state is InProgressState state2)
			{
				return BuildFinalStateFromInProgressStateAsync(solution, state2, value, cancellationToken);
			}
			throw ExceptionUtilities.Unreachable;
		}

		private async Task<Compilation> BuildCompilationFromScratchAsync(Solution solution, State state, CancellationToken cancellationToken)
		{
			try
			{
				return FinalizeCompilation(solution, await BuildDeclarationCompilationFromScratchAsync(solution, cancellationToken).ConfigureAwait(continueOnCapturedContext: false));
			}
			catch (Exception exception) when (FatalError.ReportUnlessCanceled(exception))
			{
				throw ExceptionUtilities.Unreachable;
			}
		}

		private async Task<Compilation> BuildDeclarationCompilationFromScratchAsync(Solution solution, CancellationToken cancellationToken)
		{
			try
			{
				Compilation compilation = CreateEmptyCompilation().WithReferenceLoader(CreateReferenceLoader(solution)).WithDotNetResolverFactory(CreateDotNetResolverFactory(ProjectState.AssemblyProbingPaths)).WithInternalsVisibleToModules(ProjectState.InternalsVisibleToModules.Cast<IModuleSpecification>().ToImmutableArrayOrEmpty());
				foreach (SymbolReferenceSpecification symbolReference in ProjectState.SymbolReferences)
				{
					compilation = compilation.AddReferences(symbolReference);
				}
				foreach (DocumentState orderedDocumentState in ProjectState.OrderedDocumentStates)
				{
					cancellationToken.ThrowIfCancellationRequested();
					Compilation compilation2 = compilation;
					SyntaxTree syntaxTree = await orderedDocumentState.GetSyntaxTreeAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
					compilation = compilation2.AddSyntaxTrees(syntaxTree);
				}
				IFileSystem fileSystem = new RelativeFileSystem(Path.GetDirectoryName(ProjectState.FilePath));
				compilation = compilation.WithFileSystem(fileSystem);
				compilation = WithProjectReferenceResolver(solution, compilation);
				WriteState(new FullDeclarationState(compilation), solution);
				if (ProjectState.EnableShowSymbolUsage)
				{
					compilation = compilation.WithSymbolUsageLoader(CreateSymbolUsageLoader(ProjectState.PackageCachePaths));
				}
				return compilation;
			}
			catch (Exception exception) when (FatalError.ReportUnlessCanceled(exception))
			{
				throw ExceptionUtilities.Unreachable;
			}
		}

		private Compilation CreateEmptyCompilation()
		{
			return ProjectState.LanguageServices.GetService<ICompilationFactoryService>().CreateCompilation(ProjectState.AssemblyName, ProjectState.CompilationOptions, ProjectState.ProjectDefinition);
		}

		private ISymbolReferenceLoader CreateReferenceLoader(Solution solution)
		{
			return ProjectState.LanguageServices.GetService<IReferenceLoaderFactoryService>().GetSymbolReferenceLoader(solution.Workspace, ProjectState.Id);
		}

		private ISymbolUsageLoader CreateSymbolUsageLoader(IReadOnlyList<string> cachePaths)
		{
			return ProjectState.LanguageServices.GetService<ISymbolUsageLoaderFactoryService>().GetSymbolUsageLoader(cachePaths);
		}

		private IDotNetResolverFactory CreateDotNetResolverFactory(IReadOnlyList<string> assemblyProbingPaths)
		{
			return ProjectState.LanguageServices.GetService<IDotNetResolverFactoryService>().GetDotNetResolverFactory(assemblyProbingPaths);
		}

		private async Task<Compilation> BuildFinalStateFromInProgressStateAsync(Solution solution, InProgressState state, Compilation inProgressCompilation, CancellationToken cancellationToken)
		{
			try
			{
				return FinalizeCompilation(solution, await BuildDeclarationCompilationFromInProgressAsync(solution, state, inProgressCompilation, cancellationToken).ConfigureAwait(continueOnCapturedContext: false));
			}
			catch (Exception exception) when (FatalError.ReportUnlessCanceled(exception))
			{
				throw ExceptionUtilities.Unreachable;
			}
		}

		private async Task<Compilation> BuildDeclarationCompilationFromInProgressAsync(Solution solution, InProgressState state, Compilation inProgressCompilation, CancellationToken cancellationToken)
		{
			try
			{
				ImmutableArray<(ProjectState, CompilationTranslationAction)> intermediateProjects = state.IntermediateProjects;
				while (intermediateProjects.Length > 0)
				{
					cancellationToken.ThrowIfCancellationRequested();
					inProgressCompilation = await intermediateProjects[0].Item2.InvokeAsync(inProgressCompilation, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
					intermediateProjects = intermediateProjects.RemoveAt(0);
					WriteState(State.Create(inProgressCompilation, intermediateProjects), solution);
				}
				return inProgressCompilation;
			}
			catch (Exception exception) when (FatalError.ReportUnlessCanceled(exception))
			{
				throw ExceptionUtilities.Unreachable;
			}
		}

		private Compilation FinalizeCompilation(Solution solution, Compilation compilation)
		{
			try
			{
				if (!Enumerable.SequenceEqual(compilation.SymbolReferences, ProjectState.SymbolReferences, SymbolReferenceSpecification.VersionAwareEqualityComparer))
				{
					compilation = compilation.WithReferences(ProjectState.SymbolReferences);
				}
				compilation = WithProjectReferenceResolver(solution, compilation);
				WriteState(new FinalState(State.CreateValueSource(compilation, solution.Services)), solution);
				return compilation;
			}
			catch (Exception exception) when (FatalError.ReportUnlessCanceled(exception))
			{
				throw ExceptionUtilities.Unreachable;
			}
		}

		private Compilation WithProjectReferenceResolver(Solution solution, Compilation compilation)
		{
			if (ProjectState.ProjectReferences.Count > 0)
			{
				compilation = compilation.WithReferenceManager(new ProjectReferencesReferenceManager(solution, ProjectState.Id, compilation.ReferenceManager.DelegatingResolver));
			}
			return compilation;
		}

		public bool? ContainsSymbolsWithNameFromDeclarationOnlyCompilation(Func<string, bool> predicate, SymbolFilter filter, CancellationToken cancellationToken)
		{
			State state = ReadState();
			if (state.DeclarationOnlyCompilation == null)
			{
				return null;
			}
			return state.DeclarationOnlyCompilation.ContainsSymbolsWithName(predicate, filter, cancellationToken);
		}

		public IEnumerable<SyntaxTree> GetSyntaxTreesWithNameFromDeclarationOnlyCompilation(Func<string, bool> predicate, SymbolFilter filter, CancellationToken cancellationToken)
		{
			State state = ReadState();
			if (state.DeclarationOnlyCompilation == null)
			{
				return null;
			}
			return from s in state.DeclarationOnlyCompilation.Clone().GetSymbolsWithName(predicate, filter, cancellationToken)
				select s.DeclaringSyntaxReference.SyntaxTree;
		}

		public async Task<VersionStamp> GetDependentVersionAsync(Solution solution, CancellationToken cancellationToken)
		{
			Solution solution2 = solution;
			if (lazyDependentVersion == null)
			{
				Interlocked.CompareExchange(ref lazyDependentVersion, new AsyncLazy<VersionStamp>((CancellationToken c) => ComputeDependentVersionAsync(solution2, c), cacheResult: true), null);
			}
			return await lazyDependentVersion.GetValueAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		}

		private async Task<VersionStamp> ComputeDependentVersionAsync(Solution solution, CancellationToken cancellationToken)
		{
			ProjectState projectState = ProjectState;
			VersionStamp projVersion = projectState.Version;
			VersionStamp version = (await projectState.GetLatestDocumentVersionAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false)).GetNewerVersion(projVersion);
			foreach (ProjectReference projectReference in projectState.ProjectReferences)
			{
				cancellationToken.ThrowIfCancellationRequested();
				if (solution.ContainsProject(projectReference.ProjectId))
				{
					version = (await solution.GetDependentVersionAsync(projectReference.ProjectId, cancellationToken).ConfigureAwait(continueOnCapturedContext: false)).GetNewerVersion(version);
				}
			}
			return version;
		}

		public async Task<VersionStamp> GetDependentSemanticVersionAsync(Solution solution, CancellationToken cancellationToken)
		{
			Solution solution2 = solution;
			if (lazyDependentSemanticVersion == null)
			{
				Interlocked.CompareExchange(ref lazyDependentSemanticVersion, new AsyncLazy<VersionStamp>((CancellationToken c) => ComputeDependentSemanticVersionAsync(solution2, c), cacheResult: true), null);
			}
			return await lazyDependentSemanticVersion.GetValueAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		}

		private async Task<VersionStamp> ComputeDependentSemanticVersionAsync(Solution solution, CancellationToken cancellationToken)
		{
			ProjectState projectState = ProjectState;
			VersionStamp version = await projectState.GetSemanticVersionAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
			foreach (ProjectReference projectReference in projectState.ProjectReferences)
			{
				cancellationToken.ThrowIfCancellationRequested();
				if (solution.ContainsProject(projectReference.ProjectId))
				{
					version = (await solution.GetDependentSemanticVersionAsync(projectReference.ProjectId, cancellationToken).ConfigureAwait(continueOnCapturedContext: false)).GetNewerVersion(version);
				}
			}
			return version;
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				if (disposing)
				{
					buildLock.Dispose();
				}
				disposedValue = true;
			}
		}

		public void Dispose()
		{
			Dispose(disposing: true);
		}
	}

	private abstract class CompilationTranslationAction
	{
		internal class TouchDocumentAction : CompilationTranslationAction
		{
			private readonly DocumentState oldState;

			private readonly DocumentState newState;

			public DocumentId DocumentId => newState.Info.Id;

			public TouchDocumentAction(DocumentState oldState, DocumentState newState)
			{
				this.oldState = oldState;
				this.newState = newState;
			}

			public override Task<Compilation> InvokeAsync(Compilation oldCompilation, CancellationToken cancellationToken)
			{
				return UpdateDocumentInCompilationAsync(oldCompilation, oldState, newState, cancellationToken);
			}
		}

		private class RemoveDocumentAction : SimpleCompilationTranslationAction<DocumentState>
		{
			private static readonly Func<Compilation, DocumentState, CancellationToken, Task<Compilation>> compilationAction = async delegate(Compilation o, DocumentState d, CancellationToken c)
			{
				SyntaxTree syntaxTree = await d.GetSyntaxTreeAsync(c).ConfigureAwait(continueOnCapturedContext: false);
				return o.RemoveSyntaxTrees(syntaxTree);
			};

			public RemoveDocumentAction(DocumentState document)
				: base(document, compilationAction)
			{
			}
		}

		private class AddDocumentAction : SimpleCompilationTranslationAction<DocumentState>
		{
			private static readonly Func<Compilation, DocumentState, CancellationToken, Task<Compilation>> compilationAction = async delegate(Compilation o, DocumentState d, CancellationToken c)
			{
				SyntaxTree syntaxTree = await d.GetSyntaxTreeAsync(c).ConfigureAwait(continueOnCapturedContext: false);
				return o.AddSyntaxTrees(syntaxTree);
			};

			public AddDocumentAction(DocumentState document)
				: base(document, compilationAction)
			{
			}
		}

		private class ProjectCompilationOptionsAction : SimpleCompilationTranslationAction<CompilationOptions>
		{
			private static readonly Func<Compilation, CompilationOptions, CancellationToken, Task<Compilation>> compilationAction = (Compilation o, CompilationOptions d, CancellationToken c) => Task.FromResult(o.WithOptions(d));

			public ProjectCompilationOptionsAction(CompilationOptions option)
				: base(option, compilationAction)
			{
			}
		}

		private class ProjectParseOptionsAction : SimpleCompilationTranslationAction<ProjectState>
		{
			private static readonly Func<Compilation, ProjectState, CancellationToken, Task<Compilation>> s_action = delegate(Compilation o, ProjectState d, CancellationToken c)
			{
				Compilation o2 = o;
				ProjectState d2 = d;
				return Task.Run(() => ReplaceSyntaxTreesWithTreesFromNewProjectStateAsync(o2, d2, c), c);
			};

			public ProjectParseOptionsAction(ProjectState state)
				: base(state, s_action)
			{
			}
		}

		private class SymbolReferencesChange
		{
			public Solution Solution { get; }

			public ProjectId ProjectId { get; }

			public IEnumerable<SymbolReferenceSpecification> References { get; }

			public IEnumerable<string> PackageCachePaths { get; }

			public SymbolReferencesChange(IEnumerable<SymbolReferenceSpecification> references, IEnumerable<string> packageCachePaths, Solution solution, ProjectId projectId)
			{
				References = references;
				PackageCachePaths = packageCachePaths;
				ProjectId = projectId;
				Solution = solution;
			}
		}

		private class InternalsVisibleToModulesChange
		{
			public IEnumerable<SymbolReferenceSpecification> InternalsVisibleToModules { get; }

			public InternalsVisibleToModulesChange(IEnumerable<SymbolReferenceSpecification> internalsVisibleToModules)
			{
				InternalsVisibleToModules = internalsVisibleToModules;
			}
		}

		private class ProjectSymbolReferencesAction : SimpleCompilationTranslationAction<SymbolReferencesChange>
		{
			private static readonly Func<Compilation, SymbolReferencesChange, CancellationToken, Task<Compilation>> compilationAction = (Compilation o, SymbolReferencesChange d, CancellationToken c) => Task.FromResult(o.WithReferences(d.References).WithReferenceLoader(ProjectPackageCachePathAction.CreateReferenceLoader(new ProjectPackageCacheChange(d.PackageCachePaths, d.Solution, d.ProjectId))));

			public ProjectSymbolReferencesAction(IEnumerable<SymbolReferenceSpecification> references, IEnumerable<string> packageCachePaths, Solution solution, ProjectId projectId)
				: base(new SymbolReferencesChange(references, packageCachePaths, solution, projectId), compilationAction)
			{
			}
		}

		private class ProjectInternalsVisibleToModulesAction : SimpleCompilationTranslationAction<InternalsVisibleToModulesChange>
		{
			private static readonly Func<Compilation, InternalsVisibleToModulesChange, CancellationToken, Task<Compilation>> compilationAction = (Compilation o, InternalsVisibleToModulesChange d, CancellationToken c) => Task.FromResult(o.WithInternalsVisibleToModules(d.InternalsVisibleToModules));

			public ProjectInternalsVisibleToModulesAction(IEnumerable<SymbolReferenceSpecification> internalsVisibleToModules)
				: base(new InternalsVisibleToModulesChange(internalsVisibleToModules), compilationAction)
			{
			}
		}

		private class ProjectPackageCacheChange
		{
			public Solution Solution { get; }

			public ProjectId ProjectId { get; }

			public IEnumerable<string> PackageCachePaths { get; }

			public ProjectPackageCacheChange(IEnumerable<string> packageCachePaths, Solution solution, ProjectId projectId)
			{
				ProjectId = projectId;
				PackageCachePaths = packageCachePaths;
				Solution = solution;
			}
		}

		private class ProjectPackageCachePathAction : SimpleCompilationTranslationAction<ProjectPackageCacheChange>
		{
			private static readonly Func<Compilation, ProjectPackageCacheChange, CancellationToken, Task<Compilation>> compilationAction = (Compilation o, ProjectPackageCacheChange p, CancellationToken c) => Task.FromResult(o.WithReferenceLoader(CreateReferenceLoader(p)));

			internal static ISymbolReferenceLoader CreateReferenceLoader(ProjectPackageCacheChange projectPackageCacheChange)
			{
				return new ReferenceLoaderFactoryService().GetSymbolReferenceLoader(projectPackageCacheChange.Solution.Workspace, projectPackageCacheChange.ProjectId);
			}

			public ProjectPackageCachePathAction(ProjectPackageCacheChange data)
				: base(data, compilationAction)
			{
			}
		}

		private class ReloadSymbolReferenceChange
		{
			public Solution Solution { get; }

			public ProjectId ProjectId { get; }

			public IReadOnlyList<string> PackageCachePaths { get; }

			public SymbolReferenceSpecification SymbolReferenceSpecification { get; }

			public ReloadSymbolReferenceChange(Solution solution, ProjectId projectId, SymbolReferenceSpecification specification)
			{
				ProjectId = projectId;
				Solution = solution;
				SymbolReferenceSpecification = specification;
				Project project = Solution.GetProject(ProjectId);
				PackageCachePaths = project.PackageCachePaths;
			}
		}

		private class ReloadSymbolReferencesAction : SimpleCompilationTranslationAction<ReloadSymbolReferenceChange>
		{
			private static readonly Func<Compilation, ReloadSymbolReferenceChange, CancellationToken, Task<Compilation>> compilationAction = delegate(Compilation compilation, ReloadSymbolReferenceChange reloadSymbolReferenceChange, CancellationToken cancellationToken)
			{
				SymbolReferenceSpecification symbolReferenceSpecification = reloadSymbolReferenceChange.SymbolReferenceSpecification;
				ImmutableArray<IModuleSymbol>.Enumerator enumerator = compilation.CompiledModule.ReferenceModules.GetEnumerator();
				while (enumerator.MoveNext())
				{
					IModuleSymbol current = enumerator.Current;
					if (string.Equals(current.Name, symbolReferenceSpecification.Name, StringComparison.OrdinalIgnoreCase) && string.Equals(current.Publisher, symbolReferenceSpecification.Publisher, StringComparison.OrdinalIgnoreCase) && current.AppId == symbolReferenceSpecification.AppId)
					{
						return Task.FromResult(compilation);
					}
				}
				return Task.FromResult(compilation.WithReferenceLoader(CreateReferenceLoader(reloadSymbolReferenceChange)));
			};

			internal static ISymbolReferenceLoader CreateReferenceLoader(ReloadSymbolReferenceChange reloadSymbolReferenceChange)
			{
				return new ReferenceLoaderFactoryService().GetSymbolReferenceLoader(reloadSymbolReferenceChange.Solution.Workspace, reloadSymbolReferenceChange.ProjectId);
			}

			public ReloadSymbolReferencesAction(ReloadSymbolReferenceChange data)
				: base(data, compilationAction)
			{
			}
		}

		private class ProjectAssemblyProbingPathsAction : SimpleCompilationTranslationAction<IReadOnlyList<string>>
		{
			private static readonly Func<Compilation, IReadOnlyList<string>, CancellationToken, Task<Compilation>> compilationAction = (Compilation o, IReadOnlyList<string> assemblyProbingPaths, CancellationToken c) => Task.FromResult(o.WithDotNetResolverFactory(DotNetResolverFactoryHelper.CreateLocalDotNetResolverFactory(assemblyProbingPaths)));

			public ProjectAssemblyProbingPathsAction(IReadOnlyList<string> probingPaths)
				: base(probingPaths, compilationAction)
			{
			}
		}

		private class ProjectAssemblyNameAction : SimpleCompilationTranslationAction<string>
		{
			private static readonly Func<Compilation, string, CancellationToken, Task<Compilation>> compilationAction = (Compilation o, string d, CancellationToken c) => Task.FromResult(o.WithModuleName(d));

			public ProjectAssemblyNameAction(string assemblyName)
				: base(assemblyName, compilationAction)
			{
			}
		}

		private class ProjectDefinitionAction : SimpleCompilationTranslationAction<ProjectDefinition>
		{
			private static readonly Func<Compilation, ProjectDefinition, CancellationToken, Task<Compilation>> compilationAction = (Compilation o, ProjectDefinition d, CancellationToken c) => Task.FromResult(o.WithModuleInfo(new SymbolReferenceSpecification(d.Publisher, d.Name, d.Version, exact: false, d.AppId, isPropagated: false, d.AlternateIds)));

			public ProjectDefinitionAction(ProjectDefinition projectDefinition)
				: base(projectDefinition, compilationAction)
			{
			}
		}

		private class SimpleCompilationTranslationAction<T> : CompilationTranslationAction
		{
			private readonly T data;

			private readonly Func<Compilation, T, CancellationToken, Task<Compilation>> action;

			protected SimpleCompilationTranslationAction(T data, Func<Compilation, T, CancellationToken, Task<Compilation>> action)
			{
				this.data = data;
				this.action = action;
			}

			public override Task<Compilation> InvokeAsync(Compilation oldCompilation, CancellationToken cancellationToken)
			{
				return action(oldCompilation, data, cancellationToken);
			}
		}

		public abstract Task<Compilation> InvokeAsync(Compilation oldCompilation, CancellationToken cancellationToken);

		public static CompilationTranslationAction ProjectAssemblyName(string assemblyName)
		{
			return new ProjectAssemblyNameAction(assemblyName);
		}

		public static CompilationTranslationAction ProjectDefinition(ProjectDefinition definition)
		{
			return new ProjectDefinitionAction(definition);
		}

		public static CompilationTranslationAction ProjectCompilationOptions(CompilationOptions options)
		{
			return new ProjectCompilationOptionsAction(options);
		}

		public static CompilationTranslationAction ProjectParseOptions(ProjectState state)
		{
			return new ProjectParseOptionsAction(state);
		}

		public static CompilationTranslationAction ProjectSymbolReferences(IEnumerable<SymbolReferenceSpecification> references, IEnumerable<string> packageCachePaths, Solution solution, ProjectId projectId)
		{
			return new ProjectSymbolReferencesAction(references, packageCachePaths, solution, projectId);
		}

		public static CompilationTranslationAction ProjectInternalsVisibleToModules(IEnumerable<SymbolReferenceSpecification> internalsVisibleToModules)
		{
			return new ProjectInternalsVisibleToModulesAction(internalsVisibleToModules);
		}

		public static CompilationTranslationAction ProjectPackageCachePath(IEnumerable<string> packageCachePaths, Solution solution, ProjectId projectId)
		{
			return new ProjectPackageCachePathAction(new ProjectPackageCacheChange(packageCachePaths, solution, projectId));
		}

		public static CompilationTranslationAction ReloadSymbolReferences(Solution solution, ProjectId projectId, SymbolReferenceSpecification specification)
		{
			return new ReloadSymbolReferencesAction(new ReloadSymbolReferenceChange(solution, projectId, specification));
		}

		public static CompilationTranslationAction ProjectAssemblyProbingPaths(IReadOnlyList<string> probingPaths)
		{
			return new ProjectAssemblyProbingPathsAction(probingPaths);
		}

		public static CompilationTranslationAction AddDocument(DocumentState state)
		{
			return new AddDocumentAction(state);
		}

		public static CompilationTranslationAction RemoveDocument(DocumentState state)
		{
			return new RemoveDocumentAction(state);
		}

		public static CompilationTranslationAction TouchDocument(DocumentState oldState, DocumentState newState)
		{
			return new TouchDocumentAction(oldState, newState);
		}

		[Conditional("DEBUG")]
		public static void CheckKnownActions(CompilationTranslationAction translate)
		{
			if (translate != null)
			{
				Contract.ThrowIfFalse(translate is ProjectAssemblyNameAction || translate is ProjectDefinitionAction || translate is ProjectCompilationOptionsAction || translate is ProjectParseOptionsAction || translate is AddDocumentAction || translate is RemoveDocumentAction || translate is ProjectSymbolReferencesAction || translate is ProjectPackageCachePathAction || translate is ProjectAssemblyProbingPathsAction || translate is TouchDocumentAction || translate is ReloadSymbolReferencesAction || translate is ProjectInternalsVisibleToModulesAction);
			}
		}
	}

	private class SolutionBranch
	{
		public readonly DocumentId Id;

		public readonly SourceText Text;

		public readonly Solution Solution;

		public SolutionBranch(DocumentId id, SourceText text, Solution solution)
		{
			Id = id;
			Text = text;
			Solution = solution;
		}
	}

	private readonly ImmutableDictionary<ProjectId, ProjectState> projectIdToProjectStateMap;

	private readonly ImmutableDictionary<string, ImmutableArray<DocumentId>> linkedFilesMap;

	private readonly Lazy<VersionStamp> lazyLatestProjectVersion;

	private readonly ProjectDependencyGraph dependencyGraph;

	private ImmutableHashMap<ProjectId, Project> projectIdToProjectMap;

	private ImmutableDictionary<ProjectId, CompilationTracker> projectIdToTrackerMap;

	private SolutionBranch firstBranch;

	private NonReentrantLock stateLockBackingField;

	private static readonly Func<ProjectId, Solution, Project> s_createProjectFunction = CreateProject;

	private static readonly Func<ProjectId, Solution, CompilationTracker> s_createCompilationTrackerFunction = CreateCompilationTracker;

	private WeakReference<Solution> _latestSolutionWithPartialCompilation;

	private DateTime _timeOfLatestSolutionWithPartialCompilation;

	private DocumentId _documentIdOfLatestSolutionWithPartialCompilation;

	internal int WorkspaceVersion { get; }

	internal SolutionServices Services { get; }

	internal BranchId BranchId { get; }

	public Workspace Workspace => Services.Workspace;

	public SolutionId Id { get; }

	public string FilePath { get; }

	public VersionStamp Version { get; }

	public IReadOnlyList<ProjectId> ProjectIds { get; }

	public IEnumerable<Project> Projects => ProjectIds.Select((ProjectId id) => GetProject(id));

	private NonReentrantLock StateLock => LazyInitializer.EnsureInitialized(ref stateLockBackingField, NonReentrantLock.Factory);

	private Solution(BranchId branchId, int workspaceVersion, SolutionServices solutionServices, SolutionId id, string filePath, IEnumerable<ProjectId> projectIds, ImmutableDictionary<ProjectId, ProjectState> idToProjectStateMap, ImmutableDictionary<ProjectId, CompilationTracker> projectIdToTrackerMap, ImmutableDictionary<string, ImmutableArray<DocumentId>> linkedFilesMap, ProjectDependencyGraph dependencyGraph, VersionStamp version, Lazy<VersionStamp> lazyLatestProjectVersion)
	{
		BranchId = branchId;
		WorkspaceVersion = workspaceVersion;
		Id = id;
		FilePath = filePath;
		Services = solutionServices;
		ProjectIds = projectIds.ToImmutableReadOnlyListOrEmpty();
		projectIdToProjectStateMap = idToProjectStateMap;
		this.projectIdToTrackerMap = projectIdToTrackerMap;
		this.linkedFilesMap = linkedFilesMap;
		this.dependencyGraph = dependencyGraph;
		projectIdToProjectMap = ImmutableHashMap<ProjectId, Project>.Empty;
		Version = version;
		this.lazyLatestProjectVersion = lazyLatestProjectVersion;
		CheckInvariants();
	}

	internal Solution(Workspace workspace, SolutionInfo info)
		: this(workspace.PrimaryBranchId, 0, new SolutionServices(workspace), info.Id, info.FilePath, null, version: info.Version, idToProjectStateMap: ImmutableDictionary<ProjectId, ProjectState>.Empty, projectIdToTrackerMap: ImmutableDictionary<ProjectId, CompilationTracker>.Empty, linkedFilesMap: ImmutableDictionary.Create<string, ImmutableArray<DocumentId>>(StringComparer.OrdinalIgnoreCase), dependencyGraph: ProjectDependencyGraph.Empty, lazyLatestProjectVersion: null)
	{
		lazyLatestProjectVersion = new Lazy<VersionStamp>(() => ComputeLatestProjectVersion());
	}

	internal Solution WithNewWorkspace(Workspace workspace, int workspaceVersion)
	{
		SolutionServices services = ((workspace != Services.Workspace) ? new SolutionServices(workspace) : Services);
		return CreatePrimarySolution(workspace.PrimaryBranchId, workspaceVersion, services);
	}

	private VersionStamp ComputeLatestProjectVersion()
	{
		VersionStamp versionStamp = VersionStamp.Default;
		foreach (ProjectId projectId in ProjectIds)
		{
			versionStamp = GetProject(projectId).Version.GetNewerVersion(versionStamp);
		}
		return versionStamp;
	}

	private void CheckInvariants()
	{
		Contract.ThrowIfTrue(ProjectIds.Count != projectIdToProjectStateMap.Count);
		Contract.ThrowIfTrue(projectIdToTrackerMap.Any<KeyValuePair<ProjectId, CompilationTracker>>((KeyValuePair<ProjectId, CompilationTracker> kvp) => kvp.Key != kvp.Value.ProjectState.Id));
	}

	private Solution Branch(IEnumerable<ProjectId> projectIds = null, ImmutableDictionary<ProjectId, ProjectState> idToProjectStateMap = null, ImmutableDictionary<ProjectId, CompilationTracker> newProjectIdToTrackerMap = null, ImmutableDictionary<string, ImmutableArray<DocumentId>> newLinkedFilesMap = null, ProjectDependencyGraph newDependencyGraph = null, VersionStamp? version = null, Lazy<VersionStamp> newLazyLatestProjectVersion = null)
	{
		BranchId branchId = GetBranchId();
		projectIds = projectIds ?? ProjectIds;
		idToProjectStateMap = idToProjectStateMap ?? projectIdToProjectStateMap;
		newProjectIdToTrackerMap = newProjectIdToTrackerMap ?? projectIdToTrackerMap;
		newLinkedFilesMap = newLinkedFilesMap ?? linkedFilesMap;
		newDependencyGraph = newDependencyGraph ?? dependencyGraph;
		version = (version.HasValue ? version.Value : Version);
		newLazyLatestProjectVersion = newLazyLatestProjectVersion ?? lazyLatestProjectVersion;
		if (branchId == BranchId && projectIds == ProjectIds && idToProjectStateMap == projectIdToProjectStateMap && newProjectIdToTrackerMap == projectIdToTrackerMap && newLinkedFilesMap == linkedFilesMap && newDependencyGraph == dependencyGraph)
		{
			VersionStamp? versionStamp = version;
			VersionStamp version2 = Version;
			if (versionStamp.HasValue && versionStamp.GetValueOrDefault() == version2 && newLazyLatestProjectVersion == lazyLatestProjectVersion)
			{
				return this;
			}
		}
		return new Solution(branchId, WorkspaceVersion, Services, Id, FilePath, projectIds, idToProjectStateMap, newProjectIdToTrackerMap, newLinkedFilesMap, newDependencyGraph, version.Value, newLazyLatestProjectVersion);
	}

	private Solution CreatePrimarySolution(BranchId branchId, int workspaceVersion, SolutionServices services)
	{
		if (branchId == BranchId && workspaceVersion == WorkspaceVersion && services == Services)
		{
			return this;
		}
		return new Solution(branchId, workspaceVersion, services, Id, FilePath, ProjectIds, projectIdToProjectStateMap, projectIdToTrackerMap, linkedFilesMap, dependencyGraph, Version, lazyLatestProjectVersion);
	}

	private BranchId GetBranchId()
	{
		if (BranchId != Workspace.PrimaryBranchId)
		{
			return BranchId;
		}
		return Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.BranchId.GetNextId();
	}

	public VersionStamp GetLatestProjectVersion()
	{
		return lazyLatestProjectVersion.Value;
	}

	public bool ContainsProject(ProjectId projectId)
	{
		if (projectId != null)
		{
			return projectIdToProjectStateMap.ContainsKey(projectId);
		}
		return false;
	}

	public Project? GetProject(ProjectId projectId)
	{
		if (projectId == null)
		{
			throw new ArgumentNullException("projectId");
		}
		if (ContainsProject(projectId))
		{
			return ImmutableHashMapExtensions.GetOrAdd(ref projectIdToProjectMap, projectId, s_createProjectFunction, this);
		}
		return null;
	}

	public Project FindProject(string? path)
	{
		if (string.IsNullOrEmpty(path))
		{
			throw new ArgumentException("Path is null or empty.", "path");
		}
		string asUripath = path.CreateUriFilePath();
		return Projects.FirstOrDefault((Project p) => Path.GetDirectoryName(p.FilePath).CreateUriFilePath().Equals(asUripath, StringComparison.OrdinalIgnoreCase));
	}

	private static Project CreateProject(ProjectId projectId, Solution solution)
	{
		return new Project(solution, solution.GetProjectState(projectId));
	}

	public bool ContainsDocument(DocumentId documentId)
	{
		if (documentId != null && ContainsProject(documentId.ProjectId))
		{
			return GetProjectState(documentId.ProjectId).ContainsDocument(documentId);
		}
		return false;
	}

	public bool ContainsAdditionalDocument(DocumentId documentId)
	{
		if (documentId != null && ContainsProject(documentId.ProjectId))
		{
			return GetProjectState(documentId.ProjectId).ContainsAdditionalDocument(documentId);
		}
		return false;
	}

	public DocumentId GetDocumentId(SyntaxTree syntaxTree)
	{
		return GetDocumentId(syntaxTree, null);
	}

	public DocumentId GetDocumentId(SyntaxTree syntaxTree, ProjectId projectId)
	{
		if (syntaxTree != null)
		{
			DocumentId documentIdForTree = DocumentState.GetDocumentIdForTree(syntaxTree);
			if (documentIdForTree != null && (projectId == null || documentIdForTree.ProjectId == projectId) && ContainsDocument(documentIdForTree))
			{
				return documentIdForTree;
			}
		}
		return null;
	}

	public Document GetRequiredDocument(DocumentId documentId)
	{
		return GetDocument(documentId) ?? throw CreateDocumentNotFoundException();
	}

	public Document? GetDocument(DocumentId? documentId)
	{
		if ((object)documentId != null && ContainsDocument(documentId))
		{
			return GetProject(documentId.ProjectId).GetDocument(documentId);
		}
		return null;
	}

	public ImmutableArray<Document> GetAllDocuments()
	{
		ArrayBuilder<Document> instance = ArrayBuilder<Document>.GetInstance();
		try
		{
			foreach (Project project in Projects)
			{
				instance.AddRange(project.Documents);
			}
			return instance.ToImmutable();
		}
		finally
		{
			instance.Free();
		}
	}

	public TextDocument GetAdditionalDocument(DocumentId documentId)
	{
		if (documentId != null && ContainsAdditionalDocument(documentId))
		{
			return GetProject(documentId.ProjectId).GetAdditionalDocument(documentId);
		}
		return null;
	}

	private DocumentState GetDocumentState(DocumentId documentId)
	{
		if (documentId != null)
		{
			ProjectState projectState = GetProjectState(documentId.ProjectId);
			if (projectState != null)
			{
				return projectState.GetDocumentState(documentId);
			}
		}
		return null;
	}

	private TextDocumentState GetAdditionalDocumentState(DocumentId documentId)
	{
		if (documentId != null)
		{
			ProjectState projectState = GetProjectState(documentId.ProjectId);
			if (projectState != null)
			{
				return projectState.GetAdditionalDocumentState(documentId);
			}
		}
		return null;
	}

	public Document GetRequiredDocument(SyntaxTree syntaxTree)
	{
		return GetDocument(syntaxTree) ?? throw new InvalidOperationException();
	}

	public Document? GetDocument(SyntaxTree syntaxTree)
	{
		return GetDocument(syntaxTree, null);
	}

	internal Document? GetDocument(SyntaxTree syntaxTree, ProjectId? projectId)
	{
		if (syntaxTree != null)
		{
			DocumentId documentIdForTree = DocumentState.GetDocumentIdForTree(syntaxTree);
			if (documentIdForTree != null && (projectId == null || documentIdForTree.ProjectId == projectId))
			{
				Document document = GetDocument(documentIdForTree);
				if (document != null && document.TryGetSyntaxTree(out SyntaxTree syntaxTree2) && syntaxTree2 == syntaxTree)
				{
					return document;
				}
			}
		}
		return null;
	}

	internal Task<VersionStamp> GetDependentVersionAsync(ProjectId projectId, CancellationToken cancellationToken)
	{
		return GetCompilationTracker(projectId).GetDependentVersionAsync(this, cancellationToken);
	}

	internal Task<VersionStamp> GetDependentSemanticVersionAsync(ProjectId projectId, CancellationToken cancellationToken)
	{
		return GetCompilationTracker(projectId).GetDependentSemanticVersionAsync(this, cancellationToken);
	}

	internal ProjectState GetProjectState(ProjectId projectId)
	{
		projectIdToProjectStateMap.TryGetValue(projectId, out ProjectState value);
		return value;
	}

	private bool TryGetCompilationTracker(ProjectId projectId, out CompilationTracker tracker)
	{
		return projectIdToTrackerMap.TryGetValue(projectId, out tracker);
	}

	private static CompilationTracker CreateCompilationTracker(ProjectId projectId, Solution solution)
	{
		return new CompilationTracker(solution.GetProjectState(projectId));
	}

	private CompilationTracker GetCompilationTracker(ProjectId projectId)
	{
		if (!projectIdToTrackerMap.TryGetValue(projectId, out CompilationTracker value))
		{
			return ImmutableInterlocked.GetOrAdd(ref projectIdToTrackerMap, projectId, s_createCompilationTrackerFunction, this);
		}
		return value;
	}

	private Solution AddProject(ProjectId projectId, ProjectState projectState)
	{
		ProjectState projectState2 = projectState;
		ImmutableArray<ProjectId> immutableArray = ProjectIds.ToImmutableArray().Add(projectId);
		ImmutableDictionary<ProjectId, ProjectState> immutableDictionary = projectIdToProjectStateMap.Add(projectId, projectState2);
		ProjectDependencyGraph newDependencyGraph = CreateDependencyGraph(immutableArray, immutableDictionary);
		ImmutableDictionary<ProjectId, CompilationTracker> newProjectIdToTrackerMap = CreateCompilationTrackerMap(projectId, newDependencyGraph);
		ImmutableDictionary<string, ImmutableArray<DocumentId>> newLinkedFilesMap = CreateLinkedFilesMapWithAddedProject(immutableDictionary[projectId]);
		return Branch(immutableArray, immutableDictionary, newProjectIdToTrackerMap, newLinkedFilesMap, newDependencyGraph, Version.GetNewerVersion(), new Lazy<VersionStamp>(() => projectState2.Version));
	}

	public Solution AddProject(ProjectInfo projectInfo)
	{
		if (projectInfo == null)
		{
			throw new ArgumentNullException("projectInfo");
		}
		ProjectId id = projectInfo.Id;
		string language = projectInfo.Language;
		if (language == null)
		{
			throw new ArgumentException("language");
		}
		if (projectInfo.Name == null)
		{
			throw new ArgumentException("displayName");
		}
		CheckNotContainsProject(id);
		AbstractHostLanguageServices languageServices = Workspace.Services.GetLanguageServices(language);
		if (languageServices == null)
		{
			throw new ArgumentException(string.Format(CultureInfo.CurrentUICulture, WorkspacesResources.UnsupportedLanguage, language));
		}
		ProjectState projectState = new ProjectState(projectInfo, languageServices, Services);
		return AddProject(projectState.Id, projectState);
	}

	private ImmutableDictionary<string, ImmutableArray<DocumentId>> CreateLinkedFilesMapWithAddedProject(ProjectState projectState)
	{
		return CreateLinkedFilesMapWithAddedDocuments(projectState, projectState.DocumentIds);
	}

	private ImmutableDictionary<string, ImmutableArray<DocumentId>> CreateLinkedFilesMapWithAddedDocuments(ProjectState projectState, IEnumerable<DocumentId> documentIds)
	{
		ImmutableDictionary<string, ImmutableArray<DocumentId>>.Builder builder = linkedFilesMap.ToBuilder();
		foreach (DocumentId documentId in documentIds)
		{
			string filePath = projectState.GetDocumentState(documentId).FilePath;
			if (!string.IsNullOrEmpty(filePath))
			{
				builder[filePath] = (builder.TryGetValue(filePath, out var value) ? value.Add(documentId) : ImmutableArray.Create(documentId));
			}
		}
		return builder.ToImmutable();
	}

	public Solution RemoveProject(ProjectId projectId)
	{
		if (projectId == null)
		{
			throw new ArgumentNullException("projectId");
		}
		CheckContainsProject(projectId);
		ImmutableArray<ProjectId> immutableArray = ProjectIds.ToImmutableArray().Remove(projectId);
		ImmutableDictionary<ProjectId, ProjectState> immutableDictionary = projectIdToProjectStateMap.Remove(projectId);
		ProjectDependencyGraph newDependencyGraph = CreateDependencyGraph(immutableArray, immutableDictionary);
		ImmutableDictionary<ProjectId, CompilationTracker> immutableDictionary2 = CreateCompilationTrackerMap(projectId, newDependencyGraph);
		ImmutableDictionary<string, ImmutableArray<DocumentId>> newLinkedFilesMap = CreateLinkedFilesMapWithRemovedProject(projectIdToProjectStateMap[projectId]);
		return Branch(immutableArray, immutableDictionary, immutableDictionary2.Remove(projectId), newLinkedFilesMap, newDependencyGraph, Version.GetNewerVersion());
	}

	private ImmutableDictionary<string, ImmutableArray<DocumentId>> CreateLinkedFilesMapWithRemovedProject(ProjectState projectState)
	{
		return CreateLinkedFilesMapWithRemovedDocuments(projectState, projectState.DocumentIds);
	}

	private ImmutableDictionary<string, ImmutableArray<DocumentId>> CreateLinkedFilesMapWithRemovedDocuments(ProjectState projectState, IEnumerable<DocumentId> documentIds)
	{
		ImmutableDictionary<string, ImmutableArray<DocumentId>>.Builder builder = linkedFilesMap.ToBuilder();
		foreach (DocumentId documentId in documentIds)
		{
			string filePath = projectState.GetDocumentState(documentId).FilePath;
			if (!string.IsNullOrEmpty(filePath))
			{
				if (!builder.TryGetValue(filePath, out var value) || !value.Contains(documentId))
				{
					throw new ArgumentException("The given documentId was not found in the linkedFilesMap.");
				}
				if (value.Length == 1)
				{
					builder.Remove(filePath);
				}
				else
				{
					builder[filePath] = value.Remove(documentId);
				}
			}
		}
		return builder.ToImmutable();
	}

	public Solution WithProjectAssemblyName(ProjectId projectId, string? assemblyName)
	{
		if (projectId == null)
		{
			throw new ArgumentNullException("projectId");
		}
		if (assemblyName == null)
		{
			throw new ArgumentNullException("assemblyName");
		}
		CheckContainsProject(projectId);
		ProjectState projectState = GetProjectState(projectId);
		ProjectState projectState2 = projectState.UpdateAssemblyName(assemblyName);
		if (projectState == projectState2)
		{
			return this;
		}
		return ForkProject(projectState2, CompilationTranslationAction.ProjectAssemblyName(assemblyName));
	}

	public Solution WithProjectOutputFilePath(ProjectId projectId, string? outputFilePath)
	{
		if (projectId == null)
		{
			throw new ArgumentNullException("projectId");
		}
		if (outputFilePath == null)
		{
			throw new ArgumentNullException("outputFilePath");
		}
		CheckContainsProject(projectId);
		return ForkProject(GetProjectState(projectId).UpdateOutputPath(outputFilePath));
	}

	public Solution WithProjectPackageCachePath(ProjectId? projectId, IEnumerable<string>? packageCachePaths)
	{
		if (projectId == null)
		{
			throw new ArgumentNullException("projectId");
		}
		if (packageCachePaths == null)
		{
			throw new ArgumentNullException("packageCachePaths");
		}
		CheckContainsProject(projectId);
		ProjectState projectState = GetProjectState(projectId);
		return ForkProject(projectState.UpdatePackageCachePaths(packageCachePaths), CompilationTranslationAction.ProjectPackageCachePath(packageCachePaths, this, projectState.Id));
	}

	public Solution WithAssemblyProbingPaths(ProjectId? projectId, IReadOnlyList<string> probingPaths)
	{
		if (projectId == null)
		{
			throw new ArgumentNullException("projectId");
		}
		CheckContainsProject(projectId);
		return ForkProject(GetProjectState(projectId).UpdateAssemblyProbingPaths(probingPaths), CompilationTranslationAction.ProjectAssemblyProbingPaths(probingPaths));
	}

	public Solution WithRuleSetPath(ProjectId? projectId, string? ruleSetPath)
	{
		if (projectId == null)
		{
			throw new ArgumentNullException("projectId");
		}
		CheckContainsProject(projectId);
		return ForkProject(GetProjectState(projectId).UpdateRuleSetPath(ruleSetPath));
	}

	public Solution WithNamespaceTemplate(ProjectId? projectId, string? namespaceTemplate)
	{
		if (projectId == null)
		{
			throw new ArgumentNullException("projectId");
		}
		CheckContainsProject(projectId);
		return ForkProject(GetProjectState(projectId).UpdateNamespaceTemplate(namespaceTemplate));
	}

	public Solution WithExternalRulesetsEnabled(ProjectId? projectId, bool externalRulesetsEnabled)
	{
		if (projectId == null)
		{
			throw new ArgumentNullException("projectId");
		}
		CheckContainsProject(projectId);
		return ForkProject(GetProjectState(projectId).UpdateExternalRulesetsEnabled(externalRulesetsEnabled));
	}

	public Solution WithProjectName(ProjectId? projectId, string? name)
	{
		if (projectId == null)
		{
			throw new ArgumentNullException("projectId");
		}
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		CheckContainsProject(projectId);
		return ForkProject(GetProjectState(projectId).UpdateName(name));
	}

	public Solution WithProjectFilePath(ProjectId? projectId, string? filePath)
	{
		if (projectId == null)
		{
			throw new ArgumentNullException("projectId");
		}
		if (filePath == null)
		{
			throw new ArgumentNullException("filePath");
		}
		CheckContainsProject(projectId);
		return ForkProject(GetProjectState(projectId).UpdateFilePath(filePath));
	}

	public Solution WithProjectCompilationOptions(ProjectId? projectId, CompilationOptions? options)
	{
		if (projectId == null)
		{
			throw new ArgumentNullException("projectId");
		}
		if (options == null)
		{
			throw new ArgumentNullException("options");
		}
		CheckContainsProject(projectId);
		ProjectState projectState = GetProjectState(projectId);
		ProjectState projectState2 = projectState.UpdateCompilationOptions(options);
		if (projectState == projectState2)
		{
			return this;
		}
		return ForkProject(projectState2, CompilationTranslationAction.ProjectCompilationOptions(options));
	}

	public Solution WithProjectParseOptions(ProjectId? projectId, ParseOptions? options)
	{
		if (projectId == null)
		{
			throw new ArgumentNullException("projectId");
		}
		if (options == null)
		{
			throw new ArgumentNullException("options");
		}
		CheckContainsProject(projectId);
		ProjectState projectState = GetProjectState(projectId);
		ProjectState projectState2 = projectState.UpdateParseOptions(options);
		if (projectState == projectState2)
		{
			return this;
		}
		return ForkProject(projectState2, CompilationTranslationAction.ProjectParseOptions(projectState2));
	}

	private static async Task<Compilation> ReplaceSyntaxTreesWithTreesFromNewProjectStateAsync(Compilation compilation, ProjectState projectState, CancellationToken cancellationToken)
	{
		List<SyntaxTree> syntaxTrees = new List<SyntaxTree>(projectState.DocumentIds.Count);
		foreach (DocumentState orderedDocumentState in projectState.OrderedDocumentStates)
		{
			cancellationToken.ThrowIfCancellationRequested();
			List<SyntaxTree> list = syntaxTrees;
			list.Add(await orderedDocumentState.GetSyntaxTreeAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false));
		}
		return compilation.RemoveAllSyntaxTrees().AddSyntaxTrees(syntaxTrees);
	}

	public Solution AddProjectReference(ProjectId projectId, ProjectReference projectReference)
	{
		if (projectId == null)
		{
			throw new ArgumentNullException("projectId");
		}
		if (projectReference == null)
		{
			throw new ArgumentNullException("projectReference");
		}
		CheckContainsProject(projectId);
		CheckContainsProject(projectReference.ProjectId);
		CheckNotContainsProjectReference(projectId, projectReference);
		CheckNotContainsTransitiveReference(projectReference.ProjectId, projectId);
		ProjectState newProjectState = GetProjectState(projectId).AddProjectReference(projectReference);
		return ForkProject(newProjectState, null, withProjectReferenceChange: true);
	}

	public Solution AddProjectReferences(ProjectId projectId, IEnumerable<ProjectReference> projectReferences)
	{
		if (projectId == null)
		{
			throw new ArgumentNullException("projectId");
		}
		if (projectReferences == null)
		{
			throw new ArgumentNullException("projectReferences");
		}
		CheckContainsProject(projectId);
		foreach (ProjectReference projectReference in projectReferences)
		{
			CheckContainsProject(projectReference.ProjectId);
			CheckNotContainsProjectReference(projectId, projectReference);
			CheckNotContainsTransitiveReference(projectReference.ProjectId, projectId);
		}
		ProjectState newProjectState = GetProjectState(projectId).AddProjectReferences(projectReferences);
		return ForkProject(newProjectState, null, withProjectReferenceChange: true);
	}

	public Solution RemoveProjectReferences(ProjectId projectId, IEnumerable<ProjectReference> projectReferences)
	{
		if (projectId == null)
		{
			throw new ArgumentNullException("projectId");
		}
		if (projectReferences == null)
		{
			throw new ArgumentNullException("projectReferences");
		}
		foreach (ProjectReference projectReference in projectReferences)
		{
			CheckContainsProject(projectId);
			CheckContainsProject(projectReference.ProjectId);
		}
		ProjectState newProjectState = GetProjectState(projectId).RemoveProjectReferences(projectReferences);
		return ForkProject(newProjectState, null, withProjectReferenceChange: true);
	}

	public Solution RemoveProjectReference(ProjectId projectId, ProjectReference projectReference)
	{
		if (projectId == null)
		{
			throw new ArgumentNullException("projectId");
		}
		if (projectReference == null)
		{
			throw new ArgumentNullException("projectReference");
		}
		CheckContainsProject(projectId);
		CheckContainsProject(projectReference.ProjectId);
		ProjectState newProjectState = GetProjectState(projectId).RemoveProjectReference(projectReference);
		return ForkProject(newProjectState, null, withProjectReferenceChange: true);
	}

	public Solution RemoveAllProjectReferences(ProjectId projectId)
	{
		if (projectId == null)
		{
			throw new ArgumentNullException("projectId");
		}
		CheckContainsProject(projectId);
		ProjectState newProjectState = GetProjectState(projectId).RemoveAllProjectReferences();
		return ForkProject(newProjectState, null, withProjectReferenceChange: true);
	}

	public Solution WithProjectReferences(ProjectId projectId, IEnumerable<ProjectReference> projectReferences)
	{
		if (projectId == null)
		{
			throw new ArgumentNullException("projectId");
		}
		if (projectReferences == null)
		{
			throw new ArgumentNullException("projectReferences");
		}
		CheckContainsProject(projectId);
		ProjectState newProjectState = GetProjectState(projectId).WithProjectReferences(projectReferences);
		return ForkProject(newProjectState, null, withProjectReferenceChange: true);
	}

	public Solution AddSymbolReference(ProjectId projectId, SymbolReferenceSpecification symbolReference)
	{
		if (projectId == null)
		{
			throw new ArgumentNullException("projectId");
		}
		if (symbolReference == null)
		{
			throw new ArgumentNullException("symbolReference");
		}
		CheckContainsProject(projectId);
		ProjectState projectState = GetProjectState(projectId).AddSymbolReference(symbolReference);
		return ForkProject(projectState, CompilationTranslationAction.ProjectSymbolReferences(projectState.SymbolReferences, projectState.PackageCachePaths, this, projectState.Id));
	}

	public Solution AddSymbolReferences(ProjectId projectId, IEnumerable<SymbolReferenceSpecification> symbolReferences)
	{
		if (projectId == null)
		{
			throw new ArgumentNullException("projectId");
		}
		if (symbolReferences == null)
		{
			throw new ArgumentNullException("symbolReferences");
		}
		CheckContainsProject(projectId);
		ProjectState projectState = GetProjectState(projectId).AddSymbolReferences(symbolReferences);
		return ForkProject(projectState, CompilationTranslationAction.ProjectSymbolReferences(projectState.SymbolReferences, projectState.PackageCachePaths, this, projectState.Id));
	}

	public Solution RemoveSymbolReference(ProjectId projectId, SymbolReferenceSpecification symbolReference)
	{
		if (projectId == null)
		{
			throw new ArgumentNullException("projectId");
		}
		if (symbolReference == null)
		{
			throw new ArgumentNullException("symbolReference");
		}
		CheckContainsProject(projectId);
		ProjectState projectState = GetProjectState(projectId).RemoveSymbolReference(symbolReference);
		Workspace.SymbolReferenceLoader.InvalidateSymbol(symbolReference);
		return ForkProject(projectState, CompilationTranslationAction.ProjectSymbolReferences(projectState.SymbolReferences, projectState.PackageCachePaths, this, projectState.Id));
	}

	public Solution RemoveSymbolReferences(ProjectId projectId, IEnumerable<SymbolReferenceSpecification> symbolReferences)
	{
		if (projectId == null)
		{
			throw new ArgumentNullException("projectId");
		}
		if (symbolReferences == null)
		{
			throw new ArgumentNullException("symbolReferences");
		}
		CheckContainsProject(projectId);
		ProjectState projectState = GetProjectState(projectId).RemoveSymbolReferences(symbolReferences);
		Workspace.RemoveCachedSymbolReferences(symbolReferences);
		return ForkProject(projectState, CompilationTranslationAction.ProjectSymbolReferences(projectState.SymbolReferences, projectState.PackageCachePaths, this, projectState.Id));
	}

	public Solution WithProjectSymbolReferences(ProjectId projectId, IEnumerable<SymbolReferenceSpecification> symbolReferences)
	{
		if (projectId == null)
		{
			throw new ArgumentNullException("projectId");
		}
		if (symbolReferences == null)
		{
			throw new ArgumentNullException("symbolReferences");
		}
		CheckContainsProject(projectId);
		ProjectState projectState = GetProjectState(projectId);
		ProjectState projectState2 = projectState.WithSymbolReferences(symbolReferences);
		if (projectState == projectState2)
		{
			return this;
		}
		return ForkProject(projectState2, CompilationTranslationAction.ProjectSymbolReferences(projectState2.SymbolReferences, projectState2.PackageCachePaths, this, projectState2.Id));
	}

	public Solution WithInternalsVisibleToModules(ProjectId projectId, IEnumerable<SymbolReferenceSpecification> internalsVisibleToModules)
	{
		if (projectId == null)
		{
			throw new ArgumentNullException("projectId");
		}
		if (internalsVisibleToModules == null)
		{
			throw new ArgumentNullException("internalsVisibleToModules");
		}
		CheckContainsProject(projectId);
		ProjectState projectState = GetProjectState(projectId);
		ProjectState projectState2 = projectState.WithInternalsVisibleToModules(internalsVisibleToModules);
		if (projectState == projectState2)
		{
			return this;
		}
		return ForkProject(projectState2, CompilationTranslationAction.ProjectInternalsVisibleToModules(projectState2.InternalsVisibleToModules));
	}

	public Solution WithReloadSymbolReferences(ProjectId projectId, SymbolReferenceSpecification specification)
	{
		if (projectId == null)
		{
			throw new ArgumentNullException("projectId");
		}
		CheckContainsProject(projectId);
		ProjectState projectState = GetProjectState(projectId);
		return ForkProject(projectState, CompilationTranslationAction.ReloadSymbolReferences(this, projectState.Id, specification));
	}

	public Solution AddAnalyzerReference(ProjectId projectId, AnalyzerReference analyzerReference)
	{
		if (projectId == null)
		{
			throw new ArgumentNullException("projectId");
		}
		if (analyzerReference == null)
		{
			throw new ArgumentNullException("analyzerReference");
		}
		CheckContainsProject(projectId);
		return ForkProject(GetProjectState(projectId).AddAnalyzerReference(analyzerReference));
	}

	public Solution AddAnalyzerReferences(ProjectId projectId, IEnumerable<AnalyzerReference> analyzerReferences)
	{
		if (projectId == null)
		{
			throw new ArgumentNullException("projectId");
		}
		if (analyzerReferences == null)
		{
			throw new ArgumentNullException("analyzerReferences");
		}
		CheckContainsProject(projectId);
		return ForkProject(GetProjectState(projectId).AddAnalyzerReferences(analyzerReferences));
	}

	public Solution RemoveAnalyzerReference(ProjectId? projectId, AnalyzerReference analyzerReference)
	{
		if (projectId == null)
		{
			throw new ArgumentNullException("projectId");
		}
		if (analyzerReference == null)
		{
			throw new ArgumentNullException("analyzerReference");
		}
		CheckContainsProject(projectId);
		return ForkProject(GetProjectState(projectId).RemoveAnalyzerReference(analyzerReference));
	}

	public Solution WithProjectAnalyzerReferences(ProjectId? projectId, IEnumerable<AnalyzerReference> analyzerReferences)
	{
		if (projectId == null)
		{
			throw new ArgumentNullException("projectId");
		}
		if (analyzerReferences == null)
		{
			throw new ArgumentNullException("analyzerReferences");
		}
		CheckContainsProject(projectId);
		return ForkProject(GetProjectState(projectId).WithAnalyzerReferences(analyzerReferences));
	}

	public Solution WithBackgroundCodeAnalysisScope(ProjectId? projectId, BackgroundCodeAnalysisScope backgroundCodeAnalysisScope)
	{
		if (projectId == null)
		{
			throw new ArgumentNullException("projectId");
		}
		CheckContainsProject(projectId);
		return ForkProject(GetProjectState(projectId).WithBackgroundCodeAnalysisScope(backgroundCodeAnalysisScope));
	}

	public Solution WithOutputAnalyzerStatistics(ProjectId? projectId, bool outputAnalyzerStatistics)
	{
		if (projectId == null)
		{
			throw new ArgumentNullException("projectId");
		}
		CheckContainsProject(projectId);
		return ForkProject(GetProjectState(projectId).WithOutputAnalyzerStatistics(outputAnalyzerStatistics));
	}

	public Solution WithProjectEnableCodeActions(ProjectId? projectId, bool enableCodeActions)
	{
		if (projectId == null)
		{
			throw new ArgumentNullException("projectId");
		}
		CheckContainsProject(projectId);
		return ForkProject(GetProjectState(projectId).WithEnableCodeActions(enableCodeActions));
	}

	public Solution WithProjectIncrementalBuild(ProjectId? projectId, bool incrementalBuild)
	{
		if (projectId == null)
		{
			throw new ArgumentNullException("projectId");
		}
		CheckContainsProject(projectId);
		return ForkProject(GetProjectState(projectId).WithIncrementalBuild(incrementalBuild));
	}

	public Solution WithShowSymbolUsage(ProjectId projectId, bool showSymbolUsage)
	{
		if (projectId == null)
		{
			throw new ArgumentNullException("projectId");
		}
		CheckContainsProject(projectId);
		return ForkProject(GetProjectState(projectId).WithShowSymbolUsage(showSymbolUsage));
	}

	public Solution WithCaptureSymbolUsage(ProjectId? projectId, bool captureSymbolUsage)
	{
		if ((object)projectId == null)
		{
			throw new ArgumentNullException("projectId");
		}
		CheckContainsProject(projectId);
		return ForkProject(GetProjectState(projectId).WithCaptureSymbolUsage(captureSymbolUsage));
	}

	public Solution WithProjectDefinition(ProjectId? projectId, ProjectDefinition projectDefinition)
	{
		if (projectId == null)
		{
			throw new ArgumentNullException("projectId");
		}
		CheckContainsProject(projectId);
		ProjectState newProjectState = GetProjectState(projectId).WithProjectDefinition(projectDefinition);
		return ForkProject(newProjectState, CompilationTranslationAction.ProjectDefinition(projectDefinition));
	}

	public Solution WithExpectedProjectReferences(ProjectId? projectId, ISet<ProjectDefinition>? expectedProjectReferences)
	{
		if (projectId == null)
		{
			throw new ArgumentNullException("projectId");
		}
		CheckContainsProject(projectId);
		return ForkProject(GetProjectState(projectId).WithExpectedProjectReferences(expectedProjectReferences));
	}

	private Solution AddDocument(DocumentState state)
	{
		if (state == null)
		{
			throw new ArgumentNullException("state");
		}
		CheckContainsProject(state.Id.ProjectId);
		ProjectState projectState = GetProjectState(state.Id.ProjectId).AddDocument(state);
		return ForkProject(projectState, CompilationTranslationAction.AddDocument(state), withProjectReferenceChange: false, CreateLinkedFilesMapWithAddedDocuments(projectState, SpecializedCollections.SingletonEnumerable(state.Id)));
	}

	public Solution AddDocument(DocumentId documentId, string name, string text, IEnumerable<string> folders = null, string filePath = null)
	{
		return AddDocument(documentId, name, SourceText.From(text), folders, filePath);
	}

	public Solution AddDocument(DocumentId documentId, string name, SourceText text, IEnumerable<string> folders = null, string filePath = null, bool isGenerated = false)
	{
		if (documentId == null)
		{
			throw new ArgumentNullException("documentId");
		}
		CheckContainsProject(documentId.ProjectId);
		CheckNotContainsDocument(documentId);
		DocumentState state = CreateDocumentState(documentId, name, text, folders, filePath, isGenerated);
		return AddDocument(state);
	}

	public Solution AddDocument(DocumentId documentId, string name, SyntaxNode syntaxRoot, IEnumerable<string> folders = null, string filePath = null, bool isGenerated = false, PreservationMode preservationMode = PreservationMode.PreserveValue)
	{
		return AddDocument(documentId, name, SourceText.From(string.Empty), folders, filePath, isGenerated).WithDocumentSyntaxRoot(documentId, syntaxRoot, preservationMode);
	}

	public Solution AddDocument(DocumentId documentId, string name, TextLoader loader, IEnumerable<string> folders = null)
	{
		if (documentId == null)
		{
			throw new ArgumentNullException("documentId");
		}
		CheckContainsProject(documentId.ProjectId);
		CheckNotContainsDocument(documentId);
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		if (loader == null)
		{
			throw new ArgumentNullException("loader");
		}
		GetProjectState(documentId.ProjectId);
		DocumentInfo documentInfo = DocumentInfo.Create(documentId, name, folders, loader);
		return AddDocument(documentInfo);
	}

	public Solution AddDocument(DocumentInfo documentInfo)
	{
		if (documentInfo == null)
		{
			throw new ArgumentNullException("documentInfo");
		}
		CheckContainsProject(documentInfo.Id.ProjectId);
		CheckNotContainsDocument(documentInfo.Id);
		ProjectState projectState = GetProjectState(documentInfo.Id.ProjectId);
		DocumentState state = DocumentState.Create(documentInfo, projectState.ParseOptions, projectState.LanguageServices, Services);
		return AddDocument(state);
	}

	public Solution AddAdditionalDocument(DocumentId documentId, string name, string text, IEnumerable<string> folders = null, string filePath = null)
	{
		return AddAdditionalDocument(documentId, name, SourceText.From(text), folders, filePath);
	}

	public Solution AddAdditionalDocument(DocumentId documentId, string name, SourceText text, IEnumerable<string> folders = null, string filePath = null)
	{
		if (documentId == null)
		{
			throw new ArgumentNullException("documentId");
		}
		CheckContainsProject(documentId.ProjectId);
		CheckNotContainsAdditionalDocument(documentId);
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		if (text == null)
		{
			throw new ArgumentNullException("text");
		}
		VersionStamp version = VersionStamp.Create();
		TextLoader loader = TextLoader.From(TextAndVersion.Create(text, version, name));
		TextDocumentState state = TextDocumentState.Create(DocumentInfo.Create(documentId, name, folders, loader, filePath), Services);
		return AddAdditionalDocument(state);
	}

	public Solution AddAdditionalDocument(DocumentInfo documentInfo)
	{
		if (documentInfo == null)
		{
			throw new ArgumentNullException("documentInfo");
		}
		CheckContainsProject(documentInfo.Id.ProjectId);
		CheckNotContainsAdditionalDocument(documentInfo.Id);
		TextDocumentState state = TextDocumentState.Create(documentInfo, Services);
		return AddAdditionalDocument(state);
	}

	private Solution AddAdditionalDocument(TextDocumentState state)
	{
		if (state == null)
		{
			throw new ArgumentNullException("state");
		}
		CheckContainsProject(state.Id.ProjectId);
		ProjectState newProjectState = GetProjectState(state.Id.ProjectId).AddAdditionalDocument(state);
		return ForkProject(newProjectState);
	}

	public Solution RemoveDocument(DocumentId documentId)
	{
		CheckContainsDocument(documentId);
		ProjectState projectState = GetProjectState(documentId.ProjectId);
		DocumentState documentState = projectState.GetDocumentState(documentId);
		ProjectState newProjectState = projectState.RemoveDocument(documentId);
		return ForkProject(newProjectState, CompilationTranslationAction.RemoveDocument(documentState), withProjectReferenceChange: false, CreateLinkedFilesMapWithRemovedDocuments(projectState, SpecializedCollections.SingletonEnumerable(documentId)));
	}

	public Solution RemoveAdditionalDocument(DocumentId documentId)
	{
		CheckContainsAdditionalDocument(documentId);
		ProjectState newProjectState = GetProjectState(documentId.ProjectId).RemoveAdditionalDocument(documentId);
		return ForkProject(newProjectState);
	}

	public Solution WithDocumentFolders(DocumentId documentId, IEnumerable<string> folders)
	{
		if (documentId == null)
		{
			throw new ArgumentNullException("documentId");
		}
		if (folders == null)
		{
			throw new ArgumentNullException("folders");
		}
		folders = folders?.WhereNotNull().ToReadOnlyCollection();
		DocumentState newDocument = GetDocumentState(documentId).UpdateFolders(folders.WhereNotNull().ToReadOnlyCollection());
		return WithDocumentState(newDocument);
	}

	public Solution WithDocumentText(DocumentId documentId, SourceText text, PreservationMode mode = PreservationMode.PreserveValue)
	{
		if (documentId == null)
		{
			throw new ArgumentNullException("documentId");
		}
		if (text == null)
		{
			throw new ArgumentNullException("text");
		}
		CheckContainsDocument(documentId);
		DocumentState documentState = GetDocumentState(documentId);
		if (documentState.TryGetText(out SourceText text2) && text == text2)
		{
			return this;
		}
		if (mode == PreservationMode.PreserveIdentity)
		{
			SolutionBranch solutionBranch = firstBranch;
			if (solutionBranch != null && solutionBranch.Id == documentId && solutionBranch.Text == text)
			{
				return solutionBranch.Solution;
			}
		}
		Solution solution = WithDocumentState(documentState.UpdateText(text, mode), textChanged: true);
		if (mode == PreservationMode.PreserveIdentity && firstBranch == null)
		{
			Interlocked.CompareExchange(ref firstBranch, new SolutionBranch(documentId, text, solution), null);
		}
		return solution;
	}

	public Solution WithAdditionalDocumentText(DocumentId documentId, SourceText text, PreservationMode mode = PreservationMode.PreserveValue)
	{
		if (documentId == null)
		{
			throw new ArgumentNullException("documentId");
		}
		if (text == null)
		{
			throw new ArgumentNullException("text");
		}
		CheckContainsAdditionalDocument(documentId);
		TextDocumentState additionalDocumentState = GetAdditionalDocumentState(documentId);
		if (additionalDocumentState.TryGetText(out SourceText text2) && text == text2)
		{
			return this;
		}
		return WithTextDocumentState(additionalDocumentState.UpdateText(text, mode), textChanged: true);
	}

	public Solution WithDocumentText(DocumentId documentId, TextAndVersion textAndVersion, PreservationMode mode = PreservationMode.PreserveValue)
	{
		if (documentId == null)
		{
			throw new ArgumentNullException("documentId");
		}
		if (textAndVersion == null)
		{
			throw new ArgumentNullException("textAndVersion");
		}
		CheckContainsDocument(documentId);
		DocumentState documentState = GetDocumentState(documentId);
		return WithDocumentState(documentState.UpdateText(textAndVersion, mode), textChanged: true);
	}

	public Solution WithAdditionalDocumentText(DocumentId documentId, TextAndVersion textAndVersion, PreservationMode mode = PreservationMode.PreserveValue)
	{
		if (documentId == null)
		{
			throw new ArgumentNullException("documentId");
		}
		if (textAndVersion == null)
		{
			throw new ArgumentNullException("textAndVersion");
		}
		CheckContainsAdditionalDocument(documentId);
		TextDocumentState additionalDocumentState = GetAdditionalDocumentState(documentId);
		return WithTextDocumentState(additionalDocumentState.UpdateText(textAndVersion, mode), textChanged: true);
	}

	public Solution WithDocumentSyntaxRoot(DocumentId documentId, SyntaxNode root, PreservationMode mode = PreservationMode.PreserveValue)
	{
		if (documentId == null)
		{
			throw new ArgumentNullException("documentId");
		}
		if (root == null)
		{
			throw new ArgumentNullException("root");
		}
		CheckContainsDocument(documentId);
		DocumentState documentState = GetDocumentState(documentId);
		if (documentState.TryGetSyntaxTree(out SyntaxTree syntaxTree) && syntaxTree.TryGetRoot(out SyntaxNode resultRoot) && resultRoot == root)
		{
			return this;
		}
		return WithDocumentState(documentState.UpdateTree(root, mode), textChanged: true);
	}

	private static async Task<Compilation> UpdateDocumentInCompilationAsync(Compilation compilation, DocumentState oldDocument, DocumentState newDocument, CancellationToken cancellationToken)
	{
		return compilation.ReplaceSyntaxTree(await oldDocument.GetSyntaxTreeAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false), await newDocument.GetSyntaxTreeAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false));
	}

	public Solution WithDocumentTextLoader(DocumentId documentId, TextLoader loader, PreservationMode mode)
	{
		CheckContainsDocument(documentId);
		DocumentState documentState = GetDocumentState(documentId);
		return WithDocumentState(documentState.UpdateText(loader, mode), textChanged: true, recalculateDependentVersions: true);
	}

	public Solution WithAdditionalDocumentTextLoader(DocumentId documentId, TextLoader loader, PreservationMode mode)
	{
		CheckContainsAdditionalDocument(documentId);
		TextDocumentState additionalDocumentState = GetAdditionalDocumentState(documentId);
		return WithTextDocumentState(additionalDocumentState.UpdateText(loader, mode), textChanged: true, recalculateDependentVersions: true);
	}

	private Solution WithDocumentState(DocumentState newDocument, bool textChanged = false, bool recalculateDependentVersions = false)
	{
		DocumentState newDocument2 = newDocument;
		if (newDocument2 == null)
		{
			throw new ArgumentNullException("newDocument");
		}
		CheckContainsDocument(newDocument2.Id);
		if (newDocument2 == GetDocumentState(newDocument2.Id))
		{
			return this;
		}
		return TouchDocument(newDocument2.Id, (ProjectState p) => p.UpdateDocument(newDocument2, textChanged, recalculateDependentVersions));
	}

	private Solution TouchDocument(DocumentId documentId, Func<ProjectState, ProjectState> touchProject)
	{
		ProjectState projectState = GetProjectState(documentId.ProjectId);
		ProjectState projectState2 = touchProject(projectState);
		if (projectState == projectState2)
		{
			return this;
		}
		DocumentState documentState = projectState.GetDocumentState(documentId);
		DocumentState documentState2 = projectState2.GetDocumentState(documentId);
		return ForkProject(projectState2, CompilationTranslationAction.TouchDocument(documentState, documentState2));
	}

	private Solution WithTextDocumentState(TextDocumentState newDocument, bool textChanged = false, bool recalculateDependentVersions = false)
	{
		if (newDocument == null)
		{
			throw new ArgumentNullException("newDocument");
		}
		CheckContainsAdditionalDocument(newDocument.Id);
		if (newDocument == GetAdditionalDocumentState(newDocument.Id))
		{
			return this;
		}
		ProjectState projectState = GetProjectState(newDocument.Id.ProjectId);
		ProjectState projectState2 = projectState.UpdateAdditionalDocument(newDocument, textChanged, recalculateDependentVersions);
		if (projectState == projectState2)
		{
			return this;
		}
		return ForkProject(projectState2);
	}

	private Solution ForkProject(ProjectState newProjectState, CompilationTranslationAction translate = null, bool withProjectReferenceChange = false, ImmutableDictionary<string, ImmutableArray<DocumentId>> newLinkedFilesMap = null, bool forkTracker = true)
	{
		ProjectState newProjectState2 = newProjectState;
		ProjectId id = newProjectState2.Id;
		ImmutableDictionary<ProjectId, ProjectState> immutableDictionary = projectIdToProjectStateMap.SetItem(id, newProjectState2);
		ProjectDependencyGraph projectDependencyGraph = (withProjectReferenceChange ? CreateDependencyGraph(ProjectIds, immutableDictionary) : dependencyGraph);
		ImmutableDictionary<ProjectId, CompilationTracker> immutableDictionary2 = CreateCompilationTrackerMap(id, projectDependencyGraph);
		if (immutableDictionary2.TryGetValue(id, out var value))
		{
			immutableDictionary2 = immutableDictionary2.Remove(id);
			if (forkTracker)
			{
				immutableDictionary2 = immutableDictionary2.Add(id, value.Fork(newProjectState2, translate));
			}
		}
		Lazy<VersionStamp> lazy = ((translate is CompilationTranslationAction.TouchDocumentAction) ? lazyLatestProjectVersion : new Lazy<VersionStamp>(() => newProjectState2.Version));
		ImmutableDictionary<ProjectId, CompilationTracker> newProjectIdToTrackerMap = immutableDictionary2;
		ProjectDependencyGraph newDependencyGraph = projectDependencyGraph;
		ImmutableDictionary<string, ImmutableArray<DocumentId>> newLinkedFilesMap2 = newLinkedFilesMap ?? linkedFilesMap;
		Lazy<VersionStamp> newLazyLatestProjectVersion = lazy;
		return Branch(null, immutableDictionary, newProjectIdToTrackerMap, newLinkedFilesMap2, newDependencyGraph, null, newLazyLatestProjectVersion);
	}

	internal ImmutableArray<DocumentId> GetRelatedDocumentIds(DocumentId documentId)
	{
		ProjectState projectState = GetProjectState(documentId.ProjectId);
		if (projectState == null)
		{
			return ImmutableArray<DocumentId>.Empty;
		}
		DocumentState documentState = projectState.GetDocumentState(documentId);
		if (documentState == null)
		{
			return ImmutableArray<DocumentId>.Empty;
		}
		string filePath = documentState.FilePath;
		if (string.IsNullOrEmpty(filePath))
		{
			return ImmutableArray.Create(documentId);
		}
		return GetDocumentIdsWithFilePath(filePath);
	}

	public ImmutableArray<DocumentId> GetDocumentIdsWithFilePath(string filePath)
	{
		if (string.IsNullOrEmpty(filePath))
		{
			return ImmutableArray.Create<DocumentId>();
		}
		if (!linkedFilesMap.TryGetValue(filePath, out ImmutableArray<DocumentId> value))
		{
			return ImmutableArray.Create<DocumentId>();
		}
		return value;
	}

	internal bool HasCompilation(ProjectId id)
	{
		if (TryGetCompilationTracker(id, out CompilationTracker tracker) && tracker != null)
		{
			return tracker.HasCompilation;
		}
		return false;
	}

	private static ProjectDependencyGraph CreateDependencyGraph(IReadOnlyList<ProjectId> projectIds, ImmutableDictionary<ProjectId, ProjectState> projectStates)
	{
		ImmutableDictionary<ProjectId, ProjectState> projectStates2 = projectStates;
		ImmutableDictionary<ProjectId, ImmutableHashSet<ProjectId>> referencesMap = projectStates2.Values.Select((ProjectState state) => new KeyValuePair<ProjectId, ImmutableHashSet<ProjectId>>(state.Id, (from pr in state.ProjectReferences
			where projectStates2.ContainsKey(pr.ProjectId)
			select pr.ProjectId).ToImmutableHashSet())).ToImmutableDictionary();
		return new ProjectDependencyGraph(projectIds.ToImmutableArray(), referencesMap);
	}

	private ImmutableDictionary<ProjectId, CompilationTracker> CreateCompilationTrackerMap(ProjectId projectId, ProjectDependencyGraph newDependencyGraph)
	{
		ImmutableDictionary<ProjectId, CompilationTracker>.Builder builder = ImmutableDictionary.CreateBuilder<ProjectId, CompilationTracker>();
		IEnumerable<ProjectId> projectsThatTransitivelyDependOnThisProject = newDependencyGraph.GetProjectsThatTransitivelyDependOnThisProject(projectId);
		foreach (KeyValuePair<ProjectId, CompilationTracker> item in projectIdToTrackerMap)
		{
			ProjectId key = item.Key;
			CompilationTracker value = item.Value;
			if (value.HasCompilation)
			{
				bool flag = key == projectId || !projectsThatTransitivelyDependOnThisProject.Contains(key);
				builder.Add(key, flag ? value : value.Fork(value.ProjectState));
			}
		}
		return builder.ToImmutable();
	}

	public Solution GetIsolatedSolution()
	{
		ImmutableDictionary<ProjectId, CompilationTracker> newProjectIdToTrackerMap = ImmutableDictionary.CreateRange(from kvp in projectIdToTrackerMap
			where kvp.Value.HasCompilation
			select new KeyValuePair<ProjectId, CompilationTracker>(kvp.Key, kvp.Value.Clone()));
		return Branch(null, null, newProjectIdToTrackerMap);
	}

	internal DocumentState CreateDocumentState(DocumentId documentId, string name, SourceText text, IEnumerable<string> folders = null, string filePath = null, bool isGenerated = false)
	{
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		if (text == null)
		{
			throw new ArgumentNullException("text");
		}
		ProjectState projectState = GetProjectState(documentId.ProjectId);
		VersionStamp version = VersionStamp.Create();
		TextLoader loader = TextLoader.From(TextAndVersion.Create(text, version, name));
		return DocumentState.Create(DocumentInfo.Create(documentId, name, folders, loader, filePath, isGenerated), projectState.ParseOptions, projectState.LanguageServices, Services);
	}

	internal async Task<Solution> WithFrozenPartialCompilationIncludingSpecificDocumentAsync(DocumentId documentId, CancellationToken cancellationToken)
	{
		try
		{
			Document doc = GetDocument(documentId);
			SyntaxTree tree = await doc.GetSyntaxTreeAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
			using (StateLock.DisposableWait(cancellationToken))
			{
				Workspace workspace = Workspace;
				if (workspace != null && workspace.TestHookPartialSolutionsDisabled)
				{
					return this;
				}
				Solution target = null;
				if (_latestSolutionWithPartialCompilation != null)
				{
					_latestSolutionWithPartialCompilation.TryGetTarget(out target);
				}
				if (target == null || (DateTime.UtcNow - _timeOfLatestSolutionWithPartialCompilation).TotalSeconds >= 0.1 || _documentIdOfLatestSolutionWithPartialCompilation != documentId)
				{
					CompilationTracker compilationTracker = GetCompilationTracker(documentId.ProjectId).FreezePartialStateWithTree(this, doc.State, tree, cancellationToken);
					ImmutableDictionary<ProjectId, ProjectState> immutableDictionary = projectIdToProjectStateMap.SetItem(documentId.ProjectId, compilationTracker.ProjectState);
					ImmutableDictionary<ProjectId, CompilationTracker> newProjectIdToTrackerMap = projectIdToTrackerMap.SetItem(documentId.ProjectId, compilationTracker);
					target = Branch(null, immutableDictionary, newProjectIdToTrackerMap, null, CreateDependencyGraph(ProjectIds, immutableDictionary));
					_latestSolutionWithPartialCompilation = new WeakReference<Solution>(target);
					_timeOfLatestSolutionWithPartialCompilation = DateTime.UtcNow;
					_documentIdOfLatestSolutionWithPartialCompilation = documentId;
				}
				return target;
			}
		}
		catch (Exception exception) when (FatalError.ReportUnlessCanceled(exception))
		{
			throw ExceptionUtilities.Unreachable;
		}
	}

	public Solution WithDocumentText(IEnumerable<DocumentId> documentIds, SourceText text, PreservationMode mode = PreservationMode.PreserveValue)
	{
		if (documentIds == null)
		{
			throw new ArgumentNullException("documentIds");
		}
		if (text == null)
		{
			throw new ArgumentNullException("text");
		}
		Solution solution = this;
		foreach (DocumentId documentId in documentIds)
		{
			Document document = solution.GetDocument(documentId);
			if (document != null && (!document.TryGetText(out SourceText text2) || text2 != text))
			{
				solution = solution.WithDocumentText(documentId, text, mode);
			}
		}
		return solution;
	}

	internal bool TryGetCompilation(ProjectId projectId, out Compilation compilation)
	{
		CheckContainsProject(projectId);
		compilation = null;
		if (TryGetCompilationTracker(projectId, out CompilationTracker tracker))
		{
			return tracker.TryGetCompilation(out compilation);
		}
		return false;
	}

	internal ValueTask<Compilation> GetCompilationAsync(ProjectId projectId, CancellationToken cancellationToken)
	{
		return GetCompilationAsync(GetProject(projectId), cancellationToken);
	}

	internal async ValueTask<Compilation?> GetCompilationAsync(Project project, CancellationToken cancellationToken)
	{
		return (!project.SupportsCompilation) ? null : (await GetCompilationTracker(project.Id).GetCompilationAsync(this, cancellationToken).ConfigureAwait(continueOnCapturedContext: false));
	}

	public SolutionChanges GetChanges(Solution oldSolution)
	{
		if (oldSolution == null)
		{
			throw new ArgumentNullException("oldSolution");
		}
		return new SolutionChanges(this, oldSolution);
	}

	internal async Task<bool> ContainsSymbolsWithNameAsync(ProjectId id, Func<string, bool> predicate, SymbolFilter filter, CancellationToken cancellationToken)
	{
		bool? flag = GetCompilationTracker(id).ContainsSymbolsWithNameFromDeclarationOnlyCompilation(predicate, filter, cancellationToken);
		if (flag.HasValue)
		{
			return flag.Value;
		}
		return (await GetCompilationAsync(id, cancellationToken).ConfigureAwait(continueOnCapturedContext: false))?.ContainsSymbolsWithName(predicate, filter, cancellationToken) ?? false;
	}

	internal async Task<IEnumerable<Document>> GetDocumentsWithName(ProjectId id, Func<string, bool> predicate, SymbolFilter filter, CancellationToken cancellationToken)
	{
		IEnumerable<SyntaxTree> syntaxTreesWithNameFromDeclarationOnlyCompilation = GetCompilationTracker(id).GetSyntaxTreesWithNameFromDeclarationOnlyCompilation(predicate, filter, cancellationToken);
		if (syntaxTreesWithNameFromDeclarationOnlyCompilation != null)
		{
			return ConvertTreesToDocuments(id, syntaxTreesWithNameFromDeclarationOnlyCompilation);
		}
		Compilation compilation = await GetCompilationAsync(id, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		if (compilation == null)
		{
			return SpecializedCollections.EmptyEnumerable<Document>();
		}
		return ConvertTreesToDocuments(id, from s in compilation.GetSymbolsWithName(predicate, filter, cancellationToken)
			select s.DeclaringSyntaxReference.SyntaxTree);
	}

	private IEnumerable<Document> ConvertTreesToDocuments(ProjectId id, IEnumerable<SyntaxTree> trees)
	{
		foreach (SyntaxTree tree in trees)
		{
			Document document = GetDocument(tree, id);
			if (document != null)
			{
				yield return document;
			}
		}
	}

	public ProjectDependencyGraph GetProjectDependencyGraph()
	{
		return dependencyGraph;
	}

	private void CheckNotContainsProject(ProjectId projectId)
	{
		if (ContainsProject(projectId))
		{
			throw new InvalidOperationException(WorkspacesResources.ProjectAlreadyInSolution);
		}
	}

	private void CheckContainsProject(ProjectId projectId)
	{
		if (!ContainsProject(projectId))
		{
			throw new InvalidOperationException(WorkspacesResources.ProjectNotInSolution);
		}
	}

	private void CheckNotContainsProjectReference(ProjectId projectId, ProjectReference referencedProject)
	{
		if (GetProjectState(projectId).ProjectReferences.Contains(referencedProject))
		{
			throw new InvalidOperationException(WorkspacesResources.ProjectDirectlyReferencesTargetProject);
		}
	}

	private void CheckNotContainsTransitiveReference(ProjectId fromProjectId, ProjectId toProjectId)
	{
		if (dependencyGraph.GetProjectsThatThisProjectTransitivelyDependsOn(fromProjectId).Contains(toProjectId))
		{
			throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, WorkspacesResources.ProjectTransitivelyReferencesTargetProject, fromProjectId, toProjectId));
		}
	}

	private void CheckNotContainsDocument(DocumentId documentId)
	{
		if (ContainsDocument(documentId))
		{
			throw new InvalidOperationException(WorkspacesResources.DocumentAlreadyInSolution);
		}
	}

	private void CheckNotContainsAdditionalDocument(DocumentId documentId)
	{
		if (ContainsAdditionalDocument(documentId))
		{
			throw new InvalidOperationException(WorkspacesResources.DocumentAlreadyInSolution);
		}
	}

	private void CheckContainsDocument(DocumentId documentId)
	{
		if (!ContainsDocument(documentId))
		{
			throw new InvalidOperationException(WorkspacesResources.DocumentNotInSolution);
		}
	}

	private void CheckContainsAdditionalDocument(DocumentId documentId)
	{
		if (!ContainsAdditionalDocument(documentId))
		{
			throw CreateDocumentNotFoundException();
		}
	}

	private static Exception CreateDocumentNotFoundException()
	{
		return new InvalidOperationException(WorkspacesResources.DocumentNotInSolution);
	}
}
