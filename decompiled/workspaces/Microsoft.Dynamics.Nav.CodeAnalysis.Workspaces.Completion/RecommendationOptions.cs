using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Options;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Completion;

public static class RecommendationOptions
{
	internal const string RecommendationsFeatureName = "Recommendations";

	public static PerLanguageOption<bool> HideAdvancedMembers { get; } = new PerLanguageOption<bool>("Recommendations", "HideAdvancedMembers", defaultValue: false);


	public static PerLanguageOption<bool> FilterOutOfScopeLocals { get; } = new PerLanguageOption<bool>("Recommendations", "FilterOutOfScopeLocals", defaultValue: true);

}
