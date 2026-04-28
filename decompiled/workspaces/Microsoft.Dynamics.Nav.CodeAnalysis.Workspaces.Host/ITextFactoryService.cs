using System.IO;
using System.Text;
using System.Threading;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Host;

internal interface ITextFactoryService : IWorkspaceService
{
	SourceText CreateText(Stream stream, Encoding defaultEncoding, CancellationToken cancellationToken = default(CancellationToken));

	SourceText CreateText(TextReader reader, Encoding encoding, CancellationToken cancellationToken = default(CancellationToken));
}
