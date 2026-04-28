using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Dynamics.BusinessCentral.ALMcp.Services;
using Microsoft.Dynamics.Nav.LanguageModelTools;
using Microsoft.Dynamics.Nav.LanguageModelTools.Build;
using Microsoft.Extensions.DependencyInjection;
using ModelContextProtocol.Server;

namespace Microsoft.Dynamics.BusinessCentral.ALMcp.Tools;

[McpServerToolType]
public class BuildTool
{
	private const string ToolName = "al_build";

	[McpServerTool(Name = "al_build")]
	[Description("Builds AL projects, generates .app packages, and returns diagnostics. Supports multi-project workspace builds. When to use: Use al_build for full project builds with .app generation. For quick validation without .app generation, use al_compile instead.Parameters: scope='current' (default) builds one project, scope='all' builds entire workspace with dependencies. projectPath specifies which project folder to build (absolute path to folder containing app.json). outputPath optionally specifies where to write the .app file (defaults to project output folder). onlyErrors=true (default: false) filters output to errors only (recommended for large projects). maxDiagnostics limits number of diagnostics returned (default 100). enableCodeAnalysis=true enables code analyzers (CodeCop, AppSourceCop, etc.) during the build. When not specified, uses the server's startup configuration. codeAnalyzers specifies which analyzers to run (e.g. ['${CodeCop}','${AppSourceCop}','${PerTenantExtensionCop}','${UICop}']). When not specified, uses the server's startup configuration. Returns: Build result with diagnostics (errors, warnings). On success, .app file is created in project output folder or specified outputPath. Prerequisites: Run al_downloadsymbols first if build fails with 'symbol not found' errors. Next steps: Use al_getdiagnostics to inspect errors, or al_publish to deploy.")]
	public static async Task<ToolResponse> BuildAsync(McpServer server, [Description("Build scope - 'current' for active project only, 'all' for workspace with full dependency tree.")] string scope = "current", [Description("Optional AL project folder path to build. If not specified, builds the default project.")] string? projectPath = null, [Description("Optional output path for the generated .app file. If not specified, uses the project's default output folder.")] string? outputPath = null, [Description("Set to true to return only error diagnostics (filters out warnings, info, hints).")] bool onlyErrors = false, [Description("Maximum number of diagnostics to return. Default: 100.")] int? maxDiagnostics = null, [Description("Set to true to enable code analysis with analyzers (CodeCop, AppSourceCop, etc.). When not specified, uses the server's startup configuration.")] bool? enableCodeAnalysis = null, [Description("Code analyzers to use. Well-known values: '${CodeCop}', '${AppSourceCop}', '${PerTenantExtensionCop}', '${UICop}'. When not specified, uses the server's startup configuration.")] string[]? codeAnalyzers = null, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (server == null)
		{
			throw new ArgumentNullException("server");
		}
		McpTelemetryService.TrackToolInvocation("al_build");
		BuildService service = server.Services?.GetService<BuildService>();
		if (service == null)
		{
			throw new InvalidOperationException("Build service is not available.");
		}
		McpTelemetryService telemetry = server.Services?.GetService<McpTelemetryService>();
		BuildScope scope2 = (string.Equals(scope, "all", StringComparison.OrdinalIgnoreCase) ? BuildScope.All : BuildScope.Current);
		BuildParameters parameters = new BuildParameters
		{
			Scope = scope2,
			ProjectPath = projectPath,
			OutputPath = outputPath,
			OnlyErrors = onlyErrors,
			MaxDiagnostics = maxDiagnostics,
			EnableCodeAnalysis = enableCodeAnalysis,
			CodeAnalyzers = codeAnalyzers
		};
		return await telemetry.TrackIfAvailableAsync("al_build", () => service.BuildAsync(parameters, cancellationToken)).ConfigureAwait(continueOnCapturedContext: false);
	}
}
Latest version is '10.0.0.8330' (yours is '8.2.0.7535-95108c96')
