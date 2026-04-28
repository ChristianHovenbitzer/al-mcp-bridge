using System.Collections.Immutable;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.CodeAnalysis;
using Microsoft.Dynamics.Nav.Deployment.ApiClients;

namespace Microsoft.Dynamics.Nav.Deployment.ReferenceDownloader;

internal class NavDevServerPackageDownloader : IPackageDownloader
{
	private readonly ConnectionOptions options;

	private readonly IEmitLogger logger;

	public NavDevServerPackageDownloader(ConnectionOptions options, IEmitLogger logger)
	{
		this.options = options;
		this.logger = logger;
	}

	public async Task<ImmutableArray<SymbolReferenceSpecification>> DownloadPackages(ImmutableArray<SymbolReferenceSpecification> packages, string targetDirectory)
	{
		return await new PackagesApiClient(options, logger, ServerRegistry.DevInstance).DownloadPackages(packages, targetDirectory).ConfigureAwait(continueOnCapturedContext: false);
	}

	public async Task<Stream> DownloadPackage(SymbolReferenceSpecification package)
	{
		return await new PackagesApiClient(options, logger, ServerRegistry.DevInstance).DownloadPackage(package).ConfigureAwait(continueOnCapturedContext: false);
	}
}
