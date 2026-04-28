namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Host;

public abstract class AbstractHostLanguageServices
{
	public string Language => "AL";

	internal ISyntaxTreeFactoryService SyntaxTreeFactory => GetService<ISyntaxTreeFactoryService>();

	public abstract TLanguageService GetService<TLanguageService>();
}
