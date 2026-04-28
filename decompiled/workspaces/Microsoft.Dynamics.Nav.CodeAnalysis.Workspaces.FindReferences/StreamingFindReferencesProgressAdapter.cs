using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.FindSymbols;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.FindReferences;

internal class StreamingFindReferencesProgressAdapter : IStreamingFindReferencesProgress
{
	private readonly IFindReferencesProgress progress;

	public StreamingFindReferencesProgressAdapter(IFindReferencesProgress progress)
	{
		this.progress = progress;
	}

	public Task OnCompletedAsync()
	{
		progress.OnCompleted();
		return SpecializedTasks.EmptyTask;
	}

	public Task OnDefinitionFoundAsync(SymbolAndProjectId symbolAndProjectId)
	{
		progress.OnDefinitionFound(symbolAndProjectId.Symbol);
		return SpecializedTasks.EmptyTask;
	}

	public Task OnFindInDocumentCompletedAsync(Document document)
	{
		progress.OnFindInDocumentCompleted(document);
		return SpecializedTasks.EmptyTask;
	}

	public Task OnFindInDocumentStartedAsync(Document document)
	{
		progress.OnFindInDocumentStarted(document);
		return SpecializedTasks.EmptyTask;
	}

	public Task OnReferenceFoundAsync(SymbolAndProjectId symbolAndProjectId, ReferenceLocation location)
	{
		progress.OnReferenceFound(symbolAndProjectId.Symbol, location);
		return SpecializedTasks.EmptyTask;
	}

	public Task OnStartedAsync()
	{
		progress.OnStarted();
		return SpecializedTasks.EmptyTask;
	}

	public Task ReportProgressAsync(int current, int maximum)
	{
		progress.ReportProgress(current, maximum);
		return SpecializedTasks.EmptyTask;
	}
}
