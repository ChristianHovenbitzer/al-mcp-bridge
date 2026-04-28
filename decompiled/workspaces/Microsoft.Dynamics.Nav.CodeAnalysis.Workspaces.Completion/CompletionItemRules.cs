using System.Collections.Immutable;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Completion;

public sealed class CompletionItemRules
{
	public static CompletionItemRules Default = new CompletionItemRules(default(ImmutableArray<CharacterSetModificationRule>), default(ImmutableArray<CharacterSetModificationRule>), EnterKeyRule.Default, formatOnCommit: false, preselect: false);

	public ImmutableArray<CharacterSetModificationRule> FilterCharacterRules { get; }

	public ImmutableArray<CharacterSetModificationRule> CommitCharacterRules { get; }

	public EnterKeyRule EnterKeyRule { get; }

	public bool FormatOnCommit { get; }

	public bool Preselect { get; }

	private CompletionItemRules(ImmutableArray<CharacterSetModificationRule> filterCharacterRules, ImmutableArray<CharacterSetModificationRule> commitCharacterRules, EnterKeyRule enterKeyRule, bool formatOnCommit, bool preselect)
	{
		FilterCharacterRules = (filterCharacterRules.IsDefault ? ImmutableArray<CharacterSetModificationRule>.Empty : filterCharacterRules);
		CommitCharacterRules = (commitCharacterRules.IsDefault ? ImmutableArray<CharacterSetModificationRule>.Empty : commitCharacterRules);
		EnterKeyRule = enterKeyRule;
		FormatOnCommit = formatOnCommit;
		Preselect = preselect;
	}

	public static CompletionItemRules Create(ImmutableArray<CharacterSetModificationRule> filterCharacterRules = default(ImmutableArray<CharacterSetModificationRule>), ImmutableArray<CharacterSetModificationRule> commitCharacterRules = default(ImmutableArray<CharacterSetModificationRule>), EnterKeyRule enterKeyRule = EnterKeyRule.Default, bool formatOnCommit = false, bool preselect = false)
	{
		if (filterCharacterRules.IsDefaultOrEmpty && commitCharacterRules.IsDefaultOrEmpty && enterKeyRule == Default.EnterKeyRule && formatOnCommit == Default.FormatOnCommit && preselect == Default.Preselect)
		{
			return Default;
		}
		return new CompletionItemRules(filterCharacterRules, commitCharacterRules, enterKeyRule, formatOnCommit, preselect);
	}

	private CompletionItemRules With(Optional<ImmutableArray<CharacterSetModificationRule>> filterRules = default(Optional<ImmutableArray<CharacterSetModificationRule>>), Optional<ImmutableArray<CharacterSetModificationRule>> commitRules = default(Optional<ImmutableArray<CharacterSetModificationRule>>), Optional<EnterKeyRule> enterKeyRule = default(Optional<EnterKeyRule>), Optional<bool> formatOnCommit = default(Optional<bool>), Optional<bool> preselect = default(Optional<bool>))
	{
		ImmutableArray<CharacterSetModificationRule> immutableArray = (filterRules.HasValue ? filterRules.Value : FilterCharacterRules);
		ImmutableArray<CharacterSetModificationRule> immutableArray2 = (commitRules.HasValue ? commitRules.Value : CommitCharacterRules);
		EnterKeyRule enterKeyRule2 = (enterKeyRule.HasValue ? enterKeyRule.Value : EnterKeyRule);
		bool flag = (formatOnCommit.HasValue ? formatOnCommit.Value : FormatOnCommit);
		bool flag2 = (preselect.HasValue ? preselect.Value : Preselect);
		if (immutableArray == FilterCharacterRules && immutableArray2 == CommitCharacterRules && enterKeyRule2 == EnterKeyRule && flag == FormatOnCommit && flag2 == Preselect)
		{
			return this;
		}
		return Create(immutableArray, immutableArray2, enterKeyRule2, flag, flag2);
	}

	public CompletionItemRules WithFilterCharacterRules(ImmutableArray<CharacterSetModificationRule> filterCharacterRules)
	{
		return With(filterCharacterRules);
	}

	public CompletionItemRules WithCommitCharacterRules(ImmutableArray<CharacterSetModificationRule> commitCharacterRules)
	{
		Optional<ImmutableArray<CharacterSetModificationRule>> commitRules = commitCharacterRules;
		return With(default(Optional<ImmutableArray<CharacterSetModificationRule>>), commitRules);
	}

	public CompletionItemRules WithEnterKeyRule(EnterKeyRule enterKeyRule)
	{
		Optional<EnterKeyRule> enterKeyRule2 = enterKeyRule;
		return With(default(Optional<ImmutableArray<CharacterSetModificationRule>>), default(Optional<ImmutableArray<CharacterSetModificationRule>>), enterKeyRule2);
	}

	public CompletionItemRules WithFormatOnCommit(bool formatOnCommit)
	{
		Optional<bool> formatOnCommit2 = formatOnCommit;
		return With(default(Optional<ImmutableArray<CharacterSetModificationRule>>), default(Optional<ImmutableArray<CharacterSetModificationRule>>), default(Optional<EnterKeyRule>), formatOnCommit2);
	}

	public CompletionItemRules WithPreselect(bool preselect)
	{
		Optional<bool> preselect2 = preselect;
		return With(default(Optional<ImmutableArray<CharacterSetModificationRule>>), default(Optional<ImmutableArray<CharacterSetModificationRule>>), default(Optional<EnterKeyRule>), default(Optional<bool>), preselect2);
	}
}
