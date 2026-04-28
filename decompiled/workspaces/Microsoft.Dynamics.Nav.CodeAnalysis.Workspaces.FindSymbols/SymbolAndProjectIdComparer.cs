using System.Collections.Generic;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.FindSymbols;

internal class SymbolAndProjectIdComparer<TSymbol> : IEqualityComparer<SymbolAndProjectId<TSymbol>> where TSymbol : ISymbol
{
	public static readonly SymbolAndProjectIdComparer<TSymbol> SymbolEquivalenceInstance = new SymbolAndProjectIdComparer<TSymbol>();

	private static readonly IEqualityComparer<ISymbol> underlyingComparer = SymbolEquivalenceComparer.Instance;

	private SymbolAndProjectIdComparer()
	{
	}

	public bool Equals(SymbolAndProjectId<TSymbol> x, SymbolAndProjectId<TSymbol> y)
	{
		return underlyingComparer.Equals(x.Symbol, y.Symbol);
	}

	public int GetHashCode(SymbolAndProjectId<TSymbol> obj)
	{
		return underlyingComparer.GetHashCode(obj.Symbol);
	}
}
internal class SymbolAndProjectIdComparer : IEqualityComparer<SymbolAndProjectId>
{
	public static readonly SymbolAndProjectIdComparer SymbolEquivalenceInstance = new SymbolAndProjectIdComparer(SymbolEquivalenceComparer.Instance);

	private readonly IEqualityComparer<ISymbol> underlyingComparer;

	public SymbolAndProjectIdComparer(IEqualityComparer<ISymbol> underlyingComparer)
	{
		this.underlyingComparer = underlyingComparer;
	}

	public bool Equals(SymbolAndProjectId x, SymbolAndProjectId y)
	{
		return underlyingComparer.Equals(x.Symbol, y.Symbol);
	}

	public int GetHashCode(SymbolAndProjectId obj)
	{
		return underlyingComparer.GetHashCode(obj.Symbol);
	}
}
