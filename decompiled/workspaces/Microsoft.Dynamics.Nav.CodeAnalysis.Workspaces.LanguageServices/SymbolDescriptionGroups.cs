using System;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.LanguageServices;

[Flags]
internal enum SymbolDescriptionGroups
{
	None = 0,
	MainDescription = 1,
	Documentation = 4,
	All = 5
}
