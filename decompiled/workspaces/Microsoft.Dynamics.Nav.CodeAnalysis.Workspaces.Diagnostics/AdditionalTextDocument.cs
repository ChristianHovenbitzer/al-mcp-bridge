using System;
using System.Threading;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Diagnostics;

internal sealed class AdditionalTextDocument : AdditionalText
{
	private readonly TextDocumentState document;

	public override string Path => document.FilePath ?? document.Name;

	public AdditionalTextDocument(TextDocumentState document)
	{
		if (document == null)
		{
			throw new ArgumentNullException("document");
		}
		this.document = document;
	}

	public override SourceText GetText(CancellationToken cancellationToken = default(CancellationToken))
	{
		return document.GetTextAsync(cancellationToken).WaitAndGetResult(cancellationToken);
	}
}
