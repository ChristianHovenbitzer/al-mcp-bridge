using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces;

internal class TextDocumentState
{
	private const double MaxDelaySecs = 1.0;

	private const int MaxRetries = 5;

	internal static readonly TimeSpan RetryDelay = TimeSpan.FromSeconds(0.2);

	protected internal SolutionServices SolutionServices { get; }

	protected ValueSource<TextAndVersion> TextSource { get; }

	public DocumentId Id => Info.Id;

	public string FilePath => Info.FilePath;

	public DocumentInfo Info { get; }

	public IReadOnlyList<string> Folders => Info.Folders;

	public string Name => Info.Name;

	protected TextDocumentState(SolutionServices solutionServices, DocumentInfo info, ValueSource<TextAndVersion> textSource)
	{
		SolutionServices = solutionServices;
		Info = info;
		TextSource = textSource;
	}

	public static TextDocumentState Create(DocumentInfo info, SolutionServices services)
	{
		ValueSource<TextAndVersion> textSource = ((info.TextLoader != null) ? CreateRecoverableText(info.TextLoader, info.Id, services, reportInvalidDataException: false) : CreateStrongText(TextAndVersion.Create(SourceText.From(string.Empty, Encoding.UTF8), VersionStamp.Default, info.FilePath)));
		info = info.WithTextLoader(null);
		return new TextDocumentState(services, info, textSource);
	}

	protected static ValueSource<TextAndVersion> CreateStrongText(TextAndVersion text)
	{
		return new ConstantValueSource<TextAndVersion>(text);
	}

	protected static ValueSource<TextAndVersion> CreateStrongText(TextLoader loader, DocumentId documentId, SolutionServices services, bool reportInvalidDataException)
	{
		TextLoader loader2 = loader;
		DocumentId documentId2 = documentId;
		SolutionServices services2 = services;
		return new AsyncLazy<TextAndVersion>((CancellationToken c) => LoadTextAsync(loader2, documentId2, services2, reportInvalidDataException, c), cacheResult: true);
	}

	protected static ValueSource<TextAndVersion> CreateRecoverableText(TextAndVersion text, SolutionServices services)
	{
		return new RecoverableTextAndVersion(CreateStrongText(text), services.TemporaryStorage);
	}

	protected static ValueSource<TextAndVersion> CreateRecoverableText(TextLoader loader, DocumentId documentId, SolutionServices services, bool reportInvalidDataException)
	{
		TextLoader loader2 = loader;
		DocumentId documentId2 = documentId;
		SolutionServices services2 = services;
		return new RecoverableTextAndVersion(new AsyncLazy<TextAndVersion>((CancellationToken c) => LoadTextAsync(loader2, documentId2, services2, reportInvalidDataException, c), cacheResult: false), services2.TemporaryStorage);
	}

	protected static async Task<TextAndVersion> LoadTextAsync(TextLoader loader, DocumentId documentId, SolutionServices services, bool reportInvalidDataException, CancellationToken cancellationToken)
	{
		int retries = 0;
		while (true)
		{
			try
			{
				using (ExceptionHelpers.SuppressFailFast())
				{
					return await loader.LoadTextAndVersionAsync(services.Workspace, documentId, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
				}
			}
			catch (OperationCanceledException)
			{
				throw;
			}
			catch (IOException ex2)
			{
				int num = retries + 1;
				retries = num;
				if (num > 5)
				{
					services.Workspace.OnWorkspaceFailed(new DocumentDiagnostic(WorkspaceDiagnosticKind.Failure, ex2.Message, documentId));
					return TextAndVersion.Create(SourceText.From(string.Empty, Encoding.UTF8), VersionStamp.Default, documentId.GetDebuggerDisplay());
				}
			}
			catch (InvalidDataException ex3)
			{
				if (reportInvalidDataException)
				{
					services.Workspace.OnWorkspaceFailed(new DocumentDiagnostic(WorkspaceDiagnosticKind.Failure, ex3.Message, documentId));
				}
				return TextAndVersion.Create(SourceText.From(string.Empty, Encoding.UTF8), VersionStamp.Default, documentId.GetDebuggerDisplay());
			}
			await Task.Delay(RetryDelay).ConfigureAwait(continueOnCapturedContext: false);
		}
	}

	public bool TryGetText(out SourceText text)
	{
		if (TextSource.TryGetValue(out TextAndVersion value))
		{
			text = value.Text;
			return true;
		}
		text = null;
		return false;
	}

	public bool TryGetTextVersion(out VersionStamp version)
	{
		if (TextSource is ITextVersionable textVersionable)
		{
			return textVersionable.TryGetTextVersion(out version);
		}
		if (TextSource.TryGetValue(out TextAndVersion value))
		{
			version = value.Version;
			return true;
		}
		version = default(VersionStamp);
		return false;
	}

	public async Task<SourceText> GetTextAsync(CancellationToken cancellationToken)
	{
		return (await TextSource.GetValueAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false)).Text;
	}

	public async Task<VersionStamp> GetTextVersionAsync(CancellationToken cancellationToken)
	{
		if (TryGetTextVersion(out var version))
		{
			return version;
		}
		if (TextSource.TryGetValue(out TextAndVersion value))
		{
			return value.Version;
		}
		return (await TextSource.GetValueAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false)).Version;
	}

	public TextDocumentState UpdateText(TextAndVersion newTextAndVersion, PreservationMode mode)
	{
		if (newTextAndVersion == null)
		{
			throw new ArgumentNullException("newTextAndVersion");
		}
		ValueSource<TextAndVersion> textSource = ((mode == PreservationMode.PreserveIdentity) ? CreateStrongText(newTextAndVersion) : CreateRecoverableText(newTextAndVersion, SolutionServices));
		return new TextDocumentState(SolutionServices, Info, textSource);
	}

	public TextDocumentState UpdateText(SourceText newText, PreservationMode mode)
	{
		if (newText == null)
		{
			throw new ArgumentNullException("newText");
		}
		VersionStamp newerVersion = GetNewerVersion();
		TextAndVersion newTextAndVersion = TextAndVersion.Create(newText, newerVersion, FilePath);
		return UpdateText(newTextAndVersion, mode);
	}

	public TextDocumentState UpdateText(TextLoader loader, PreservationMode mode)
	{
		if (loader == null)
		{
			throw new ArgumentNullException("loader");
		}
		ValueSource<TextAndVersion> textSource = ((mode == PreservationMode.PreserveIdentity) ? CreateStrongText(loader, Id, SolutionServices, reportInvalidDataException: false) : CreateRecoverableText(loader, Id, SolutionServices, reportInvalidDataException: false));
		return new TextDocumentState(SolutionServices, Info, textSource);
	}

	private VersionStamp GetNewerVersion()
	{
		if (TextSource.TryGetValue(out TextAndVersion value))
		{
			return value.Version.GetNewerVersion();
		}
		return VersionStamp.Create();
	}

	public virtual async Task<VersionStamp> GetTopLevelChangeTextVersionAsync(CancellationToken cancellationToken)
	{
		return (await TextSource.GetValueAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false)).Version;
	}
}
