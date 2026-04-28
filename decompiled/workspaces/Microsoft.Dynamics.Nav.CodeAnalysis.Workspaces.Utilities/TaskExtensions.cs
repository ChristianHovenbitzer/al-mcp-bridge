using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Utilities;

internal static class TaskExtensions
{
	public static T WaitAndGetResult<T>(this Task<T> task, CancellationToken cancellationToken)
	{
		task.Wait(cancellationToken);
		return task.Result;
	}

	public static Task SafeContinueWith(this Task task, Action<Task> continuationAction, CancellationToken cancellationToken, TaskContinuationOptions continuationOptions, TaskScheduler scheduler)
	{
		Action<Task> continuationAction2 = continuationAction;
		Func<Task, bool> continuationFunction = delegate(Task antecedent)
		{
			continuationAction2(antecedent);
			return true;
		};
		return task.SafeContinueWith(continuationFunction, cancellationToken, continuationOptions, scheduler);
	}

	public static Task<TResult> SafeContinueWith<TInput, TResult>(this Task<TInput> task, Func<Task<TInput>, TResult> continuationFunction, CancellationToken cancellationToken, TaskScheduler scheduler)
	{
		return task.SafeContinueWith(continuationFunction, cancellationToken, TaskContinuationOptions.None, scheduler);
	}

	public static Task<TResult> SafeContinueWith<TInput, TResult>(this Task<TInput> task, Func<Task<TInput>, TResult> continuationFunction, CancellationToken cancellationToken, TaskContinuationOptions continuationOptions, TaskScheduler scheduler)
	{
		Func<Task<TInput>, TResult> continuationFunction2 = continuationFunction;
		return task.SafeContinueWith((Task antecedent) => continuationFunction2((Task<TInput>)antecedent), cancellationToken, continuationOptions, scheduler);
	}

	public static Task SafeContinueWith<TInput>(this Task<TInput> task, Action<Task<TInput>> continuationAction, CancellationToken cancellationToken, TaskContinuationOptions continuationOptions, TaskScheduler scheduler)
	{
		Action<Task<TInput>> continuationAction2 = continuationAction;
		return task.SafeContinueWith(delegate(Task antecedent)
		{
			continuationAction2((Task<TInput>)antecedent);
		}, cancellationToken, continuationOptions, scheduler);
	}

	public static Task<TResult> SafeContinueWith<TResult>(this Task task, Func<Task, TResult> continuationFunction, CancellationToken cancellationToken, TaskContinuationOptions continuationOptions, TaskScheduler scheduler)
	{
		Func<Task, TResult> continuationFunction2 = continuationFunction;
		Func<Task, TResult> continuationFunction3 = delegate(Task t)
		{
			try
			{
				return continuationFunction2(t);
			}
			catch (Exception exception) when (FatalError.ReportUnlessCanceled(exception))
			{
				throw ExceptionUtilities.Unreachable;
			}
		};
		return task.ContinueWith(continuationFunction3, cancellationToken, continuationOptions | TaskContinuationOptions.LazyCancellation, scheduler);
	}

	public static Task<TResult> SafeContinueWith<TResult>(this Task task, Func<Task, TResult> continuationFunction, CancellationToken cancellationToken, TaskScheduler scheduler)
	{
		return task.SafeContinueWith(continuationFunction, cancellationToken, TaskContinuationOptions.None, scheduler);
	}

	public static Task SafeContinueWith(this Task task, Action<Task> continuationAction, TaskScheduler scheduler)
	{
		return task.SafeContinueWith(continuationAction, CancellationToken.None, TaskContinuationOptions.None, scheduler);
	}

	public static Task SafeContinueWith<TInput>(this Task<TInput> task, Action<Task<TInput>> continuationFunction, TaskScheduler scheduler)
	{
		return task.SafeContinueWith(continuationFunction, CancellationToken.None, TaskContinuationOptions.None, scheduler);
	}

	public static Task<TResult> SafeContinueWith<TInput, TResult>(this Task<TInput> task, Func<Task<TInput>, TResult> continuationFunction, TaskScheduler scheduler)
	{
		return task.SafeContinueWith(continuationFunction, CancellationToken.None, TaskContinuationOptions.None, scheduler);
	}

	public static Task SafeContinueWith(this Task task, Action<Task> continuationAction, CancellationToken cancellationToken, TaskScheduler scheduler)
	{
		return task.SafeContinueWith(continuationAction, cancellationToken, TaskContinuationOptions.None, scheduler);
	}

	public static Task<TResult> ContinueWithAfterDelay<TInput, TResult>(this Task<TInput> task, Func<Task<TInput>, TResult> continuationFunction, CancellationToken cancellationToken, int millisecondsDelay, TaskContinuationOptions taskContinuationOptions, TaskScheduler scheduler)
	{
		Func<Task<TInput>, TResult> continuationFunction2 = continuationFunction;
		TaskScheduler scheduler2 = scheduler;
		return task.SafeContinueWith(delegate(Task<TInput> t)
		{
			Task<TInput> t2 = t;
			return Task.Delay(millisecondsDelay, cancellationToken).SafeContinueWith((Task _) => continuationFunction2(t2), cancellationToken, TaskContinuationOptions.None, scheduler2);
		}, cancellationToken, taskContinuationOptions, scheduler2).Unwrap();
	}

