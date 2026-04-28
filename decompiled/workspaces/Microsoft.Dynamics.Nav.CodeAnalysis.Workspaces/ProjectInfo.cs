using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.Dynamics.Nav.CodeAnalysis.Analyzers;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces;

[DebuggerDisplay("{GetDebuggerDisplay(), nq}")]
public sealed class ProjectInfo
{
	public ProjectId Id { get; }

	public VersionStamp Version { get; }

	public string Name { get; }

	public string AssemblyName { get; }

	public string Language { get; }

	public string? FilePath { get; }

	public string? OutputFilePath { get; }

	public CompilationOptions? CompilationOptions { get; }

	public ParseOptions? ParseOptions { get; }

	public IReadOnlyList<DocumentInfo> Documents { get; }

	public IReadOnlyList<ProjectReference> ProjectReferences { get; }

	public IReadOnlyList<SymbolReferenceSpecification> SymbolReferences { get; }

	public IReadOnlyList<SymbolReferenceSpecification> InternalsVisibleToModules { get; }

	public IReadOnlyList<string> PackageCachePaths { get; }

	public IReadOnlyList<string> AssemblyProbingPaths { get; }

	public string? RuleSetPath { get; }

	public string? NamespaceTemplate { get; }

	public bool ExternalRulesetsEnabled { get; }

	public IReadOnlyList<AnalyzerReference> AnalyzerReferences { get; }

	public BackgroundCodeAnalysisScope BackgroundCodeAnalysisScope { get; }

	public bool OutputAnalyzerStatistics { get; }

	public ImmutableArray<DiagnosticAnalyzer> DiagnosticAnalyzers { get; }

	public IReadOnlyList<DocumentInfo> AdditionalDocuments { get; }

	public Type? HostObjectType { get; }

	public bool EnableCodeActions { get; }

	public bool IncrementalBuild { get; }

	public bool EnableShowSymbolUsage { get; }

	public bool EnableCaptureSymbolUsage { get; }

	public ProjectDefinition? ProjectDefinition { get; }

	public ISet<ProjectDefinition>? ExpectedProjectReferences { get; }

	private ProjectInfo(ProjectId id, VersionStamp version, string name, string assemblyName, string language, string? filePath, string? outputFilePath, IEnumerable<string>? packageCachePaths, CompilationOptions? compilationOptions, ParseOptions? parseOptions, IEnumerable<DocumentInfo>? documents, IEnumerable<ProjectReference>? projectReferences, IEnumerable<SymbolReferenceSpecification>? symbolReferences, IEnumerable<SymbolReferenceSpecification>? internalsVisibleToModules, IEnumerable<AnalyzerReference>? analyzerReferences, BackgroundCodeAnalysisScope backgroundCodeAnalysisScope, bool outputAnalyzerStatistics, IEnumerable<DocumentInfo>? additionalDocuments, Type? hostObjectType, IEnumerable<string>? assemblyProbingPaths, string? ruleSetPath, string? namespaceTemplate, bool externalRulesetsEnabled, bool enableCodeActions, bool incrementalBuild, bool enableShowSymbolUsage, bool enableCaptureSymbolUsage, ProjectDefinition? projectDefinition, ISet<ProjectDefinition>? expectedProjectReferences)
	{
		if (id == null)
		{
			throw new ArgumentNullException("id");
		}
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		if (assemblyName == null)
		{
			throw new ArgumentNullException("assemblyName");
		}
		if (language == null)
		{
			throw new ArgumentNullException("language");
		}
		Id = id;
		Version = version;
		Name = name;
		AssemblyName = assemblyName;
		Language = language;
		FilePath = filePath;
		OutputFilePath = outputFilePath;
		PackageCachePaths = CreatePackageCachePaths(packageCachePaths);
		CompilationOptions = compilationOptions;
		ParseOptions = parseOptions;
		AssemblyProbingPaths = assemblyProbingPaths?.ToImmutableReadOnlyListOrEmpty() ?? ImmutableList<string>.Empty;
		Documents = documents?.ToImmutableReadOnlyListOrEmpty() ?? ImmutableList<DocumentInfo>.Empty;
		ProjectReferences = projectReferences?.ToImmutableReadOnlyListOrEmpty() ?? ImmutableList<ProjectReference>.Empty;
		SymbolReferences = symbolReferences?.ToImmutableReadOnlyListOrEmpty() ?? ImmutableList<SymbolReferenceSpecification>.Empty;
		InternalsVisibleToModules = internalsVisibleToModules?.ToImmutableReadOnlyListOrEmpty() ?? ImmutableList<SymbolReferenceSpecification>.Empty;
		AnalyzerReferences = analyzerReferences?.ToImmutableReadOnlyListOrEmpty() ?? ImmutableList<AnalyzerReference>.Empty;
		BackgroundCodeAnalysisScope = backgroundCodeAnalysisScope;
		ExternalRulesetsEnabled = externalRulesetsEnabled;
		OutputAnalyzerStatistics = outputAnalyzerStatistics;
		DiagnosticAnalyzers = AnalyzerReferences.SelectMany((AnalyzerReference a) => a.GetAnalyzers()).ToImmutableArray();
		AdditionalDocuments = additionalDocuments?.ToImmutableReadOnlyListOrEmpty() ?? ImmutableList<DocumentInfo>.Empty;
		HostObjectType = hostObjectType;
		RuleSetPath = ruleSetPath;
		NamespaceTemplate = namespaceTemplate;
		EnableCodeActions = enableCodeActions;
		IncrementalBuild = incrementalBuild;
		EnableShowSymbolUsage = enableShowSymbolUsage;
		EnableCaptureSymbolUsage = enableCaptureSymbolUsage;
		ProjectDefinition = projectDefinition;
		ExpectedProjectReferences = expectedProjectReferences ?? SpecializedCollections.EmptySet<ProjectDefinition>();
	}

