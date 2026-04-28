using System.Threading;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces;

internal class BranchId
{
	private static int nextId;

	internal int Id { get; }

	private BranchId(int id)
	{
		Id = id;
	}

	internal static BranchId GetNextId()
	{
		return new BranchId(Interlocked.Increment(ref nextId));
	}
}
