using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.CodeAnalysis;
using Microsoft.Identity.Client;
using Microsoft.Identity.Client.Extensions.Msal;

namespace Microsoft.Dynamics.Nav.Deployment.Authentication;

internal class AccessTokenManager
{
	private const string CommonTenantId = "common";

	private const string tokenCacheFilename = "TokenCache.dat";

	private const string tokenCacheUnprotectedFilename = "TokenCacheUnprotected.dat";

	private readonly ConcurrentDictionary<string, IPublicClientApplication> appCache = new ConcurrentDictionary<string, IPublicClientApplication>();

	private static readonly string TokenCacheDirectoryPath = GetOrCreateTokenCacheDirectory();

	public async Task<TenantToken?> GetAccessToken(IEmitLogger logger, string authority, string resourceScope, string clientId, string redirectUri, string? usernameHint, string? aadTenantIdHint, bool useInteractiveLogin)
	{
		try
		{
			return await TryGetAccessToken(logger, authority, resourceScope, clientId, redirectUri, usernameHint, aadTenantIdHint, useInteractiveLogin).ConfigureAwait(continueOnCapturedContext: false);
		}
		catch (MsalException ex)
		{
			logger.Error(DeploymentResources.ActiveDirectoryError, ex.ErrorCode, ex.Message);
			throw new UserNotAuthenticatedException(ex.Message, ex);
		}
	}

	public async Task ClearCache(IEmitLogger logger)
	{
		UserProtectedFileStorage clientUsageMap = UserProtectedFileStorage.CreateClientUsageMap(logger);
		if (clientUsageMap.Exists())
		{
			HashSet<(string, string, string, string)> hashSet = clientUsageMap.Read<HashSet<(string, string, string, string)>>();
			foreach (var item in hashSet)
			{
				await ClearAppAccounts(await GetPublicClientApplication(item.Item1, item.Item2, item.Item3, item.Item4, logger).ConfigureAwait(continueOnCapturedContext: false)).ConfigureAwait(continueOnCapturedContext: false);
			}
		}
		clientUsageMap.Clear();
		UserProtectedFileStorage userProtectedFileStorage = UserProtectedFileStorage.CreateTenantMapCache(logger);
		if (userProtectedFileStorage.Exists())
		{
			userProtectedFileStorage.Clear();
		}
		foreach (KeyValuePair<string, IPublicClientApplication> item2 in appCache)
		{
			await ClearAppAccounts(item2.Value).ConfigureAwait(continueOnCapturedContext: false);
		}
		appCache.Clear();
		File.Delete(Path.Combine(TokenCacheDirectoryPath, "TokenCacheUnprotected.dat"));
		File.Delete(Path.Combine(TokenCacheDirectoryPath, "TokenCache.dat"));
	}

	private static async Task ClearAppAccounts(IPublicClientApplication app)
	{
		if (app == null)
		{
			return;
		}
		foreach (IAccount item in await app.GetAccountsAsync().ConfigureAwait(continueOnCapturedContext: false))
		{
			await app.RemoveAsync(item).ConfigureAwait(continueOnCapturedContext: false);
		}
	}

