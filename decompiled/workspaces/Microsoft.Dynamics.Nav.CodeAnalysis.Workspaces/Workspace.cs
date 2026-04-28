using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.CodeAnalysis.Analyzers;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Host;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Log;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Options;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces;

public abstract class Workspace : IDisposable
{
	private class TextTracker
	{
		private readonly Workspace workspace;

		private readonly DocumentId documentId;

		internal readonly SourceTextContainer TextContainer;

		private readonly EventHandler<TextChangeEventArgs> weakOnTextChanged;

		private readonly Action<Workspace, DocumentId, SourceText, PreservationMode> onChangedHandler;

		internal TextTracker(Workspace workspace, DocumentId documentId, SourceTextContainer textContainer, Action<Workspace, DocumentId, SourceText, PreservationMode> onChangedHandler)
		{
			this.workspace = workspace;
			this.documentId = documentId;
			TextContainer = textContainer;
			this.onChangedHandler = onChangedHandler;
			weakOnTextChanged = WeakEventHandler<TextChangeEventArgs>.Create(this, delegate(TextTracker target, object sender, TextChangeEventArgs args)
			{
				target.OnTextChanged(sender, args);
			});
		}

		public void Connect()
		{
			TextContainer.TextChanged += weakOnTextChanged;
		}

		public void Disconnect()
		{
			TextContainer.TextChanged -= weakOnTextChanged;
		}

		private void OnTextChanged(object sender, TextChangeEventArgs e)
		{
			onChangedHandler(workspace, documentId, e.NewText, PreservationMode.PreserveIdentity);
		}
	}

	private readonly NonReentrantLock serializationLock = new NonReentrantLock(useThisInstanceForSynchronization: true);

	private readonly NonReentrantLock stateLock = new NonReentrantLock(useThisInstanceForSynchronization: true);

	private Solution latestSolution;

	private readonly IWorkspaceTaskScheduler taskQueue;

	private readonly Dictionary<ProjectId, ISet<DocumentId>> projectToOpenDocumentsMap = new Dictionary<ProjectId, ISet<DocumentId>>();

	private readonly Dictionary<SourceTextContainer, DocumentId> bufferToDocumentInCurrentContextMap = new Dictionary<SourceTextContainer, DocumentId>();

	private readonly Dictionary<DocumentId, TextTracker> textTrackers = new Dictionary<DocumentId, TextTracker>();

	private readonly EventMap eventMap = new EventMap();

	private const string WorkspaceChangeEventName = "WorkspaceChanged";

	private const string WorkspaceFailedEventName = "WorkspaceFailed";

	private const string DocumentOpenedEventName = "DocumentOpened";

	private const string DocumentClosedEventName = "DocumentClosed";

	private const string DocumentActiveContextChangedName = "DocumentActiveContextChanged";

	private static readonly ConditionalWeakTable<SourceTextContainer, WorkspaceRegistration> bufferToWorkspaceRegistrationMap = new ConditionalWeakTable<SourceTextContainer, WorkspaceRegistration>();

	private static readonly ConditionalWeakTable<SourceTextContainer, WorkspaceRegistration>.CreateValueCallback createRegistration = CreateRegistration;

	internal bool TestHookPartialSolutionsDisabled { get; set; }

	public AbstractHostWorkspaceServices Services { get; }

	internal BranchId PrimaryBranchId { get; }

	protected internal virtual bool PartialSemanticsEnabled => false;

	public string Kind { get; }

	internal NonReentrantLock SerializationLock => serializationLock;

	public Solution CurrentSolution => Volatile.Read(ref latestSolution);

	internal WorkspaceSymbolReferenceLoader SymbolReferenceLoader { get; private set; }

	public OptionSet Options
	{
		get
		{
			return Services.GetService<IOptionService>()?.GetOptions();
		}
		set
		{
			Services.GetService<IOptionService>().SetOptions(value);
		}
	}

	public virtual bool CanOpenDocuments => false;

	internal virtual bool CanChangeActiveContextDocument => false;

	public event EventHandler<WorkspaceChangeEventArgs> WorkspaceChanged
	{
		add
		{
			eventMap.AddEventHandler("WorkspaceChanged", value);
		}
		remove
		{
			eventMap.RemoveEventHandler("WorkspaceChanged", value);
		}
	}

	public event EventHandler<WorkspaceDiagnosticEventArgs> WorkspaceFailed
	{
		add
		{
			eventMap.AddEventHandler("WorkspaceFailed", value);
		}
		remove
		{
			eventMap.RemoveEventHandler("WorkspaceFailed", value);
		}
	}

	public event EventHandler<DocumentEventArgs> DocumentOpened
	{
		add
		{
			eventMap.AddEventHandler("DocumentOpened", value);
		}
		remove
		{
			eventMap.RemoveEventHandler("DocumentOpened", value);
		}
	}

	public event EventHandler<DocumentEventArgs> DocumentClosed
	{
		add
		{
			eventMap.AddEventHandler("DocumentClosed", value);
		}
		remove
		{
			eventMap.RemoveEventHandler("DocumentClosed", value);
		}
	}

	internal event EventHandler<DocumentEventArgs> DocumentActiveContextChanged
	{
		add
		{
			eventMap.AddEventHandler("DocumentActiveContextChanged", value);
		}
		remove
		{
			eventMap.RemoveEventHandler("DocumentActiveContextChanged", value);
		}
	}

	protected Workspace(AbstractHostServices host, string workspaceKind)
	{
		PrimaryBranchId = BranchId.GetNextId();
		Kind = workspaceKind;
		Services = host.CreateWorkspaceServices(this);
		IWorkspaceTaskSchedulerFactory service = Services.GetService<IWorkspaceTaskSchedulerFactory>();
		taskQueue = service.CreateTaskQueue();
		latestSolution = CreateSolution(SolutionId.CreateNewId());
		SymbolReferenceLoader = new WorkspaceSymbolReferenceLoader();
	}

	protected internal Solution CreateSolution(SolutionInfo solutionInfo)
	{
		return new Solution(this, solutionInfo);
	}

	protected internal Solution CreateSolution(SolutionId id)
	{
		return CreateSolution(SolutionInfo.Create(id, VersionStamp.Create()));
	}

	protected internal Solution SetCurrentSolution(Solution solution)
	{
		Solution solution2 = Volatile.Read(ref latestSolution);
		if (solution == solution2)
		{
			return solution;
		}
		Solution solution3;
		while (true)
		{
			solution3 = solution.WithNewWorkspace(this, solution2.WorkspaceVersion + 1);
			Solution solution4 = Interlocked.CompareExchange(ref latestSolution, solution3, solution2);
			if (solution4 == solution2)
			{
				break;
			}
			solution2 = solution4;
		}
		return solution3;
	}

	internal void RemoveCachedSymbolReferences(IEnumerable<SymbolReferenceSpecification> symbols)
	{
		foreach (SymbolReferenceSpecification symbol in symbols)
		{
			SymbolReferenceLoader.InvalidateSymbol(symbol);
		}
	}

	protected internal Task ScheduleTask(Action action, CancellationToken token, string taskName = "Workspace.Task")
	{
		return taskQueue.ScheduleTask(action, taskName, token);
	}

	protected internal Task ScheduleTask(Action action, string taskName = "Workspace.Task")
	{
		return taskQueue.ScheduleTask(action, taskName);
	}

	protected internal Task<T> ScheduleTask<T>(Func<T> func, string taskName = "Workspace.Task")
	{
		return taskQueue.ScheduleTask(func, taskName);
	}

	protected internal virtual void OnDocumentTextChanged(Document document)
	{
	}

	protected internal virtual void OnDocumentClosing(DocumentId documentId)
	{
	}

	protected internal virtual void ClearSolution()
	{
		Solution currentSolution = CurrentSolution;
		ClearSolutionData();
		RaiseWorkspaceChangedEventAsync(WorkspaceChangeKind.SolutionCleared, currentSolution, CurrentSolution);
	}

	protected virtual void ClearSolutionData()
	{
		ClearOpenDocuments();
		SetCurrentSolution(CreateSolution(CurrentSolution.Id));
	}

	protected internal virtual void ClearProjectData(ProjectId projectId)
	{
		ClearOpenDocuments(projectId);
	}

	protected internal virtual void ClearDocumentData(DocumentId documentId)
	{
		ClearOpenDocument(documentId);
	}

	public void Dispose()
	{
		Dispose(finalize: false);
		GC.SuppressFinalize(this);
	}

	protected virtual void Dispose(bool finalize)
	{
		if (!finalize)
		{
			ClearSolutionData();
		}
	}

	protected internal void OnSolutionAdded(SolutionInfo solutionInfo)
	{
		using (serializationLock.DisposableWait())
		{
			Solution currentSolution = CurrentSolution;
			CheckSolutionIsEmpty();
			SetCurrentSolution(CreateSolution(solutionInfo));
			solutionInfo.Projects.Do(delegate(ProjectInfo p)
			{
				OnProjectAdded_NoLock(p, silent: true);
			});
			Solution currentSolution2 = CurrentSolution;
			RaiseWorkspaceChangedEventAsync(WorkspaceChangeKind.SolutionAdded, currentSolution, currentSolution2);
		}
	}

	protected internal void OnSolutionReloaded(SolutionInfo reloadedSolutionInfo)
	{
		using (serializationLock.DisposableWait())
		{
			Solution currentSolution = CurrentSolution;
			Solution solution = SetCurrentSolution(CreateSolution(reloadedSolutionInfo));
			reloadedSolutionInfo.Projects.Do(delegate(ProjectInfo pi)
			{
				OnProjectAdded_NoLock(pi, silent: true);
			});
			solution = AdjustReloadedSolution(currentSolution, CurrentSolution);
			solution = SetCurrentSolution(solution);
			RaiseWorkspaceChangedEventAsync(WorkspaceChangeKind.SolutionReloaded, currentSolution, solution);
		}
	}

	protected internal void OnSolutionRemoved()
	{
		using (serializationLock.DisposableWait())
		{
			Solution currentSolution = CurrentSolution;
			ClearSolutionData();
			SetCurrentSolution(CreateSolution(SolutionId.CreateNewId()));
			RaiseWorkspaceChangedEventAsync(WorkspaceChangeKind.SolutionRemoved, currentSolution, CurrentSolution);
		}
	}

