using System.Collections.Generic;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces;

internal static class ITypeSymbolExtensions
{
	internal static IEnumerable<ITypeSymbol> GetContainingTypesAndThis(this ITypeSymbol type)
	{
		for (ITypeSymbol current = type; current != null; current = current.ContainingType)
		{
			yield return current;
		}
	}

	internal static IEnumerable<ITypeSymbol> GetContainingTypes(this ITypeSymbol type)
	{
		for (ITypeSymbol current = type.ContainingType; current != null; current = current.ContainingType)
		{
			yield return current;
		}
	}

	internal static bool IsOptionType(this ITypeSymbol type)
	{
		return type.Kind == SymbolKind.Option;
	}
}
