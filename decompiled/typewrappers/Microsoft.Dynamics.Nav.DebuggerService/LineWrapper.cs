using Newtonsoft.Json;

namespace Microsoft.Dynamics.Nav.DebuggerService;

[JsonObject]
public class LineWrapper
{
	[JsonProperty]
	public SourcePosition Position { get; set; }

	[JsonProperty]
	public string? Condition { get; set; }
}
