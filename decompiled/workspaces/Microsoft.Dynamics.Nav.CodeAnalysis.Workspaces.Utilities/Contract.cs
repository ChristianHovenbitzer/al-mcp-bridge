using System;
using System.Diagnostics;
using System.Globalization;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Utilities;

internal static class Contract
{
	[Conditional("DEBUG")]
	[DebuggerHidden]
	public static void Requires(bool condition, string message = null)
	{
	}

	[Conditional("DEBUG")]
	[DebuggerHidden]
	public static void Assert(bool condition, string message = null)
	{
		if (!condition)
		{
			string.IsNullOrEmpty(message);
		}
	}

	[Conditional("DEBUG")]
	public static void Assume(bool condition, string message = null)
	{
		string.IsNullOrEmpty(message);
	}

	public static void ThrowIfNull<T>(T value, string message = null) where T : class
	{
		if (value == null)
		{
			message = message ?? "Unexpected Null";
			Fail(message);
		}
	}

	public static void ThrowIfFalse(bool condition, string message = null)
	{
		if (!condition)
		{
			message = message ?? "Unexpected false";
			Fail(message);
		}
	}

	public static void ThrowIfTrue(bool condition, string message = null)
	{
		if (condition)
		{
			message = message ?? "Unexpected true";
			Fail(message);
		}
	}

	[DebuggerHidden]
	public static void Fail(string message = "Unexpected")
	{
		throw new InvalidOperationException(message);
	}

	[DebuggerHidden]
	public static T FailWithReturn<T>(string message = "Unexpected")
	{
		throw new InvalidOperationException(message);
	}

	public static void InvalidEnumValue<T>(T value)
	{
		Fail(string.Format(CultureInfo.InvariantCulture, "Invalid Enumeration value {0}", value));
	}
}
