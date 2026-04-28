using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.CodeAnalysis;
using Microsoft.Dynamics.Nav.CodeAnalysis.Packaging;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.Deployment.ReferenceDownloader;

internal class NuGetPackageDownloader : IPackageDownloader, IDisposable
{
	private class PackageSearchResult
	{
		public string Id { get; set; } = string.Empty;


		public string Version { get; set; } = string.Empty;


		public string Publisher { get; set; } = string.Empty;

	}

	private sealed class AppManifestSummary
	{
		public SymbolReferenceSpecification PackageSpec { get; }

		public List<SymbolReferenceSpecification> Dependencies { get; }

		public bool PropagateDependencies { get; }

		public AppManifestSummary(SymbolReferenceSpecification packageSpecification, List<SymbolReferenceSpecification> dependencySpecifications, bool propagateDependencies)
		{
			PackageSpec = packageSpecification;
			Dependencies = dependencySpecifications;
			PropagateDependencies = propagateDependencies;
		}
	}

	private sealed record PackageProcessResult(bool Success, bool WasDownloaded, IReadOnlyList<SymbolReferenceSpecification> Dependencies, bool PropagateDependencies);

	private const string MicrosoftSymbolsFeedUrl = "https://pkgs.dev.azure.com/dynamicssmb2/DynamicsBCPublicFeeds/_packaging/MSSymbolsV2";

	private const string AppSourceSymbolsFeedUrl = "https://pkgs.dev.azure.com/dynamicssmb2/DynamicsBCPublicFeeds/_packaging/AppSourceSymbols";

	private const string ApplicationPackageName = "Microsoft.Application";

	private const string SystemPackageName = "Microsoft.Platform";

	private const string W1 = "w1";

	private readonly IEmitLogger logger;

	private readonly HttpClient httpClient;

	private readonly Dictionary<string, string> resolvedNuGetIds = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

	private readonly ConcurrentDictionary<string, string> cachedServiceIndexes = new ConcurrentDictionary<string, string>(StringComparer.OrdinalIgnoreCase);

	private readonly Dictionary<string, List<PackageSearchResult>> searchCache = new Dictionary<string, List<PackageSearchResult>>(StringComparer.OrdinalIgnoreCase);

	private readonly object manifestsLock = new object();

	private readonly string symbolsCountryRegion;

	private readonly IReadOnlyList<string> privateFeeds;

	private readonly bool useOnlyCustomFeeds;

	public NuGetPackageDownloader(IEmitLogger logger, string symbolsCountryRegion = "w1", IReadOnlyList<string>? privateFeeds = null, bool useOnlyCustomFeeds = false)
	{
		this.logger = logger ?? throw new ArgumentNullException("logger");
		this.symbolsCountryRegion = symbolsCountryRegion ?? "w1";
		this.privateFeeds = privateFeeds ?? Array.Empty<string>();
		this.useOnlyCustomFeeds = useOnlyCustomFeeds;
		HttpClientHandler httpClientHandler = new HttpClientHandler
		{
			UseCookies = false,
			AllowAutoRedirect = true,
			MaxAutomaticRedirections = 10
		};
		httpClient = InitializeHttpClient(httpClientHandler);
	}

	public NuGetPackageDownloader(IEmitLogger logger, string symbolsCountryRegion, HttpClient httpClient, IReadOnlyList<string>? privateFeeds = null, bool useOnlyCustomFeeds = false)
		: this(logger, symbolsCountryRegion, privateFeeds, useOnlyCustomFeeds)
	{
		this.httpClient = httpClient;
	}

