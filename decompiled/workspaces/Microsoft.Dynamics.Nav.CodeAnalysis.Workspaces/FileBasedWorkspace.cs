using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Dynamics.Nav.CodeAnalysis.Analyzers;
using Microsoft.Dynamics.Nav.CodeAnalysis.CommandLine;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Host;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces;

public abstract class FileBasedWorkspace : Workspace
{
	protected FileBasedWorkspace(AbstractHostServices host, string workspaceKind)
		: base(host, workspaceKind)
	{
	}

	protected (ProjectManifest Manifest, ProjectManifestProps? Props) LoadProjectManifest(string projectPath, List<Diagnostic> diagnostics)
	{
		ProjectManifestProps projectManifestProps = ProjectManifestProps.LoadFromFile(projectPath, diagnostics);
		return (Manifest: ProjectLoader.LoadFromFolder(projectPath, diagnostics, manifestIsMandatory: false, projectManifestProps), Props: projectManifestProps);
	}

	protected ProjectDefinition CreateProjectDefinitionFromManifest(ProjectManifest manifest)
	{
		return new ProjectDefinition
		{
			AppId = manifest.AppManifest.AppId,
			Name = manifest.AppManifest.AppName,
			Publisher = manifest.AppManifest.AppPublisher,
			Version = manifest.AppManifest.AppVersion,
			PropagateDependencies = manifest.AppManifest.PropagateDependencies,
			AlternateIds = manifest.AppManifest.AppAlternateIds
		};
	}

	protected CompilationOptions CreateCompilationOptionsFromManifest(ProjectManifest manifest)
	{
		return new CompilationOptions().WithManifestOptions(manifest.AppManifest).WithGenerationOptions(CompilationGenerationOptions.ReportLayout).WithMergedWarningSuppressions(manifest.AppManifest.SuppressWarnings);
	}

	protected ParseOptions CreateParseOptionsFromManifest(ProjectManifest manifest)
	{
		return ParseOptions.Default.WithManifestOptions(manifest.AppManifest, includeRuntimeVersion: true);
	}

	protected void AddDocumentsFromProjectPath(string projectPath, ProjectId projectId)
	{
		foreach (string item in Directory.EnumerateFiles(projectPath, "*", SearchOption.AllDirectories))
		{
			if (Path.GetExtension(item).Equals(".al", StringComparison.OrdinalIgnoreCase))
			{
				string fullPath = Path.GetFullPath(item);
				SourceText text = SourceText.From(File.ReadAllText(item));
				VersionStamp version = VersionStamp.Create();
				TextLoader loader = TextLoader.From(TextAndVersion.Create(text, version));
				DocumentInfo documentInfo = DocumentInfo.Create(DocumentId.CreateNewId(projectId), fullPath, null, loader, fullPath);
				OnDocumentAdded(documentInfo);
			}
		}
	}

	protected static ProjectInfo CreateProjectInfo(string? projectPath, string? assemblyName = null, string[]? packageCachePaths = null, CompilationOptions? options = null, ParseOptions? parseOptions = null, IEnumerable<ProjectReference>? projectReferences = null, IEnumerable<SymbolReferenceSpecification>? symbolReferences = null, IEnumerable<SymbolReferenceSpecification>? internalsVisibleToModules = null, IEnumerable<AnalyzerReference>? analyzerReferences = null, BackgroundCodeAnalysisScope backgroundCodeAnalysisScope = BackgroundCodeAnalysisScope.File, bool outputAnalyzerStatistics = false, string[]? probingFolders = null, string? ruleSetPath = null, bool externalRulesetsEnabled = true, bool enableCodeActions = true, bool incrementalBuild = false, bool enableShowSymbolUsage = false, bool enableCaptureSymbolUsage = false, ProjectDefinition? projectDefinition = null, ISet<ProjectDefinition>? expectedProjectReferences = null)
	{
		string text = projectDefinition?.Name ?? new DirectoryInfo(projectPath).Name;
		string text2 = null;
		try
		{
			text2 = Path.Combine(projectPath, "app.json").CreateUriFilePath();
		}
		catch (UriFormatException)
		{
			LocalMachineLogger.LogNormal(FormattableString.Invariant($"Project path invalid: {projectPath}."));
			throw;
		}
		ProjectId id = ProjectId.CreateNewId(text);
		VersionStamp version = VersionStamp.Create();
		string? assemblyName2 = assemblyName ?? text;
		string filePath = text2;
		bool externalRulesetsEnabled2 = externalRulesetsEnabled;
		IEnumerable<SymbolReferenceSpecification>? symbolReferences2 = symbolReferences ?? Enumerable.Empty<SymbolReferenceSpecification>();
		bool outputAnalyzerStatistics2 = outputAnalyzerStatistics;
		bool enableCodeActions2 = enableCodeActions;
		bool incrementalBuild2 = incrementalBuild;
		bool enableShowSymbolUsage2 = enableShowSymbolUsage;
		bool enableCaptureSymbolUsage2 = enableCaptureSymbolUsage;
		return ProjectInfo.Create(id, version, text, assemblyName2, "AL", filePath, null, packageCachePaths, options, parseOptions, null, projectReferences, symbolReferences2, internalsVisibleToModules, analyzerReferences, backgroundCodeAnalysisScope, outputAnalyzerStatistics2, null, null, probingFolders, ruleSetPath, null, externalRulesetsEnabled2, enableCodeActions2, incrementalBuild2, enableShowSymbolUsage2, enableCaptureSymbolUsage2, projectDefinition, expectedProjectReferences);
	}
}
