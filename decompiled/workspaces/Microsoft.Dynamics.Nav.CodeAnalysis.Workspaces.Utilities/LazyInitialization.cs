using System;
using System.Threading;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Utilities;

internal static class LazyInitialization
{
	internal static T InterlockedStore<T>(ref T target, T value) where T : class
	{
		return Interlocked.CompareExchange(ref target, value, null) ?? value;
	}

	public static T EnsureInitialized<T>(ref T target, Func<T> valueFactory) where T : class
	{
		return Volatile.Read(ref target) ?? InterlockedStore(ref target, valueFactory());
	}

	public static T EnsureInitialized<T, U>(ref T target, Func<U, T> valueFactory, U state) where T : class
	{
		return Volatile.Read(ref target) ?? InterlockedStore(ref target, valueFactory(state));
	}
}
