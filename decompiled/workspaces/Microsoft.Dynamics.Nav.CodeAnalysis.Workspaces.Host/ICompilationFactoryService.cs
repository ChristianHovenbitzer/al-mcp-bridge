namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Host;

internal interface ICompilationFactoryService : ILanguageService
{
	Compilation CreateCompilation(string assemblyName, CompilationOptions options, ProjectDefinition definition);

	CompilationOptions GetDefaultCompilationOptions();
}
