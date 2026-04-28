using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;

namespace Microsoft.Dynamics.Nav.Deployment.Telemetry;

internal class ExceptionTelemetryMessageProcessor : ITelemetryProcessor
{
	private ITelemetryProcessor Next { get; set; }

	public ExceptionTelemetryMessageProcessor(ITelemetryProcessor next)
	{
		Next = next;
	}

	public void Process(ITelemetry item)
	{
		if (item is ExceptionTelemetry exceptionTelemetry)
		{
			foreach (ExceptionDetailsInfo exceptionDetailsInfo in exceptionTelemetry.ExceptionDetailsInfoList)
			{
				exceptionDetailsInfo.Message = "Redacted";
			}
		}
		Next.Process(item);
	}
}
