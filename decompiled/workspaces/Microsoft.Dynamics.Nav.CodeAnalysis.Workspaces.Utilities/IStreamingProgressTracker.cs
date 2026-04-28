using System.Threading.Tasks;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Utilities;

internal interface IStreamingProgressTracker
{
	int CompletedItems { get; }

	int TotalItems { get; }

	Task AddItemsAsync(int count);

	Task ItemCompletedAsync();

	Task ClearAsync();
}
