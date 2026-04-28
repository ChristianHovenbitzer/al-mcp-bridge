using System;
using System.Globalization;

namespace Microsoft.Dynamics.Nav.Deployment;

public class ConnectionOptions : IEquatable<ConnectionOptions>
{
	public static string DefaultSandboxEnvironmentName = "sandbox";

	public static string DefaultProductionEnvironmentName = "production";

	public string? Server { get; set; }

	public int? Port { get; set; }

	public string? ServerInstance { get; set; }

	public string? Tenant { get; set; }

	public string? ApplicationFamily { get; set; }

	public AuthenticationMethod Authentication { get; set; }

	public PublishEnvironment Environment { get; set; }

	public string? DeploymentId { get; set; }

	public int ConfigurationIdentifier { get; set; }

	public bool DisableHttpRequestTimeout { get; set; }

	public EnvironmentType EnvironmentType { get; set; }

	public string? EnvironmentName { get; set; }

	public string? PrimaryTenantDomain { get; set; }

	public bool UsePublicURLFromServer { get; set; }

	public bool ValidateServerCertificate { get; set; } = true;


	public bool UseInteractiveLogin { get; set; }

	public bool UseModernTieAuthUrl { get; set; }

	public bool UseVsCodeAuthentication { get; set; }

	public string? VsCodeAuthenticationProvider { get; set; }

	public string? AccessToken { get; set; }

	public string? AccessTokenTenantId { get; set; }

	public string? AccessTokenUserPrincipalName { get; set; }

	public EntraIdAuthenticationDetails? EntraIdAuthentication { get; set; }

	public override string ToString()
	{
		return string.Format(CultureInfo.InvariantCulture, "Server: {0}, Port: {1}, ServerInstance: {2}, Tenant: {3}, PrimaryTenantDomain: {4}, Auth: {5}, Env: {6}, EnvironmentType: {7}, EnvironmentName: {8}, Deployment ID: {9}; Disable HTTP client timeout: {10}, ConfigurationId: {11}, UsePublicURLFromServer: {12}, ValidateServerCertificate: {13}, UseInteractiveLogin: {14}, UseModernTieAuthUrl: {15}, UseVsCodeAuthentication: {16}, VsCodeAuthenticationProvider: {17}, HasAccessToken: {18}, AccessTokenTenantId: {19}, AccessTokenUserPrincipalName: {20}, EntraIdAuthenticationOverride: {21}", Server ?? string.Empty, Port.GetValueOrDefault(), ServerInstance ?? string.Empty, Tenant ?? string.Empty, PrimaryTenantDomain ?? string.Empty, Authentication, Environment, EnvironmentType, EnvironmentName, DeploymentId ?? string.Empty, DisableHttpRequestTimeout, ConfigurationIdentifier, UsePublicURLFromServer, ValidateServerCertificate, UseInteractiveLogin, UseModernTieAuthUrl, UseVsCodeAuthentication, VsCodeAuthenticationProvider ?? string.Empty, !string.IsNullOrEmpty(AccessToken), AccessTokenTenantId ?? string.Empty, AccessTokenUserPrincipalName ?? string.Empty, EntraIdAuthentication);
	}

	public string GetCacheKey()
	{
		return string.Format(CultureInfo.InvariantCulture, "Server: {0}, Port: {1}, ServerInstance: {2}, Tenant: {3}, Env: {4}, EnvironmentType: {5}, EnvironmentName: {6}, AppFamily: {7}, PrimaryTenantDomain: {8}, ValidateServerCertificate: {9}, UseModernTieAuthUrl: {10}, UseVsCodeAuthentication: {11}, VsCodeAuthenticationProvider: {12}, AccessTokenTenantId: {13}, AccessTokenUserPrincipalName: {14}, EntraIdAuthentication: {15}", Server ?? string.Empty, Port.GetValueOrDefault(), ServerInstance ?? string.Empty, Tenant ?? string.Empty, Environment, EnvironmentType, EnvironmentName, ApplicationFamily, PrimaryTenantDomain, ValidateServerCertificate, UseModernTieAuthUrl, UseVsCodeAuthentication, VsCodeAuthenticationProvider ?? string.Empty, AccessTokenTenantId ?? string.Empty, AccessTokenUserPrincipalName ?? string.Empty, EntraIdAuthentication).ToUpper();
	}

	public override bool Equals(object obj)
	{
		return Equals(obj as ConnectionOptions);
	}

