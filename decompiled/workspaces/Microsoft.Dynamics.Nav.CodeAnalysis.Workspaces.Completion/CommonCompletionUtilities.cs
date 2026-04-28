using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.LanguageServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Completion;

internal static class CommonCompletionUtilities
{
	private const string NonBreakingSpaceString = "\u00a0";

	public static TextSpan GetWordSpan(SourceText text, int position, Func<char, bool> isWordStartCharacter, Func<char, bool> isWordCharacter)
	{
		int num = position;
		while (num > 0 && isWordStartCharacter(text[num - 1]))
		{
			num--;
		}
		int i = position;
		if (num != position)
		{
			for (; i < text.Length && isWordCharacter(text[i]); i++)
			{
			}
		}
		return TextSpan.FromBounds(num, i);
	}

	public static bool IsStartingNewWord(SourceText text, int characterPosition, Func<char, bool> isWordStartCharacter, Func<char, bool> isWordCharacter)
	{
		char arg = text[characterPosition];
		if (!isWordStartCharacter(arg))
		{
			return false;
		}
		if (characterPosition > 0 && isWordCharacter(text[characterPosition - 1]))
		{
			return false;
		}
		if (characterPosition < text.Length - 1 && isWordCharacter(text[characterPosition + 1]))
		{
			return false;
		}
		return true;
	}

	public static Func<CancellationToken, Task<CompletionDescription>> CreateDescriptionFactory(Workspace workspace, SemanticModel semanticModel, int position, ISymbol symbol)
	{
		return CreateDescriptionFactory(workspace, semanticModel, position, new ISymbol[1] { symbol });
	}

	public static Func<CancellationToken, Task<CompletionDescription>> CreateDescriptionFactory(Workspace workspace, SemanticModel semanticModel, int position, IReadOnlyList<ISymbol> symbols)
	{
		Workspace workspace2 = workspace;
		SemanticModel semanticModel2 = semanticModel;
		IReadOnlyList<ISymbol> symbols2 = symbols;
		return (CancellationToken c) => CreateDescriptionAsync(workspace2, semanticModel2, position, symbols2, null, c);
	}

	public static Func<CancellationToken, Task<CompletionDescription>> CreateDescriptionFactory(Workspace workspace, SemanticModel semanticModel, int position, IReadOnlyList<ISymbol> symbols, SupportedPlatformData supportedPlatform)
	{
		Workspace workspace2 = workspace;
		SemanticModel semanticModel2 = semanticModel;
		IReadOnlyList<ISymbol> symbols2 = symbols;
		SupportedPlatformData supportedPlatform2 = supportedPlatform;
		return (CancellationToken c) => CreateDescriptionAsync(workspace2, semanticModel2, position, symbols2, supportedPlatform2, c);
	}

	public static async Task<CompletionDescription> CreateDescriptionAsync(Workspace workspace, SemanticModel semanticModel, int position, IReadOnlyList<ISymbol> symbols, SupportedPlatformData supportedPlatforms, CancellationToken cancellationToken)
	{
		ISymbolDisplayService service = workspace.Services.GetLanguageServices("AL").GetService<ISymbolDisplayService>();
		ISymbol symbol = symbols[0];
		IDictionary<SymbolDescriptionGroups, ImmutableArray<SymbolDisplayPart>> dictionary = await service.ToDescriptionGroupsAsync(workspace, semanticModel, position, ImmutableArray.Create(symbol), cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		if (!dictionary.ContainsKey(SymbolDescriptionGroups.MainDescription))
		{
			return CompletionDescription.Empty;
		}
		List<SymbolDisplayPart> list = new List<SymbolDisplayPart>();
		list.AddRange(dictionary[SymbolDescriptionGroups.MainDescription]);
		SymbolKind kind = symbol.Kind;
		if ((kind == SymbolKind.NamedType || kind == SymbolKind.Method) && symbols.Count > 1)
		{
			int overloadCount = symbols.Count - 1;
			list.AddSpace();
			list.AddPunctuation("(");
			list.AddPunctuation("+");
			list.AddText("\u00a0" + overloadCount);
			AddOverloadPart(list, overloadCount);
			list.AddPunctuation(")");
		}
		if (supportedPlatforms != null)
		{
			list.AddLineBreak();
			list.AddRange(supportedPlatforms.ToDisplayParts());
		}
		return CompletionDescription.Create(list.Select((SymbolDisplayPart p) => new TaggedText(p.Kind.ToString(), p.ToString())).ToImmutableArray());
	}

	private static void AddOverloadPart(List<SymbolDisplayPart> textContentBuilder, int overloadCount)
	{
		string text = ((overloadCount == 1) ? WorkspacesResources.Overload : WorkspacesResources.Overloads);
		textContentBuilder.AddText("\u00a0" + text);
	}

	internal static bool IsTextualTriggerString(SourceText text, int characterPosition, string value)
	{
		characterPosition = characterPosition - value.Length + 1;
		int num = 0;
		while (num < value.Length)
		{
			if (characterPosition < 0 || characterPosition >= text.Length)
			{
				return false;
			}
			if (text[characterPosition] != value[num])
			{
				return false;
			}
			num++;
			characterPosition++;
		}
		return true;
	}
}
