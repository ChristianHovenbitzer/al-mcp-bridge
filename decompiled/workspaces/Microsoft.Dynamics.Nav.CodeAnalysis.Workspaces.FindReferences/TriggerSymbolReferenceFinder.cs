using System;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.FindSymbols;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.LanguageServices;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.FindReferences;

internal class TriggerSymbolReferenceFinder : AbstractReferenceFinder<IMethodSymbol>
{
	protected override bool CanFind(IMethodSymbol symbol)
	{
		return symbol.MethodKind == MethodKind.Trigger;
	}

	protected override Func<SyntaxToken, bool> GetTokensMatch(ISyntaxFactsService syntaxFacts, ISymbol symbol)
	{
		return TriggerReferenceHelpers.GetTokenMatchFunction(syntaxFacts, symbol);
	}

	protected override Task<ImmutableArray<Document>> DetermineDocumentsToSearchAsync(IMethodSymbol triggerSymbol, Project project, IImmutableSet<Document> documents, CancellationToken cancellationToken)
	{
		using ArrayBuilder<Task<ImmutableArray<Document>>> arrayBuilder = ArrayBuilder<Task<ImmutableArray<Document>>>.GetInstance();
		arrayBuilder.Add(FindDocumentsAsync(project, documents, cancellationToken, triggerSymbol.Name));
		foreach (string triggerEventSymbolName in TriggerReferenceHelpers.GetTriggerEventSymbolNames(triggerSymbol))
		{
			arrayBuilder.Add(FindDocumentsAsync(project, documents, cancellationToken, triggerEventSymbolName));
		}
		foreach (string relatedTriggerAndBuiltInMethodSymbolName in TriggerReferenceHelpers.GetRelatedTriggerAndBuiltInMethodSymbolNames(triggerSymbol))
		{
			arrayBuilder.Add(FindDocumentsAsync(project, documents, cancellationToken, relatedTriggerAndBuiltInMethodSymbolName));
		}
		return Task<ImmutableArray<Document>>.Factory.ContinueWhenAll(arrayBuilder.ToArray(), AbstractReferenceFinder<IMethodSymbol>.Concat);
	}

	protected override Task<ImmutableArray<ReferenceLocation>> FindReferencesInDocumentAsync(IMethodSymbol triggerSymbol, Document document, CancellationToken cancellationToken)
	{
		using ArrayBuilder<Task<ImmutableArray<ReferenceLocation>>> arrayBuilder = ArrayBuilder<Task<ImmutableArray<ReferenceLocation>>>.GetInstance();
		arrayBuilder.Add(FindReferencesInDocumentUsingSymbolNameAsync(triggerSymbol, document, cancellationToken));
		foreach (IMethodSymbol triggerEventSymbol in TriggerReferenceHelpers.GetTriggerEventSymbols(triggerSymbol))
		{
			arrayBuilder.Add(FindReferencesInDocumentUsingSymbolNameAsync(triggerEventSymbol, document, cancellationToken));
		}
		foreach (string relatedTriggerAndBuiltInMethodSymbolName in TriggerReferenceHelpers.GetRelatedTriggerAndBuiltInMethodSymbolNames(triggerSymbol))
		{
			arrayBuilder.Add(FindReferencesInDocumentUsingIdentifierAsync(triggerSymbol, relatedTriggerAndBuiltInMethodSymbolName, document, cancellationToken));
		}
		return Task<ImmutableArray<ReferenceLocation>>.Factory.ContinueWhenAll(arrayBuilder.ToArray(), AbstractReferenceFinder<IMethodSymbol>.Concat);
	}
}
