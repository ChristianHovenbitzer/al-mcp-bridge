using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Dynamics.Nav.Deployment.Http;

internal class HttpClientImpl : IHttpClient, IDisposable
{
	private readonly HttpClient client;

	public AuthenticationHeaderValue AuthorizationHeader
	{
		get
		{
			return client.DefaultRequestHeaders.Authorization;
		}
		set
		{
			client.DefaultRequestHeaders.Authorization = value;
		}
	}

	public Uri BaseAddress
	{
		get
		{
			return client.BaseAddress;
		}
		set
		{
			client.BaseAddress = value;
		}
	}

	public HttpClient HttpClient => client;

	public HttpClientImpl(HttpClient client)
	{
		this.client = client;
	}

	public Task<HttpResponseMessage> GetAsync(Uri uri)
	{
		return client.GetAsync(uri);
	}

	public Task<HttpResponseMessage> PostAsync(Uri uri, HttpContent content, CancellationToken cancellationToken)
	{
		return client.PostAsync(uri, content, cancellationToken);
	}

	public void Dispose()
	{
		client.Dispose();
	}
}
