using System.Collections.Generic;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Utilities;

internal sealed class LinkedFilesSymbolEquivalenceComparer : IEqualityComparer<ISymbol>
{
	public static readonly LinkedFilesSymbolEquivalenceComparer Instance = new LinkedFilesSymbolEquivalenceComparer();

	bool IEqualityComparer<ISymbol>.Equals(ISymbol x, ISymbol y)
	{
		return x.Name == y.Name;
	}

	int IEqualityComparer<ISymbol>.GetHashCode(ISymbol symbol)
	{
		return symbol.Name.GetHashCode();
	}
}
