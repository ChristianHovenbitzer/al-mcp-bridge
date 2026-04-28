namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Host;

public interface IPersistentStorageService : IWorkspaceService
{
	IPersistentStorage GetStorage(Solution solution);
}
