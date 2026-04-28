using Microsoft.Dynamics.Nav.Deployment.Authentication;
using Newtonsoft.Json;

namespace Microsoft.Dynamics.Nav.Deployment;

public class ServerConnectionConfiguration : ConfigurationBase
{
	public virtual string? Server { get; set; }

	public virtual int? Port { get; set; }

	public virtual string? ServerInstance { get; set; }

	public virtual string? Tenant { get; set; }

	public virtual string? PrimaryTenantDomain { get; set; }

	public virtual string? ApplicationFamily { get; set; }

	[JsonConverter(typeof(AuthenticationMethodJsonConverter))]
	public virtual AuthenticationMethod Authentication { get; set; }

	public virtual PublishEnvironment Environment { get; set; }

	public virtual string? SandboxName { get; set; }

	public virtual string? DeploymentId { get; set; }

	public virtual bool DisableHttpRequestTimeout { get; set; }

	public virtual EnvironmentType EnvironmentType { get; set; }

	public virtual string? EnvironmentName { get; set; }

	public virtual bool UsePublicURLFromServer { get; set; }

	public virtual bool ValidateServerCertificate { get; set; } = true;


	public virtual bool UseInteractiveLogin { get; set; } = true;


	public virtual bool UseVsCodeAuthentication { get; set; }

	public virtual string? VsCodeAuthenticationProvider { get; set; }

	public virtual string? AccessToken { get; set; }

	public virtual EntraIdAuthenticationDetails? EntraIdAuthentication { get; set; }

	public virtual bool UseModernTieAuthUrl { get; set; }

	public virtual int? McpServerPort { get; set; }

	public virtual ConnectionOptions CreateConnectionOptions(PublishEnvironment? environment = null)
	{
		string sandboxName = SandboxName;
		string text = EnvironmentName;
		if (!ConnectionOptions.IsOnPremise(EnvironmentType, Authentication) && string.IsNullOrEmpty(text))
		{
			text = (string.IsNullOrEmpty(sandboxName) ? CreateEnvironmentNameOrDefault(EnvironmentType, base.Request, text) : sandboxName);
		}
		ConnectionOptions obj = new ConnectionOptions
		{
			Authentication = Authentication,
			Environment = (environment ?? Environment),
			Server = Server,
			Port = Port,
			ServerInstance = ServerInstance,
			Tenant = Tenant,
			PrimaryTenantDomain = PrimaryTenantDomain,
			ApplicationFamily = ApplicationFamily,
			EnvironmentType = EnvironmentType,
			EnvironmentName = text,
			DisableHttpRequestTimeout = DisableHttpRequestTimeout,
			DeploymentId = DeploymentId,
			UsePublicURLFromServer = UsePublicURLFromServer,
			ValidateServerCertificate = ValidateServerCertificate,
			UseInteractiveLogin = UseInteractiveLogin,
			UseVsCodeAuthentication = UseVsCodeAuthentication,
			VsCodeAuthenticationProvider = VsCodeAuthenticationProvider,
			AccessToken = AccessToken,
			EntraIdAuthentication = EntraIdAuthentication,
			UseModernTieAuthUrl = UseModernTieAuthUrl,
			ConfigurationIdentifier = CreateConfigurationIdentifier()
		};
		VsCodeBrokerAccessToken.PopulateConnectionOptions(obj);
		return obj;
		static string CreateEnvironmentNameOrDefault(EnvironmentType environmentType, ConfigurationRequestType request, string? environmentName)
		{
			if (!string.IsNullOrEmpty(environmentName))
			{
				return environmentName;
			}
			switch (environmentType)
			{
			case EnvironmentType.OnPrem:
				return string.Empty;
			case EnvironmentType.Production:
				return ConnectionOptions.DefaultProductionEnvironmentName;
			case EnvironmentType.Sandbox:
				return ConnectionOptions.DefaultSandboxEnvironmentName;
			default:
				if (request != ConfigurationRequestType.SnapshotInitialize)
				{
					return ConnectionOptions.DefaultSandboxEnvironmentName;
				}
				return ConnectionOptions.DefaultProductionEnvironmentName;
			}
		}
	}
}
