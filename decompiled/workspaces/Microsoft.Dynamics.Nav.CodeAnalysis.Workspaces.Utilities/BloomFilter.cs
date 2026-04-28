using System;
using System.Collections;
using System.Collections.Generic;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Utilities;

internal class BloomFilter
{
	private readonly BitArray bitArray;

	private readonly int hashFunctionCount;

	private readonly bool isCaseSensitive;

	public BloomFilter(int expectedCount, double falsePositiveProbability, bool isCaseSensitive)
	{
		int num = Math.Max(1, ComputeM(expectedCount, falsePositiveProbability));
		int num2 = Math.Max(1, ComputeK(expectedCount, falsePositiveProbability));
		int length = (num + 7) & -8;
		bitArray = new BitArray(length);
		hashFunctionCount = num2;
		this.isCaseSensitive = isCaseSensitive;
	}

	public BloomFilter(double falsePositiveProbability, bool isCaseSensitive, ICollection<string> values)
		: this(values.Count, falsePositiveProbability, isCaseSensitive)
	{
		AddRange(values);
	}

	private static int ComputeM(int expectedCount, double falsePositiveProbability)
	{
		double num = (double)expectedCount * Math.Log(falsePositiveProbability);
		double num2 = Math.Log(1.0 / Math.Pow(2.0, Math.Log(2.0)));
		return (int)Math.Ceiling(num / num2);
	}

	private static int ComputeK(int expectedCount, double falsePositiveProbability)
	{
		double num = expectedCount;
		double num2 = ComputeM(expectedCount, falsePositiveProbability);
		return (int)Math.Round(Math.Log(2.0) * num2 / num);
	}

	private int ComputeHash(string key, int seed)
	{
		return ComputeHash(key.AsSpan(), seed);
	}

	private int ComputeHash(ReadOnlySpan<char> key, int seed)
	{
		int num = key.Length;
		uint num2 = (uint)(seed ^ num);
		int num3 = 0;
		while (num >= 2)
		{
			uint character = GetCharacter(key, num3);
			uint character2 = GetCharacter(key, num3 + 1);
			uint num4 = character | (character2 << 16);
			num4 *= 1540483477;
			num4 ^= num4 >> 24;
			num4 *= 1540483477;
			num2 *= 1540483477;
			num2 ^= num4;
			num3 += 2;
			num -= 2;
		}
		if (num == 1)
		{
			num2 ^= GetCharacter(key, num3);
			num2 *= 1540483477;
		}
		num2 ^= num2 >> 13;
		num2 *= 1540483477;
		return (int)(num2 ^ (num2 >> 15));
	}

	private uint GetCharacter(ReadOnlySpan<char> key, int index)
	{
		char c = key[index];
		if (!isCaseSensitive)
		{
			return char.ToLowerInvariant(c);
		}
		return c;
	}

	public void AddRange(IEnumerable<string> values)
	{
		foreach (string value in values)
		{
			Add(value);
		}
	}

	public void Add(string value)
	{
		Add(value.AsSpan());
	}

	public void Add(ReadOnlySpan<char> value)
	{
		for (int i = 0; i < hashFunctionCount; i++)
		{
			int num = ComputeHash(value, i);
			num %= bitArray.Length;
			bitArray[Math.Abs(num)] = true;
		}
	}

	public bool ProbablyContains(string value)
	{
		return ProbablyContains(value.AsSpan());
	}

	public bool ProbablyContains(ReadOnlySpan<char> value)
	{
		for (int i = 0; i < hashFunctionCount; i++)
		{
			int num = ComputeHash(value, i);
			num %= bitArray.Length;
			if (!bitArray[Math.Abs(num)])
			{
				return false;
			}
		}
		return true;
	}
}
