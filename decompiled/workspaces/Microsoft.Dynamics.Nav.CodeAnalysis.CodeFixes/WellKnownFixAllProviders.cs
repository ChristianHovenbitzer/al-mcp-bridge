namespace Microsoft.Dynamics.Nav.CodeAnalysis.CodeFixes;

public static class WellKnownFixAllProviders
{
	public static FixAllProvider BatchFixer => BatchFixAllProvider.Instance;
}
