using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Host;

public interface ITemporaryTextStorage : IDisposable
{
	SourceText ReadText(CancellationToken cancellationToken = default(CancellationToken));

	Task<SourceText> ReadTextAsync(CancellationToken cancellationToken = default(CancellationToken));

	void WriteText(SourceText text, CancellationToken cancellationToken = default(CancellationToken));

	Task WriteTextAsync(SourceText text, CancellationToken cancellationToken = default(CancellationToken));
}
