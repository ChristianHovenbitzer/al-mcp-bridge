using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Utilities;

internal static class IDictionaryExtensions
{
	public static V GetOrAdd<K, V>(this IDictionary<K, V> dictionary, K key, Func<K, V> function)
	{
		if (!dictionary.TryGetValue(key, out V value))
		{
			value = function(key);
			dictionary.Add(key, value);
		}
		return value;
	}

	public static TValue GetValueOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key)
	{
		if (dictionary.TryGetValue(key, out TValue value))
		{
			return value;
		}
		return default(TValue);
	}

	public static bool DictionaryEquals<K, V>(this IDictionary<K, V> left, IDictionary<K, V> right, IEqualityComparer<KeyValuePair<K, V>> comparer = null)
	{
		IEqualityComparer<KeyValuePair<K, V>> comparer2 = comparer;
		IEqualityComparer<KeyValuePair<K, V>> equalityComparer = comparer2;
		comparer2 = equalityComparer ?? EqualityComparer<KeyValuePair<K, V>>.Default;
		if (left.Count != right.Count)
		{
			return false;
		}
		return left.All<KeyValuePair<K, V>>((KeyValuePair<K, V> pair) => comparer2.Equals(pair));
	}

	public static void MultiAdd<TKey, TValue, TCollection>(this IDictionary<TKey, TCollection> dictionary, TKey key, TValue value) where TCollection : ICollection<TValue>, new()
	{
		if (!dictionary.TryGetValue(key, out TCollection value2))
		{
			value2 = new TCollection();
			dictionary.Add(key, value2);
		}
		value2.Add(value);
	}

	public static void MultiRemove<TKey, TValue, TCollection>(this IDictionary<TKey, TCollection> dictionary, TKey key, TValue value) where TCollection : ICollection<TValue>
	{
		if (dictionary.TryGetValue(key, out TCollection value2))
		{
			value2.Remove(value);
			if (value2.Count == 0)
			{
				dictionary.Remove(key);
			}
		}
	}

	public static void MultiAddRange<TKey, TValue>(this IDictionary<TKey, ArrayBuilder<TValue>> dictionary, TKey key, IEnumerable<TValue> values) where TKey : notnull
	{
		if (!dictionary.TryGetValue(key, out ArrayBuilder<TValue> value))
		{
			value = ArrayBuilder<TValue>.GetInstance();
			dictionary.Add(key, value);
		}
		value.AddRange(values);
	}

	public static ImmutableDictionary<K, ImmutableArray<V>> ToImmutableMultiDictionaryAndFree<K, V>(this PooledDictionary<K, ArrayBuilder<V>> builders) where K : notnull
	{
		ImmutableDictionary<K, ImmutableArray<V>>.Builder builder = ImmutableDictionary.CreateBuilder<K, ImmutableArray<V>>();
		foreach (var (key, arrayBuilder2) in builders)
		{
			builder.Add(key, arrayBuilder2.ToImmutableAndFree());
		}
		builders.Free();
		return builder.ToImmutable();
	}

	public static void MultiAddRange<TKey, TValue, TCollection>(this IDictionary<TKey, TCollection> dictionary, TKey key, IEnumerable<TValue> values) where TCollection : ICollection<TValue>, new()
	{
		if (!dictionary.TryGetValue(key, out TCollection value))
		{
			value = new TCollection();
			dictionary.Add(key, value);
		}
		value.AddRange(values);
	}
}
