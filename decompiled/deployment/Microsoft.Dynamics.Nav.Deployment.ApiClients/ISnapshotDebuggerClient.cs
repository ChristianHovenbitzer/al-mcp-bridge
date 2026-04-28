using System.Collections.Immutable;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.CodeAnalysis;
using Microsoft.Dynamics.Nav.DebuggerService;
using Microsoft.Dynamics.Nav.TypeWrappers;

namespace Microsoft.Dynamics.Nav.Deployment.ApiClients;

public interface ISnapshotDebuggerClient
{
	ConnectionOptions Options { get; }

	Task<(bool Success, string? Cookie, SnapshotDebuggerAttachKindWrapper AttachKind)> InitalizeAttachAsync(SnapshotDebuggerAttachPayloadWrapper? wrapper);

	Task<bool> FinishAttachAsync(FinishSnapshotDebuggerSessionPayloadWrapper? wrapper, IFileSystem fileSystem, string? snapshotFileDirectory, string? affinitCookie);

	Task<(bool Success, SnapshotDebuggerSessionStatusWrapper Status)> GetStatusAsync(SnapshotDebuggerSessionGetStatusPayloadWrapper? wrapper, string? affinitCookie);

	Task<ImmutableArray<SymbolReferenceSpecification>> DownloadPackages(ImmutableArray<SymbolReferenceSpecification> references, string targetDir);

	Task<Stream> DownloadPackage(SymbolReferenceSpecification reference);
}
