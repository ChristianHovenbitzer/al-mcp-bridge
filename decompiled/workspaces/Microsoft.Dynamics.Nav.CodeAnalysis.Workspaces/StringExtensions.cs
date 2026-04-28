using System;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces;

internal static class StringExtensions
{
	public static int GetTextColumn(this string text, int tabSize, int initialColumn)
	{
		string lastLineText = text.GetLastLineText();
		if (text != lastLineText)
		{
			return lastLineText.GetColumnFromLineOffset(lastLineText.Length, tabSize);
		}
		return text.ConvertTabToSpace(tabSize, initialColumn, text.Length) + initialColumn;
	}

	public static int ConvertTabToSpace(this string textSnippet, int tabSize, int initialColumn, int endPosition)
	{
		int num = initialColumn;
		for (int i = 0; i < endPosition; i++)
		{
			num = ((textSnippet[i] != '\t') ? (num + 1) : (num + (tabSize - num % tabSize)));
		}
		return num - initialColumn;
	}

	public static int IndexOf(this string text, Func<char, bool> predicate)
	{
		if (text == null)
		{
			return -1;
		}
		for (int i = 0; i < text.Length; i++)
		{
			if (predicate(text[i]))
			{
				return i;
			}
		}
		return -1;
	}

	public static string GetFirstLineText(this string text)
	{
		int num = text.IndexOf('\n');
		if (num < 0)
		{
			return text;
		}
		return text.Substring(0, num + 1);
	}

	public static string GetLastLineText(this string text)
	{
		int num = text.LastIndexOf('\n');
		if (num < 0)
		{
			return text;
		}
		return text.Substring(num + 1);
	}

	public static bool ContainsLineBreak(this string text)
	{
		foreach (char c in text)
		{
			if (c == '\n' || c == '\r')
			{
				return true;
			}
		}
		return false;
	}

	public static int GetNumberOfLineBreaks(this string text)
	{
		int num = 0;
		for (int i = 0; i < text.Length; i++)
		{
			if (text[i] == '\n')
			{
				num++;
			}
			else if (text[i] == '\r' && (i + 1 == text.Length || text[i + 1] != '\n'))
			{
				num++;
			}
		}
		return num;
	}

	public static bool ContainsTab(this string text)
	{
		for (int i = 0; i < text.Length; i++)
		{
			if (text[i] == '\t')
			{
				return true;
			}
		}
		return false;
	}

	public static int GetColumnOfFirstNonWhitespaceCharacterOrEndOfLine(this string line, int tabSize)
	{
		int? firstNonWhitespaceOffset = line.GetFirstNonWhitespaceOffset();
		if (firstNonWhitespaceOffset.HasValue)
		{
			return line.GetColumnFromLineOffset(firstNonWhitespaceOffset.Value, tabSize);
		}
		return line.GetColumnFromLineOffset(line.Length, tabSize);
	}

	public static int GetColumnFromLineOffset(this string line, int endPosition, int tabSize)
	{
		Contract.ThrowIfNull(line);
		Contract.ThrowIfFalse(0 <= endPosition && endPosition <= line.Length);
		Contract.ThrowIfFalse(tabSize > 0);
		return line.ConvertTabToSpace(tabSize, 0, endPosition);
	}

	public static int GetLineOffsetFromColumn(this string line, int column, int tabSize)
	{
		Contract.ThrowIfNull(line);
		Contract.ThrowIfFalse(column >= 0);
		Contract.ThrowIfFalse(tabSize > 0);
		int num = 0;
		for (int i = 0; i < line.Length; i++)
		{
			if (num >= column)
			{
				return i;
			}
			num = ((line[i] != '\t') ? (num + 1) : (num + (tabSize - num % tabSize)));
		}
		return line.Length;
	}

	internal static char Last(this string arg)
	{
		return arg[arg.Length - 1];
	}
}
