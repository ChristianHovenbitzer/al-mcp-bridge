using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.DebuggerService;

[DataContract]
public enum BreakpointResponse
{
	[EnumMember]
	Continue,
	[EnumMember]
	StepOver,
	[EnumMember]
	StepIn,
	[EnumMember]
	StepOut,
	[EnumMember]
	ReleaseConnection,
	[EnumMember]
	AbortActivity
}
