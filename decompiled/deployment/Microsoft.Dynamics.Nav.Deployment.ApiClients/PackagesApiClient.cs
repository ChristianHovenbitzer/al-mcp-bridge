using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.CodeAnalysis;
using Microsoft.Dynamics.Nav.CodeAnalysis.Packaging;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;
using Microsoft.Dynamics.Nav.Deployment.Http;

namespace Microsoft.Dynamics.Nav.Deployment.ApiClients;

internal class PackagesApiClient : ApiClient
{
	private static string SnapshotRelativePath = "snapshotdebugger/packages";

	private static string DevRelativePath = "dev/packages";

	private readonly IFileSystem fileSystem;

	private readonly bool snapshotConnection;

	private readonly ServerRegistry serverRegistry;

	protected string RelativeUri { get; set; }

	public PackagesApiClient(ConnectionOptions options, IEmitLogger logger, ServerRegistry serverRegistry, bool snapshotConnection = false)
		: this(FileSystem.Instance, options, logger, serverRegistry, snapshotConnection)
	{
	}

	public PackagesApiClient(IFileSystem fileSystem, ConnectionOptions options, IEmitLogger logger, ServerRegistry serverRegistry, bool snapshotConnection = false)
		: base(options, logger)
	{
		this.fileSystem = fileSystem;
		this.snapshotConnection = snapshotConnection;
		RelativeUri = (snapshotConnection ? SnapshotRelativePath : DevRelativePath);
		this.serverRegistry = serverRegistry;
	}

	public async Task<ImmutableArray<SymbolReferenceSpecification>> DownloadPackages(ImmutableArray<SymbolReferenceSpecification> references, string targetDir)
	{
		string targetDir2 = targetDir;
		HashSet<SymbolReferenceSpecification> result = new HashSet<SymbolReferenceSpecification>(SymbolReferenceSpecification.VersionLessEqualityComparer);
		IHttpClient client = await GetHttpClientFactory().Create(base.ConnectionOptions, base.Logger).ConfigureAwait(continueOnCapturedContext: false);
		List<Task<(bool Success, ImmutableArray<SymbolReferenceSpecification> Dependencies)>> tasksDownloadingDirectReferences = references.Select((SymbolReferenceSpecification x) => DownloadPackage(client, x, targetDir2)).ToList();
		await Task.WhenAll(tasksDownloadingDirectReferences).ConfigureAwait(continueOnCapturedContext: false);
		for (int i = 0; i < tasksDownloadingDirectReferences.Count; i++)
		{
			if (tasksDownloadingDirectReferences[i].Result.Success)
			{
				result.Add(references[i]);
			}
		}
		IEnumerable<SymbolReferenceSpecification> secondLevelDependencies = tasksDownloadingDirectReferences.Where((Task<(bool Success, ImmutableArray<SymbolReferenceSpecification> Dependencies)> s) => s.Result.Success).SelectMany((Task<(bool Success, ImmutableArray<SymbolReferenceSpecification> Dependencies)> d) => d.Result.Dependencies);
		await DownloadPropagatedDependencies(result, secondLevelDependencies, targetDir2, client).ConfigureAwait(continueOnCapturedContext: false);
		return result.ToImmutableArray();
	}

	private async Task DownloadPropagatedDependencies(HashSet<SymbolReferenceSpecification> firstLevelDependencies, IEnumerable<SymbolReferenceSpecification> secondLevelDependencies, string targetDir, IHttpClient client)
	{
		HashSet<SymbolReferenceSpecification> firstLevelDependencies2 = firstLevelDependencies;
		IHttpClient client2 = client;
		string targetDir2 = targetDir;
		if (!secondLevelDependencies.Any())
		{
			return;
		}
		IEnumerable<SymbolReferenceSpecification> source = (from d in secondLevelDependencies
			where !firstLevelDependencies2.Contains(d)
			select d into specification
			orderby specification.Version descending
			select specification).Distinct(SymbolReferenceSpecification.VersionLessEqualityComparer);
		base.Logger.Info("The following dependencies will be queried for propagated dependencies:{0}{1}", Environment.NewLine, string.Join(Environment.NewLine, source.Select((SymbolReferenceSpecification r) => r.ToDisplayString())));
		List<(SymbolReferenceSpecification Reference, Task<(bool Success, ImmutableArray<SymbolReferenceSpecification> Dependencies)> DownloadTask)> referenceAndTaskDownloadingReference = source.Select((SymbolReferenceSpecification x) => (Reference: x, DownloadTask: DownloadPackage(client2, x, targetDir2, isSecondLevelDependency: true))).ToList();
		await Task.WhenAll(referenceAndTaskDownloadingReference.Select<(SymbolReferenceSpecification, Task<(bool, ImmutableArray<SymbolReferenceSpecification>)>), Task<(bool, ImmutableArray<SymbolReferenceSpecification>)>>(((SymbolReferenceSpecification Reference, Task<(bool Success, ImmutableArray<SymbolReferenceSpecification> Dependencies)> DownloadTask) x) => x.DownloadTask).ToList()).ConfigureAwait(continueOnCapturedContext: false);
		IEnumerable<SymbolReferenceSpecification> source2 = from tuple in referenceAndTaskDownloadingReference
			where !tuple.DownloadTask.Result.Success
			select tuple.Reference;
		if (source2.Any())
		{
			base.Logger.Error("Failed to download the following references: {0}", string.Join(Environment.NewLine, source2.Select((SymbolReferenceSpecification r) => r.ToDisplayString())));
		}
	}

