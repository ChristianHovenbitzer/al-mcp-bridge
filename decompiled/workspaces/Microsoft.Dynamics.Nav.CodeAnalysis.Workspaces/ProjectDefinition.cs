using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Globalization;
using Microsoft.Dynamics.Nav.CodeAnalysis.Packaging;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces;

[DebuggerDisplay("{GetDebuggerDisplay(),nq}")]
public class ProjectDefinition : IEquatable<ProjectDefinition>, IModuleSpecification
{
	public Guid AppId { get; set; }

	public string Name { get; set; }

	public string Publisher { get; set; }

	public Version Version { get; set; }

	public bool PropagateDependencies { get; set; }

	public ImmutableArray<Guid> AlternateIds { get; set; }

	public override string ToString()
	{
		return string.Format(CultureInfo.CurrentCulture, "{0}, {1}, {2}", Name, Publisher, Version);
	}

	public override bool Equals(object obj)
	{
		return Equals(obj as ProjectDefinition);
	}

	public bool Equals(ProjectDefinition? other)
	{
		if ((object)this == other)
		{
			return true;
		}
		if (other == null)
		{
			return false;
		}
		if (((Name == null) ? (other.Name == null) : Name.Equals(other.Name, StringComparison.OrdinalIgnoreCase)) && AppId == other.AppId && ((Publisher == null) ? (other.Publisher == null) : Publisher.Equals(other.Publisher, StringComparison.OrdinalIgnoreCase)))
		{
			return Version == other.Version;
		}
		return false;
	}

	public bool IsVersionGreaterOrEqual(ProjectDefinition other)
	{
		if (AppId == other.AppId)
		{
			return Version <= other.Version;
		}
		return false;
	}

	public static bool operator ==(ProjectDefinition left, ProjectDefinition right)
	{
		return EqualityComparer<ProjectDefinition>.Default.Equals(left, right);
	}

	public static bool operator !=(ProjectDefinition left, ProjectDefinition right)
	{
		return !(left == right);
	}

	public override int GetHashCode()
	{
		return Hash.CombineValues(Name, AppId, Publisher, Version);
	}

	private string GetDebuggerDisplay()
	{
		return ToString();
	}

	public static explicit operator ProjectDefinition(NavAppDependency dependency)
	{
		if (dependency == null)
		{
			return null;
		}
		return new ProjectDefinition
		{
			AppId = dependency.AppId,
			Name = dependency.Name,
			Publisher = dependency.Publisher,
			Version = dependency.MinVersion
		};
	}

	public static explicit operator ProjectDefinition(ProjectModelDefinition projectModel)
	{
		if (projectModel == null)
		{
			return null;
		}
		return new ProjectDefinition
		{
			AppId = new Guid(projectModel.AppId),
			Name = projectModel.Name,
			Publisher = projectModel.Publisher,
			Version = new Version(projectModel.Version)
		};
	}
}
