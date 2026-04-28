using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.Deployment.Authentication;
using Microsoft.Dynamics.Nav.Deployment.Telemetry;

namespace Microsoft.Dynamics.Nav.Deployment.Http;

internal class CloudHttpClientFactory : HttpClientFactory
{
	private const string BearerAuthenticationSchemeName = "Bearer";

	private const string V2EnvironmentPathFormat = "/v2.0/{0}/";

	private const string FixedEndpointFormat = "https://api.businesscentral.dynamics{0}.com{1}";

	private const string ApplicationFamilyFixedEndpointFormat = "https://{0}.api.bc.dynamics{1}.com{2}";

	private const string ProductionLoginUrl = "https://login.microsoftonline.com";

	private const string ProductionResource = "https://api.businesscentral.dynamics.com/.default";

	private const string ProductionClientId = "41839ce3-4041-4bac-8c17-0941f25d7aaf";

	private const string ProductionRedirectUrl = "https://developer.businesscentral.dynamics.com";

	private const string StagingLoginUrl = "https://login.windows-ppe.net";

	private const string StagingLoginUrlModern = "https://login.microsoftonline-ppe.com";

	private const string StagingResource = "https://api.businesscentral.dynamics-tie.com/.default";

	private const string LocalPPE = "https://api.businesscentral.dynamics-tie.com";

	private const string StagingClientId = "1d69c890-658f-40f0-8e24-639c6ada0b1f";

	private const string StagingRedirectUrl = "https://developer.businesscentral.dynamics-tie.com";

	private const string DevResource = "996def3d-b36c-4153-8607-a6fd3c01b89f";

	private static readonly IDictionary<string, (string? userPrincipalName, string tenantId)> CacheKeyAccountCache = new Dictionary<string, (string, string)>();

	public static readonly Lazy<CloudHttpClientFactory> Instance = new Lazy<CloudHttpClientFactory>(() => new CloudHttpClientFactory());

	private static readonly AccessTokenManager AccessTokenManager = new AccessTokenManager();

	private CloudHttpClientFactory()
	{
	}

	public override Uri? CreateBaseClientUri(ConnectionOptions connectionOptions, IEmitLogger logger)
	{
		if (connectionOptions.IsOnPremiseWithAAD())
		{
			return UriHelper.CreateBaseClientUri(connectionOptions, logger);
		}
		Uri uri = ResolveServerAddress(connectionOptions, logger);
		if (uri == null)
		{
			logger.Error(DeploymentResources.ServerAddressResolutionError);
			return null;
		}
		return uri;
	}

	public override async Task<IHttpClient> Create(ConnectionOptions connectionOptions, IEmitLogger logger, bool skipRequestLogging = false, CookieContainer? cookieContainer = null)
	{
		TenantToken tenantToken = await GetTenantToken(connectionOptions, logger).ConfigureAwait(continueOnCapturedContext: false);
		if (tenantToken == null)
		{
			throw new UserNotAuthenticatedException();
		}
		Uri baseAddress = CreateBaseClientUri(connectionOptions, logger);
		TelemetryServiceManager.CurrentTelemetryService.SetAadTenantId(tenantToken.TenantId);
		IHttpClient httpClient = CreateBearerClient(tenantToken.AccessToken, logger, skipRequestLogging, connectionOptions.DisableHttpRequestTimeout, connectionOptions.ValidateServerCertificate, cookieContainer);
		httpClient.BaseAddress = baseAddress;
		return httpClient;
	}

