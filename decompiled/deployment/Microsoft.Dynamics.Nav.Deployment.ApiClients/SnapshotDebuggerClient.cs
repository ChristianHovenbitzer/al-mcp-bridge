using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.CodeAnalysis;
using Microsoft.Dynamics.Nav.DebuggerService;
using Microsoft.Dynamics.Nav.Deployment.Http;
using Microsoft.Dynamics.Nav.TypeWrappers;

namespace Microsoft.Dynamics.Nav.Deployment.ApiClients;

internal class SnapshotDebuggerClient : ApiClient, ISnapshotDebuggerClient, IServerInfoApiClient
{
	private const string PackagesRelativeUri = "snapshotdebugger/packages";

	public ConnectionOptions Options => base.ConnectionOptions;

	public SnapshotDebuggerClient(ConnectionOptions options, IEmitLogger logger)
		: base(options, logger)
	{
	}

	public async Task<Stream> DownloadPackage(SymbolReferenceSpecification reference)
	{
		return await new PackagesApiClient(Options, base.Logger, ServerRegistry.SnapshotInstance, snapshotConnection: true).DownloadPackage(reference).ConfigureAwait(continueOnCapturedContext: false);
	}

	public async Task<ImmutableArray<SymbolReferenceSpecification>> DownloadPackages(ImmutableArray<SymbolReferenceSpecification> references, string targetDir)
	{
		return await new PackagesApiClient(Options, base.Logger, ServerRegistry.SnapshotInstance, snapshotConnection: true).DownloadPackages(references, targetDir).ConfigureAwait(continueOnCapturedContext: false);
	}

	public async Task<ServerInfo?> GetServerInfo()
	{
		Dictionary<string, string> queryParams = new Dictionary<string, string>();
		Uri metadataUri = CreateUri("snapshotendpointmetadata", queryParams);
		base.Logger.Info(DeploymentResources.SnapshotDebuggerMetadataRequest);
		ServerInfo serverInfo = await GetServerInfo(await (await CreateHttpClient().ConfigureAwait(continueOnCapturedContext: false)).GetAsync(metadataUri).ConfigureAwait(continueOnCapturedContext: false), allowNullReturnValue: true).ConfigureAwait(continueOnCapturedContext: false);
		if (serverInfo != null)
		{
			serverInfo.Kind = ServerInfoKind.Snapshot;
		}
		return serverInfo;
	}

	public async Task<(bool Success, string? Cookie, SnapshotDebuggerAttachKindWrapper AttachKind)> InitalizeAttachAsync(SnapshotDebuggerAttachPayloadWrapper? wrapper)
	{
		if (wrapper == null || string.IsNullOrEmpty(wrapper.DebuggingContext))
		{
			return (Success: false, Cookie: string.Empty, AttachKind: SnapshotDebuggerAttachKindWrapper.Undefined);
		}
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		dictionary.Add("debuggingcontext", wrapper.DebuggingContext);
		if (wrapper.SessionId != -1)
		{
			dictionary.Add("sessionid", wrapper.SessionId.ToString(CultureInfo.InvariantCulture));
		}
		if (!string.IsNullOrEmpty(wrapper.UserId))
		{
			dictionary.Add("userid", wrapper.UserId);
		}
		Uri uri = CreateUri("attach", dictionary);
		base.Logger.Info(DeploymentResources.SnapshotDebuggerInitializeRequestStarted, wrapper.DebuggingContext);
		return await PostInitializationRequestAsync(uri, wrapper).ConfigureAwait(continueOnCapturedContext: false);
	}

	public async Task<bool> FinishAttachAsync(FinishSnapshotDebuggerSessionPayloadWrapper? wrapper, IFileSystem fileSystem, string? snapshotFileDirectory, string? affinityCookieValue)
	{
		if (wrapper == null || snapshotFileDirectory == null || string.IsNullOrEmpty(wrapper.DebuggingContext))
		{
			return false;
		}
		string payLoad = StringSerializerHelper.SerializePayload(wrapper);
		if (payLoad.Length == 0)
		{
			return false;
		}
		(Uri, IHttpClient) tuple = await CreateClient("finish", wrapper.DebuggingContext, affinityCookieValue);
		base.Logger.Info(DeploymentResources.FinishingSnapshotDebuggerSessionRequest, wrapper.DebuggingContext);
		HttpResponseMessage response = await tuple.Item2.PostAsync(tuple.Item1, new StringContent(payLoad), CancellationToken.None).ConfigureAwait(continueOnCapturedContext: false);
		response.EnsureSuccessStatusCode();
		bool flag = false;
		using (Stream snapshotStream = await response.Content.ReadAsStreamAsync().ConfigureAwait(continueOnCapturedContext: false))
		{
			if (snapshotStream == null || snapshotStream.Length == 0L)
			{
				base.Logger.Info(DeploymentResources.SnapshotDebuggerSessionRequestFinishedWithNoFiles);
				return true;
			}
			string snapshotFilePath = Path.Combine(snapshotFileDirectory, wrapper.DebuggingContext + ".zip");
			flag = await TryWriteToFile(snapshotStream, fileSystem, snapshotFilePath).ConfigureAwait(continueOnCapturedContext: false);
			if (flag)
			{
				base.Logger.Info(DeploymentResources.SnapshotDebuggerSessionRequestFinished, snapshotFilePath);
				if (response.Headers?.ETag?.Tag != null && Enum.TryParse<ProfileKindWrapper>(response.Headers.ETag.Tag.Trim('"'), out var result) && result == ProfileKindWrapper.Sampling)
				{
					flag = ExtractProfileFile(snapshotFilePath, wrapper.DebuggingContext);
				}
			}
		}
		return flag;
	}

