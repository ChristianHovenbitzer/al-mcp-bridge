using System;
using System.Threading;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Utilities;

internal static class SemaphoreSlimFactory
{
	public static readonly Func<SemaphoreSlim> Instance = () => new SemaphoreSlim(1);
}
