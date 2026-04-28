using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Shared.Collections;

internal static class IntervalTree
{
	public static IntervalTree<T> Create<T>(IIntervalIntrospector<T> introspector, params T[] values)
	{
		return Create(introspector, (IEnumerable<T>)values);
	}

	public static IntervalTree<T> Create<T>(IIntervalIntrospector<T> introspector, IEnumerable<T> values = null)
	{
		Contract.ThrowIfNull(introspector);
		return new IntervalTree<T>(introspector, values ?? SpecializedCollections.EmptyEnumerable<T>());
	}
}
internal class IntervalTree<T> : IEnumerable<T>, IEnumerable
{
	private delegate bool TestInterval(T value, int start, int length, IIntervalIntrospector<T> introspector);

	protected class Node
	{
		internal T Value { get; }

		internal Node Left { get; private set; }

		internal Node Right { get; private set; }

		internal int Height { get; private set; }

		internal Node MaxEndNode { get; private set; }

		internal Node(T value)
		{
			Value = value;
			Height = 1;
			MaxEndNode = this;
		}

		internal Node(IIntervalIntrospector<T> introspector, T value, Node left, Node right)
		{
			Value = value;
			SetLeftRight(left, right, introspector);
		}

		internal void SetLeftRight(Node left, Node right, IIntervalIntrospector<T> introspector)
		{
			Left = left;
			Right = right;
			Height = 1 + Math.Max(IntervalTree<T>.Height(left), IntervalTree<T>.Height(right));
			int end = IntervalTree<T>.GetEnd(Value, introspector);
			int num = IntervalTree<T>.MaxEndValue(left, introspector);
			int num2 = IntervalTree<T>.MaxEndValue(right, introspector);
			if (end >= num && end >= num2)
			{
				MaxEndNode = this;
			}
			else if (num >= num2 && left != null)
			{
				MaxEndNode = left.MaxEndNode;
			}
			else if (right != null)
			{
				MaxEndNode = right.MaxEndNode;
			}
			else
			{
				Contract.Fail("We have no MaxEndNode? Huh?");
			}
		}

		internal Node RightRotation(IIntervalIntrospector<T> introspector)
		{
			Node left = Left;
			SetLeftRight(Left.Right, Right, introspector);
			left.SetLeftRight(left.Left, this, introspector);
			return left;
		}

		internal Node LeftRotation(IIntervalIntrospector<T> introspector)
		{
			Node right = Right;
			SetLeftRight(Left, Right.Left, introspector);
			right.SetLeftRight(this, right.Right, introspector);
			return right;
		}

		internal Node InnerRightOuterLeftRotation(IIntervalIntrospector<T> introspector)
		{
			Node left = Right.Left;
			Node right = Right;
			SetLeftRight(Left, Right.Left.Left, introspector);
			right.SetLeftRight(right.Left.Right, right.Right, introspector);
			left.SetLeftRight(this, right, introspector);
			return left;
		}

		internal Node InnerLeftOuterRightRotation(IIntervalIntrospector<T> introspector)
		{
			Node right = Left.Right;
			Node left = Left;
			SetLeftRight(Left.Right.Right, Right, introspector);
			left.SetLeftRight(left.Left, left.Right.Left, introspector);
			right.SetLeftRight(left, this, introspector);
			return right;
		}
	}

	public static readonly IntervalTree<T> Empty = new IntervalTree<T>();

	protected Node root;

	private static readonly TestInterval s_intersectsWithTest = IntersectsWith;

	private static readonly TestInterval s_containsTest = Contains;

	private static readonly TestInterval s_overlapsWithTest = OverlapsWith;

	private static readonly ObjectPool<Stack<(Node, bool)>> s_stackPool = new ObjectPool<Stack<(Node, bool)>>(() => new Stack<(Node, bool)>());

	public IntervalTree()
	{
	}

	public IntervalTree(IIntervalIntrospector<T> introspector, IEnumerable<T> values)
	{
		foreach (T value in values)
		{
			root = Insert(root, new Node(value), introspector);
		}
	}

