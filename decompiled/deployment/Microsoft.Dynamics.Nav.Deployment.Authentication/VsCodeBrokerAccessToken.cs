using System;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace Microsoft.Dynamics.Nav.Deployment.Authentication;

internal static class VsCodeBrokerAccessToken
{
	private static class ClaimNames
	{
		internal const string TenantIdShort = "tid";

		internal const string TenantId = "tenantId";

		internal const string Realm = "realm";

		internal const string Upn = "upn";
	}

	internal static void PopulateConnectionOptions(ConnectionOptions options)
	{
		if (!(options == null) && options.UseVsCodeAuthentication && !string.IsNullOrWhiteSpace(options.AccessToken))
		{
			var (text, text2) = ExtractClaims(options.AccessToken);
			if (!string.IsNullOrEmpty(text))
			{
				options.AccessTokenTenantId = text;
			}
			if (!string.IsNullOrEmpty(text2))
			{
				options.AccessTokenUserPrincipalName = text2;
			}
		}
	}

	private static (string? TenantId, string? UserPrincipalName) ExtractClaims(string accessToken)
	{
		try
		{
			JsonWebToken token = new JsonWebToken(accessToken);
			string? item = GetClaim(token, "tid") ?? GetClaim(token, "tenantId") ?? GetClaim(token, "realm");
			string item2 = GetClaim(token, "upn") ?? GetClaim(token, "preferred_username") ?? GetClaim(token, "email") ?? GetClaim(token, "unique_name");
			return (TenantId: item, UserPrincipalName: item2);
		}
		catch (ArgumentException)
		{
			return (TenantId: null, UserPrincipalName: null);
		}
		catch (SecurityTokenException)
		{
			return (TenantId: null, UserPrincipalName: null);
		}
	}

	private static string? GetClaim(JsonWebToken token, string claimType)
	{
		if (!token.TryGetPayloadValue<string>(claimType, out var value))
		{
			return null;
		}
		return value;
	}
}
