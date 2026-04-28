using System;
using System.Globalization;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Options;

public sealed class PerLanguageOption<T> : IOption
{
	public string Feature { get; }

	public string Name { get; }

	public T DefaultValue { get; }

	Type IOption.Type => typeof(T);

	object IOption.DefaultValue => DefaultValue;

	bool IOption.IsPerLanguage => true;

	public PerLanguageOption(string feature, string name, T defaultValue)
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
}
