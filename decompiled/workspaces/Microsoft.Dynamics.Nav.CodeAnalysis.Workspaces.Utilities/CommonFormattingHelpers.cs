using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Formatting;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Formatting.Rules;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Utilities;

internal static class CommonFormattingHelpers
{
	public static readonly Comparison<SuppressOperation> SuppressOperationComparer = (SuppressOperation o1, SuppressOperation o2) => o1.TextSpan.Start - o2.TextSpan.Start;

	public static readonly Comparison<IndentBlockOperation> IndentBlockOperationComparer = delegate(IndentBlockOperation o1, IndentBlockOperation o2)
	{
		int num = o1.TextSpan.Start - o2.TextSpan.Start;
		if (num != 0)
		{
			return num;
		}
		int num2 = o2.TextSpan.End - o1.TextSpan.End;
		return (num2 != 0) ? num2 : 0;
	};

	public static IEnumerable<(SyntaxToken, SyntaxToken)> ConvertToTokenPairs(this SyntaxNode root, IList<TextSpan> spans)
	{
		Contract.ThrowIfNull(root);
		Contract.ThrowIfFalse(spans.Count > 0);
		if (spans.Count == 1)
		{
			yield return root.ConvertToTokenPair(spans[0]);
			yield break;
		}
		new List<(SyntaxToken, SyntaxToken)>();
		(SyntaxToken, SyntaxToken) tuple = root.ConvertToTokenPair(spans[0]);
		for (int i = 1; i < spans.Count; i++)
		{
			(SyntaxToken, SyntaxToken) currentOne = root.ConvertToTokenPair(spans[i]);
			if (currentOne.Item1.SpanStart <= tuple.Item2.Span.End)
			{
				tuple = ValueTuple.Create(tuple.Item1, (tuple.Item2.Span.End < currentOne.Item2.Span.End) ? currentOne.Item2 : tuple.Item2);
				continue;
			}
			yield return tuple;
			tuple = currentOne;
		}
		yield return tuple;
	}

	public static (SyntaxToken, SyntaxToken) ConvertToTokenPair(this SyntaxNode root, TextSpan textSpan)
	{
		Contract.ThrowIfNull(root);
		Contract.ThrowIfTrue(textSpan.IsEmpty);
		SyntaxToken syntaxToken = root.FindToken(textSpan.Start);
		if (syntaxToken.IsMissing)
		{
			syntaxToken = syntaxToken.GetPreviousToken();
		}
		if (textSpan.Start < syntaxToken.SpanStart)
		{
			syntaxToken = syntaxToken.GetPreviousToken();
		}
		SyntaxToken syntaxToken2 = ((root.FullSpan.End <= textSpan.End) ? root.GetLastToken(includeZeroWidth: true) : root.FindToken(textSpan.End));
		if (syntaxToken2.IsMissing)
		{
			syntaxToken2 = syntaxToken2.GetNextToken();
		}
		if (syntaxToken2.Span.End < textSpan.End)
		{
			syntaxToken2 = syntaxToken2.GetNextToken();
		}
		syntaxToken = ((syntaxToken.Kind != 0) ? syntaxToken : root.GetFirstToken(includeZeroWidth: true));
		syntaxToken2 = ((syntaxToken2.Kind != 0) ? syntaxToken2 : root.GetLastToken(includeZeroWidth: true));
		Contract.ThrowIfFalse(syntaxToken.Equals(syntaxToken2) || syntaxToken.Span.End <= syntaxToken2.SpanStart);
		return ValueTuple.Create(syntaxToken, syntaxToken2);
	}

	public static bool IsInvalidTokenRange(this SyntaxNode root, SyntaxToken startToken, SyntaxToken endToken)
	{
		if (startToken.Kind == SyntaxKind.None || endToken.Kind == SyntaxKind.None)
		{
			return true;
		}
		if (startToken.Equals(endToken))
		{
			return false;
		}
		if (root.FullSpan.End != startToken.SpanStart)
		{
			return startToken.FullSpan.End > endToken.FullSpan.Start;
		}
		return true;
	}

