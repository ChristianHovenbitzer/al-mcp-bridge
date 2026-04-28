using System;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Utilities;

internal static class SemaphoreSlimExtensions
{
	internal struct SemaphoreDisposer : IDisposable
	{
		private readonly SemaphoreSlim semaphore;

		public SemaphoreDisposer(SemaphoreSlim semaphore)
		{
			this.semaphore = semaphore;
		}

		public void Dispose()
		{
			semaphore.Release();
		}
	}

	public static SemaphoreDisposer DisposableWait(this SemaphoreSlim semaphore, CancellationToken cancellationToken = default(CancellationToken))
	{
		semaphore.Wait(cancellationToken);
		return new SemaphoreDisposer(semaphore);
	}

	public static async Task<SemaphoreDisposer> DisposableWaitAsync(this SemaphoreSlim semaphore, CancellationToken cancellationToken = default(CancellationToken))
	{
		await semaphore.WaitAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		return new SemaphoreDisposer(semaphore);
	}
}
