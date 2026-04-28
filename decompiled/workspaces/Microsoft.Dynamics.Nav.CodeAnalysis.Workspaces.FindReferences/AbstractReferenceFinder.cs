using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.FindSymbols;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.LanguageServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.FindReferences;

internal abstract class AbstractReferenceFinder : IReferenceFinder
{
	public abstract Task<ImmutableArray<SymbolAndProjectId>> DetermineCascadedSymbolsAsync(SymbolAndProjectId symbolAndProject, Solution solution, IImmutableSet<Project> projects, CancellationToken cancellationToken);

	public abstract Task<ImmutableArray<Project>> DetermineProjectsToSearchAsync(ISymbol symbol, Solution solution, IImmutableSet<Project> projects, CancellationToken cancellationToken);

	public abstract Task<ImmutableArray<Document>> DetermineDocumentsToSearchAsync(ISymbol symbol, Project project, IImmutableSet<Document> documents, CancellationToken cancellationToken);

	public abstract Task<ImmutableArray<ReferenceLocation>> FindReferencesInDocumentAsync(SymbolAndProjectId symbolAndProjectId, Document document, CancellationToken cancellationToken);

	protected async Task<ImmutableArray<Document>> FindDocumentsAsync(Project project, IImmutableSet<Document> scope, Func<Document, CancellationToken, Task<bool>> predicateAsync, CancellationToken cancellationToken)
	{
		if (scope != null && scope.Count == 1)
		{
			if (scope.First().Project == project)
			{
				return scope.ToImmutableArray();
			}
			return ImmutableArray<Document>.Empty;
		}
		ArrayBuilder<Document> documents = ArrayBuilder<Document>.GetInstance();
		foreach (Document document in project.Documents)
		{
			if ((scope == null || scope.Contains(document)) && await predicateAsync(document, cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
			{
				documents.Add(document);
			}
		}
		return documents.ToImmutableAndFree();
	}

	protected Task<ImmutableArray<Document>> FindDocumentsAsync(Project project, IImmutableSet<Document> documents, CancellationToken cancellationToken, params string[] values)
	{
		string[] values2 = values;
		return FindDocumentsAsync(project, documents, async delegate(Document d, CancellationToken c)
		{
			SyntaxTreeIndex syntaxTreeIndex = await SyntaxTreeIndex.GetIndexAsync(d, c).ConfigureAwait(continueOnCapturedContext: false);
			string[] array = values2;
			foreach (string identifier in array)
			{
				if (!syntaxTreeIndex.ProbablyContainsIdentifier(identifier))
				{
					return false;
				}
			}
			return true;
		}, cancellationToken);
	}

	protected static bool IdentifiersMatch(ISyntaxFactsService syntaxFacts, string name, SyntaxToken token)
	{
		if (syntaxFacts.IsIdentifier(token))
		{
			return SemanticFacts.IsSameName(token.ValueText.UnquoteIdentifier(), name);
		}
		return false;
	}

	protected virtual Func<SyntaxToken, bool> GetTokensMatch(ISyntaxFactsService syntaxFacts, ISymbol symbol)
	{
		ISyntaxFactsService syntaxFacts2 = syntaxFacts;
		ISymbol symbol2 = symbol;
		return (SyntaxToken t) => IdentifiersMatch(syntaxFacts2, symbol2.Name, t);
	}

	protected Task<ImmutableArray<ReferenceLocation>> FindReferencesInDocumentUsingIdentifierAsync(ISymbol symbol, string identifier, Document document, CancellationToken cancellationToken)
	{
		return FindReferencesInDocumentUsingIdentifierAsync(symbol, identifier, document, null, cancellationToken);
	}

	protected Task<ImmutableArray<ReferenceLocation>> FindReferencesInDocumentUsingIdentifierAsync(ISymbol symbol, string identifier, Document document, Func<SyntaxToken, SyntaxNode> findParentNode, CancellationToken cancellationToken)
	{
		Func<SyntaxToken, SemanticModel, Tuple<bool, CandidateReason>> standardSymbolsMatchFunction = GetStandardSymbolsMatchFunction(symbol, findParentNode, cancellationToken);
		ISyntaxFactsService languageService = document.GetLanguageService<ISyntaxFactsService>();
		Func<SyntaxToken, bool> tokensMatch = GetTokensMatch(languageService, symbol);
		return FindReferencesInDocumentUsingIdentifierAsync(identifier, document, tokensMatch, standardSymbolsMatchFunction, cancellationToken);
	}

	protected async Task<ImmutableArray<ReferenceLocation>> FindReferencesInDocumentUsingIdentifierAsync(string identifier, Document document, Func<SyntaxToken, bool> tokensMatch, Func<SyntaxToken, SemanticModel, Tuple<bool, CandidateReason>> symbolsMatch, CancellationToken cancellationToken)
	{
		return await FindReferencesInTokensAsync(document, await document.GetIdentifierTokensWithTextAsync(identifier, cancellationToken).ConfigureAwait(continueOnCapturedContext: false), tokensMatch, symbolsMatch, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
	}

	protected static Func<SyntaxToken, SemanticModel, Tuple<bool, CandidateReason>> GetStandardSymbolsMatchFunction(ISymbol symbol, Func<SyntaxToken, SyntaxNode> findParentNode, CancellationToken cancellationToken)
	{
		Func<SyntaxToken, SyntaxNode> findParentNode2 = findParentNode;
		Func<SyntaxNode, SemanticModel, Tuple<bool, CandidateReason>> nodeMatch = GetStandardSymbolsNodeMatchFunction(symbol, cancellationToken);
		findParentNode2 = findParentNode2 ?? ((Func<SyntaxToken, SyntaxNode>)((SyntaxToken t) => t.Parent));
		return (SyntaxToken token, SemanticModel model) => nodeMatch(findParentNode2(token), model);
	}

	protected static Func<SyntaxNode, SemanticModel, Tuple<bool, CandidateReason>> GetStandardSymbolsNodeMatchFunction(ISymbol searchSymbol, CancellationToken cancellationToken)
	{
		ISymbol searchSymbol2 = searchSymbol;
		return delegate(SyntaxNode node, SemanticModel model)
		{
			SyntaxNode node2 = node;
			SemanticModel model2 = model;
			SymbolInfo symbolInfo = FindReferenceCache.GetSymbolInfo(model2, node2, cancellationToken);
			if (SymbolFinder.OriginalSymbolsMatch(searchSymbol2, symbolInfo.Symbol, node2, model2, cancellationToken))
			{
				return Tuple.Create(item1: true, CandidateReason.None);
			}
			return symbolInfo.CandidateSymbols.Any((ISymbol s) => SymbolFinder.OriginalSymbolsMatch(searchSymbol2, s, node2, model2, cancellationToken)) ? Tuple.Create(item1: true, symbolInfo.CandidateReason) : Tuple.Create(item1: false, CandidateReason.None);
		};
	}

	protected static async Task<ImmutableArray<ReferenceLocation>> FindReferencesInTokensAsync(Document document, IEnumerable<SyntaxToken> tokens, Func<SyntaxToken, bool> tokensMatch, Func<SyntaxToken, SemanticModel, Tuple<bool, CandidateReason>> symbolsMatch, CancellationToken cancellationToken)
	{
		SemanticModel arg = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		ArrayBuilder<ReferenceLocation> instance = ArrayBuilder<ReferenceLocation>.GetInstance();
		foreach (SyntaxToken token in tokens)
		{
			cancellationToken.ThrowIfCancellationRequested();
			if (tokensMatch(token))
			{
				Tuple<bool, CandidateReason> tuple = symbolsMatch(token, arg);
				if (tuple.Item1)
				{
					Location location = token.GetLocation();
					bool isWrittenTo = true;
					instance.Add(new ReferenceLocation(document, location, isImplicit: false, isWrittenTo, tuple.Item2));
				}
			}
		}
		return instance.ToImmutableAndFree();
	}
}
internal abstract class AbstractReferenceFinder<TSymbol> : AbstractReferenceFinder where TSymbol : ISymbol
{
	public override Task<ImmutableArray<Project>> DetermineProjectsToSearchAsync(ISymbol symbol, Solution solution, IImmutableSet<Project> projects, CancellationToken cancellationToken)
	{
		if (!(symbol is TSymbol) || !CanFind((TSymbol)symbol))
		{
			return SpecializedTasks.EmptyImmutableArray<Project>();
		}
		return DetermineProjectsToSearchAsync((TSymbol)symbol, solution, projects, cancellationToken);
	}

	public override Task<ImmutableArray<Document>> DetermineDocumentsToSearchAsync(ISymbol symbol, Project project, IImmutableSet<Document> documents, CancellationToken cancellationToken)
	{
		if (!(symbol is TSymbol) || !CanFind((TSymbol)symbol))
		{
			return SpecializedTasks.EmptyImmutableArray<Document>();
		}
		return DetermineDocumentsToSearchAsync((TSymbol)symbol, project, documents, cancellationToken);
	}

	public override Task<ImmutableArray<ReferenceLocation>> FindReferencesInDocumentAsync(SymbolAndProjectId symbolAndProjectId, Document document, CancellationToken cancellationToken)
	{
		ISymbol symbol = symbolAndProjectId.Symbol;
		if (!(symbol is TSymbol) || !CanFind((TSymbol)symbol))
		{
			return SpecializedTasks.EmptyImmutableArray<ReferenceLocation>();
		}
		return FindReferencesInDocumentAsync((TSymbol)symbol, document, cancellationToken);
	}

	public override Task<ImmutableArray<SymbolAndProjectId>> DetermineCascadedSymbolsAsync(SymbolAndProjectId symbolAndProjectId, Solution solution, IImmutableSet<Project> projects, CancellationToken cancellationToken)
	{
		ISymbol symbol = symbolAndProjectId.Symbol;
		if (symbol is TSymbol && CanFind((TSymbol)symbol))
		{
			return DetermineCascadedSymbolsAsync(symbolAndProjectId.WithSymbol((TSymbol)symbol), solution, projects, cancellationToken);
		}
		return SpecializedTasks.EmptyImmutableArray<SymbolAndProjectId>();
	}

	protected abstract bool CanFind(TSymbol symbol);

	protected abstract Task<ImmutableArray<Document>> DetermineDocumentsToSearchAsync(TSymbol symbol, Project project, IImmutableSet<Document> documents, CancellationToken cancellationToken);

	protected abstract Task<ImmutableArray<ReferenceLocation>> FindReferencesInDocumentAsync(TSymbol symbol, Document document, CancellationToken cancellationToken);

	protected virtual Task<ImmutableArray<Project>> DetermineProjectsToSearchAsync(TSymbol symbol, Solution solution, IImmutableSet<Project> projects, CancellationToken cancellationToken)
	{
		return Task.FromResult(solution.Projects.ToImmutableArrayOrEmpty());
	}

	protected virtual Task<ImmutableArray<SymbolAndProjectId>> DetermineCascadedSymbolsAsync(SymbolAndProjectId<TSymbol> symbolAndProject, Solution solution, IImmutableSet<Project> projects, CancellationToken cancellationToken)
	{
		return SpecializedTasks.EmptyImmutableArray<SymbolAndProjectId>();
	}

	protected virtual Task<ImmutableArray<ReferenceLocation>> FindReferencesInDocumentUsingSymbolNameAsync(ISymbol symbol, Document document, CancellationToken cancellationToken)
	{
		return FindReferencesInDocumentUsingIdentifierAsync(symbol, symbol.Name, document, cancellationToken);
	}

	protected static ImmutableArray<T> Concat<T>(Task<ImmutableArray<T>>[] tasks)
	{
		ArrayBuilder<T> instance = ArrayBuilder<T>.GetInstance();
		try
		{
			foreach (Task<ImmutableArray<T>> task in tasks)
			{
				instance.AddRange(task.Result);
			}
			return instance.ToImmutable();
		}
		finally
		{
			instance.Free();
		}
	}
}
