using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.FindSymbols;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Host;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.LanguageServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.FindReferences;

public class FindReferencesService : IFindReferencesService, ILanguageService
{
	public IEnumerable<ReferencedSymbol> FindReferencedSymbolsAsync(Document document, int position, CancellationToken cancellationToken)
	{
		return FindReferencedSymbolsInternalAsync(document, position, cancellationToken).WaitAndGetResult(cancellationToken)?.Item1 ?? SpecializedCollections.EmptyEnumerable<ReferencedSymbol>();
	}

	private async Task<Tuple<IEnumerable<ReferencedSymbol>, Solution>> FindReferencedSymbolsInternalAsync(Document document, int position, CancellationToken cancellationToken)
	{
		Tuple<ISymbol, Project> tuple = await FindUsagesHelpers.GetRelevantSymbolAndProjectAtPositionAsync(document, position, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		if (tuple == null)
		{
			return null;
		}
		ISymbol symbol = tuple?.Item1;
		Project project = tuple?.Item2;
		return Tuple.Create(await SymbolFinder.FindReferencesAsync(symbol, project.Solution, cancellationToken).ConfigureAwait(continueOnCapturedContext: false), project.Solution);
	}
}
