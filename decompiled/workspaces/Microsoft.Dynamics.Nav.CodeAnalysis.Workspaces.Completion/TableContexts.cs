using System;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Completion;

[Flags]
public enum TableContexts : long
{
	None = 0L,
	TopLevelTable = 2L,
	Fields = 4L,
	Keys = 8L,
	FieldGroup = 0x10L,
	Field = 0x20L,
	ModifyContext = 0x40L
}
