using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces;

[DebuggerDisplay("{GetDebuggerDisplay(),nq}")]
public sealed class ProjectId : IEquatable<ProjectId>
{
	public static readonly ProjectId Empty = new ProjectId(Guid.Empty, "Empty projectId");

	public Guid Id { get; }

	internal string DebugName { get; }

	private ProjectId(string debugName)
	{
		Id = Guid.NewGuid();
		DebugName = debugName;
	}

	internal ProjectId(Guid guid, string debugName)
	{
		Id = guid;
		DebugName = debugName;
	}

	public static ProjectId CreateNewId(string debugName = null)
	{
		return new ProjectId(debugName);
	}

	public static ProjectId CreateFromSerialized(Guid id, string debugName = null)
	{
		if (id == Guid.Empty)
		{
			throw new ArgumentException(WorkspacesResources.IdCanNotBeEmpty, "id");
		}
		return new ProjectId(id, debugName);
	}

	private string GetDebuggerDisplay()
	{
		return string.Format(CultureInfo.InvariantCulture, "({0}, #{1} - {2})", GetType().Name, Id, DebugName);
	}

	public override string ToString()
	{
		return GetDebuggerDisplay();
	}

	public override bool Equals(object obj)
	{
		return Equals(obj as ProjectId);
	}

	public bool Equals(ProjectId other)
	{
		if ((object)other != null)
		{
			return Id == other.Id;
		}
		return false;
	}

	public static bool operator ==(ProjectId left, ProjectId right)
	{
		return EqualityComparer<ProjectId>.Default.Equals(left, right);
	}

	public static bool operator !=(ProjectId left, ProjectId right)
	{
		return !(left == right);
	}

	public override int GetHashCode()
	{
		return Id.GetHashCode();
	}
}
