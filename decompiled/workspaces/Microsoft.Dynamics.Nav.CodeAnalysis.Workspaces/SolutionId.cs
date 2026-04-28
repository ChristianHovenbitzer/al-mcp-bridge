using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces;

[DebuggerDisplay("{GetDebuggerDisplay(),nq}")]
public sealed class SolutionId : IEquatable<SolutionId>
{
	private readonly string debugName;

	public Guid Id { get; }

	private SolutionId(string debugName)
	{
		Id = Guid.NewGuid();
		this.debugName = debugName;
	}

	public static SolutionId CreateNewId(string debugName = null)
	{
		debugName = debugName ?? "unsaved";
		return new SolutionId(debugName);
	}

	private string GetDebuggerDisplay()
	{
		return string.Format(CultureInfo.InvariantCulture, "({0}, #{1} - {2})", GetType().Name, Id, debugName);
	}

	public override bool Equals(object obj)
	{
		return Equals(obj as SolutionId);
	}

	public bool Equals(SolutionId other)
	{
		if ((object)other != null)
		{
			return Id == other.Id;
		}
		return false;
	}

	public static bool operator ==(SolutionId left, SolutionId right)
	{
		return EqualityComparer<SolutionId>.Default.Equals(left, right);
	}

	public static bool operator !=(SolutionId left, SolutionId right)
	{
		return !(left == right);
	}

	public override int GetHashCode()
	{
		return Id.GetHashCode();
	}
}