	public static ProjectInfo Create(ProjectId id, VersionStamp version, string name, string assemblyName, string language, string? filePath = null, string? outputFilePath = null, IEnumerable<string>? packageCachePaths = null, CompilationOptions? compilationOptions = null, ParseOptions? parseOptions = null, IEnumerable<DocumentInfo>? documents = null, IEnumerable<ProjectReference>? projectReferences = null, IEnumerable<SymbolReferenceSpecification>? symbolReferences = null, IEnumerable<SymbolReferenceSpecification>? internalsVisibleToModules = null, IEnumerable<AnalyzerReference>? analyzerReferences = null, BackgroundCodeAnalysisScope backgroundCodeAnalysisScope = BackgroundCodeAnalysisScope.File, bool outputAnalyzerStatistics = false, IEnumerable<DocumentInfo>? additionalDocuments = null, Type? hostObjectType = null, IEnumerable<string>? assemblyProbingPaths = null, string? ruleSetPath = null, string? namespaceTemplate = null, bool externalRulesetsEnabled = true, bool enableCodeActions = true, bool incrementalBuild = false, bool enableShowSymbolUsage = false, bool enableCaptureSymbolUsage = false, ProjectDefinition? projectDefinition = null, ISet<ProjectDefinition>? expectedProjectReferences = null)
	{
		return new ProjectInfo(id, version, name, assemblyName, language, filePath, outputFilePath, packageCachePaths, compilationOptions, parseOptions, documents, projectReferences, symbolReferences, internalsVisibleToModules, analyzerReferences, backgroundCodeAnalysisScope, outputAnalyzerStatistics, additionalDocuments, hostObjectType, assemblyProbingPaths, ruleSetPath, namespaceTemplate, externalRulesetsEnabled, enableCodeActions, incrementalBuild, enableShowSymbolUsage, enableCaptureSymbolUsage, projectDefinition, expectedProjectReferences);
	}

