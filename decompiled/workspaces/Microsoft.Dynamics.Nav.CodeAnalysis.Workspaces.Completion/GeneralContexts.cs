using System;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Completion;

[Flags]
public enum GeneralContexts : long
{
	None = 0L,
	Statement = 2L,
	AnyExpression = 4L,
	AttributeName = 8L,
	PropertyDeclaration = 0x10L,
	PropertyValue = 0x20L,
	ReferenceMemberList = 0x40L,
	Trigger = 0x80L,
	Type = 0x100L,
	ApplicationObject = 0x200L,
	Extends = 0x400L,
	AttributeArgumentList = 0x800L,
	AnyComplexPropertyExpression = 0x1000L,
	MemberName = 0x2000L,
	EventTrigger = 0x4000L,
	Implements = 0x8000L,
	Namespace = 0x10000L,
	Interface = 0x20000L
}