	protected internal void OnProjectAdded(ProjectInfo projectInfo)
	{
		OnProjectAdded(projectInfo, silent: false);
	}

	private void OnProjectAdded(ProjectInfo projectInfo, bool silent)
	{
		using (serializationLock.DisposableWait())
		{
			OnProjectAdded_NoLock(projectInfo, silent);
		}
	}

	private void OnProjectAdded_NoLock(ProjectInfo projectInfo, bool silent)
	{
		ProjectId id = projectInfo.Id;
		CheckProjectIsNotInCurrentSolution(id);
		Solution currentSolution = CurrentSolution;
		Solution newSolution = SetCurrentSolution(currentSolution.AddProject(projectInfo));
		if (!silent)
		{
			RaiseWorkspaceChangedEventAsync(WorkspaceChangeKind.ProjectAdded, currentSolution, newSolution, id);
		}
	}

	protected internal virtual void OnProjectReloaded(ProjectInfo reloadedProjectInfo)
	{
		using (serializationLock.DisposableWait())
		{
			ProjectId id = reloadedProjectInfo.Id;
			CheckProjectIsInCurrentSolution(id);
			Solution currentSolution = CurrentSolution;
			Solution solution = currentSolution.RemoveProject(id).AddProject(reloadedProjectInfo);
			solution = AdjustReloadedProject(currentSolution.GetProject(id), solution.GetProject(id)).Solution;
			solution = SetCurrentSolution(solution);
			RaiseWorkspaceChangedEventAsync(WorkspaceChangeKind.ProjectReloaded, currentSolution, solution, id);
		}
	}

	protected internal virtual void OnProjectRemoved(ProjectId projectId)
	{
		using (serializationLock.DisposableWait())
		{
			CheckProjectIsInCurrentSolution(projectId);
			CheckProjectCanBeRemoved(projectId);
			Solution currentSolution = CurrentSolution;
			ClearProjectData(projectId);
			Solution newSolution = SetCurrentSolution(currentSolution.RemoveProject(projectId));
			RaiseWorkspaceChangedEventAsync(WorkspaceChangeKind.ProjectRemoved, currentSolution, newSolution, projectId);
		}
	}

	protected virtual void CheckProjectCanBeRemoved(ProjectId projectId)
	{
		CheckProjectDoesNotContainOpenDocuments(projectId);
	}

	protected internal void OnAssemblyNameChanged(ProjectId projectId, string assemblyName)
	{
		using (serializationLock.DisposableWait())
		{
			CheckProjectIsInCurrentSolution(projectId);
			Solution currentSolution = CurrentSolution;
			Solution newSolution = SetCurrentSolution(currentSolution.WithProjectAssemblyName(projectId, assemblyName));
			RaiseWorkspaceChangedEventAsync(WorkspaceChangeKind.ProjectChanged, currentSolution, newSolution, projectId);
		}
	}

	protected internal void OnOutputFilePathChanged(ProjectId projectId, string outputFilePath)
	{
		using (serializationLock.DisposableWait())
		{
			CheckProjectIsInCurrentSolution(projectId);
			Solution currentSolution = CurrentSolution;
			Solution newSolution = SetCurrentSolution(currentSolution.WithProjectOutputFilePath(projectId, outputFilePath));
			RaiseWorkspaceChangedEventAsync(WorkspaceChangeKind.ProjectChanged, currentSolution, newSolution, projectId);
		}
	}

	protected internal void OnProjectNameChanged(ProjectId projectId, string name, string filePath)
	{
		using (serializationLock.DisposableWait())
		{
			CheckProjectIsInCurrentSolution(projectId);
			Solution currentSolution = CurrentSolution;
			Solution newSolution = SetCurrentSolution(currentSolution.WithProjectName(projectId, name).WithProjectFilePath(projectId, filePath));
			RaiseWorkspaceChangedEventAsync(WorkspaceChangeKind.ProjectChanged, currentSolution, newSolution, projectId);
		}
	}

	protected internal void OnPackageCachePathsChanged(ProjectId projectId, IEnumerable<string> packageCachePaths)
	{
		using (serializationLock.DisposableWait())
		{
			CheckProjectIsInCurrentSolution(projectId);
			Solution currentSolution = CurrentSolution;
			Solution newSolution = SetCurrentSolution(currentSolution.WithProjectPackageCachePath(projectId, packageCachePaths));
			RaiseWorkspaceChangedEventAsync(WorkspaceChangeKind.ProjectChanged, currentSolution, newSolution, projectId);
		}
	}

	protected internal void OnAssemblyProbingPathsChanged(ProjectId projectId, IReadOnlyList<string> probingPaths)
	{
		using (serializationLock.DisposableWait())
		{
			CheckProjectIsInCurrentSolution(projectId);
			Solution currentSolution = CurrentSolution;
			Solution newSolution = SetCurrentSolution(currentSolution.WithAssemblyProbingPaths(projectId, probingPaths));
			RaiseWorkspaceChangedEventAsync(WorkspaceChangeKind.ProjectChanged, currentSolution, newSolution, projectId);
		}
	}

	protected internal void OnRuleSetPathChanged(ProjectId projectId, string ruleSetPath)
	{
		using (serializationLock.DisposableWait())
		{
			CheckProjectIsInCurrentSolution(projectId);
			Solution currentSolution = CurrentSolution;
			Solution newSolution = SetCurrentSolution(currentSolution.WithRuleSetPath(projectId, ruleSetPath));
			RaiseWorkspaceChangedEventAsync(WorkspaceChangeKind.ProjectChanged, currentSolution, newSolution, projectId);
		}
	}

	protected internal void OnNamespaceTemplateChanged(ProjectId projectId, string? namespaceTemplate)
	{
		using (serializationLock.DisposableWait())
		{
			CheckProjectIsInCurrentSolution(projectId);
			Solution currentSolution = CurrentSolution;
			Solution newSolution = SetCurrentSolution(currentSolution.WithNamespaceTemplate(projectId, namespaceTemplate));
			RaiseWorkspaceChangedEventAsync(WorkspaceChangeKind.ProjectChanged, currentSolution, newSolution, projectId);
		}
	}

	protected internal void OnExternalRulesetsEnabledChanged(ProjectId projectId, bool externalRulesetsEnabled)
	{
		using (serializationLock.DisposableWait())
		{
			CheckProjectIsInCurrentSolution(projectId);
			Solution currentSolution = CurrentSolution;
			Solution newSolution = SetCurrentSolution(currentSolution.WithExternalRulesetsEnabled(projectId, externalRulesetsEnabled));
			RaiseWorkspaceChangedEventAsync(WorkspaceChangeKind.ProjectChanged, currentSolution, newSolution, projectId);
		}
	}

	protected internal void OnCompilationOptionsChanged(ProjectId projectId, CompilationOptions options)
	{
		using (serializationLock.DisposableWait())
		{
			CheckProjectIsInCurrentSolution(projectId);
			Solution currentSolution = CurrentSolution;
			Solution newSolution = SetCurrentSolution(currentSolution.WithProjectCompilationOptions(projectId, options));
			RaiseWorkspaceChangedEventAsync(WorkspaceChangeKind.ProjectChanged, currentSolution, newSolution, projectId);
		}
	}

	protected internal void OnParseOptionsChanged(ProjectId projectId, ParseOptions options)
	{
		using (serializationLock.DisposableWait())
		{
			CheckProjectIsInCurrentSolution(projectId);
			Solution currentSolution = CurrentSolution;
			Solution newSolution = SetCurrentSolution(currentSolution.WithProjectParseOptions(projectId, options));
			RaiseWorkspaceChangedEventAsync(WorkspaceChangeKind.ProjectChanged, currentSolution, newSolution, projectId);
		}
	}

	protected internal void OnEnableCodeActionsChanged(ProjectId projectId, bool enableCodeActions)
	{
		using (serializationLock.DisposableWait())
		{
			CheckProjectIsInCurrentSolution(projectId);
			Solution currentSolution = CurrentSolution;
			Solution newSolution = SetCurrentSolution(currentSolution.WithProjectEnableCodeActions(projectId, enableCodeActions));
			RaiseWorkspaceChangedEventAsync(WorkspaceChangeKind.ProjectChanged, currentSolution, newSolution, projectId);
		}
	}

	protected internal void OnIncrementalBuildChanged(ProjectId projectId, bool incrementalBuild)
	{
		using (serializationLock.DisposableWait())
		{
			CheckProjectIsInCurrentSolution(projectId);
			Solution currentSolution = CurrentSolution;
			Solution newSolution = SetCurrentSolution(currentSolution.WithProjectIncrementalBuild(projectId, incrementalBuild));
			RaiseWorkspaceChangedEventAsync(WorkspaceChangeKind.ProjectChanged, currentSolution, newSolution, projectId);
		}
	}

	protected internal void OnCaptureSymbolUsageChanged(ProjectId projectId, bool captureSymbolUsage)
	{
		using (serializationLock.DisposableWait())
		{
			CheckProjectIsInCurrentSolution(projectId);
			Solution currentSolution = CurrentSolution;
			Solution newSolution = SetCurrentSolution(currentSolution.WithCaptureSymbolUsage(projectId, captureSymbolUsage));
			RaiseWorkspaceChangedEventAsync(WorkspaceChangeKind.ProjectChanged, currentSolution, newSolution, projectId);
		}
	}

	protected internal void OnProjectDefinitionChanged(ProjectId projectId, ProjectDefinition projectDefinition)
	{
		using (serializationLock.DisposableWait())
		{
			CheckProjectIsInCurrentSolution(projectId);
			Solution currentSolution = CurrentSolution;
			Solution newSolution = SetCurrentSolution(currentSolution.WithProjectDefinition(projectId, projectDefinition));
			RaiseWorkspaceChangedEventAsync(WorkspaceChangeKind.ProjectChanged, currentSolution, newSolution, projectId);
		}
	}

	protected internal void OnExpectedProjectReferencesChanged(ProjectId projectId, ISet<ProjectDefinition>? expectedProjectReferences)
	{
		using (serializationLock.DisposableWait())
		{
			CheckProjectIsInCurrentSolution(projectId);
			Solution currentSolution = CurrentSolution;
			Solution newSolution = SetCurrentSolution(currentSolution.WithExpectedProjectReferences(projectId, expectedProjectReferences));
			RaiseWorkspaceChangedEventAsync(WorkspaceChangeKind.ProjectChanged, currentSolution, newSolution, projectId);
		}
	}

