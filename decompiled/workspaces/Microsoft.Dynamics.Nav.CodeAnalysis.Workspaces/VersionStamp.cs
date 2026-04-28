using System;
using System.Globalization;
using System.Threading;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces;

public struct VersionStamp : IEquatable<VersionStamp>
{
	private const int GlobalVersionMarker = -1;

	private const int InitialGlobalVersion = 10000;

	private static int globalVersion = 10000;

	private readonly DateTime utcLastModified;

	private readonly int localIncrement;

	private readonly int globalIncrement;

	public static VersionStamp Default => default(VersionStamp);

	private VersionStamp(DateTime utcLastModified)
		: this(utcLastModified, 0)
	{
	}

	private VersionStamp(DateTime utcLastModified, int localIncrement)
	{
		this.utcLastModified = utcLastModified;
		this.localIncrement = localIncrement;
		globalIncrement = GetNextGlobalVersion();
	}

	private VersionStamp(DateTime utcLastModified, int localIncrement, int globalIncrement)
	{
		Contract.ThrowIfFalse(utcLastModified == default(DateTime) || utcLastModified.Kind == DateTimeKind.Utc);
		this.utcLastModified = utcLastModified;
		this.localIncrement = localIncrement;
		this.globalIncrement = globalIncrement;
	}

	public static VersionStamp Create()
	{
		return new VersionStamp(DateTime.UtcNow);
	}

	public static VersionStamp Create(DateTime utcTimeLastModified)
	{
		return new VersionStamp(utcTimeLastModified);
	}

	public VersionStamp GetNewerVersion(VersionStamp version)
	{
		if (utcLastModified > version.utcLastModified)
		{
			return this;
		}
		if (utcLastModified == version.utcLastModified)
		{
			int num = GetGlobalVersion(this);
			int num2 = GetGlobalVersion(version);
			if (num == num2)
			{
				return this;
			}
			return new VersionStamp(utcLastModified, (num > num2) ? num : num2, -1);
		}
		return version;
	}

	public VersionStamp GetNewerVersion()
	{
		DateTime utcNow = DateTime.UtcNow;
		int num = ((utcNow == utcLastModified) ? (localIncrement + 1) : 0);
		return new VersionStamp(utcNow, num);
	}

	public override string ToString()
	{
		return utcLastModified.ToString("o") + "-" + globalIncrement.ToString(CultureInfo.InvariantCulture) + "-" + localIncrement.ToString(CultureInfo.InvariantCulture);
	}

	public override int GetHashCode()
	{
		return Hash.Combine(utcLastModified.GetHashCode(), localIncrement);
	}

	public override bool Equals(object obj)
	{
		if (obj is VersionStamp)
		{
			return Equals((VersionStamp)obj);
		}
		return false;
	}

	public bool Equals(VersionStamp other)
	{
		if (utcLastModified == other.utcLastModified)
		{
			return GetGlobalVersion(this) == GetGlobalVersion(other);
		}
		return false;
	}

	public static bool operator ==(VersionStamp left, VersionStamp right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(VersionStamp left, VersionStamp right)
	{
		return !left.Equals(right);
	}

	internal static bool CanReusePersistedVersion(VersionStamp baseVersion, VersionStamp persistedVersion)
	{
		if (baseVersion == persistedVersion)
		{
			return true;
		}
		if (baseVersion.localIncrement != 0 || persistedVersion.localIncrement != 0)
		{
			return false;
		}
		return baseVersion.utcLastModified == persistedVersion.utcLastModified;
	}

	private static int GetGlobalVersion(VersionStamp version)
	{
		if (version.globalIncrement < 0)
		{
			return version.localIncrement;
		}
		return version.globalIncrement;
	}

	private static int GetNextGlobalVersion()
	{
		return Interlocked.Increment(ref globalVersion);
	}
}
