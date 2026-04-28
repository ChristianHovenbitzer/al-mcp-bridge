using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Dynamics.BusinessCentral.ALMcp.Services;
using Microsoft.Dynamics.Nav.Deployment;
using Microsoft.Dynamics.Nav.LanguageModelTools;
using Microsoft.Dynamics.Nav.LanguageModelTools.DownloadSymbols;
using Microsoft.Extensions.DependencyInjection;
using ModelContextProtocol.Server;

namespace Microsoft.Dynamics.BusinessCentral.ALMcp.Tools;

[McpServerToolType]
public class DownloadSymbolsTool
{
	private const string ToolName = "al_downloadsymbols";

	private static int GetDownloadedCount(ToolResponse response)
	{
		if (response?.Data is DownloadSymbolsResult downloadSymbolsResult)
		{
			return downloadSymbolsResult.DownloadedCount;
		}
		return 0;
	}

	[McpServerTool(Name = "al_downloadsymbols")]
	[Description("Downloads dependent symbols (.app files) required for AL compilation. Parameters: projectPath (AL project folder with app.json). For cloud: environmentName (e.g., 'sandbox'), tenant, authentication='AAD'. For on-premise: serverUrl, serverInstance, port, authentication='Windows'|'UserPassword'. Common: globalSourcesOnly=true (default: false) skips server connection (AppSource/Microsoft symbols only, no authentication required), force=true (default: false) re-downloads cached symbols, noCache=true (default: false) bypasses token cache, useInteractiveLogin=true (default) opens browser for auth automatically. Returns: Download result with count of symbols downloaded. System symbols included if platform/application in app.json. Downloads are saved to the .alpackages folder. Authentication: useInteractiveLogin=true (default) handles auth automatically via browser - no need to call al_auth_login first. globalSourcesOnly=true requires no authentication. For on-premise Windows auth, no login is needed. For on-premise UserPassword, credentials are prompted. Next steps: Run al_build or al_compile after downloading symbols.")]
	public static async Task<ToolResponse> DownloadSymbolsAsync(McpServer server, [Description("Optional AL project folder path. Used to locate app.json and launch.json.")] string? projectPath = null, [Description("Server URL for on-premise deployment (e.g., 'http://localhost').")] string? serverUrl = null, [Description("Server instance name for on-premise deployment (e.g., 'BC').")] string? serverInstance = null, [Description("Port number for on-premise development service.")] int? port = null, [Description("Environment name for cloud deployment (e.g., 'sandbox', 'production').")] string? environmentName = null, [Description("Environment type: 'OnPrem', 'Sandbox', or 'Production'.")] string? environmentType = null, [Description("Authentication method: 'AAD' (default for cloud), 'Windows', or 'UserPassword'.")] string? authentication = null, [Description("Tenant ID for multi-tenant environments.")] string? tenant = null, [Description("Set to true to force re-authentication (bypass cached tokens).")] bool noCache = false, [Description("Set to true to force re-download all symbols even if they exist in cache.")] bool force = false, [Description("Set to true to download symbols from global sources only (AppSource, Microsoft). No server connection required.")] bool globalSourcesOnly = false, [Description("If true, uses interactive browser login when AAD authentication is required (VS Code-like). No tokens or device codes are returned.")] bool useInteractiveLogin = true, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (server == null)
		{
			throw new ArgumentNullException("server");
		}
		McpTelemetryService.TrackToolInvocation("al_downloadsymbols");
		McpEmitLogger logger = server.Services.GetRequiredService<McpEmitLogger>();
		AuthContextService authContextService = server.Services?.GetService<AuthContextService>();
		CompilationService compilationService = server.Services?.GetService<CompilationService>();
		if (compilationService == null)
		{
			return new ToolResponse
			{
				Succeeded = false,
				Message = "Compilation service not available. Ensure a project is loaded."
			};
		}
		DownloadSymbolsService service = new DownloadSymbolsService(compilationService.Workspace, logger);
		PublishEnvironment valueOrDefault = (authContextService?.Environment).GetValueOrDefault();
		DownloadSymbolsParameters parameters = new DownloadSymbolsParameters
		{
			ProjectPath = projectPath,
			Environment = valueOrDefault,
			Server = serverUrl,
			ServerInstance = serverInstance,
			Port = port,
			EnvironmentName = environmentName,
			EnvironmentType = environmentType,
			Authentication = authentication,
			Tenant = ((!string.IsNullOrWhiteSpace(tenant)) ? tenant : authContextService?.Tenant),
			NoCache = noCache,
			Force = force,
			GlobalSourcesOnly = globalSourcesOnly,
			UseModernTieAuthUrl = (authContextService?.UseModernTieAuthUrl ?? false),
			UseInteractiveLogin = useInteractiveLogin
		};
		if (string.IsNullOrEmpty(parameters.ProjectPath))
		{
			ALMcpOptions aLMcpOptions = server.Services?.GetService<ALMcpOptions>();
			if (aLMcpOptions?.Projects != null && aLMcpOptions.Projects.Length != 0)
			{
				parameters.ProjectPath = aLMcpOptions.Projects[0];
			}
		}
		McpTelemetryService telemetry = server.Services?.GetService<McpTelemetryService>();
		ToolResponse toolResponse = await telemetry.TrackIfAvailableAsync("al_downloadsymbols", () => service.DownloadSymbolsAsync(parameters, cancellationToken)).ConfigureAwait(continueOnCapturedContext: false);
		if (toolResponse.Succeeded && GetDownloadedCount(toolResponse) > 0)
		{
			try
			{
				logger.Info("Reloading workspace after symbol download...");
				compilationService.ReloadWorkspace();
				logger.Info("Workspace reloaded.");
			}
			catch (Exception ex)
			{
				logger.Info("Note: Workspace reload after symbol download failed: " + ex.Message);
			}
		}
		return toolResponse;
	}
}
Latest version is '10.0.0.8330' (yours is '8.2.0.7535-95108c96')
