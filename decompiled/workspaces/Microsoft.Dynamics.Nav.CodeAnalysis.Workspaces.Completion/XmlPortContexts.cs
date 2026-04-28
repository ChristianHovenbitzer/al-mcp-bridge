using System;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Completion;

[Flags]
public enum XmlPortContexts : long
{
	None = 0L,
	FieldNodeSource = 2L,
	TableNodeSource = 4L
}
