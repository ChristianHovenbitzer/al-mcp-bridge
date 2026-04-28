using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Utilities;

internal sealed class AsyncLazy<T> : ValueSource<T>, IDisposable
{
	private struct WaitThatValidatesInvariants : IDisposable
	{
		private readonly AsyncLazy<T> _asyncLazy;

		public WaitThatValidatesInvariants(AsyncLazy<T> asyncLazy)
		{
			_asyncLazy = asyncLazy;
		}

		public void Dispose()
		{
			_asyncLazy.AssertInvariants_NoLock();
			AsyncLazy<T>.gate.Release();
		}
	}

	private struct AsynchronousComputationToStart
	{
		public readonly Func<CancellationToken, Task<T>> AsynchronousComputeFunction;

		public readonly CancellationTokenSource CancellationTokenSource;

		public AsynchronousComputationToStart(Func<CancellationToken, Task<T>> asynchronousComputeFunction, CancellationTokenSource cancellationTokenSource)
		{
			AsynchronousComputeFunction = asynchronousComputeFunction;
			CancellationTokenSource = cancellationTokenSource;
		}
	}

	private sealed class Request
	{
		private CancellationToken _cancellationToken;

		private CancellationTokenRegistration _cancellationTokenRegistration;

		private AsyncTaskMethodBuilder<T> _taskBuilder;

		public Task<T> Task => _taskBuilder.Task;

		public Request()
		{
			_ = _taskBuilder.Task;
		}

		public void RegisterForCancellation(Action<object> callback, CancellationToken cancellationToken)
		{
			_cancellationToken = cancellationToken;
			_cancellationTokenRegistration = cancellationToken.Register(callback, this);
		}

		public void CompleteFromTaskAsynchronously(Task<T> task)
		{
			System.Threading.Tasks.Task.Factory.StartNew(CompleteFromTaskSynchronouslyStub, task, CancellationToken.None, TaskCreationOptions.None, TaskScheduler.Default);
		}

		private void CompleteFromTaskSynchronouslyStub(object task)
		{
			CompleteFromTaskSynchronously((Task<T>)task);
		}

		public void CompleteFromTaskSynchronously(Task<T> task)
		{
			if (_taskBuilder.Task.IsCompleted)
			{
				return;
			}
			try
			{
				if (task.IsCanceled || _cancellationToken.IsCancellationRequested)
				{
					CancelSynchronously();
				}
				else if (task.IsFaulted)
				{
					_taskBuilder.SetException(task.Exception);
				}
				else
				{
					_taskBuilder.SetResult(task.Result);
				}
			}
			catch (InvalidOperationException)
			{
			}
			_cancellationTokenRegistration.Dispose();
		}

		public void CancelAsynchronously()
		{
			System.Threading.Tasks.Task.Factory.StartNew(CancelSynchronously, CancellationToken.None, TaskCreationOptions.None, TaskScheduler.Default);
		}

		private void CancelSynchronously()
		{
			if (_taskBuilder.Task.IsCompleted)
			{
				return;
			}
			try
			{
				_taskBuilder.SetException(new OperationCanceledException(_cancellationToken));
			}
			catch (InvalidOperationException)
			{
			}
		}
	}

	private Func<CancellationToken, Task<T>> asynchronousComputeFunction;

	private Func<CancellationToken, T> synchronousComputeFunction;

	private readonly bool cacheResult;

	private Task<T> cachedResult;

	private static readonly NonReentrantLock gate = new NonReentrantLock(useThisInstanceForSynchronization: true);

	private HashSet<Request> requests;

	private CancellationTokenSource asynchronousComputationCancellationSource;

	private bool computationActive;

	private bool disposedValue;

	public AsyncLazy(T value)
	{
		cacheResult = true;
		cachedResult = Task.FromResult(value);
	}

	public AsyncLazy(Func<CancellationToken, Task<T>> asynchronousComputeFunction, bool cacheResult)
		: this(asynchronousComputeFunction, (Func<CancellationToken, T>)null, cacheResult)
	{
	}