	public async Task<ImmutableArray<SymbolReferenceSpecification>> DownloadPackages(ImmutableArray<SymbolReferenceSpecification> packages, string targetDirectory)
	{
		string targetDirectory2 = targetDirectory;
		if (packages.IsDefaultOrEmpty)
		{
			logger?.Info("No packages to download");
			return ImmutableArray<SymbolReferenceSpecification>.Empty;
		}
		PrepareTargetDirectory(targetDirectory2);
		LogDirectDependencies(packages);
		Dictionary<string, AppManifestSummary> existingManifests = ScanExistingAppManifests(targetDirectory2);
		List<SymbolReferenceSpecification> successful = new List<SymbolReferenceSpecification>();
		List<string> failedDisplayNames = new List<string>();
		List<string> newlyDownloadedDisplayNames = new List<string>();
		ImmutableArray<SymbolReferenceSpecification> immutableArray = AdjustPlatformVersions(packages);
		Queue<(SymbolReferenceSpecification spec, bool resolveDependencies)> queue = new Queue<(SymbolReferenceSpecification, bool)>(immutableArray.Select((SymbolReferenceSpecification p) => (p: p, true)));
		HashSet<string> processedKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
		new HashSet<string>(packages.Select((SymbolReferenceSpecification p) => p.GetKeyString()), StringComparer.OrdinalIgnoreCase);
		while (queue.Count > 0)
		{
			List<(SymbolReferenceSpecification spec, bool resolveDeps)> batch = new List<(SymbolReferenceSpecification, bool)>();
			while (batch.Count < 5 && queue.Count > 0)
			{
				var (symbolReferenceSpecification, item2) = queue.Dequeue();
				if (processedKeys.Add(symbolReferenceSpecification.GetKeyString()))
				{
					batch.Add((symbolReferenceSpecification, item2));
				}
			}
			if (batch.Count == 0)
			{
				continue;
			}
			PackageProcessResult[] array = await Task.WhenAll(batch.Select<(SymbolReferenceSpecification, bool), Task<PackageProcessResult>>(((SymbolReferenceSpecification spec, bool resolveDeps) item) => ProcessPackageAsync(item.spec, item.resolveDeps, existingManifests, targetDirectory2))).ConfigureAwait(continueOnCapturedContext: false);
			for (int i = 0; i < batch.Count; i++)
			{
				(SymbolReferenceSpecification spec, bool resolveDeps) tuple2 = batch[i];
				SymbolReferenceSpecification item3 = tuple2.spec;
				bool item4 = tuple2.resolveDeps;
				PackageProcessResult packageProcessResult = array[i];
				try
				{
					if (!packageProcessResult.Success)
					{
						failedDisplayNames.Add(GetPackageDisplayName(item3));
						continue;
					}
					successful.Add(item3);
					if (packageProcessResult.WasDownloaded)
					{
						string packageDisplayName = GetPackageDisplayName(item3);
						newlyDownloadedDisplayNames.Add(packageDisplayName);
						logger?.Info("Downloaded package: " + packageDisplayName);
					}
					EnqueueSecondLevelDependencies(queue, processedKeys, packageProcessResult, item4, item3);
				}
				catch (Exception ex)
				{
					failedDisplayNames.Add(GetPackageDisplayName(item3));
					logger?.Error("Error processing " + GetPackageDisplayName(item3) + ": " + ex.Message);
				}
			}
		}
		LogResults(successful, failedDisplayNames, newlyDownloadedDisplayNames);
		return successful.ToImmutableArray();
	}

	private static HttpClient InitializeHttpClient(HttpClientHandler httpClientHandler)
	{
		return new HttpClient(httpClientHandler)
		{
			DefaultRequestHeaders = 
			{
				{ "User-Agent", "AL-NuGet-Downloader/1.0" },
				{ "Accept", "application/octet-stream, */*" }
			}
		};
	}

	private void PrepareTargetDirectory(string targetDirectory)
	{
		Directory.CreateDirectory(targetDirectory);
		logger?.Info("Starting download of packages to " + targetDirectory);
	}

	private void LogDirectDependencies(IEnumerable<SymbolReferenceSpecification> packages)
	{
		logger?.Info("Direct dependencies to download:");
		foreach (SymbolReferenceSpecification package in packages)
		{
			logger?.Info($"  - {GetPackageDisplayName(package)} (Publisher: {package.Publisher}, AppId: {package.AppId})");
		}
	}

