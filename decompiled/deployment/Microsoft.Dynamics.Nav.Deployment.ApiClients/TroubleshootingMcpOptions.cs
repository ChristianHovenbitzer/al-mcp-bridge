using Newtonsoft.Json;

namespace Microsoft.Dynamics.Nav.Deployment.ApiClients;

[JsonObject]
public class TroubleshootingMcpOptions
{
	public const string CurrentVersion = "1.0";

	[JsonProperty]
	public string VersionNumber { get; set; } = "1.0";


	[JsonProperty]
	public int SessionId { get; set; }

	[JsonProperty]
	public string? HostId { get; set; }
}
