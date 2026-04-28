namespace Microsoft.Dynamics.Nav.Deployment.Authentication;

internal class TenantToken
{
	public string TenantId { get; }

	public string AccessToken { get; }

	public string? UserPrincipalName { get; }

	public TenantToken(string tenantId, string accessToken, string? userPrincipalName)
	{
		TenantId = tenantId;
		AccessToken = accessToken;
		UserPrincipalName = userPrincipalName;
	}
}
