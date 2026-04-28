using System;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Completion;

[Flags]
public enum PageContexts : long
{
	None = 0L,
	Area = 2L,
	TopLevelPage = 4L,
	PageLayout = 8L,
	PageActions = 0x10L,
	ControlGroup = 0x20L,
	ActionGroup = 0x40L,
	Control = 0x80L,
	Action = 0x100L,
	PartPage = 0x200L,
	SystemPartPage = 0x400L,
	PageChangeAnchor = 0x800L,
	ControlAddIn = 0x1000L,
	View = 0x2000L,
	ActionRef = 0x4000L,
	SystemAction = 0x8000L
}
