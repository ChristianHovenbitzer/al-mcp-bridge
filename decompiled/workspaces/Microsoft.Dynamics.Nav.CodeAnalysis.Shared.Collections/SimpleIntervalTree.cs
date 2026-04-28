using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Shared.Collections;

internal class SimpleIntervalTree
{
	public static SimpleIntervalTree<T> Create<T>(IIntervalIntrospector<T> introspector, params T[] values)
	{
		return Create(introspector, (IEnumerable<T>)values);
	}

	public static SimpleIntervalTree<T> Create<T>(IIntervalIntrospector<T> introspector, IEnumerable<T> values = null)
	{
		return new SimpleIntervalTree<T>(introspector, values);
	}
}
internal class SimpleIntervalTree<T> : IntervalTree<T>
{
	private readonly IIntervalIntrospector<T> introspector;

	protected IIntervalIntrospector<T> Introspector => introspector;

	public SimpleIntervalTree(IIntervalIntrospector<T> introspector, IEnumerable<T> values)
	{
		this.introspector = introspector;
		if (values == null)
		{
			return;
		}
		foreach (T value in values)
		{
			root = IntervalTree<T>.Insert(root, new IntervalTree<T>.Node(value), introspector);
		}
	}

	public void AddIntervalInPlace(T value)
	{
		IntervalTree<T>.Node newNode = new IntervalTree<T>.Node(value);
		root = IntervalTree<T>.Insert(root, newNode, Introspector);
	}

	public ImmutableArray<T> GetIntervalsThatOverlapWith(int start, int length)
	{
		return GetIntervalsThatOverlapWith(start, length, introspector);
	}

	public ImmutableArray<T> GetIntervalsThatIntersectWith(int start, int length)
	{
		return GetIntervalsThatIntersectWith(start, length, introspector);
	}

	public ImmutableArray<T> GetIntervalsThatContain(int start, int length)
	{
		return GetIntervalsThatContain(start, length, introspector);
	}

	public void FillWithIntervalsThatOverlapWith(int start, int length, ArrayBuilder<T> builder)
	{
		FillWithIntervalsThatOverlapWith(start, length, builder, introspector);
	}

	public void FillWithIntervalsThatIntersectWith(int start, int length, ArrayBuilder<T> builder)
	{
		FillWithIntervalsThatIntersectWith(start, length, builder, introspector);
	}

	public void FillWithIntervalsThatContain(int start, int length, ArrayBuilder<T> builder)
	{
		FillWithIntervalsThatContain(start, length, builder, introspector);
	}

	public bool HasIntervalThatIntersectsWith(int position)
	{
		return HasIntervalThatIntersectsWith(position, introspector);
	}

	public bool HasIntervalThatOverlapsWith(int start, int length)
	{
		return HasIntervalThatOverlapsWith(start, length, introspector);
	}

	public bool HasIntervalThatIntersectsWith(int start, int length)
	{
		return HasIntervalThatIntersectsWith(start, length, introspector);
	}

	public bool HasIntervalThatContains(int start, int length)
	{
		return HasIntervalThatContains(start, length, introspector);
	}

	protected int MaxEndValue(Node node)
	{
		return IntervalTree<T>.GetEnd(node.MaxEndNode.Value, introspector);
	}
}
