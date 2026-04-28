namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Host;

public abstract class AbstractHostServices
{
	protected internal abstract AbstractHostWorkspaceServices CreateWorkspaceServices(Workspace workspace);
}
