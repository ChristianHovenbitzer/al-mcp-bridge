using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Formatting.Rules;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Formatting;

internal static class FormattingExtensions
{
	private static readonly char[] s_trimChars = new char[2] { '\r', '\n' };

	public static SyntaxNode GetParentWithBiggerSpan(this SyntaxNode node)
	{
		if (node.Parent == null)
		{
			return node;
		}
		if (node.Parent.Span != node.Span)
		{
			return node.Parent;
		}
		return node.Parent.GetParentWithBiggerSpan();
	}

	public static IEnumerable<IFormattingRule> Concat(this IFormattingRule rule, IEnumerable<IFormattingRule> rules)
	{
		return SpecializedCollections.SingletonEnumerable(rule).Concat(rules);
	}

	public static void AddRange<T>(this IList<T> list, IEnumerable<T> values)
	{
		foreach (T value in values)
		{
			list.Add(value);
		}
	}

	public static List<T> Combine<T>(this List<T> list1, List<T> list2)
	{
		if (list1 == null)
		{
			return list2;
		}
		if (list2 == null)
		{
			return list1;
		}
		List<T> list3 = new List<T>(list1);
		list3.AddRange(list2);
		return list3;
	}

	public static bool ContainsElasticTrivia(this SuppressOperation operation, TokenStream tokenStream)
	{
		TokenData tokenData = tokenStream.GetTokenData(operation.StartToken);
		TokenData nextTokenData = tokenData.GetNextTokenData();
		TokenData tokenData2 = tokenStream.GetTokenData(operation.EndToken);
		TokenData previousTokenData = tokenData2.GetPreviousTokenData();
		if (!tokenStream.GetTriviaData(tokenData, nextTokenData).TreatAsElastic)
		{
			return tokenStream.GetTriviaData(previousTokenData, tokenData2).TreatAsElastic;
		}
		return true;
	}

	public static bool HasAnyWhitespaceElasticTrivia(this SyntaxTriviaList list)
	{
		SyntaxTriviaList.Enumerator enumerator = list.GetEnumerator();
		while (enumerator.MoveNext())
		{
			if (enumerator.Current.IsElastic())
			{
				return true;
			}
		}
		return false;
	}

	public static bool IsOn(this IndentBlockOption option, IndentBlockOption flag)
	{
		return (option & flag) == flag;
	}

	public static bool IsMaskOn(this IndentBlockOption option, IndentBlockOption mask)
	{
		return (option & mask) != 0;
	}

	public static bool IsOn(this SuppressOption option, SuppressOption flag)
	{
		return (option & flag) == flag;
	}

	public static bool IsMaskOn(this SuppressOption option, SuppressOption mask)
	{
		return (option & mask) != 0;
	}

	public static SuppressOption RemoveFlag(this SuppressOption option, SuppressOption flag)
	{
		return option & ~flag;
	}

	public static string CreateIndentationString(this int desiredIndentation, bool useTab, int tabSize)
	{
		int num = 0;
		int num2 = Math.Max(0, desiredIndentation);
		if (useTab)
		{
			num = desiredIndentation / tabSize;
			num2 -= num * tabSize;
		}
		return new string('\t', num) + new string(' ', num2);
	}

	public static StringBuilder AppendIndentationString(this StringBuilder sb, int desiredIndentation, bool useTab, int tabSize)
	{
		int num = 0;
		int num2 = Math.Max(0, desiredIndentation);
		if (useTab)
		{
			num = desiredIndentation / tabSize;
			num2 -= num * tabSize;
		}
		return sb.Append('\t', num).Append(' ', num2);
	}

