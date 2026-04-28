using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces;

public class TextDocument
{
	private readonly TextDocumentState state;

	public Project Project { get; protected set; }

	public DocumentId Id => GetDocumentState().Id;

	public string FilePath => GetDocumentState().FilePath;

	public string Name => GetDocumentState().Name;

	public IReadOnlyList<string> Folders => GetDocumentState().Folders;

	internal virtual TextDocumentState GetDocumentState()
	{
		return state;
	}

	protected TextDocument()
	{
	}

	internal TextDocument(Project project, TextDocumentState state)
	{
		Contract.ThrowIfNull(project);
		Contract.ThrowIfNull(state);
		Project = project;
		this.state = state;
	}

	public bool TryGetText(out SourceText text)
	{
		return GetDocumentState().TryGetText(out text);
	}

	public bool TryGetTextVersion(out VersionStamp version)
	{
		return GetDocumentState().TryGetTextVersion(out version);
	}

	public Task<SourceText> GetTextAsync(CancellationToken cancellationToken = default(CancellationToken))
	{
		return GetDocumentState().GetTextAsync(cancellationToken);
	}

	public Task<VersionStamp> GetTextVersionAsync(CancellationToken cancellationToken = default(CancellationToken))
	{
		return GetDocumentState().GetTextVersionAsync(cancellationToken);
	}

	internal Task<VersionStamp> GetTopLevelChangeTextVersionAsync(CancellationToken cancellationToken = default(CancellationToken))
	{
		return GetDocumentState().GetTopLevelChangeTextVersionAsync(cancellationToken);
	}
}
