using System;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Utilities;

internal sealed class WeakConstantValueSource<T> : ValueSource<T> where T : class
{
	private readonly WeakReference<T> weakValue;

	public WeakConstantValueSource(T value)
	{
		weakValue = new WeakReference<T>(value);
	}

	public override T GetValue(CancellationToken cancellationToken)
	{
		if (weakValue != null && weakValue.TryGetTarget(out var target))
		{
			return target;
		}
		return null;
	}

	public override bool TryGetValue(out T value)
	{
		if (weakValue != null && weakValue.TryGetTarget(out value))
		{
			return true;
		}
		value = null;
		return false;
	}

	public override Task<T> GetValueAsync(CancellationToken cancellationToken)
	{
		return Task.FromResult(GetValue(cancellationToken));
	}
}