	private async Task<TenantToken?> GetTenantToken(ConnectionOptions connectionOptions, IEmitLogger logger)
	{
		CacheKeyAccountCache.TryGetValue(connectionOptions.GetCacheKey(), out var accountDetails);
		TenantToken tenantToken = await ObtainAccessToken(connectionOptions, accountDetails.userPrincipalName, logger).ConfigureAwait(continueOnCapturedContext: false);
		if (tenantToken == null)
		{
			return tenantToken;
		}
		if (!string.IsNullOrEmpty(tenantToken.UserPrincipalName) && (string.IsNullOrEmpty(accountDetails.userPrincipalName) || !tenantToken.UserPrincipalName.Equals(accountDetails.userPrincipalName, StringComparison.OrdinalIgnoreCase)))
		{
			if (!connectionOptions.UseVsCodeAuthentication)
			{
				logger.Info(DeploymentResources.AADAuthenticatedWithCacheNotification, tenantToken.UserPrincipalName, tenantToken.TenantId);
			}
			string tenant = connectionOptions.Tenant;
			if (!string.IsNullOrEmpty(tenant))
			{
				if (connectionOptions.PrimaryTenantDomain != null)
				{
					logger.Info(DeploymentResources.TargetingD365BusinessCentralWithTenantAndAADTenantId, tenant, connectionOptions.PrimaryTenantDomain);
				}
				else
				{
					logger.Info(DeploymentResources.TargetingD365BusinessCentralWithTenant, tenant);
				}
			}
			else
			{
				logger.Info(DeploymentResources.TargetingD365BusinessCentralWithoutTenant);
			}
			CacheKeyAccountCache[connectionOptions.GetCacheKey()] = (tenantToken.UserPrincipalName, tenantToken.TenantId);
		}
		return tenantToken;
	}

	private Uri ResolveServerAddress(ConnectionOptions connectionOptions, IEmitLogger logger)
	{
		string environmentPart = GetEnvironmentPart(connectionOptions.EnvironmentName);
		if (!string.IsNullOrEmpty(connectionOptions.ApplicationFamily))
		{
			string text = string.Format(CultureInfo.InvariantCulture, "https://{0}.api.bc.dynamics{1}.com{2}", connectionOptions.ApplicationFamily, connectionOptions.Environment.DeploymentSuffix(), environmentPart);
			if (UriHelper.TryParseAbsoluteUri(text, out Uri uri))
			{
				return uri;
			}
			string message = string.Format(CultureInfo.InvariantCulture, DeploymentResources.MalformedAbsoluteUriError, text);
			logger.Error(message);
			throw new HttpRequestException(message);
		}
		return new Uri(string.Format(CultureInfo.InvariantCulture, "https://api.businesscentral.dynamics{0}.com{1}", connectionOptions.Environment.DeploymentSuffix(), environmentPart));
	}

	private static string GetEnvironmentPart(string? environmentName)
	{
		if (string.IsNullOrEmpty(environmentName))
		{
			return string.Empty;
		}
		return string.Format(CultureInfo.InvariantCulture, "/v2.0/{0}/", environmentName);
	}

	private static IHttpClient CreateBearerClient(string accessToken, IEmitLogger logger, bool skipRequestLogging, bool infiniteTimeout, bool validateServerCertificate, CookieContainer? cookieContainer = null)
	{
		IEmitLogger logger2 = logger;
		IHttpClient httpClient = HttpClientFactory.CreateWithHandlerAndLogger(new HttpClientHandler
		{
			ServerCertificateCustomValidationCallback = (HttpRequestMessage message, X509Certificate2? cert, X509Chain? chain, SslPolicyErrors sslPolicyErrors) => ValidateServerCertificate(message, cert, chain, sslPolicyErrors, validateServerCertificate, logger2)
		}, logger2, skipRequestLogging, infiniteTimeout, cookieContainer);
		httpClient.AuthorizationHeader = new AuthenticationHeaderValue("Bearer", accessToken);
		return httpClient;
	}

	private static bool ValidateServerCertificate(HttpRequestMessage message, X509Certificate2 cert, X509Chain chain, SslPolicyErrors sslPolicyErrors, bool validateCertificate, IEmitLogger logger)
	{
		if (sslPolicyErrors != 0 && !validateCertificate)
		{
			logger.Info(string.Format(CultureInfo.CurrentCulture, DeploymentResources.ServerCertificateValidationDisabled));
			return true;
		}
		return sslPolicyErrors == SslPolicyErrors.None;
	}

	private async Task<TenantToken?> ObtainAccessToken(ConnectionOptions connectionOptions, string? usernameHint, IEmitLogger logger)
	{
		if (connectionOptions.UseVsCodeAuthentication && !string.IsNullOrEmpty(connectionOptions.AccessToken))
		{
			return new TenantToken(connectionOptions.AccessTokenTenantId ?? "VS Code tenant", connectionOptions.AccessToken, connectionOptions.AccessTokenUserPrincipalName ?? "VS Code user");
		}
		GetAuthenticationParameters(connectionOptions, logger, out string resource, out string clientId, out string redirectUrl, out string aadTenantId, out string authority);
		return await AccessTokenManager.GetAccessToken(logger, authority, resource, clientId, redirectUrl, usernameHint, aadTenantId, connectionOptions.UseInteractiveLogin).ConfigureAwait(continueOnCapturedContext: false);
	}

