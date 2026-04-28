using Microsoft.Dynamics.Nav.CodeAnalysis.Text;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces;

internal static class TextLineExtensions
{
	public static int? GetLastNonWhitespacePosition(this TextLine line)
	{
		int start = line.Start;
		string text = line.ToString();
		for (int num = text.Length - 1; num >= 0; num--)
		{
			if (!char.IsWhiteSpace(text[num]))
			{
				return start + num;
			}
		}
		return null;
	}

	public static int? GetFirstNonWhitespacePosition(this TextLine line)
	{
		int? firstNonWhitespaceOffset = line.GetFirstNonWhitespaceOffset();
		if (!firstNonWhitespaceOffset.HasValue)
		{
			return null;
		}
		return firstNonWhitespaceOffset + line.Start;
	}

	public static int? GetFirstNonWhitespaceOffset(this TextLine line)
	{
		return line.ToString().GetFirstNonWhitespaceOffset();
	}

	public static string GetLeadingWhitespace(this TextLine line)
	{
		return line.ToString().GetLeadingWhitespace();
	}

	public static bool IsEmptyOrWhitespace(this TextLine line)
	{
		return string.IsNullOrWhiteSpace(line.ToString());
	}

	public static int GetColumnOfFirstNonWhitespaceCharacterOrEndOfLine(this TextLine line, int tabSize)
	{
		return line.ToString().GetColumnOfFirstNonWhitespaceCharacterOrEndOfLine(tabSize);
	}

	public static int GetColumnFromLineOffset(this TextLine line, int lineOffset, int tabSize)
	{
		return line.ToString().GetColumnFromLineOffset(lineOffset, tabSize);
	}

	public static int GetLineOffsetFromColumn(this TextLine line, int column, int tabSize)
	{
		return line.ToString().GetLineOffsetFromColumn(column, tabSize);
	}
}
