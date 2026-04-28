using System;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.FindSymbols;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.LanguageServices;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.FindReferences;

internal class BuiltInMethodSymbolReferenceFinder : AbstractReferenceFinder<IMethodSymbol>
{
	protected override bool CanFind(IMethodSymbol symbol)
	{
		return symbol.MethodKind == MethodKind.BuiltInMethod;
	}

	protected override Func<SyntaxToken, bool> GetTokensMatch(ISyntaxFactsService syntaxFacts, ISymbol symbol)
	{
		return BuiltInMethodReferenceHelper.GetTokenMatchFunction(syntaxFacts, symbol);
	}

	protected override Task<ImmutableArray<Document>> DetermineDocumentsToSearchAsync(IMethodSymbol methodSymbol, Project project, IImmutableSet<Document> documents, CancellationToken cancellationToken)
	{
		using ArrayBuilder<Task<ImmutableArray<Document>>> arrayBuilder = ArrayBuilder<Task<ImmutableArray<Document>>>.GetInstance();
		if (methodSymbol.ContainingType.Kind == SymbolKind.Class)
		{
			arrayBuilder.Add(FindDocumentsAsync(project, documents, cancellationToken, methodSymbol.Name));
		}
		else
		{
			foreach (string allRelatedSymbolName in BuiltInMethodReferenceHelper.GetAllRelatedSymbolNames(methodSymbol))
			{
				arrayBuilder.Add(FindDocumentsAsync(project, documents, cancellationToken, allRelatedSymbolName));
			}
		}
		return Task<ImmutableArray<Document>>.Factory.ContinueWhenAll(arrayBuilder.ToArray(), AbstractReferenceFinder<IMethodSymbol>.Concat);
	}

	protected override Task<ImmutableArray<ReferenceLocation>> FindReferencesInDocumentAsync(IMethodSymbol methodSymbol, Document document, CancellationToken cancellationToken)
	{
		using ArrayBuilder<Task<ImmutableArray<ReferenceLocation>>> arrayBuilder = ArrayBuilder<Task<ImmutableArray<ReferenceLocation>>>.GetInstance();
		if (methodSymbol.ContainingType.Kind == SymbolKind.Class)
		{
			arrayBuilder.Add(FindReferencesInDocumentUsingIdentifierAsync(methodSymbol, methodSymbol.Name, document, cancellationToken));
		}
		else
		{
			foreach (string allRelatedSymbolName in BuiltInMethodReferenceHelper.GetAllRelatedSymbolNames(methodSymbol))
			{
				arrayBuilder.Add(FindReferencesInDocumentUsingIdentifierAsync(methodSymbol, allRelatedSymbolName, document, cancellationToken));
			}
		}
		return Task<ImmutableArray<ReferenceLocation>>.Factory.ContinueWhenAll(arrayBuilder.ToArray(), AbstractReferenceFinder<IMethodSymbol>.Concat);
	}
}
