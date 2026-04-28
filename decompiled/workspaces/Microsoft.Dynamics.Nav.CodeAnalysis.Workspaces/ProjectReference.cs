using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces;

[DebuggerDisplay("{GetDebuggerDisplay(),nq}")]
public sealed class ProjectReference : IEquatable<ProjectReference>
{
	public ProjectId ProjectId { get; }

	public string ProjectDirectory { get; }

	public ImmutableArray<string> Aliases { get; }

	internal ProjectReference(Project project)
		: this(project.Id, project.FilePath)
	{
	}

	public ProjectReference(ProjectId projectId, string path, ImmutableArray<string> aliases = default(ImmutableArray<string>))
	{
		Contract.ThrowIfNull(projectId);
		if (string.IsNullOrEmpty(path))
		{
			throw new ArgumentException("path");
		}
		ProjectId = projectId;
		Aliases = aliases.NullToEmpty();
		ProjectDirectory = Path.GetDirectoryName(path);
	}

	public override bool Equals(object obj)
	{
		return Equals(obj as ProjectReference);
	}

	public bool Equals(ProjectReference other)
	{
		if ((object)this == other)
		{
			return true;
		}
		if ((object)other != null && ProjectId == other.ProjectId && Aliases.SequenceEqual(other.Aliases))
		{
			return ProjectDirectory.Equals(other.ProjectDirectory, StringComparison.OrdinalIgnoreCase);
		}
		return false;
	}

	public static bool operator ==(ProjectReference left, ProjectReference right)
	{
		return EqualityComparer<ProjectReference>.Default.Equals(left, right);
	}

	public static bool operator !=(ProjectReference left, ProjectReference right)
	{
		return !(left == right);
	}

	public override int GetHashCode()
	{
		return Hash.CombineValues(ProjectDirectory.ToLowerInvariant(), Aliases, ProjectId.GetHashCode());
	}

	private string GetDebuggerDisplay()
	{
		return ProjectId.ToString();
	}
}
