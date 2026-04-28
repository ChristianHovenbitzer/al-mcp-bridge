using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Dynamics.BusinessCentral.ALMcp.Services;
using Microsoft.Extensions.DependencyInjection;
using ModelContextProtocol.Server;

namespace Microsoft.Dynamics.BusinessCentral.ALMcp.Tools;

[McpServerToolType]
public class CompileTool
{
	private const string ToolName = "al_compile";

	[McpServerTool(Name = "al_compile")]
	[Description("Compiles the AL workspace and returns diagnostics without generating .app package. Fastest option for validation-only scenarios (no .app output). When to use: Quick syntax/semantic validation during development. Use al_build instead when you need the .app package or multi-project workspace builds. Parameters: Set OnlyErrors=true to filter out warnings (recommended). MaxDiagnosticsPerCompilation limits output (default 100). EnableCodeAnalysis=true enables code analyzers (CodeCop, AppSourceCop, etc.) during compilation. CodeAnalyzers specifies which analyzers to run (e.g. ['${CodeCop}','${AppSourceCop}']). Returns: Compilation result with Succeeded flag and array of diagnostics (Severity, Code, Description, Location). Prerequisites: Run al_downloadsymbols first if compilation fails with missing symbol errors. Use cases: Quick syntax/semantic validation, checking code changes before full build. Use al_build instead when you need the .app package.")]
	public static async Task<CompilationServiceResponse> Compile(McpServer server, [Description("Optional compilation options: Set 'OnlyErrors=true' to return only error diagnostics (filters out warnings, info, hidden - recommended), 'MaxDiagnosticsPerCompilation' to limit results (default: 100)")] CompilationServiceOptions? options = null, CancellationToken cancellationToken = default(CancellationToken))
	{
		CompilationServiceOptions options2 = options;
		McpTelemetryService telemetry = server.Services?.GetService<McpTelemetryService>();
		CompilationService compilationService = server.Services.GetService<CompilationService>();
		return await telemetry.TrackIfAvailableAsync("al_compile", () => compilationService.Compile(options2, cancellationToken)).ConfigureAwait(continueOnCapturedContext: false);
	}
}
Latest version is '10.0.0.8330' (yours is '8.2.0.7535-95108c96')
