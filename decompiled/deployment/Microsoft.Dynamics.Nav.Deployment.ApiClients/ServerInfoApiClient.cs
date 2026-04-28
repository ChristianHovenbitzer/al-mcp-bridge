using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.Deployment.Http;

namespace Microsoft.Dynamics.Nav.Deployment.ApiClients;

internal class ServerInfoApiClient : ApiClient, IServerInfoApiClient
{
	private class LegacyWebEndpointResponse
	{
		public Uri WebEndpoint { get; set; }
	}

	public ServerInfoApiClient(ConnectionOptions options, IEmitLogger logger)
		: base(options, logger)
	{
	}

	public async Task<ServerInfo?> GetServerInfo()
	{
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		AddTenantIfNeeded(dictionary);
		AddDeploymentIdIfNeeded(dictionary);
		Uri metadataUri = new Uri("dev/metadata" + UriHelper.CreateQueryString(dictionary), UriKind.Relative);
		IHttpClient client = await CreateHttpClient().ConfigureAwait(continueOnCapturedContext: false);
		HttpResponseMessage httpResponseMessage = await client.GetAsync(metadataUri).ConfigureAwait(continueOnCapturedContext: false);
		if (httpResponseMessage.StatusCode == HttpStatusCode.NotFound)
		{
			return await TryGetLegacyServerInfo(client).ConfigureAwait(continueOnCapturedContext: false);
		}
		ServerInfo obj = await GetServerInfo(httpResponseMessage).ConfigureAwait(continueOnCapturedContext: false);
		obj.Kind = ServerInfoKind.Dev;
		return obj;
	}

	private async Task<ServerInfo> TryGetLegacyServerInfo(IHttpClient client)
	{
		HttpResponseMessage obj = await client.GetAsync(new Uri("dev/webendpoint", UriKind.Relative)).ConfigureAwait(continueOnCapturedContext: false);
		obj.EnsureSuccessStatusCode();
		LegacyWebEndpointResponse legacyWebEndpointResponse = await obj.TryReadAsAsync<LegacyWebEndpointResponse>().ConfigureAwait(continueOnCapturedContext: false);
		return new ServerInfo
		{
			ConnectionOptions = base.ConnectionOptions,
			WebEndpoint = legacyWebEndpointResponse.WebEndpoint,
			DebuggerVersion = new Version(1, 0),
			RuntimeVersion = new Version(1, 0),
			WebApiVersion = new Version(1, 0),
			Kind = ServerInfoKind.Dev
		};
	}
}
