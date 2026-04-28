using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Globalization;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Dynamics.Nav.CodeAnalysis.Telemetry;

namespace Microsoft.Dynamics.Nav.Deployment.Telemetry;

internal sealed class AppInsightsTelemetryService : ITelemetryService, IDisposable
{
	private const string ClientActivityIdKey = "clientActivityId";

	private const string AadTenantIdKey = "aadTenantId";

	private const int maxTraceLength = 32678;

	private readonly IAppInsightsTelemetryClient client;

	private string sessionId;

	public TelemetryLevel TelemetryLevel { get; set; }

	public IDictionary<string, string> Context => client.Context.GlobalProperties;

	public AppInsightsTelemetryService(string key, TelemetryLevel telemetryLevel, IAppInsightsTelemetryClient client = null)
	{
		if (string.IsNullOrWhiteSpace(key))
		{
			throw new ArgumentNullException("key");
		}
		if (client == null)
		{
			TelemetryConfiguration telemetryConfiguration = new TelemetryConfiguration
			{
				InstrumentationKey = key,
				TelemetryChannel = new InMemoryChannel()
			};
			telemetryConfiguration.DefaultTelemetrySink.TelemetryProcessorChainBuilder.Use((ITelemetryProcessor next) => new ExceptionTelemetryMessageProcessor(next)).Build();
			this.client = new AppInsightsTelemetryClient(telemetryConfiguration);
			this.client.Context.Cloud.RoleInstance = "Redacted";
			this.client.Context.Cloud.RoleName = "Redacted";
			this.client.Context.Location.Ip = "255.0.0.1";
		}
		else
		{
			this.client = client;
		}
		TelemetryLevel = telemetryLevel;
	}

	public void SetUserId(string userId)
	{
		client.Context.User.Id = userId;
	}

	public string GetUserId()
	{
		return client.Context.User.Id;
	}

	public void StartSession(string id, bool? isFirst = null)
	{
		sessionId = id;
		client.Context.Session.Id = id.MarkAsInternal();
		client.Context.Session.IsFirst = isFirst;
	}

	public string GetSessionId()
	{
		return sessionId;
	}

	public string GenerateAndAttachNewRequestId()
	{
		string text = Guid.NewGuid().ToString();
		Context["clientActivityId"] = text.MarkAsInternal();
		return text;
	}

	public bool ClearRequestId()
	{
		return Context.Remove("clientActivityId");
	}

	public void SetAadTenantId(string aadTenantId)
	{
		Context["aadTenantId"] = aadTenantId?.MarkAsInternal();
	}

	public void Flush()
	{
		try
		{
			client.Flush();
		}
		catch
		{
		}
	}

	public void TrackInfo(string message, params object[] args)
	{
		TrackTrace(message, args, SeverityLevel.Information);
	}

	public void TrackRequest(string name, DateTimeOffset startTime, TimeSpan duration, string responseCode, bool success)
	{
		if (TelemetryLevel == TelemetryLevel.All || (!success && TelemetryLevel >= TelemetryLevel.Error))
		{
			client.TrackRequest(name, startTime, duration, responseCode, success);
		}
	}

	public void TrackError(string error, params object[] args)
	{
		TrackTrace(error, args, SeverityLevel.Error);
	}

	public void TrackError(string error, IDictionary<string, string> properties)
	{
		client.TrackTrace(error, SeverityLevel.Error, properties);
	}

	public void TrackException(Exception ex, EventLevel level = EventLevel.Error)
	{
		if ((level == EventLevel.Error && TelemetryLevel >= TelemetryLevel.Crash) || (level == EventLevel.Informational && TelemetryLevel == TelemetryLevel.All))
		{
			client.TrackTrace(ex.GetType().ToString(), (level != EventLevel.Error) ? SeverityLevel.Information : SeverityLevel.Error);
		}
	}

	private void TrackTrace(string message, object[] args, SeverityLevel severity)
	{
		if (!ShouldLog(TelemetryLevel, severity))
		{
			return;
		}
		string text = Format(message, args);
		if (text.Length <= 32678)
		{
			client.TrackTrace(text, severity);
			return;
		}
		for (int i = 0; i <= text.Length / 32678; i++)
		{
			int num = 32678 * i;
			client.TrackTrace(text.Substring(num, Math.Min(32678, text.Length - num)), severity);
		}
	}

	private static bool ShouldLog(TelemetryLevel telemetryLevel, SeverityLevel level)
	{
		return level switch
		{
			SeverityLevel.Critical => telemetryLevel >= TelemetryLevel.Crash, 
			SeverityLevel.Error => telemetryLevel >= TelemetryLevel.Error, 
			_ => telemetryLevel >= TelemetryLevel.All, 
		};
	}

	public void TrackMetric(string name, double value)
	{
		if (TelemetryLevel == TelemetryLevel.All)
		{
			client.TrackMetric(name, value);
		}
	}

	public void TrackEvent(string eventName, IDictionary<string, string> properties = null, IDictionary<string, double> metrics = null)
	{
		if (TelemetryLevel == TelemetryLevel.All)
		{
			client.TrackEvent(eventName, properties, metrics);
		}
	}

	public void TrackEvent(CustomTelemetryEvent eventInfo)
	{
		TrackEvent(eventInfo.Name, eventInfo.Dimensions, eventInfo.Metrics);
	}

	public void Dispose()
	{
		Flush();
	}

	private static string Format(string s, object[] args)
	{
		if (args != null && args.Length != 0)
		{
			return string.Format(CultureInfo.InvariantCulture, s, args);
		}
		return s;
	}
}
