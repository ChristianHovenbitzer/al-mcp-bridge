using System;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Options;

public struct OptionKey : IEquatable<OptionKey>
{
	public IOption Option { get; }

	public string Language { get; }

	public OptionKey(IOption option, string language = null)
	{
		if (option == null)
		{
			throw new ArgumentNullException("option");
		}
		if (language != null && !option.IsPerLanguage)
		{
			throw new ArgumentException(WorkspacesResources.InvalidLanguageNameOption);
		}
		if (language == null && option.IsPerLanguage)
		{
			throw new ArgumentNullException(WorkspacesResources.InvalidLanguageNameOption2);
		}
		Option = option;
		Language = language;
	}

	public override bool Equals(object obj)
	{
		if (obj is OptionKey)
		{
			return Equals((OptionKey)obj);
		}
		return false;
	}

	public bool Equals(OptionKey other)
	{
		if (Option == other.Option)
		{
			return Language == other.Language;
		}
		return false;
	}

	public override int GetHashCode()
	{
		int num = Option.GetHashCode();
		if (Language != null)
		{
			num = Hash.Combine(Language.GetHashCode(), num);
		}
		return num;
	}

	public static bool operator ==(OptionKey left, OptionKey right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(OptionKey left, OptionKey right)
	{
		return !left.Equals(right);
	}
}
