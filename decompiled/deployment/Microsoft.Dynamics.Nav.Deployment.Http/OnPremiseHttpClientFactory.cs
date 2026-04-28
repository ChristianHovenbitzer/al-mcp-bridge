using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.Deployment.Authentication;

namespace Microsoft.Dynamics.Nav.Deployment.Http;

internal class OnPremiseHttpClientFactory : HttpClientFactory
{
	private const string BasicAuthenticationSchemeName = "Basic";

	private static readonly IDictionary<string, (AuthenticationMethod, UsernamePassword)> CredentialsCache = new Dictionary<string, (AuthenticationMethod, UsernamePassword)>();

	public static readonly Lazy<OnPremiseHttpClientFactory> Instance = new Lazy<OnPremiseHttpClientFactory>(() => new OnPremiseHttpClientFactory());

	public static void ClearCredentialsCache()
	{
		CredentialsCache.Clear();
	}

	private OnPremiseHttpClientFactory()
	{
	}

	public override Uri? CreateBaseClientUri(ConnectionOptions connectionOptions, IEmitLogger logger)
	{
		if (string.IsNullOrWhiteSpace(connectionOptions.Server) || string.IsNullOrWhiteSpace(connectionOptions.ServerInstance))
		{
			logger.Error(DeploymentResources.ServerAndInstanceMustBeSpecified);
			return null;
		}
		return UriHelper.CreateBaseClientUri(connectionOptions, logger);
	}

	public override Task<IHttpClient> Create(ConnectionOptions connectionOptions, IEmitLogger logger, bool skipRequestLogging = false, CookieContainer? cookieContainer = null)
	{
		(AuthenticationMethod, UsernamePassword) credentials = GetCredentials(connectionOptions, logger);
		AuthenticationMethod item = credentials.Item1;
		UsernamePassword item2 = credentials.Item2;
		Uri uri = CreateBaseClientUri(connectionOptions, logger);
		IHttpClient obj = ((item == AuthenticationMethod.Windows) ? NewIntegratedClient(logger, skipRequestLogging, connectionOptions.DisableHttpRequestTimeout, connectionOptions.ValidateServerCertificate) : NewBasicClient(item2.Username, item2.Password, logger, skipRequestLogging, connectionOptions.DisableHttpRequestTimeout, connectionOptions.ValidateServerCertificate));
		obj.BaseAddress = uri ?? throw new UserSetupException(DeploymentResources.InvalidLaunchJson);
		return Task.FromResult(obj);
	}

	public (AuthenticationMethod, UsernamePassword) GetCredentials(ConnectionOptions connectionOptions, IEmitLogger logger)
	{
		if (CredentialsCache.TryGetValue(connectionOptions.GetCacheKey(), out (AuthenticationMethod, UsernamePassword) value))
		{
			return value;
		}
		string server = connectionOptions.Server;
		string serverInstance = connectionOptions.ServerInstance;
		string tenant = connectionOptions.Tenant;
		if (!string.IsNullOrEmpty(tenant))
		{
			logger.Info(DeploymentResources.TargetingOnPremiseServiceWithTenant, server, serverInstance, tenant);
		}
		else
		{
			logger.Info(DeploymentResources.TargetingOnPremiseServiceWithoutTenant, server, serverInstance);
		}
		if (connectionOptions.Authentication == AuthenticationMethod.Windows)
		{
			logger.Info(DeploymentResources.UsingWindowsAuthentication);
			value = (AuthenticationMethod.Windows, null);
			CredentialsCache[connectionOptions.GetCacheKey()] = value;
			return value;
		}
		UsernamePassword usernamePassword = TryGetSavedCredentials(connectionOptions, logger);
		if (usernamePassword == null)
		{
			throw new UserNotAuthenticatedException();
		}
		if (!string.IsNullOrWhiteSpace(usernamePassword.Username))
		{
			logger.Info(DeploymentResources.UsingUserNameAndPasswordAuthentication, usernamePassword.Username);
		}
		value = (AuthenticationMethod.UserPassword, usernamePassword);
		CredentialsCache[connectionOptions.GetCacheKey()] = value;
		return value;
	}

