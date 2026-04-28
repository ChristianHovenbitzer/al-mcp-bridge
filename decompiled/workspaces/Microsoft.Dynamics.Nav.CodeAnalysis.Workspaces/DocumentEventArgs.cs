using System;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces;

public class DocumentEventArgs : EventArgs
{
	public Document Document { get; }

	public DocumentEventArgs(Document document)
	{
		Document = document;
	}
}