	protected internal void OnProjectReferenceAdded(ProjectId projectId, ProjectReference projectReference)
	{
		using (serializationLock.DisposableWait())
		{
			CheckProjectIsInCurrentSolution(projectId);
			CheckProjectIsInCurrentSolution(projectReference.ProjectId);
			CheckProjectDoesNotHaveProjectReference(projectId, projectReference);
			CheckProjectDoesNotHaveTransitiveProjectReference(projectId, projectReference.ProjectId);
			Solution currentSolution = CurrentSolution;
			Solution newSolution = SetCurrentSolution(currentSolution.AddProjectReference(projectId, projectReference));
			RaiseWorkspaceChangedEventAsync(WorkspaceChangeKind.ProjectChanged, currentSolution, newSolution, projectId);
		}
	}

	protected internal void OnProjectReferenceRemoved(ProjectId projectId, ProjectReference projectReference)
	{
		using (serializationLock.DisposableWait())
		{
			CheckProjectIsInCurrentSolution(projectId);
			CheckProjectIsInCurrentSolution(projectReference.ProjectId);
			CheckProjectHasProjectReference(projectId, projectReference);
			Solution currentSolution = CurrentSolution;
			Solution newSolution = SetCurrentSolution(currentSolution.RemoveProjectReference(projectId, projectReference));
			RaiseWorkspaceChangedEventAsync(WorkspaceChangeKind.ProjectChanged, currentSolution, newSolution, projectId);
		}
	}

	protected internal void OnSymbolReferenceAdded(ProjectId projectId, SymbolReferenceSpecification symbolReference)
	{
		using (serializationLock.DisposableWait())
		{
			CheckProjectIsInCurrentSolution(projectId);
			CheckProjectDoesNotHaveSymbolReference(projectId, symbolReference);
			Solution currentSolution = CurrentSolution;
			Solution newSolution = SetCurrentSolution(currentSolution.AddSymbolReference(projectId, symbolReference));
			RaiseWorkspaceChangedEventAsync(WorkspaceChangeKind.ProjectChanged, currentSolution, newSolution, projectId);
		}
	}

	protected internal void OnSymbolReferenceRemoved(ProjectId projectId, SymbolReferenceSpecification symbolReference)
	{
		using (serializationLock.DisposableWait())
		{
			CheckProjectIsInCurrentSolution(projectId);
			CheckProjectHasSymbolReference(projectId, symbolReference);
			Solution currentSolution = CurrentSolution;
			Solution newSolution = SetCurrentSolution(currentSolution.RemoveSymbolReference(projectId, symbolReference));
			RaiseWorkspaceChangedEventAsync(WorkspaceChangeKind.ProjectChanged, currentSolution, newSolution, projectId);
		}
	}

	protected internal void OnAnalyzerReferenceAdded(ProjectId projectId, AnalyzerReference analyzerReference)
	{
		using (serializationLock.DisposableWait())
		{
			CheckProjectIsInCurrentSolution(projectId);
			CheckProjectDoesNotHaveAnalyzerReference(projectId, analyzerReference);
			Solution currentSolution = CurrentSolution;
			Solution newSolution = SetCurrentSolution(currentSolution.AddAnalyzerReference(projectId, analyzerReference));
			RaiseWorkspaceChangedEventAsync(WorkspaceChangeKind.ProjectChanged, currentSolution, newSolution, projectId);
		}
	}

	protected internal void OnAnalyzerReferenceRemoved(ProjectId projectId, AnalyzerReference analyzerReference)
	{
		using (serializationLock.DisposableWait())
		{
			CheckProjectIsInCurrentSolution(projectId);
			CheckProjectHasAnalyzerReference(projectId, analyzerReference);
			Solution currentSolution = CurrentSolution;
			Solution newSolution = SetCurrentSolution(currentSolution.RemoveAnalyzerReference(projectId, analyzerReference));
			RaiseWorkspaceChangedEventAsync(WorkspaceChangeKind.ProjectChanged, currentSolution, newSolution, projectId);
		}
	}

	protected internal void OnEnableCodeAnalysisInBackgroundChanged(ProjectId projectId, BackgroundCodeAnalysisScope enableCodeAnalysisInBackground)
	{
		using (serializationLock.DisposableWait())
		{
			CheckProjectIsInCurrentSolution(projectId);
			Solution currentSolution = CurrentSolution;
			Solution newSolution = SetCurrentSolution(currentSolution.WithBackgroundCodeAnalysisScope(projectId, enableCodeAnalysisInBackground));
			RaiseWorkspaceChangedEventAsync(WorkspaceChangeKind.ProjectChanged, currentSolution, newSolution, projectId);
		}
	}

	protected internal void OnOutputAnalyzerStatisticsChanged(ProjectId projectId, bool outputAnalyzerStatistics)
	{
		using (serializationLock.DisposableWait())
		{
			CheckProjectIsInCurrentSolution(projectId);
			Solution currentSolution = CurrentSolution;
			Solution newSolution = SetCurrentSolution(currentSolution.WithOutputAnalyzerStatistics(projectId, outputAnalyzerStatistics));
			RaiseWorkspaceChangedEventAsync(WorkspaceChangeKind.ProjectChanged, currentSolution, newSolution, projectId);
		}
	}

	protected internal Task OnDocumentAdded(DocumentInfo documentInfo, bool handleRad = false)
	{
		using (serializationLock.DisposableWait())
		{
			DocumentId id = documentInfo.Id;
			CheckProjectIsInCurrentSolution(id.ProjectId);
			CheckDocumentIsNotInCurrentSolution(id);
			Solution currentSolution = CurrentSolution;
			Solution newSolution = SetCurrentSolution(currentSolution.AddDocument(documentInfo));
			return RaiseWorkspaceChangedEventAsync(WorkspaceChangeKind.DocumentAdded, currentSolution, newSolution, null, id, handleRad);
		}
	}

	protected internal void OnDocumentReloaded(DocumentInfo newDocumentInfo, bool handleRad = false)
	{
		using (serializationLock.DisposableWait())
		{
			DocumentId id = newDocumentInfo.Id;
			CheckProjectIsInCurrentSolution(id.ProjectId);
			CheckDocumentIsInCurrentSolution(id);
			Solution currentSolution = CurrentSolution;
			Solution newSolution = SetCurrentSolution(currentSolution.RemoveDocument(id).AddDocument(newDocumentInfo));
			RaiseWorkspaceChangedEventAsync(WorkspaceChangeKind.DocumentReloaded, currentSolution, newSolution, null, id, handleRad);
		}
	}

	protected internal Task OnDocumentRemoved(DocumentId documentId, bool handleRad = false)
	{
		using (serializationLock.DisposableWait())
		{
			CheckDocumentIsInCurrentSolution(documentId);
			CheckDocumentCanBeRemoved(documentId);
			Solution currentSolution = CurrentSolution;
			ClearDocumentData(documentId);
			Solution newSolution = SetCurrentSolution(currentSolution.RemoveDocument(documentId));
			return RaiseWorkspaceChangedEventAsync(WorkspaceChangeKind.DocumentRemoved, currentSolution, newSolution, null, documentId, handleRad);
		}
	}

	protected virtual void CheckDocumentCanBeRemoved(DocumentId documentId)
	{
		CheckDocumentIsClosed(documentId);
	}

	protected internal void OnDocumentTextLoaderChanged(DocumentId documentId, TextLoader loader)
	{
		using (serializationLock.DisposableWait())
		{
			CheckDocumentIsInCurrentSolution(documentId);
			Solution currentSolution = CurrentSolution;
			Solution currentSolution2 = currentSolution.WithDocumentTextLoader(documentId, loader, PreservationMode.PreserveValue);
			currentSolution2 = SetCurrentSolution(currentSolution2);
			Document document = currentSolution2.GetDocument(documentId);
			OnDocumentTextChanged(document);
			RaiseWorkspaceChangedEventAsync(WorkspaceChangeKind.DocumentChanged, currentSolution, currentSolution2, null, documentId);
		}
	}

	protected internal void OnAdditionalDocumentTextLoaderChanged(DocumentId documentId, TextLoader loader)
	{
		using (serializationLock.DisposableWait())
		{
			CheckAdditionalDocumentIsInCurrentSolution(documentId);
			Solution currentSolution = CurrentSolution;
			Solution currentSolution2 = currentSolution.WithAdditionalDocumentTextLoader(documentId, loader, PreservationMode.PreserveValue);
			currentSolution2 = SetCurrentSolution(currentSolution2);
			RaiseWorkspaceChangedEventAsync(WorkspaceChangeKind.AdditionalDocumentChanged, currentSolution, currentSolution2, null, documentId);
		}
	}

	protected internal Task OnDocumentTextChanged(DocumentId documentId, SourceText newText, PreservationMode mode, bool handleRad = true)
	{
		using (serializationLock.DisposableWait())
		{
			CheckDocumentIsInCurrentSolution(documentId);
			Solution currentSolution = CurrentSolution;
			Solution solution = SetCurrentSolution(currentSolution.WithDocumentText(documentId, newText, mode));
			Document document = solution.GetDocument(documentId);
			OnDocumentTextChanged(document);
			return RaiseWorkspaceChangedEventAsync(WorkspaceChangeKind.DocumentChanged, currentSolution, solution, null, documentId, handleRad);
		}
	}

	protected internal void OnAdditionalDocumentTextChanged(DocumentId documentId, SourceText newText, PreservationMode mode)
	{
		using (serializationLock.DisposableWait())
		{
			CheckAdditionalDocumentIsInCurrentSolution(documentId);
			Solution currentSolution = CurrentSolution;
			Solution solution = SetCurrentSolution(currentSolution.WithAdditionalDocumentText(documentId, newText, mode));
			solution.GetAdditionalDocument(documentId);
			RaiseWorkspaceChangedEventAsync(WorkspaceChangeKind.AdditionalDocumentChanged, currentSolution, solution, null, documentId);
		}
	}

