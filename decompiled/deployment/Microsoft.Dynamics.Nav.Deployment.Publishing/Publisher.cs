using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.Deployment.ApiClients;
using Microsoft.Dynamics.Nav.Deployment.Authentication;
using Microsoft.Dynamics.Nav.Deployment.Http;

namespace Microsoft.Dynamics.Nav.Deployment.Publishing;

internal class Publisher
{
	private readonly IEmitLogger logger;

	public Publisher(IEmitLogger logger)
	{
		this.logger = logger;
	}

	public async Task<PublishResult> Publish(PublishOptions options, CancellationToken cancellationToken)
	{
		if (string.IsNullOrEmpty(options.Directory))
		{
			logger.Error(DeploymentResources.NoWorkspaceDirectoryError);
			return PublishResult.Failure;
		}
		if (string.IsNullOrEmpty(options.PackageFileName))
		{
			logger.Error(DeploymentResources.NoPackageFileError);
			return PublishResult.Failure;
		}
		string text = Path.Combine(options.Directory, options.PackageFileName);
		if (!File.Exists(text))
		{
			logger.Error(DeploymentResources.FileDoesNotExistError, text);
			return PublishResult.Failure;
		}
		ConnectionOptions connectionOptions = options.CreateConnectionOptions();
		connectionOptions.DisableHttpRequestTimeout = true;
		try
		{
			cancellationToken.ThrowIfCancellationRequested();
			return await new AppsApiClient(connectionOptions, logger).PublishPackageFile(text, options.SchemaUpdateMode, cancellationToken, options.IsRad, options.DependencyPublishingOption, options.ForceUpgrade, options.DisableInstallDebugging).ConfigureAwait(continueOnCapturedContext: false);
		}
		catch (UserNotAuthorizedException)
		{
			OnPremiseHttpClientFactory.Instance.Value.ClearCredentials(connectionOptions, logger);
		}
		catch (Exception ex2) when (((ex2 is IOException || ex2 is OperationCanceledException || ex2 is UnauthorizedAccessException) ? 1 : 0) != 0)
		{
			logger.Exception(ex2);
		}
		catch (Exception ex3)
		{
			logger.Exception(ex3);
			throw;
		}
		return PublishResult.Failure;
	}
}