	public static void ProcessTextBetweenTokens(this string text, TreeData treeInfo, SyntaxToken baseToken, int tabSize, out int lineBreaks, out int spaceOrIndentation)
	{
		lineBreaks = text.GetNumberOfLineBreaks();
		if (lineBreaks > 0)
		{
			string lastLineText = text.GetLastLineText();
			spaceOrIndentation = lastLineText.GetColumnFromLineOffset(lastLineText.Length, tabSize);
		}
		else
		{
			int initialColumn = ((baseToken.Kind != 0) ? treeInfo.GetOriginalColumn(tabSize, baseToken) : 0);
			spaceOrIndentation = text.ConvertTabToSpace(tabSize, baseToken.ToString().GetTextColumn(tabSize, initialColumn), text.Length);
		}
	}

	public static string AdjustIndentForXmlDocExteriorTrivia(this string triviaText, bool forceIndentation, int indentation, int indentationDelta, bool useTab, int tabSize)
	{
		bool flag = false;
		StringBuilder stringBuilder = StringBuilderPool.Allocate();
		triviaText.TrimEnd(s_trimChars);
		int num = triviaText.GetFirstNonWhitespaceIndexInString();
		if (num == -1)
		{
			flag = true;
			num = triviaText.Length;
		}
		int newIndentationForComments = triviaText.GetNewIndentationForComments(num, forceIndentation, indentation, indentationDelta, tabSize);
		stringBuilder.AppendIndentationString(newIndentationForComments, useTab, tabSize);
		if (!flag)
		{
			stringBuilder.Append(triviaText, num, triviaText.Length - num);
		}
		return StringBuilderPool.ReturnAndFree(stringBuilder);
	}

	public static string ReindentStartOfXmlDocumentationComment(this string triviaText, bool forceIndentation, int indentation, int indentationDelta, bool useTab, int tabSize, string newLine)
	{
		StringBuilder stringBuilder = StringBuilderPool.Allocate();
		string[] array = triviaText.Split('\n');
		Contract.ThrowIfFalse(array.Length != 0);
		stringBuilder.Append(array[0].Trim(s_trimChars));
		if (0 < array.Length - 1)
		{
			stringBuilder.Append(newLine);
		}
		for (int i = 1; i < array.Length; i++)
		{
			string text = array[i].TrimEnd(s_trimChars);
			int firstNonWhitespaceIndexInString = text.GetFirstNonWhitespaceIndexInString();
			if (firstNonWhitespaceIndexInString >= 0)
			{
				int newIndentationForComments = text.GetNewIndentationForComments(firstNonWhitespaceIndexInString, forceIndentation, indentation, indentationDelta, tabSize);
				stringBuilder.AppendIndentationString(newIndentationForComments, useTab, tabSize);
				stringBuilder.Append(text, firstNonWhitespaceIndexInString, text.Length - firstNonWhitespaceIndexInString);
			}
			if (i < array.Length - 1)
			{
				stringBuilder.Append(newLine);
			}
		}
		return StringBuilderPool.ReturnAndFree(stringBuilder);
	}

	public static int GetFirstNonWhitespaceIndexInString(this string text)
	{
		for (int i = 0; i < text.Length; i++)
		{
			if (text[i] != ' ' && text[i] != '\t')
			{
				return i;
			}
		}
		return -1;
	}

	public static TextChange SimpleDiff(this TextChange textChange, string text)
	{
		TextSpan span = textChange.Span;
		string text2 = textChange.NewText;
		int i;
		for (i = 0; i < span.Length && i < text2.Length && text[i] == text2[i]; i++)
		{
		}
		if (i == span.Length && text.Length == text2.Length)
		{
			return textChange;
		}
		if (i > 0)
		{
			span = new TextSpan(span.Start + i, span.Length - i);
			text2 = text2.Substring(i);
		}
		return new TextChange(span, text2);
	}

	private static int GetNewIndentationForComments(this string line, int nonWhitespaceCharIndex, bool forceIndentation, int indentation, int indentationDelta, int tabSize)
	{
		if (forceIndentation)
		{
			return indentation;
		}
		return Math.Max(line.GetColumnFromLineOffset(nonWhitespaceCharIndex, tabSize) + indentationDelta, 0);
	}
}
