using System.Collections.Generic;
using System.Collections.Immutable;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Options;

public sealed class OptionSet
{
	private readonly IOptionService service;

	private readonly object gate = new object();

	private ImmutableDictionary<OptionKey, object> values;

	internal OptionSet(IOptionService service)
	{
		this.service = service;
		values = ImmutableDictionary.Create<OptionKey, object>();
	}

	private OptionSet(IOptionService service, ImmutableDictionary<OptionKey, object> values)
	{
		this.service = service;
		this.values = values;
	}

	public T GetOption<T>(Option<T> option)
	{
		return (T)GetOption(new OptionKey(option));
	}

	public T GetOption<T>(PerLanguageOption<T> option, string language)
	{
		return (T)GetOption(new OptionKey(option, language));
	}

	public object GetOption(OptionKey optionKey)
	{
		lock (gate)
		{
			if (!values.TryGetValue(optionKey, out object value))
			{
				value = ((service != null) ? service.GetOption(optionKey) : optionKey.Option.DefaultValue);
				values = values.Add(optionKey, value);
			}
			return value;
		}
	}

	public OptionSet WithChangedOption<T>(Option<T> option, T value)
	{
		return WithChangedOption(new OptionKey(option), value);
	}

	public OptionSet WithChangedOption<T>(PerLanguageOption<T> option, string language, T value)
	{
		return WithChangedOption(new OptionKey(option, language), value);
	}

	public OptionSet WithChangedOption(OptionKey optionAndLanguage, object value)
	{
		GetOption(optionAndLanguage);
		return new OptionSet(service, values.SetItem(optionAndLanguage, value));
	}

	internal IEnumerable<OptionKey> GetAccessedOptions()
	{
		OptionSet options = service.GetOptions();
		return GetChangedOptions(options);
	}

	internal IEnumerable<OptionKey> GetChangedOptions(OptionSet optionSet)
	{
		foreach (KeyValuePair<OptionKey, object> value in values)
		{
			if (!object.Equals(optionSet.GetOption(value.Key), value.Value))
			{
				yield return value.Key;
			}
		}
	}
}
