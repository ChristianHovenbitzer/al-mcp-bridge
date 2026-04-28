using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Dynamics.Nav.Deployment.Http;

internal class NavHttpClientHandler : DelegatingHandler
{
	private readonly IEmitLogger logger;

	private readonly bool skipRequestLog;

	public NavHttpClientHandler(DelegatingHandler innerHandler, IEmitLogger logger, bool skipRequestLog)
		: base(innerHandler)
	{
		this.logger = logger;
		this.skipRequestLog = skipRequestLog;
	}

	protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
	{
		if (!skipRequestLog)
		{
			logger.Info(DeploymentResources.SendingRequest, request.RequestUri);
		}
		return await (await base.SendAsync(request, cancellationToken).ConfigureAwait(continueOnCapturedContext: false)).LogIfResponseIsNotOkOrUnauthorized(request.RequestUri, logger).ConfigureAwait(continueOnCapturedContext: false);
	}
}
