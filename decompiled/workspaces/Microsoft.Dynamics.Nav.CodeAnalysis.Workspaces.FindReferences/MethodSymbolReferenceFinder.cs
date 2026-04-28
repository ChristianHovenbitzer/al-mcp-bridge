using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.FindSymbols;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.FindReferences;

internal class MethodSymbolReferenceFinder : AbstractReferenceFinder<IMethodSymbol>
{
	protected override bool CanFind(IMethodSymbol symbol)
	{
		if (!symbol.IsLocal || symbol.IsEvent)
		{
			if (symbol.MethodKind != 0)
			{
				return symbol.MethodKind == MethodKind.DeclareMethod;
			}
			return true;
		}
		return false;
	}

	protected override Task<ImmutableArray<Document>> DetermineDocumentsToSearchAsync(IMethodSymbol symbol, Project project, IImmutableSet<Document> documents, CancellationToken cancellationToken)
	{
		return FindDocumentsAsync(project, documents, cancellationToken, symbol.Name);
	}

	protected override Task<ImmutableArray<ReferenceLocation>> FindReferencesInDocumentAsync(IMethodSymbol symbol, Document document, CancellationToken cancellationToken)
	{
		return FindReferencesInDocumentUsingSymbolNameAsync(symbol, document, cancellationToken);
	}
}
