using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.CodeAnalysis.Analyzers;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Host;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces;

internal class ProjectState : IDisposable
{
	private readonly SolutionServices solutionServices;

	private readonly AsyncLazy<VersionStamp> lazyLatestDocumentVersion;

	private readonly AsyncLazy<VersionStamp> lazyLatestDocumentTopLevelChangeVersion;

	private AnalyzerOptions analyzerOptions;

	private readonly Lazy<IObjectChangeManager> lazyObjectChangeManager;

	private bool disposedValue;

	public ProjectId Id => ProjectInfo.Id;

	public string FilePath => ProjectInfo.FilePath;

	public string OutputFilePath => ProjectInfo.OutputFilePath;

	public AbstractHostLanguageServices LanguageServices { get; }

	public string Name => ProjectInfo.Name;

	public Type HostObjectType => ProjectInfo.HostObjectType;

	[DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
	public VersionStamp Version => ProjectInfo.Version;

	public AnalyzerOptions AnalyzerOptions
	{
		get
		{
			if (analyzerOptions == null)
			{
				analyzerOptions = new AnalyzerOptions(((IEnumerable<AdditionalText>)AdditionalDocumentStates.Values.Select((TextDocumentState d) => new AdditionalTextDocument(d))).ToImmutableArray());
			}
			return analyzerOptions;
		}
	}

	internal IObjectChangeManager ObjectChangeManager => lazyObjectChangeManager.Value;

	[DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
	public ProjectInfo ProjectInfo { get; }

	[DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
	public string AssemblyName => ProjectInfo.AssemblyName;

	[DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
	public CompilationOptions CompilationOptions => ProjectInfo.CompilationOptions;

	[DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
	public ParseOptions ParseOptions => ProjectInfo.ParseOptions;

	[DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
	public IReadOnlyList<SymbolReferenceSpecification> SymbolReferences => ProjectInfo.SymbolReferences;

	[DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
	public IReadOnlyList<SymbolReferenceSpecification> InternalsVisibleToModules => ProjectInfo.InternalsVisibleToModules;

	[DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
	public IReadOnlyList<string> PackageCachePaths => ProjectInfo.PackageCachePaths;

	[DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
	public IReadOnlyList<string> AssemblyProbingPaths => ProjectInfo.AssemblyProbingPaths;

	[DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
	public string RuleSetPath => ProjectInfo.RuleSetPath;

	[DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
	public string? NamespaceTemplate => ProjectInfo.NamespaceTemplate;

	[DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
	public bool ExternalRulesetsEnabled => ProjectInfo.ExternalRulesetsEnabled;

	[DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
	public IReadOnlyList<AnalyzerReference> AnalyzerReferences => ProjectInfo.AnalyzerReferences;

	[DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
	public BackgroundCodeAnalysisScope BackgroundCodeAnalysisScope => ProjectInfo.BackgroundCodeAnalysisScope;

	[DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
	public bool OutputAnalyzerStatistics => ProjectInfo.OutputAnalyzerStatistics;

	[DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
	public bool EnableCodeActions => ProjectInfo.EnableCodeActions;

	[DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
	public bool IncrementalBuild => ProjectInfo.IncrementalBuild;

	[DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
	public bool EnableShowSymbolUsage => ProjectInfo.EnableShowSymbolUsage;

	[DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
	public bool EnableCaptureSymbolUsage => ProjectInfo.EnableCaptureSymbolUsage;

	[DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
	public ProjectDefinition ProjectDefinition => ProjectInfo.ProjectDefinition;

	[DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
	public ISet<ProjectDefinition>? ExpectedProjectReferences => ProjectInfo.ExpectedProjectReferences;

	[DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
	public ImmutableArray<DiagnosticAnalyzer> DiagnosticAnalyzers => ProjectInfo.DiagnosticAnalyzers;

	[DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
	public IReadOnlyList<ProjectReference> ProjectReferences => ProjectInfo.ProjectReferences;

	[DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
	public bool HasDocuments => DocumentIds.Count > 0;

	[DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
	public IEnumerable<DocumentState> OrderedDocumentStates => DocumentIds.Select(GetDocumentState);

	[DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
	public IReadOnlyList<DocumentId> DocumentIds { get; }

	[DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
	public IReadOnlyList<DocumentId> AdditionalDocumentIds { get; }

	[DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
	private ImmutableDictionary<DocumentId, DocumentState> DocumentStates { get; }

	[DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
	private ImmutableDictionary<DocumentId, TextDocumentState> AdditionalDocumentStates { get; }

	private ProjectState(ProjectInfo projectInfo, AbstractHostLanguageServices languageServices, SolutionServices solutionServices, IEnumerable<DocumentId> documentIds, IEnumerable<DocumentId> additionalDocumentIds, ImmutableDictionary<DocumentId, DocumentState> documentStates, ImmutableDictionary<DocumentId, TextDocumentState> additionalDocumentStates, AsyncLazy<VersionStamp> lazyLatestDocumentVersion, AsyncLazy<VersionStamp> lazyLatestDocumentTopLevelChangeVersion, Lazy<IObjectChangeManager> lazyRadManager)
	{
		ProjectInfo = projectInfo;
		this.solutionServices = solutionServices;
		LanguageServices = languageServices;
		DocumentIds = documentIds.ToImmutableReadOnlyListOrEmpty();
		AdditionalDocumentIds = additionalDocumentIds.ToImmutableReadOnlyListOrEmpty();
		DocumentStates = documentStates;
		AdditionalDocumentStates = additionalDocumentStates;
		this.lazyLatestDocumentVersion = lazyLatestDocumentVersion;
		this.lazyLatestDocumentTopLevelChangeVersion = lazyLatestDocumentTopLevelChangeVersion;
		lazyObjectChangeManager = lazyRadManager;
	}

	internal ProjectState(ProjectInfo projectInfo, AbstractHostLanguageServices languageServices, SolutionServices solutionServices)
	{
		AbstractHostLanguageServices languageServices2 = languageServices;
		SolutionServices solutionServices2 = solutionServices;
		base._002Ector();
		ProjectState projectState = this;
		Contract.ThrowIfNull(projectInfo);
		Contract.ThrowIfNull(languageServices2);
		Contract.ThrowIfNull(solutionServices2);
		LanguageServices = languageServices2;
		this.solutionServices = solutionServices2;
		ProjectInfo = FixProjectInfo(projectInfo);
		DocumentIds = ProjectInfo.Documents.Select((DocumentInfo d) => d.Id).ToImmutableArray();
		AdditionalDocumentIds = ProjectInfo.AdditionalDocuments.Select((DocumentInfo d) => d.Id).ToImmutableArray();
		ImmutableDictionary<DocumentId, DocumentState> docStates = ImmutableDictionary.CreateRange(ProjectInfo.Documents.Select((DocumentInfo d) => new KeyValuePair<DocumentId, DocumentState>(d.Id, CreateDocument(projectState.ProjectInfo, d, languageServices2, solutionServices2))));
		DocumentStates = docStates;
		ImmutableDictionary<DocumentId, TextDocumentState> additionalDocStates = ImmutableDictionary.CreateRange(ProjectInfo.AdditionalDocuments.Select((DocumentInfo d) => new KeyValuePair<DocumentId, TextDocumentState>(d.Id, TextDocumentState.Create(d, solutionServices2))));
		AdditionalDocumentStates = additionalDocStates;
		lazyLatestDocumentVersion = new AsyncLazy<VersionStamp>((CancellationToken c) => ComputeLatestDocumentVersionAsync(docStates, additionalDocStates, c), cacheResult: true);
		lazyLatestDocumentTopLevelChangeVersion = new AsyncLazy<VersionStamp>((CancellationToken c) => ComputeLatestDocumentTopLevelChangeVersionAsync(docStates, additionalDocStates, c), cacheResult: true);
		lazyObjectChangeManager = new Lazy<IObjectChangeManager>(delegate
		{
			ObjectChangeManager objectChangeManager = new ObjectChangeManager();
			string obj = (string.IsNullOrEmpty(projectState.FilePath) ? ".\\" : Path.GetDirectoryName(projectState.FilePath));
			Path.Combine(obj, ".vscode");
			string path = obj.RadPath();
			if (File.Exists(path))
			{
				objectChangeManager.Load(path, new FileSystem());
			}
			return objectChangeManager;
		});
	}

	private ProjectInfo FixProjectInfo(ProjectInfo projectInfo)
	{
		if (projectInfo.CompilationOptions == null)
		{
			ICompilationFactoryService service = LanguageServices.GetService<ICompilationFactoryService>();
			if (service != null)
			{
				projectInfo = projectInfo.WithCompilationOptions(service.GetDefaultCompilationOptions());
			}
		}
		return projectInfo;
	}

	private static async Task<VersionStamp> ComputeLatestDocumentVersionAsync(ImmutableDictionary<DocumentId, DocumentState> documentStates, ImmutableDictionary<DocumentId, TextDocumentState> additionalDocumentStates, CancellationToken cancellationToken)
	{
		VersionStamp latestVersion = VersionStamp.Default;
		foreach (DocumentState value in documentStates.Values)
		{
			cancellationToken.ThrowIfCancellationRequested();
			if (!value.IsGenerated)
			{
				latestVersion = (await value.GetTextVersionAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false)).GetNewerVersion(latestVersion);
			}
		}
		foreach (TextDocumentState value2 in additionalDocumentStates.Values)
		{
			cancellationToken.ThrowIfCancellationRequested();
			latestVersion = (await value2.GetTextVersionAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false)).GetNewerVersion(latestVersion);
		}
		return latestVersion;
	}

	private AsyncLazy<VersionStamp> CreateLazyLatestDocumentTopLevelChangeVersion(TextDocumentState newDocument, ImmutableDictionary<DocumentId, DocumentState> newDocumentStates, ImmutableDictionary<DocumentId, TextDocumentState> newAdditionalDocumentStates)
	{
		TextDocumentState newDocument2 = newDocument;
		ImmutableDictionary<DocumentId, DocumentState> newDocumentStates2 = newDocumentStates;
		ImmutableDictionary<DocumentId, TextDocumentState> newAdditionalDocumentStates2 = newAdditionalDocumentStates;
		if (lazyLatestDocumentTopLevelChangeVersion.TryGetValue(out var oldVersion))
		{
			return new AsyncLazy<VersionStamp>((CancellationToken c) => ComputeTopLevelChangeTextVersionAsync(oldVersion, newDocument2, c), cacheResult: true);
		}
		return new AsyncLazy<VersionStamp>((CancellationToken c) => ComputeLatestDocumentTopLevelChangeVersionAsync(newDocumentStates2, newAdditionalDocumentStates2, c), cacheResult: true);
	}

	private static async Task<VersionStamp> ComputeTopLevelChangeTextVersionAsync(VersionStamp oldVersion, TextDocumentState newDocument, CancellationToken cancellationToken)
	{
		return (await newDocument.GetTopLevelChangeTextVersionAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false)).GetNewerVersion(oldVersion);
	}

	private static async Task<VersionStamp> ComputeLatestDocumentTopLevelChangeVersionAsync(ImmutableDictionary<DocumentId, DocumentState> documentStates, ImmutableDictionary<DocumentId, TextDocumentState> additionalDocumentStates, CancellationToken cancellationToken)
	{
		VersionStamp latestVersion = VersionStamp.Default;
		foreach (DocumentState value in documentStates.Values)
		{
			cancellationToken.ThrowIfCancellationRequested();
			latestVersion = (await value.GetTopLevelChangeTextVersionAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false)).GetNewerVersion(latestVersion);
		}
		foreach (TextDocumentState value2 in additionalDocumentStates.Values)
		{
			cancellationToken.ThrowIfCancellationRequested();
			latestVersion = (await value2.GetTopLevelChangeTextVersionAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false)).GetNewerVersion(latestVersion);
		}
		return latestVersion;
	}

	private static DocumentState CreateDocument(ProjectInfo projectInfo, DocumentInfo documentInfo, AbstractHostLanguageServices languageServices, SolutionServices solutionServices)
	{
		return DocumentState.Create(documentInfo, projectInfo.ParseOptions, languageServices, solutionServices);
	}

	public Task<VersionStamp> GetLatestDocumentVersionAsync(CancellationToken cancellationToken)
	{
		return lazyLatestDocumentVersion.GetValueAsync(cancellationToken);
	}

	public Task<VersionStamp> GetLatestDocumentTopLevelChangeVersionAsync(CancellationToken cancellationToken)
	{
		return lazyLatestDocumentTopLevelChangeVersion.GetValueAsync(cancellationToken);
	}

	public async Task<VersionStamp> GetSemanticVersionAsync(CancellationToken cancellationToken = default(CancellationToken))
	{
		return (await GetLatestDocumentTopLevelChangeVersionAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false)).GetNewerVersion(Version);
	}

	public bool ContainsDocument(DocumentId documentId)
	{
		return DocumentStates.ContainsKey(documentId);
	}

	public bool ContainsAdditionalDocument(DocumentId documentId)
	{
		return AdditionalDocumentStates.ContainsKey(documentId);
	}

	public DocumentState GetDocumentState(DocumentId documentId)
	{
		DocumentStates.TryGetValue(documentId, out DocumentState value);
		return value;
	}

	public TextDocumentState GetAdditionalDocumentState(DocumentId documentId)
	{
		AdditionalDocumentStates.TryGetValue(documentId, out TextDocumentState value);
		return value;
	}

	private ProjectState With(ProjectInfo projectInfo = null, ImmutableArray<DocumentId> documentIds = default(ImmutableArray<DocumentId>), ImmutableArray<DocumentId> additionalDocumentIds = default(ImmutableArray<DocumentId>), ImmutableDictionary<DocumentId, DocumentState> documentStates = null, ImmutableDictionary<DocumentId, TextDocumentState> additionalDocumentStates = null, AsyncLazy<VersionStamp> latestDocumentVersion = null, AsyncLazy<VersionStamp> latestDocumentTopLevelChangeVersion = null, Lazy<IObjectChangeManager> lazyRadManager = null)
	{
		ProjectInfo projectInfo2 = projectInfo ?? ProjectInfo;
		AbstractHostLanguageServices languageServices = LanguageServices;
		SolutionServices obj = solutionServices;
		IReadOnlyList<DocumentId> documentIds2;
		if (!documentIds.IsDefault)
		{
			IReadOnlyList<DocumentId> readOnlyList = documentIds;
			documentIds2 = readOnlyList;
		}
		else
		{
			documentIds2 = DocumentIds;
		}
		IReadOnlyList<DocumentId> additionalDocumentIds2;
		if (!additionalDocumentIds.IsDefault)
		{
			IReadOnlyList<DocumentId> readOnlyList = additionalDocumentIds;
			additionalDocumentIds2 = readOnlyList;
		}
		else
		{
			additionalDocumentIds2 = AdditionalDocumentIds;
		}
		return new ProjectState(projectInfo2, languageServices, obj, documentIds2, additionalDocumentIds2, documentStates ?? DocumentStates, additionalDocumentStates ?? AdditionalDocumentStates, latestDocumentVersion ?? lazyLatestDocumentVersion, latestDocumentTopLevelChangeVersion ?? lazyLatestDocumentTopLevelChangeVersion, lazyRadManager ?? lazyObjectChangeManager);
	}

	public ProjectState UpdateName(string name)
	{
		if (name == Name)
		{
			return this;
		}
		return With(ProjectInfo.WithName(name).WithVersion(Version.GetNewerVersion()));
	}

	public ProjectState UpdateFilePath(string filePath)
	{
		if (filePath == FilePath)
		{
			return this;
		}
		return With(ProjectInfo.WithFilePath(filePath).WithVersion(Version.GetNewerVersion()));
	}

	public ProjectState UpdateAssemblyName(string assemblyName)
	{
		if (assemblyName == AssemblyName)
		{
			return this;
		}
		return With(ProjectInfo.WithAssemblyName(assemblyName).WithVersion(Version.GetNewerVersion()));
	}

	public ProjectState UpdatePackageCachePaths(IEnumerable<string> packageCachePaths)
	{
		if (packageCachePaths == PackageCachePaths)
		{
			return this;
		}
		return With(ProjectInfo.WithPackageCachePaths(packageCachePaths).WithVersion(Version.GetNewerVersion()));
	}

	public ProjectState UpdateAssemblyProbingPaths(IReadOnlyList<string> assemblyProbingPaths)
	{
		if (assemblyProbingPaths.SequenceEqual(AssemblyProbingPaths))
		{
			return this;
		}
		return With(ProjectInfo.WithAssemblyProbingPaths(assemblyProbingPaths).WithVersion(Version.GetNewerVersion()));
	}

	public ProjectState UpdateRuleSetPath(string ruleSetPath)
	{
		if (ruleSetPath == RuleSetPath)
		{
			return this;
		}
		return With(ProjectInfo.WithRuleSetPath(ruleSetPath).WithVersion(Version.GetNewerVersion()));
	}

	public ProjectState UpdateNamespaceTemplate(string namespaceTemplate)
	{
		if (namespaceTemplate == NamespaceTemplate)
		{
			return this;
		}
		return With(ProjectInfo.WithNamespaceTemplate(namespaceTemplate).WithVersion(Version.GetNewerVersion()));
	}

	public ProjectState UpdateExternalRulesetsEnabled(bool externalRulesetsEnabled)
	{
		if (externalRulesetsEnabled == ExternalRulesetsEnabled)
		{
			return this;
		}
		return With(ProjectInfo.WithExternalRulesetsEnabled(externalRulesetsEnabled).WithVersion(Version.GetNewerVersion()));
	}

	public ProjectState UpdateOutputPath(string outputFilePath)
	{
		if (outputFilePath == OutputFilePath)
		{
			return this;
		}
		return With(ProjectInfo.WithOutputFilePath(outputFilePath).WithVersion(Version.GetNewerVersion()));
	}

	public ProjectState UpdateCompilationOptions(CompilationOptions options)
	{
		if (options == CompilationOptions)
		{
			return this;
		}
		return With(ProjectInfo.WithCompilationOptions(options).WithVersion(Version.GetNewerVersion()));
	}

	public ProjectState UpdateParseOptions(ParseOptions options)
	{
		if (options == ParseOptions)
		{
			return this;
		}
		ImmutableDictionary<DocumentId, DocumentState> immutableDictionary = DocumentStates;
		foreach (KeyValuePair<DocumentId, DocumentState> documentState in DocumentStates)
		{
			DocumentState value = GetDocumentState(documentState.Key).ReParse(options);
			immutableDictionary = immutableDictionary.SetItem(documentState.Key, value);
		}
		ProjectInfo projectInfo = ProjectInfo.WithParseOptions(options).WithVersion(Version.GetNewerVersion());
		ImmutableDictionary<DocumentId, DocumentState> documentStates = immutableDictionary;
		return With(projectInfo, default(ImmutableArray<DocumentId>), default(ImmutableArray<DocumentId>), documentStates);
	}

	public static bool IsSameLanguage(ProjectState project1, ProjectState project2)
	{
		return project1.LanguageServices == project2.LanguageServices;
	}

	public ProjectState AddProjectReference(ProjectReference projectReference)
	{
		return With(ProjectInfo.WithProjectReferences(ProjectReferences.ToImmutableArray().Add(projectReference)).WithVersion(Version.GetNewerVersion()));
	}

	public ProjectState RemoveProjectReference(ProjectReference projectReference)
	{
		return With(ProjectInfo.WithProjectReferences(ProjectReferences.ToImmutableArray().Remove(projectReference)).WithVersion(Version.GetNewerVersion()));
	}

	public ProjectState RemoveAllProjectReferences()
	{
		return With(ProjectInfo.WithProjectReferences(ImmutableArray<ProjectReference>.Empty).WithVersion(Version.GetNewerVersion()));
	}

	public ProjectState RemoveProjectReferences(IEnumerable<ProjectReference> projectReferences)
	{
		IReadOnlyList<ProjectReference> readOnlyList = ProjectReferences;
		foreach (ProjectReference projectReference in projectReferences)
		{
			readOnlyList = readOnlyList.ToImmutableArray().Remove(projectReference);
		}
		return With(ProjectInfo.WithProjectReferences(readOnlyList).WithVersion(Version.GetNewerVersion()));
	}

	public ProjectState AddProjectReferences(IEnumerable<ProjectReference> projectReferences)
	{
		IReadOnlyList<ProjectReference> readOnlyList = ProjectReferences;
		foreach (ProjectReference projectReference in projectReferences)
		{
			readOnlyList = readOnlyList.ToImmutableArray().Add(projectReference);
		}
		return With(ProjectInfo.WithProjectReferences(readOnlyList).WithVersion(Version.GetNewerVersion()));
	}

	public ProjectState WithProjectReferences(IEnumerable<ProjectReference> projectReferences)
	{
		return With(ProjectInfo.WithProjectReferences(projectReferences).WithVersion(Version.GetNewerVersion()));
	}

	public ProjectState AddSymbolReference(SymbolReferenceSpecification toMetadata)
	{
		return With(ProjectInfo.WithSymbolReferences(SymbolReferences.ToImmutableArray().Add(toMetadata)).WithVersion(Version.GetNewerVersion()));
	}

	public ProjectState RemoveSymbolReference(SymbolReferenceSpecification toMetadata)
	{
		return With(ProjectInfo.WithSymbolReferences(SymbolReferences.ToImmutableArray().Remove(toMetadata)).WithVersion(Version.GetNewerVersion()));
	}

	public ProjectState AddSymbolReferences(IEnumerable<SymbolReferenceSpecification> symbolReferences)
	{
		IReadOnlyList<SymbolReferenceSpecification> readOnlyList = SymbolReferences;
		foreach (SymbolReferenceSpecification symbolReference in symbolReferences)
		{
			readOnlyList = readOnlyList.ToImmutableArray().Add(symbolReference);
		}
		return With(ProjectInfo.WithSymbolReferences(readOnlyList).WithVersion(Version.GetNewerVersion()));
	}

	public ProjectState RemoveSymbolReferences(IEnumerable<SymbolReferenceSpecification> symbolReferences)
	{
		IReadOnlyList<SymbolReferenceSpecification> readOnlyList = SymbolReferences;
		foreach (SymbolReferenceSpecification symbolReference in symbolReferences)
		{
			readOnlyList = readOnlyList.ToImmutableArray().Remove(symbolReference);
		}
		return With(ProjectInfo.WithSymbolReferences(readOnlyList).WithVersion(Version.GetNewerVersion()));
	}

	public ProjectState WithSymbolReferences(IEnumerable<SymbolReferenceSpecification> symbolReferences)
	{
		return With(ProjectInfo.WithSymbolReferences(symbolReferences).WithVersion(Version.GetNewerVersion()));
	}

	public ProjectState WithInternalsVisibleToModules(IEnumerable<SymbolReferenceSpecification> internalsVisibleToModules)
	{
		return With(ProjectInfo.WithInternalsVisibleToModules(internalsVisibleToModules).WithVersion(Version.GetNewerVersion()));
	}

	public ProjectState AddAnalyzerReference(AnalyzerReference analyzerReference)
	{
		return With(ProjectInfo.WithAnalyzerReferences(AnalyzerReferences.ToImmutableArray().Add(analyzerReference)).WithVersion(Version.GetNewerVersion()));
	}

	public ProjectState RemoveAnalyzerReference(AnalyzerReference analyzerReference)
	{
		return With(ProjectInfo.WithAnalyzerReferences(AnalyzerReferences.ToImmutableArray().Remove(analyzerReference)).WithVersion(Version.GetNewerVersion()));
	}

	public ProjectState AddAnalyzerReferences(IEnumerable<AnalyzerReference> analyzerReferences)
	{
		IReadOnlyList<AnalyzerReference> readOnlyList = AnalyzerReferences;
		foreach (AnalyzerReference analyzerReference in analyzerReferences)
		{
			readOnlyList = readOnlyList.ToImmutableArray().Add(analyzerReference);
		}
		return With(ProjectInfo.WithAnalyzerReferences(readOnlyList).WithVersion(Version.GetNewerVersion()));
	}

	public ProjectState WithAnalyzerReferences(IEnumerable<AnalyzerReference> analyzerReferences)
	{
		return With(ProjectInfo.WithAnalyzerReferences(analyzerReferences).WithVersion(Version.GetNewerVersion()));
	}

	public ProjectState WithBackgroundCodeAnalysisScope(BackgroundCodeAnalysisScope backgroundCodeAnalysisScope)
	{
		return With(ProjectInfo.WithBackgroundCodeAnalysisScope(backgroundCodeAnalysisScope).WithVersion(Version.GetNewerVersion()));
	}

	public ProjectState WithOutputAnalyzerStatistics(bool outputAnalyzerStatistics)
	{
		return With(ProjectInfo.WithOutputAnalyzerStatistics(outputAnalyzerStatistics).WithVersion(Version.GetNewerVersion()));
	}

	public ProjectState WithEnableCodeActions(bool enableCodeActions)
	{
		return With(ProjectInfo.WithEnableCodeActions(enableCodeActions).WithVersion(Version.GetNewerVersion()));
	}

	public ProjectState WithIncrementalBuild(bool incrementalBuild)
	{
		return With(ProjectInfo.WithIncrementalBuild(incrementalBuild).WithVersion(Version.GetNewerVersion()));
	}

	public ProjectState WithShowSymbolUsage(bool showSymbolUsage)
	{
		return With(ProjectInfo.WithShowSymbolUsage(showSymbolUsage).WithVersion(Version.GetNewerVersion()));
	}

	public ProjectState WithCaptureSymbolUsage(bool captureSymbolUsage)
	{
		return With(ProjectInfo.WithCaptureSymbolUsage(captureSymbolUsage).WithVersion(Version.GetNewerVersion()));
	}

	public ProjectState WithProjectDefinition(ProjectDefinition projectDefinition)
	{
		return With(ProjectInfo.WithProjectDefinition(projectDefinition).WithVersion(Version.GetNewerVersion()));
	}

	public ProjectState WithExpectedProjectReferences(ISet<ProjectDefinition>? expectedProjectReferences)
	{
		return With(ProjectInfo.WithExpectedProjectReferences(expectedProjectReferences).WithVersion(Version.GetNewerVersion()));
	}

	public ProjectState AddDocument(DocumentState document)
	{
		ProjectInfo projectInfo = ProjectInfo.WithVersion(Version.GetNewerVersion()).WithDocuments(ProjectInfo.Documents.Concat(document.Info));
		ImmutableArray<DocumentId> documentIds = DocumentIds.ToImmutableArray().Add(document.Id);
		ImmutableDictionary<DocumentId, DocumentState> documentStates = DocumentStates.Add(document.Id, document);
		return With(projectInfo, documentIds, default(ImmutableArray<DocumentId>), documentStates);
	}

	public ProjectState AddAdditionalDocument(TextDocumentState document)
	{
		ProjectInfo projectInfo = ProjectInfo.WithVersion(Version.GetNewerVersion()).WithAdditionalDocuments(ProjectInfo.AdditionalDocuments.Concat(document.Info));
		ImmutableArray<DocumentId> additionalDocumentIds = AdditionalDocumentIds.ToImmutableArray().Add(document.Id);
		ImmutableDictionary<DocumentId, TextDocumentState> additionalDocumentStates = AdditionalDocumentStates.Add(document.Id, document);
		return With(projectInfo, default(ImmutableArray<DocumentId>), additionalDocumentIds, null, additionalDocumentStates);
	}

	public ProjectState RemoveDocument(DocumentId documentId)
	{
		DocumentId documentId2 = documentId;
		ProjectInfo projectInfo = ProjectInfo.WithVersion(Version.GetNewerVersion()).WithDocuments(ProjectInfo.Documents.Where((DocumentInfo info) => info.Id != documentId2));
		ImmutableArray<DocumentId> documentIds = DocumentIds.ToImmutableArray().Remove(documentId2);
		ImmutableDictionary<DocumentId, DocumentState> documentStates = DocumentStates.Remove(documentId2);
		return With(projectInfo, documentIds, default(ImmutableArray<DocumentId>), documentStates);
	}

	public ProjectState RemoveAdditionalDocument(DocumentId documentId)
	{
		DocumentId documentId2 = documentId;
		ProjectInfo projectInfo = ProjectInfo.WithVersion(Version.GetNewerVersion()).WithDocuments(ProjectInfo.AdditionalDocuments.Where((DocumentInfo info) => info.Id != documentId2));
		ImmutableArray<DocumentId> additionalDocumentIds = AdditionalDocumentIds.ToImmutableArray().Remove(documentId2);
		ImmutableDictionary<DocumentId, TextDocumentState> additionalDocumentStates = AdditionalDocumentStates.Remove(documentId2);
		return With(projectInfo, default(ImmutableArray<DocumentId>), additionalDocumentIds, null, additionalDocumentStates);
	}

	public ProjectState RemoveAllDocuments()
	{
		ProjectInfo projectInfo = ProjectInfo.WithVersion(Version.GetNewerVersion()).WithDocuments(SpecializedCollections.EmptyEnumerable<DocumentInfo>());
		ImmutableArray<DocumentId> documentIds = ImmutableArray.Create<DocumentId>();
		ImmutableDictionary<DocumentId, DocumentState> empty = ImmutableDictionary<DocumentId, DocumentState>.Empty;
		return With(projectInfo, documentIds, default(ImmutableArray<DocumentId>), empty);
	}

	public ProjectState UpdateDocument(DocumentState newDocument, bool textChanged, bool recalculateDependentVersions)
	{
		DocumentState documentState = GetDocumentState(newDocument.Id);
		if (documentState == newDocument)
		{
			return this;
		}
		ImmutableDictionary<DocumentId, DocumentState> immutableDictionary = DocumentStates.SetItem(newDocument.Id, newDocument);
		GetLatestDependentVersions(immutableDictionary, AdditionalDocumentStates, documentState, newDocument, recalculateDependentVersions, textChanged, out AsyncLazy<VersionStamp> dependentDocumentVersion, out AsyncLazy<VersionStamp> dependentSemanticVersion);
		ImmutableDictionary<DocumentId, DocumentState> documentStates = immutableDictionary;
		AsyncLazy<VersionStamp> latestDocumentVersion = dependentDocumentVersion;
		AsyncLazy<VersionStamp> latestDocumentTopLevelChangeVersion = dependentSemanticVersion;
		return With(null, default(ImmutableArray<DocumentId>), default(ImmutableArray<DocumentId>), documentStates, null, latestDocumentVersion, latestDocumentTopLevelChangeVersion);
	}

	public ProjectState UpdateAdditionalDocument(TextDocumentState newDocument, bool textChanged, bool recalculateDependentVersions)
	{
		TextDocumentState additionalDocumentState = GetAdditionalDocumentState(newDocument.Id);
		if (additionalDocumentState == newDocument)
		{
			return this;
		}
		ImmutableDictionary<DocumentId, TextDocumentState> immutableDictionary = AdditionalDocumentStates.SetItem(newDocument.Id, newDocument);
		GetLatestDependentVersions(DocumentStates, immutableDictionary, additionalDocumentState, newDocument, recalculateDependentVersions, textChanged, out AsyncLazy<VersionStamp> dependentDocumentVersion, out AsyncLazy<VersionStamp> dependentSemanticVersion);
		ImmutableDictionary<DocumentId, TextDocumentState> additionalDocumentStates = immutableDictionary;
		AsyncLazy<VersionStamp> latestDocumentVersion = dependentDocumentVersion;
		AsyncLazy<VersionStamp> latestDocumentTopLevelChangeVersion = dependentSemanticVersion;
		return With(null, default(ImmutableArray<DocumentId>), default(ImmutableArray<DocumentId>), null, additionalDocumentStates, latestDocumentVersion, latestDocumentTopLevelChangeVersion);
	}

	private void GetLatestDependentVersions(ImmutableDictionary<DocumentId, DocumentState> newDocumentStates, ImmutableDictionary<DocumentId, TextDocumentState> newAdditionalDocumentStates, TextDocumentState oldDocument, TextDocumentState newDocument, bool recalculateDependentVersions, bool textChanged, out AsyncLazy<VersionStamp> dependentDocumentVersion, out AsyncLazy<VersionStamp> dependentSemanticVersion)
	{
		ImmutableDictionary<DocumentId, DocumentState> newDocumentStates2 = newDocumentStates;
		ImmutableDictionary<DocumentId, TextDocumentState> newAdditionalDocumentStates2 = newAdditionalDocumentStates;
		bool flag = false;
		bool flag2 = false;
		if (recalculateDependentVersions && oldDocument.TryGetTextVersion(out var version))
		{
			if (!lazyLatestDocumentVersion.TryGetValue(out var value) || value == version)
			{
				flag = true;
			}
			if (!lazyLatestDocumentTopLevelChangeVersion.TryGetValue(out var value2) || value2 == version)
			{
				flag2 = true;
			}
		}
		dependentDocumentVersion = (flag ? new AsyncLazy<VersionStamp>((CancellationToken c) => ComputeLatestDocumentVersionAsync(newDocumentStates2, newAdditionalDocumentStates2, c), cacheResult: true) : (textChanged ? new AsyncLazy<VersionStamp>(newDocument.GetTextVersionAsync, cacheResult: true) : lazyLatestDocumentVersion));
		dependentSemanticVersion = (flag2 ? new AsyncLazy<VersionStamp>((CancellationToken c) => ComputeLatestDocumentTopLevelChangeVersionAsync(newDocumentStates2, newAdditionalDocumentStates2, c), cacheResult: true) : (textChanged ? CreateLazyLatestDocumentTopLevelChangeVersion(newDocument, newDocumentStates2, newAdditionalDocumentStates2) : lazyLatestDocumentTopLevelChangeVersion));
	}

	protected virtual void Dispose(bool disposing)
	{
		if (disposedValue)
		{
			return;
		}
		if (disposing)
		{
			if (lazyLatestDocumentTopLevelChangeVersion != null)
			{
				lazyLatestDocumentTopLevelChangeVersion.Dispose();
			}
			if (lazyLatestDocumentVersion != null)
			{
				lazyLatestDocumentVersion.Dispose();
			}
		}
		disposedValue = true;
	}

	public void Dispose()
	{
		Dispose(disposing: true);
	}
}
