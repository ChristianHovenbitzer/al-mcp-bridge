namespace Microsoft.Dynamics.Nav.Deployment;

public class EntraIdAuthenticationDetails
{
	public string? Endpoint { get; set; }

	public string? ClientId { get; set; }

	public string? RedirectUri { get; set; }

	public string? Scope { get; set; }

	public override string ToString()
	{
		return $"{Endpoint}/{ClientId}/{RedirectUri}/{Scope}";
	}
}
