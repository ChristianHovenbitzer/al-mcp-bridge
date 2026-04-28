using System.Collections.Generic;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Utilities;

internal static class KeyValuePairUtil
{
	public static KeyValuePair<K, V> Create<K, V>(K key, V value)
	{
		return new KeyValuePair<K, V>(key, value);
	}

	public static void Deconstruct<TKey, TValue>(this KeyValuePair<TKey, TValue> keyValuePair, out TKey key, out TValue value)
	{
		key = keyValuePair.Key;
		value = keyValuePair.Value;
	}

	public static KeyValuePair<TKey, TValue> ToKeyValuePair<TKey, TValue>(this (TKey, TValue) tuple)
	{
		return Create(tuple.Item1, tuple.Item2);
	}
}
