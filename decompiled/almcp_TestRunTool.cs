using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Dynamics.BusinessCentral.ALMcp.Services;
using Microsoft.Dynamics.Nav.Deployment;
using Microsoft.Dynamics.Nav.LanguageModelTools;
using Microsoft.Dynamics.Nav.LanguageModelTools.TestRunning;
using Microsoft.Extensions.DependencyInjection;
using ModelContextProtocol.Server;

namespace Microsoft.Dynamics.BusinessCentral.ALMcp.Tools;

[McpServerToolType]
public class TestRunTool
{
	private const string ToolName = "al_run_tests";

	[McpServerTool(Name = "al_run_tests")]
	[Description("Runs AL unit tests in Business Central. Connects to a BC server and executes tests in the specified codeunit, returning pass/fail results. Parameters: codeunitId (required, integer codeunit ID), testMethods (optional, array of method names), company (optional, startup company name). For cloud: environmentName (e.g., 'sandbox'), environmentType='Sandbox'|'Production', tenant. For on-premise: serverUrl, serverInstance, port, authentication='Windows'|'UserPassword'. Auth: useInteractiveLogin=true (default) opens browser automatically. Returns: Summary of test results with pass/fail counts and details for failing tests.")]
	public static async Task<ToolResponse> RunTestsAsync(McpServer server, [Description("The test codeunit ID to run (e.g., 50100).")] int codeunitId, [Description("Optional list of test method names to run within the codeunit (e.g., ['TestMethod1','TestMethod2']). Runs all methods if not specified.")] string[]? testMethods = null, [Description("Optional project folder path. Used to read connection settings from launch.json.")] string? projectPath = null, [Description("The company name to use when running tests (e.g., 'CRONUS International Ltd.'). Uses default if not specified.")] string? company = null, [Description("Server URL for on-premise deployment (e.g., 'http://localhost').")] string? serverUrl = null, [Description("Server instance name for on-premise deployment (e.g., 'BC').")] string? serverInstance = null, [Description("Port number for on-premise development service.")] int? port = null, [Description("Environment name for cloud deployment (e.g., 'sandbox', 'production').")] string? environmentName = null, [Description("Environment type: 'OnPrem', 'Sandbox', or 'Production'.")] string? environmentType = null, [Description("Authentication method: 'AAD' (default for cloud), 'Windows', or 'UserPassword'.")] string? authentication = null, [Description("Tenant ID for multi-tenant environments.")] string? tenant = null, [Description("Set to true to force re-authentication (bypass cached tokens).")] bool noCache = false, [Description("If true, uses interactive browser login when AAD authentication is required.")] bool useInteractiveLogin = true, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (server == null)
		{
			throw new ArgumentNullException("server");
		}
		McpTelemetryService.TrackToolInvocation("al_run_tests");
		AuthContextService authContextService = server.Services?.GetService<AuthContextService>();
		PublishEnvironment valueOrDefault = (authContextService?.Environment).GetValueOrDefault();
		TestRunService service = server.Services?.GetService<TestRunService>();
		if (service == null)
		{
			return new ToolResponse
			{
				Succeeded = false,
				Message = "Test run service not available."
			};
		}
		TestRunParameters parameters = new TestRunParameters
		{
			CodeunitId = codeunitId,
			TestMethods = testMethods,
			Company = company,
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
			UseInteractiveLogin = useInteractiveLogin,
			UseModernTieAuthUrl = (authContextService?.UseModernTieAuthUrl ?? false)
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
		return await telemetry.TrackIfAvailableAsync("al_run_tests", () => service.RunTestsAsync(parameters, cancellationToken)).ConfigureAwait(continueOnCapturedContext: false);
	}
}
Latest version is '10.0.0.8330' (yours is '8.2.0.7535-95108c96')
