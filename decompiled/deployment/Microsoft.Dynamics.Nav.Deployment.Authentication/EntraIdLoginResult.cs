namespace Microsoft.Dynamics.Nav.Deployment.Authentication;

public sealed class EntraIdLoginResult
{
	public bool Success { get; set; }

	public bool UsedDeviceCode { get; set; }

	public string? TenantId { get; set; }

	public string? UserPrincipalName { get; set; }

	public string? Error { get; set; }
}
