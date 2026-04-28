using System;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Utilities;

internal class StreamingProgressTracker : IStreamingProgressTracker
{
	private readonly Func<int, int, Task> updateActionOpt;

	private int completedItems;

	private int totalItems;

	public int CompletedItems => completedItems;

	public int TotalItems => totalItems;

	public StreamingProgressTracker()
		: this(null)
	{
	}

	public StreamingProgressTracker(Func<int, int, Task> updateActionOpt)
	{
		this.updateActionOpt = updateActionOpt;
	}

	public Task AddItemsAsync(int count)
	{
		Interlocked.Add(ref totalItems, count);
		return UpdateAsync();
	}

	public Task ItemCompletedAsync()
	{
		Interlocked.Increment(ref completedItems);
		return UpdateAsync();
	}

	public Task ClearAsync()
	{
		totalItems = 0;
		completedItems = 0;
		return UpdateAsync();
	}

	private Task UpdateAsync()
	{
		if (updateActionOpt == null)
		{
			return SpecializedTasks.EmptyTask;
		}
		return updateActionOpt(completedItems, totalItems);
	}
}