	protected internal void OnAdditionalDocumentAdded(DocumentInfo documentInfo)
	{
		using (serializationLock.DisposableWait())
		{
			DocumentId id = documentInfo.Id;
			CheckProjectIsInCurrentSolution(id.ProjectId);
			CheckDocumentIsNotInCurrentSolution(id);
			Solution currentSolution = CurrentSolution;
			Solution newSolution = SetCurrentSolution(currentSolution.AddAdditionalDocument(documentInfo));
			RaiseWorkspaceChangedEventAsync(WorkspaceChangeKind.AdditionalDocumentAdded, currentSolution, newSolution, null, id);
		}
	}

	protected internal void OnAdditionalDocumentRemoved(DocumentId documentId)
	{
		using (serializationLock.DisposableWait())
		{
			CheckAdditionalDocumentIsInCurrentSolution(documentId);
			CheckDocumentCanBeRemoved(documentId);
			Solution currentSolution = CurrentSolution;
			ClearDocumentData(documentId);
			Solution newSolution = SetCurrentSolution(currentSolution.RemoveAdditionalDocument(documentId));
			RaiseWorkspaceChangedEventAsync(WorkspaceChangeKind.AdditionalDocumentRemoved, currentSolution, newSolution, null, documentId);
		}
	}

	protected internal void OnActiveDocumentChanged(DocumentId documentId)
	{
		using (serializationLock.DisposableWait())
		{
			if (CurrentSolution != null && CurrentSolution.ContainsDocument(documentId))
			{
				RaiseWorkspaceChangedEventAsync(WorkspaceChangeKind.ActiveDocumentChanged, CurrentSolution, CurrentSolution, documentId.ProjectId, documentId);
			}
		}
	}

	public virtual bool CanApplyChange(ApplyChangesKind feature)
	{
		return false;
	}

	public virtual bool TryApplyChanges(Solution newSolution)
	{
		using (Logger.LogBlock(FunctionId.Workspace_ApplyChanges, CancellationToken.None))
		{
			if (newSolution.Workspace != this)
			{
				return false;
			}
			Solution currentSolution = CurrentSolution;
			if (newSolution.WorkspaceVersion != currentSolution.WorkspaceVersion)
			{
				return false;
			}
			if (currentSolution.BranchId == newSolution.BranchId)
			{
				return true;
			}
			SolutionChanges changes = newSolution.GetChanges(currentSolution);
			CheckAllowedSolutionChanges(changes);
			foreach (Project addedProject in changes.GetAddedProjects())
			{
				ApplyProjectAdded(CreateProjectInfo(addedProject));
			}
			foreach (ProjectChanges projectChange in changes.GetProjectChanges())
			{
				ApplyProjectChanges(projectChange);
			}
			foreach (Project removedProject in changes.GetRemovedProjects())
			{
				ApplyProjectRemoved(removedProject.Id);
			}
			return true;
		}
	}

	private void CheckAllowedSolutionChanges(SolutionChanges solutionChanges)
	{
		if (solutionChanges.GetRemovedProjects().Any() && !CanApplyChange(ApplyChangesKind.RemoveProject))
		{
			throw new NotSupportedException(WorkspacesResources.RemovingProjectsNotSupported);
		}
		if (solutionChanges.GetAddedProjects().Any() && !CanApplyChange(ApplyChangesKind.AddProject))
		{
			throw new NotSupportedException(WorkspacesResources.AddingProjectsNotSupported);
		}
		foreach (ProjectChanges projectChange in solutionChanges.GetProjectChanges())
		{
			CheckAllowedProjectChanges(projectChange);
		}
	}

	private void CheckAllowedProjectChanges(ProjectChanges projectChanges)
	{
		if (projectChanges.OldProject.CompilationOptions != projectChanges.NewProject.CompilationOptions && !CanApplyChange(ApplyChangesKind.ChangeCompilationOptions))
		{
			throw new NotSupportedException(WorkspacesResources.ChangingCompilationOptionsNotSupported);
		}
		if (projectChanges.GetAddedDocuments().Any() && !CanApplyChange(ApplyChangesKind.AddDocument))
		{
			throw new NotSupportedException(WorkspacesResources.AddingDocumentsNotSupported);
		}
		if (projectChanges.GetRemovedDocuments().Any() && !CanApplyChange(ApplyChangesKind.RemoveDocument))
		{
			throw new NotSupportedException(WorkspacesResources.RemovingDocumentsNotSupported);
		}
		if (projectChanges.GetChangedDocuments().Any() && !CanApplyChange(ApplyChangesKind.ChangeDocument))
		{
			throw new NotSupportedException(WorkspacesResources.ChangingDocumentsNotSupported);
		}
		if (projectChanges.GetAddedAdditionalDocuments().Any() && !CanApplyChange(ApplyChangesKind.AddAdditionalDocument))
		{
			throw new NotSupportedException(WorkspacesResources.AddingAdditionalDocumentsNotSupported);
		}
		if (projectChanges.GetRemovedAdditionalDocuments().Any() && !CanApplyChange(ApplyChangesKind.RemoveAdditionalDocument))
		{
			throw new NotSupportedException(WorkspacesResources.RemovingAdditionalDocumentsIsNotSupported);
		}
		if (projectChanges.GetChangedAdditionalDocuments().Any() && !CanApplyChange(ApplyChangesKind.ChangeAdditionalDocument))
		{
			throw new NotSupportedException(WorkspacesResources.ChangingAdditionalDocumentsIsNotSupported);
		}
		if (projectChanges.GetAddedProjectReferences().Any() && !CanApplyChange(ApplyChangesKind.AddProjectReference))
		{
			throw new NotSupportedException(WorkspacesResources.AddingProjectReferencesNotSupported);
		}
		if (projectChanges.GetRemovedProjectReferences().Any() && !CanApplyChange(ApplyChangesKind.RemoveProjectReference))
		{
			throw new NotSupportedException(WorkspacesResources.RemovingProjectReferencesNotSupported);
		}
		if (projectChanges.GetAddedSymbolReferences().Any() && !CanApplyChange(ApplyChangesKind.AddSymbolReference))
		{
			throw new NotSupportedException(WorkspacesResources.AddingProjectReferencesNotSupported);
		}
		if (projectChanges.GetRemovedSymbolReferences().Any() && !CanApplyChange(ApplyChangesKind.RemoveSymbolReference))
		{
			throw new NotSupportedException(WorkspacesResources.RemovingProjectReferencesNotSupported);
		}
		if (projectChanges.GetAddedAnalyzerReferences().Any() && !CanApplyChange(ApplyChangesKind.AddAnalyzerReference))
		{
			throw new NotSupportedException(WorkspacesResources.AddingAnalyzerReferencesNotSupported);
		}
		if (projectChanges.GetRemovedAnalyzerReferences().Any() && !CanApplyChange(ApplyChangesKind.RemoveAnalyzerReference))
		{
			throw new NotSupportedException(WorkspacesResources.RemovingAnalyzerReferencesNotSupported);
		}
	}