	public UsernamePassword TryGetSavedCredentials(ConnectionOptions connectionOptions, IEmitLogger logger)
	{
		if (string.IsNullOrEmpty(connectionOptions.Server) || string.IsNullOrEmpty(connectionOptions.ServerInstance))
		{
			return null;
		}
		Dictionary<string, UsernamePassword> dictionary = UserProtectedFileStorage.CreateUserPasswordCache(logger).Read<Dictionary<string, UsernamePassword>>();
		if (dictionary == null)
		{
			return null;
		}
		string key = CreateCredentialsKey(connectionOptions.Server, connectionOptions.ServerInstance);
		dictionary.TryGetValue(key, out var value);
		return value;
	}

	public void SaveCredentials(ConnectionOptions connectionOptions, IEmitLogger logger, UsernamePassword userCredentials)
	{
		if (!string.IsNullOrEmpty(connectionOptions.Server) && !string.IsNullOrEmpty(connectionOptions.ServerInstance))
		{
			UserProtectedFileStorage userProtectedFileStorage = UserProtectedFileStorage.CreateUserPasswordCache(logger);
			Dictionary<string, UsernamePassword> dictionary = userProtectedFileStorage.Read<Dictionary<string, UsernamePassword>>();
			if (dictionary == null)
			{
				dictionary = new Dictionary<string, UsernamePassword>();
			}
			string key = CreateCredentialsKey(connectionOptions.Server, connectionOptions.ServerInstance);
			dictionary[key] = userCredentials;
			userProtectedFileStorage.Write(dictionary);
		}
	}

	public void ClearCredentials(ConnectionOptions connectionOptions, IEmitLogger logger)
	{
		if (string.IsNullOrEmpty(connectionOptions.Server) || string.IsNullOrEmpty(connectionOptions.ServerInstance))
		{
			return;
		}
		UserProtectedFileStorage userProtectedFileStorage = UserProtectedFileStorage.CreateUserPasswordCache(logger);
		Dictionary<string, UsernamePassword> dictionary = userProtectedFileStorage.Read<Dictionary<string, UsernamePassword>>();
		if (dictionary != null)
		{
			string key = CreateCredentialsKey(connectionOptions.Server, connectionOptions.ServerInstance);
			if (dictionary.ContainsKey(key))
			{
				dictionary.Remove(key);
				userProtectedFileStorage.Write(dictionary);
			}
		}
	}

	private static string CreateCredentialsKey(string server, string serverInstance)
	{
		return server.ToLowerInvariant() + "_" + serverInstance.ToLowerInvariant();
	}

	private IHttpClient NewIntegratedClient(IEmitLogger logger, bool skipRequestLogging, bool infiniteTimeout, bool validateServerCertificate)
	{
		IEmitLogger logger2 = logger;
		return HttpClientFactory.CreateWithHandlerAndLogger(new HttpClientHandler
		{
			UseDefaultCredentials = true,
			ServerCertificateCustomValidationCallback = (HttpRequestMessage message, X509Certificate2? cert, X509Chain? chain, SslPolicyErrors sslPolicyErrors) => ValidateServerCertificate(message, cert, chain, sslPolicyErrors, validateServerCertificate, logger2)
		}, logger2, skipRequestLogging, infiniteTimeout);
	}

	private IHttpClient NewBasicClient(string username, string password, IEmitLogger logger, bool skipRequestLogging, bool infiniteTimeout, bool validateServerCertificate)
	{
		IEmitLogger logger2 = logger;
		IHttpClient httpClient = HttpClientFactory.CreateWithHandlerAndLogger(new HttpClientHandler
		{
			ServerCertificateCustomValidationCallback = (HttpRequestMessage message, X509Certificate2? cert, X509Chain? chain, SslPolicyErrors sslPolicyErrors) => ValidateServerCertificate(message, cert, chain, sslPolicyErrors, validateServerCertificate, logger2)
		}, logger2, skipRequestLogging, infiniteTimeout);
		string parameter = Convert.ToBase64String(Encoding.UTF8.GetBytes(FormattableString.Invariant($"{username}:{password}")));
		httpClient.AuthorizationHeader = new AuthenticationHeaderValue("Basic", parameter);
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
}
