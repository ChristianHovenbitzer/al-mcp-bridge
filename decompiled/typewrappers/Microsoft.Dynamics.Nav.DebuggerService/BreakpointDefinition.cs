using Newtonsoft.Json;

namespace Microsoft.Dynamics.Nav.DebuggerService;

[JsonObject]
public class BreakpointDefinition
{
	[JsonProperty]
	public long BreakpointId { get; set; }

	[JsonProperty]
	public string? Condition { get; set; }

	[JsonProperty]
	public string? InternalMethodName { get; set; }

	[JsonProperty]
	public string? MethodName { get; set; }

	[JsonProperty]
	public ApplicationObjectIdWrapper ObjectId { get; set; }

	[JsonProperty]
	public SourceSpan SourceSpan { get; set; }

	[JsonProperty]
	public SourceSpan RelativeSourceSpan { get; set; }

	public BreakpointDefinition()
	{
	}

	[JsonConstructor]
	public BreakpointDefinition(ApplicationObjectIdWrapper objectId, SourceSpan sourceSpan, SourceSpan relativeSourceSpan, long breakpointId, string methodName, string internalMethodName, string condition)
	{
		ObjectId = objectId;
		Condition = condition;
		SourceSpan = sourceSpan;
		RelativeSourceSpan = relativeSourceSpan;
		BreakpointId = breakpointId;
		MethodName = methodName;
		InternalMethodName = internalMethodName;
	}
}