	public static int GetTokenColumn(this SyntaxTree tree, SyntaxToken token, int tabSize)
	{
		Contract.ThrowIfNull(tree);
		Contract.ThrowIfTrue(token.Kind == SyntaxKind.None);
		int spanStart = token.SpanStart;
		TextLine lineFromPosition = tree.GetText().Lines.GetLineFromPosition(spanStart);
		return lineFromPosition.GetColumnFromLineOffset(spanStart - lineFromPosition.Start, tabSize);
	}

	public static string GetText(this SourceText text, SyntaxToken token1, SyntaxToken token2)
	{
		if (token1.Kind != 0)
		{
			return text.ToString(TextSpan.FromBounds(token1.Span.End, token2.SpanStart));
		}
		return text.ToString(TextSpan.FromBounds(0, token2.SpanStart));
	}

	public static string GetTextBetween(SyntaxToken token1, SyntaxToken token2)
	{
		StringBuilder stringBuilder = new StringBuilder();
		AppendTextBetween(token1, token2, stringBuilder);
		return stringBuilder.ToString();
	}

	public static void AppendTextBetween(SyntaxToken token1, SyntaxToken token2, StringBuilder builder)
	{
		Contract.ThrowIfTrue(token1.Kind == SyntaxKind.None && token2.Kind == SyntaxKind.None);
		Contract.ThrowIfTrue(token1.Equals(token2));
		if (token1.Kind == SyntaxKind.None)
		{
			AppendLeadingTriviaText(token2, builder);
			return;
		}
		if (token2.Kind == SyntaxKind.None)
		{
			AppendTrailingTriviaText(token1, builder);
			return;
		}
		_ = token1.FullSpan.Start;
		_ = token2.FullSpan.Start;
		if (token1.FullSpan.End == token2.FullSpan.Start)
		{
			AppendTextBetweenTwoAdjacentTokens(token1, token2, builder);
			return;
		}
		AppendTrailingTriviaText(token1, builder);
		SyntaxToken nextToken = token1.GetNextToken(includeZeroWidth: true);
		while (nextToken.FullSpan.End <= token2.FullSpan.Start)
		{
			builder.Append(nextToken.ToFullString());
			nextToken = nextToken.GetNextToken(includeZeroWidth: true);
		}
		AppendPartialLeadingTriviaText(token2, builder, token1.TrailingTrivia.FullSpan.End);
	}

	public static TextSpan GetSpanIncludingTrailingAndLeadingTriviaOfAdjacentTokens(SyntaxToken startToken, SyntaxToken endToken)
	{
		int startPositionOfSpan = GetStartPositionOfSpan(startToken);
		int endPositionOfSpan = GetEndPositionOfSpan(endToken);
		return TextSpan.FromBounds(startPositionOfSpan, endPositionOfSpan);
	}

	public static int GetStartPositionOfSpan(SyntaxToken token)
	{
		SyntaxToken previousToken = token.GetPreviousToken();
		if (previousToken.Kind != 0)
		{
			return previousToken.Span.End;
		}
		int start = token.FullSpan.Start;
		if (start <= 0)
		{
			return 0;
		}
		SyntaxNode parentThatContainsGivenSpan = GetParentThatContainsGivenSpan(token.Parent, start, forward: true);
		if (parentThatContainsGivenSpan == null)
		{
			return Contract.FailWithReturn<int>("This can't happen");
		}
		Contract.ThrowIfFalse(parentThatContainsGivenSpan.FullSpan.Start < start);
		previousToken = parentThatContainsGivenSpan.FindToken(start + 1);
		Contract.ThrowIfTrue(previousToken.Kind == SyntaxKind.None);
		return previousToken.Span.End;
	}

