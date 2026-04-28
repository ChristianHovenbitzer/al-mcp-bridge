using Newtonsoft.Json;

namespace Microsoft.Dynamics.Nav.DebuggerService;

[JsonObject]
public class SnapshotDebuggerAttachPayloadWrapper
{
	[JsonProperty]
	public string? DebuggingContext { get; set; }

	[JsonProperty]
	public AttachClientTypeWrapper ClientType { get; set; }

	[JsonProperty]
	public string? UserId { get; set; }

	[JsonProperty]
	public SourceBreakpointLocationWrapper[]? SourceBreakpointLocations { get; set; }

	[JsonProperty]
	public SnapshotVerbosityWrapper SnapshotVerbosity { get; set; }

	[JsonProperty]
	public int SessionId { get; set; }

	[JsonProperty]
	public SnapshotDebuggingExecutionContextWrapper ExecutionContext { get; set; }

	[JsonProperty]
	public ProfileKindWrapper? Kind { get; set; }

	[JsonProperty]
	public ProfilerSamplingIntervalWrapper? SamplingInterval { get; set; }
}
