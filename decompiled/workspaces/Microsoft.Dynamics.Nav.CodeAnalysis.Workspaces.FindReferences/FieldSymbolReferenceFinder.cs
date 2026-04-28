using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.FindSymbols;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.FindReferences;

internal class FieldSymbolReferenceFinder : AbstractReferenceFinder<IFieldSymbol>
{
	protected override bool CanFind(IFieldSymbol symbol)
	{
		return true;
	}

	protected override Task<ImmutableArray<Document>> DetermineDocumentsToSearchAsync(IFieldSymbol symbol, Project project, IImmutableSet<Document> documents, CancellationToken cancellationToken)
	{
		return FindDocumentsAsync(project, documents, cancellationToken, symbol.Name);
	}

	protected override Task<ImmutableArray<ReferenceLocation>> FindReferencesInDocumentAsync(IFieldSymbol symbol, Document document, CancellationToken cancellationToken)
	{
		return FindReferencesInDocumentUsingSymbolNameAsync(symbol, document, cancellationToken);
	}
}
