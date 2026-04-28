using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.FindSymbols;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.FindReferences;

internal class OptionValueSymbolReferenceFinder : AbstractReferenceFinder<IOptionSymbol>
{
	protected override bool CanFind(IOptionSymbol symbol)
	{
		return true;
	}

	protected override Task<ImmutableArray<Document>> DetermineDocumentsToSearchAsync(IOptionSymbol symbol, Project project, IImmutableSet<Document> documents, CancellationToken cancellationToken)
	{
		return FindDocumentsAsync(project, documents, cancellationToken, symbol.Name);
	}

	protected override Task<ImmutableArray<ReferenceLocation>> FindReferencesInDocumentAsync(IOptionSymbol symbol, Document document, CancellationToken cancellationToken)
	{
		return FindReferencesInDocumentUsingSymbolNameAsync(symbol, document, cancellationToken);
	}
}
