using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.FindSymbols;

internal class StreamingFindReferencesProgress : IStreamingFindReferencesProgress
{
	public static readonly IStreamingFindReferencesProgress Instance = new StreamingFindReferencesProgress();

	private StreamingFindReferencesProgress()
	{
	}

	public Task ReportProgressAsync(int current, int maximum)
	{
		return SpecializedTasks.EmptyTask;
	}

	public Task OnCompletedAsync()
	{
		return SpecializedTasks.EmptyTask;
	}

	public Task OnStartedAsync()
	{
		return SpecializedTasks.EmptyTask;
	}

	public Task OnDefinitionFoundAsync(SymbolAndProjectId symbol)
	{
		return SpecializedTasks.EmptyTask;
	}

	public Task OnReferenceFoundAsync(SymbolAndProjectId symbol, ReferenceLocation location)
	{
		return SpecializedTasks.EmptyTask;
	}

	public Task OnFindInDocumentStartedAsync(Document document)
	{
		return SpecializedTasks.EmptyTask;
	}

	public Task OnFindInDocumentCompletedAsync(Document document)
	{
		return SpecializedTasks.EmptyTask;
	}
}
