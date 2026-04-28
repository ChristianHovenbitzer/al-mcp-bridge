using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.Deployment.Telemetry;

namespace Microsoft.Dynamics.Nav.Deployment.Http;

internal abstract class HttpClientFactory : IHttpClientFactory
{
	private static readonly TimeSpan InfiniteRequestTimeout = Timeout.InfiniteTimeSpan;

	private static readonly TimeSpan RequestTimeout = TimeSpan.FromSeconds(600.0);

	protected static IHttpClient CreateWithHandlerAndLogger(HttpClientHandler finalHandler, IEmitLogger logger, bool skipRequestLogging, bool infiniteTimeout, CookieContainer? cookieContainer = null)
	{
		if (cookieContainer != null)
		{
			finalHandler.UseCookies = true;
			finalHandler.CookieContainer = cookieContainer;
		}
		finalHandler.CheckCertificateRevocationList = true;
		HttpClient client = new HttpClient(new NavHttpClientHandler(new TelemetryHttpClientHandler(finalHandler, logger), logger, skipRequestLogging));
		SetDefaultSettings(client, infiniteTimeout);
		return new HttpClientImpl(client);
	}

	private static void SetDefaultSettings(HttpClient client, bool infiniteTimeout)
	{
		client.Timeout = (infiniteTimeout ? InfiniteRequestTimeout : RequestTimeout);
		client.DefaultRequestHeaders.ExpectContinue = false;
	}

	public abstract Uri? CreateBaseClientUri(ConnectionOptions connectionOptions, IEmitLogger logger);

	public abstract Task<IHttpClient> Create(ConnectionOptions connectionOptions, IEmitLogger logger, bool skipRequestLogging = false, CookieContainer? cookieContainer = null);
}
