using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Dynamics.BusinessCentral.ALMcp.Services;
using Microsoft.Dynamics.Nav.Deployment;
using Microsoft.Dynamics.Nav.LanguageModelTools;
using Microsoft.Dynamics.Nav.LanguageModelTools.Publish;
using Microsoft.Extensions.DependencyInjection;
using ModelContextProtocol.Server;

namespace Microsoft.Dynamics.BusinessCentral.ALMcp.Tools;

[McpServerToolType]
public class PublishAppTool
{
	private const string ToolName = "al_publish";

	[McpServerTool(Name = "al_publish")]
	[Description("Publishes AL packages with flexible options. Supports building and publishing current project (default), publishing without building (skipBuild=true), or full dependency chain publishing (buildDependencies=true). Parameters: appPath (direct .app file) or projectPath (folder with app.json, uses built .app). For cloud: environmentName (e.g., 'sandbox'), environmentType='Sandbox'|'Production', tenant. For on-premise: serverUrl, serverInstance, port, authentication='Windows'|'UserPassword'. Deployment options: schemaUpdateMode='Synchronize' (default)|'ForceSync'|'Recreate', forceUpgrade=true (default: false) bypasses version check. skipBuild=true publishes existing .app without building (like VS Code 'Publish extension without building'). buildDependencies=true builds and publishes the full dependency tree (like VS Code 'Publish full dependency tree'). Note: RAD/incremental (delta) publishing is only available in VS Code because it requires real-time change tracking. This tool always performs a full build. Auth: useInteractiveLogin=true (default) opens browser automatically - no need to call al_auth_login first, noCache=true (default: false) forces re-auth. Returns: Publish result with success/failure and server messages. No local files are modified. Prerequisites: Build project first with al_build (unless skipBuild=false which is default). Authentication: useInteractiveLogin=true (default) handles auth automatically via browser. Manual al_auth_login is only needed when caching tokens for multiple operations or troubleshooting auth issues. On-premise Windows auth requires no login. Next steps: Test extension in Business Central. Use al_auth_logout to switch accounts.")]
	public static async Task<ToolResponse> PublishAsync(McpServer server, [Description("Optional path to the .app file to publish. If not specified, uses the built package from the project's output folder.")] string? appPath = null, [Description("Optional AL project folder path. Used to locate the .app file if appPath is not specified.")] string? projectPath = null, [Description("Server URL for on-premise deployment (e.g., 'http://localhost').")] string? serverUrl = null, [Description("Server instance name for on-premise deployment (e.g., 'BC').")] string? serverInstance = null, [Description("Port number for on-premise development service.")] int? port = null, [Description("Environment name for cloud deployment (e.g., 'sandbox', 'production').")] string? environmentName = null, [Description("Environment type: 'OnPrem', 'Sandbox', or 'Production'.")] string? environmentType = null, [Description("Authentication method: 'AAD' (default for cloud), 'Windows', or 'UserPassword'.")] string? authentication = null, [Description("Tenant ID for multi-tenant environments.")] string? tenant = null, [Description("Schema update mode: 'Synchronize' (default), 'ForceSync', or 'Recreate'.")] string? schemaUpdateMode = null, [Description("Set to true to force re-authentication (bypass cached tokens).")] bool noCache = false, [Description("Set to true to force upgrade without requiring version change.")] bool forceUpgrade = false, [Description("If true, uses interactive browser login when AAD authentication is required (VS Code-like). No tokens or device codes are returned.")] bool useInteractiveLogin = true, [Description("Skip the build step and publish the existing .app package from the project's output folder. Use when the project has already been built with al_build.")] bool skipBuild = false, [Description("Include full dependency chain in publish. Set to true to build and publish all transitive dependencies.")] bool buildDependencies = false, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (server == null)
		{
			throw new ArgumentNullException("server");
		}
		McpTelemetryService.TrackToolInvocation("al_publish");
		AuthContextService authContextService = server.Services?.GetService<AuthContextService>();
		PublishService service = server.Services?.GetService<PublishService>();
		if (service == null)
		{
			return new ToolResponse
			{
				Succeeded = false,
				Message = "Publish service not available."
			};
		}
		PublishEnvironment valueOrDefault = (authContextService?.Environment).GetValueOrDefault();
		PublishParameters parameters = new PublishParameters
		{
			AppPath = appPath,
			ProjectPath = projectPath,
			Environment = valueOrDefault,
			Server = serverUrl,
			ServerInstance = serverInstance,
			Port = port,
			EnvironmentName = environmentName,
			EnvironmentType = environmentType,
			Authentication = authentication,
			Tenant = ((!string.IsNullOrWhiteSpace(tenant)) ? tenant : authContextService?.Tenant),
			SchemaUpdateMode = schemaUpdateMode,
			NoCache = noCache,
			ForceUpgrade = forceUpgrade,
			UseModernTieAuthUrl = (authContextService?.UseModernTieAuthUrl ?? false),
			UseInteractiveLogin = useInteractiveLogin,
			SkipBuild = skipBuild,
			BuildDependencies = buildDependencies
		};
		if (string.IsNullOrEmpty(parameters.ProjectPath) && string.IsNullOrEmpty(parameters.AppPath))
		{
			ALMcpOptions aLMcpOptions = server.Services?.GetService<ALMcpOptions>();
			if (aLMcpOptions?.Projects != null && aLMcpOptions.Projects.Length != 0)
			{
				parameters.ProjectPath = aLMcpOptions.Projects[0];
			}
		}
		McpTelemetryService telemetry = server.Services?.GetService<McpTelemetryService>();
		return await telemetry.TrackIfAvailableAsync("al_publish", () => service.PublishAsync(parameters, cancellationToken)).ConfigureAwait(continueOnCapturedContext: false);
	}
}
Latest version is '10.0.0.8330' (yours is '8.2.0.7535-95108c96')
