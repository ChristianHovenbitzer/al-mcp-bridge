using System;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.LanguageServices;

[Flags]
internal enum DisplayNameOptions
{
	None = 0,
	IncludeMemberKeyword = 1,
	IncludeNamespaces = 2,
	IncludeParameters = 4,
	IncludeType = 8,
	IncludeTypeParameters = 0x10
}
