namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Options;

internal static class CacheOption
{
	internal const string FeatureName = "Cache Options";

	internal static readonly Option<int> RecoverableTreeLengthThreshold = new Option<int>("Cache Options", "RecoverableTreeLengthThreshold", 4096);
}