	protected static bool Contains(T value, int start, int length, IIntervalIntrospector<T> introspector)
	{
		int num = start + length;
		int end = GetEnd(value, introspector);
		int start2 = introspector.GetStart(value);
		if (length == 0)
		{
			if (start2 <= start)
			{
				return num < end;
			}
			return false;
		}
		if (start2 <= start)
		{
			return num <= end;
		}
		return false;
	}

	private static bool IntersectsWith(T value, int start, int length, IIntervalIntrospector<T> introspector)
	{
		int num = start + length;
		int end = GetEnd(value, introspector);
		int start2 = introspector.GetStart(value);
		if (start <= end)
		{
			return num >= start2;
		}
		return false;
	}

	private static bool OverlapsWith(T value, int start, int length, IIntervalIntrospector<T> introspector)
	{
		int val = start + length;
		int end = GetEnd(value, introspector);
		int start2 = introspector.GetStart(value);
		if (length == 0)
		{
			if (start2 < start)
			{
				return start < end;
			}
			return false;
		}
		int num = Math.Max(start2, start);
		int num2 = Math.Min(end, val);
		return num < num2;
	}

	public ImmutableArray<T> GetIntervalsThatOverlapWith(int start, int length, IIntervalIntrospector<T> introspector)
	{
		return GetIntervalsThatMatch(start, length, s_overlapsWithTest, introspector);
	}

	public ImmutableArray<T> GetIntervalsThatIntersectWith(int start, int length, IIntervalIntrospector<T> introspector)
	{
		return GetIntervalsThatMatch(start, length, s_intersectsWithTest, introspector);
	}

	public ImmutableArray<T> GetIntervalsThatContain(int start, int length, IIntervalIntrospector<T> introspector)
	{
		return GetIntervalsThatMatch(start, length, s_containsTest, introspector);
	}

	public void FillWithIntervalsThatOverlapWith(int start, int length, ArrayBuilder<T> builder, IIntervalIntrospector<T> introspector)
	{
		FillWithIntervalsThatMatch(start, length, s_overlapsWithTest, builder, introspector, stopAfterFirst: false);
	}

	public void FillWithIntervalsThatIntersectWith(int start, int length, ArrayBuilder<T> builder, IIntervalIntrospector<T> introspector)
	{
		FillWithIntervalsThatMatch(start, length, s_intersectsWithTest, builder, introspector, stopAfterFirst: false);
	}

	public void FillWithIntervalsThatContain(int start, int length, ArrayBuilder<T> builder, IIntervalIntrospector<T> introspector)
	{
		FillWithIntervalsThatMatch(start, length, s_containsTest, builder, introspector, stopAfterFirst: false);
	}

	public bool HasIntervalThatIntersectsWith(int position, IIntervalIntrospector<T> introspector)
	{
		return HasIntervalThatIntersectsWith(position, 0, introspector);
	}

	public bool HasIntervalThatIntersectsWith(int start, int length, IIntervalIntrospector<T> introspector)
	{
		return Any(start, length, s_intersectsWithTest, introspector);
	}

	public bool HasIntervalThatOverlapsWith(int start, int length, IIntervalIntrospector<T> introspector)
	{
		return Any(start, length, s_overlapsWithTest, introspector);
	}

	public bool HasIntervalThatContains(int start, int length, IIntervalIntrospector<T> introspector)
	{
		return Any(start, length, s_containsTest, introspector);
	}

	private bool Any(int start, int length, TestInterval testInterval, IIntervalIntrospector<T> introspector)
	{
		ArrayBuilder<T> instance = ArrayBuilder<T>.GetInstance();
		FillWithIntervalsThatMatch(start, length, testInterval, instance, introspector, stopAfterFirst: true);
		bool result = instance.Count > 0;
		instance.Free();
		return result;
	}

	private ImmutableArray<T> GetIntervalsThatMatch(int start, int length, TestInterval testInterval, IIntervalIntrospector<T> introspector)
	{
		ArrayBuilder<T> instance = ArrayBuilder<T>.GetInstance();
		FillWithIntervalsThatMatch(start, length, testInterval, instance, introspector, stopAfterFirst: false);
		return instance.ToImmutableAndFree();
	}

