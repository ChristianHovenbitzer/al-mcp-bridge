using Newtonsoft.Json;

namespace Microsoft.Dynamics.Nav.TypeWrappers;

[JsonObject]
public class SnapshotDebuggerSessionGetStatusPayloadWrapper
{
	[JsonProperty]
	public string? DebuggingContext { get; set; }
}
