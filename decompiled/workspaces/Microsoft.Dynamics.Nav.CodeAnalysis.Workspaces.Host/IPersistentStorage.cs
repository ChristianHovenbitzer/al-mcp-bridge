using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Host;

public interface IPersistentStorage : IDisposable
{
	Task<Stream> ReadStreamAsync(string name, CancellationToken cancellationToken = default(CancellationToken));

	Task<Stream> ReadStreamAsync(Project project, string name, CancellationToken cancellationToken = default(CancellationToken));

	Task<Stream> ReadStreamAsync(Document document, string name, CancellationToken cancellationToken = default(CancellationToken));

	Task<bool> WriteStreamAsync(string name, Stream stream, CancellationToken cancellationToken = default(CancellationToken));

	Task<bool> WriteStreamAsync(Project project, string name, Stream stream, CancellationToken cancellationToken = default(CancellationToken));

	Task<bool> WriteStreamAsync(Document document, string name, Stream stream, CancellationToken cancellationToken = default(CancellationToken));
}
