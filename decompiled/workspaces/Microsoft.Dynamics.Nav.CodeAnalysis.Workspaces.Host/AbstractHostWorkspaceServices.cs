namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Host;

public abstract class AbstractHostWorkspaceServices
{
	public abstract AbstractHostServices HostServices { get; }

	public abstract Workspace Workspace { get; }

	public abstract TWorkspaceService GetService<TWorkspaceService>();

	public abstract TWorkspaceService GetRequiredService<TWorkspaceService>();

	public abstract AbstractHostLanguageServices GetLanguageServices(string languageName);
}
