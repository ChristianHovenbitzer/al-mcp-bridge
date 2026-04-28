using System.Net.Http.Headers;

namespace Microsoft.Dynamics.Nav.Deployment.Publishing;

internal sealed class ClientConnectionInfo
{
	public string TenantId { get; }

	public AuthenticationHeaderValue AuthenticationHeader { get; }

	public ClientConnectionInfo(string tenantId = null, AuthenticationHeaderValue authenticationHeader = null)
	{
		TenantId = tenantId;
		AuthenticationHeader = authenticationHeader;
	}
}
