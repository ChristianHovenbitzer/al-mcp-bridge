using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.LanguageServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces;

internal static class SyntaxTreeExtensions
{
	public static Task<SyntaxToken> GetTouchingWordAsync(this SyntaxTree syntaxTree, int position, ISyntaxFactsService syntaxFacts, CancellationToken cancellationToken, bool findInsideTrivia = false)
	{
		return syntaxTree.GetTouchingTokenAsync(position, syntaxFacts.IsWord, cancellationToken, findInsideTrivia);
	}

	public static Task<SyntaxToken> GetTouchingTokenAsync(this SyntaxTree syntaxTree, int position, CancellationToken cancellationToken, bool findInsideTrivia = false)
	{
		return syntaxTree.GetTouchingTokenAsync(position, (SyntaxToken _) => true, cancellationToken, findInsideTrivia);
	}

	public static async Task<SyntaxToken> GetTouchingTokenAsync(this SyntaxTree syntaxTree, int position, Predicate<SyntaxToken> predicate, CancellationToken cancellationToken, bool findInsideTrivia = false)
	{
		Contract.ThrowIfNull(syntaxTree);
		if (position >= syntaxTree.Length)
		{
			return default(SyntaxToken);
		}
		SyntaxToken syntaxToken = (await syntaxTree.GetRootAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false)).FindToken(position, findInsideTrivia);
		if ((syntaxToken.Span.Contains(position) || syntaxToken.Span.End == position) && predicate(syntaxToken))
		{
			return syntaxToken;
		}
		syntaxToken = syntaxToken.GetPreviousToken();
		if (syntaxToken.Span.End == position && predicate(syntaxToken))
		{
			return syntaxToken;
		}
		return default(SyntaxToken);
	}

	public static bool OverlapsHiddenPosition(this SyntaxTree tree, TextSpan span, CancellationToken cancellationToken)
	{
		SyntaxTree tree2 = tree;
		if (tree2 == null)
		{
			return false;
		}
		return tree2.GetText(cancellationToken).OverlapsHiddenPosition(span, delegate(int position, CancellationToken cancellationToken2)
		{
			LineVisibility lineVisibility = tree2.GetLineVisibility(position, cancellationToken2);
			return lineVisibility == LineVisibility.Hidden || lineVisibility == LineVisibility.BeforeFirstLineDirective;
		}, cancellationToken);
	}

	public static bool IsEntirelyHidden(this SyntaxTree tree, TextSpan span, CancellationToken cancellationToken)
	{
		return true;
	}

	public static async Task<bool> IsBeforeFirstTokenAsync(this SyntaxTree syntaxTree, int position, CancellationToken cancellationToken)
	{
		return position <= (await syntaxTree.GetRootAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false)).GetFirstToken(includeZeroWidth: true, includeSkipped: true).SpanStart;
	}

	public static SyntaxToken FindTokenOrEndToken(this SyntaxTree syntaxTree, int position, CancellationToken cancellationToken)
	{
		Contract.ThrowIfNull(syntaxTree);
		SyntaxNode root = syntaxTree.GetRoot(cancellationToken);
		CompilationUnitSyntax compilationUnitSyntax = root as CompilationUnitSyntax;
		SyntaxToken result = root.FindToken(position, findInsideTrivia: true);
		if (result.Kind != 0)
		{
			return result;
		}
		SyntaxTriviaList.Reversed.Enumerator enumerator = compilationUnitSyntax.GetEndOfFileToken().LeadingTrivia.Reverse().GetEnumerator();
		while (enumerator.MoveNext())
		{
			SyntaxTrivia current = enumerator.Current;
			if (current.HasStructure)
			{
				SyntaxToken lastToken = current.GetStructure().GetLastToken(includeZeroWidth: true);
				if (lastToken.Span.End == position)
				{
					return lastToken;
				}
			}
		}
		if (position == root.FullSpan.End)
		{
			return compilationUnitSyntax.GetEndOfFileToken();
		}
		return default(SyntaxToken);
	}

	internal static SyntaxTrivia FindTriviaAndAdjustForEndOfFile(this SyntaxTree syntaxTree, int position, CancellationToken cancellationToken, bool findInsideTrivia = false)
	{
		SyntaxNode root = syntaxTree.GetRoot(cancellationToken);
		CompilationUnitSyntax compilationUnitSyntax = root as CompilationUnitSyntax;
		SyntaxTrivia result = root.FindTrivia(position, findInsideTrivia);
		if (position == root.FullWidth())
		{
			SyntaxToken endOfFileToken = compilationUnitSyntax.GetEndOfFileToken();
			if (endOfFileToken.HasLeadingTrivia)
			{
				result = endOfFileToken.LeadingTrivia.Last();
			}
			else
			{
				SyntaxToken previousToken = endOfFileToken.GetPreviousToken(includeZeroWidth: false, includeSkipped: true);
				if (previousToken.HasTrailingTrivia)
				{
					result = previousToken.TrailingTrivia.Last();
				}
			}
		}
		return result;
	}

	public static SyntaxToken FindTokenOnRightOfPosition(this SyntaxTree syntaxTree, int position, CancellationToken cancellationToken, bool includeSkipped = true, bool includeDirectives = false, bool includeDocumentationComments = false)
	{
		return syntaxTree.GetRoot(cancellationToken).FindTokenOnRightOfPosition(position, includeSkipped, includeDirectives, includeDocumentationComments);
	}

	public static SyntaxToken FindTokenOnLeftOfPosition(this SyntaxTree syntaxTree, int position, CancellationToken cancellationToken, bool includeSkipped = true, bool includeDirectives = false, bool includeDocumentationComments = false)
	{
		return syntaxTree.GetRoot(cancellationToken).FindTokenOnLeftOfPosition(position, includeSkipped, includeDirectives, includeDocumentationComments);
	}
}
