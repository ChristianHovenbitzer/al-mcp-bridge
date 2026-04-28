using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Utilities;

internal class SupportedPlatformData
{
	public readonly List<ProjectId> InvalidProjects;

	public readonly IEnumerable<ProjectId> CandidateProjects;

	public readonly Workspace Workspace;

	public SupportedPlatformData(List<ProjectId> invalidProjects, IEnumerable<ProjectId> candidateProjects, Workspace workspace)
	{
		InvalidProjects = invalidProjects;
		CandidateProjects = candidateProjects;
		Workspace = workspace;
	}

	public IList<SymbolDisplayPart> ToDisplayParts()
	{
		if (InvalidProjects == null || InvalidProjects.Count == 0)
		{
			return SpecializedCollections.EmptyList<SymbolDisplayPart>();
		}
		IList<SymbolDisplayPart> list = new List<SymbolDisplayPart>();
		list.AddLineBreak();
		foreach (Project item in from p in CandidateProjects
			select Workspace.CurrentSolution.GetProject(p) into p
			orderby p.Name
			select p)
		{
			string text = string.Format(CultureInfo.CurrentCulture, WorkspacesResources.ProjectAvailability, item.Name, Supported(!InvalidProjects.Contains(item.Id)));
			list.AddText(text);
			list.AddLineBreak();
		}
		list.AddLineBreak();
		list.AddText(WorkspacesResources.UseTheNavigationBarToSwitchContext);
		return list;
	}

	private static string Supported(bool supported)
	{
		if (!supported)
		{
			return WorkspacesResources.NotAvailable;
		}
		return WorkspacesResources.Available;
	}

	public bool HasValidAndInvalidProjects()
	{
		if (InvalidProjects.Any())
		{
			return InvalidProjects.Count != CandidateProjects.Count();
		}
		return false;
	}
}
