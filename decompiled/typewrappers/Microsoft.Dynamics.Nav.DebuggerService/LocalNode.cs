using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace Microsoft.Dynamics.Nav.DebuggerService;

[DataContract]
[JsonObject]
public class LocalNode
{
	[DataMember]
	private readonly LocalNodeChangeState changeState;

	[DataMember]
	private readonly ReadOnlyCollection<LocalNode> children;

	[DataMember]
	private readonly bool hasChildren;

	[DataMember]
	private readonly string name;

	[DataMember]
	private readonly string summary;

	[DataMember]
	private readonly string typeName;

	[JsonProperty]
	public LocalNodeChangeState ChangeState
	{
		[DebuggerStepThrough]
		get
		{
			return changeState;
		}
	}

	[JsonProperty]
	public ReadOnlyCollection<LocalNode> Children
	{
		[DebuggerStepThrough]
		get
		{
			return children;
		}
	}

	[JsonProperty]
	public bool HasChildren
	{
		[DebuggerStepThrough]
		get
		{
			return hasChildren;
		}
	}

	[JsonProperty]
	public string Name
	{
		[DebuggerStepThrough]
		get
		{
			return name;
		}
	}

	[JsonProperty]
	public string Summary
	{
		[DebuggerStepThrough]
		get
		{
			return summary;
		}
	}

	[JsonProperty]
	public string TypeName
	{
		[DebuggerStepThrough]
		get
		{
			return typeName;
		}
	}

	[JsonConstructor]
	public LocalNode(string name, string typeName, string summary, bool hasChildren, LocalNodeChangeState localChangeState, IEnumerable<LocalNode> children)
	{
		this.name = name;
		this.typeName = typeName;
		this.summary = summary;
		this.hasChildren = hasChildren;
		this.children = children?.ToList().AsReadOnly();
		if (localChangeState == LocalNodeChangeState.Unchanged && this.children != null && this.children.Any((LocalNode c) => c.ChangeState != LocalNodeChangeState.Unchanged))
		{
			localChangeState = LocalNodeChangeState.DescendantChanged;
		}
		changeState = localChangeState;
	}
}
