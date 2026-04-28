using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.CodeAnalysis;
using Microsoft.Dynamics.Nav.Deployment.Http;

namespace Microsoft.Dynamics.Nav.Deployment.ApiClients;

internal abstract class ApiClient
{
	public ConnectionOptions ConnectionOptions { get; }

	public IEmitLogger Logger { get; }

	public ApiClient(ConnectionOptions options, IEmitLogger logger)
	{
		ConnectionOptions = options;
		Logger = logger;
	}

	public virtual IHttpClientFactory GetHttpClientFactory()
	{
		if (ConnectionOptions.IsOnPremise() && !ConnectionOptions.IsOnPremiseWithAAD())
		{
			return OnPremiseHttpClientFactory.Instance.Value;
		}
		return CloudHttpClientFactory.Instance.Value;
	}

	public async Task<IHttpClient> CreateHttpClient(CookieContainer? cookieContainer = null)
	{
		return await GetHttpClientFactory().Create(ConnectionOptions, Logger, skipRequestLogging: false, cookieContainer).ConfigureAwait(continueOnCapturedContext: false);
	}

	protected void AddTenantIfNeeded(Dictionary<string, string> queryParams)
	{
		if (!string.IsNullOrWhiteSpace(ConnectionOptions.Tenant))
		{
			queryParams.Add("tenant", Uri.EscapeDataString(ConnectionOptions.Tenant));
		}
	}

	protected void AddDeploymentIdIfNeeded(Dictionary<string, string> queryParams)
	{
		if (!string.IsNullOrEmpty(ConnectionOptions.DeploymentId))
		{
			queryParams.Add("deploymentId", Uri.EscapeDataString(ConnectionOptions.DeploymentId));
		}
	}

	protected async Task<bool> TryWriteToFile(Stream stream, IFileSystem fileSystem, string filepath)
	{
		IFileSystem fileSystem2 = fileSystem;
		string filepath2 = filepath;
		Stream stream2 = stream;
		return await WithRetriesOnFileUsedByAnotherProcess(async delegate
		{
			try
			{
				fileSystem2.CreateDirectoryForFile(filepath2);
				if (fileSystem2.Exists(filepath2))
				{
					fileSystem2.DeleteFile(filepath2);
				}
				stream2.Position = 0L;
				using (Stream fileStream = fileSystem2.CreateFile(filepath2))
				{
					await stream2.CopyToAsync(fileStream).ConfigureAwait(continueOnCapturedContext: false);
				}
				return true;
			}
			catch (UnauthorizedAccessException ex)
			{
				Logger.Exception(ex);
				return false;
			}
		}, 3).ConfigureAwait(continueOnCapturedContext: false);
	}

	private async Task<bool> WithRetriesOnFileUsedByAnotherProcess(Func<Task<bool>> action, int retries)
	{
		for (int i = 0; i < retries; i++)
		{
			try
			{
				return await action().ConfigureAwait(continueOnCapturedContext: false);
			}
			catch (IOException ex)
			{
				Logger.Exception(ex);
				if (ex.HResult != 32)
				{
					return false;
				}
				Logger.Info(string.Format(CultureInfo.CurrentCulture, DeploymentResources.RetryingDownloadMessage, i + 1, retries));
				await Task.Delay(TimeSpan.FromSeconds(1.0)).ConfigureAwait(continueOnCapturedContext: false);
			}
		}
		return false;
	}

	protected async Task<ServerInfo?> GetServerInfo(HttpResponseMessage response, bool allowNullReturnValue = false)
	{
		if (!allowNullReturnValue)
		{
			response.EnsureSuccessStatusCode();
		}
		else if (!response.IsSuccessStatusCode)
		{
			return null;
		}
		ServerInfo serverInfo = await response.TryReadAsAsync<ServerInfo>().ConfigureAwait(continueOnCapturedContext: false);
		if (serverInfo == null)
		{
			throw new HttpRequestException(DeploymentResources.WrongServerInfo);
		}
		if (serverInfo != null)
		{
			serverInfo.ConnectionOptions = ConnectionOptions;
		}
		return serverInfo;
	}
}
