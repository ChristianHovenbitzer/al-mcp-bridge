using System;
using System.Threading;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Utilities;

internal static class ImmutableHashMapExtensions
{
	public static TValue GetOrAdd<TKey, TValue, TArg>(ref ImmutableHashMap<TKey, TValue> location, TKey key, Func<TKey, TArg, TValue> valueFactory, TArg factoryArgument)
	{
		Contract.ThrowIfNull(valueFactory);
		ImmutableHashMap<TKey, TValue> immutableHashMap = Volatile.Read(ref location);
		Contract.ThrowIfNull(immutableHashMap);
		if (immutableHashMap.TryGetValue(key, out var value))
		{
			return value;
		}
		TValue val = valueFactory(key, factoryArgument);
		do
		{
			ImmutableHashMap<TKey, TValue> value2 = immutableHashMap.Add(key, val);
			ImmutableHashMap<TKey, TValue> immutableHashMap2 = Interlocked.CompareExchange(ref location, value2, immutableHashMap);
			if (immutableHashMap2 == immutableHashMap)
			{
				return val;
			}
			immutableHashMap = immutableHashMap2;
		}
		while (!immutableHashMap.TryGetValue(key, out value));
		return value;
	}
}
