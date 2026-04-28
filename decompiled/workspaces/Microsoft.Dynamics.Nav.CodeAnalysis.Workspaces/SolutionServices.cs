using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Host;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces;

internal class SolutionServices
{
	internal readonly Workspace Workspace;

	internal readonly ITemporaryStorageService TemporaryStorage;

	internal readonly IProjectCacheHostService CacheService;

	internal bool SupportsCachingRecoverableObjects => CacheService != null;

	public SolutionServices(Workspace workspace)
	{
		Workspace = workspace;
		TemporaryStorage = workspace.Services.GetService<ITemporaryStorageService>();
		CacheService = workspace.Services.GetService<IProjectCacheHostService>();
	}
}
