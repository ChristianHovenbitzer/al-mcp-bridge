using System;
using System.Globalization;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Utilities;

internal struct ArraySlice<T>
{
	private readonly T[] array;

	private int start;

	private int length;

	public int Length => length;

	public T this[int i] => array[i + start];

	public ArraySlice(T[] array)
		: this(array, 0, array.Length)
	{
	}

	public ArraySlice(T[] array, TextSpan span)
		: this(array, span.Start, span.Length)
	{
	}

	public ArraySlice(T[] array, int start, int length)
	{
		this = default(ArraySlice<T>);
		this.array = array;
		SetStartAndLength(start, length);
	}

	private void SetStartAndLength(int start, int length)
	{
		if (start < 0)
		{
			throw new ArgumentOutOfRangeException("start", string.Format(CultureInfo.CurrentCulture, "{0} < 0", start));
		}
		if (start > array.Length)
		{
			throw new ArgumentOutOfRangeException("start", string.Format(CultureInfo.CurrentCulture, "{0} < {1}", start, array.Length));
		}
		CheckLength(start, length);
		this.start = start;
		this.length = length;
	}

	private void CheckLength(int start, int length)
	{
		if (length < 0)
		{
			throw new ArgumentOutOfRangeException("length", string.Format(CultureInfo.CurrentCulture, "{0} < 0", length));
		}
		if (start + length > array.Length)
		{
			throw new ArgumentOutOfRangeException("start", string.Format(CultureInfo.CurrentCulture, "{0} + {1} > {2}", start, length, array.Length));
		}
	}

	public void MoveStartForward(int amount)
	{
		SetStartAndLength(start + amount, length - amount);
	}

	public void SetLength(int length)
	{
		CheckLength(start, length);
		this.length = length;
	}
}