	private async Task<TenantToken?> TryGetAccessToken(IEmitLogger logger, string authority, string resourceScope, string clientId, string redirectUri, string? usernameHint, string? aadTenantIdHint, bool useInteractiveLogin = false)
	{
		string aadTenantIdHint2 = aadTenantIdHint;
		string usernameHint2 = usernameHint;
		IEmitLogger logger2 = logger;
		IPublicClientApplication app = await GetPublicClientApplication(authority, resourceScope, clientId, useInteractiveLogin ? "http://localhost" : redirectUri, logger2).ConfigureAwait(continueOnCapturedContext: false);
		Guid correlationId = Guid.NewGuid();
		Uri authorityUri = new Uri(authority);
		IEnumerable<IAccount> source = await app.GetAccountsAsync();
		bool registerTenantMapOnAcquireToken = false;
		IAccount account = null;
		if (string.IsNullOrEmpty(usernameHint2) && !string.IsNullOrEmpty(aadTenantIdHint2))
		{
			if (!aadTenantIdHint2.Equals("common", StringComparison.OrdinalIgnoreCase))
			{
				account = source.FirstOrDefault((IAccount acc) => acc.GetTenantProfiles().Any((TenantProfile pro) => aadTenantIdHint2.Equals(pro.TenantId, StringComparison.OrdinalIgnoreCase)));
				if (account == null)
				{
					UserProtectedFileStorage userProtectedFileStorage = UserProtectedFileStorage.CreateTenantMapCache(logger2);
					if (!userProtectedFileStorage.Exists())
					{
						registerTenantMapOnAcquireToken = true;
					}
					else
					{
						Dictionary<string, string> tenantMaps = userProtectedFileStorage.Read<Dictionary<string, string>>();
						if (tenantMaps.ContainsKey(aadTenantIdHint2.ToUpperInvariant()))
						{
							account = source.FirstOrDefault((IAccount acc) => acc.GetTenantProfiles().Any((TenantProfile pro) => tenantMaps[aadTenantIdHint2.ToUpperInvariant()].Equals(pro.TenantId, StringComparison.OrdinalIgnoreCase)));
						}
						else
						{
							registerTenantMapOnAcquireToken = true;
						}
					}
				}
			}
			else
			{
				account = source.FirstOrDefault();
			}
		}
		else if (!string.IsNullOrEmpty(usernameHint2))
		{
			account = source.FirstOrDefault((IAccount acc) => !string.IsNullOrEmpty(acc.Username) && acc.Username.Equals(usernameHint2, StringComparison.OrdinalIgnoreCase));
		}
		AuthenticationResult result2;
		try
		{
			result2 = await app.AcquireTokenSilent(new string[1] { resourceScope }, account).WithCorrelationId(correlationId).WithTenantIdFromAuthority(authorityUri)
				.ExecuteAsync()
				.ConfigureAwait(continueOnCapturedContext: false);
		}
		catch (MsalUiRequiredException)
		{
			try
			{
				logger2.Info(DeploymentResources.AADAcquireToken, authorityUri, correlationId);
				result2 = ((!useInteractiveLogin) ? (await app.AcquireTokenWithDeviceCode(new string[1] { resourceScope }, (DeviceCodeResult result) => AcquireDeviceCode(logger2, result)).WithCorrelationId(correlationId).WithTenantIdFromAuthority(authorityUri)
					.ExecuteAsync()
					.ConfigureAwait(continueOnCapturedContext: false)) : (await app.AcquireTokenInteractive(new string[1] { resourceScope }).WithCorrelationId(correlationId).WithTenantIdFromAuthority(authorityUri)
					.ExecuteAsync()
					.ConfigureAwait(continueOnCapturedContext: false)));
			}
			catch (MsalServiceException ex)
			{
				if (ex.Message.StartsWith("AADSTS900023:", StringComparison.OrdinalIgnoreCase))
				{
					throw new UserSetupException(string.Format(DeploymentResources.AADUsePrimaryTenantDomain, ex.Message), ex);
				}
				logger2.Info(ex.Message);
				throw new UserSetupException(ex.Message, ex);
			}
			catch (MsalClientException ex2)
			{
				logger2.Info(ex2.Message);
				throw new UserSetupException(ex2.Message, ex2);
			}
			catch (Exception ex3)
			{
				logger2.Exception(ex3);
				throw new UserSetupException(ex3.Message, ex3);
			}
		}
		if (result2 != null && registerTenantMapOnAcquireToken)
		{
			RegisterTenantMap(aadTenantIdHint2, result2.TenantId, logger2);
		}
		return (result2 != null) ? new TenantToken(result2.TenantId, result2.AccessToken, result2.Account.Username) : null;
	}

	private async Task<IPublicClientApplication> GetPublicClientApplication(string authority, string resourceScope, string clientId, string redirectUri, IEmitLogger logger)
	{
		IEmitLogger logger2 = logger;
		string cacheKey = CreateCacheKey(authority, resourceScope, clientId, redirectUri);
		if (appCache.TryGetValue(cacheKey, out IPublicClientApplication app2))
		{
			return app2;
		}
		PublicClientApplicationBuilder publicClientApplicationBuilder = PublicClientApplicationBuilder.Create(clientId).WithRedirectUri(redirectUri).WithAuthority(authority);
		if (LocalMachineLogger.LogLevel <= Microsoft.Dynamics.Nav.CodeAnalysis.LogLevel.Verbose)
		{
			publicClientApplicationBuilder = publicClientApplicationBuilder.WithLogging(LogCallback, GetLogLevel());
		}
		app2 = publicClientApplicationBuilder.Build();
		MsalCacheHelper msalCacheHelper = await MsalCacheHelper.CreateAsync(GetCacheStorageProperties()).ConfigureAwait(continueOnCapturedContext: false);
		try
		{
			msalCacheHelper.VerifyPersistence();
		}
		catch (MsalCachePersistenceException ex)
		{
			logger2.Info(DeploymentResources.FallbackToUnprotectedStorage, ex.Message);
			msalCacheHelper = await MsalCacheHelper.CreateAsync(GetCacheStorageProperties(forceLinuxUnprotectedFile: true)).ConfigureAwait(continueOnCapturedContext: false);
		}
		msalCacheHelper.RegisterCache(app2.UserTokenCache);
		app2 = appCache.GetOrAdd(cacheKey, app2);
		RegisterClientAppUsage((authority, resourceScope, clientId, redirectUri), logger2);
		return app2;
		static Microsoft.Identity.Client.LogLevel GetLogLevel()
		{
			return LocalMachineLogger.LogLevel switch
			{
				Microsoft.Dynamics.Nav.CodeAnalysis.LogLevel.Verbose => Microsoft.Identity.Client.LogLevel.Verbose, 
				Microsoft.Dynamics.Nav.CodeAnalysis.LogLevel.Debug => Microsoft.Identity.Client.LogLevel.Always, 
				_ => Microsoft.Identity.Client.LogLevel.Info, 
			};
		}
		void LogCallback(Microsoft.Identity.Client.LogLevel level, string message, bool containsPii)
		{
			if (!containsPii)
			{
				logger2.Info("[MSAL] " + message);
			}
		}
	}

