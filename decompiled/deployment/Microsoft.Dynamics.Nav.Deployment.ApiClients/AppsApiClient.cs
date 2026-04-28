using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.CodeAnalysis;
using Microsoft.Dynamics.Nav.CodeAnalysis.Collections;
using Microsoft.Dynamics.Nav.CodeAnalysis.Packaging;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;
using Microsoft.Dynamics.Nav.Deployment.Http;
using Microsoft.Dynamics.Nav.Deployment.Publishing;

namespace Microsoft.Dynamics.Nav.Deployment.ApiClients;

internal class AppsApiClient : ApiClient
{
	private readonly IFileSystem fileSystem;

	public AppsApiClient(ConnectionOptions options, IEmitLogger logger)
		: this(FileSystem.Instance, options, logger)
	{
	}

	public AppsApiClient(IFileSystem fileSystem, ConnectionOptions options, IEmitLogger logger)
		: base(options, logger)
	{
		this.fileSystem = fileSystem;
	}

	public async Task<ClientConnectionInfo> GetClientConnectionInfo()
	{
		(string, IHttpClient) tuple = await GetHttpClient().ConfigureAwait(continueOnCapturedContext: false);
		return new ClientConnectionInfo(tuple.Item1, tuple.Item2.AuthorizationHeader);
	}

	public async Task<PublishResult> PublishPackageFile(string packageFilePath, SchemaUpdateMode schemaUpdateMode, CancellationToken cancellationToken, bool isRad = false, DependencyPublishingOption publishingOption = DependencyPublishingOption.Default, bool forceUpgrade = false, bool disableInstallDebugging = false)
	{
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		if (!string.IsNullOrWhiteSpace(base.ConnectionOptions.Tenant))
		{
			base.Logger.Info(DeploymentResources.PublishingToTenant, base.ConnectionOptions.Tenant);
		}
		AddTenantIfNeeded(dictionary);
		AddDeploymentIdIfNeeded(dictionary);
		dictionary.Add("SchemaUpdateMode", schemaUpdateMode.ToString().ToLowerInvariant());
		if (isRad)
		{
			dictionary.Add("IsRad", "true");
		}
		if (forceUpgrade)
		{
			dictionary.Add("ForceUpgrade", "true");
		}
		if (disableInstallDebugging)
		{
			dictionary.Add("UseSystemSession", "true");
		}
		dictionary.Add("DependencyPublishingOption", publishingOption.ToString().ToLowerInvariant());
		string uriString = "dev/apps" + UriHelper.CreateQueryString(dictionary);
		return await SendPackage(new Uri(uriString, UriKind.Relative), packageFilePath, publishingOption, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
	}

	private async Task<PublishResult> SendPackage(Uri uri, string packageFilePath, DependencyPublishingOption publishingOption, CancellationToken cancellationToken)
	{
		(string TenantId, IHttpClient HttpClient) client = await GetHttpClient().ConfigureAwait(continueOnCapturedContext: false);
		MultipartFormDataContent multipartFormDataContent = new MultipartFormDataContent();
		using Stream stream = fileSystem.OpenRead(packageFilePath);
		string fileName = Path.GetFileName(packageFilePath);
		multipartFormDataContent.Add(new StreamContent(stream), fileName, fileName);
		PublishResult publishResult = new PublishResult((await client.HttpClient.PostAsync(uri, multipartFormDataContent, cancellationToken).ConfigureAwait(continueOnCapturedContext: false)).IsSuccessStatusCode, client.TenantId, client.HttpClient.AuthorizationHeader, null, null, stream.Length);
		if (publishResult.Success)
		{
			(IList<ProjectModelDefinition>, IList<ProjectModelDefinition>) tuple = ((publishingOption == DependencyPublishingOption.Ignore) ? (SpecializedCollections.EmptyList<ProjectModelDefinition>(), SpecializedCollections.EmptyList<ProjectModelDefinition>()) : PackageCommon.GetPackagedDependencies(fileSystem, packageFilePath));
			LogSuccessMessage(fileName, tuple.Item1, tuple.Item2);
			(publishResult.PublishedProjectReferences, _) = tuple;
			(publishResult.PublishedProjectsThatThisProjectDependOn, _) = tuple;
		}
		return publishResult;
	}

	private void LogSuccessMessage(string fileName, IList<ProjectModelDefinition> projectsThatThisProjectDirectlyDependsOn, IList<ProjectModelDefinition> projectsThatDirectlyDependOnThisProject)
	{
		if (base.Logger == null)
		{
			return;
		}
		bool flag = projectsThatThisProjectDirectlyDependsOn.Count > 0;
		bool flag2 = projectsThatDirectlyDependOnThisProject.Count > 0;
		if (!flag && !flag2)
		{
			base.Logger.Info(string.Format(CultureInfo.CurrentCulture, DeploymentResources.PackagePublished, fileName));
		}
		else
		{
			string text = string.Empty;
			if (flag)
			{
				text = AccumulateProjectNames(projectsThatThisProjectDirectlyDependsOn);
			}
			string text2 = string.Empty;
			if (flag2)
			{
				text2 = AccumulateProjectNames(projectsThatDirectlyDependOnThisProject);
			}
			if (text.Length > 0 && text2.Length > 0)
			{
				base.Logger.Info(string.Format(CultureInfo.CurrentCulture, DeploymentResources.PackagesWithProjectReferencesAndDependenciesPublished, fileName, text, text2));
			}
			else if (text.Length == 0)
			{
				base.Logger.Info(string.Format(CultureInfo.CurrentCulture, DeploymentResources.PackagesWithDependenciesPublished, fileName, text2, fileName));
			}
			else
			{
				base.Logger.Info(string.Format(CultureInfo.CurrentCulture, DeploymentResources.PackagesWithProjectReferencesPublished, fileName, text));
			}
		}
		if (base.ConnectionOptions.IsSandbox())
		{
			base.Logger.Info(string.Format(CultureInfo.CurrentCulture, DeploymentResources.PTEDisclaimerWhenExtensionsPublished));
			base.Logger.Info(string.Format(CultureInfo.CurrentCulture, DeploymentResources.ExtensionsPublishedToOnlineSandboxWillBeLost));
		}
	}

	private static string AccumulateProjectNames(IList<ProjectModelDefinition> projectModels)
	{
		PooledStringBuilder instance = PooledStringBuilder.GetInstance();
		for (int i = 0; i < projectModels.Count; i++)
		{
			ProjectModelDefinition projectModelDefinition = projectModels[i];
			instance.Builder.Append(projectModelDefinition.Name);
			if (i < projectModels.Count - 1)
			{
				instance.Builder.Append(", ");
			}
		}
		return instance.ToStringAndFree();
	}

	private async Task<(string TenantId, IHttpClient HttpClient)> GetHttpClient()
	{
		IHttpClient item = await GetHttpClientFactory().Create(base.ConnectionOptions, base.Logger).ConfigureAwait(continueOnCapturedContext: false);
		return (TenantId: GetCurrentTenantId(), HttpClient: item);
	}

	private string GetCurrentTenantId()
	{
		string tenant = base.ConnectionOptions.Tenant;
		if (string.IsNullOrEmpty(tenant) && (!base.ConnectionOptions.IsOnPremise() || base.ConnectionOptions.IsOnPremiseWithAAD()))
		{
			return CloudHttpClientFactory.GetTenantIdFromTenantTokenCache(base.ConnectionOptions);
		}
		return tenant;
	}
}
