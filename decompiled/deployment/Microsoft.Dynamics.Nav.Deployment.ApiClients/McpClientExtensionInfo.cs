using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.Dynamics.Nav.CodeAnalysis;

namespace Microsoft.Dynamics.Nav.Deployment.ApiClients;

internal class McpClientExtensionInfo : ExtensionInfo
{
	public IReadOnlyList<Version> SupportedMcpClientVersions { get; }

	public static McpClientExtensionInfo Instance { get; } = new McpClientExtensionInfo(RuntimeVersion.SupportedVersions.OrderBy((Version x) => x).ToList(), McpClientApiVersions.All);


	public McpClientExtensionInfo(IReadOnlyList<Version> supportedRuntimeVersions, IReadOnlyList<Version> supportedMcpClientVersions)
		: base(supportedRuntimeVersions)
	{
		SupportedMcpClientVersions = supportedMcpClientVersions;
	}

	public override bool AssertSupports(ServerInfo serverInfo)
	{
		if (serverInfo == null)
		{
			return false;
		}
		if (serverInfo.Kind != ServerInfoKind.Mcp)
		{
			return false;
		}
		return true;
	}

	public override string ToString()
	{
		return string.Format(CultureInfo.InvariantCulture, DeploymentResources.McpExtensionInfoFormat, ExtensionInfo.VersionsToString(base.SupportedRuntimeVersions), ExtensionInfo.VersionsToString(SupportedMcpClientVersions));
	}
}
