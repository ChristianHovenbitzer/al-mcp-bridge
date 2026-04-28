using System;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Utilities;

internal static class ArrayExtensions
{
	internal static T[] Copy<T>(this T[] array, int start, int length)
	{
		if (start + length > array.Length)
		{
			length = array.Length - start;
		}
		T[] array2 = new T[length];
		Array.Copy(array, start, array2, 0, length);
		return array2;
	}

	internal static bool ValueEquals(this uint[] array, uint[] other)
	{
		if (array == other)
		{
			return true;
		}
		if (array == null || other == null || array.Length != other.Length)
		{
			return false;
		}
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i] != other[i])
			{
				return false;
			}
		}
		return true;
	}

	internal static T[] InsertAt<T>(this T[] array, int position, T item)
	{
		T[] array2 = new T[array.Length + 1];
		if (position > 0)
		{
			Array.Copy(array, array2, position);
		}
		if (position < array.Length)
		{
			Array.Copy(array, position, array2, position + 1, array.Length - position);
		}
		array2[position] = item;
		return array2;
	}

	internal static T[] Append<T>(this T[] array, T item)
	{
		return array.InsertAt(array.Length, item);
	}

	internal static T[] InsertAt<T>(this T[] array, int position, T[] items)
	{
		T[] array2 = new T[array.Length + items.Length];
		if (position > 0)
		{
			Array.Copy(array, array2, position);
		}
		if (position < array.Length)
		{
			Array.Copy(array, position, array2, position + items.Length, array.Length - position);
		}
		items.CopyTo(array2, position);
		return array2;
	}

	internal static T[] Append<T>(this T[] array, T[] items)
	{
		return array.InsertAt(array.Length, items);
	}

	internal static T[] RemoveAt<T>(this T[] array, int position)
	{
		return array.RemoveAt(position, 1);
	}

	internal static T[] RemoveAt<T>(this T[] array, int position, int length)
	{
		if (position + length > array.Length)
		{
			length = array.Length - position;
		}
		T[] array2 = new T[array.Length - length];
		if (position > 0)
		{
			Array.Copy(array, array2, position);
		}
		if (position < array2.Length)
		{
			Array.Copy(array, position + length, array2, position, array2.Length - position);
		}
		return array2;
	}

	internal static T[] ReplaceAt<T>(this T[] array, int position, T item)
	{
		T[] array2 = new T[array.Length];
		Array.Copy(array, array2, array.Length);
		array2[position] = item;
		return array2;
	}

	internal static T[] ReplaceAt<T>(this T[] array, int position, int length, T[] items)
	{
		return array.RemoveAt(position, length).InsertAt(position, items);
	}

	internal static void ReverseContents<T>(this T[] array)
	{
		array.ReverseContents(0, array.Length);
	}

	internal static void ReverseContents<T>(this T[] array, int start, int count)
	{
		int num = start + count - 1;
		int num2 = start;
		int num3 = num;
		while (num2 < num3)
		{
			T val = array[num2];
			array[num2] = array[num3];
			array[num3] = val;
			num2++;
			num3--;
		}
	}

	internal static int BinarySearch(this int[] array, int value)
	{
		int num = 0;
		int num2 = array.Length - 1;
		while (num <= num2)
		{
			int num3 = num + (num2 - num >> 1);
			int num4 = array[num3];
			if (num4 == value)
			{
				return num3;
			}
			if (num4 > value)
			{
				num2 = num3 - 1;
			}
			else
			{
				num = num3 + 1;
			}
		}
		return ~num;
	}

	internal static int BinarySearchUpperBound(this int[] array, int value)
	{
		int num = 0;
		int num2 = array.Length - 1;
		while (num <= num2)
		{
			int num3 = num + (num2 - num >> 1);
			if (array[num3] > value)
			{
				num2 = num3 - 1;
			}
			else
			{
				num = num3 + 1;
			}
		}
		return num;
	}
}
