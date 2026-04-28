using System;
using System.Diagnostics;
using System.Globalization;
using Newtonsoft.Json;

namespace Microsoft.Dynamics.Nav.DebuggerService;

[DebuggerDisplay("({Line}, {Column})")]
[JsonObject]
public struct SourcePosition : IEquatable<SourcePosition>
{
	[JsonIgnore]
	public static readonly SourcePosition Default;

	[JsonProperty]
	public ushort Line { get; }

	[JsonProperty]
	public ushort Column { get; }

	[JsonConstructor]
	public SourcePosition(ushort line, ushort column)
	{
		Line = line;
		Column = column;
	}

	public override bool Equals(object obj)
	{
		if (obj is SourcePosition)
		{
			return Equals((SourcePosition)obj);
		}
		return false;
	}

	public static bool operator ==(SourcePosition left, SourcePosition right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(SourcePosition left, SourcePosition right)
	{
		return !(left == right);
	}

	public bool Equals(SourcePosition other)
	{
		if (Line == other.Line)
		{
			return Column == other.Column;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return ((Line << 5) + Line + (Line >> 27)) ^ Column;
	}

	public override string ToString()
	{
		return string.Format(CultureInfo.InvariantCulture, "({0}-{1})", Line, Column);
	}
}
