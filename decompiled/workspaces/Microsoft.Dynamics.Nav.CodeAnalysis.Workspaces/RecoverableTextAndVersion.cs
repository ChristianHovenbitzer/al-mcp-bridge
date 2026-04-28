using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Host;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Log;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces;

internal class RecoverableTextAndVersion : ValueSource<TextAndVersion>, ITextVersionable, IDisposable
{
	private sealed class RecoverableText : RecoverableWeakValueSource<SourceText>
	{
		private readonly RecoverableTextAndVersion _parent;

		private ITemporaryTextStorage _storage;

		public RecoverableText(RecoverableTextAndVersion parent, SourceText text)
			: base((ValueSource<SourceText>)new ConstantValueSource<SourceText>(text))
		{
			_parent = parent;
		}

		protected override async Task<SourceText> RecoverAsync(CancellationToken cancellationToken)
		{
			Contract.ThrowIfNull(_storage);
			using (Logger.LogBlock(FunctionId.Workspace_Recoverable_RecoverTextAsync, _parent.filePath, cancellationToken))
			{
				return await _storage.ReadTextAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
			}
		}

		protected override SourceText Recover(CancellationToken cancellationToken)
		{
			Contract.ThrowIfNull(_storage);
			using (Logger.LogBlock(FunctionId.Workspace_Recoverable_RecoverText, _parent.filePath, cancellationToken))
			{
				return _storage.ReadText(cancellationToken);
			}
		}

		protected override Task SaveAsync(SourceText text, CancellationToken cancellationToken)
		{
			Contract.ThrowIfFalse(_storage == null);
			_storage = _parent.storageService.CreateTemporaryTextStorage(CancellationToken.None);
			return _storage.WriteTextAsync(text);
		}
	}

	private readonly ITemporaryStorageService storageService;

	private SemaphoreSlim gateDoNotAccessDirectly;

	private ValueSource<TextAndVersion> initialSource;

	private RecoverableText text;

	private VersionStamp version;

	private string filePath;

	private bool disposedValue;

	private SemaphoreSlim Gate => LazyInitialization.EnsureInitialized(ref gateDoNotAccessDirectly, SemaphoreSlimFactory.Instance);

	public RecoverableTextAndVersion(ValueSource<TextAndVersion> initialTextAndVersion, ITemporaryStorageService storageService)
	{
		initialSource = initialTextAndVersion;
		this.storageService = storageService;
	}

	public override bool TryGetValue(out TextAndVersion value)
	{
		if (text != null && text.TryGetValue(out SourceText value2))
		{
			value = TextAndVersion.Create(value2, version, filePath);
			return true;
		}
		value = null;
		return false;
	}

	public bool TryGetTextVersion(out VersionStamp result)
	{
		result = version;
		if (result == default(VersionStamp) && TryGetValue(out TextAndVersion value))
		{
			result = value.Version;
		}
		return result != default(VersionStamp);
	}

	public override TextAndVersion GetValue(CancellationToken cancellationToken = default(CancellationToken))
	{
		if (text == null)
		{
			using (Gate.DisposableWait(cancellationToken))
			{
				if (text == null)
				{
					return InitRecoverable(initialSource.GetValue(cancellationToken));
				}
			}
		}
		return TextAndVersion.Create(text.GetValue(cancellationToken), version, filePath);
	}

	public override async Task<TextAndVersion> GetValueAsync(CancellationToken cancellationToken = default(CancellationToken))
	{
		if (text == null)
		{
			using (Gate.DisposableWait(cancellationToken))
			{
				if (text == null)
				{
					return InitRecoverable(await initialSource.GetValueAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false));
				}
			}
		}
		return TextAndVersion.Create(await text.GetValueAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false), version, filePath);
	}

	private TextAndVersion InitRecoverable(TextAndVersion textAndVersion)
	{
		initialSource = null;
		version = textAndVersion.Version;
		filePath = textAndVersion.FilePath;
		text = new RecoverableText(this, textAndVersion.Text);
		text.GetValue(CancellationToken.None);
		return textAndVersion;
	}

	protected virtual void Dispose(bool disposing)
	{
		if (disposedValue)
		{
			return;
		}
		if (disposing)
		{
			if (gateDoNotAccessDirectly != null)
			{
				gateDoNotAccessDirectly.Dispose();
			}
			if (text != null)
			{
				text.Dispose();
			}
		}
		disposedValue = true;
	}

	public void Dispose()
	{
		Dispose(disposing: true);
	}
}
