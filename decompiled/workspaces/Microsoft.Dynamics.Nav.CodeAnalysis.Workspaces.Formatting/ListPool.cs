using System.Collections.Generic;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Formatting;

internal static class ListPool<T>
{
	public static List<T> Allocate()
	{
		return SharedPools.Default<List<T>>().AllocateAndClear();
	}

	public static void Free(List<T> list)
	{
		SharedPools.Default<List<T>>().ClearAndFree(list);
	}

	public static List<T> ReturnAndFree(List<T> list)
	{
		return list;
	}
}
