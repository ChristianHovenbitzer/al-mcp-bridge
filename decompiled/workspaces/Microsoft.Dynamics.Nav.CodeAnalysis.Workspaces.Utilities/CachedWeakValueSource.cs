using System;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Utilities;

internal class CachedWeakValueSource<T> : ValueSource<T> where T : class
{
	private SemaphoreSlim gateDoNotAccessDirectly;

	private readonly ValueSource<T> source;

	private WeakReference<T> reference;

	private static readonly WeakReference<T> weakReference = new WeakReference<T>(null);

	private SemaphoreSlim Gate => LazyInitialization.EnsureInitialized(ref gateDoNotAccessDirectly, SemaphoreSlimFactory.Instance);

	public CachedWeakValueSource(ValueSource<T> source)
	{
		this.source = source;
		reference = weakReference;
	}

	public override bool TryGetValue(out T value)
	{
		if (!reference.TryGetTarget(out value))
		{
			return source.TryGetValue(out value);
		}
		return true;
	}

	public override T GetValue(CancellationToken cancellationToken = default(CancellationToken))
	{
		if (!reference.TryGetTarget(out var target))
		{
			using (Gate.DisposableWait(cancellationToken))
			{
				if (!reference.TryGetTarget(out target))
				{
					target = source.GetValue(cancellationToken);
					reference = new WeakReference<T>(target);
				}
			}
		}
		return target;
	}

	public override async Task<T> GetValueAsync(CancellationToken cancellationToken = default(CancellationToken))
	{
		if (!reference.TryGetTarget(out var target))
		{
			using (await Gate.DisposableWaitAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
			{
				if (!reference.TryGetTarget(out target))
				{
					target = await source.GetValueAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
					reference = new WeakReference<T>(target);
				}
			}
		}
		return target;
	}
}