	public AsyncLazy(Func<CancellationToken, Task<T>> asynchronousComputeFunction, Func<CancellationToken, T> synchronousComputeFunction, bool cacheResult)
	{
		Contract.ThrowIfNull(asynchronousComputeFunction);
		this.asynchronousComputeFunction = asynchronousComputeFunction;
		this.synchronousComputeFunction = synchronousComputeFunction;
		this.cacheResult = cacheResult;
	}

	private WaitThatValidatesInvariants TakeLock(CancellationToken cancellationToken)
	{
		gate.Wait(cancellationToken);
		AssertInvariants_NoLock();
		return new WaitThatValidatesInvariants(this);
	}

	private void AssertInvariants_NoLock()
	{
		Contract.ThrowIfTrue(asynchronousComputationCancellationSource != null && !computationActive);
		Contract.ThrowIfTrue(requests != null && requests.Count == 0);
		Contract.ThrowIfTrue(requests != null && !computationActive);
		Contract.ThrowIfTrue(cachedResult != null && (synchronousComputeFunction != null || asynchronousComputeFunction != null));
		Contract.ThrowIfTrue(asynchronousComputeFunction == null && synchronousComputeFunction != null);
	}

	public override bool TryGetValue(out T result)
	{
		if (cachedResult != null)
		{
			result = cachedResult.Result;
			return true;
		}
		result = default(T);
		return false;
	}

	public override T GetValue(CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();
		Request request = null;
		AsynchronousComputationToStart? asynchronousComputationToStart = null;
		using (TakeLock(cancellationToken))
		{
			if (cachedResult != null)
			{
				return cachedResult.Result;
			}
			if (computationActive)
			{
				request = CreateNewRequest_NoLock();
			}
			else if (synchronousComputeFunction == null)
			{
				request = CreateNewRequest_NoLock();
				asynchronousComputationToStart = RegisterAsynchronousComputation_NoLock();
			}
			else
			{
				computationActive = true;
			}
		}
		if (request != null)
		{
			request.RegisterForCancellation(OnAsynchronousRequestCancelled, cancellationToken);
			if (asynchronousComputationToStart.HasValue)
			{
				StartAsynchronousComputation(asynchronousComputationToStart.Value, request, cancellationToken);
			}
			return request.Task.WaitAndGetResult(cancellationToken);
		}
		T result;
		try
		{
			result = synchronousComputeFunction(cancellationToken);
		}
		catch (OperationCanceledException)
		{
			using (TakeLock(CancellationToken.None))
			{
				computationActive = false;
				if (requests != null)
				{
					asynchronousComputationToStart = RegisterAsynchronousComputation_NoLock();
				}
			}
			if (asynchronousComputationToStart.HasValue)
			{
				StartAsynchronousComputation(asynchronousComputationToStart.Value, null, cancellationToken);
			}
			throw;
		}
		catch (Exception exception)
		{
			TaskCompletionSource<T> taskCompletionSource = new TaskCompletionSource<T>();
			taskCompletionSource.SetException(exception);
			CompleteWithTask(taskCompletionSource.Task, CancellationToken.None);
			throw;
		}
		CompleteWithTask(Task.FromResult(result), CancellationToken.None);
		cancellationToken.ThrowIfCancellationRequested();
		return result;
	}

	private Request CreateNewRequest_NoLock()
	{
		if (requests == null)
		{
			requests = new HashSet<Request>();
		}
		Request request = new Request();
		requests.Add(request);
		return request;
	}

	public override Task<T> GetValueAsync(CancellationToken cancellationToken)
	{
		if (cancellationToken.IsCancellationRequested)
		{
			return new Task<T>(() => default(T), cancellationToken);
		}
		AsynchronousComputationToStart? asynchronousComputationToStart = null;
		Request request;
		using (TakeLock(cancellationToken))
		{
			if (cachedResult != null)
			{
				return cachedResult;
			}
			request = CreateNewRequest_NoLock();
			if (!computationActive)
			{
				asynchronousComputationToStart = RegisterAsynchronousComputation_NoLock();
			}
		}
		request.RegisterForCancellation(OnAsynchronousRequestCancelled, cancellationToken);
		if (asynchronousComputationToStart.HasValue)
		{
			StartAsynchronousComputation(asynchronousComputationToStart.Value, request, cancellationToken);
		}
		return request.Task;
	}

