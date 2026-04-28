using System.Collections.Generic;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Utilities;

internal class SymbolEquivalenceComparer : IEqualityComparer<ISymbol>
{
	public static readonly SymbolEquivalenceComparer Instance = new SymbolEquivalenceComparer();

	public static readonly SymbolEquivalenceComparer IgnoreAssembliesInstance = new SymbolEquivalenceComparer();

	public bool Equals(ISymbol x, ISymbol y)
	{
		return x == y;
	}

	public int GetHashCode(ISymbol x)
	{
		return x.GetHashCode();
	}
}
