using System.Text;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Formatting;

internal static class StringBuilderPool
{
	public static StringBuilder Allocate()
	{
		return SharedPools.Default<StringBuilder>().AllocateAndClear();
	}

	public static void Free(StringBuilder builder)
	{
		SharedPools.Default<StringBuilder>().ClearAndFree(builder);
	}

	public static string ReturnAndFree(StringBuilder builder)
	{
		return builder.ToString();
	}
}