	private ProjectInfo With(ProjectId? id = null, VersionStamp? version = null, string? name = null, string? assemblyName = null, string? language = null, Optional<string> filePath = default(Optional<string>), Optional<string> outputPath = default(Optional<string>), IEnumerable<string>? packageCachePaths = null, CompilationOptions? compilationOptions = null, ParseOptions? parseOptions = null, IEnumerable<DocumentInfo>? documents = null, IEnumerable<ProjectReference>? projectReferences = null, IEnumerable<SymbolReferenceSpecification>? symbolReferences = null, IEnumerable<SymbolReferenceSpecification>? internalsVisibleToModules = null, IEnumerable<AnalyzerReference>? analyzerReferences = null, Optional<BackgroundCodeAnalysisScope> backgroundCodeAnalysisScope = default(Optional<BackgroundCodeAnalysisScope>), Optional<bool> outputAnalyzerStatistics = default(Optional<bool>), IEnumerable<DocumentInfo>? additionalDocuments = null, Optional<Type> hostObjectType = default(Optional<Type>), IEnumerable<string>? assemblyProbingPaths = null, Optional<string> ruleSetPath = default(Optional<string>), Optional<string> namespaceTemplate = default(Optional<string>), Optional<bool> externalRulesetsEnabled = default(Optional<bool>), Optional<bool> enableCodeActions = default(Optional<bool>), Optional<bool> incrementalBuild = default(Optional<bool>), Optional<bool> enableShowSymbolUsage = default(Optional<bool>), Optional<bool> enableCaptureSymbolUsage = default(Optional<bool>), ProjectDefinition? projectDefinition = null, ISet<ProjectDefinition>? expectedProjectReferences = null)
	{
		ProjectId projectId = id ?? Id;
		VersionStamp versionStamp = (version.HasValue ? version.Value : Version);
		string text = name ?? Name;
		string text2 = assemblyName ?? AssemblyName;
		string text3 = language ?? Language;
		string text4 = (filePath.HasValue ? filePath.Value : FilePath);
		string text5 = (outputPath.HasValue ? outputPath.Value : OutputFilePath);
		IEnumerable<string> enumerable = packageCachePaths ?? PackageCachePaths;
		CompilationOptions compilationOptions2 = compilationOptions ?? CompilationOptions;
		ParseOptions parseOptions2 = parseOptions ?? ParseOptions;
		IEnumerable<DocumentInfo> enumerable2 = documents ?? Documents;
		IEnumerable<ProjectReference> enumerable3 = projectReferences ?? ProjectReferences;
		IEnumerable<SymbolReferenceSpecification> enumerable4 = symbolReferences ?? SymbolReferences;
		IEnumerable<SymbolReferenceSpecification> enumerable5 = internalsVisibleToModules ?? InternalsVisibleToModules;
		IEnumerable<AnalyzerReference> enumerable6 = analyzerReferences ?? AnalyzerReferences;
		BackgroundCodeAnalysisScope backgroundCodeAnalysisScope2 = (backgroundCodeAnalysisScope.HasValue ? backgroundCodeAnalysisScope.Value : BackgroundCodeAnalysisScope);
		bool flag = (outputAnalyzerStatistics.HasValue ? outputAnalyzerStatistics.Value : OutputAnalyzerStatistics);
		IEnumerable<DocumentInfo> enumerable7 = additionalDocuments ?? AdditionalDocuments;
		Type type = (hostObjectType.HasValue ? hostObjectType.Value : HostObjectType);
		IEnumerable<string> enumerable8 = assemblyProbingPaths ?? AssemblyProbingPaths;
		string text6 = (ruleSetPath.HasValue ? ruleSetPath.Value : RuleSetPath);
		string text7 = (namespaceTemplate.HasValue ? namespaceTemplate.Value : NamespaceTemplate);
		bool flag2 = (externalRulesetsEnabled.HasValue ? externalRulesetsEnabled.Value : ExternalRulesetsEnabled);
		bool flag3 = (enableCodeActions.HasValue ? enableCodeActions.Value : EnableCodeActions);
		bool flag4 = (incrementalBuild.HasValue ? incrementalBuild.Value : IncrementalBuild);
		bool flag5 = (enableShowSymbolUsage.HasValue ? enableShowSymbolUsage.Value : EnableShowSymbolUsage);
		bool flag6 = (enableCaptureSymbolUsage.HasValue ? enableCaptureSymbolUsage.Value : EnableCaptureSymbolUsage);
		ProjectDefinition projectDefinition2 = projectDefinition ?? ProjectDefinition;
		ISet<ProjectDefinition> set = expectedProjectReferences ?? ExpectedProjectReferences;
		if (projectId == Id && versionStamp == Version && text == Name && text2 == AssemblyName && text3 == Language && text4 == FilePath && text5 == OutputFilePath && enumerable == PackageCachePaths && compilationOptions2 == CompilationOptions && parseOptions2 == ParseOptions && enumerable2 == Documents && enumerable3 == ProjectReferences && enumerable4 == SymbolReferences && enumerable5 == InternalsVisibleToModules && enumerable6 == AnalyzerReferences && backgroundCodeAnalysisScope2 == BackgroundCodeAnalysisScope && flag == OutputAnalyzerStatistics && enumerable7 == AdditionalDocuments && type == HostObjectType && enumerable8 == AssemblyProbingPaths && text6 == RuleSetPath && text7 == NamespaceTemplate && flag3 == EnableCodeActions && flag4 == IncrementalBuild && flag5 == EnableShowSymbolUsage && flag6 == EnableCaptureSymbolUsage && projectDefinition2 == ProjectDefinition && flag2 == ExternalRulesetsEnabled && set != null && set.SetEquals(ExpectedProjectReferences))
		{
			return this;
		}
		return new ProjectInfo(projectId, versionStamp, text, text2, text3, text4, text5, enumerable, compilationOptions2, parseOptions2, enumerable2, enumerable3, enumerable4, enumerable5, enumerable6, backgroundCodeAnalysisScope2, flag, enumerable7, type, enumerable8, text6, text7, flag2, flag3, flag4, flag5, flag6, projectDefinition2, set);
	}

