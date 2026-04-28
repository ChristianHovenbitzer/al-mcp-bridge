using System.Collections.Immutable;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.CodeAnalysis;

namespace Microsoft.Dynamics.Nav.Deployment.ReferenceDownloader;

internal interface IPackageDownloader
{
	Task<ImmutableArray<SymbolReferenceSpecification>> DownloadPackages(ImmutableArray<SymbolReferenceSpecification> packages, string targetDirectory);

	Task<Stream> DownloadPackage(SymbolReferenceSpecification package);
}
