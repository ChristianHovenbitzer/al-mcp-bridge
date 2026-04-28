using System;
using System.Collections.Generic;
using Microsoft.Dynamics.Nav.CodeAnalysis.Shared.Collections;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Formatting;

internal class ContextIntervalTree<T> : SimpleIntervalTree<T>
{
	private readonly Func<T, int, int, bool> containPredicate;

	private readonly Func<T, int, int, bool> edgeExclusivePredicate;

	private readonly Func<T, int, int, bool> edgeInclusivePredicate;

	public ContextIntervalTree(IIntervalIntrospector<T> introspector)
		: base(introspector, (IEnumerable<T>)null)
	{
		edgeExclusivePredicate = ContainsEdgeExclusive;
		edgeInclusivePredicate = ContainsEdgeInclusive;
		containPredicate = (T value, int start, int end) => IntervalTree<T>.Contains(value, start, end, base.Introspector);
	}

	public T GetSmallestEdgeExclusivelyContainingInterval(int start, int length)
	{
		return GetSmallestContainingIntervalWorker(start, length, edgeExclusivePredicate);
	}

	public T GetSmallestEdgeInclusivelyContainingInterval(int start, int length)
	{
		return GetSmallestContainingIntervalWorker(start, length, edgeInclusivePredicate);
	}

	public T GetSmallestContainingInterval(int start, int length)
	{
		return GetSmallestContainingIntervalWorker(start, length, containPredicate);
	}

	private bool ContainsEdgeExclusive(T value, int start, int length)
	{
		int num = start + length;
		int end = IntervalTree<T>.GetEnd(value, base.Introspector);
		if (base.Introspector.GetStart(value) < start)
		{
			return num < end;
		}
		return false;
	}

	private bool ContainsEdgeInclusive(T value, int start, int length)
	{
		int num = start + length;
		int end = IntervalTree<T>.GetEnd(value, base.Introspector);
		if (base.Introspector.GetStart(value) <= start)
		{
			return num <= end;
		}
		return false;
	}

	private T GetSmallestContainingIntervalWorker(int start, int length, Func<T, int, int, bool> predicate)
	{
		T val = default(T);
		if (root == null || MaxEndValue(root) < start)
		{
			return val;
		}
		int num = start + length;
		using PooledObject<Stack<IntervalTree<T>.Node>> pooledObject = SharedPools.Default<Stack<IntervalTree<T>.Node>>().GetPooledObject();
		Stack<IntervalTree<T>.Node> @object = pooledObject.Object;
		@object.Push(root);
		while (@object.Count > 0)
		{
			IntervalTree<T>.Node node = @object.Peek();
			if (base.Introspector.GetStart(node.Value) <= start)
			{
				IntervalTree<T>.Node right = node.Right;
				if (right != null && num < MaxEndValue(right))
				{
					@object.Push(right);
					continue;
				}
			}
			IntervalTree<T>.Node left = node.Left;
			if (left != null && num <= MaxEndValue(left))
			{
				@object.Push(left);
				continue;
			}
			while (@object.Count > 0)
			{
				node = @object.Pop();
				if (predicate(node.Value, start, length) && (EqualityComparer<T>.Default.Equals(val, default(T)) || (base.Introspector.GetStart(val) <= base.Introspector.GetStart(node.Value) && base.Introspector.GetLength(node.Value) < base.Introspector.GetLength(val))))
				{
					val = node.Value;
				}
				if (@object.Count == 0)
				{
					return val;
				}
				IntervalTree<T>.Node node2 = @object.Peek();
				if (node2.Left != node && node2.Right == node && node2.Left != null && num <= MaxEndValue(node2.Left) && (EqualityComparer<T>.Default.Equals(val, default(T)) || base.Introspector.GetStart(node2.Value) == base.Introspector.GetStart(node.Value)))
				{
					@object.Push(node2.Left);
					break;
				}
			}
		}
		return val;
	}
}
