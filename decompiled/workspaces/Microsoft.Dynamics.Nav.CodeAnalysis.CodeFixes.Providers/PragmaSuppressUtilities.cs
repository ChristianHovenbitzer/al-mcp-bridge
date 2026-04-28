using System.Linq;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.CodeFixes.Providers;

internal static class PragmaSuppressUtilities
{
	private static readonly SyntaxTrivia WhitespaceTrivia = SyntaxFactory.WhiteSpace(" ");

	internal static SyntaxNode GetNodeWithPragmaSuppression(SyntaxNode syntaxNode, string diagnosticId)
	{
		var (pragmaSuppress, pragmaRestore) = GetSuppressionTrivias(diagnosticId);
		return GetNodeWithPragmaSuppression(syntaxNode, pragmaSuppress, pragmaRestore);
	}

	internal static SyntaxNode GetNodeWithPragmaSuppressWrappedLine(SyntaxNode syntaxNode, int startLine, int endLine, string diagnosticId)
	{
		var (pragmaSuppress, pragmaRestore) = GetSuppressionTrivias(diagnosticId);
		return WrapLinesWithPragmaSuppress(syntaxNode, startLine, endLine, pragmaSuppress, pragmaRestore);
	}

	internal static (SyntaxToken first, SyntaxNodeOrToken last) GetFirstAndLastTokenOrNode(SyntaxNode syntaxNode)
	{
		SyntaxToken firstToken = syntaxNode.GetFirstToken();
		int line = syntaxNode.SyntaxTree.GetLineSpan(firstToken.Span).StartLinePosition.Line;
		SyntaxNodeOrToken lastNodeOrToken = GetLastNodeOrToken(syntaxNode, line);
		return (first: firstToken, last: lastNodeOrToken);
	}

	internal static SyntaxNode GetAncesstorWrappingLine(SyntaxNode syntaxNode, int line)
	{
		SyntaxNode syntaxNode2 = syntaxNode;
		while (syntaxNode2.Parent != null && syntaxNode2.Parent.Kind != SyntaxKind.CompilationUnit && syntaxNode2.Parent.GetLocation().GetLineSpan().StartLinePosition.Line == line)
		{
			syntaxNode2 = syntaxNode2.Parent;
		}
		return syntaxNode2;
	}

	private static SyntaxToken GetFirstTokenOnLine(SyntaxNode syntaxNode, int line)
	{
		return syntaxNode.DescendantTokens().FirstOrDefault((SyntaxToken t) => t.GetLocation().GetLineSpan().StartLinePosition.Line == line);
	}

	private static SyntaxToken GetLastTokenOnLine(SyntaxNode syntaxNode, int line)
	{
		return syntaxNode.DescendantTokens().LastOrDefault((SyntaxToken t) => t.GetLocation().GetLineSpan().StartLinePosition.Line == line);
	}

	private static (SyntaxToken, SyntaxToken) GetFirstAndLastLineTokens(SyntaxNode syntaxNode, int startLine, int endLine)
	{
		SyntaxToken firstTokenOnLine = GetFirstTokenOnLine(syntaxNode, startLine);
		SyntaxToken lastTokenOnLine = GetLastTokenOnLine(syntaxNode, endLine);
		return (firstTokenOnLine, lastTokenOnLine);
	}

	internal static (PragmaWarningDirectiveTriviaSyntax suppress, PragmaWarningDirectiveTriviaSyntax restore) GetSuppressionTrivias(string diagnosticId)
	{
		SeparatedSyntaxList<CodeExpressionSyntax> diagnosticList = default(SeparatedSyntaxList<CodeExpressionSyntax>).Add(SyntaxFactory.IdentifierName(SyntaxFactory.Identifier(diagnosticId)));
		PragmaWarningDirectiveTriviaSyntax item = CreatePragmaSuppressSyntax(SyntaxKind.DisableKeyword, diagnosticList);
		PragmaWarningDirectiveTriviaSyntax item2 = CreatePragmaSuppressSyntax(SyntaxKind.RestoreKeyword, diagnosticList);
		return (suppress: item, restore: item2);
	}

	internal static SyntaxTriviaList GetLeadingTriviaWithPragmaSuppress(SyntaxNodeOrToken syntaxNode, PragmaWarningDirectiveTriviaSyntax pragmaSuppress)
	{
		return syntaxNode.GetLeadingTrivia().AddRange(new SyntaxTrivia[2]
		{
			SyntaxFactory.Trivia(pragmaSuppress),
			SyntaxFactory.CarriageReturnLinefeed
		});
	}

	internal static SyntaxTriviaList GetTrailingTriviaWithPragmaRestore(SyntaxNodeOrToken syntaxNode, PragmaWarningDirectiveTriviaSyntax pragmaRestore)
	{
		return syntaxNode.GetTrailingTrivia().InsertRange(0, new SyntaxTrivia[2]
		{
			SyntaxFactory.CarriageReturnLinefeed,
			SyntaxFactory.Trivia(pragmaRestore)
		});
	}

