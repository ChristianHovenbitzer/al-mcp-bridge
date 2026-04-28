using System.Reflection;
using Microsoft.Dynamics.Nav.CodeAnalysis.Telemetry;

namespace Microsoft.Dynamics.Nav.Deployment.Telemetry;

internal static class TelemetryServiceManager
{
	private static readonly ITelemetryService voidTelemetryService = new VoidTelemetryService();

	private static readonly AppInsightsTelemetryService appInsightsTelemetryService = CreateTelemetryService();

	private static TelemetryLevel telemetryLevel;

	internal const string InstrumentationKey = "02131901-fea4-4243-b78a-e6e3a0e8a51a";

	internal const string Area = "md";

	public static string VersionString => typeof(TelemetryServiceManager).GetTypeInfo().Assembly.GetName().Version.ToString();

	public static bool LogTelemetry { get; set; } = false;


	public static TelemetryLevel TelemetryLevel
	{
		get
		{
			return telemetryLevel;
		}
		set
		{
			telemetryLevel = value;
			if (appInsightsTelemetryService != null)
			{
				appInsightsTelemetryService.TelemetryLevel = telemetryLevel;
			}
		}
	}

	public static ITelemetryService CurrentTelemetryService
	{
		get
		{
			if (!LogTelemetry && TelemetryLevel == TelemetryLevel.None)
			{
				return voidTelemetryService;
			}
			return appInsightsTelemetryService;
		}
	}

	private static AppInsightsTelemetryService CreateTelemetryService()
	{
		AppInsightsTelemetryService obj = new AppInsightsTelemetryService("02131901-fea4-4243-b78a-e6e3a0e8a51a", TelemetryLevel);
		obj.Context["area"] = "md";
		obj.Context["version"] = VersionString;
		return obj;
	}
}
