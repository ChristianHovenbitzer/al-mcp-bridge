using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Formatting.Rules;

internal sealed class AdjustNewLinesOperation
{
	public int Line { get; }

	public AdjustNewLinesOption Option { get; }

	internal AdjustNewLinesOperation(int line, AdjustNewLinesOption option)
	{
		Contract.ThrowIfFalse(option != AdjustNewLinesOption.ForceLines || line > 0);
		Contract.ThrowIfFalse(option != 0 || line >= 0);
		Contract.ThrowIfFalse(option != AdjustNewLinesOption.ForceLinesIfOnSingleLine || line > 0);
		Line = line;
		Option = option;
	}
}