	private async Task<PackageProcessResult> ProcessPackageAsync(SymbolReferenceSpecification packageSpec, bool resolveDependencies, Dictionary<string, AppManifestSummary> existingManifests, string targetDirectory)
	{
		string key = packageSpec.GetKeyString();
		if (GetCachedManifest(packageSpec, key, resolveDependencies, existingManifests, out IReadOnlyList<SymbolReferenceSpecification> dependencies, out bool propagateDependencies))
		{
			return new PackageProcessResult(Success: true, WasDownloaded: false, dependencies, propagateDependencies);
		}
		(bool, List<SymbolReferenceSpecification>, bool, List<string>)? tuple = await DownloadPackageAndExtractManifest(packageSpec, targetDirectory).ConfigureAwait(continueOnCapturedContext: false);
		if (tuple.HasValue && tuple.GetValueOrDefault().Item1)
		{
			UpdateInMemoryManifestFromPackage(targetDirectory, packageSpec, existingManifests, tuple.Value.Item4);
			if (resolveDependencies)
			{
				logger?.Info($"Package {GetPackageDisplayName(packageSpec)} manifest: Dependencies={tuple.Value.Item2.Count}, PropagateDependencies={tuple.Value.Item3}");
				if (tuple.Value.Item2.Count > 0)
				{
					logger?.Info("  Dependencies found:");
					foreach (SymbolReferenceSpecification item in tuple.Value.Item2)
					{
						logger?.Info("    - " + GetPackageDisplayName(item));
					}
				}
			}
			IReadOnlyList<SymbolReferenceSpecification> dependencies2;
			if (!resolveDependencies)
			{
				IReadOnlyList<SymbolReferenceSpecification> readOnlyList = Array.Empty<SymbolReferenceSpecification>();
				dependencies2 = readOnlyList;
			}
			else
			{
				IReadOnlyList<SymbolReferenceSpecification> readOnlyList = tuple.Value.Item2;
				dependencies2 = readOnlyList;
			}
			return new PackageProcessResult(Success: true, WasDownloaded: true, dependencies2, resolveDependencies && tuple.Value.Item3);
		}
		if (GetCachedManifest(packageSpec, key, resolveDependencies, existingManifests, out IReadOnlyList<SymbolReferenceSpecification> dependencies3, out bool propagateDependencies2))
		{
			logger?.Info("Package " + GetPackageDisplayName(packageSpec) + " found in local cache despite resolution failure");
			return new PackageProcessResult(Success: true, WasDownloaded: false, dependencies3, propagateDependencies2);
		}
		return new PackageProcessResult(Success: false, WasDownloaded: false, Array.Empty<SymbolReferenceSpecification>(), PropagateDependencies: false);
	}

	private bool GetCachedManifest(SymbolReferenceSpecification spec, string key, bool resolveDependencies, Dictionary<string, AppManifestSummary> existingManifests, out IReadOnlyList<SymbolReferenceSpecification> dependencies, out bool propagateDependencies)
	{
		dependencies = Array.Empty<SymbolReferenceSpecification>();
		propagateDependencies = false;
		AppManifestSummary value = null;
		existingManifests.TryGetValue(key, out value);
		if (value == null || value.PackageSpec.Version < spec.Version)
		{
			return false;
		}
		if (resolveDependencies)
		{
			dependencies = value.Dependencies;
			propagateDependencies = value.PropagateDependencies;
			logger?.Info($"Using cached manifest for {GetPackageDisplayName(spec)}: Dependencies={dependencies.Count}, PropagateDependencies={propagateDependencies}");
		}
		return true;
	}

	private ImmutableArray<SymbolReferenceSpecification> AdjustPlatformVersions(ImmutableArray<SymbolReferenceSpecification> packages)
	{
		SymbolReferenceSpecification applicationPackage = packages.FirstOrDefault((SymbolReferenceSpecification p) => p.IsApplicationSymbolReference());
		if (applicationPackage == null)
		{
			return packages;
		}
		return packages.Select(delegate(SymbolReferenceSpecification p)
		{
			if (p.IsPlatformSymbolReference() && p.Version < applicationPackage.Version)
			{
				Version version = new Version(applicationPackage.Version.Major, applicationPackage.Version.Minor);
				logger?.Info($"Adjusted Platform package version from {p.Version} to {version} to match Application version");
				return new SymbolReferenceSpecification(p.Publisher, p.Name, version, exact: false, p.AppId, isPropagated: false, p.AlternateIds);
			}
			return p;
		}).ToImmutableArray();
	}

	private void EnqueueSecondLevelDependencies(Queue<(SymbolReferenceSpecification spec, bool resolveDependencies)> queue, HashSet<string> processedKeys, PackageProcessResult processResult, bool originalResolveFlag, SymbolReferenceSpecification parentPackageSpec)
	{
		if (!originalResolveFlag || processResult.Dependencies.Count == 0 || !processResult.PropagateDependencies)
		{
			return;
		}
		logger?.Info($"Package has PropagateDependencies=true, enqueuing {processResult.Dependencies.Count} transitive dependencies");
		ImmutableArray<SymbolReferenceSpecification>.Enumerator enumerator = AdjustPlatformVersions(processResult.Dependencies.ToImmutableArray()).GetEnumerator();
		while (enumerator.MoveNext())
		{
			SymbolReferenceSpecification current = enumerator.Current;
			if (processedKeys.Contains(current.GetKeyString()))
			{
				logger?.Info("  Skipping already processed: " + GetPackageDisplayName(current));
				continue;
			}
			logger?.Info("  Enqueuing propagated dependency: " + GetPackageDisplayName(current));
			queue.Enqueue((current, false));
		}
	}

