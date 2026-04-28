using System;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.LanguageServices;

[Flags]
public enum SymbolDisplayMemberOptions
{
	None = 0,
	IncludeType = 1,
	IncludeModifiers = 2,
	IncludeAccessibility = 4,
	IncludeParameters = 8,
	IncludeContainingType = 0x10,
	IncludeConstantValue = 0x20
}
