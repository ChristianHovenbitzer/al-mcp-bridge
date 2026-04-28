using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.FindSymbols;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.FindReferences;

internal class EventSymbolReferenceFinder : AbstractReferenceFinder<IEventSymbol>
{
	protected override bool CanFind(IEventSymbol symbol)
	{
		return true;
	}

	protected override Task<ImmutableArray<Document>> DetermineDocumentsToSearchAsync(IEventSymbol symbol, Project project, IImmutableSet<Document> documents, CancellationToken cancellationToken)
	{
		return FindDocumentsAsync(project, documents, cancellationToken, symbol.Name);
	}

	protected override Task<ImmutableArray<ReferenceLocation>> FindReferencesInDocumentAsync(IEventSymbol symbol, Document document, CancellationToken cancellationToken)
	{
		return FindReferencesInDocumentUsingSymbolNameAsync(symbol, document, cancellationToken);
	}
}
