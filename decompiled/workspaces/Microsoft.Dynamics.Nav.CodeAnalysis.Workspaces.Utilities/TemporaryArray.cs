using System;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using System.Threading;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Utilities;

internal struct TemporaryArray<T> : IDisposable
{
	public struct Enumerator
	{
		private readonly TemporaryArray<T> array;

		private T current;

		private int nextIndex;

		public T Current => current;

		public Enumerator(in TemporaryArray<T> array)
		{
			this.array = new TemporaryArray<T>(in array);
			current = default(T);
			nextIndex = 0;
		}

		public bool MoveNext()
		{
			if (nextIndex >= array.Count)
			{
				return false;
			}
			current = array[nextIndex];
			nextIndex++;
			return true;
		}
	}

	internal static class TestAccessor
	{
		public static int InlineCapacity => 4;

		public static bool HasDynamicStorage(in TemporaryArray<T> array)
		{
			return array.builder != null;
		}

		public static int InlineCount(in TemporaryArray<T> array)
		{
			return array.count;
		}
	}

	private const int InlineCapacity = 4;

	private T item0;

	private T item1;

	private T item2;

	private T item3;

	private int count;

	private ArrayBuilder<T>? builder;

	public static TemporaryArray<T> Empty => default(TemporaryArray<T>);

	public readonly int Count => builder?.Count ?? count;

	public T this[int index]
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		readonly get
		{
			if (builder != null)
			{
				return builder[index];
			}
			if ((uint)index >= count)
			{
				ThrowIndexOutOfRangeException();
			}
			return index switch
			{
				0 => item0, 
				1 => item1, 
				2 => item2, 
				_ => item3, 
			};
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		set
		{
			if (builder != null)
			{
				builder[index] = value;
				return;
			}
			if ((uint)index >= count)
			{
				ThrowIndexOutOfRangeException();
			}
			switch (index)
			{
			case 0:
			{
				T val = (item0 = value);
				break;
			}
			case 1:
			{
				T val = (item1 = value);
				break;
			}
			case 2:
			{
				T val = (item2 = value);
				break;
			}
			default:
			{
				T val = (item3 = value);
				break;
			}
			}
		}
	}

	private TemporaryArray(in TemporaryArray<T> array)
	{
		this = array;
	}

	public void Dispose()
	{
		Interlocked.Exchange(ref builder, null)?.Free();
	}

	public void Add(T item)
	{
		if (builder != null)
		{
			builder.Add(item);
		}
		else if (count < 4)
		{
			count++;
			this[count - 1] = item;
		}
		else
		{
			MoveInlineToBuilder();
			builder.Add(item);
		}
	}

	public void AddRange(ImmutableArray<T> items)
	{
		if (builder != null)
		{
			builder.AddRange(items);
		}
		else if (count + items.Length <= 4)
		{
			ImmutableArray<T>.Enumerator enumerator = items.GetEnumerator();
			while (enumerator.MoveNext())
			{
				T current = enumerator.Current;
				count++;
				this[count - 1] = current;
			}
		}
		else
		{
			MoveInlineToBuilder();
			builder.AddRange(items);
		}
	}

	public void AddRange(in TemporaryArray<T> items)
	{
		if (count + items.Count <= 4)
		{
			Enumerator enumerator = items.GetEnumerator();
			while (enumerator.MoveNext())
			{
				T current = enumerator.Current;
				count++;
				this[count - 1] = current;
			}
		}
		else
		{
			MoveInlineToBuilder();
			Enumerator enumerator = items.GetEnumerator();
			while (enumerator.MoveNext())
			{
				T current2 = enumerator.Current;
				builder.Add(current2);
			}
		}
	}

	public void Clear()
	{
		if (builder != null)
		{
			builder.Clear();
		}
		else
		{
			this = Empty;
		}
	}

	public readonly Enumerator GetEnumerator()
	{
		return new Enumerator(in this);
	}

	public ImmutableArray<T> ToImmutableAndClear()
	{
		if (builder != null)
		{
			return builder.ToImmutableAndClear();
		}
		object result = count switch
		{
			0 => ImmutableArray<T>.Empty, 
			1 => ImmutableArray.Create(item0), 
			2 => ImmutableArray.Create(item0, item1), 
			3 => ImmutableArray.Create(item0, item1, item2), 
			4 => ImmutableArray.Create(item0, item1, item2, item3), 
			_ => throw ExceptionUtilities.Unreachable, 
		};
		this = Empty;
		return (ImmutableArray<T>)result;
	}

	private void MoveInlineToBuilder()
	{
		ArrayBuilder<T> instance = ArrayBuilder<T>.GetInstance();
		for (int i = 0; i < count; i++)
		{
			instance.Add(this[i]);
			if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
			{
				this[i] = default(T);
			}
		}
		count = 0;
		builder = instance;
	}

	private static void ThrowIndexOutOfRangeException()
	{
		throw new IndexOutOfRangeException();
	}
}
