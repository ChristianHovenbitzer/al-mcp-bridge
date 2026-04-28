using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces;

internal static class SourceTextExtensions
{
	public static string GetLeadingWhitespaceOfLineAtPosition(this SourceText text, int position)
	{
		Contract.ThrowIfNull(text);
		TextLine lineFromPosition = text.Lines.GetLineFromPosition(position);
		int? firstNonWhitespacePosition = lineFromPosition.GetFirstNonWhitespacePosition();
		if (!firstNonWhitespacePosition.HasValue)
		{
			return lineFromPosition.ToString();
		}
		return lineFromPosition.ToString().Substring(0, firstNonWhitespacePosition.Value - lineFromPosition.Start);
	}

	public static void GetLineAndOffset(this SourceText text, int position, out int lineNumber, out int offset)
	{
		TextLine lineFromPosition = text.Lines.GetLineFromPosition(position);
		lineNumber = lineFromPosition.LineNumber;
		offset = position - lineFromPosition.Start;
	}

	public static void GetLinesAndOffsets(this SourceText text, TextSpan textSpan, out int startLineNumber, out int startOffset, out int endLineNumber, out int endOffset)
	{
		text.GetLineAndOffset(textSpan.Start, out startLineNumber, out startOffset);
		text.GetLineAndOffset(textSpan.End, out endLineNumber, out endOffset);
	}

	public static bool OverlapsHiddenPosition(this SourceText text, TextSpan span, Func<int, CancellationToken, bool> isPositionHidden, CancellationToken cancellationToken)
	{
		bool result = text.TryOverlapsHiddenPosition(span, isPositionHidden, cancellationToken);
		cancellationToken.ThrowIfCancellationRequested();
		return result;
	}

	public static bool TryOverlapsHiddenPosition(this SourceText text, TextSpan span, Func<int, CancellationToken, bool> isPositionHidden, CancellationToken cancellationToken)
	{
		int num = text.Lines.IndexOf(span.Start);
		int num2 = text.Lines.IndexOf(span.End);
		for (int i = num; i <= num2; i++)
		{
			if (cancellationToken.IsCancellationRequested)
			{
				break;
			}
			int start = text.Lines[i].Start;
			if (isPositionHidden(start, cancellationToken))
			{
				return true;
			}
		}
		return false;
	}

	public static TextChangeRange GetEncompassingTextChangeRange(this SourceText newText, SourceText oldText)
	{
		IReadOnlyList<TextChangeRange> changeRanges = newText.GetChangeRanges(oldText);
		if (changeRanges.Count == 0)
		{
			return default(TextChangeRange);
		}
		if (changeRanges.Count == 1)
		{
			return changeRanges[0];
		}
		return TextChangeRange.Collapse(changeRanges);
	}

	public static int IndexOf(this SourceText text, string value, int startIndex, bool caseSensitive)
	{
		int num = text.Length - value.Length;
		string text2 = (caseSensitive ? value : CaseInsensitiveComparison.ToLower(value));
		for (int i = startIndex; i <= num; i++)
		{
			bool flag = true;
			for (int j = 0; j < text2.Length; j++)
			{
				if (!Match(text2[j], text[i + j], caseSensitive))
				{
					flag = false;
					break;
				}
			}
			if (flag)
			{
				return i;
			}
		}
		return -1;
	}

	public static int LastIndexOf(this SourceText text, string value, int startIndex, bool caseSensitive)
	{
		string text2 = (caseSensitive ? value : CaseInsensitiveComparison.ToLower(value));
		startIndex = ((startIndex + text2.Length > text.Length) ? (text.Length - text2.Length) : startIndex);
		for (int num = startIndex; num >= 0; num--)
		{
			bool flag = true;
			for (int i = 0; i < text2.Length; i++)
			{
				if (!Match(text2[i], text[num + i], caseSensitive))
				{
					flag = false;
					break;
				}
			}
			if (flag)
			{
				return num;
			}
		}
		return -1;
	}

	private static bool Match(char normalizedLeft, char right, bool caseSensitive)
	{
		if (!caseSensitive)
		{
			return normalizedLeft == CaseInsensitiveComparison.ToLower(right);
		}
		return normalizedLeft == right;
	}

	public static bool AreOnSameLine(this SourceText text, SyntaxToken token1, SyntaxToken token2)
	{
		if (token1.Kind != 0 && token2.Kind != 0)
		{
			return text.Lines.IndexOf(token1.Span.End) == text.Lines.IndexOf(token2.SpanStart);
		}
		return false;
	}
}
