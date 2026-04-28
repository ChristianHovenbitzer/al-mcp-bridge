namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces;

public enum ApplyChangesKind
{
	AddProject,
	RemoveProject,
	AddProjectReference,
	RemoveProjectReference,
	AddSymbolReference,
	RemoveSymbolReference,
	AddDocument,
	RemoveDocument,
	ChangeDocument,
	AddAnalyzerReference,
	RemoveAnalyzerReference,
	AddAdditionalDocument,
	RemoveAdditionalDocument,
	ChangeAdditionalDocument,
	ChangeCompilationOptions,
	ChangeParseOptions,
	ChangePackageCachePath,
	ChangeAssemblyProbingPaths,
	ChangeRuleSetPath,
	ChangeEnableCodeActions,
	ChangeIncrementalBuild,
	ChangeProjectDefinition,
	ChangeIsBackgroundCodeAnalysisScope,
	ChangeIsCaptureSymbolUsageAnalysisEnabled,
	ChangeOutputAnalyzerStatistics,
	ChangeNamespaceTemplate
}