	public ProjectInfo WithDocuments(IEnumerable<DocumentInfo> documents)
	{
		IEnumerable<DocumentInfo> documents2 = documents.ToImmutableReadOnlyListOrEmpty();
		return With(null, null, null, null, null, default(Optional<string>), default(Optional<string>), null, null, null, documents2);
	}

	public ProjectInfo WithAdditionalDocuments(IEnumerable<DocumentInfo> additionalDocuments)
	{
		IEnumerable<DocumentInfo> additionalDocuments2 = additionalDocuments.ToImmutableReadOnlyListOrEmpty();
		return With(null, null, null, null, null, default(Optional<string>), default(Optional<string>), null, null, null, null, null, null, null, null, default(Optional<BackgroundCodeAnalysisScope>), default(Optional<bool>), additionalDocuments2);
	}

	public ProjectInfo WithVersion(VersionStamp version)
	{
		return With(null, version);
	}

	public ProjectInfo WithName(string name)
	{
		return With(null, null, name);
	}

	public ProjectInfo WithFilePath(string filePath)
	{
		Optional<string> filePath2 = filePath;
		return With(null, null, null, null, null, filePath2);
	}

	public ProjectInfo WithAssemblyName(string assemblyName)
	{
		return With(null, null, null, assemblyName);
	}

	public ProjectInfo WithOutputFilePath(string outputFilePath)
	{
		Optional<string> outputPath = outputFilePath;
		return With(null, null, null, null, null, default(Optional<string>), outputPath);
	}

	public ProjectInfo WithPackageCachePaths(IEnumerable<string> packageCachePaths)
	{
		return With(null, null, null, null, null, default(Optional<string>), default(Optional<string>), packageCachePaths);
	}

	public ProjectInfo WithAssemblyProbingPaths(IReadOnlyList<string> assemblyProbingPaths)
	{
		return With(null, null, null, null, null, default(Optional<string>), default(Optional<string>), null, null, null, null, null, null, null, null, default(Optional<BackgroundCodeAnalysisScope>), default(Optional<bool>), null, default(Optional<Type>), assemblyProbingPaths);
	}

	public ProjectInfo WithRuleSetPath(string ruleSetPath)
	{
		Optional<string> ruleSetPath2 = ruleSetPath;
		return With(null, null, null, null, null, default(Optional<string>), default(Optional<string>), null, null, null, null, null, null, null, null, default(Optional<BackgroundCodeAnalysisScope>), default(Optional<bool>), null, default(Optional<Type>), null, ruleSetPath2);
	}

