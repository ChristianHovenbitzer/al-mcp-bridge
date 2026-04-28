using System;
using System.Collections.Generic;

namespace Microsoft.Dynamics.Nav.Deployment.ApiClients;

internal class McpClientApiVersions
{
	public static readonly Version Version1_0 = new Version(1, 0);

	public static readonly IReadOnlyList<Version> All = new Version[1] { Version1_0 };
}