	private Dictionary<string, AppManifestSummary> ScanExistingAppManifests(string targetDirectory)
	{
		Dictionary<string, AppManifestSummary> dictionary = new Dictionary<string, AppManifestSummary>(StringComparer.OrdinalIgnoreCase);
		if (!Directory.Exists(targetDirectory))
		{
			return dictionary;
		}
		try
		{
			string[] files = Directory.GetFiles(targetDirectory, "*.app", SearchOption.TopDirectoryOnly);
			foreach (string path in files)
			{
				try
				{
					using FileStream appStream = File.OpenRead(path);
					AppManifestSummary appManifestSummary = ReadManifest(appStream, leaveOpen: false);
					if (appManifestSummary != null)
					{
						string keyString = appManifestSummary.PackageSpec.GetKeyString();
						dictionary[keyString] = appManifestSummary;
					}
				}
				catch (Exception ex)
				{
					logger?.Error("Failed reading manifest '" + Path.GetFileName(path) + "': " + ex.Message);
				}
			}
		}
		catch (Exception ex2)
		{
			logger?.Error("Error scanning existing apps: " + ex2.Message);
		}
		return dictionary;
	}

	private void UpdateInMemoryManifestFromPackage(string targetDirectory, SymbolReferenceSpecification packageSpecification, Dictionary<string, AppManifestSummary> existingManifestMap, IEnumerable<string> packageFilePaths)
	{
		foreach (string packageFilePath in packageFilePaths)
		{
			try
			{
				using FileStream appStream = File.OpenRead(packageFilePath);
				AppManifestSummary appManifestSummary = ReadManifest(appStream, leaveOpen: false);
				if (appManifestSummary == null)
				{
					continue;
				}
				string keyString = appManifestSummary.PackageSpec.GetKeyString();
				lock (manifestsLock)
				{
					if (!existingManifestMap.ContainsKey(keyString))
					{
						existingManifestMap[keyString] = new AppManifestSummary(appManifestSummary.PackageSpec, appManifestSummary.Dependencies, appManifestSummary.PropagateDependencies);
					}
				}
			}
			catch (Exception ex)
			{
				logger?.Error("Failed to update existing extensions from '" + Path.GetFileName(packageFilePath) + "': " + ex.Message);
			}
		}
	}

	private async Task<(bool Success, List<SymbolReferenceSpecification> Dependencies, bool PropagateDependencies, List<string> ExtractedFiles)?> DownloadPackageAndExtractManifest(SymbolReferenceSpecification packageSpecification, string targetDirectory)
	{
		Stream stream = await FetchPackageStreamAsync(packageSpecification).ConfigureAwait(continueOnCapturedContext: false);
		if (stream == null)
		{
			return null;
		}
		List<string> list = ExtractAppFromNuGetPackage(stream, packageSpecification, targetDirectory);
		if (list.Count == 0)
		{
			return null;
		}
		stream.Position = 0L;
		var (item, item2) = GetDependencies(list, packageSpecification);
		return (true, item, item2, list);
	}

	private async Task<Stream?> FetchPackageStreamAsync(SymbolReferenceSpecification packageSpecification)
	{
		Stream stream = await DownloadPackage(packageSpecification).ConfigureAwait(continueOnCapturedContext: false);
		if (stream == Stream.Null || stream.Length == 0L)
		{
			return null;
		}
		return stream;
	}

	private (List<SymbolReferenceSpecification> Dependencies, bool PropagateDependencies) GetDependencies(List<string> packagePaths, SymbolReferenceSpecification packageSpecification)
	{
		(List<SymbolReferenceSpecification>, bool)? tuple = ExtractDependenciesFromPackage(packagePaths, packageSpecification);
		if (!tuple.HasValue)
		{
			return (Dependencies: new List<SymbolReferenceSpecification>(), PropagateDependencies: false);
		}
		return (Dependencies: tuple.Value.Item1, PropagateDependencies: tuple.Value.Item2);
	}

