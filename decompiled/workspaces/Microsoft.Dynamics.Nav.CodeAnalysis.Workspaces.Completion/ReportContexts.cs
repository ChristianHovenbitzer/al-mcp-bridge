using System;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Completion;

[Flags]
public enum ReportContexts : long
{
	None = 0L,
	DataItemSource = 2L
}
