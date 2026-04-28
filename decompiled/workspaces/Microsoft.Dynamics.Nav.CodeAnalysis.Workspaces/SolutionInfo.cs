using System.Collections.Generic;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces;

public sealed class SolutionInfo
{
	public SolutionId Id { get; }

	public VersionStamp Version { get; }

	public string FilePath { get; }

	public IReadOnlyList<ProjectInfo> Projects { get; }

	private SolutionInfo(SolutionId id, VersionStamp version, string filePath, IEnumerable<ProjectInfo> projects)
	{
		Id = id;
		Version = version;
		FilePath = filePath;
		Projects = projects.ToImmutableReadOnlyListOrEmpty();
	}

	public static SolutionInfo Create(SolutionId id, VersionStamp version, string filePath = null, IEnumerable<ProjectInfo> projects = null)
	{
		return new SolutionInfo(id, version, filePath, projects);
	}
}
