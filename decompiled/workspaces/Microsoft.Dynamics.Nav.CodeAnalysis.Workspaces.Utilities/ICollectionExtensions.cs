using System;
using System.Collections.Generic;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Utilities;

internal static class ICollectionExtensions
{
	public static void AddRange<T>(this ICollection<T> collection, IEnumerable<T> values)
	{
		if (collection == null)
		{
			throw new ArgumentNullException("collection");
		}
		if (values == null)
		{
			return;
		}
		foreach (T value in values)
		{
			collection.Add(value);
		}
	}
}
