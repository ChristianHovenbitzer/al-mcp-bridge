using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Host;

public interface ITemporaryStreamStorage : IDisposable
{
	Stream ReadStream(CancellationToken cancellationToken = default(CancellationToken));

	Task<Stream> ReadStreamAsync(CancellationToken cancellationToken = default(CancellationToken));

	void WriteStream(Stream stream, CancellationToken cancellationToken = default(CancellationToken));

	Task WriteStreamAsync(Stream stream, CancellationToken cancellationToken = default(CancellationToken));
}