	protected virtual void ApplyProjectChanges(ProjectChanges projectChanges)
	{
		if (projectChanges.OldProject.PackageCachePaths != projectChanges.NewProject.PackageCachePaths || !projectChanges.OldProject.PackageCachePaths.SetEquals(projectChanges.NewProject.PackageCachePaths))
		{
			ApplyPackageCachePathChanged(projectChanges.ProjectId, projectChanges.NewProject.PackageCachePaths);
		}
		if (projectChanges.OldProject.AssemblyProbingPaths != projectChanges.NewProject.AssemblyProbingPaths || !projectChanges.OldProject.AssemblyProbingPaths.SetEquals(projectChanges.NewProject.AssemblyProbingPaths))
		{
			ApplyAssemblyProbingPathsChanged(projectChanges.ProjectId, projectChanges.NewProject.AssemblyProbingPaths);
		}
		if (!string.Equals(projectChanges.OldProject.RuleSetPath, projectChanges.NewProject.RuleSetPath, StringComparison.CurrentCulture))
		{
			ApplyRuleSetPathChanged(projectChanges.ProjectId, projectChanges.NewProject.RuleSetPath);
		}
		if (!string.Equals(projectChanges.OldProject.NamespaceTemplate, projectChanges.NewProject.NamespaceTemplate, StringComparison.InvariantCulture))
		{
			ApplyNamespaceTemplateChanged(projectChanges.ProjectId, projectChanges.NewProject.NamespaceTemplate);
		}
		if (projectChanges.OldProject.CompilationOptions != projectChanges.NewProject.CompilationOptions)
		{
			ApplyCompilationOptionsChanged(projectChanges.ProjectId, projectChanges.NewProject.CompilationOptions);
		}
		if (projectChanges.OldProject.ParseOptions != projectChanges.NewProject.ParseOptions)
		{
			ApplyParseOptionsChanged(projectChanges.ProjectId, projectChanges.NewProject.ParseOptions);
		}
		if (projectChanges.OldProject.EnableCodeActions != projectChanges.NewProject.EnableCodeActions)
		{
			ApplyEnableCodeActionsChanged(projectChanges.ProjectId, projectChanges.NewProject.EnableCodeActions);
		}
		if (projectChanges.OldProject.IncrementalBuild != projectChanges.NewProject.IncrementalBuild)
		{
			ApplyIncrementalBuildChanged(projectChanges.ProjectId, projectChanges.NewProject.IncrementalBuild);
		}
		if (projectChanges.OldProject.CaptureSymbolUsage != projectChanges.NewProject.CaptureSymbolUsage)
		{
			ApplyCaptureSymbolUsageChanged(projectChanges.ProjectId, projectChanges.NewProject.CaptureSymbolUsage);
		}
		if (projectChanges.OldProject.ProjectDefinition != projectChanges.NewProject.ProjectDefinition)
		{
			ApplyProjectDefinitionChanged(projectChanges.ProjectId, projectChanges.NewProject.ProjectDefinition);
		}
		ISet<ProjectDefinition>? expectedProjectReferences = projectChanges.OldProject.ExpectedProjectReferences;
		if (expectedProjectReferences == null || !expectedProjectReferences.SetEquals(projectChanges.NewProject.ExpectedProjectReferences))
		{
			ApplyExpectedProjectReferencesChanged(projectChanges.ProjectId, projectChanges.NewProject.ExpectedProjectReferences);
		}
		foreach (ProjectReference removedProjectReference in projectChanges.GetRemovedProjectReferences())
		{
			ApplyProjectReferenceRemoved(projectChanges.ProjectId, removedProjectReference);
		}
		foreach (ProjectReference addedProjectReference in projectChanges.GetAddedProjectReferences())
		{
			ApplyProjectReferenceAdded(projectChanges.ProjectId, addedProjectReference);
		}
		if (projectChanges.OldProject.BackgroundCodeAnalysisScope != projectChanges.NewProject.BackgroundCodeAnalysisScope)
		{
			ApplyBackgroundCodeAnalysisScope(projectChanges.ProjectId, projectChanges.NewProject.BackgroundCodeAnalysisScope);
		}
		if (projectChanges.OldProject.OutputAnalyzerStatistics != projectChanges.NewProject.OutputAnalyzerStatistics)
		{
			ApplyOutputAnalyzerStatistics(projectChanges.ProjectId, projectChanges.NewProject.OutputAnalyzerStatistics);
		}
		foreach (SymbolReferenceSpecification removedSymbolReference in projectChanges.GetRemovedSymbolReferences())
		{
			ApplySymbolReferenceRemoved(projectChanges.ProjectId, removedSymbolReference);
		}
		foreach (SymbolReferenceSpecification addedSymbolReference in projectChanges.GetAddedSymbolReferences())
		{
			ApplySymbolReferenceAdded(projectChanges.ProjectId, addedSymbolReference);
		}
		foreach (AnalyzerReference removedAnalyzerReference in projectChanges.GetRemovedAnalyzerReferences())
		{
			ApplyAnalyzerReferenceRemoved(projectChanges.ProjectId, removedAnalyzerReference);
		}
		foreach (AnalyzerReference addedAnalyzerReference in projectChanges.GetAddedAnalyzerReferences())
		{
			ApplyAnalyzerReferenceAdded(projectChanges.ProjectId, addedAnalyzerReference);
		}
		foreach (DocumentId removedDocument in projectChanges.GetRemovedDocuments())
		{
			ApplyDocumentRemoved(removedDocument);
		}
		foreach (DocumentId removedAdditionalDocument in projectChanges.GetRemovedAdditionalDocuments())
		{
			ApplyAdditionalDocumentRemoved(removedAdditionalDocument);
		}
		foreach (DocumentId addedDocument in projectChanges.GetAddedDocuments())
		{
			Document? document = projectChanges.NewProject.GetDocument(addedDocument);
			SourceText textForced = GetTextForced(document);
			DocumentInfo info = CreateDocumentInfoWithoutText(document);
			ApplyDocumentAdded(info, textForced);
		}
		foreach (DocumentId addedAdditionalDocument in projectChanges.GetAddedAdditionalDocuments())
		{
			TextDocument additionalDocument = projectChanges.NewProject.GetAdditionalDocument(addedAdditionalDocument);
			SourceText textForced2 = GetTextForced(additionalDocument);
			DocumentInfo info2 = CreateDocumentInfoWithoutText(additionalDocument);
			ApplyAdditionalDocumentAdded(info2, textForced2);
		}
		foreach (DocumentId changedDocument in projectChanges.GetChangedDocuments())
		{
			Document document2 = projectChanges.OldProject.GetDocument(changedDocument);
			Document document3 = projectChanges.NewProject.GetDocument(changedDocument);
			SourceText text3;
			if (!document2.TryGetText(out SourceText text))
			{
				SourceText text2 = document3.GetTextAsync(CancellationToken.None).WaitAndGetResult(CancellationToken.None);
				ApplyDocumentTextChanged(changedDocument, text2);
			}
			else if (!document3.TryGetText(out text3))
			{
				IEnumerable<TextChange> changes = document3.GetTextChangesAsync(document2, CancellationToken.None).WaitAndGetResult(CancellationToken.None);
				ApplyDocumentTextChanged(changedDocument, text.WithChanges(changes));
			}
			else
			{
				ApplyDocumentTextChanged(changedDocument, text3);
			}
		}
		foreach (DocumentId changedAdditionalDocument in projectChanges.GetChangedAdditionalDocuments())
		{
			projectChanges.OldProject.GetAdditionalDocument(changedAdditionalDocument);
			SourceText text4 = projectChanges.NewProject.GetAdditionalDocument(changedAdditionalDocument).GetTextAsync(CancellationToken.None).WaitAndGetResult(CancellationToken.None);
			ApplyAdditionalDocumentTextChanged(changedAdditionalDocument, text4);
		}
	}

	[Conditional("DEBUG")]
	private static void CheckNoChanges(Solution oldSolution, Solution newSolution)
	{
		SolutionChanges changes = newSolution.GetChanges(oldSolution);
		Contract.ThrowIfTrue(changes.GetAddedProjects().Any());
		Contract.ThrowIfTrue(changes.GetRemovedProjects().Any());
		Contract.ThrowIfTrue(changes.GetProjectChanges().Any());
	}

	protected static ProjectInfo CreateProjectInfo(Project project)
	{
		return ProjectInfo.Create(project.Id, VersionStamp.Create(), project.Name, project.AssemblyName, project.Language, project.FilePath, project.OutputFilePath, project.PackageCachePaths, project.CompilationOptions, project.ParseOptions, project.Documents.Select((Document d) => CreateDocumentInfoWithText(d)), project.ProjectReferences, project.SymbolReferences, project.InternalsVisibleToModules, project.AnalyzerReferences, project.BackgroundCodeAnalysisScope, project.OutputAnalyzerStatistics, project.AdditionalDocuments.Select((TextDocument d) => CreateDocumentInfoWithText(d)), null, project.AssemblyProbingPaths, project.RuleSetPath, project.NamespaceTemplate, project.ExternalRulesetsEnabled, project.EnableCodeActions, project.IncrementalBuild, enableShowSymbolUsage: false, project.CaptureSymbolUsage, project.ProjectDefinition);
	}

	private static SourceText GetTextForced(TextDocument doc)
	{
		return doc.GetTextAsync(CancellationToken.None).WaitAndGetResult(CancellationToken.None);
	}

	private static DocumentInfo CreateDocumentInfoWithText(TextDocument doc)
	{
		return CreateDocumentInfoWithoutText(doc).WithTextLoader(TextLoader.From(TextAndVersion.Create(GetTextForced(doc), VersionStamp.Create(), doc.FilePath)));
	}

	private static DocumentInfo CreateDocumentInfoWithoutText(TextDocument doc)
	{
		return DocumentInfo.Create(doc.Id, doc.Name, doc.Folders, null, doc.FilePath);
	}

	protected virtual void ApplyProjectAdded(ProjectInfo project)
	{
		OnProjectAdded(project);
	}

	protected virtual void ApplyProjectRemoved(ProjectId projectId)
	{
		OnProjectRemoved(projectId);
	}

	protected virtual void ApplyPackageCachePathChanged(ProjectId projectId, IEnumerable<string> packageCachePath)
	{
		OnPackageCachePathsChanged(projectId, packageCachePath);
	}

	protected virtual void ApplyAssemblyProbingPathsChanged(ProjectId projectId, IReadOnlyList<string> probingPaths)
	{
		OnAssemblyProbingPathsChanged(projectId, probingPaths);
	}

	protected virtual void ApplyRuleSetPathChanged(ProjectId projectId, string ruleSetPath)
	{
		OnRuleSetPathChanged(projectId, ruleSetPath);
	}

	protected virtual void ApplyNamespaceTemplateChanged(ProjectId projectId, string? namespaceTemplate)
	{
		OnNamespaceTemplateChanged(projectId, namespaceTemplate);
	}

	protected virtual void ApplyExternalRulesetsEnabledChanged(ProjectId projectId, bool externalRulesetsEnabledChanged)
	{
		OnExternalRulesetsEnabledChanged(projectId, externalRulesetsEnabledChanged);
	}

	protected virtual void ApplyCompilationOptionsChanged(ProjectId projectId, CompilationOptions options)
	{
		OnCompilationOptionsChanged(projectId, options);
	}

	protected virtual void ApplyParseOptionsChanged(ProjectId projectId, ParseOptions options)
	{
		OnParseOptionsChanged(projectId, options);
	}

	protected virtual void ApplyEnableCodeActionsChanged(ProjectId projectId, bool enableCodeActions)
	{
		OnEnableCodeActionsChanged(projectId, enableCodeActions);
	}

	protected virtual void ApplyIncrementalBuildChanged(ProjectId projectId, bool incrementalBuild)
	{
		OnIncrementalBuildChanged(projectId, incrementalBuild);
	}

	protected virtual void ApplyCaptureSymbolUsageChanged(ProjectId projectId, bool captureSymbolUsage)
	{
		OnCaptureSymbolUsageChanged(projectId, captureSymbolUsage);
	}

	protected virtual void ApplyOutputAnalyzerStatistics(ProjectId projectId, bool outputAnalyzerStatistics)
	{
		OnOutputAnalyzerStatisticsChanged(projectId, outputAnalyzerStatistics);
	}

	protected virtual void ApplyProjectDefinitionChanged(ProjectId projectId, ProjectDefinition projectDefinition)
	{
		OnProjectDefinitionChanged(projectId, projectDefinition);
	}

	protected virtual void ApplyExpectedProjectReferencesChanged(ProjectId projectId, ISet<ProjectDefinition>? expectedProjectReferences)
	{
		OnExpectedProjectReferencesChanged(projectId, expectedProjectReferences);
	}

	protected virtual void ApplyProjectReferenceAdded(ProjectId projectId, ProjectReference projectReference)
	{
		OnProjectReferenceAdded(projectId, projectReference);
	}

	protected virtual void ApplyProjectReferenceRemoved(ProjectId projectId, ProjectReference projectReference)
	{
		OnProjectReferenceRemoved(projectId, projectReference);
	}

	protected virtual void ApplySymbolReferenceAdded(ProjectId projectId, SymbolReferenceSpecification symbolReference)
	{
		OnSymbolReferenceAdded(projectId, symbolReference);
	}

	protected virtual void ApplySymbolReferenceRemoved(ProjectId projectId, SymbolReferenceSpecification symbolReference)
	{
		OnSymbolReferenceRemoved(projectId, symbolReference);
	}

	protected virtual void ApplyAnalyzerReferenceAdded(ProjectId projectId, AnalyzerReference analyzerReference)
	{
		OnAnalyzerReferenceAdded(projectId, analyzerReference);
	}

	protected virtual void ApplyAnalyzerReferenceRemoved(ProjectId projectId, AnalyzerReference analyzerReference)
	{
		OnAnalyzerReferenceRemoved(projectId, analyzerReference);
	}