	private async Task<SymbolReferenceSpecification?> ResolvePackageVersion(SymbolReferenceSpecification packageSpecification)
	{
		(string, List<PackageSearchResult>)? tuple = await ResolvePackageIdByAppIdAsync(packageSpecification).ConfigureAwait(continueOnCapturedContext: false);
		if (!tuple.HasValue || tuple.Value.Item1 == null)
		{
			logger?.Info("Package ID not resolved for " + GetPackageDisplayName(packageSpecification) + " - package may already exist locally");
			return null;
		}
		string resolvedId = tuple.Value.Item1;
		List<PackageSearchResult> item = tuple.Value.Item2;
		UpdateResolvedNugetIds(packageSpecification, resolvedId);
		List<PackageSearchResult> list = ((!item.Any()) ? (await SearchPackagesAsync(resolvedId, packageSpecification.Publisher).ConfigureAwait(continueOnCapturedContext: false)) : item);
		List<PackageSearchResult> source = list;
		Version requestedVersion = packageSpecification.Version;
		Version version = (from candidate in source
			where candidate.Id.Equals(resolvedId, StringComparison.OrdinalIgnoreCase)
			select ParseVersionOrDefault(candidate.Version) into versionCandidate
			where versionCandidate != null && versionCandidate.Equals(requestedVersion)
			select versionCandidate).FirstOrDefault();
		if (version == null)
		{
			logger?.Info($"Exact version {requestedVersion} not found for {resolvedId}, looking for compatible version with same major version");
			version = (from candidate in source
				where candidate.Id.Equals(resolvedId, StringComparison.OrdinalIgnoreCase)
				select ParseVersionOrDefault(candidate.Version) into versionCandidate
				where versionCandidate != null && versionCandidate.Major == requestedVersion.Major
				orderby versionCandidate descending
				select versionCandidate).FirstOrDefault();
		}
		if (version == null)
		{
			logger?.Info($"Version {requestedVersion} not resolved for package {resolvedId} - package may already exist locally");
			return null;
		}
		return new SymbolReferenceSpecification(packageSpecification.Publisher, packageSpecification.Name, version, exact: false, packageSpecification.AppId, isPropagated: false, packageSpecification.AlternateIds);
	}

	private string GetSpecialPackageName(SymbolReferenceSpecification packageSpec)
	{
		if (packageSpec.IsPlatformSymbolReference())
		{
			return "Microsoft.Platform";
		}
		if (packageSpec.IsApplicationSymbolReference())
		{
			return "Microsoft.Application";
		}
		throw new Exception("Invalid package name is encountered.");
	}

	private static bool IsSpecialCasePackage(SymbolReferenceSpecification packageSpecification)
	{
		if (!packageSpecification.IsPlatformSymbolReference())
		{
			return packageSpecification.IsApplicationSymbolReference();
		}
		return true;
	}

	private string BuildNuGetPackageUrl(SymbolReferenceSpecification packageSpecification, string feedUrl)
	{
		if (!resolvedNuGetIds.TryGetValue(packageSpecification.GetKeyString(), out string value))
		{
			throw new InvalidOperationException($"Package ID not resolved for {GetPackageDisplayName(packageSpecification)}. AppId: {packageSpecification.AppId}");
		}
		string value2 = value.ToLowerInvariant();
		return $"{feedUrl.TrimEnd('/')}/nuget/v3/flat2/{value2}/{packageSpecification.Version}/{value2}.{packageSpecification.Version}.nupkg";
	}

