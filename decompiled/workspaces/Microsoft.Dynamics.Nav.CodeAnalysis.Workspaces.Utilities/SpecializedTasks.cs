using System.Collections.Generic;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Utilities;

internal static class SpecializedTasks
{
	private static class Empty<T>
	{
		public static readonly Task<T> Default = Task.FromResult(default(T));

		public static readonly Task<IEnumerable<T>> EmptyEnumerable = Task.FromResult(SpecializedCollections.EmptyEnumerable<T>());

		public static readonly Task<ImmutableArray<T>> EmptyImmutableArray = Task.FromResult(ImmutableArray<T>.Empty);
	}

	private static class FromResultCache<T> where T : class
	{
		private static readonly ConditionalWeakTable<T, Task<T>> fromResultCache = new ConditionalWeakTable<T, Task<T>>();

		private static readonly ConditionalWeakTable<T, Task<T>>.CreateValueCallback taskCreationCallback = Task.FromResult;

		public static Task<T> FromResult(T t)
		{
			return fromResultCache.GetValue(t, taskCreationCallback);
		}
	}

	public static readonly Task<bool> True = Task.FromResult(result: true);

	public static readonly Task<bool> False = Task.FromResult(result: false);

	public static readonly Task EmptyTask = Empty<object>.Default;

	public static Task<T> Default<T>()
	{
		return Empty<T>.Default;
	}

	public static Task<ImmutableArray<T>> EmptyImmutableArray<T>()
	{
		return Empty<T>.EmptyImmutableArray;
	}

	public static Task<IEnumerable<T>> EmptyEnumerable<T>()
	{
		return Empty<T>.EmptyEnumerable;
	}

	public static Task<T> FromResult<T>(T t) where T : class
	{
		return FromResultCache<T>.FromResult(t);
	}
}