	public static bool HasAnyWhitespaceElasticTrivia(SyntaxToken previousToken, SyntaxToken currentToken)
	{
		if ((!previousToken.ContainsAnnotations && !currentToken.ContainsAnnotations) || (!previousToken.HasTrailingTrivia && !currentToken.HasLeadingTrivia))
		{
			return false;
		}
		if (!previousToken.TrailingTrivia.HasAnyWhitespaceElasticTrivia())
		{
			return currentToken.LeadingTrivia.HasAnyWhitespaceElasticTrivia();
		}
		return true;
	}

	public static bool IsNull<T>(T t) where T : class
	{
		return t == null;
	}

	public static bool IsNotNull<T>(T t) where T : class
	{
		return !IsNull(t);
	}

	public static TextSpan GetFormattingSpan(SyntaxNode root, TextSpan span)
	{
		Contract.ThrowIfNull(root);
		SyntaxToken previousToken = root.FindToken(span.Start).GetPreviousToken();
		SyntaxToken nextToken = root.FindTokenFromEnd(span.End).GetNextToken();
		int spanStart = previousToken.SpanStart;
		int end = ((nextToken.Kind == SyntaxKind.None) ? root.Span.End : nextToken.Span.End);
		return TextSpan.FromBounds(spanStart, end);
	}

	private static void AppendTextBetweenTwoAdjacentTokens(SyntaxToken token1, SyntaxToken token2, StringBuilder builder)
	{
		AppendTrailingTriviaText(token1, builder);
		AppendLeadingTriviaText(token2, builder);
	}

	private static void AppendLeadingTriviaText(SyntaxToken token, StringBuilder builder)
	{
		if (token.HasLeadingTrivia)
		{
			SyntaxTriviaList.Enumerator enumerator = token.LeadingTrivia.GetEnumerator();
			while (enumerator.MoveNext())
			{
				builder.Append(enumerator.Current.ToFullString());
			}
		}
	}

	private static void AppendPartialLeadingTriviaText(SyntaxToken token, StringBuilder builder, int token1FullSpanEnd)
	{
		if (!token.HasLeadingTrivia)
		{
			return;
		}
		SyntaxTriviaList.Enumerator enumerator = token.LeadingTrivia.GetEnumerator();
		while (enumerator.MoveNext())
		{
			SyntaxTrivia current = enumerator.Current;
			if (current.FullSpan.End > token1FullSpanEnd)
			{
				builder.Append(current.ToFullString());
			}
		}
	}

	private static void AppendTrailingTriviaText(SyntaxToken token, StringBuilder builder)
	{
		if (token.HasTrailingTrivia)
		{
			SyntaxTriviaList.Enumerator enumerator = token.TrailingTrivia.GetEnumerator();
			while (enumerator.MoveNext())
			{
				builder.Append(enumerator.Current.ToFullString());
			}
		}
	}

	private static int GetEndPositionOfSpan(SyntaxToken token)
	{
		SyntaxToken nextToken = token.GetNextToken();
		if (nextToken.Kind != 0)
		{
			return nextToken.SpanStart;
		}
		int end = token.FullSpan.End;
		SyntaxNode parentThatContainsGivenSpan = GetParentThatContainsGivenSpan(token.Parent, end, forward: false);
		if (parentThatContainsGivenSpan == null)
		{
			return token.FullSpan.End;
		}
		Contract.ThrowIfFalse(end < parentThatContainsGivenSpan.FullSpan.End);
		nextToken = parentThatContainsGivenSpan.FindToken(end + 1);
		Contract.ThrowIfTrue(nextToken.Kind == SyntaxKind.None);
		return nextToken.SpanStart;
	}

	private static SyntaxNode GetParentThatContainsGivenSpan(SyntaxNode node, int position, bool forward)
	{
		while (node != null)
		{
			TextSpan fullSpan = node.FullSpan;
			if (forward)
			{
				if (fullSpan.Start < position)
				{
					return node;
				}
			}
			else if (position > fullSpan.End)
			{
				return node;
			}
			node = node.Parent;
		}
		return null;
	}
}
