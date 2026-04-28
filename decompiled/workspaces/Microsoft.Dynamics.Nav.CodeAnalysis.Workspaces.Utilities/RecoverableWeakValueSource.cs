using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Utilities;

internal abstract class RecoverableWeakValueSource<T> : ValueSource<T>, IDisposable where T : class
{
	private SemaphoreSlim gateDoNotAccessDirectly;

	private bool saved;

	private WeakReference<T> weakInstance;

	private ValueSource<T> recoverySource;

	private static readonly WeakReference<T> noReference = new WeakReference<T>(null);

	private static Task s_latestTask = SpecializedTasks.EmptyTask;

	private static readonly NonReentrantLock s_taskGuard = new NonReentrantLock();

	private bool disposedValue;

	private SemaphoreSlim Gate => LazyInitialization.EnsureInitialized(ref gateDoNotAccessDirectly, SemaphoreSlimFactory.Instance);

	public RecoverableWeakValueSource(ValueSource<T> initialValue)
	{
		weakInstance = noReference;
		recoverySource = initialValue;
	}

	public RecoverableWeakValueSource(RecoverableWeakValueSource<T> savedSource)
	{
		Contract.ThrowIfFalse(savedSource.saved);
		Contract.ThrowIfFalse(savedSource.GetType() == GetType());
		saved = true;
		weakInstance = noReference;
		recoverySource = new AsyncLazy<T>(RecoverAsync, Recover, cacheResult: false);
	}

	protected abstract Task SaveAsync(T instance, CancellationToken cancellationToken);

	protected abstract Task<T> RecoverAsync(CancellationToken cancellationToken);

	protected abstract T Recover(CancellationToken cancellationToken);

	public override bool TryGetValue(out T value)
	{
		return weakInstance.TryGetTarget(out value);
	}

	public override T GetValue(CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();
		if (!weakInstance.TryGetTarget(out var target))
		{
			Task saveTask = null;
			using (Gate.DisposableWait(cancellationToken))
			{
				if (!weakInstance.TryGetTarget(out target))
				{
					target = recoverySource.GetValue(cancellationToken);
					saveTask = EnsureInstanceIsSaved(target);
				}
			}
			ResetRecoverySource(saveTask, target);
		}
		return target;
	}

	public override async Task<T> GetValueAsync(CancellationToken cancellationToken)
	{
		if (!weakInstance.TryGetTarget(out var target))
		{
			Task saveTask = null;
			using (await Gate.DisposableWaitAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
			{
				if (!weakInstance.TryGetTarget(out target))
				{
					target = await recoverySource.GetValueAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
					saveTask = EnsureInstanceIsSaved(target);
				}
			}
			ResetRecoverySource(saveTask, target);
		}
		return target;
	}

	private void ResetRecoverySource(Task saveTask, T instance)
	{
		T instance2 = instance;
		saveTask?.SafeContinueWith(delegate
		{
			using (Gate.DisposableWait(CancellationToken.None))
			{
				recoverySource = new AsyncLazy<T>(RecoverAsync, Recover, cacheResult: false);
				GC.KeepAlive(instance2);
			}
		}, TaskScheduler.Default);
	}

	private Task EnsureInstanceIsSaved(T instance)
	{
		T instance2 = instance;
		if (weakInstance == noReference)
		{
			weakInstance = new WeakReference<T>(instance2);
		}
		else
		{
			weakInstance.SetTarget(instance2);
		}
		if (!saved)
		{
			saved = true;
			using (s_taskGuard.DisposableWait())
			{
				s_latestTask = s_latestTask.SafeContinueWithFromAsync((Task t) => SaveAsync(instance2, CancellationToken.None), CancellationToken.None, TaskScheduler.Default);
				return s_latestTask;
			}
		}
		return null;
	}

	protected virtual void Dispose(bool disposing)
	{
		if (!disposedValue)
		{
			if (disposing && gateDoNotAccessDirectly != null)
			{
				gateDoNotAccessDirectly.Dispose();
			}
			disposedValue = true;
		}
	}

	public void Dispose()
	{
		Dispose(disposing: true);
	}
}
