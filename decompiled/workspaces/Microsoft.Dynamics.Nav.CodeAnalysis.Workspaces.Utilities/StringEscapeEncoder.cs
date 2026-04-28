using System;
using System.Globalization;
using System.Text;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Utilities;

internal static class StringEscapeEncoder
{
	public static string Escape(this string text, char escapePrefix, params char[] prohibitedCharacters)
	{
		StringBuilder stringBuilder = null;
		int num = 0;
		while (num < text.Length)
		{
			int num2 = text.IndexOf(escapePrefix, num);
			int num3 = text.IndexOfAny(prohibitedCharacters, num);
			int num4 = ((num2 > 0 && num3 > 0) ? Math.Min(num2, num3) : ((num2 > 0) ? num2 : ((num3 > 0) ? num3 : (-1))));
			if (num4 < 0)
			{
				stringBuilder?.Append(text, num, text.Length - num);
				break;
			}
			if (stringBuilder == null)
			{
				stringBuilder = new StringBuilder();
			}
			if (num4 > num)
			{
				stringBuilder.Append(text, num, num4 - num);
			}
			stringBuilder.Append(escapePrefix);
			stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "{0:X2}", (int)text[num4]);
			num = num4 + 1;
		}
		if (stringBuilder != null)
		{
			return stringBuilder.ToString();
		}
		return text;
	}

	public static string Unescape(this string text, char escapePrefix)
	{
		StringBuilder stringBuilder = null;
		int num = 0;
		while (num < text.Length)
		{
			int num2 = text.IndexOf(escapePrefix, num);
			if (num2 < 0)
			{
				stringBuilder?.Append(text, num, text.Length - num);
				break;
			}
			if (stringBuilder == null)
			{
				stringBuilder = new StringBuilder();
			}
			stringBuilder.Append(text, num, num2 - num);
			int num3 = ParseHex(text, num2 + 1, 2);
			stringBuilder.Append((char)num3);
			num = num2 + 3;
		}
		if (stringBuilder != null)
		{
			return stringBuilder.ToString();
		}
		return text;
	}

	private static int ParseHex(string text, int start, int length)
	{
		int num = 0;
		int i = start;
		for (int num2 = start + length; i < num2; i++)
		{
			char ch = text[i];
			if (!IsHexDigit(ch))
			{
				break;
			}
			num = (num << 4) + GetHexValue(ch);
		}
		return num;
	}

	private static bool IsHexDigit(char ch)
	{
		if ((ch < '0' || ch > '9') && (ch < 'A' || ch > 'F'))
		{
			if (ch >= 'a')
			{
				return ch <= 'f';
			}
			return false;
		}
		return true;
	}

	private static int GetHexValue(char ch)
	{
		if (ch >= '0' && ch <= '9')
		{
			return ch - 48;
		}
		if (ch >= 'A' && ch <= 'F')
		{
			return ch - 65 + 10;
		}
		if (ch >= 'a' && ch <= 'f')
		{
			return ch - 97 + 10;
		}
		throw new InvalidOperationException();
	}
}
