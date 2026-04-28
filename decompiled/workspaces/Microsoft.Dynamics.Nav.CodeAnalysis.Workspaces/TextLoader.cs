using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces;

public abstract class TextLoader
{
	private class TextDocumentLoader : TextLoader
	{
		private readonly TextAndVersion textAndVersion;

		internal TextDocumentLoader(TextAndVersion textAndVersion)
		{
			this.textAndVersion = textAndVersion;
		}

		public override Task<TextAndVersion> LoadTextAndVersionAsync(Workspace workspace, DocumentId documentId, CancellationToken cancellationToken)
		{
			return Task.FromResult(textAndVersion);
		}
	}

	private class TextContainerLoader : TextLoader
	{
		private readonly SourceTextContainer container;

		private readonly VersionStamp version;

		private readonly string filePath;

		internal TextContainerLoader(SourceTextContainer container, VersionStamp version, string filePath)
		{
			this.container = container;
			this.version = version;
			this.filePath = filePath;
		}

		public override Task<TextAndVersion> LoadTextAndVersionAsync(Workspace workspace, DocumentId documentId, CancellationToken cancellationToken)
		{
			return Task.FromResult(TextAndVersion.Create(container.CurrentText, version, filePath));
		}
	}

	public abstract Task<TextAndVersion> LoadTextAndVersionAsync(Workspace workspace, DocumentId documentId, CancellationToken cancellationToken);

	public static TextLoader From(TextAndVersion textAndVersion)
	{
		if (textAndVersion == null)
		{
			throw new ArgumentNullException("textAndVersion");
		}
		return new TextDocumentLoader(textAndVersion);
	}

	public static TextLoader From(SourceTextContainer container, VersionStamp version, string filePath = null)
	{
		if (container == null)
		{
			throw new ArgumentNullException("container");
		}
		return new TextContainerLoader(container, version, filePath);
	}
}
