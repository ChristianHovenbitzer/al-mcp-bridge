using System;
using System.Buffers;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Utilities;

internal class EditDistance : IDisposable
{
	private const int Infinity = 1073741823;

	public const int BeyondThreshold = int.MaxValue;

	private string? source;

	private char[]? sourceLowerCaseCharacters;

	private const int MaxMatrixPoolDimension = 64;

	private static readonly ThreadLocal<int[,]> t_matrixPool = new ThreadLocal<int[,]>(() => InitializeMatrix(new int[64, 64]));

	private const int LastSeenIndexLength = 128;

	private static ThreadLocal<int[]> t_lastSeenIndexPool = new ThreadLocal<int[]>(() => new int[128]);

	public EditDistance(string text)
	{
		source = text ?? throw new ArgumentNullException("text");
		sourceLowerCaseCharacters = ConvertToLowercaseArray(text);
	}

	private static char[] ConvertToLowercaseArray(string text)
	{
		char[] array = ArrayPool<char>.Shared.Rent(text.Length);
		for (int i = 0; i < text.Length; i++)
		{
			array[i] = CaseInsensitiveComparison.ToLower(text[i]);
		}
		return array;
	}

	public void Dispose()
	{
		ArrayPool<char>.Shared.Return(sourceLowerCaseCharacters);
		source = null;
		sourceLowerCaseCharacters = null;
	}

	public static int GetEditDistance(string source, string target, int threshold = int.MaxValue)
	{
		using EditDistance editDistance = new EditDistance(source);
		return editDistance.GetEditDistance(target, threshold);
	}

	public static int GetEditDistance(char[] source, char[] target, int threshold = int.MaxValue)
	{
		return GetEditDistance(new ArraySlice<char>(source), new ArraySlice<char>(target), threshold);
	}

	public int GetEditDistance(string target, int threshold = int.MaxValue)
	{
		if (sourceLowerCaseCharacters == null)
		{
			throw new ObjectDisposedException("EditDistance");
		}
		char[] array = ConvertToLowercaseArray(target);
		try
		{
			return GetEditDistance(new ArraySlice<char>(sourceLowerCaseCharacters, 0, source.Length), new ArraySlice<char>(array, 0, target.Length), threshold);
		}
		finally
		{
			ArrayPool<char>.Shared.Return(array);
		}
	}

	private static int[,] GetMatrix(int width, int height)
	{
		if (width > 64 || height > 64)
		{
			return InitializeMatrix(new int[width, height]);
		}
		return t_matrixPool.Value;
	}

	private static int[,] InitializeMatrix(int[,] matrix)
	{
		int length = matrix.GetLength(0);
		int length2 = matrix.GetLength(1);
		for (int i = 0; i < length; i++)
		{
			matrix[i, 0] = 1073741823;
			if (i < length - 1)
			{
				matrix[i + 1, 1] = i;
			}
		}
		for (int j = 0; j < length2; j++)
		{
			matrix[0, j] = 1073741823;
			if (j < length2 - 1)
			{
				matrix[1, j + 1] = j;
			}
		}
		return matrix;
	}

	public static int GetEditDistance(ArraySlice<char> source, ArraySlice<char> target, int threshold = int.MaxValue)
	{
		if (source.Length > target.Length)
		{
			return GetEditDistanceWorker(target, source, threshold);
		}
		return GetEditDistanceWorker(source, target, threshold);
	}

	private static int GetEditDistanceWorker(ArraySlice<char> source, ArraySlice<char> target, int threshold)
	{
		while (source.Length > 0 && source[source.Length - 1] == target[target.Length - 1])
		{
			source.SetLength(source.Length - 1);
			target.SetLength(target.Length - 1);
		}
		while (source.Length > 0 && source[0] == target[0])
		{
			source.MoveStartForward(1);
			target.MoveStartForward(1);
		}
		int length = source.Length;
		int length2 = target.Length;
		if (length == 0)
		{
			if (length2 > threshold)
			{
				return int.MaxValue;
			}
			return length2;
		}
		int num = length2 - length;
		if (num > threshold)
		{
			return int.MaxValue;
		}
		threshold = Math.Min(threshold, length2);
		int num2 = threshold - num;
		int[,] matrix = GetMatrix(length + 2, length2 + 2);
		int[] value = t_lastSeenIndexPool.Value;
		Array.Clear(value, 0, 128);
		for (int i = 1; i <= length; i++)
		{
			int num3 = 0;
			char c = source[i - 1];
			int num4 = Math.Max(1, i - num2);
			int num5 = Math.Min(length2, i + num + num2);
			if (num4 > 1)
			{
				matrix[i + 1, num4] = 1073741823;
			}
			if (num5 < length2)
			{
				matrix[i + 1, num5 + 2] = 1073741823;
			}
			for (int j = num4; j <= num5; j++)
			{
				char c2 = target[j - 1];
				int num6 = ((c2 < '\u0080') ? value[(uint)c2] : 0);
				int num7 = num3;
				bool flag = c == c2;
				if (flag)
				{
					num3 = j;
				}
				matrix[i + 1, j + 1] = Min(matrix[i, j] + ((!flag) ? 1 : 0), matrix[i + 1, j] + 1, matrix[i, j + 1] + 1, matrix[num6, num7] + (i - num6 - 1) + 1 + (j - num7 - 1));
			}
			if (c < '\u0080')
			{
				value[(uint)c] = i;
			}
			if (matrix[i + 1, i + num + 1] > threshold)
			{
				return int.MaxValue;
			}
		}
		return matrix[length + 1, length2 + 1];
	}

	private static string ToString(int[,] matrix, int width, int height)
	{
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < height; i++)
		{
			for (int j = 0; j < width; j++)
			{
				int num = matrix[j + 2, i + 2];
				stringBuilder.Append(((num == 1073741823) ? "∞" : num.ToString(CultureInfo.CurrentCulture)) + " ");
			}
			stringBuilder.AppendLine();
		}
		return stringBuilder.ToString().Trim();
	}

	private static int GetValue(Dictionary<char, int> da, char c)
	{
		if (!da.TryGetValue(c, out var value))
		{
			return 0;
		}
		return value;
	}

	private static int Min(int v1, int v2, int v3, int v4)
	{
		int num = v1;
		if (v2 < num)
		{
			num = v2;
		}
		if (v3 < num)
		{
			num = v3;
		}
		if (v4 < num)
		{
			num = v4;
		}
		return num;
	}

	private static void SetValue(int[,] matrix, int i, int j, int val)
	{
		matrix[i + 1, j + 1] = val;
	}
}
