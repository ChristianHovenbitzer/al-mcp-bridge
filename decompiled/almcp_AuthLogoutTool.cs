using System;
using System.ComponentModel;
using Microsoft.Dynamics.BusinessCentral.ALMcp.Services;
using Microsoft.Dynamics.Nav.Deployment.Authentication;
using Microsoft.Extensions.DependencyInjection;
using ModelContextProtocol.Server;

namespace Microsoft.Dynamics.BusinessCentral.ALMcp.Tools;

[McpServerToolType]
public class AuthLogoutTool
{
	private const string ToolName = "al_auth_logout";

	[McpServerTool(Name = "al_auth_logout")]
	[Description("Clears cached Entra ID (Azure AD) authentication tokens. Parameters: None. Returns: Confirmation that token cache was cleared. Use cases: Switch to a different account, resolve authentication errors from expired/invalid tokens, sign out for security. Next steps: Run al_auth_login to authenticate with a different account.")]
	public static AuthResponse Logout(McpServer server)
	{
		if (server == null)
		{
			throw new ArgumentNullException("server");
		}
		McpTelemetryService.TrackToolInvocation("al_auth_logout");
		McpEmitLogger requiredService = server.Services.GetRequiredService<McpEmitLogger>();
		try
		{
			EntraIdAuthService.Logout(requiredService);
		}
		catch (Exception ex)
		{
			return new AuthResponse
			{
				Success = false,
				Message = ex.Message
			};
		}
		(server.Services?.GetService<AuthContextService>())?.ClearContext();
		return new AuthResponse
		{
			Success = true,
			AuthRequired = false,
			Message = "Token cache cleared."
		};
	}
}
Latest version is '10.0.0.8330' (yours is '8.2.0.7535-95108c96')
