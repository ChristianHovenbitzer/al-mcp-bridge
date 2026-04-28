using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.Deployment.Http;

namespace Microsoft.Dynamics.Nav.Deployment.Authentication;

public static class EntraIdAuthService
{
	public static async Task<EntraIdLoginResult> LoginAsync(IEmitLogger logger, EntraIdLoginParameters parameters, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (logger == null)
		{
			throw new ArgumentNullException("logger");
		}
		if (parameters == null)
		{
			throw new ArgumentNullException("parameters");
		}
		AccessTokenManager accessTokenManager = new AccessTokenManager();
		if (parameters.NoCache)
		{
			await accessTokenManager.ClearCache(logger).ConfigureAwait(continueOnCapturedContext: false);
		}
		CloudHttpClientFactory.GetAuthenticationParameters(new ConnectionOptions
		{
			Authentication = AuthenticationMethod.AAD,
			Environment = parameters.Environment,
			EnvironmentType = parameters.EnvironmentType,
			EnvironmentName = parameters.EnvironmentName,
			Tenant = parameters.Tenant,
			PrimaryTenantDomain = parameters.PrimaryTenantDomain,
			ApplicationFamily = parameters.ApplicationFamily,
			UseInteractiveLogin = parameters.UseInteractiveLogin,
			UseModernTieAuthUrl = parameters.UseModernTieAuthUrl,
			EntraIdAuthentication = parameters.EntraIdAuthentication
		}, logger, out string resource, out string clientId, out string redirectUrl, out string aadTenantId, out string authority);
		try
		{
			TenantToken tenantToken = await accessTokenManager.GetAccessToken(logger, authority, resource, clientId, redirectUrl, parameters.UsernameHint, aadTenantId, parameters.UseInteractiveLogin).ConfigureAwait(continueOnCapturedContext: false);
			cancellationToken.ThrowIfCancellationRequested();
			return (tenantToken != null) ? new EntraIdLoginResult
			{
				Success = true,
				TenantId = tenantToken.TenantId,
				UserPrincipalName = tenantToken.UserPrincipalName,
				UsedDeviceCode = !parameters.UseInteractiveLogin
			} : new EntraIdLoginResult
			{
				Success = false,
				Error = "Authentication failed."
			};
		}
		catch (UserNotAuthenticatedException) when (parameters.UseInteractiveLogin && parameters.AllowDeviceCodeFallback)
		{
			try
			{
				TenantToken tenantToken2 = await accessTokenManager.GetAccessToken(logger, authority, resource, clientId, redirectUrl, parameters.UsernameHint, aadTenantId, useInteractiveLogin: false).ConfigureAwait(continueOnCapturedContext: false);
				cancellationToken.ThrowIfCancellationRequested();
				return (tenantToken2 != null) ? new EntraIdLoginResult
				{
					Success = true,
					TenantId = tenantToken2.TenantId,
					UserPrincipalName = tenantToken2.UserPrincipalName,
					UsedDeviceCode = true
				} : new EntraIdLoginResult
				{
					Success = false,
					Error = "Authentication failed."
				};
			}
			catch (Exception ex)
			{
				return new EntraIdLoginResult
				{
					Success = false,
					Error = ex.Message
				};
			}
		}
		catch (Exception ex3)
		{
			return new EntraIdLoginResult
			{
				Success = false,
				Error = ex3.Message
			};
		}
	}

	public static void Logout(IEmitLogger logger)
	{
		if (logger == null)
		{
			throw new ArgumentNullException("logger");
		}
		new AccessTokenManager().ClearCache(logger).GetAwaiter().GetResult();
	}
}
