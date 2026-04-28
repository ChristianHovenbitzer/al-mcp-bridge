using System;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Formatting.Rules;

[Flags]
internal enum SuppressOption
{
	None = 0,
	NoWrappingIfOnSingleLine = 1,
	NoWrappingIfOnMultipleLine = 2,
	NoWrapping = 3,
	NoSpacingIfOnSingleLine = 4,
	NoSpacingIfOnMultipleLine = 8,
	NoSpacing = 0xC,
	IgnoreElastic = 0x10
}
