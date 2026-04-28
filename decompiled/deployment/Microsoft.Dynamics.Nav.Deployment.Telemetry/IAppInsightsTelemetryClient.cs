using System;
using System.Collections.Generic;
using Microsoft.ApplicationInsights.DataContracts;

namespace Microsoft.Dynamics.Nav.Deployment.Telemetry;

public interface IAppInsightsTelemetryClient
{
	TelemetryContext Context { get; }

	void Flush();

	void TrackRequest(string name, DateTimeOffset startTime, TimeSpan duration, string responseCode, bool success);

	void TrackTrace(string message, SeverityLevel severityLevel, IDictionary<string, string> properties);

	void TrackTrace(string message, SeverityLevel severityLevel);

	void TrackMetric(string name, double value, IDictionary<string, string> properties = null);

	void TrackEvent(string eventName, IDictionary<string, string> properties = null, IDictionary<string, double> metrics = null);

	void TrackException(Exception exception, IDictionary<string, string> properties = null, IDictionary<string, double> metrics = null);
}