	protected virtual void ApplyBackgroundCodeAnalysisScope(ProjectId projectId, BackgroundCodeAnalysisScope enableCodeAnalysisInBackground)
	{
		OnEnableCodeAnalysisInBackgroundChanged(projectId, enableCodeAnalysisInBackground);
	}

	protected virtual void ApplyDocumentAdded(DocumentInfo info, SourceText text)
	{
		OnDocumentAdded(info.WithTextLoader(TextLoader.From(TextAndVersion.Create(text, VersionStamp.Create()))));
	}

	protected virtual void ApplyDocumentRemoved(DocumentId documentId)
	{
		OnDocumentRemoved(documentId);
	}

	protected virtual void ApplyDocumentTextChanged(DocumentId id, SourceText text)
	{
		OnDocumentTextChanged(id, text, PreservationMode.PreserveValue);
	}

	protected virtual void ApplyAdditionalDocumentAdded(DocumentInfo info, SourceText text)
	{
		OnAdditionalDocumentAdded(info.WithTextLoader(TextLoader.From(TextAndVersion.Create(text, VersionStamp.Create()))));
	}

	protected virtual void ApplyAdditionalDocumentRemoved(DocumentId documentId)
	{
		OnAdditionalDocumentRemoved(documentId);
	}

	protected virtual void ApplyAdditionalDocumentTextChanged(DocumentId id, SourceText text)
	{
		OnAdditionalDocumentTextChanged(id, text, PreservationMode.PreserveValue);
	}

	protected void CheckSolutionIsEmpty()
	{
		if (CurrentSolution.ProjectIds.Any())
		{
			throw new ArgumentException(WorkspacesResources.WorkspaceIsNotEmpty);
		}
	}

