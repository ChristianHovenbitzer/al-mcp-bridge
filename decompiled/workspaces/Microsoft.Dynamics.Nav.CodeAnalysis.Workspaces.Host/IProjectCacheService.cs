using System;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Host;

internal interface IProjectCacheService : IWorkspaceService
{
	IDisposable EnableCaching(ProjectId key);
}
