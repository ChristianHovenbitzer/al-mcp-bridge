namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Completion;

internal static class PermissionPropertyRecommendationHelper
{
	internal static readonly PropertyValueRecommendation[] ExecuteRecommendations = new PropertyValueRecommendation[2]
	{
		new PropertyValueRecommendation("X")
		{
			DetailText = "Execute"
		},
		new PropertyValueRecommendation("x")
		{
			DetailText = "Indirect Execute"
		}
	};

	internal static readonly PropertyValueRecommendation[] RimdxRecommendations = new PropertyValueRecommendation[4]
	{
		new PropertyValueRecommendation("R")
		{
			DetailText = "Read"
		},
		new PropertyValueRecommendation("r")
		{
			DetailText = "Indirect read"
		},
		new PropertyValueRecommendation("RIMDX")
		{
			DetailText = "Full"
		},
		new PropertyValueRecommendation("rimdx")
		{
			DetailText = "Indirect full"
		}
	};

	internal static readonly PropertyValueRecommendation[] RimdRecommendations = new PropertyValueRecommendation[4]
	{
		new PropertyValueRecommendation("R")
		{
			DetailText = "Read"
		},
		new PropertyValueRecommendation("r")
		{
			DetailText = "Indirect read"
		},
		new PropertyValueRecommendation("RIMD")
		{
			DetailText = "Full"
		},
		new PropertyValueRecommendation("rimd")
		{
			DetailText = "Indirect full"
		}
	};
}
