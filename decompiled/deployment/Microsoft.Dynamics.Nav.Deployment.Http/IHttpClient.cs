using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Dynamics.Nav.Deployment.Http;

internal interface IHttpClient : IDisposable
{
	AuthenticationHeaderValue AuthorizationHeader { get; set; }

	Uri BaseAddress { get; set; }

	HttpClient HttpClient { get; }

	Task<HttpResponseMessage> GetAsync(Uri uri);

	Task<HttpResponseMessage> PostAsync(Uri uri, HttpContent content, CancellationToken cancellationToken);
}
