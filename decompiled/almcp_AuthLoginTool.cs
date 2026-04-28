using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Dynamics.BusinessCentral.ALMcp.Services;
using Microsoft.Dynamics.Nav.Deployment;
using Microsoft.Dynamics.Nav.Deployment.Authentication;
using Microsoft.Extensions.DependencyInjection;
using ModelContextProtocol.Server;

namespace Microsoft.Dynamics.BusinessCentral.ALMcp.Tools;

[McpServerToolType]
public class AuthLoginTool
{
	private const string ToolName = "al_auth_login";

	[McpServerTool(Name = "al_auth_login")]
	[Description("Authenticates to Entra ID (Azure AD) for Business Central cloud operations. Opens browser for interactive login. Parameters: tenant (domain or GUID, default 'common'), environmentType='OnPrem'|'Sandbox'|'Production', environmentName (e.g., 'sandbox'), applicationFamily (optional), usernameHint for account selection. noCache=true clears cache before login. Returns: Authentication result. Tokens cached securely for subsequent operations. Use cases: Authenticate before al_downloadsymbols or al_publish for cloud. Not needed for on-premise Windows auth. Next steps: Run al_downloadsymbols or al_publish.")]
	public static async Task<AuthResponse> LoginAsync(McpServer server, [Description("Tenant to authenticate against (domain or GUID). Defaults to 'common'.")] string? tenant = null, [Description("Environment type: 'OnPrem', 'Sandbox', or 'Production'. When omitted, defaults to the value from launch.json or 'Sandbox'.")] string? environmentType = null, [Description("Environment name (e.g., 'sandbox', 'production'). Optional.")] string? environmentName = null, [Description("Application family for the cloud server (optional).")] string? applicationFamily = null, [Description("Optional username hint for selecting an account.")] string? usernameHint = null, [Description("Set to true to clear token cache before login.")] bool noCache = false, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (server == null)
		{
			throw new ArgumentNullException("server");
		}
		McpTelemetryService.TrackToolInvocation("al_auth_login");
		McpEmitLogger requiredService = server.Services.GetRequiredService<McpEmitLogger>();
		AuthContextService authContext = server.Services?.GetService<AuthContextService>();
		PublishEnvironment publishEnvironment = (authContext?.Environment).GetValueOrDefault();
		bool useModernTieAuthUrl = authContext?.UseModernTieAuthUrl ?? false;
		EnvironmentType environmentType2 = EnvironmentType.Sandbox;
		if (!string.IsNullOrWhiteSpace(environmentType) && Enum.TryParse<EnvironmentType>(environmentType, ignoreCase: true, out var result))
		{
			environmentType2 = result;
		}
		EntraIdLoginParameters parameters = new EntraIdLoginParameters
		{
			Tenant = (string.IsNullOrWhiteSpace(tenant) ? null : tenant),
			Environment = publishEnvironment,
			EnvironmentType = environmentType2,
			EnvironmentName = (string.IsNullOrWhiteSpace(environmentName) ? null : environmentName),
			ApplicationFamily = (string.IsNullOrWhiteSpace(applicationFamily) ? null : applicationFamily),
			UsernameHint = (string.IsNullOrWhiteSpace(usernameHint) ? null : usernameHint),
			UseInteractiveLogin = true,
			AllowDeviceCodeFallback = false,
			UseModernTieAuthUrl = useModernTieAuthUrl,
			NoCache = noCache
		};
		requiredService.Info("Authenticating (interactive browser)...");
		try
		{
			EntraIdLoginResult entraIdLoginResult = await EntraIdAuthService.LoginAsync(requiredService, parameters, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
			if (entraIdLoginResult.Success)
			{
				authContext?.SetContext(publishEnvironment, parameters.Tenant);
				return new AuthResponse
				{
					Success = true,
					AuthRequired = false,
					Message = "Authenticated. Tokens cached for subsequent operations."
				};
			}
			return new AuthResponse
			{
				Success = false,
				AuthRequired = false,
				Message = (entraIdLoginResult.Error ?? "Authentication failed.")
			};
		}
		catch (UserNotAuthenticatedException)
		{
			return new AuthResponse
			{
				Success = false,
				AuthRequired = true,
				Message = "Authentication requires user interaction outside MCP. Run 'altool auth login' in a terminal and retry."
			};
		}
	}
}
Latest version is '10.0.0.8330' (yours is '8.2.0.7535-95108c96')
