namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces;

public class DocumentDiagnostic : WorkspaceDiagnostic
{
	public DocumentId DocumentId { get; }

	public DocumentDiagnostic(WorkspaceDiagnosticKind kind, string message, DocumentId documentId)
		: base(kind, message)
	{
		DocumentId = documentId;
	}
}
