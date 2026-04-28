using System.Collections.Immutable;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Completion;

public sealed class CompletionRules
{
	private static readonly ImmutableArray<char> s_defaultCommitKeys = ImmutableArray.Create<char>(' ', '{', '}', '[', ']', '(', ')', '.', ',', ':', ';', '+', '-', '*', '/', '%', '&', '|', '^', '!', '~', '=', '<', '>', '?', '@', '#', '\'', '"', '\\');

	public static readonly CompletionRules Default = new CompletionRules(dismissIfEmpty: false, dismissIfLastCharacterDeleted: false, s_defaultCommitKeys, EnterKeyRule.Never);

	public bool DismissIfEmpty { get; }

	public bool DismissIfLastCharacterDeleted { get; }

	public ImmutableArray<char> DefaultCommitCharacters { get; }

	public EnterKeyRule DefaultEnterKeyRule { get; }

	private CompletionRules(bool dismissIfEmpty, bool dismissIfLastCharacterDeleted, ImmutableArray<char> defaultCommitCharacters, EnterKeyRule defaultEnterKeyRule)
	{
		DismissIfEmpty = dismissIfEmpty;
		DismissIfLastCharacterDeleted = dismissIfLastCharacterDeleted;
		DefaultCommitCharacters = (defaultCommitCharacters.IsDefault ? ImmutableArray<char>.Empty : defaultCommitCharacters);
		DefaultEnterKeyRule = defaultEnterKeyRule;
	}

	public static CompletionRules Create(bool dismissIfEmpty = false, bool dismissIfLastCharacterDeleted = false, ImmutableArray<char> defaultCommitCharacters = default(ImmutableArray<char>), EnterKeyRule defaultEnterKeyRule = EnterKeyRule.Default)
	{
		return new CompletionRules(dismissIfEmpty, dismissIfLastCharacterDeleted, defaultCommitCharacters, defaultEnterKeyRule);
	}

	private CompletionRules With(Optional<bool> dismissIfEmpty = default(Optional<bool>), Optional<bool> dismissIfLastCharacterDeleted = default(Optional<bool>), Optional<ImmutableArray<char>> defaultCommitCharacters = default(Optional<ImmutableArray<char>>), Optional<EnterKeyRule> defaultEnterKeyRule = default(Optional<EnterKeyRule>))
	{
		bool flag = (dismissIfEmpty.HasValue ? dismissIfEmpty.Value : DismissIfEmpty);
		bool flag2 = (dismissIfLastCharacterDeleted.HasValue ? dismissIfLastCharacterDeleted.Value : DismissIfLastCharacterDeleted);
		ImmutableArray<char> immutableArray = (defaultCommitCharacters.HasValue ? defaultCommitCharacters.Value : DefaultCommitCharacters);
		EnterKeyRule enterKeyRule = (defaultEnterKeyRule.HasValue ? defaultEnterKeyRule.Value : DefaultEnterKeyRule);
		if (flag == DismissIfEmpty && flag2 == DismissIfLastCharacterDeleted && immutableArray == DefaultCommitCharacters && enterKeyRule == DefaultEnterKeyRule)
		{
			return this;
		}
		return Create(flag, flag2, immutableArray, enterKeyRule);
	}

	public CompletionRules WithDismissIfEmpty(bool dismissIfEmpty)
	{
		return With(dismissIfEmpty);
	}

	public CompletionRules WithDismissIfLastCharacterDeleted(bool dismissIfLastCharacterDeleted)
	{
		Optional<bool> dismissIfLastCharacterDeleted2 = dismissIfLastCharacterDeleted;
		return With(default(Optional<bool>), dismissIfLastCharacterDeleted2);
	}

	public CompletionRules WithDefaultCommitCharacters(ImmutableArray<char> defaultCommitCharacters)
	{
		Optional<ImmutableArray<char>> defaultCommitCharacters2 = defaultCommitCharacters;
		return With(default(Optional<bool>), default(Optional<bool>), defaultCommitCharacters2);
	}

	public CompletionRules WithDefaultEnterKeyRule(EnterKeyRule defaultEnterKeyRule)
	{
		Optional<EnterKeyRule> defaultEnterKeyRule2 = defaultEnterKeyRule;
		return With(default(Optional<bool>), default(Optional<bool>), default(Optional<ImmutableArray<char>>), defaultEnterKeyRule2);
	}
}
