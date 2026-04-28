using Newtonsoft.Json;

namespace Microsoft.Dynamics.Nav.DebuggerService;

[JsonObject]
public class SourceBreakpointLocationWrapper
{
	[JsonProperty]
	public ApplicationObjectIdWrapper Id { get; set; }

	[JsonProperty]
	public LineWrapper[]? Lines { get; set; }
}
