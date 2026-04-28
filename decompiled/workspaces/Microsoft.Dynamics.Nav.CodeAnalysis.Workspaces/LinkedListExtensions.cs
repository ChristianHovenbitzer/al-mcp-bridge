using System.Collections.Generic;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces;

internal static class LinkedListExtensions
{
	public static void AddRangeAtHead<T>(this LinkedList<T> list, IEnumerable<T> values)
	{
		LinkedListNode<T> linkedListNode = null;
		foreach (T value in values)
		{
			linkedListNode = ((linkedListNode == null) ? list.AddFirst(value) : list.AddAfter(linkedListNode, value));
		}
	}
}
