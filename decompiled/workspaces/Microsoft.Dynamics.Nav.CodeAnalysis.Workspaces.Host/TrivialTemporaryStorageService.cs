using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Host;

internal sealed class TrivialTemporaryStorageService : ITemporaryStorageService, IWorkspaceService
{
	private sealed class StreamStorage : ITemporaryStreamStorage, IDisposable
	{
		private MemoryStream internalStream;

		public void Dispose()
		{
			if (internalStream != null)
			{
				internalStream.Dispose();
			}
			internalStream = null;
		}

		public Stream ReadStream(CancellationToken cancellationToken = default(CancellationToken))
		{
			if (internalStream == null)
			{
				throw new InvalidOperationException();
			}
			internalStream.Position = 0L;
			return internalStream;
		}

		public Task<Stream> ReadStreamAsync(CancellationToken cancellationToken = default(CancellationToken))
		{
			if (internalStream == null)
			{
				throw new InvalidOperationException();
			}
			internalStream.Position = 0L;
			return Task.FromResult((Stream)internalStream);
		}

		public void WriteStream(Stream stream, CancellationToken cancellationToken = default(CancellationToken))
		{
			MemoryStream destination = new MemoryStream();
			stream.CopyTo(destination);
			internalStream = destination;
		}

		public async Task WriteStreamAsync(Stream stream, CancellationToken cancellationToken = default(CancellationToken))
		{
			MemoryStream newStream = new MemoryStream();
			await stream.CopyToAsync(newStream).ConfigureAwait(continueOnCapturedContext: false);
			internalStream = newStream;
		}
	}

	private sealed class TextStorage : ITemporaryTextStorage, IDisposable
	{
		private SourceText sourceText;

		public void Dispose()
		{
			sourceText = null;
		}

		public SourceText ReadText(CancellationToken cancellationToken = default(CancellationToken))
		{
			return sourceText;
		}

		public Task<SourceText> ReadTextAsync(CancellationToken cancellationToken = default(CancellationToken))
		{
			return Task.FromResult(ReadText(cancellationToken));
		}

		public void WriteText(SourceText text, CancellationToken cancellationToken = default(CancellationToken))
		{
			sourceText = text;
		}

		public Task WriteTextAsync(SourceText text, CancellationToken cancellationToken = default(CancellationToken))
		{
			WriteText(text, cancellationToken);
			return SpecializedTasks.EmptyTask;
		}
	}

	public ITemporaryStreamStorage CreateTemporaryStreamStorage(CancellationToken cancellationToken = default(CancellationToken))
	{
		return new StreamStorage();
	}

	public ITemporaryTextStorage CreateTemporaryTextStorage(CancellationToken cancellationToken = default(CancellationToken))
	{
		return new TextStorage();
	}
}
