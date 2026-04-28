using System;
using System.Net;
using System.Threading.Tasks;

namespace Microsoft.Dynamics.Nav.Deployment.Http;

internal interface IHttpClientFactory
{
	Task<IHttpClient> Create(ConnectionOptions connectionOptions, IEmitLogger logger, bool skipRequestLogging = false, CookieContainer? cookieContainer = null);

	Uri CreateBaseClientUri(ConnectionOptions connectionOptions, IEmitLogger logger);
}
