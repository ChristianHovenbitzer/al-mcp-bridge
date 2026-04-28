using System.Collections.Generic;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces;

public struct SolutionChanges
{
	private readonly Solution newSolution;

	private readonly Solution oldSolution;

	internal SolutionChanges(Solution newSolution, Solution oldSolution)
	{
		this.newSolution = newSolution;
		this.oldSolution = oldSolution;
	}

	public IEnumerable<Project> GetAddedProjects()
	{
		foreach (ProjectId projectId in newSolution.ProjectIds)
		{
			if (!oldSolution.ContainsProject(projectId))
			{
				yield return newSolution.GetProject(projectId);
			}
		}
	}

	public IEnumerable<ProjectChanges> GetProjectChanges()
	{
		Solution old = oldSolution;
		foreach (ProjectId projectId in newSolution.ProjectIds)
		{
			ProjectState projectState = newSolution.GetProjectState(projectId);
			ProjectState projectState2 = old.GetProjectState(projectId);
			if (projectState2 != null && projectState != null && projectState != projectState2)
			{
				yield return newSolution.GetProject(projectId).GetChanges(oldSolution.GetProject(projectId));
			}
		}
	}

	public IEnumerable<Project> GetRemovedProjects()
	{
		foreach (ProjectId projectId in oldSolution.ProjectIds)
		{
			if (!newSolution.ContainsProject(projectId))
			{
				yield return oldSolution.GetProject(projectId);
			}
		}
	}
}
