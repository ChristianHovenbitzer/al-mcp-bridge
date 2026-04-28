using System.Collections.Generic;
using System.Linq;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces;

internal static class SyntaxTriviaExtensions
{
	public static int Width(this SyntaxTrivia trivia)
	{
		return trivia.Span.Length;
	}

	public static int FullWidth(this SyntaxTrivia trivia)
	{
		return trivia.FullSpan.Length;
	}

	public static bool IsElastic(this SyntaxTrivia trivia)
	{
		return trivia.HasAnnotation(SyntaxAnnotation.ElasticAnnotation);
	}

	public static bool IsRegularOrDocComment(this SyntaxTrivia trivia)
	{
		if (!trivia.IsRegularComment())
		{
			return trivia.IsDocComment();
		}
		return true;
	}

	public static bool IsPositionInCommentTrivia(this IEnumerable<SyntaxTrivia> trivias, int position)
	{
		return trivias.Any((SyntaxTrivia t) => t.Span.End >= position && position >= t.Span.Start && t.IsRegularComment());
	}
}
