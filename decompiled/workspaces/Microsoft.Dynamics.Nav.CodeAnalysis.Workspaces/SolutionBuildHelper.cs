using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces;

public static class SolutionBuildHelper
{
	public static async Task<(bool Success, ImmutableArray<(ProjectId Id, T Result)> Outputs)> ProcessTransitiveDependenciesInOrderAsync<T>(Solution solution, ProjectId targetProjectId, Func<Project, CancellationToken, Task<(bool Success, T Result)>> processProjectAsync, CancellationToken cancellationToken)
	{
		ProjectDependencyGraph projectDependencyGraph = solution.GetProjectDependencyGraph();
		if (projectDependencyGraph == null)
		{
			return (Success: true, Outputs: ImmutableArray<(ProjectId, T)>.Empty);
		}
		IList<ProjectId> projectsThatThisProjectTransitivelyDependsOnTopologicallySorted = projectDependencyGraph.GetProjectsThatThisProjectTransitivelyDependsOnTopologicallySorted(targetProjectId);
		if (projectsThatThisProjectTransitivelyDependsOnTopologicallySorted.Count == 0)
		{
			return (Success: true, Outputs: ImmutableArray<(ProjectId, T)>.Empty);
		}
		List<(ProjectId, T)> outputs = new List<(ProjectId, T)>();
		foreach (ProjectId depId in projectsThatThisProjectTransitivelyDependsOnTopologicallySorted)
		{
			cancellationToken.ThrowIfCancellationRequested();
			if (depId == targetProjectId)
			{
				continue;
			}
			Project project = solution.GetProject(depId);
			if (project != null)
			{
				var (flag, item) = await processProjectAsync(project, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
				if (!flag)
				{
					return (Success: false, Outputs: ImmutableArray<(ProjectId, T)>.Empty);
				}
				outputs.Add((depId, item));
			}
		}
		return (Success: true, Outputs: ImmutableArray.CreateRange(outputs));
	}
}
