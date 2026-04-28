using System;
using System.Threading;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Utilities;

internal static class CancellableLazy
{
	public static CancellableLazy<T> Create<T>(T value)
	{
		return new CancellableLazy<T>(value);
	}

	public static CancellableLazy<T> Create<T>(Func<CancellationToken, T> valueFactory)
	{
		return new CancellableLazy<T>(valueFactory);
	}
}
internal class CancellableLazy<T>
{
	private NonReentrantLock gate;

	private Func<CancellationToken, T> valueFactory;

	private T value;

	public bool HasValue
	{
		get
		{
			T val;
			return TryGetValue(out val);
		}
	}

	public CancellableLazy(Func<CancellationToken, T> valueFactory)
	{
		gate = new NonReentrantLock();
		this.valueFactory = valueFactory;
	}

	public CancellableLazy(T value)
	{
		this.value = value;
	}

	public bool TryGetValue(out T value)
	{
		if (valueFactory == null)
		{
			value = this.value;
			return true;
		}
		value = default(T);
		return false;
	}

	public T GetValue(CancellationToken cancellationToken = default(CancellationToken))
	{
		NonReentrantLock nonReentrantLock = gate;
		if (nonReentrantLock != null)
		{
			using (nonReentrantLock.DisposableWait(cancellationToken))
			{
				if (valueFactory != null)
				{
					value = valueFactory(cancellationToken);
					Interlocked.Exchange(ref valueFactory, null);
				}
				Interlocked.Exchange(ref gate, null);
			}
		}
		return value;
	}
}