	private async Task<List<PackageSearchResult>> SearchPackagesAsync(string searchTerm, string publisher)
	{
		string cacheKey = searchTerm + "|" + publisher;
		if (searchCache.TryGetValue(cacheKey, out List<PackageSearchResult> value))
		{
			logger?.Info($"Using cached search results for '{searchTerm}' ({value.Count} packages)");
			return value;
		}
		IReadOnlyList<string> feedUrlsForPublisher = GetFeedUrlsForPublisher(publisher);
		foreach (string feedUrl in feedUrlsForPublisher)
		{
			try
			{
				string indexUrl = feedUrl.TrimEnd('/') + "/nuget/v3/index.json";
				if (cachedServiceIndexes.TryGetValue(indexUrl, out string value2))
				{
					goto IL_031d;
				}
				HttpResponseMessage httpResponseMessage = await httpClient.GetAsync(indexUrl).ConfigureAwait(continueOnCapturedContext: false);
				if (httpResponseMessage.IsSuccessStatusCode)
				{
					value2 = GetServiceEndpoint(await httpResponseMessage.Content.ReadAsStringAsync().ConfigureAwait(continueOnCapturedContext: false), "SearchQueryService");
					if (!string.IsNullOrEmpty(value2))
					{
						cachedServiceIndexes[indexUrl] = value2;
						logger?.Info("Cached service index for " + feedUrl);
						goto IL_031d;
					}
					logger?.Info("SearchQueryService endpoint not found in service index for " + feedUrl);
				}
				else
				{
					logger?.Info($"Failed to fetch service index from {feedUrl}: {httpResponseMessage.StatusCode} - {httpResponseMessage.ReasonPhrase}");
				}
				goto end_IL_00ff;
				IL_031d:
				if (string.IsNullOrEmpty(value2))
				{
					continue;
				}
				string text = Uri.EscapeDataString(searchTerm);
				string requestUri = value2.TrimEnd('/') + "?q=" + text + "&prerelease=false&take=100";
				HttpResponseMessage httpResponseMessage2 = await httpClient.GetAsync(requestUri).ConfigureAwait(continueOnCapturedContext: false);
				if (httpResponseMessage2.IsSuccessStatusCode)
				{
					List<PackageSearchResult> list = ParseSearchResults(await httpResponseMessage2.Content.ReadAsStringAsync().ConfigureAwait(continueOnCapturedContext: false));
					if (list.Count > 0)
					{
						logger?.Info($"Found {list.Count} packages in feed {feedUrl}");
						searchCache[cacheKey] = list;
						return list;
					}
					logger?.Info("No packages found in feed " + feedUrl);
				}
				else
				{
					logger?.Info($"SearchQueryService request failed for {feedUrl} with status {httpResponseMessage2.StatusCode}: {httpResponseMessage2.ReasonPhrase}");
				}
				end_IL_00ff:;
			}
			catch (Exception ex)
			{
				logger?.Error("Error searching packages in feed " + feedUrl + ": " + ex.Message);
			}
		}
		return new List<PackageSearchResult>();
	}

	private static string? GetServiceEndpoint(string indexJson, string serviceType)
	{
		try
		{
			using JsonDocument jsonDocument = JsonDocument.Parse(indexJson);
			if (!jsonDocument.RootElement.TryGetProperty("resources", out var value))
			{
				return null;
			}
			foreach (JsonElement item in value.EnumerateArray())
			{
				if (item.TryGetProperty("@type", out var value2) && item.TryGetProperty("@id", out var value3))
				{
					string @string = value2.GetString();
					if (@string != null && @string.Contains(serviceType, StringComparison.OrdinalIgnoreCase))
					{
						return value3.GetString();
					}
				}
			}
		}
		catch
		{
		}
		return null;
	}

	private async Task<(string? Id, List<PackageSearchResult> SearchResults)?> ResolvePackageIdByAppIdAsync(SymbolReferenceSpecification packageSpec)
	{
		Guid appId = packageSpec.AppId;
		string publisher = packageSpec.Publisher;
		string text = appId.ToString();
		try
		{
			logger?.Info("Resolving package ID for AppId: " + text + ", Publisher: " + publisher);
			string deterministicSearchTerm = (IsSpecialCasePackage(packageSpec) ? GetSpecialPackageName(packageSpec) : text);
			List<string> list = new List<string>();
			list.Add(deterministicSearchTerm);
			List<PackageSearchResult> allPackages = new List<PackageSearchResult>();
			foreach (string searchTerm in list)
			{
				List<PackageSearchResult> list2 = await SearchPackagesAsync(searchTerm, publisher).ConfigureAwait(continueOnCapturedContext: false);
				if (list2.Count > 0)
				{
					logger?.Info($"Found {list2.Count} packages for '{searchTerm}'");
					allPackages.AddRange(list2);
					if (!string.IsNullOrWhiteSpace(symbolsCountryRegion))
					{
						if (symbolsCountryRegion.Equals("w1", StringComparison.OrdinalIgnoreCase))
						{
							List<PackageSearchResult> list3 = list2.Where((PackageSearchResult p) => !Regex.IsMatch(p.Id, "\\.([a-z]{2})\\.symbols", RegexOptions.IgnoreCase)).ToList();
							if (list3.Count > 0)
							{
								list2 = list3;
								logger?.Info($"W1 (worldwide) symbols selected (no localization suffix): {list3.Count} packages");
							}
							else
							{
								logger?.Info("No W1 (non-localized) packages found, using all available packages");
							}
						}
						else
						{
							List<PackageSearchResult> list4 = list2.Where((PackageSearchResult p) => p.Id.Contains("." + symbolsCountryRegion + ".", StringComparison.OrdinalIgnoreCase)).ToList();
							if (list4.Count > 0)
							{
								list2 = list4;
								logger?.Info("Localized symbols for country/region " + symbolsCountryRegion + " is selected.");
							}
						}
					}
					PackageSearchResult packageSearchResult = list2.FirstOrDefault((PackageSearchResult p) => p.Id.Contains(deterministicSearchTerm, StringComparison.OrdinalIgnoreCase));
					if (packageSearchResult != null)
					{
						logger?.Info("Selected package: " + packageSearchResult.Id);
						return (packageSearchResult.Id, allPackages);
					}
				}
				else
				{
					logger?.Info("No packages found for search term '" + searchTerm + "'");
				}
			}
		}
		catch (Exception ex)
		{
			logger?.Error($"Error resolving package ID for AppId {appId}: {ex.Message}");
		}
		return null;
	}

