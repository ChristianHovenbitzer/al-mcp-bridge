using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.Dynamics.Nav.CodeAnalysis;

namespace Microsoft.Dynamics.Nav.Deployment.ApiClients;

internal class SnapshotExtensionInfo : ExtensionInfo
{
	public IReadOnlyList<Version> SupportedSnapshotWebApiVersions { get; }

	public static SnapshotExtensionInfo Instance { get; } = new SnapshotExtensionInfo(RuntimeVersion.SupportedVersions.OrderBy((Version x) => x).ToList(), SnapshotApiVersions.All);


	public SnapshotExtensionInfo(IReadOnlyList<Version> supportedRuntimeVersions, IReadOnlyList<Version> supportedSnapshotWebApiVersions)
		: base(supportedRuntimeVersions)
	{
		SupportedSnapshotWebApiVersions = supportedSnapshotWebApiVersions;
	}

	public override bool AssertSupports(ServerInfo serverInfo)
	{
		if (serverInfo == null)
		{
			return false;
		}
		if (serverInfo.Kind != ServerInfoKind.Snapshot)
		{
			return false;
		}
		AssertVersionSupported(SupportedSnapshotWebApiVersions, serverInfo.WebApiVersion, serverInfo, ignoreMinors: false);
		AssertVersionSupported(base.SupportedRuntimeVersions, serverInfo.RuntimeVersion, serverInfo, ignoreMinors: true);
		return true;
	}

	public override string ToString()
	{
		return string.Format(CultureInfo.InvariantCulture, DeploymentResources.SnapshotExtensionInfoFormat, ExtensionInfo.VersionsToString(base.SupportedRuntimeVersions), ExtensionInfo.VersionsToString(SupportedSnapshotWebApiVersions));
	}
}
