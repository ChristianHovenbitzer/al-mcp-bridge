using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.CodeAnalysis.CommandLine;
using Microsoft.Dynamics.Nav.CodeAnalysis.Logging;
using Microsoft.Dynamics.Nav.CodeAnalysis.Packaging;
using Microsoft.Dynamics.Nav.CodeAnalysis.Telemetry;
using Microsoft.Dynamics.Nav.Deployment.Telemetry;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Log;

internal static class ExternalTelemetryLogger
{
	private const string UserIdFile = "userid.txt";

	private static readonly object SyncRoot = new object();

	public static bool EnableTelemetry
	{
		set
		{
			TelemetryServiceManager.LogTelemetry = value;
		}
	}

	public static TelemetryLevel TelemetryLevel
	{
		set
		{
			TelemetryServiceManager.TelemetryLevel = value;
		}
	}

	public static ITelemetryService TelemetryService => TelemetryServiceManager.CurrentTelemetryService;

	public static void Initialize(ProjectManifest manifest)
	{
		if (manifest == null)
		{
			throw new ArgumentNullException("manifest");
		}
		lock (SyncRoot)
		{
			NavAppManifest appManifest = manifest.AppManifest;
			IDictionary<string, string> context = TelemetryService.Context;
			context["app_id"] = appManifest.AppId.ToString().MarkAsInternal();
			string value = string.Empty;
			if (appManifest.AppVersion != null)
			{
				value = appManifest.AppVersion.ToString();
			}
			context["app_version"] = value;
			string value2 = string.Empty;
			if (appManifest.Dependencies != null)
			{
				value2 = string.Join(", ", appManifest.Dependencies.Select((NavAppDependency x) => x.AppId.ToString().MarkAsInternal() + "_" + x.MinVersion));
			}
			context["app_dependencies"] = value2;
			string value3 = string.Empty;
			if (appManifest.Application != null)
			{
				value3 = "Version: " + appManifest.Application;
			}
			context["app_application"] = value3;
			string value4 = string.Empty;
			if (appManifest.Platform != null)
			{
				value4 = appManifest.Platform.ToString();
			}
			context["app_platform"] = value4;
		}
	}

	public static async Task InitializeNewSessionAsync(string? sessionId)
	{
		bool flag = await CheckIsInternalAsync();
		lock (SyncRoot)
		{
			string orCreateUserId = GetOrCreateUserId();
			if (!string.IsNullOrWhiteSpace(orCreateUserId))
			{
				TelemetryService.SetUserId(orCreateUserId.MarkAsInternal());
			}
			TelemetryService.StartSession(sessionId ?? Guid.NewGuid().ToString());
			TelemetryService.Context["internal"] = flag.ToString();
			TelemetryService.TrackInfo("New session");
		}
	}

	public static PooledStopwatch BeginOperation()
	{
		PooledStopwatch instance = PooledStopwatch.GetInstance();
		instance.Start();
		return instance;
	}

	public static void EndOperation(string operationName, PooledStopwatch stopwatch, TimeSpan threshold)
	{
		try
		{
			stopwatch.Stop();
			if (stopwatch.Elapsed > threshold)
			{
				TelemetryService.TrackMetric(operationName, stopwatch.ElapsedMilliseconds);
			}
		}
		finally
		{
			stopwatch.Dispose();
		}
	}

	public static void TrackException(Exception ex)
	{
		TelemetryService.TrackException(ex);
	}

	public static void TrackError(string error, params object[] args)
	{
		TelemetryService.TrackError(error, args);
	}

	public static void TrackError(string error, IDictionary<string, string> dict)
	{
		TelemetryService.TrackError(error, dict);
	}

	public static void TrackEvent(CustomTelemetryEvent eventInfo)
	{
		TelemetryService.TrackEvent(eventInfo.Name, eventInfo.Dimensions, eventInfo.Metrics);
	}

	public static void Dispose()
	{
		TelemetryService.Dispose();
	}

	private static string GetOrCreateUserId()
	{
		string text = Path.Combine(GetBaseDirectory(), "userid.txt");
		try
		{
			if (File.Exists(text))
			{
				return Guid.Parse(File.ReadAllText(text)).ToString();
			}
		}
		catch
		{
		}
		return CreateUserId(text);
	}

	private static string CreateUserId(string file)
	{
		try
		{
			string text = Guid.NewGuid().ToString();
			File.WriteAllText(file, text);
			return text;
		}
		catch
		{
			return string.Empty;
		}
	}

	private static async Task<bool> CheckIsInternalAsync()
	{
		try
		{
			IPHostEntry iPHostEntry = await Dns.GetHostEntryAsync("localhost");
			if (string.IsNullOrEmpty(iPHostEntry.HostName))
			{
				return false;
			}
			return iPHostEntry.HostName.EndsWith(".microsoft.com", StringComparison.OrdinalIgnoreCase);
		}
		catch
		{
			return false;
		}
	}

	private static string GetBaseDirectory()
	{
		return AppDomain.CurrentDomain.BaseDirectory;
	}
}
