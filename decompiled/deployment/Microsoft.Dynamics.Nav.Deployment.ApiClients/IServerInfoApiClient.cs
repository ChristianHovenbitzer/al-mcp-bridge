using System.Threading.Tasks;

namespace Microsoft.Dynamics.Nav.Deployment.ApiClients;

internal interface IServerInfoApiClient
{
	Task<ServerInfo?> GetServerInfo();
}
