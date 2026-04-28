namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Formatting;

internal class SuppressSpacingData
{
	public TextSpan TextSpan { get; }

	public bool NoSpacing { get; }

	public SuppressSpacingData(TextSpan textSpan, bool noSpacing)
	{
		TextSpan = textSpan;
		NoSpacing = noSpacing;
	}
}
