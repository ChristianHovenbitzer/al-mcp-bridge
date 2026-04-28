using Newtonsoft.Json;

namespace Microsoft.Dynamics.Nav.DebuggerService;

[JsonObject]
public class NstSessionInfo
{
	[JsonProperty]
	public int SessionId { get; set; }

	[JsonProperty]
	public string? HostId { get; set; }
}