	private static string CreateCacheKey(string authority, string resourceScope, string clientId, string redirectUri)
	{
		return FormattableString.Invariant($"{authority}_{resourceScope}_{clientId}_{redirectUri}");
	}

	private static StorageCreationProperties GetCacheStorageProperties(bool forceLinuxUnprotectedFile = false)
	{
		string tokenCacheDirectoryPath = TokenCacheDirectoryPath;
		if (!forceLinuxUnprotectedFile && File.Exists(Path.Combine(tokenCacheDirectoryPath, "TokenCacheUnprotected.dat")))
		{
			forceLinuxUnprotectedFile = true;
		}
		StorageCreationPropertiesBuilder storageCreationPropertiesBuilder = new StorageCreationPropertiesBuilder(forceLinuxUnprotectedFile ? "TokenCacheUnprotected.dat" : "TokenCache.dat", tokenCacheDirectoryPath);
		storageCreationPropertiesBuilder = ((!forceLinuxUnprotectedFile) ? storageCreationPropertiesBuilder.WithLinuxKeyring("com.microsoft.businesscentral.devtools", "default", "MSAL token cache for Business Central AL dev tools", new KeyValuePair<string, string>("Version", "1"), new KeyValuePair<string, string>("ProductGroup", "Business Central")) : storageCreationPropertiesBuilder.WithLinuxUnprotectedFile());
		return storageCreationPropertiesBuilder.WithMacKeyChain("businesscentral_service", "businesscentral_account").Build();
	}

	private static string GetOrCreateTokenCacheDirectory()
	{
		string text = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
		if (string.IsNullOrEmpty(text))
		{
			text = AppDomain.CurrentDomain.BaseDirectory;
		}
		string text2 = Path.Combine(text, "Microsoft", "BusinessCentral", "DevTools");
		Directory.CreateDirectory(text2);
		return text2;
	}

	private Task AcquireDeviceCode(IEmitLogger logger, DeviceCodeResult result)
	{
		logger.ShowDeviceLoginDialog(result.Message, result.VerificationUrl, result.UserCode);
		return Task.CompletedTask;
	}

	private static void RegisterClientAppUsage((string, string, string, string) appParameters, IEmitLogger logger)
	{
		UserProtectedFileStorage userProtectedFileStorage = UserProtectedFileStorage.CreateClientUsageMap(logger);
		HashSet<(string, string, string, string)> hashSet = (userProtectedFileStorage.Exists() ? userProtectedFileStorage.Read<HashSet<(string, string, string, string)>>() : new HashSet<(string, string, string, string)>());
		if (hashSet.Add(appParameters))
		{
			userProtectedFileStorage.Write(hashSet);
		}
	}

	private static void RegisterTenantMap(string tenantName, string tenantId, IEmitLogger logger)
	{
		UserProtectedFileStorage userProtectedFileStorage = UserProtectedFileStorage.CreateTenantMapCache(logger);
		Dictionary<string, string> dictionary = (userProtectedFileStorage.Exists() ? userProtectedFileStorage.Read<Dictionary<string, string>>() : new Dictionary<string, string>());
		try
		{
			dictionary.Add(tenantName.ToUpperInvariant(), tenantId);
			userProtectedFileStorage.Write(dictionary);
		}
		catch (ArgumentException)
		{
		}
	}
}
