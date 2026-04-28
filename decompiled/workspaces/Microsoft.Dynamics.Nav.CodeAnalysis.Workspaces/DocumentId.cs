using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces;

[DebuggerDisplay("{GetDebuggerDisplay(),nq}")]
public sealed class DocumentId : IEquatable<DocumentId>
{
	public ProjectId ProjectId { get; }

	public Guid Id { get; }

	internal string DebugName { get; }

	private DocumentId(ProjectId projectId, string debugName)
	{
		ProjectId = projectId;
		Id = Guid.NewGuid();
		DebugName = debugName;
	}

	internal DocumentId(ProjectId projectId, Guid guid, string debugName)
	{
		ProjectId = projectId;
		Id = guid;
		DebugName = debugName;
	}

	public static DocumentId CreateNewId(ProjectId projectId, string debugName = null)
	{
		if (projectId == null)
		{
			throw new ArgumentNullException("projectId");
		}
		return new DocumentId(projectId, debugName);
	}

	public static DocumentId CreateFromSerialized(ProjectId projectId, Guid id, string debugName = null)
	{
		if (projectId == null)
		{
			throw new ArgumentNullException("projectId");
		}
		if (id == Guid.Empty)
		{
			throw new ArgumentException(WorkspacesResources.IdIsEmpty, "id");
		}
		return new DocumentId(projectId, id, debugName);
	}

	internal string GetDebuggerDisplay()
	{
		return string.Format(CultureInfo.InvariantCulture, "({0}, #{1} - {2})", GetType().Name, Id, DebugName);
	}

	public override string ToString()
	{
		return GetDebuggerDisplay();
	}

	public override bool Equals(object obj)
	{
		return Equals(obj as DocumentId);
	}

	public bool Equals(DocumentId other)
	{
		if ((object)other != null && Id == other.Id)
		{
			return ProjectId == other.ProjectId;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return Hash.Combine(ProjectId.GetHashCode(), Id.GetHashCode());
	}

	public static bool operator ==(DocumentId left, DocumentId right)
	{
		return EqualityComparer<DocumentId>.Default.Equals(left, right);
	}

	public static bool operator !=(DocumentId left, DocumentId right)
	{
		return !(left == right);
	}
}
