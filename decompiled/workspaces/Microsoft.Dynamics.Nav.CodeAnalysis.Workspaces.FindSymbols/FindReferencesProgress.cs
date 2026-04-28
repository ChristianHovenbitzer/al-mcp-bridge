namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.FindSymbols;

internal class FindReferencesProgress : IFindReferencesProgress
{
	public static readonly IFindReferencesProgress Instance = new FindReferencesProgress();

	private FindReferencesProgress()
	{
	}

	public void ReportProgress(int current, int maximum)
	{
	}

	public void OnCompleted()
	{
	}

	public void OnStarted()
	{
	}

	public void OnDefinitionFound(ISymbol symbol)
	{
	}

	public void OnReferenceFound(ISymbol symbol, ReferenceLocation location)
	{
	}

	public void OnFindInDocumentStarted(Document document)
	{
	}

	public void OnFindInDocumentCompleted(Document document)
	{
	}
}
