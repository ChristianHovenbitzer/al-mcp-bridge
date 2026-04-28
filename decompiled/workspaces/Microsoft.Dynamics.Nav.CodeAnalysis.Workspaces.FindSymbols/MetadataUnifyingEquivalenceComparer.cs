using System.Collections.Generic;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.FindSymbols;

internal sealed class MetadataUnifyingEquivalenceComparer : IEqualityComparer<ISymbol>
{
	public static readonly IEqualityComparer<ISymbol> Instance = new MetadataUnifyingEquivalenceComparer();

	private MetadataUnifyingEquivalenceComparer()
	{
	}

	public bool Equals(ISymbol x, ISymbol y)
	{
		if (x == null || y == null || IsInSource(x) || IsInSource(y))
		{
			return object.Equals(x, y);
		}
		return SymbolEquivalenceComparer.Instance.Equals(x, y);
	}

	public int GetHashCode(ISymbol obj)
	{
		if (IsInSource(obj))
		{
			return obj.GetHashCode();
		}
		return SymbolEquivalenceComparer.Instance.GetHashCode(obj);
	}

	private static bool IsInSource(ISymbol symbol)
	{
		if (symbol.Location != null)
		{
			return symbol.Location.IsInSource;
		}
		return false;
	}
}
