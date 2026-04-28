using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Formatting;

internal abstract class TaskExecutor
{
	private class ConcurrentExecutor : TaskExecutor
	{
		public override Task StartNew(Action action, CancellationToken cancellationToken)
		{
			return Task.Factory.StartNew(action, cancellationToken, TaskCreationOptions.None, TaskScheduler.Default);
		}

		public override Task<T> StartNew<T>(Func<T> action, CancellationToken cancellationToken)
		{
			return Task.Factory.StartNew(action, cancellationToken, TaskCreationOptions.None, TaskScheduler.Default);
		}

		public override Task<T2> ContinueWith<T1, T2>(Task<T1> previousTask, Func<Task<T1>, T2> nextAction, CancellationToken cancellationToken)
		{
			return previousTask.ContinueWith(nextAction, cancellationToken, TaskContinuationOptions.OnlyOnRanToCompletion, TaskScheduler.Default);
		}

		public override Task ContinueWith<T>(Task<T> previousTask, Action<Task<T>> nextAction, CancellationToken cancellationToken)
		{
			return previousTask.ContinueWith(nextAction, cancellationToken, TaskContinuationOptions.OnlyOnRanToCompletion, TaskScheduler.Default);
		}

		public override void ForEach<T>(IEnumerable<T> source, Action<T> action, CancellationToken cancellationToken)
		{
			Action<T> action2 = action;
			ParallelOptions parallelOptions = new ParallelOptions
			{
				CancellationToken = cancellationToken,
				TaskScheduler = TaskScheduler.Default
			};
			Parallel.ForEach(source, parallelOptions, delegate(T pair)
			{
				action2(pair);
			});
		}

		public override void For(int fromInclusive, int toExclusive, Action<int> body, CancellationToken cancellationToken)
		{
			ParallelOptions parallelOptions = new ParallelOptions
			{
				CancellationToken = cancellationToken,
				TaskScheduler = TaskScheduler.Default
			};
			Parallel.For(fromInclusive, toExclusive, parallelOptions, body);
		}

		public override IEnumerable<TResult> Filter<TSource, TResult>(IEnumerable<TSource> source, Func<TSource, bool> filter, Func<TSource, TResult> selector, CancellationToken cancellationToken)
		{
			return source.AsParallel().AsOrdered().WithCancellation(cancellationToken)
				.Where(filter)
				.Select(selector);
		}
	}

	private class SynchronousExecutor : TaskExecutor
	{
		public override Task StartNew(Action action, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();
			action();
			return SpecializedTasks.EmptyTask;
		}

		public override Task<T> StartNew<T>(Func<T> action, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();
			return Task.FromResult(action());
		}

		public override Task<T2> ContinueWith<T1, T2>(Task<T1> previousTask, Func<Task<T1>, T2> nextAction, CancellationToken cancellationToken)
		{
			previousTask.Wait(cancellationToken);
			return Task.FromResult(nextAction(previousTask));
		}

		public override Task ContinueWith<T>(Task<T> previousTask, Action<Task<T>> nextAction, CancellationToken cancellationToken)
		{
			previousTask.Wait(cancellationToken);
			nextAction(previousTask);
			return SpecializedTasks.EmptyTask;
		}

		public override void ForEach<T>(IEnumerable<T> source, Action<T> action, CancellationToken cancellationToken)
		{
			foreach (T item in source)
			{
				cancellationToken.ThrowIfCancellationRequested();
				action(item);
			}
		}

		public override void For(int fromInclusive, int toExclusive, Action<int> body, CancellationToken cancellationToken)
		{
			for (int i = fromInclusive; i < toExclusive; i++)
			{
				cancellationToken.ThrowIfCancellationRequested();
				body(i);
			}
		}

		public override IEnumerable<TResult> Filter<TSource, TResult>(IEnumerable<TSource> source, Func<TSource, bool> filter, Func<TSource, TResult> selector, CancellationToken cancellationToken)
		{
			foreach (TSource item in source)
			{
				cancellationToken.ThrowIfCancellationRequested();
				if (filter(item))
				{
					yield return selector(item);
				}
			}
		}
	}

	public static readonly TaskExecutor Concurrent = new ConcurrentExecutor();

	public static readonly TaskExecutor Synchronous = new SynchronousExecutor();

	private const int NumberOfTasksToRunTreshold = 1000;

	public abstract Task<T2> ContinueWith<T1, T2>(Task<T1> previousTask, Func<Task<T1>, T2> nextAction, CancellationToken cancellationToken);

	public abstract Task ContinueWith<T>(Task<T> previousTask, Action<Task<T>> nextAction, CancellationToken cancellationToken);

	public abstract Task StartNew(Action action, CancellationToken cancellationToken);

	public abstract Task<T> StartNew<T>(Func<T> action, CancellationToken cancellationToken);

	public abstract void ForEach<T>(IEnumerable<T> source, Action<T> action, CancellationToken cancellationToken);

	public abstract void For(int fromInclusive, int toExclusive, Action<int> body, CancellationToken cancellationToken);

	public abstract IEnumerable<TResult> Filter<TSource, TResult>(IEnumerable<TSource> source, Func<TSource, bool> filter, Func<TSource, TResult> projection, CancellationToken cancellationToken);

	public static TaskExecutor GetSuitableInstance(int numberOfTasksToRun)
	{
		if (numberOfTasksToRun <= 1000 * Environment.ProcessorCount)
		{
			return Synchronous;
		}
		return Concurrent;
	}
}
