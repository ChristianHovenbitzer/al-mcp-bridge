using System;
using System.Collections.Generic;

namespace Microsoft.Dynamics.Nav.Deployment.ApiClients;

internal class DebuggerVersions
{
	public static readonly Version Version1_0 = new Version(1, 0);

	public static readonly Version Version2_0 = new Version(2, 0);

	public static readonly Version Version3_0 = new Version(3, 0);

	public static readonly Version Version4_0 = new Version(4, 0);

	public static readonly Version Version5_0 = new Version(5, 0);

	public static readonly Version Version6_0 = new Version(6, 0);

	public static readonly Version Version7_0 = new Version(7, 0);

	public static readonly IReadOnlyList<Version> All = new Version[7] { Version1_0, Version2_0, Version3_0, Version4_0, Version5_0, Version6_0, Version7_0 };
}