	protected void CheckProjectIsInCurrentSolution(ProjectId projectId)
	{
		if (!CurrentSolution.ContainsProject(projectId))
		{
			throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, WorkspacesResources.ProjectOrDocumentNotInWorkspace, GetProjectName(projectId)));
		}
	}

	protected void CheckProjectIsNotInCurrentSolution(ProjectId projectId)
	{
		if (CurrentSolution.ContainsProject(projectId))
		{
			throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, WorkspacesResources.ProjectOrDocumentAlreadyInWorkspace, GetProjectName(projectId)));
		}
	}

	protected void CheckProjectHasProjectReference(ProjectId fromProjectId, ProjectReference projectReference)
	{
		if (!CurrentSolution.GetProject(fromProjectId).ProjectReferences.Contains(projectReference))
		{
			throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, WorkspacesResources.ProjectNotReferenced, GetProjectName(projectReference.ProjectId)));
		}
	}

	protected void CheckProjectDoesNotHaveProjectReference(ProjectId fromProjectId, ProjectReference projectReference)
	{
		if (CurrentSolution.GetProject(fromProjectId).ProjectReferences.Contains(projectReference))
		{
			throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, WorkspacesResources.ProjectAlreadyReferenced, GetProjectName(projectReference.ProjectId)));
		}
	}

	protected void CheckProjectDoesNotHaveTransitiveProjectReference(ProjectId fromProjectId, ProjectId toProjectId)
	{
		if (GetTransitiveProjectReferences(toProjectId).Contains(fromProjectId))
		{
			throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, WorkspacesResources.CausesCircularProjectReference, GetProjectName(fromProjectId), GetProjectName(toProjectId)));
		}
	}

	private ISet<ProjectId> GetTransitiveProjectReferences(ProjectId project, ISet<ProjectId> projects = null)
	{
		ISet<ProjectId> projects2 = projects;
		projects2 = projects2 ?? new HashSet<ProjectId>();
		if (projects2.Add(project))
		{
			CurrentSolution.GetProject(project).ProjectReferences.Do(delegate(ProjectReference p)
			{
				GetTransitiveProjectReferences(p.ProjectId, projects2);
			});
		}
		return projects2;
	}

	protected void CheckProjectHasSymbolReference(ProjectId projectId, SymbolReferenceSpecification symbolReference)
	{
		if (!CurrentSolution.GetProject(projectId).SymbolReferences.Contains(symbolReference))
		{
			throw new ArgumentException(WorkspacesResources.SymbolIsNotReferenced);
		}
	}

	protected void CheckProjectDoesNotHaveSymbolReference(ProjectId projectId, SymbolReferenceSpecification symbolReference)
	{
		if (CurrentSolution.GetProject(projectId).SymbolReferences.Contains(symbolReference))
		{
			throw new ArgumentException(WorkspacesResources.SymbolIsAlreadyReferenced);
		}
	}

	protected void CheckProjectHasAnalyzerReference(ProjectId projectId, AnalyzerReference analyzerReference)
	{
		if (!CurrentSolution.GetProject(projectId).AnalyzerReferences.Contains(analyzerReference))
		{
			throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, WorkspacesResources.AnalyzerIsNotPresent, analyzerReference));
		}
	}

	protected void CheckProjectDoesNotHaveAnalyzerReference(ProjectId projectId, AnalyzerReference analyzerReference)
	{
		if (CurrentSolution.GetProject(projectId).AnalyzerReferences.Contains(analyzerReference))
		{
			throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, WorkspacesResources.AnalyzerIsAlreadyPresent, analyzerReference));
		}
	}

	protected void CheckDocumentIsInCurrentSolution(DocumentId documentId)
	{
		if (CurrentSolution.GetDocument(documentId) == null)
		{
			throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, WorkspacesResources.ProjectOrDocumentNotInWorkspace, GetDocumentName(documentId)));
		}
	}

	protected void CheckAdditionalDocumentIsInCurrentSolution(DocumentId documentId)
	{
		if (CurrentSolution.GetAdditionalDocument(documentId) == null)
		{
			throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, WorkspacesResources.ProjectOrDocumentNotInWorkspace, GetDocumentName(documentId)));
		}
	}

	protected void CheckDocumentIsNotInCurrentSolution(DocumentId documentId)
	{
		if (CurrentSolution.ContainsDocument(documentId))
		{
			throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, WorkspacesResources.ProjectOrDocumentAlreadyInWorkspace, GetDocumentName(documentId)));
		}
	}

	protected void CheckAdditionalDocumentIsNotInCurrentSolution(DocumentId documentId)
	{
		if (CurrentSolution.ContainsAdditionalDocument(documentId))
		{
			throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, WorkspacesResources.ProjectOrDocumentAlreadyInWorkspace, GetAdditionalDocumentName(documentId)));
		}
	}

	protected virtual string GetProjectName(ProjectId projectId)
	{
		Project project = CurrentSolution.GetProject(projectId);
		if (project == null)
		{
			return "<Project" + projectId.Id.ToString() + ">";
		}
		return project.Name;
	}

	protected virtual string GetDocumentName(DocumentId documentId)
	{
		Document document = CurrentSolution.GetDocument(documentId);
		if (document == null)
		{
			return "<Document" + documentId.Id.ToString() + ">";
		}
		return document.Name;
	}

	protected virtual string GetAdditionalDocumentName(DocumentId documentId)
	{
		TextDocument additionalDocument = CurrentSolution.GetAdditionalDocument(documentId);
		if (additionalDocument == null)
		{
			return "<Document" + documentId.Id.ToString() + ">";
		}
		return additionalDocument.Name;
	}

	private static void RemoveIfEmpty<TKey, TValue>(IDictionary<TKey, ISet<TValue>> dictionary, TKey key)
	{
		if (dictionary.TryGetValue(key, out ISet<TValue> value) && value.Count == 0)
		{
			dictionary.Remove(key);
		}
	}

	private void ClearOpenDocuments()
	{
		List<DocumentId> list;
		using (stateLock.DisposableWait())
		{
			list = projectToOpenDocumentsMap.Values.SelectMany((ISet<DocumentId> x) => x).ToList();
		}
		foreach (DocumentId item in list)
		{
			ClearOpenDocument(item, isSolutionClosing: true);
		}
	}

	private void ClearOpenDocuments(ProjectId projectId)
	{
		ISet<DocumentId> value;
		using (stateLock.DisposableWait())
		{
			projectToOpenDocumentsMap.TryGetValue(projectId, out value);
		}
		if (value == null)
		{
			return;
		}
		foreach (DocumentId item in value)
		{
			ClearOpenDocument(item);
		}
	}

	protected void ClearOpenDocument(DocumentId documentId, bool isSolutionClosing = false)
	{
		DocumentId documentId2;
		using (stateLock.DisposableWait())
		{
			documentId2 = ClearOpenDocument_NoLock(documentId);
		}
		if (!isSolutionClosing && CanChangeActiveContextDocument && documentId2 != null)
		{
			SetDocumentContext(documentId2);
		}
	}

	private DocumentId ClearOpenDocument_NoLock(DocumentId documentId)
	{
		stateLock.AssertHasLock();
		if (projectToOpenDocumentsMap.TryGetValue(documentId.ProjectId, out ISet<DocumentId> value))
		{
			value?.Remove(documentId);
		}
		RemoveIfEmpty(projectToOpenDocumentsMap, documentId.ProjectId);
		if (textTrackers.TryGetValue(documentId, out TextTracker value2))
		{
			value2.Disconnect();
			textTrackers.Remove(documentId);
			DocumentId documentId2 = UpdateCurrentContextMapping_NoLock(value2.TextContainer, documentId);
			if (documentId2 != null)
			{
				return documentId2;
			}
			UnregisterText(value2.TextContainer);
		}
		return null;
	}

	public virtual void OpenDocument(DocumentId documentId, bool activate = true)
	{
		CheckCanOpenDocuments();
	}

	public virtual void CloseDocument(DocumentId documentId)
	{
		CheckCanOpenDocuments();
	}

	public virtual void OpenAdditionalDocument(DocumentId documentId, bool activate = true)
	{
		CheckCanOpenDocuments();
	}

	public virtual void CloseAdditionalDocument(DocumentId documentId)
	{
		CheckCanOpenDocuments();
	}

	protected void CheckCanOpenDocuments()
	{
		if (!CanOpenDocuments)
		{
			throw new NotSupportedException(WorkspacesResources.OpenDocumentNotSupported);
		}
	}

	protected void CheckProjectDoesNotContainOpenDocuments(ProjectId projectId)
	{
		if (ProjectHasOpenDocuments(projectId))
		{
			throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, WorkspacesResources.ProjectContainsOpenDocuments, GetProjectName(projectId)));
		}
	}

	private bool ProjectHasOpenDocuments(ProjectId projectId)
	{
		using (stateLock.DisposableWait())
		{
			return projectToOpenDocumentsMap.ContainsKey(projectId);
		}
	}

	public virtual bool IsDocumentOpen(DocumentId documentId)
	{
		using (stateLock.DisposableWait())
		{
			return GetProjectOpenDocuments_NoLock(documentId.ProjectId)?.Contains(documentId) ?? false;
		}
	}

	public virtual IEnumerable<DocumentId> GetOpenDocumentIds(ProjectId projectId = null)
	{
		using (stateLock.DisposableWait())
		{
			if (projectToOpenDocumentsMap.Count == 0)
			{
				return SpecializedCollections.EmptyEnumerable<DocumentId>();
			}
			if (projectId != null)
			{
				if (projectToOpenDocumentsMap.TryGetValue(projectId, out ISet<DocumentId> value))
				{
					return value;
				}
				return SpecializedCollections.EmptyEnumerable<DocumentId>();
			}
			return projectToOpenDocumentsMap.SelectMany<KeyValuePair<ProjectId, ISet<DocumentId>>, DocumentId>((KeyValuePair<ProjectId, ISet<DocumentId>> kvp) => kvp.Value);
		}
	}

	public virtual IEnumerable<DocumentId> GetRelatedDocumentIds(SourceTextContainer container)
	{
		if (container == null)
		{
			throw new ArgumentNullException("container");
		}
		using (stateLock.DisposableWait())
		{
			return GetRelatedDocumentIds_NoLock(container);
		}
	}

	private ImmutableArray<DocumentId> GetRelatedDocumentIds_NoLock(SourceTextContainer container)
	{
		if (!bufferToDocumentInCurrentContextMap.TryGetValue(container, out DocumentId value))
		{
			return ImmutableArray<DocumentId>.Empty;
		}
		return CurrentSolution.GetRelatedDocumentIds(value);
	}

	public virtual DocumentId GetDocumentIdInCurrentContext(SourceTextContainer container)
	{
		if (container == null)
		{
			throw new ArgumentNullException("container");
		}
		using (stateLock.DisposableWait())
		{
			return GetDocumentIdInCurrentContext_NoLock(container);
		}
	}

	internal virtual DocumentId GetDocumentIdInCurrentContext(DocumentId documentId)
	{
		if (documentId == null)
		{
			throw new ArgumentNullException("documentId");
		}
		using (stateLock.DisposableWait())
		{
			SourceTextContainer openDocumentSourceTextContainer_NoLock = GetOpenDocumentSourceTextContainer_NoLock(documentId);
			return (openDocumentSourceTextContainer_NoLock != null) ? GetDocumentIdInCurrentContext_NoLock(openDocumentSourceTextContainer_NoLock) : documentId;
		}
	}

	private SourceTextContainer GetOpenDocumentSourceTextContainer_NoLock(DocumentId documentId)
	{
		ImmutableArray<DocumentId> documentIds = CurrentSolution.GetRelatedDocumentIds(documentId);
		return (from kvp in bufferToDocumentInCurrentContextMap
			where documentIds.Contains(kvp.Value)
			select kvp.Key).FirstOrDefault();
	}

	private DocumentId GetDocumentIdInCurrentContext_NoLock(SourceTextContainer container)
	{
		if (bufferToDocumentInCurrentContextMap.TryGetValue(container, out DocumentId value))
		{
			return value;
		}
		return null;
	}

	internal virtual void SetDocumentContext(DocumentId documentId)
	{
		throw new NotSupportedException();
	}

	protected void OnDocumentContextUpdated(DocumentId documentId)
	{
		SourceTextContainer openDocumentSourceTextContainer_NoLock;
		using (stateLock.DisposableWait())
		{
			openDocumentSourceTextContainer_NoLock = GetOpenDocumentSourceTextContainer_NoLock(documentId);
		}
		if (openDocumentSourceTextContainer_NoLock != null)
		{
			OnDocumentContextUpdated(documentId, openDocumentSourceTextContainer_NoLock);
		}
	}

	internal void OnDocumentContextUpdated(DocumentId documentId, SourceTextContainer container)
	{
		using (serializationLock.DisposableWait())
		{
			using (stateLock.DisposableWait())
			{
				bufferToDocumentInCurrentContextMap[container] = documentId;
			}
			RaiseDocumentActiveContextChangedEventAsync(CurrentSolution.GetDocument(documentId));
		}
	}

	protected void CheckDocumentIsClosed(DocumentId documentId)
	{
		if (IsDocumentOpen(documentId))
		{
			throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, WorkspacesResources.DocumentIsOpen, GetDocumentName(documentId)));
		}
	}

	protected void CheckDocumentIsOpen(DocumentId documentId)
	{
		if (!IsDocumentOpen(documentId))
		{
			throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, WorkspacesResources.DocumentIsNotOpen, GetDocumentName(documentId)));
		}
	}

	private ISet<DocumentId> GetProjectOpenDocuments_NoLock(ProjectId project)
	{
		stateLock.AssertHasLock();
		projectToOpenDocumentsMap.TryGetValue(project, out ISet<DocumentId> value);
		return value;
	}

	protected internal void OnDocumentOpened(DocumentId documentId, SourceTextContainer textContainer, bool isCurrentContext = true)
	{
		CheckDocumentIsInCurrentSolution(documentId);
		CheckDocumentIsClosed(documentId);
		using (serializationLock.DisposableWait())
		{
			Solution currentSolution = CurrentSolution;
			Document document = currentSolution.GetDocument(documentId);
			SourceText sourceText = document.GetTextAsync(CancellationToken.None).WaitAndGetResult(CancellationToken.None);
			AddToOpenDocumentMap(documentId);
			SourceText currentText = textContainer.CurrentText;
			Solution solution = currentSolution;
			if (sourceText == currentText || sourceText.ContentEquals(currentText))
			{
				VersionStamp version = document.GetTextVersionAsync(CancellationToken.None).WaitAndGetResult(CancellationToken.None);
				TextAndVersion textAndVersion = TextAndVersion.Create(currentText, version, document.FilePath);
				solution = currentSolution.WithDocumentText(documentId, textAndVersion, PreservationMode.PreserveIdentity);
			}
			else
			{
				solution = currentSolution.WithDocumentText(documentId, currentText, PreservationMode.PreserveIdentity);
			}
			Solution solution2 = SetCurrentSolution(solution);
			SignupForTextChanges(documentId, textContainer, isCurrentContext, delegate(Workspace w, DocumentId id, SourceText text, PreservationMode mode)
			{
				w.OnDocumentTextChanged(id, text, mode);
			});
			Document document2 = solution2.GetDocument(documentId);
			OnDocumentTextChanged(document2);
			RaiseWorkspaceChangedEventAsync(WorkspaceChangeKind.DocumentChanged, currentSolution, solution2, null, documentId);
			RaiseDocumentOpenedEventAsync(document2);
		}
		RegisterText(textContainer);
	}

	private void SignupForTextChanges(DocumentId documentId, SourceTextContainer textContainer, bool isCurrentContext, Action<Workspace, DocumentId, SourceText, PreservationMode> onChangedHandler)
	{
		TextTracker textTracker = new TextTracker(this, documentId, textContainer, onChangedHandler);
		textTrackers.Add(documentId, textTracker);
		UpdateCurrentContextMapping_NoLock(textContainer, documentId, isCurrentContext);
		textTracker.Connect();
	}

	private void AddToOpenDocumentMap(DocumentId documentId)
	{
		using (stateLock.DisposableWait())
		{
			ISet<DocumentId> projectOpenDocuments_NoLock = GetProjectOpenDocuments_NoLock(documentId.ProjectId);
			if (projectOpenDocuments_NoLock != null)
			{
				projectOpenDocuments_NoLock.Add(documentId);
				return;
			}
			projectToOpenDocumentsMap.Add(documentId.ProjectId, new HashSet<DocumentId> { documentId });
		}
	}

	protected internal void OnAdditionalDocumentOpened(DocumentId documentId, SourceTextContainer textContainer, bool isCurrentContext = true)
	{
		CheckAdditionalDocumentIsInCurrentSolution(documentId);
		CheckDocumentIsClosed(documentId);
		using (serializationLock.DisposableWait())
		{
			Solution currentSolution = CurrentSolution;
			TextDocument additionalDocument = currentSolution.GetAdditionalDocument(documentId);
			SourceText sourceText = additionalDocument.GetTextAsync(CancellationToken.None).WaitAndGetResult(CancellationToken.None);
			SourceText currentText = textContainer.CurrentText;
			Solution solution = currentSolution;
			if (sourceText == currentText || sourceText.ContentEquals(currentText))
			{
				VersionStamp version = additionalDocument.GetTextVersionAsync(CancellationToken.None).WaitAndGetResult(CancellationToken.None);
				TextAndVersion textAndVersion = TextAndVersion.Create(currentText, version, additionalDocument.FilePath);
				solution = currentSolution.WithAdditionalDocumentText(documentId, textAndVersion, PreservationMode.PreserveIdentity);
			}
			else
			{
				solution = currentSolution.WithAdditionalDocumentText(documentId, currentText, PreservationMode.PreserveIdentity);
			}
			Solution newSolution = SetCurrentSolution(solution);
			SignupForTextChanges(documentId, textContainer, isCurrentContext, delegate(Workspace w, DocumentId id, SourceText text, PreservationMode mode)
			{
				w.OnAdditionalDocumentTextChanged(id, text, mode);
			});
			RaiseWorkspaceChangedEventAsync(WorkspaceChangeKind.AdditionalDocumentChanged, currentSolution, newSolution, null, documentId);
		}
		RegisterText(textContainer);
	}

	protected internal void OnDocumentClosed(DocumentId documentId, TextLoader reloader, bool updateActiveContext = false)
	{
		CheckDocumentIsInCurrentSolution(documentId);
		CheckDocumentIsOpen(documentId);
		DocumentId documentId2;
		using (serializationLock.DisposableWait())
		{
			documentId2 = ForgetAnyOpenDocumentInfo(documentId);
			Solution currentSolution = CurrentSolution;
			OnDocumentClosing(documentId);
			Solution currentSolution2 = currentSolution.WithDocumentTextLoader(documentId, reloader, PreservationMode.PreserveValue);
			currentSolution2 = SetCurrentSolution(currentSolution2);
			Document document = currentSolution2.GetDocument(documentId);
			OnDocumentTextChanged(document);
			RaiseWorkspaceChangedEventAsync(WorkspaceChangeKind.DocumentChanged, currentSolution, currentSolution2, null, documentId);
			RaiseDocumentClosedEventAsync(document);
		}
		if (updateActiveContext && documentId2 != null && CanChangeActiveContextDocument)
		{
			SetDocumentContext(documentId2);
		}
	}

	private DocumentId ForgetAnyOpenDocumentInfo(DocumentId documentId)
	{
		using (stateLock.DisposableWait())
		{
			return ClearOpenDocument_NoLock(documentId);
		}
	}

	protected internal void OnAdditionalDocumentClosed(DocumentId documentId, TextLoader reloader)
	{
		CheckAdditionalDocumentIsInCurrentSolution(documentId);
		using (serializationLock.DisposableWait())
		{
			ForgetAnyOpenDocumentInfo(documentId);
			Solution currentSolution = CurrentSolution;
			Solution currentSolution2 = currentSolution.WithAdditionalDocumentTextLoader(documentId, reloader, PreservationMode.PreserveValue);
			currentSolution2 = SetCurrentSolution(currentSolution2);
			RaiseWorkspaceChangedEventAsync(WorkspaceChangeKind.AdditionalDocumentChanged, currentSolution, currentSolution2, null, documentId);
		}
	}

	private void UpdateCurrentContextMapping_NoLock(SourceTextContainer textContainer, DocumentId id, bool isCurrentContext)
	{
		if (isCurrentContext || !bufferToDocumentInCurrentContextMap.ContainsKey(textContainer))
		{
			bufferToDocumentInCurrentContextMap[textContainer] = id;
		}
	}

	private DocumentId UpdateCurrentContextMapping_NoLock(SourceTextContainer textContainer, DocumentId id)
	{
		ImmutableArray<DocumentId> relatedDocumentIds = CurrentSolution.GetRelatedDocumentIds(id);
		if (relatedDocumentIds.Length == 0)
		{
			bufferToDocumentInCurrentContextMap.Remove(textContainer);
			return null;
		}
		if (relatedDocumentIds.Length == 1 && relatedDocumentIds[0] == id)
		{
			bufferToDocumentInCurrentContextMap.Remove(textContainer);
			return null;
		}
		if (relatedDocumentIds[0] != id)
		{
			bufferToDocumentInCurrentContextMap[textContainer] = relatedDocumentIds[0];
			return relatedDocumentIds[0];
		}
		bufferToDocumentInCurrentContextMap[textContainer] = relatedDocumentIds[1];
		return relatedDocumentIds[1];
	}

	private SourceText GetOpenDocumentText(Solution solution, DocumentId documentId)
	{
		CheckDocumentIsOpen(documentId);
		solution.GetDocument(documentId).TryGetText(out SourceText text);
		return text;
	}

	protected virtual Solution AdjustReloadedSolution(Solution oldSolution, Solution reloadedSolution)
	{
		Solution solution = reloadedSolution;
		foreach (DocumentId openDocumentId in GetOpenDocumentIds())
		{
			if (solution.ContainsDocument(openDocumentId))
			{
				solution = solution.WithDocumentText(openDocumentId, GetOpenDocumentText(oldSolution, openDocumentId), PreservationMode.PreserveIdentity);
			}
		}
		return solution;
	}

	protected virtual Project AdjustReloadedProject(Project oldProject, Project reloadedProject)
	{
		Solution solution = oldProject.Solution;
		Solution solution2 = reloadedProject.Solution;
		foreach (DocumentId openDocumentId in GetOpenDocumentIds(oldProject.Id))
		{
			if (solution2.ContainsDocument(openDocumentId))
			{
				solution2 = solution2.WithDocumentText(openDocumentId, GetOpenDocumentText(solution, openDocumentId), PreservationMode.PreserveIdentity);
			}
		}
		return solution2.GetProject(oldProject.Id);
	}

	protected Task RaiseWorkspaceChangedEventAsync(WorkspaceChangeKind kind, Solution oldSolution, Solution newSolution, ProjectId projectId = null, DocumentId documentId = null, bool handleRad = false)
	{
		if (newSolution == null)
		{
			throw new ArgumentNullException("newSolution");
		}
		if (oldSolution == newSolution && kind != WorkspaceChangeKind.ActiveDocumentChanged)
		{
			return SpecializedTasks.EmptyTask;
		}
		if (projectId == null && documentId != null)
		{
			projectId = documentId.ProjectId;
		}
		return RaiseWorkspaceChangedEventInternalAsync(kind, oldSolution, newSolution, projectId, documentId, handleRad);
	}

	internal void ForceProjectClosureDiagnosticsEmit(ProjectId projectId)
	{
		RaiseWorkspaceChangedEventInternalAsync(WorkspaceChangeKind.ProjectChanged, CurrentSolution, CurrentSolution, projectId, null, handleRad: false);
	}

	private Task RaiseWorkspaceChangedEventInternalAsync(WorkspaceChangeKind kind, Solution oldSolution, Solution newSolution, ProjectId projectId, DocumentId documentId, bool handleRad)
	{
		Solution oldSolution2 = oldSolution;
		Solution newSolution2 = newSolution;
		ProjectId projectId2 = projectId;
		DocumentId documentId2 = documentId;
		EventMap.EventHandlerSet<EventHandler<WorkspaceChangeEventArgs>> ev = eventMap.GetEventHandlers<EventHandler<WorkspaceChangeEventArgs>>("WorkspaceChanged");
		if (ev.HasHandlers)
		{
			return ScheduleTask(delegate
			{
				WorkspaceChangeEventArgs args = new WorkspaceChangeEventArgs(kind, oldSolution2, newSolution2, projectId2, documentId2, handleRad);
				ev.RaiseEvent(delegate(EventHandler<WorkspaceChangeEventArgs> handler)
				{
					handler(this, args);
				});
			}, "Workspace.WorkspaceChanged");
		}
		return SpecializedTasks.EmptyTask;
	}

	protected internal virtual void OnWorkspaceFailed(WorkspaceDiagnostic diagnostic)
	{
		EventMap.EventHandlerSet<EventHandler<WorkspaceDiagnosticEventArgs>> eventHandlers = eventMap.GetEventHandlers<EventHandler<WorkspaceDiagnosticEventArgs>>("WorkspaceFailed");
		if (eventHandlers.HasHandlers)
		{
			WorkspaceDiagnosticEventArgs args = new WorkspaceDiagnosticEventArgs(diagnostic);
			eventHandlers.RaiseEvent(delegate(EventHandler<WorkspaceDiagnosticEventArgs> handler)
			{
				handler(this, args);
			});
		}
	}

	protected Task RaiseDocumentOpenedEventAsync(Document document)
	{
		Document document2 = document;
		EventMap.EventHandlerSet<EventHandler<DocumentEventArgs>> ev = eventMap.GetEventHandlers<EventHandler<DocumentEventArgs>>("DocumentOpened");
		if (ev.HasHandlers)
		{
			return ScheduleTask(delegate
			{
				DocumentEventArgs args = new DocumentEventArgs(document2);
				ev.RaiseEvent(delegate(EventHandler<DocumentEventArgs> handler)
				{
					handler(this, args);
				});
			}, "Workspace.WorkspaceChanged");
		}
		return SpecializedTasks.EmptyTask;
	}

	protected Task RaiseDocumentClosedEventAsync(Document document)
	{
		Document document2 = document;
		EventMap.EventHandlerSet<EventHandler<DocumentEventArgs>> ev = eventMap.GetEventHandlers<EventHandler<DocumentEventArgs>>("DocumentClosed");
		if (ev.HasHandlers)
		{
			return ScheduleTask(delegate
			{
				DocumentEventArgs args = new DocumentEventArgs(document2);
				ev.RaiseEvent(delegate(EventHandler<DocumentEventArgs> handler)
				{
					handler(this, args);
				});
			}, "Workspace.DocumentClosed");
		}
		return SpecializedTasks.EmptyTask;
	}

	protected Task RaiseDocumentActiveContextChangedEventAsync(Document document)
	{
		Document document2 = document;
		EventMap.EventHandlerSet<EventHandler<DocumentEventArgs>> ev = eventMap.GetEventHandlers<EventHandler<DocumentEventArgs>>("DocumentActiveContextChanged");
		if (ev.HasHandlers)
		{
			return ScheduleTask(delegate
			{
				DocumentEventArgs args = new DocumentEventArgs(document2);
				ev.RaiseEvent(delegate(EventHandler<DocumentEventArgs> handler)
				{
					handler(this, args);
				});
			}, "Workspace.WorkspaceChanged");
		}
		return SpecializedTasks.EmptyTask;
	}

	public static bool TryGetWorkspace(SourceTextContainer textContainer, out Workspace workspace)
	{
		if (textContainer == null)
		{
			throw new ArgumentNullException("textContainer");
		}
		WorkspaceRegistration workspaceRegistration = GetWorkspaceRegistration(textContainer);
		workspace = workspaceRegistration.Workspace;
		return workspace != null;
	}

	protected void RegisterText(SourceTextContainer textContainer)
	{
		if (textContainer == null)
		{
			throw new ArgumentNullException("textContainer");
		}
		GetWorkspaceRegistration(textContainer).SetWorkspaceAndRaiseEvents(this);
	}

	protected static void UnregisterText(SourceTextContainer textContainer)
	{
		if (textContainer == null)
		{
			throw new ArgumentNullException("textContainer");
		}
		GetWorkspaceRegistration(textContainer).SetWorkspaceAndRaiseEvents(null);
	}

	private static WorkspaceRegistration CreateRegistration(SourceTextContainer container)
	{
		return new WorkspaceRegistration();
	}

	public static WorkspaceRegistration GetWorkspaceRegistration(SourceTextContainer textContainer)
	{
		if (textContainer == null)
		{
			throw new ArgumentNullException("textContainer");
		}
		return bufferToWorkspaceRegistrationMap.GetValue(textContainer, createRegistration);
	}
}
