using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.FindSymbols;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.FindReferences;

internal class GenericReferenceFinder<T> : AbstractReferenceFinder<T> where T : ISymbol
{
	protected override bool CanFind(T symbol)
	{
		return true;
	}

	protected override Task<ImmutableArray<Document>> DetermineDocumentsToSearchAsync(T symbol, Project project, IImmutableSet<Document> documents, CancellationToken cancellationToken)
	{
		return FindDocumentsAsync(project, documents, cancellationToken, symbol.Name);
	}

	protected override Task<ImmutableArray<ReferenceLocation>> FindReferencesInDocumentAsync(T symbol, Document document, CancellationToken cancellationToken)
	{
		return FindReferencesInDocumentUsingSymbolNameAsync(symbol, document, cancellationToken);
	}
}
