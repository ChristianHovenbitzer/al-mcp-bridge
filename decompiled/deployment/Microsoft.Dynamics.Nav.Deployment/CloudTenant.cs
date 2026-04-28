using System;
using System.Globalization;
using System.Text;

namespace Microsoft.Dynamics.Nav.Deployment;

internal class CloudTenant
{
	private const string FixedEndpointFormat = "https://businesscentral.dynamics{0}.com";

	private const string ApplicationFamilyFixedEndpointFormat = "https://{0}.bc.dynamics{1}.com";

	internal static Uri FindFixedWebClientUri(PublishEnvironment env, string? tenantDomain = null, string? applicationFamily = null, string? environmentName = null, string? deploymentId = null)
	{
		StringBuilder stringBuilder = new StringBuilder();
		if (string.IsNullOrEmpty(applicationFamily))
		{
			stringBuilder.Append(string.Format(CultureInfo.InvariantCulture, "https://businesscentral.dynamics{0}.com", env.DeploymentSuffix()));
		}
		else
		{
			stringBuilder.Append(string.Format(CultureInfo.InvariantCulture, "https://{0}.bc.dynamics{1}.com", applicationFamily, env.DeploymentSuffix()));
		}
		if (!string.IsNullOrEmpty(tenantDomain))
		{
			stringBuilder.Append("/");
			stringBuilder.Append(Uri.EscapeDataString(tenantDomain));
		}
		if (!string.IsNullOrEmpty(environmentName))
		{
			stringBuilder.Append("/");
			stringBuilder.Append(environmentName);
		}
		if (!string.IsNullOrEmpty(deploymentId))
		{
			StringBuilder stringBuilder2 = stringBuilder;
			StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(14, 1, stringBuilder2);
			handler.AppendLiteral("?deploymentId=");
			handler.AppendFormatted(Uri.EscapeDataString(deploymentId));
			stringBuilder2.Append(ref handler);
		}
		return new Uri(stringBuilder.ToString());
	}
}
