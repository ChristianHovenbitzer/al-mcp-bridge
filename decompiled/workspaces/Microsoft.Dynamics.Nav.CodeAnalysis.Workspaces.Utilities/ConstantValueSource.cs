using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Utilities;

internal sealed class ConstantValueSource<T> : ValueSource<T>
{
	private readonly T value;

	private Task<T> task;

	public ConstantValueSource(T value)
	{
		this.value = value;
	}

	public override T GetValue(CancellationToken cancellationToken = default(CancellationToken))
	{
		return value;
	}

	public override bool TryGetValue(out T getValue)
	{
		getValue = value;
		return true;
	}

	public override Task<T> GetValueAsync(CancellationToken cancellationToken = default(CancellationToken))
	{
		if (task == null)
		{
			Interlocked.CompareExchange(ref task, Task.FromResult(value), null);
		}
		return task;
	}
}
