using System.Threading;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Host;

public interface ITemporaryStorageService : IWorkspaceService
{
	ITemporaryStreamStorage CreateTemporaryStreamStorage(CancellationToken cancellationToken = default(CancellationToken));

	ITemporaryTextStorage CreateTemporaryTextStorage(CancellationToken cancellationToken = default(CancellationToken));
}
