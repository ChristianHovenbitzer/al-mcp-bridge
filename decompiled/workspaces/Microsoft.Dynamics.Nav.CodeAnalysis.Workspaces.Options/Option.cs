using System;
using System.Globalization;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Options;

public sealed class Option<T> : IOption
{
	public string Feature { get; }

	public string Name { get; }

	public T DefaultValue { get; }

	Type IOption.Type => typeof(T);

	object IOption.DefaultValue => DefaultValue;

	bool IOption.IsPerLanguage => false;

	public Option(string feature, string name, T defaultValue = default(T))
	{
		if (string.IsNullOrWhiteSpace(feature))
		{
			throw new ArgumentNullException("feature");
		}
		if (string.IsNullOrWhiteSpace(name))
		{
			throw new ArgumentException(WorkspacesResources.NameIsEmpty, "name");
		}
		Feature = feature;
		Name = name;
		DefaultValue = defaultValue;
	}

	public override string ToString()
	{
		return string.Format(CultureInfo.InvariantCulture, "{0} - {1}", Feature, Name);
	}

	public static implicit operator OptionKey(Option<T> option)
	{
		return new OptionKey(option);
	}
}
