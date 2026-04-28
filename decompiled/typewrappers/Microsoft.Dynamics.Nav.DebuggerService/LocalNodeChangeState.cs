using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.DebuggerService;

[DataContract]
public enum LocalNodeChangeState
{
	[EnumMember]
	Unchanged,
	[EnumMember]
	New,
	[EnumMember]
	ValueChanged,
	[EnumMember]
	DescendantChanged
}
