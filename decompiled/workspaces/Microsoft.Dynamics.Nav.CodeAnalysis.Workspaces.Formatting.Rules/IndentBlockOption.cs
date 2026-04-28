using System;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Formatting.Rules;

[Flags]
internal enum IndentBlockOption
{
	RelativeToFirstTokenOnBaseTokenLine = 2,
	RelativePosition = 4,
	AbsolutePosition = 8,
	RelativePositionMask = 6,
	PositionMask = 0xE
}
