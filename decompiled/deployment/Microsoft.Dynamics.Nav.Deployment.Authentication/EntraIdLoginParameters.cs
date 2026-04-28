namespace Microsoft.Dynamics.Nav.Deployment.Authentication;

public sealed class EntraIdLoginParameters
{
	public PublishEnvironment Environment { get; set; }

	public EnvironmentType EnvironmentType { get; set; } = EnvironmentType.Sandbox;


	public string? EnvironmentName { get; set; }

	public string? Tenant { get; set; }

	public string? PrimaryTenantDomain { get; set; }

	public string? ApplicationFamily { get; set; }

	public EntraIdAuthenticationDetails? EntraIdAuthentication { get; set; }

	public string? UsernameHint { get; set; }

	public bool UseInteractiveLogin { get; set; } = true;


	public bool UseModernTieAuthUrl { get; set; }

	public bool AllowDeviceCodeFallback { get; set; } = true;


	public bool NoCache { get; set; }
}
