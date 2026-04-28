using System;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.Deployment.ApiClients;

internal static class McpClientFeaturesExtensions
{
	public static Version RequiredVersion(this McpClientFeatures feature)
	{
		if ((uint)feature <= 1u)
		{
			return McpClientApiVersions.Version1_0;
		}
		throw ExceptionUtilities.UnexpectedValue(feature);
	}
}
