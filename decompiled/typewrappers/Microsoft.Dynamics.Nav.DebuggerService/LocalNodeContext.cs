using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace Microsoft.Dynamics.Nav.DebuggerService;

[DataContract]
[JsonObject]
public class LocalNodeContext
{
	[JsonProperty]
	[DataMember]
	public int Index { get; set; }

	[JsonProperty]
	[DataMember]
	public string Path { get; set; }

	[JsonProperty]
	[DataMember]
	public LocalNode Node { get; set; }

	public bool NeedsExpansion
	{
		get
		{
			if (Node != null && Node.HasChildren)
			{
				return Node.Children == null;
			}
			return false;
		}
	}
}
