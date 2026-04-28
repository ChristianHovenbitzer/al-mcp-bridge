using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.FindSymbols;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.FindReferences;

internal static class FindUsagesHelpers
{
	public static async Task<Tuple<ISymbol, Project>> GetRelevantSymbolAndProjectAtPositionAsync(Document document, int position, CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();
		ISymbol symbol = await SymbolFinder.FindSymbolAtPositionAsync(document, position, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		if (symbol == null)
		{
			return null;
		}
		return Tuple.Create(symbol, document.Project);
	}
}
