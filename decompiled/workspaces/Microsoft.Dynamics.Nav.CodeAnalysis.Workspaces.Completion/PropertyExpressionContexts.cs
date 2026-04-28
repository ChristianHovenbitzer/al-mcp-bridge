using System;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Completion;

[Flags]
public enum PropertyExpressionContexts : long
{
	None = 0L,
	DestinationTable = 4L,
	DestinationField = 8L
}
