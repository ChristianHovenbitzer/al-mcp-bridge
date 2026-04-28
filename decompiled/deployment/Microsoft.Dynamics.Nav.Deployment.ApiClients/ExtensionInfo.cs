using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Dynamics.Nav.Deployment.Telemetry;

namespace Microsoft.Dynamics.Nav.Deployment.ApiClients;

internal abstract class ExtensionInfo
{
	public IReadOnlyList<Version> SupportedRuntimeVersions { get; }

	public ExtensionInfo(IReadOnlyList<Version> supportedRuntimeVersions)
	{
		SupportedRuntimeVersions = supportedRuntimeVersions;
	}

	public abstract bool AssertSupports(ServerInfo serverInfo);

	protected static string VersionsToString(IReadOnlyList<Version> versions)
	{
		return string.Join(", ", versions);
	}

	protected void AssertVersionSupported(IReadOnlyList<Version> clientVersions, Version? serverVersion, ServerInfo serverInfo, bool ignoreMinors)
	{
		Version serverVersion2 = serverVersion;
		if (serverVersion2 == null || clientVersions.First().Major > serverVersion2.Major)
		{
			throw new ExtensionTooNewException(this, serverInfo);
		}
		if (clientVersions.Last().Major < serverVersion2.Major)
		{
			throw new ExtensionTooOldException(this, serverInfo);
		}
		List<Version> list = (from x in clientVersions
			where x.Major == serverVersion2.Major
			orderby x
			select x).ToList();
		if (list.Count == 0)
		{
			return;
		}
		if (list.First().Minor > serverVersion2.Minor)
		{
			if (!ignoreMinors)
			{
				throw new ExtensionTooNewException(this, serverInfo);
			}
			TelemetryServiceManager.CurrentTelemetryService.TrackInfo($"VSCode extension supported runtime versions do not have an exact match for the server's runtime version. Server version: {serverVersion2}");
		}
		if (list.Last().Minor < serverVersion2.Minor)
		{
			if (!ignoreMinors)
			{
				throw new ExtensionTooOldException(this, serverInfo);
			}
			TelemetryServiceManager.CurrentTelemetryService.TrackInfo($"VSCode extension supported runtime versions do not have an exact match for the server's runtime version. Server version: {serverVersion2}");
		}
	}
}
