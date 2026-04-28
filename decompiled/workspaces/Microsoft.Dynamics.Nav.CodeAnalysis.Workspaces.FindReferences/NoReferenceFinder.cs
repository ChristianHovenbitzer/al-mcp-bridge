using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.FindSymbols;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.FindReferences;

internal class NoReferenceFinder<T> : AbstractReferenceFinder<T> where T : ISymbol
{
	protected override bool CanFind(T symbol)
	{
		return false;
	}

	protected override Task<ImmutableArray<Document>> DetermineDocumentsToSearchAsync(T symbol, Project project, IImmutableSet<Document> documents, CancellationToken cancellationToken)
	{
		return Task.FromResult(ImmutableArray<Document>.Empty);
	}

	protected override Task<ImmutableArray<ReferenceLocation>> FindReferencesInDocumentAsync(T symbol, Document document, CancellationToken cancellationToken)
	{
		return Task.FromResult(ImmutableArray<ReferenceLocation>.Empty);
	}
}