	public async Task<(bool Success, SnapshotDebuggerSessionStatusWrapper Status)> GetStatusAsync(SnapshotDebuggerSessionGetStatusPayloadWrapper? wrapper, string? affinityCookieValue)
	{
		if (wrapper == null || string.IsNullOrEmpty(wrapper.DebuggingContext))
		{
			return (Success: false, Status: SnapshotDebuggerSessionStatusWrapper.Initialized);
		}
		string payLoad = StringSerializerHelper.SerializePayload(wrapper);
		if (payLoad.Length == 0)
		{
			return (Success: false, Status: SnapshotDebuggerSessionStatusWrapper.Failed);
		}
		(Uri, IHttpClient) tuple = await CreateClient("status", wrapper.DebuggingContext, affinityCookieValue).ConfigureAwait(continueOnCapturedContext: false);
		HttpResponseMessage response = await tuple.Item2.PostAsync(tuple.Item1, new StringContent(payLoad), CancellationToken.None).ConfigureAwait(continueOnCapturedContext: false);
		response.EnsureSuccessStatusCode();
		Enum.TryParse<SnapshotDebuggerSessionStatusWrapper>(await response.Content.ReadAsStringAsync().ConfigureAwait(continueOnCapturedContext: false), out var result);
		return (Success: response.IsSuccessStatusCode, Status: result);
	}

	private async Task<(Uri Uri, IHttpClient Client)> CreateClient(string relativeUri, string debuggingContext, string? affinityCookieValue)
	{
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		dictionary.Add("debuggingcontext", debuggingContext);
		CookieContainer cookieContainer = null;
		if (affinityCookieValue != null && affinityCookieValue.Length > 0)
		{
			cookieContainer = new CookieContainer();
			dictionary.Add("ApplicationGatewayAffinity".ToLowerInvariant(), affinityCookieValue);
		}
		Uri uri = CreateUri(relativeUri, dictionary);
		IHttpClient httpClient = await CreateHttpClient(cookieContainer).ConfigureAwait(continueOnCapturedContext: false);
		if (cookieContainer != null)
		{
			Cookie cookie = new Cookie("ApplicationGatewayAffinity", affinityCookieValue);
			cookie.Domain = httpClient.BaseAddress.Host;
			cookieContainer.Add(cookie);
		}
		return (Uri: uri, Client: httpClient);
	}

	private Uri CreateUri(string relativeUri, Dictionary<string, string> queryParams)
	{
		AddTenantIfNeeded(queryParams);
		AddDeploymentIdIfNeeded(queryParams);
		return new Uri("snapshotdebugger/" + relativeUri + UriHelper.CreateQueryString(queryParams), UriKind.Relative);
	}

	private async Task<(bool succes, string? Cookie, SnapshotDebuggerAttachKindWrapper AttachKind)> PostInitializationRequestAsync(Uri uri, SnapshotDebuggerAttachPayloadWrapper? wrapper)
	{
		CookieContainer cookieContainer = new CookieContainer();
		IHttpClient client = await CreateHttpClient(cookieContainer).ConfigureAwait(continueOnCapturedContext: false);
		string text = StringSerializerHelper.SerializePayload(wrapper);
		if (text.Length == 0)
		{
			return (succes: false, Cookie: string.Empty, AttachKind: SnapshotDebuggerAttachKindWrapper.Undefined);
		}
		string affinityCookie = string.Empty;
		HttpResponseMessage response = await client.PostAsync(uri, new StringContent(text.ToString()), CancellationToken.None).ConfigureAwait(continueOnCapturedContext: false);
		SnapshotDebuggerAttachKindWrapper result = SnapshotDebuggerAttachKindWrapper.Undefined;
		if (response.IsSuccessStatusCode)
		{
			Enum.TryParse<SnapshotDebuggerAttachKindWrapper>(await response.Content.ReadAsStringAsync().ConfigureAwait(continueOnCapturedContext: false), out result);
			Uri uri2 = ((!uri.IsAbsoluteUri) ? new Uri(client.BaseAddress, uri.OriginalString) : uri);
			Cookie cookie = cookieContainer.GetCookies(uri2).Cast<Cookie>().FirstOrDefault((Cookie c) => c.Name == "ApplicationGatewayAffinity");
			if (cookie != null)
			{
				affinityCookie = cookie.Value;
			}
			base.Logger.Info(string.Format(CultureInfo.CurrentCulture, DeploymentResources.SnapshotDebuggerSessionInitializeSucceeded, wrapper.DebuggingContext));
		}
		return (succes: response.IsSuccessStatusCode, Cookie: affinityCookie, AttachKind: result);
	}

	private bool ExtractProfileFile(string snapshotFilePath, string debuggingContext)
	{
		using ZipArchive zipArchive = ZipFile.OpenRead(snapshotFilePath);
		string text = debuggingContext + ".alcpuprofile";
		ZipArchiveEntry entry = zipArchive.GetEntry(text);
		if (entry != null)
		{
			string text2 = Path.Combine(Path.GetDirectoryName(snapshotFilePath), text);
			entry.ExtractToFile(text2);
			base.Logger.Info(DeploymentResources.SnapshotDebuggerSessionWithSamplingRequestFinished, text2);
			return true;
		}
		base.Logger.Info(DeploymentResources.SnapshotDebuggerSessionWithSamplingFailedToUnzip, snapshotFilePath);
		return false;
	}
}