	public ProjectInfo WithNamespaceTemplate(string namespaceTemplate)
	{
		Optional<string> namespaceTemplate2 = namespaceTemplate;
		return With(null, null, null, null, null, default(Optional<string>), default(Optional<string>), null, null, null, null, null, null, null, null, default(Optional<BackgroundCodeAnalysisScope>), default(Optional<bool>), null, default(Optional<Type>), null, default(Optional<string>), namespaceTemplate2);
	}

	public ProjectInfo WithExternalRulesetsEnabled(bool externalRulesetsEnabled)
	{
		Optional<bool> externalRulesetsEnabled2 = externalRulesetsEnabled;
		return With(null, null, null, null, null, default(Optional<string>), default(Optional<string>), null, null, null, null, null, null, null, null, default(Optional<BackgroundCodeAnalysisScope>), default(Optional<bool>), null, default(Optional<Type>), null, default(Optional<string>), default(Optional<string>), externalRulesetsEnabled2);
	}

	public ProjectInfo WithCompilationOptions(CompilationOptions compilationOptions)
	{
		return With(null, null, null, null, null, default(Optional<string>), default(Optional<string>), null, compilationOptions);
	}

	public ProjectInfo WithParseOptions(ParseOptions parseOptions)
	{
		return With(null, null, null, null, null, default(Optional<string>), default(Optional<string>), null, null, parseOptions);
	}

	public ProjectInfo WithProjectReferences(IEnumerable<ProjectReference> projectReferences)
	{
		IEnumerable<ProjectReference> projectReferences2 = projectReferences.ToImmutableReadOnlyListOrEmpty();
		return With(null, null, null, null, null, default(Optional<string>), default(Optional<string>), null, null, null, null, projectReferences2);
	}

	public ProjectInfo WithSymbolReferences(IEnumerable<SymbolReferenceSpecification> symbolReferences)
	{
		IEnumerable<SymbolReferenceSpecification> symbolReferences2 = symbolReferences.ToImmutableReadOnlyListOrEmpty();
		return With(null, null, null, null, null, default(Optional<string>), default(Optional<string>), null, null, null, null, null, symbolReferences2);
	}

	public ProjectInfo WithInternalsVisibleToModules(IEnumerable<SymbolReferenceSpecification> internalsVisibleToModules)
	{
		IEnumerable<SymbolReferenceSpecification> internalsVisibleToModules2 = internalsVisibleToModules.ToImmutableReadOnlyListOrEmpty();
		return With(null, null, null, null, null, default(Optional<string>), default(Optional<string>), null, null, null, null, null, null, internalsVisibleToModules2);
	}

	public ProjectInfo WithAnalyzerReferences(IEnumerable<AnalyzerReference> analyzerReferences)
	{
		IEnumerable<AnalyzerReference> analyzerReferences2 = analyzerReferences.ToImmutableReadOnlyListOrEmpty();
		return With(null, null, null, null, null, default(Optional<string>), default(Optional<string>), null, null, null, null, null, null, null, analyzerReferences2);
	}

	public ProjectInfo WithBackgroundCodeAnalysisScope(BackgroundCodeAnalysisScope backgroundCodeAnalysisScope)
	{
		Optional<BackgroundCodeAnalysisScope> backgroundCodeAnalysisScope2 = backgroundCodeAnalysisScope;
		return With(null, null, null, null, null, default(Optional<string>), default(Optional<string>), null, null, null, null, null, null, null, null, backgroundCodeAnalysisScope2);
	}

	public ProjectInfo WithOutputAnalyzerStatistics(bool outputAnalyzerStatistics)
	{
		Optional<bool> outputAnalyzerStatistics2 = outputAnalyzerStatistics;
		return With(null, null, null, null, null, default(Optional<string>), default(Optional<string>), null, null, null, null, null, null, null, null, default(Optional<BackgroundCodeAnalysisScope>), outputAnalyzerStatistics2);
	}

