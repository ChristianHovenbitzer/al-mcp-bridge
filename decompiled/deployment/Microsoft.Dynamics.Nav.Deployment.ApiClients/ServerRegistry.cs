using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;
using Microsoft.Dynamics.Nav.Deployment.Authentication;

namespace Microsoft.Dynamics.Nav.Deployment.ApiClients;

internal class ServerRegistry
{
	private readonly Dictionary<string, ServerInfo> cache = new Dictionary<string, ServerInfo>();

	private readonly SemaphoreSlim mutex = new SemaphoreSlim(1, 1);

	private readonly ExtensionInfo extensionInfo;

	private readonly Func<ConnectionOptions, IEmitLogger, IServerInfoApiClient> serverInfoClientFactory;

	private readonly Func<IEmitLogger, IFileCache> fileCacheFactory;

	private static readonly Func<IEmitLogger, IFileCache> DefaultFactory = UserProtectedFileStorage.CreateServerInfoCache;

	private bool checkedFileCache;

	public static ServerRegistry DevInstance { get; } = new ServerRegistry(DevExtensionInfo.Instance, (ConnectionOptions options, IEmitLogger logger) => new ServerInfoApiClient(options, logger), DefaultFactory);


	public static ServerRegistry SnapshotInstance { get; } = new ServerRegistry(SnapshotExtensionInfo.Instance, (ConnectionOptions options, IEmitLogger logger) => new SnapshotDebuggerClient(options, logger), DefaultFactory);


	public static ServerRegistry McpInstance { get; } = new ServerRegistry(McpClientExtensionInfo.Instance, (ConnectionOptions options, IEmitLogger logger) => new McpClientProvider(options, logger), DefaultFactory);


	public ServerRegistry(ExtensionInfo extensionInfo, Func<ConnectionOptions, IEmitLogger, IServerInfoApiClient> serverInfoClientFactory, Func<IEmitLogger, IFileCache> fileCacheFactory)
	{
		this.extensionInfo = extensionInfo;
		this.serverInfoClientFactory = serverInfoClientFactory;
		this.fileCacheFactory = fileCacheFactory;
	}

	public virtual async Task<ServerInfo?> GetServerInfo(ConnectionOptions options, IEmitLogger logger)
	{
		string key = options.GetCacheKey();
		if (!cache.TryGetValue(key, out ServerInfo value))
		{
			await mutex.WaitAsync();
			LoadFromFileCacheIfNeeded(logger);
			try
			{
				if (!cache.TryGetValue(key, out value))
				{
					value = await QueryMetadata(options, logger).ConfigureAwait(continueOnCapturedContext: false);
					if (value != null && extensionInfo.AssertSupports(value))
					{
						cache[key] = value;
						FlushFileCache(logger);
					}
				}
				else if (options != value.ConnectionOptions)
				{
					value.ConnectionOptions = options;
					FlushFileCache(logger);
				}
			}
			finally
			{
				mutex.Release();
			}
		}
		return value;
	}

	private void LoadFromFileCacheIfNeeded(IEmitLogger logger)
	{
		if (checkedFileCache)
		{
			return;
		}
		checkedFileCache = true;
		ServerInfo[] array = fileCacheFactory(logger).Read<ServerInfo[]>();
		if (array == null)
		{
			return;
		}
		ServerInfo[] array2 = array;
		foreach (ServerInfo serverInfo in array2)
		{
			try
			{
				if (extensionInfo.AssertSupports(serverInfo) && serverInfo.ConnectionOptions != null)
				{
					string cacheKey = serverInfo.ConnectionOptions.GetCacheKey();
					if (!cache.ContainsKey(cacheKey))
					{
						cache[cacheKey] = serverInfo;
					}
				}
			}
			catch (IncompatibleExtensionException)
			{
			}
		}
	}

	private void FlushFileCache(IEmitLogger logger)
	{
		IFileCache fileCache = fileCacheFactory(logger);
		ArrayBuilder<ServerInfo> instance = ArrayBuilder<ServerInfo>.GetInstance();
		try
		{
			instance.AddRange(cache.Values);
			ServerInfo[] array = fileCache.Read<ServerInfo[]>();
			if (array != null)
			{
				ServerInfo[] array2 = array;
				foreach (ServerInfo serverInfo in array2)
				{
					if (serverInfo.ConnectionOptions != null)
					{
						string cacheKey = serverInfo.ConnectionOptions.GetCacheKey();
						if (!cache.ContainsKey(cacheKey))
						{
							instance.Add(serverInfo);
						}
					}
				}
			}
			fileCache.Write(instance.ToArray());
		}
		finally
		{
			instance.Free();
		}
	}

	private async Task<ServerInfo?> QueryMetadata(ConnectionOptions options, IEmitLogger logger)
	{
		return await serverInfoClientFactory(options, logger).GetServerInfo().ConfigureAwait(continueOnCapturedContext: false);
	}
}
