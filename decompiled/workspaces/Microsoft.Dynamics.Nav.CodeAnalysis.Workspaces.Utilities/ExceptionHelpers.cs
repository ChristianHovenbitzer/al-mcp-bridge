using System;
using System.Runtime.InteropServices;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Utilities;

internal static class ExceptionHelpers
{
	[StructLayout(LayoutKind.Sequential, Size = 1)]
	public struct FailFastReset : IDisposable
	{
		public void Dispose()
		{
		}
	}

	public static FailFastReset SuppressFailFast()
	{
		return default(FailFastReset);
	}

	public static bool IsFailFastSuppressed()
	{
		return false;
	}
}