	public ProjectInfo WithEnableCodeActions(bool enableCodeActions)
	{
		Optional<bool> enableCodeActions2 = enableCodeActions;
		return With(null, null, null, null, null, default(Optional<string>), default(Optional<string>), null, null, null, null, null, null, null, null, default(Optional<BackgroundCodeAnalysisScope>), default(Optional<bool>), null, default(Optional<Type>), null, default(Optional<string>), default(Optional<string>), default(Optional<bool>), enableCodeActions2);
	}

	public ProjectInfo WithIncrementalBuild(bool incrementalBuild)
	{
		Optional<bool> incrementalBuild2 = incrementalBuild;
		return With(null, null, null, null, null, default(Optional<string>), default(Optional<string>), null, null, null, null, null, null, null, null, default(Optional<BackgroundCodeAnalysisScope>), default(Optional<bool>), null, default(Optional<Type>), null, default(Optional<string>), default(Optional<string>), default(Optional<bool>), default(Optional<bool>), incrementalBuild2);
	}

	public ProjectInfo WithShowSymbolUsage(bool showSymbolUsage)
	{
		Optional<bool> enableShowSymbolUsage = showSymbolUsage;
		return With(null, null, null, null, null, default(Optional<string>), default(Optional<string>), null, null, null, null, null, null, null, null, default(Optional<BackgroundCodeAnalysisScope>), default(Optional<bool>), null, default(Optional<Type>), null, default(Optional<string>), default(Optional<string>), default(Optional<bool>), default(Optional<bool>), default(Optional<bool>), enableShowSymbolUsage);
	}

	public ProjectInfo WithCaptureSymbolUsage(bool captureSymbolUsage)
	{
		Optional<bool> enableCaptureSymbolUsage = captureSymbolUsage;
		return With(null, null, null, null, null, default(Optional<string>), default(Optional<string>), null, null, null, null, null, null, null, null, default(Optional<BackgroundCodeAnalysisScope>), default(Optional<bool>), null, default(Optional<Type>), null, default(Optional<string>), default(Optional<string>), default(Optional<bool>), default(Optional<bool>), default(Optional<bool>), default(Optional<bool>), enableCaptureSymbolUsage);
	}

	public ProjectInfo WithProjectDefinition(ProjectDefinition projectDefinition)
	{
		return With(null, null, null, null, null, default(Optional<string>), default(Optional<string>), null, null, null, null, null, null, null, null, default(Optional<BackgroundCodeAnalysisScope>), default(Optional<bool>), null, default(Optional<Type>), null, default(Optional<string>), default(Optional<string>), default(Optional<bool>), default(Optional<bool>), default(Optional<bool>), default(Optional<bool>), default(Optional<bool>), projectDefinition);
	}

	public ProjectInfo WithExpectedProjectReferences(ISet<ProjectDefinition>? expectedProjectReferences)
	{
		return With(null, null, null, null, null, default(Optional<string>), default(Optional<string>), null, null, null, null, null, null, null, null, default(Optional<BackgroundCodeAnalysisScope>), default(Optional<bool>), null, default(Optional<Type>), null, default(Optional<string>), default(Optional<string>), default(Optional<bool>), default(Optional<bool>), default(Optional<bool>), default(Optional<bool>), default(Optional<bool>), null, expectedProjectReferences);
	}

	internal string GetDebuggerDisplay()
	{
		return "ProjectInfo " + Name + ((!string.IsNullOrWhiteSpace(FilePath)) ? (" " + FilePath) : string.Empty);
	}

	private IReadOnlyList<string> CreatePackageCachePaths(IEnumerable<string>? paths)
	{
		if (paths == null)
		{
			return ImmutableArray<string>.Empty;
		}
		return paths.Select((string x) => CreatePackageCachePath(x)).ToImmutableReadOnlyListOrEmpty();
	}

	private string? CreatePackageCachePath(string? path)
	{
		if (string.IsNullOrEmpty(path))
		{
			return null;
		}
		if (PathUtilities.IsAbsolute(path))
		{
			return path;
		}
		return PathUtilities.ResolveRelativePath(path, Path.GetDirectoryName(FilePath));
	}
}
