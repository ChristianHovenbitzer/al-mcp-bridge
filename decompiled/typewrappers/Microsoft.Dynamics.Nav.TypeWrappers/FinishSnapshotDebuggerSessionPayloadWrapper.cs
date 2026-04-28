using Newtonsoft.Json;

namespace Microsoft.Dynamics.Nav.TypeWrappers;

[JsonObject]
public class FinishSnapshotDebuggerSessionPayloadWrapper
{
	[JsonProperty]
	public string? DebuggingContext { get; set; }
}
