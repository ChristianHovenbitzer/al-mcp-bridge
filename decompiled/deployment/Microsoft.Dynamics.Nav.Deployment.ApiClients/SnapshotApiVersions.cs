using System;
using System.Collections.Generic;

namespace Microsoft.Dynamics.Nav.Deployment.ApiClients;

internal class SnapshotApiVersions
{
	public static readonly Version Version1_0 = new Version(1, 0);

	public static readonly Version Version2_0 = new Version(2, 0);

	public static readonly Version Version3_0 = new Version(3, 0);

	public static readonly IReadOnlyList<Version> All = new Version[3] { Version1_0, Version2_0, Version3_0 };
}
