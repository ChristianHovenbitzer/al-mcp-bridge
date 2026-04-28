using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.Deployment.Http;
using ModelContextProtocol.Client;

namespace Microsoft.Dynamics.Nav.Deployment.ApiClients;

internal class McpClientProvider : ApiClient, IMcpClientProvider, IServerInfoApiClient
{
	private const string McpDevServerName = "Business Central AL Development MCP";

	public const string DevMcpServerHeaderName = "Dev";

	private readonly Dictionary<string, string> headers = new Dictionary<string, string>();

	public ConnectionOptions Options => base.ConnectionOptions;

	public McpClientProvider(ConnectionOptions options, IEmitLogger logger, TroubleshootingMcpContext troubleshootingMcpContext = TroubleshootingMcpContext.Debugging, IList<(string headerName, string headerValue)>? additionalHeaderValues = null)
		: base(options, logger)
	{
		headers.TryAdd("Dev", troubleshootingMcpContext.ToString());
		if (additionalHeaderValues == null)
		{
			return;
		}
		foreach (var (key, value) in additionalHeaderValues)
		{
			headers.TryAdd(key, value);
		}
	}

	public async Task<McpClient> GetMcpClient(CancellationToken cancellationToken)
	{
		return await McpClient.CreateAsync(await CreateTransportOptions().ConfigureAwait(continueOnCapturedContext: false), null, null, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
	}

	private async Task<IClientTransport> CreateTransportOptions()
	{
		IHttpClient httpClient = await CreateHttpClient().ConfigureAwait(continueOnCapturedContext: false);
		return new HttpClientTransport(new HttpClientTransportOptions
		{
			Name = "Business Central AL Development MCP",
			Endpoint = new Uri(httpClient.BaseAddress?.ToString() + "mcp/"),
			TransportMode = HttpTransportMode.StreamableHttp,
			AdditionalHeaders = headers
		}, httpClient.HttpClient);
	}

	public Task<ServerInfo?> GetServerInfo()
	{
		return Task.FromResult<ServerInfo>(null);
	}
}