	public async Task<Stream> DownloadPackage(SymbolReferenceSpecification package)
	{
		_ = 2;
		try
		{
			SymbolReferenceSpecification resolvedPackage = await ResolvePackageVersion(package).ConfigureAwait(continueOnCapturedContext: false);
			if (resolvedPackage == null)
			{
				return Stream.Null;
			}
			IReadOnlyList<string> feedUrlsForPublisher = GetFeedUrlsForPublisher(package.Publisher);
			foreach (string feedUrl in feedUrlsForPublisher)
			{
				try
				{
					string requestUri = BuildNuGetPackageUrl(resolvedPackage, feedUrl);
					HttpResponseMessage httpResponseMessage = await httpClient.GetAsync(requestUri).ConfigureAwait(continueOnCapturedContext: false);
					if (httpResponseMessage.IsSuccessStatusCode)
					{
						MemoryStream memoryStream = new MemoryStream();
						await httpResponseMessage.Content.CopyToAsync(memoryStream).ConfigureAwait(continueOnCapturedContext: false);
						memoryStream.Position = 0L;
						logger?.Info("Successfully downloaded package from " + feedUrl);
						return memoryStream;
					}
					logger?.Info($"Failed to download package from {feedUrl}: {httpResponseMessage.StatusCode}");
				}
				catch (Exception ex)
				{
					logger?.Error("Error downloading package from " + feedUrl + ": " + ex.Message);
				}
			}
			logger?.Info("Package " + GetPackageDisplayName(package) + " not found in any configured feed");
			return Stream.Null;
		}
		catch (Exception ex2)
		{
			logger?.Error("Error downloading package " + GetPackageDisplayName(package) + ": " + ex2.Message);
			return Stream.Null;
		}
	}

	private static string GetPackageDisplayName(SymbolReferenceSpecification package)
	{
		return $"{package.Name} {package.Version}";
	}

	private static List<PackageSearchResult> ParseSearchResults(string json)
	{
		List<PackageSearchResult> list = new List<PackageSearchResult>();
		try
		{
			using JsonDocument jsonDocument = JsonDocument.Parse(json);
			if (!jsonDocument.RootElement.TryGetProperty("data", out var value))
			{
				return list;
			}
			foreach (JsonElement item in value.EnumerateArray())
			{
				if (!item.TryGetProperty("id", out var value2))
				{
					continue;
				}
				string text = null;
				if (item.TryGetProperty("tags", out var value3))
				{
					foreach (JsonElement item2 in value3.EnumerateArray())
					{
						string @string = item2.GetString();
						if (@string != null && @string.StartsWith("publisher:", StringComparison.OrdinalIgnoreCase))
						{
							text = @string.Substring("publisher:".Length);
							break;
						}
					}
				}
				string id = value2.GetString() ?? string.Empty;
				JsonElement value6;
				if (item.TryGetProperty("versions", out var value4))
				{
					foreach (JsonElement item3 in value4.EnumerateArray())
					{
						if (item3.TryGetProperty("version", out var value5))
						{
							list.Add(new PackageSearchResult
							{
								Id = id,
								Version = (value5.GetString() ?? string.Empty),
								Publisher = (text ?? string.Empty)
							});
						}
					}
				}
				else if (item.TryGetProperty("version", out value6))
				{
					list.Add(new PackageSearchResult
					{
						Id = id,
						Version = (value6.GetString() ?? string.Empty),
						Publisher = (text ?? string.Empty)
					});
				}
			}
		}
		catch
		{
		}
		return list;
	}

