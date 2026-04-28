using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Utilities;

internal abstract class ValueSource<T>
{
	public static readonly ConstantValueSource<T> Empty = new ConstantValueSource<T>(default(T));

	public bool HasValue
	{
		get
		{
			T value;
			return TryGetValue(out value);
		}
	}

	public abstract bool TryGetValue(out T value);

	public abstract T GetValue(CancellationToken cancellationToken = default(CancellationToken));

	public abstract Task<T> GetValueAsync(CancellationToken cancellationToken = default(CancellationToken));
}
