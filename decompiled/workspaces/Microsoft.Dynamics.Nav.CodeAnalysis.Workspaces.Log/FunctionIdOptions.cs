using System;
using System.Collections.Concurrent;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Options;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Log;

internal static class FunctionIdOptions
{
	private const string FeatureName = "Performance/FunctionId";

	private static readonly ConcurrentDictionary<FunctionId, Option<bool>> Options = new ConcurrentDictionary<FunctionId, Option<bool>>();

	private static readonly Func<FunctionId, Option<bool>> OptionGetter = (FunctionId id) => new Option<bool>("Performance/FunctionId", Enum.GetName(typeof(FunctionId), id), GetDefaultValue(id));

	public static Option<bool> GetOption(FunctionId id)
	{
		return Options.GetOrAdd(id, OptionGetter);
	}

	private static bool GetDefaultValue(FunctionId id)
	{
		return false;
	}
}
