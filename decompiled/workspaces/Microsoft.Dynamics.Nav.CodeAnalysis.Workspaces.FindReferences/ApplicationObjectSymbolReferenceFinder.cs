using System;
using System.Collections.Immutable;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.FindSymbols;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.LanguageServices;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.FindReferences;

internal class ApplicationObjectSymbolReferenceFinder : AbstractReferenceFinder<IObjectTypeSymbol>
{
	protected override bool CanFind(IObjectTypeSymbol symbol)
	{
		return true;
	}

	protected static bool TokensMatch(ISyntaxFactsService syntaxFacts, int? id, string name, SyntaxToken token)
	{
		if (!syntaxFacts.IsIdentifier(token) || !SemanticFacts.IsSameName(token.ValueText.UnquoteIdentifier(), name))
		{
			if (token.IsKind(SyntaxKind.Int32LiteralToken))
			{
				return (int)token.Value == id;
			}
			return false;
		}
		return true;
	}

	protected override Func<SyntaxToken, bool> GetTokensMatch(ISyntaxFactsService syntaxFacts, ISymbol symbol)
	{
		ISyntaxFactsService syntaxFacts2 = syntaxFacts;
		ISymbol symbol2 = symbol;
		IApplicationObjectTypeSymbol appObj = symbol2 as IApplicationObjectTypeSymbol;
		return (SyntaxToken t) => TokensMatch(syntaxFacts2, appObj?.Id, symbol2.Name, t);
	}

	protected override Task<ImmutableArray<Document>> DetermineDocumentsToSearchAsync(IObjectTypeSymbol symbol, Project project, IImmutableSet<Document> documents, CancellationToken cancellationToken)
	{
		return FindDocumentsAsync(project, documents, cancellationToken, symbol.Name);
	}

	protected override Task<ImmutableArray<ReferenceLocation>> FindReferencesInDocumentAsync(IObjectTypeSymbol symbol, Document document, CancellationToken cancellationToken)
	{
		Task<ImmutableArray<ReferenceLocation>> findReferencesByName = FindReferencesInDocumentUsingSymbolNameAsync(symbol, document, cancellationToken);
		Task<ImmutableArray<ReferenceLocation>> findReferencesById;
		if (symbol is IApplicationObjectTypeSymbol applicationObjectTypeSymbol)
		{
			findReferencesById = FindReferencesInDocumentUsingIdentifierAsync(symbol, applicationObjectTypeSymbol.Id.ToString(CultureInfo.InvariantCulture), document, cancellationToken);
		}
		else
		{
			findReferencesById = Task.FromResult(ImmutableArray<ReferenceLocation>.Empty);
		}
		return Task<ImmutableArray<ReferenceLocation>>.Factory.ContinueWhenAll(new Task[2] { findReferencesByName, findReferencesById }, (Task[] tasks) => findReferencesByName.Result.Concat(findReferencesById.Result));
	}
}
