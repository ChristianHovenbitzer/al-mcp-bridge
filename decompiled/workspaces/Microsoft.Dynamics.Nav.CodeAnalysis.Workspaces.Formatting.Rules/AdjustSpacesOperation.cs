using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Formatting.Rules;

internal sealed class AdjustSpacesOperation
{
	public int Space { get; }

	public AdjustSpacesOption Option { get; }

	internal AdjustSpacesOperation(int space, AdjustSpacesOption option)
	{
		Contract.ThrowIfFalse(space >= 0);
		Space = space;
		Option = option;
	}
}
