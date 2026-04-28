namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.FindSymbols;

public interface IFindReferencesProgress
{
	void OnStarted();

	void OnCompleted();

	void OnFindInDocumentStarted(Document document);

	void OnFindInDocumentCompleted(Document document);

	void OnDefinitionFound(ISymbol symbol);

	void OnReferenceFound(ISymbol symbol, ReferenceLocation location);

	void ReportProgress(int current, int maximum);
}