	public async Task<Stream> DownloadPackage(SymbolReferenceSpecification reference)
	{
		if (!reference.IsValid)
		{
			base.Logger.Info(DeploymentResources.InValidSymbolNotDownloaded, reference.Name ?? string.Empty, reference.Publisher ?? string.Empty, reference.Version?.ToString() ?? string.Empty);
			return null;
		}
		HttpResponseMessage obj = await SendRequest(await GetHttpClientFactory().Create(base.ConnectionOptions, base.Logger).ConfigureAwait(continueOnCapturedContext: false), reference).ConfigureAwait(continueOnCapturedContext: false);
		obj.EnsureSuccessStatusCode();
		return await obj.Content.ReadAsStreamAsync().ConfigureAwait(continueOnCapturedContext: false);
	}

	private async Task<HttpResponseMessage> SendRequest(IHttpClient client, SymbolReferenceSpecification reference)
	{
		Dictionary<string, string> dictionary = new Dictionary<string, string>
		{
			{ "publisher", reference.Publisher },
			{ "appName", reference.Name },
			{
				"versionText",
				reference.Version.ToString()
			}
		};
		if (!reference.IsApplicationConceptReference())
		{
			dictionary["appId"] = reference.AppId.ToString();
		}
		AddTenantIfNeeded(dictionary);
		AddDeploymentIdIfNeeded(dictionary);
		Uri uri = new Uri(RelativeUri + UriHelper.CreateQueryString(dictionary), UriKind.Relative);
		return await client.GetAsync(uri).ConfigureAwait(continueOnCapturedContext: false);
	}

	private async Task<(bool Success, ImmutableArray<SymbolReferenceSpecification> Dependencies)> DownloadPackage(IHttpClient client, SymbolReferenceSpecification specification, string directory, bool isSecondLevelDependency = false)
	{
		if (!specification.IsValid)
		{
			base.Logger.Info(DeploymentResources.InValidSymbolNotDownloaded, specification.Name ?? string.Empty, specification.Publisher ?? string.Empty, specification.Version?.ToString() ?? string.Empty);
			return (Success: false, Dependencies: ImmutableArray<SymbolReferenceSpecification>.Empty);
		}
		HttpResponseMessage httpResponseMessage = await SendRequest(client, specification);
		if (!httpResponseMessage.IsSuccessStatusCode)
		{
			return (Success: false, Dependencies: ImmutableArray<SymbolReferenceSpecification>.Empty);
		}
		string filePath = Path.Combine(directory, httpResponseMessage.Content.Headers.ContentDisposition.FileName.UnquoteIdentifier());
		using Stream content = await httpResponseMessage.Content.ReadAsStreamAsync().ConfigureAwait(continueOnCapturedContext: false);
		NavAppManifest navAppManifest = await WritePackageToFileIfNeeded(content, filePath, specification, isSecondLevelDependency);
		if (navAppManifest == null)
		{
			return (Success: false, Dependencies: ImmutableArray<SymbolReferenceSpecification>.Empty);
		}
		return (Success: true, Dependencies: navAppManifest.DependencyReferences.ToImmutableArrayOrEmpty());
	}

	private async Task<NavAppManifest?> WritePackageToFileIfNeeded(Stream content, string filepath, SymbolReferenceSpecification specification, bool isSecondLevelDependency)
	{
		MemoryStream manifestStream = new MemoryStream();
		await content.CopyToAsync(manifestStream).ConfigureAwait(continueOnCapturedContext: false);
		NavAppManifest manifest = ReadNavAppManifest(manifestStream);
		if (manifest == null)
		{
			return null;
		}
		if (isSecondLevelDependency && !specification.IsPropagated && !manifest.PropagateDependencies)
		{
			return manifest;
		}
		if (!(await TryWriteToFile(manifestStream, fileSystem, filepath).ConfigureAwait(continueOnCapturedContext: false)))
		{
			return null;
		}
		return manifest;
	}

	private static NavAppManifest ReadNavAppManifest(Stream manifestStream)
	{
		try
		{
			using NavAppPackageReader navAppPackageReader = NavAppPackageReader.Create(manifestStream, leaveOpen: true);
			return navAppPackageReader.ReadNavAppManifest();
		}
		catch (Exception ex)
		{
			LocalMachineLogger.LogError("An exception happened while reading the manifest from the package '{0}'. {1}", manifestStream, ex.Message);
			return null;
		}
	}
}
