using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.Dynamics.Nav.CodeAnalysis;

namespace Microsoft.Dynamics.Nav.Deployment.ApiClients;

internal class DevExtensionInfo : ExtensionInfo
{
	public IReadOnlyList<Version> SupportedWebApiVersions { get; }

	public IReadOnlyList<Version> SupportedDebuggerVersions { get; }

	public static DevExtensionInfo Instance { get; } = new DevExtensionInfo(DevApiVersions.All, DebuggerVersions.All, RuntimeVersion.SupportedVersions.OrderBy((Version x) => x).ToList());


	public DevExtensionInfo(IReadOnlyList<Version> supportedWebApiVersions, IReadOnlyList<Version> supportedDebuggerVersions, IReadOnlyList<Version> supportedRuntimeVersions)
		: base(supportedRuntimeVersions)
	{
		SupportedWebApiVersions = supportedWebApiVersions;
		SupportedDebuggerVersions = supportedDebuggerVersions;
	}

	public override bool AssertSupports(ServerInfo serverInfo)
	{
		if (serverInfo == null)
		{
			return false;
		}
		if (serverInfo.Kind != 0)
		{
			return false;
		}
		AssertVersionSupported(SupportedWebApiVersions, serverInfo.WebApiVersion, serverInfo, ignoreMinors: false);
		AssertVersionSupported(SupportedDebuggerVersions, serverInfo.DebuggerVersion, serverInfo, ignoreMinors: false);
		AssertVersionSupported(base.SupportedRuntimeVersions, serverInfo.RuntimeVersion, serverInfo, ignoreMinors: true);
		return true;
	}

	public override string ToString()
	{
		return string.Format(CultureInfo.InvariantCulture, DeploymentResources.DevExtensionInfoFormat, ExtensionInfo.VersionsToString(base.SupportedRuntimeVersions), ExtensionInfo.VersionsToString(SupportedWebApiVersions), ExtensionInfo.VersionsToString(SupportedDebuggerVersions));
	}
}
