using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.CodeAnalysis;
using Microsoft.Dynamics.Nav.CodeAnalysis.Telemetry;
using Microsoft.Dynamics.Nav.Deployment.Http;

namespace Microsoft.Dynamics.Nav.Deployment.Telemetry;

internal class TelemetryHttpClientHandler : DelegatingHandler
{
	private const string ClientSessionHeaderName = "d365-bc-client-session-telemetry-id";

	private const string ClientActivityHeaderName = "d365-bc-client-activity-telemetry-id";

	private const string CorrelationVectorInitializationHeaderName = "x-ms-correlation-id";

	private const string DevEndpointPart = "/dev/";

	private const string SnapshotEndpointPart = "/snapshotdebugger/";

	private readonly IEmitLogger logger;

	private readonly ITelemetryService telemetryService;

	public TelemetryHttpClientHandler(HttpMessageHandler innerHandler, IEmitLogger logger)
		: this(innerHandler, TelemetryServiceManager.CurrentTelemetryService, logger)
	{
	}

	public TelemetryHttpClientHandler(HttpMessageHandler innerHandler, ITelemetryService telemetryService, IEmitLogger logger)
		: base(innerHandler)
	{
		this.logger = logger;
		this.telemetryService = telemetryService;
	}

	protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
	{
		bool success = false;
		string statusCode = string.Empty;
		Stopwatch sw = new Stopwatch();
		sw.Start();
		DateTimeOffset startTime = DateTimeOffset.UtcNow;
		string requestId = telemetryService.GenerateAndAttachNewRequestId();
		string sessionId = telemetryService.GetSessionId();
		bool useRequestCorrelationIds = !string.IsNullOrEmpty(requestId) && !string.IsNullOrEmpty(sessionId);
		if (useRequestCorrelationIds)
		{
			request.Headers.Add("d365-bc-client-activity-telemetry-id", requestId);
			request.Headers.Add("d365-bc-client-session-telemetry-id", sessionId);
			request.Headers.Add("x-ms-correlation-id", requestId);
			LocalMachineLogger.LogNormal($"Sending request to {request.RequestUri} with request ID {requestId} and session ID {sessionId}");
		}
		try
		{
			HttpResponseMessage httpResponseMessage = await base.SendAsync(request, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
			success = !httpResponseMessage.IsServerSideError();
			statusCode = httpResponseMessage.StatusCode.ToString();
			if (!httpResponseMessage.IsSuccessStatusCode && useRequestCorrelationIds)
			{
				logger.Error(httpResponseMessage.ReasonPhrase);
				logger.Error(DeploymentResources.RequestError, requestId, sessionId);
			}
			return httpResponseMessage;
		}
		catch (Exception ex) when (ex is HttpRequestException || ex is IOException || ex is TaskCanceledException)
		{
			logger.NetworkException(ex);
			if (useRequestCorrelationIds)
			{
				logger.Error(ex.Message);
				logger.Error(DeploymentResources.RequestError, requestId, sessionId);
			}
			throw ex;
		}
		finally
		{
			telemetryService.TrackRequest(TrimUri(request.RequestUri), startTime, sw.Elapsed, statusCode, success);
			telemetryService.ClearRequestId();
		}
	}

	private static string TrimUri(Uri uri)
	{
		string absolutePath = uri.AbsolutePath;
		int num = absolutePath.LastIndexOf("/dev/", StringComparison.OrdinalIgnoreCase);
		if (num != -1)
		{
			return absolutePath.Substring(num);
		}
		int num2 = absolutePath.LastIndexOf("/snapshotdebugger/", StringComparison.OrdinalIgnoreCase);
		if (num2 != -1)
		{
			return absolutePath.Substring(num2);
		}
		return "/sanitized";
	}
}
