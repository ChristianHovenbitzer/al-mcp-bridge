using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using Microsoft.Dynamics.Nav.CodeAnalysis.Telemetry;

namespace Microsoft.Dynamics.Nav.Deployment.Telemetry;

internal class VoidTelemetryService : ITelemetryService, IDisposable
{
	public IDictionary<string, string> Context { get; }

	public VoidTelemetryService()
	{
		Context = new Dictionary<string, string>();
	}

	public void Dispose()
	{
	}

	public void SetUserId(string userId)
	{
	}

	public string GetUserId()
	{
		return null;
	}

	public void StartSession(string id, bool? isFirst = null)
	{
	}

	public string GetSessionId()
	{
		return null;
	}

	public string GenerateAndAttachNewRequestId()
	{
		return null;
	}

	public bool ClearRequestId()
	{
		return true;
	}

	public void SetAadTenantId(string aadTenantId)
	{
	}

	public void TrackRequest(string name, DateTimeOffset startTime, TimeSpan duration, string responseCode, bool success)
	{
	}

	public void TrackInfo(string message, params object[] args)
	{
	}

	public void TrackError(string error, params object[] args)
	{
	}

	public void TrackError(string error, IDictionary<string, string> properties)
	{
	}

	public void TrackException(Exception ex, EventLevel level = EventLevel.Error)
	{
	}

	public void TrackMetric(string name, double value)
	{
	}

	public void Flush()
	{
	}

	public void TrackEvent(string eventName, IDictionary<string, string> properties = null, IDictionary<string, double> metrics = null)
	{
	}

	public void TrackEvent(CustomTelemetryEvent eventInfo)
	{
	}
}
