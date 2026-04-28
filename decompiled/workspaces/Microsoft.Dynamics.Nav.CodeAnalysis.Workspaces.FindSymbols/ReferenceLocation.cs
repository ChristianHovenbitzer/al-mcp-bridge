using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.FindSymbols;

[DebuggerDisplay("{GetDebuggerDisplay(),nq}")]
public struct ReferenceLocation : IComparable<ReferenceLocation>, IEquatable<ReferenceLocation>
{
	internal bool IsDuplicateReferenceLocation;

	public Document Document { get; }

	public Location Location { get; }

	public bool IsImplicit { get; }

	internal bool IsWrittenTo { get; }

	public CandidateReason CandidateReason { get; }

	public bool IsCandidateLocation => CandidateReason != CandidateReason.None;

	internal ReferenceLocation(Document document, Location location, bool isImplicit, bool isWrittenTo, CandidateReason candidateReason)
	{
		this = default(ReferenceLocation);
		Document = document;
		Location = location;
		IsImplicit = isImplicit;
		IsWrittenTo = isWrittenTo;
		CandidateReason = candidateReason;
	}

	public static bool operator ==(ReferenceLocation left, ReferenceLocation right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(ReferenceLocation left, ReferenceLocation right)
	{
		return !(left == right);
	}

	public override bool Equals(object obj)
	{
		if (obj is ReferenceLocation)
		{
			return Equals((ReferenceLocation)obj);
		}
		return false;
	}

	public bool Equals(ReferenceLocation other)
	{
		if (EqualityComparer<Microsoft.Dynamics.Nav.CodeAnalysis.Text.Location>.Default.Equals(Location, other.Location) && EqualityComparer<DocumentId>.Default.Equals(Document.Id, other.Document.Id) && CandidateReason == other.CandidateReason)
		{
			return IsImplicit == other.IsImplicit;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return Hash.Combine(IsImplicit.GetHashCode(), Hash.Combine((int)CandidateReason, Hash.Combine(Location.GetHashCode(), Document.Id.GetHashCode())));
	}

	public int CompareTo(ReferenceLocation other)
	{
		string filePath = Location.SourceTree.FilePath;
		string filePath2 = other.Location.SourceTree.FilePath;
		int result;
		if ((result = StringComparer.OrdinalIgnoreCase.Compare(filePath, filePath2)) != 0 || (result = Location.SourceSpan.CompareTo(other.Location.SourceSpan)) != 0)
		{
			return result;
		}
		return 0;
	}

	private string GetDebuggerDisplay()
	{
		return string.Format(CultureInfo.InvariantCulture, "{0}: {1}", Document.Name, Location);
	}
}