	private static SyntaxNode GetNodeWithPragmaSuppression(SyntaxNode syntaxNode, PragmaWarningDirectiveTriviaSyntax pragmaSuppress, PragmaWarningDirectiveTriviaSyntax pragmaRestore)
	{
		var (firstToken, syntaxNodeOrToken) = GetFirstAndLastTokenOrNode(syntaxNode);
		if (syntaxNodeOrToken != null && firstToken != syntaxNodeOrToken)
		{
			SyntaxTriviaList leadingTrivias = GetLeadingTriviaWithPragmaSuppress(firstToken, pragmaSuppress);
			SyntaxTriviaList trailingTrivias = GetTrailingTriviaWithPragmaRestore(syntaxNodeOrToken, pragmaRestore);
			if (syntaxNodeOrToken.IsToken)
			{
				return syntaxNode.ReplaceCore<SyntaxNode>(null, null, new SyntaxToken[2]
				{
					firstToken,
					(SyntaxToken)syntaxNodeOrToken
				}, (SyntaxToken o, SyntaxToken r) => (!(o == firstToken)) ? o.WithTrailingTrivia(trailingTrivias) : o.WithLeadingTrivia(leadingTrivias));
			}
			return syntaxNode.ReplaceCore(tokens: new SyntaxToken[1] { firstToken }, nodes: new SyntaxNode[1] { (SyntaxNode?)syntaxNodeOrToken }, computeReplacementNode: (SyntaxNode o, SyntaxNode r) => o.WithTrailingTrivia(trailingTrivias), computeReplacementToken: (SyntaxToken o, SyntaxToken r) => o.WithLeadingTrivia(leadingTrivias));
		}
		SyntaxTriviaList leadingTriviaWithPragmaSuppress = GetLeadingTriviaWithPragmaSuppress(syntaxNode, pragmaSuppress);
		SyntaxTriviaList trailingTriviaWithPragmaRestore = GetTrailingTriviaWithPragmaRestore(syntaxNode, pragmaRestore);
		return syntaxNode.WithLeadingTrivia(leadingTriviaWithPragmaSuppress).WithTrailingTrivia(trailingTriviaWithPragmaRestore);
	}

	private static SyntaxNode WrapLinesWithPragmaSuppress(SyntaxNode syntaxNode, int startLine, int endLine, PragmaWarningDirectiveTriviaSyntax pragmaSuppress, PragmaWarningDirectiveTriviaSyntax pragmaRestore)
	{
		(SyntaxToken, SyntaxToken) firstAndLastLineTokens = GetFirstAndLastLineTokens(syntaxNode, startLine, endLine);
		SyntaxToken firstToken = firstAndLastLineTokens.Item1;
		SyntaxToken item = firstAndLastLineTokens.Item2;
		SyntaxTriviaList leadingTrivias = GetLeadingTriviaWithPragmaSuppress(firstToken, pragmaSuppress);
		SyntaxTriviaList trailingTrivias = GetTrailingTriviaWithPragmaRestore(item, pragmaRestore);
		return syntaxNode.ReplaceCore<SyntaxNode>(null, null, new SyntaxToken[2] { firstToken, item }, (SyntaxToken o, SyntaxToken r) => (!(o == firstToken)) ? o.WithTrailingTrivia(trailingTrivias) : o.WithLeadingTrivia(leadingTrivias));
	}

	private static SyntaxNodeOrToken GetLastNodeOrToken(SyntaxNode syntaxNode, int firstTokenStartLinePosition)
	{
		SyntaxNodeOrToken syntaxNodeOrToken = null;
		SyntaxTree syntaxTree = syntaxNode.SyntaxTree;
		ChildSyntaxList.Enumerator enumerator = syntaxNode.ChildNodesAndTokens().GetEnumerator();
		while (enumerator.MoveNext())
		{
			SyntaxNodeOrToken current = enumerator.Current;
			if (syntaxTree.GetLineSpan(current.Span).StartLinePosition.Line != firstTokenStartLinePosition)
			{
				break;
			}
			if (syntaxNodeOrToken == null || current.SpanStart >= syntaxNodeOrToken.SpanStart)
			{
				syntaxNodeOrToken = current;
			}
		}
		return syntaxNodeOrToken;
	}

	private static PragmaWarningDirectiveTriviaSyntax CreatePragmaSuppressSyntax(SyntaxKind disableOrRestoreSyntaxKind, SeparatedSyntaxList<CodeExpressionSyntax> diagnosticList)
	{
		return SyntaxFactory.PragmaWarningDirectiveTrivia(SyntaxFactory.Token(SyntaxKind.HashToken), SyntaxFactory.Token(SyntaxKind.PragmaKeyword).WithTrailingTrivia(WhitespaceTrivia), SyntaxFactory.Token(SyntaxKind.WarningKeyword).WithTrailingTrivia(WhitespaceTrivia), SyntaxFactory.Token(disableOrRestoreSyntaxKind).WithTrailingTrivia(WhitespaceTrivia), diagnosticList, SyntaxFactory.Token(SyntaxKind.EndOfDirectiveToken), isActive: true);
	}
}