	private void FillWithIntervalsThatMatch(int start, int length, TestInterval testInterval, ArrayBuilder<T> builder, IIntervalIntrospector<T> introspector, bool stopAfterFirst)
	{
		if (root != null)
		{
			Stack<(Node, bool)> stack = s_stackPool.Allocate();
			FillWithIntervalsThatMatch(start, length, testInterval, builder, introspector, stopAfterFirst, stack);
			s_stackPool.ClearAndFree<(Node, bool)>(stack);
		}
	}

	private void FillWithIntervalsThatMatch(int start, int length, TestInterval testInterval, ArrayBuilder<T> builder, IIntervalIntrospector<T> introspector, bool stopAfterFirst, Stack<(Node, bool)> candidates)
	{
		int num = start + length;
		candidates.Push(ValueTuple.Create(root, item2: true));
		while (candidates.Count > 0)
		{
			(Node, bool) tuple = candidates.Pop();
			var (node, _) = tuple;
			if (!tuple.Item2)
			{
				if (testInterval(node.Value, start, length, introspector))
				{
					builder.Add(node.Value);
					if (stopAfterFirst)
					{
						break;
					}
				}
				continue;
			}
			if (introspector.GetStart(node.Value) <= num)
			{
				Node right = node.Right;
				if (right != null && GetEnd(right.MaxEndNode.Value, introspector) >= start)
				{
					candidates.Push(ValueTuple.Create(right, item2: true));
				}
			}
			candidates.Push(ValueTuple.Create(node, item2: false));
			Node left = node.Left;
			if (left != null && GetEnd(left.MaxEndNode.Value, introspector) >= start)
			{
				candidates.Push(ValueTuple.Create(left, item2: true));
			}
		}
	}

	public bool IsEmpty()
	{
		return root == null;
	}

	protected static Node Insert(Node root, Node newNode, IIntervalIntrospector<T> introspector)
	{
		int start = introspector.GetStart(newNode.Value);
		return Insert(root, newNode, start, introspector);
	}

	private static Node Insert(Node root, Node newNode, int newNodeStart, IIntervalIntrospector<T> introspector)
	{
		if (root == null)
		{
			return newNode;
		}
		Node left;
		Node right;
		if (newNodeStart < introspector.GetStart(root.Value))
		{
			left = Insert(root.Left, newNode, newNodeStart, introspector);
			right = root.Right;
		}
		else
		{
			left = root.Left;
			right = Insert(root.Right, newNode, newNodeStart, introspector);
		}
		root.SetLeftRight(left, right, introspector);
		return Balance(root, introspector);
	}

	private static Node Balance(Node node, IIntervalIntrospector<T> introspector)
	{
		switch (BalanceFactor(node))
		{
		case -2:
			if (BalanceFactor(node.Right) == -1)
			{
				return node.LeftRotation(introspector);
			}
			return node.InnerRightOuterLeftRotation(introspector);
		case 2:
			if (BalanceFactor(node.Left) == 1)
			{
				return node.RightRotation(introspector);
			}
			return node.InnerLeftOuterRightRotation(introspector);
		default:
			return node;
		}
	}

	public IEnumerator<T> GetEnumerator()
	{
		if (root == null)
		{
			yield break;
		}
		Stack<(Node, bool)> candidates = new Stack<(Node, bool)>();
		candidates.Push(ValueTuple.Create(root, item2: true));
		while (candidates.Count != 0)
		{
			(Node, bool) tuple = candidates.Pop();
			var (node, _) = tuple;
			if (node != null)
			{
				if (tuple.Item2)
				{
					candidates.Push(ValueTuple.Create(node.Right, item2: true));
					candidates.Push(ValueTuple.Create(node, item2: false));
					candidates.Push(ValueTuple.Create(node.Left, item2: true));
				}
				else
				{
					yield return node.Value;
				}
			}
		}
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	protected static int GetEnd(T value, IIntervalIntrospector<T> introspector)
	{
		return introspector.GetStart(value) + introspector.GetLength(value);
	}

	protected static int MaxEndValue(Node node, IIntervalIntrospector<T> arg)
	{
		if (node != null)
		{
			return GetEnd(node.MaxEndNode.Value, arg);
		}
		return 0;
	}

	private static int Height(Node node)
	{
		return node?.Height ?? 0;
	}

	private static int BalanceFactor(Node node)
	{
		if (node != null)
		{
			return Height(node.Left) - Height(node.Right);
		}
		return 0;
	}
}