	private List<string> ExtractAppFromNuGetPackage(Stream nugetStream, SymbolReferenceSpecification package, string targetDirectory)
	{
		List<string> list = new List<string>();
		try
		{
			using ZipArchive zipArchive = new ZipArchive(nugetStream, ZipArchiveMode.Read, leaveOpen: true);
			foreach (ZipArchiveEntry entry in zipArchive.Entries)
			{
				if (entry.FullName.EndsWith(".app", StringComparison.OrdinalIgnoreCase))
				{
					string fileName = Path.GetFileName(entry.FullName);
					string text = Path.Combine(targetDirectory, fileName);
					entry.ExtractToFile(text, overwrite: true);
					list.Add(text);
				}
			}
		}
		catch (Exception ex)
		{
			logger?.Error("Failed to extract .app files from NuGet package: " + ex.Message);
		}
		return list;
	}

	private (List<SymbolReferenceSpecification> Dependencies, bool PropagateDependencies)? ExtractDependenciesFromPackage(List<string> packagePaths, SymbolReferenceSpecification package)
	{
		foreach (string packagePath in packagePaths)
		{
			using FileStream appStream = File.Open(packagePath, FileMode.Open);
			AppManifestSummary appManifestSummary = ReadManifest(appStream, leaveOpen: false);
			if (appManifestSummary != null)
			{
				return (appManifestSummary.Dependencies, appManifestSummary.PropagateDependencies);
			}
		}
		return null;
	}

	private AppManifestSummary? ReadManifest(Stream appStream, bool leaveOpen)
	{
		using NavAppPackageReader navAppPackageReader = NavAppPackageReader.Create(appStream, leaveOpen);
		NavAppManifest navAppManifest = navAppPackageReader.ReadNavAppManifest();
		if (navAppManifest == null)
		{
			return null;
		}
		List<SymbolReferenceSpecification> list = new List<SymbolReferenceSpecification>();
		IEnumerable<SymbolReferenceSpecification> dependencyReferences = navAppManifest.DependencyReferences;
		if (dependencyReferences != null && !dependencyReferences.IsEmpty())
		{
			foreach (SymbolReferenceSpecification dependencyReference in navAppManifest.DependencyReferences)
			{
				list.Add(dependencyReference);
			}
		}
		bool propagateDependencies = navAppManifest.PropagateDependencies;
		return new AppManifestSummary(new SymbolReferenceSpecification(navAppManifest.AppPublisher, navAppManifest.AppName, navAppManifest.AppVersion, exact: false, navAppManifest.AppId, isPropagated: false, navAppManifest.AppAlternateIds), list, propagateDependencies);
	}

	private static Version? ParseVersionOrDefault(string versionString)
	{
		if (Version.TryParse(versionString, out Version result))
		{
			return result;
		}
		return null;
	}

	private IReadOnlyList<string> GetFeedUrlsForPublisher(string publisher)
	{
		List<string> list = new List<string>();
		if (!useOnlyCustomFeeds)
		{
			if (publisher.Equals(SymbolReferenceSpecification.PublisherName, StringComparison.OrdinalIgnoreCase))
			{
				list.Add("https://pkgs.dev.azure.com/dynamicssmb2/DynamicsBCPublicFeeds/_packaging/MSSymbolsV2");
			}
			else
			{
				list.Add("https://pkgs.dev.azure.com/dynamicssmb2/DynamicsBCPublicFeeds/_packaging/AppSourceSymbols");
			}
		}
		if (privateFeeds != null && privateFeeds.Count > 0)
		{
			list.AddRange(privateFeeds);
		}
		return list;
	}

	private void UpdateResolvedNugetIds(SymbolReferenceSpecification package, string resolvedId)
	{
		resolvedNuGetIds[package.GetKeyString()] = resolvedId;
	}

	private void LogResults(List<SymbolReferenceSpecification> successful, List<string> failedDisplayNames, List<string> newlyDownloadedDisplayNames)
	{
		if (newlyDownloadedDisplayNames.Count > 0)
		{
			logger?.Info($"Successfully downloaded {newlyDownloadedDisplayNames.Count} package(s)");
		}
		if (failedDisplayNames.Count > 0)
		{
			logger?.Error($"Failed to download {failedDisplayNames.Count} package(s):");
			foreach (string failedDisplayName in failedDisplayNames)
			{
				logger?.Error("  - " + failedDisplayName);
			}
		}
		logger?.Info($"Total packages processed: {successful.Count}");
	}

	public void Dispose()
	{
		httpClient?.Dispose();
	}
}
