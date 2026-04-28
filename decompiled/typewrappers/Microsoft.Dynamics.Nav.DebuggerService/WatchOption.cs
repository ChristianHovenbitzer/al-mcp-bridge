using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.DebuggerService;

[DataContract]
public enum WatchOption
{
	[EnumMember]
	None,
	[EnumMember]
	AllowLargeStrings
}
