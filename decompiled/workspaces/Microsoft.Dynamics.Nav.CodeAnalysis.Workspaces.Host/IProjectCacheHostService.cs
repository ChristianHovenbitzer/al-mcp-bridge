namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Host;

internal interface IProjectCacheHostService : IProjectCacheService, IWorkspaceService
{
	T CacheObjectIfCachingEnabledForKey<T>(ProjectId key, object owner, T instance) where T : class;

	T CacheObjectIfCachingEnabledForKey<T>(ProjectId key, ICachedObjectOwner owner, T instance) where T : class;
}
