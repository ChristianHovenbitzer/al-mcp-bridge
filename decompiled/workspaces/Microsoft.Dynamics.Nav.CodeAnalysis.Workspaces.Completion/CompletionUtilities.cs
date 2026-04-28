using Microsoft.Dynamics.Nav.CodeAnalysis.Text;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Options;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Completion;

internal static class CompletionUtilities
{
	internal static TextSpan GetCompletionItemSpan(SourceText text, int position)
	{
		return CommonCompletionUtilities.GetWordSpan(text, position, IsCompletionItemStartCharacter, IsWordCharacter);
	}

	public static bool IsWordStartCharacter(char ch)
	{
		return SyntaxFacts.IsIdentifierStartCharacter(ch);
	}

	public static bool IsWordCharacter(char ch)
	{
		if (!SyntaxFacts.IsIdentifierStartCharacter(ch))
		{
			return SyntaxFacts.IsIdentifierPartCharacter(ch);
		}
		return true;
	}

	public static CompletionItem CreateTriggerCompletionItem(string displayText, string insertionText, string documentationText = null, string descriptionText = null, string detailText = null)
	{
		string detailText2 = detailText ?? string.Empty;
		string descriptionText2 = descriptionText ?? string.Empty;
		string documentation = documentationText ?? string.Empty;
		Glyph? glyph = Glyph.Trigger;
		return CommonCompletionItem.Create(displayText, default(TextSpan), glyph, descriptionText2, documentation, detailText2, null, null, insertionText, null, preselect: false, showsWarningIcon: false, shouldFormatOnCommit: false, isArgumentName: false, isSnippet: false, isMarkdownDocs: true);
	}

	public static CompletionItem CreateEnumAccessCompilationItem(ISymbol symbol, TextSpan span)
	{
		string text = symbol.ToDisplayString(SymbolDisplayFormat.SymbolCompletionFormat);
		return SymbolCompletionItem.Create(text, span, new ISymbol[1] { symbol }, -1, -1, null, text + SyntaxKind.ColonColonToken.GetText(), symbol.GetGlyph());
	}

	public static bool IsCompletionItemStartCharacter(char ch)
	{
		if (ch != '"')
		{
			return IsWordCharacter(ch);
		}
		return true;
	}

	internal static bool IsTriggerCharacter(SourceText text, int characterPosition, OptionSet options)
	{
		switch (text[characterPosition])
		{
		case '.':
			return true;
		case ':':
			if (characterPosition >= 1 && text[characterPosition - 1] == ':')
			{
				return true;
			}
			break;
		}
		if (options.GetOption(CompletionOptions.TriggerOnTypingLetters, "AL") && IsStartingNewWord(text, characterPosition))
		{
			return true;
		}
		return false;
	}

	internal static bool IsTriggerAfterSpaceOrStartOfWordCharacter(SourceText text, int characterPosition, OptionSet options)
	{
		if (!SpaceTypedNotBeforeWord(text[characterPosition], text, characterPosition))
		{
			if (IsStartingNewWord(text, characterPosition))
			{
				return options.GetOption(CompletionOptions.TriggerOnTypingLetters, "AL");
			}
			return false;
		}
		return true;
	}

	private static bool SpaceTypedNotBeforeWord(char ch, SourceText text, int characterPosition)
	{
		if (ch == ' ')
		{
			if (characterPosition != text.Length - 1)
			{
				return !IsWordStartCharacter(text[characterPosition + 1]);
			}
			return true;
		}
		return false;
	}

	public static bool IsStartingNewWord(SourceText text, int characterPosition)
	{
		return CommonCompletionUtilities.IsStartingNewWord(text, characterPosition, IsWordStartCharacter, IsWordCharacter);
	}
}
