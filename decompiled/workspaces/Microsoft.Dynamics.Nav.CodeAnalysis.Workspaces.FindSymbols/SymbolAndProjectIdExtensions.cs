using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.FindSymbols;

internal static class SymbolAndProjectIdExtensions
{
	public static IEnumerable<SymbolAndProjectId<TConvert>> Convert<TOriginal, TConvert>(this IEnumerable<SymbolAndProjectId<TOriginal>> list) where TOriginal : ISymbol where TConvert : ISymbol
	{
		return list.Select<SymbolAndProjectId<TOriginal>, SymbolAndProjectId<TConvert>>((SymbolAndProjectId<TOriginal> s) => SymbolAndProjectId.Create((TConvert)(object)s.Symbol, s.ProjectId));
	}
}
