using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.FindSymbols;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.LanguageServices;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.FindReferences;

internal class AttributeArgumentSymbolReferenceFinder : AbstractReferenceFinder<IAttributeArgumentSymbol>
{
	protected override bool CanFind(IAttributeArgumentSymbol argumentSymbol)
	{
		return argumentSymbol.ValueAsSymbol != null;
	}

	protected override Func<SyntaxToken, bool> GetTokensMatch(ISyntaxFactsService syntaxFacts, ISymbol symbol)
	{
		return TriggerReferenceHelpers.GetTokenMatchFunction(syntaxFacts, symbol);
	}

	protected override Task<ImmutableArray<Document>> DetermineDocumentsToSearchAsync(IAttributeArgumentSymbol argumentSymbol, Project project, IImmutableSet<Document> documents, CancellationToken cancellationToken)
	{
		using ArrayBuilder<Task<ImmutableArray<Document>>> arrayBuilder = ArrayBuilder<Task<ImmutableArray<Document>>>.GetInstance();
		using PooledNameComparisonHashSet pooledNameComparisonHashSet = PooledNameComparisonHashSet.GetInstance();
		arrayBuilder.Add(FindDocumentsAsync(project, documents, cancellationToken, argumentSymbol.ValueAsSymbol.Name));
		foreach (IMethodSymbol relatedTriggerSymbol in GetRelatedTriggerSymbols(argumentSymbol))
		{
			arrayBuilder.Add(FindDocumentsAsync(project, documents, cancellationToken, relatedTriggerSymbol.Name));
			foreach (IMethodSymbol triggerEventSymbol in TriggerReferenceHelpers.GetTriggerEventSymbols(relatedTriggerSymbol))
			{
				if (pooledNameComparisonHashSet.Add(triggerEventSymbol.Name))
				{
					arrayBuilder.Add(FindDocumentsAsync(project, documents, cancellationToken, triggerEventSymbol.Name));
				}
			}
			foreach (string relatedTriggerAndBuiltInMethodSymbolName in TriggerReferenceHelpers.GetRelatedTriggerAndBuiltInMethodSymbolNames(relatedTriggerSymbol))
			{
				arrayBuilder.Add(FindDocumentsAsync(project, documents, cancellationToken, relatedTriggerAndBuiltInMethodSymbolName));
			}
		}
		return Task<ImmutableArray<Document>>.Factory.ContinueWhenAll(arrayBuilder.ToArray(), AbstractReferenceFinder<IAttributeArgumentSymbol>.Concat);
	}

	protected override Task<ImmutableArray<ReferenceLocation>> FindReferencesInDocumentAsync(IAttributeArgumentSymbol argumentSymbol, Document document, CancellationToken cancellationToken)
	{
		using ArrayBuilder<Task<ImmutableArray<ReferenceLocation>>> arrayBuilder = ArrayBuilder<Task<ImmutableArray<ReferenceLocation>>>.GetInstance();
		using PooledNameComparisonHashSet pooledNameComparisonHashSet = PooledNameComparisonHashSet.GetInstance();
		arrayBuilder.Add(FindReferencesInDocumentUsingSymbolNameAsync(argumentSymbol.ValueAsSymbol, document, cancellationToken));
		pooledNameComparisonHashSet.Add(argumentSymbol.ValueAsSymbol.Name);
		foreach (IMethodSymbol relatedTriggerSymbol in GetRelatedTriggerSymbols(argumentSymbol))
		{
			arrayBuilder.Add(FindReferencesInDocumentUsingSymbolNameAsync(relatedTriggerSymbol, document, cancellationToken));
			foreach (IMethodSymbol triggerEventSymbol in TriggerReferenceHelpers.GetTriggerEventSymbols(relatedTriggerSymbol))
			{
				if (pooledNameComparisonHashSet.Add(triggerEventSymbol.Name))
				{
					arrayBuilder.Add(FindReferencesInDocumentUsingSymbolNameAsync(triggerEventSymbol, document, cancellationToken));
				}
			}
			foreach (string relatedTriggerAndBuiltInMethodSymbolName in TriggerReferenceHelpers.GetRelatedTriggerAndBuiltInMethodSymbolNames(relatedTriggerSymbol))
			{
				arrayBuilder.Add(FindReferencesInDocumentUsingIdentifierAsync(relatedTriggerSymbol, relatedTriggerAndBuiltInMethodSymbolName, document, cancellationToken));
			}
		}
		return Task<ImmutableArray<ReferenceLocation>>.Factory.ContinueWhenAll(arrayBuilder.ToArray(), AbstractReferenceFinder<IAttributeArgumentSymbol>.Concat);
	}

	private static IEnumerable<IMethodSymbol> GetRelatedTriggerSymbols(IAttributeArgumentSymbol argumentSymbol)
	{
		if (argumentSymbol.ValueAsSymbol is BuiltInTriggerEventSymbol builtInTriggerEventSymbol)
		{
			return builtInTriggerEventSymbol.GetRelatedTriggerSymbols();
		}
		return ImmutableArray<IMethodSymbol>.Empty;
	}
}
