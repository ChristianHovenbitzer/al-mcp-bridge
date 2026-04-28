using System.Linq;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces;

internal static class AttributeArgumentListSyntaxExtensions
{
	private const char separator = ',';

	public static int CalculateAttributeArgumentPosition(this AttributeArgumentListSyntax syntax, int cursorPosition)
	{
		int num = 0;
		SeparatedSyntaxList<AttributeArgumentSyntax>.Enumerator enumerator = syntax.Arguments.GetEnumerator();
		while (enumerator.MoveNext())
		{
			AttributeArgumentSyntax current = enumerator.Current;
			if (current.Span.IntersectsWith(cursorPosition) || current.EndPosition >= cursorPosition)
			{
				break;
			}
			num++;
		}
		return num + ConsiderSkippedEmptyArguments(syntax, num, cursorPosition);
	}

	private static int ConsiderSkippedEmptyArguments(AttributeArgumentListSyntax syntax, int argumentPosition, int cursorPosition)
	{
		return CountCommaOnParenthesisTrivias(syntax, cursorPosition) + CountCommaOnListSeparators(syntax, argumentPosition, cursorPosition);
	}

	private static int CountCommaOnParenthesisTrivias(AttributeArgumentListSyntax syntax, int cursorPosition)
	{
		int num = 0;
		if (syntax.OpenParenthesisToken.HasTrailingTrivia)
		{
			SyntaxTriviaList.Enumerator enumerator = syntax.OpenParenthesisToken.TrailingTrivia.GetEnumerator();
			while (enumerator.MoveNext())
			{
				SyntaxTrivia current = enumerator.Current;
				if (current.Span.End < cursorPosition)
				{
					num += current.ToString().Count((char x) => x == ',');
				}
			}
		}
		return num;
	}

	private static int CountCommaOnListSeparators(AttributeArgumentListSyntax syntax, int argumentPosition, int cursorPosition)
	{
		int num = 0;
		SyntaxToken[] array = syntax.Arguments.GetSeparators().ToArray();
		for (int i = 0; i < argumentPosition; i++)
		{
			if (!array[i].HasTrailingTrivia)
			{
				continue;
			}
			foreach (SyntaxTrivia item in array[i].GetAllTrailingTriviaToNextToken())
			{
				if (item.Span.End < cursorPosition)
				{
					num += item.ToString().Count((char x) => x == ',');
				}
			}
		}
		return num;
	}
}
