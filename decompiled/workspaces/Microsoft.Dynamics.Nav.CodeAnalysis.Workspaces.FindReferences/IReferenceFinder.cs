using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.FindSymbols;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.FindReferences;

internal interface IReferenceFinder
{
	Task<ImmutableArray<SymbolAndProjectId>> DetermineCascadedSymbolsAsync(SymbolAndProjectId symbolAndProject, Solution solution, IImmutableSet<Project> projects, CancellationToken cancellationToken);

	Task<ImmutableArray<Project>> DetermineProjectsToSearchAsync(ISymbol symbol, Solution solution, IImmutableSet<Project> projects, CancellationToken cancellationToken);

	Task<ImmutableArray<Document>> DetermineDocumentsToSearchAsync(ISymbol symbol, Project project, IImmutableSet<Document> documents, CancellationToken cancellationToken);

	Task<ImmutableArray<ReferenceLocation>> FindReferencesInDocumentAsync(SymbolAndProjectId symbolAndProjectId, Document document, CancellationToken cancellationToken);
}
