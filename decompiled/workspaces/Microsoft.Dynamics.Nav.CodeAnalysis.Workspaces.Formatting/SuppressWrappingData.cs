namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Formatting;

internal class SuppressWrappingData
{
	public TextSpan TextSpan { get; }

	public bool NoWrapping { get; }

	public SuppressWrappingData(TextSpan textSpan, bool noWrapping)
	{
		TextSpan = textSpan;
		NoWrapping = noWrapping;
	}
}
