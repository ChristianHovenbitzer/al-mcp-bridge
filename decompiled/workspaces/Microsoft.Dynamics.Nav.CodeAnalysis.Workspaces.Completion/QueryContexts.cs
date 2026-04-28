using System;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Completion;

[Flags]
public enum QueryContexts : long
{
	None = 0L,
	DataItemSource = 2L,
	ColumnOrFilterSource = 4L
}