	public static Task<TNResult> ContinueWithAfterDelay<TNResult>(this Task task, Func<Task, TNResult> continuationFunction, CancellationToken cancellationToken, int millisecondsDelay, TaskContinuationOptions taskContinuationOptions, TaskScheduler scheduler)
	{
		Func<Task, TNResult> continuationFunction2 = continuationFunction;
		TaskScheduler scheduler2 = scheduler;
		return task.SafeContinueWith(delegate(Task t)
		{
			Task t2 = t;
			return Task.Delay(millisecondsDelay, cancellationToken).SafeContinueWith((Task _) => continuationFunction2(t2), cancellationToken, TaskContinuationOptions.None, scheduler2);
		}, cancellationToken, taskContinuationOptions, scheduler2).Unwrap();
	}

	public static Task ContinueWithAfterDelay(this Task task, Action continuationAction, CancellationToken cancellationToken, int millisecondsDelay, TaskContinuationOptions taskContinuationOptions, TaskScheduler scheduler)
	{
		Action continuationAction2 = continuationAction;
		TaskScheduler scheduler2 = scheduler;
		return task.SafeContinueWith((Task t) => Task.Delay(millisecondsDelay, cancellationToken).SafeContinueWith(delegate
		{
			continuationAction2();
		}, cancellationToken, TaskContinuationOptions.None, scheduler2), cancellationToken, taskContinuationOptions, scheduler2).Unwrap();
	}

	public static Task<TResult> SafeContinueWithFromAsync<TInput, TResult>(this Task<TInput> task, Func<Task<TInput>, Task<TResult>> continuationFunction, CancellationToken cancellationToken, TaskContinuationOptions continuationOptions, TaskScheduler scheduler)
	{
		Func<Task<TInput>, Task<TResult>> continuationFunction2 = continuationFunction;
		return task.SafeContinueWithFromAsync((Task antecedent) => continuationFunction2((Task<TInput>)antecedent), cancellationToken, continuationOptions, scheduler);
	}

	public static Task<TResult> SafeContinueWithFromAsync<TResult>(this Task task, Func<Task, Task<TResult>> continuationFunction, CancellationToken cancellationToken, TaskContinuationOptions continuationOptions, TaskScheduler scheduler)
	{
		Task<TResult> task2 = task.ContinueWith(continuationFunction, cancellationToken, continuationOptions | TaskContinuationOptions.LazyCancellation, scheduler).Unwrap();
		task2.ContinueWith(ReportFatalError, continuationFunction, CancellationToken.None, TaskContinuationOptions.OnlyOnFaulted | TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
		return task2;
	}

	public static Task SafeContinueWithFromAsync(this Task task, Func<Task, Task> continuationFunction, CancellationToken cancellationToken, TaskScheduler scheduler)
	{
		return task.SafeContinueWithFromAsync(continuationFunction, cancellationToken, TaskContinuationOptions.None, scheduler);
	}

	public static Task SafeContinueWithFromAsync(this Task task, Func<Task, Task> continuationFunction, CancellationToken cancellationToken, TaskContinuationOptions continuationOptions, TaskScheduler scheduler)
	{
		Task task2 = task.ContinueWith(continuationFunction, cancellationToken, continuationOptions | TaskContinuationOptions.LazyCancellation, scheduler).Unwrap();
		ReportFatalError(task2, continuationFunction);
		return task2;
	}

	public static Task<TNResult> ContinueWithAfterDelayFromAsync<TNResult>(this Task task, Func<Task, Task<TNResult>> continuationFunction, CancellationToken cancellationToken, int millisecondsDelay, TaskContinuationOptions taskContinuationOptions, TaskScheduler scheduler)
	{
		Func<Task, Task<TNResult>> continuationFunction2 = continuationFunction;
		TaskScheduler scheduler2 = scheduler;
		return task.SafeContinueWith(delegate(Task t)
		{
			Task t2 = t;
			return Task.Delay(millisecondsDelay, cancellationToken).SafeContinueWithFromAsync((Task _) => continuationFunction2(t2), cancellationToken, TaskContinuationOptions.None, scheduler2);
		}, cancellationToken, taskContinuationOptions, scheduler2).Unwrap();
	}

	public static Task ContinueWithAfterDelayFromAsync(this Task task, Func<Task, Task> continuationFunction, CancellationToken cancellationToken, int millisecondsDelay, TaskContinuationOptions taskContinuationOptions, TaskScheduler scheduler)
	{
		Func<Task, Task> continuationFunction2 = continuationFunction;
		TaskScheduler scheduler2 = scheduler;
		return task.SafeContinueWith(delegate(Task t)
		{
			Task t2 = t;
			return Task.Delay(millisecondsDelay, cancellationToken).SafeContinueWithFromAsync((Task _) => continuationFunction2(t2), cancellationToken, TaskContinuationOptions.None, scheduler2);
		}, cancellationToken, taskContinuationOptions, scheduler2).Unwrap();
	}

	internal static void ReportFatalError(Task task, object continuationFunction)
	{
		task.ContinueWith(ReportFatalErrorWorker, continuationFunction, CancellationToken.None, TaskContinuationOptions.OnlyOnFaulted | TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
	}

	[MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
	private static void ReportFatalErrorWorker(Task task, object continuationFunction)
	{
		AggregateException? exception = task.Exception;
		MethodInfo methodInfo = ((Delegate)continuationFunction).GetMethodInfo();
		exception.Data["ContinuationFunction"] = methodInfo.DeclaringType.FullName + "::" + methodInfo.Name;
		FatalError.Report(exception);
	}

	public static T WaitAndGetResult_CanCallOnBackground<T>(this Task<T> task, CancellationToken cancellationToken)
	{
		try
		{
			task.Wait(cancellationToken);
		}
		catch (AggregateException ex)
		{
			ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
		}
		return task.Result;
	}
}
