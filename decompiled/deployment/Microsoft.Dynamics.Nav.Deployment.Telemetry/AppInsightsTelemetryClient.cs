using System;
using System.Collections.Generic;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;

namespace Microsoft.Dynamics.Nav.Deployment.Telemetry;

internal class AppInsightsTelemetryClient : IAppInsightsTelemetryClient
{
	private readonly TelemetryClient client;

	public TelemetryContext Context => client.Context;

	public AppInsightsTelemetryClient(TelemetryConfiguration configuration)
	{
		client = new TelemetryClient(configuration);
	}

	public void Flush()
	{
		client.Flush();
	}

	public void TrackEvent(string eventName, IDictionary<string, string> properties = null, IDictionary<string, double> metrics = null)
	{
		client.TrackEvent(eventName, properties, metrics);
	}

	public void TrackMetric(string name, double value, IDictionary<string, string> properties = null)
	{
		client.TrackMetric(name, value, properties);
	}

	public void TrackRequest(string name, DateTimeOffset startTime, TimeSpan duration, string responseCode, bool success)
	{
		client.TrackRequest(name, startTime, duration, responseCode, success);
	}

	public void TrackTrace(string message, SeverityLevel severityLevel, IDictionary<string, string> properties)
	{
		client.TrackTrace(message, severityLevel, properties);
	}

	public void TrackTrace(string message, SeverityLevel severityLevel)
	{
		client.TrackTrace(message, severityLevel);
	}

	public void TrackException(Exception exception, IDictionary<string, string> properties = null, IDictionary<string, double> metrics = null)
	{
		client.TrackException(exception, properties, metrics);
	}
}
