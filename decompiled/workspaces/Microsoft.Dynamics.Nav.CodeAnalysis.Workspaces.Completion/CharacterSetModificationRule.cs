using System.Collections.Immutable;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Completion;

public struct CharacterSetModificationRule
{
	public CharacterSetModificationKind Kind { get; }

	public ImmutableArray<char> Characters { get; }

	private CharacterSetModificationRule(CharacterSetModificationKind kind, ImmutableArray<char> characters)
	{
		Kind = kind;
		Characters = characters;
	}

	public static CharacterSetModificationRule Create(CharacterSetModificationKind kind, ImmutableArray<char> characters)
	{
		return new CharacterSetModificationRule(kind, characters);
	}

	public static CharacterSetModificationRule Create(CharacterSetModificationKind kind, params char[] characters)
	{
		return new CharacterSetModificationRule(kind, characters.ToImmutableArray());
	}

	public static bool operator ==(CharacterSetModificationRule characterSetModificationRule1, CharacterSetModificationRule characterSetModificationRule2)
	{
		if (characterSetModificationRule1.Characters.Equals(characterSetModificationRule2.Characters))
		{
			return characterSetModificationRule1.Kind == characterSetModificationRule2.Kind;
		}
		return false;
	}

	public static bool operator !=(CharacterSetModificationRule characterSetModificationRule1, CharacterSetModificationRule characterSetModificationRule2)
	{
		return !(characterSetModificationRule1 == characterSetModificationRule2);
	}

	public override bool Equals(object obj)
	{
		if (obj == null || GetType() != obj.GetType())
		{
			return false;
		}
		CharacterSetModificationRule characterSetModificationRule = (CharacterSetModificationRule)obj;
		return this == characterSetModificationRule;
	}

	public override int GetHashCode()
	{
		return Characters.GetHashCode() ^ Kind.GetHashCode();
	}
}
