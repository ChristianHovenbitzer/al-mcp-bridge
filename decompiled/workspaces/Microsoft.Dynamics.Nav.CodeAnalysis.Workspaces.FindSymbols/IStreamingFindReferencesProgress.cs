using System.Threading.Tasks;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.FindSymbols;

internal interface IStreamingFindReferencesProgress
{
	Task OnStartedAsync();

	Task OnCompletedAsync();

	Task OnFindInDocumentStartedAsync(Document document);

	Task OnFindInDocumentCompletedAsync(Document document);

	Task OnDefinitionFoundAsync(SymbolAndProjectId symbolAndProjectId);

	Task OnReferenceFoundAsync(SymbolAndProjectId symbolAndProjectId, ReferenceLocation location);

	Task ReportProgressAsync(int current, int maximum);
}