	public override int GetHashCode()
	{
		return ((((((((((((((((((((((((((((((((((((((((((((((((Server?.GetHashCode() ?? 0) * 397) ^ Port?.GetHashCode()).GetValueOrDefault() * 397) ^ ServerInstance?.GetHashCode()).GetValueOrDefault() * 397) ^ Tenant?.GetHashCode()).GetValueOrDefault() * 397) ^ ApplicationFamily?.GetHashCode()).GetValueOrDefault() * 397) ^ Authentication.GetHashCode()) * 397) ^ Environment.GetHashCode()) * 397) ^ DeploymentId?.GetHashCode()).GetValueOrDefault() * 397) ^ DisableHttpRequestTimeout.GetHashCode()) * 397) ^ ConfigurationIdentifier.GetHashCode()) * 397) ^ EnvironmentType.GetHashCode()) * 397) ^ EnvironmentName?.GetHashCode()).GetValueOrDefault() * 397) ^ PrimaryTenantDomain?.GetHashCode()).GetValueOrDefault() * 397) ^ UsePublicURLFromServer.GetHashCode()) * 397) ^ ValidateServerCertificate.GetHashCode()) * 397) ^ UseInteractiveLogin.GetHashCode()) * 397) ^ UseModernTieAuthUrl.GetHashCode()) * 397) ^ UseVsCodeAuthentication.GetHashCode()) * 397) ^ VsCodeAuthenticationProvider?.GetHashCode()).GetValueOrDefault() * 397) ^ AccessTokenTenantId?.GetHashCode()).GetValueOrDefault() * 397) ^ AccessTokenUserPrincipalName?.GetHashCode()).GetValueOrDefault() * 397) ^ (EntraIdAuthentication?.Endpoint?.GetHashCode()).GetValueOrDefault()) * 397) ^ (EntraIdAuthentication?.ClientId?.GetHashCode()).GetValueOrDefault()) * 397) ^ (EntraIdAuthentication?.RedirectUri?.GetHashCode()).GetValueOrDefault()) * 397) ^ (EntraIdAuthentication?.Scope?.GetHashCode()).GetValueOrDefault();
	}

	public bool Equals(ConnectionOptions? other)
	{
		if ((object)other != null && string.Equals(Server, other.Server, StringComparison.Ordinal) && Port == other.Port && string.Equals(ServerInstance, other.ServerInstance, StringComparison.Ordinal) && string.Equals(Tenant, other.Tenant, StringComparison.Ordinal) && string.Equals(ApplicationFamily, other.ApplicationFamily, StringComparison.Ordinal) && Authentication == other.Authentication && Environment == other.Environment && string.Equals(DeploymentId, other.DeploymentId, StringComparison.Ordinal) && DisableHttpRequestTimeout == other.DisableHttpRequestTimeout && ConfigurationIdentifier == other.ConfigurationIdentifier && EnvironmentType == other.EnvironmentType && string.Equals(EnvironmentName, other.EnvironmentName, StringComparison.Ordinal) && string.Equals(PrimaryTenantDomain, other.PrimaryTenantDomain, StringComparison.Ordinal) && UsePublicURLFromServer == other.UsePublicURLFromServer && ValidateServerCertificate == other.ValidateServerCertificate && UseInteractiveLogin == other.UseInteractiveLogin && UseModernTieAuthUrl == other.UseModernTieAuthUrl && UseVsCodeAuthentication == other.UseVsCodeAuthentication && string.Equals(VsCodeAuthenticationProvider, other.VsCodeAuthenticationProvider, StringComparison.Ordinal) && string.Equals(AccessTokenTenantId, other.AccessTokenTenantId, StringComparison.Ordinal) && string.Equals(AccessTokenUserPrincipalName, other.AccessTokenUserPrincipalName, StringComparison.Ordinal) && string.Equals(EntraIdAuthentication?.Endpoint, other.EntraIdAuthentication?.Endpoint, StringComparison.Ordinal) && string.Equals(EntraIdAuthentication?.ClientId, other.EntraIdAuthentication?.ClientId, StringComparison.Ordinal) && string.Equals(EntraIdAuthentication?.RedirectUri, other.EntraIdAuthentication?.RedirectUri, StringComparison.Ordinal))
		{
			return string.Equals(EntraIdAuthentication?.Scope, other.EntraIdAuthentication?.Scope, StringComparison.Ordinal);
		}
		return false;
	}

	public static bool operator ==(ConnectionOptions? left, ConnectionOptions? right)
	{
		if ((object)left != right)
		{
			return left?.Equals(right) ?? false;
		}
		return true;
	}

	public static bool operator !=(ConnectionOptions? left, ConnectionOptions? right)
	{
		return !(left == right);
	}

	public bool IsOnPremise()
	{
		return IsOnPremise(EnvironmentType, Authentication);
	}

	public bool IsOnPremiseWithAAD()
	{
		if (EnvironmentType == EnvironmentType.OnPrem)
		{
			if (Authentication != AuthenticationMethod.AAD)
			{
				return Authentication == AuthenticationMethod.MicrosoftEntraID;
			}
			return true;
		}
		return false;
	}

	public bool IsSandbox()
	{
		return EnvironmentType == EnvironmentType.Sandbox;
	}

	public static bool IsOnPremise(EnvironmentType environmentType, AuthenticationMethod authentication)
	{
		if (environmentType == EnvironmentType.OnPrem)
		{
			return true;
		}
		if (authentication != AuthenticationMethod.Windows)
		{
			return authentication == AuthenticationMethod.UserPassword;
		}
		return true;
	}
}
