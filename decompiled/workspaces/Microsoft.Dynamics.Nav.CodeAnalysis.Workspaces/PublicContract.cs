using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces;

internal static class PublicContract
{
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static IEnumerable<T> RequireNonNullItems<T>(IEnumerable<T>? sequence, string argumentName) where T : class
	{
		if (sequence == null)
		{
			throw new ArgumentNullException(argumentName);
		}
		if (Enumerable.Contains(sequence, null))
		{
			ThrowArgumentItemNullException(sequence, argumentName);
		}
		return sequence;
	}

	private static string MakeIndexedArgumentName(string argumentName, int index)
	{
		return $"{argumentName}[{index}]";
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private static void ThrowArgumentItemNullException<T>(IEnumerable<T> sequence, string argumentName) where T : class
	{
		throw new ArgumentNullException(MakeIndexedArgumentName(argumentName, sequence.IndexOf(null)));
	}
}