	internal static void GetAuthenticationParameters(ConnectionOptions connectionOptions, IEmitLogger logger, out string resource, out string clientId, out string redirectUrl, out string aadTenantId, out string authority)
	{
		string loginUrl = LogAndSelect(connectionOptions.EntraIdAuthentication?.Endpoint, "https://login.microsoftonline.com", "Endpoint", logger);
		resource = LogAndSelect(connectionOptions.EntraIdAuthentication?.Scope, "https://api.businesscentral.dynamics.com/.default", "Scope", logger);
		clientId = LogAndSelect(connectionOptions.EntraIdAuthentication?.ClientId, "41839ce3-4041-4bac-8c17-0941f25d7aaf", "ClientId", logger);
		redirectUrl = LogAndSelect(connectionOptions.EntraIdAuthentication?.RedirectUri, "https://developer.businesscentral.dynamics.com", "RedirectUri", logger);
		if (connectionOptions.Environment == PublishEnvironment.Tie)
		{
			loginUrl = (connectionOptions.UseModernTieAuthUrl ? "https://login.microsoftonline-ppe.com" : "https://login.windows-ppe.net");
			resource = (connectionOptions.IsOnPremiseWithAAD() ? "https://api.businesscentral.dynamics-tie.com" : "https://api.businesscentral.dynamics-tie.com/.default");
			clientId = "1d69c890-658f-40f0-8e24-639c6ada0b1f";
			redirectUrl = "https://developer.businesscentral.dynamics-tie.com";
		}
		else if (connectionOptions.Environment == PublishEnvironment.ServicesTie)
		{
			loginUrl = (connectionOptions.UseModernTieAuthUrl ? "https://login.microsoftonline-ppe.com" : "https://login.windows-ppe.net");
			resource = "996def3d-b36c-4153-8607-a6fd3c01b89f";
			clientId = "1d69c890-658f-40f0-8e24-639c6ada0b1f";
			redirectUrl = "https://developer.businesscentral.dynamics-tie.com";
		}
		aadTenantId = GetAADTenantIdFromConnectionOptions(connectionOptions);
		authority = GetLoginAuthority(aadTenantId, loginUrl) ?? throw new HttpRequestException(string.Format(CultureInfo.InvariantCulture, DeploymentResources.MalformedTenantIdError, aadTenantId));
		static string LogAndSelect(string? value, string defaultValue, string paramName, IEmitLogger logger)
		{
			if (string.IsNullOrEmpty(value))
			{
				return defaultValue;
			}
			logger.Info(DeploymentResources.LogCustomEntraIdParameter, paramName, value);
			return value;
		}
	}

	private static string? GetLoginAuthority(string aadTenantId, string loginUrl)
	{
		if (Uri.IsWellFormedUriString(FormattableString.Invariant($"{loginUrl}/{aadTenantId}"), UriKind.Absolute) && Uri.TryCreate(FormattableString.Invariant($"{loginUrl}/{Uri.EscapeDataString(aadTenantId)}"), UriKind.Absolute, out Uri result))
		{
			return result.AbsoluteUri;
		}
		return null;
	}

	private static string GetAADTenantIdFromConnectionOptions(ConnectionOptions connectionOptions)
	{
		string text = null;
		text = ((!connectionOptions.IsOnPremiseWithAAD()) ? connectionOptions.Tenant : connectionOptions.PrimaryTenantDomain);
		if (string.IsNullOrWhiteSpace(text) && !string.IsNullOrEmpty(connectionOptions.PrimaryTenantDomain))
		{
			text = connectionOptions.PrimaryTenantDomain;
		}
		if (string.IsNullOrEmpty(text))
		{
			text = "common";
		}
		return text;
	}

	public static string GetTenantIdFromTenantTokenCache(ConnectionOptions connectionOptions)
	{
		if (CacheKeyAccountCache.TryGetValue(connectionOptions.GetCacheKey(), out (string, string) value))
		{
			return value.Item2;
		}
		return null;
	}

	public static void ClearTokenCache(IEmitLogger logger)
	{
		CacheKeyAccountCache.Clear();
		AccessTokenManager.ClearCache(logger).GetAwaiter().GetResult();
	}
}
