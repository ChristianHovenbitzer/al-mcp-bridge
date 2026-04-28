using System.Threading;
using System.Threading.Tasks;
using ModelContextProtocol.Client;

namespace Microsoft.Dynamics.Nav.Deployment.ApiClients;

public interface IMcpClientProvider
{
	ConnectionOptions Options { get; }

	Task<McpClient> GetMcpClient(CancellationToken cancellationToken);
}