	private AsynchronousComputationToStart RegisterAsynchronousComputation_NoLock()
	{
		Contract.ThrowIfTrue(computationActive);
		asynchronousComputationCancellationSource = new CancellationTokenSource();
		computationActive = true;
		return new AsynchronousComputationToStart(asynchronousComputeFunction, asynchronousComputationCancellationSource);
	}

	private void StartAsynchronousComputation(AsynchronousComputationToStart computationToStart, Request requestToCompleteSynchronously, CancellationToken callerCancellationToken)
	{
		CancellationToken token = computationToStart.CancellationTokenSource.Token;
		try
		{
			token.ThrowIfCancellationRequested();
			try
			{
				Task<T> task = computationToStart.AsynchronousComputeFunction(token);
				if (requestToCompleteSynchronously != null && task.IsCompleted)
				{
					using (TakeLock(CancellationToken.None))
					{
						task = GetCachedValueAndCacheThisValueIfNoneCached_NoLock(task);
					}
					requestToCompleteSynchronously.CompleteFromTaskSynchronously(task);
				}
				task.ContinueWith(delegate(Task<T> t, object? s)
				{
					CompleteWithTask(t, ((CancellationTokenSource)s).Token);
				}, computationToStart.CancellationTokenSource, token, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
			}
			catch (Exception exception) when (FatalError.ReportUnlessCanceled(exception))
			{
				throw ExceptionUtilities.Unreachable;
			}
		}
		catch (OperationCanceledException exception2) when (CrashIfCanceledWithDifferentToken(exception2, token))
		{
			callerCancellationToken.ThrowIfCancellationRequested();
			throw ExceptionUtilities.Unreachable;
		}
	}

	private static bool CrashIfCanceledWithDifferentToken(OperationCanceledException exception, CancellationToken cancellationToken)
	{
		if (exception.CancellationToken != cancellationToken)
		{
			FatalError.Report(exception);
		}
		return true;
	}

	private void CompleteWithTask(Task<T> task, CancellationToken cancellationToken)
	{
		IEnumerable<Request> enumerable2;
		using (TakeLock(cancellationToken))
		{
			cancellationToken.ThrowIfCancellationRequested();
			IEnumerable<Request> enumerable = requests;
			enumerable2 = enumerable ?? SpecializedCollections.EmptyEnumerable<Request>();
			requests = null;
			asynchronousComputationCancellationSource = null;
			computationActive = false;
			task = GetCachedValueAndCacheThisValueIfNoneCached_NoLock(task);
		}
		foreach (Request item in enumerable2)
		{
			item.CompleteFromTaskAsynchronously(task);
		}
	}

	private Task<T> GetCachedValueAndCacheThisValueIfNoneCached_NoLock(Task<T> task)
	{
		if (cachedResult != null)
		{
			return cachedResult;
		}
		if (cacheResult && task.Status == TaskStatus.RanToCompletion)
		{
			cachedResult = task;
			asynchronousComputeFunction = null;
			synchronousComputeFunction = null;
		}
		return task;
	}

	private void OnAsynchronousRequestCancelled(object state)
	{
		Request request = (Request)state;
		CancellationTokenSource cancellationTokenSource = null;
		using (TakeLock(CancellationToken.None))
		{
			if (requests != null && requests.Remove(request) && requests.Count == 0)
			{
				requests = null;
				if (asynchronousComputationCancellationSource != null)
				{
					cancellationTokenSource = asynchronousComputationCancellationSource;
					asynchronousComputationCancellationSource = null;
					computationActive = false;
				}
			}
		}
		request.CancelAsynchronously();
		cancellationTokenSource?.Cancel();
	}

	private void Dispose(bool disposing)
	{
		if (!disposedValue)
		{
			if (disposing)
			{
				asynchronousComputationCancellationSource.Dispose();
				asynchronousComputationCancellationSource = null;
			}
			disposedValue = true;
		}
	}

	public void Dispose()
	{
		Dispose(disposing: true);
	}
}
