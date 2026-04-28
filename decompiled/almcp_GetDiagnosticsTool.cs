using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Dynamics.BusinessCentral.ALMcp.Services;
using Microsoft.Dynamics.Nav.LanguageModelTools.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using ModelContextProtocol.Server;

namespace Microsoft.Dynamics.BusinessCentral.ALMcp.Tools;

[McpServerToolType]
public class GetDiagnosticsTool
{
	private const string ToolName = "al_getdiagnostics";

	[McpServerTool(Name = "al_getdiagnostics")]
	[Description("Retrieves compilation diagnostics (errors, warnings, info, hints) from AL code. Parameters: Scope by filePath (single .al file), folderPath (recursive), or projectPath (AL project folder). Filter by severities (['error','warning']) and areas (['AL']). Use limit to cap results (default 200, max 500). Returns: List of diagnostics with severity, code, message, and file location. Use cases: Inspect build failures, find all errors in a project, check specific file for issues. Next steps: Fix reported issues in source files, then run al_build to verify fixes.")]
	public static async Task<DiagnosticsResult> GetDiagnosticsAsync(McpServer server, [Description("Diagnostics scope: file path (.al/.dal).")] string? filePath = null, [Description("Diagnostics scope: folder path (recursive).")] string? folderPath = null, [Description("Diagnostics scope: AL project folder path.")] string? projectPath = null, [Description("Filter by diagnostic severity. Default: all.")] string[]? severities = null, [Description("Filter by diagnostic source (matches diagnostic.source). Default: all.")] string[]? areas = null, [Description("Max diagnostics returned. Default: 200, max: 500.")] int? limit = null, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (server == null)
		{
			throw new ArgumentNullException("server");
		}
		McpTelemetryService.TrackToolInvocation("al_getdiagnostics");
		DiagnosticsService service = server.Services?.GetService<DiagnosticsService>();
		if (service == null)
		{
			throw new InvalidOperationException("Diagnostics service is not available.");
		}
		McpTelemetryService telemetry = server.Services?.GetService<McpTelemetryService>();
		DiagnosticsParameters parameters = new DiagnosticsParameters
		{
			FilePath = filePath,
			FolderPath = folderPath,
			ProjectPath = projectPath,
			Severities = severities,
			Areas = areas,
			Limit = limit
		};
		return await telemetry.TrackIfAvailableAsync("al_getdiagnostics", () => service.GetDiagnosticsAsync(parameters, cancellationToken)).ConfigureAwait(continueOnCapturedContext: false);
	}
}
Latest version is '10.0.0.8330' (yours is '8.2.0.7535-95108c96')
