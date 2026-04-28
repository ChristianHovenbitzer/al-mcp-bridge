using System;
using System.Diagnostics;
using Newtonsoft.Json;

namespace Microsoft.Dynamics.Nav.DebuggerService;

[DebuggerDisplay("({From.Line}, {From.Column}) => ({To.Line}, {To.Column})")]
[JsonObject]
public struct SourceSpan : IEquatable<SourceSpan>
{
	[JsonIgnore]
	public static readonly SourceSpan Default;

	[JsonProperty]
	public SourcePosition To { get; }

	[JsonProperty]
	public SourcePosition From { get; }

	[JsonConstructor]
	public SourceSpan(SourcePosition from, SourcePosition to)
	{
		To = to;
		From = from;
	}

	public override bool Equals(object obj)
	{
		if (obj is SourceSpan)
		{
			return Equals((SourceSpan)obj);
		}
		return false;
	}

	public static bool operator ==(SourceSpan left, SourceSpan right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(SourceSpan left, SourceSpan right)
	{
		return !(left == right);
	}

	public bool Equals(SourceSpan other)
	{
		if (To == other.To)
		{
			return From == other.From;
		}
		return false;
	}

	public override int GetHashCode()
	{
		int hashCode = From.GetHashCode();
		int hashCode2 = To.GetHashCode();
		return ((hashCode << 5) + hashCode + (hashCode >> 27)) ^ hashCode2;
	}

	public override string ToString()
	{
		return FormattableString.Invariant($"({From.Line}, {From.Column}) => ({To.Line}, {To.Column})");
	}
}
