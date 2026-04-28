using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Host;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Options;

internal class OptionService : IOptionService, IWorkspaceService
{
	private readonly Lazy<HashSet<IOption>> options;

	private readonly object gate = new object();

	private ImmutableDictionary<OptionKey, object> currentValues;

	public event EventHandler<OptionChangedEventArgs> OptionChanged;

	public OptionService(IEnumerable<IOptionProvider> optionProviders)
	{
		IEnumerable<IOptionProvider> optionProviders2 = optionProviders;
		base._002Ector();
		options = new Lazy<HashSet<IOption>>(delegate
		{
			HashSet<IOption> hashSet = new HashSet<IOption>();
			foreach (IOptionProvider item in optionProviders2)
			{
				hashSet.AddRange(item.Options);
			}
			return hashSet;
		});
		currentValues = ImmutableDictionary.Create<OptionKey, object>();
	}

	public IEnumerable<IOption> GetRegisteredOptions()
	{
		return options.Value;
	}

	public OptionSet GetOptions()
	{
		return new OptionSet(this);
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
			if (currentValues.TryGetValue(optionKey, out object value))
			{
				return value;
			}
			value = optionKey.Option.DefaultValue;
			currentValues = currentValues.Add(optionKey, value);
			return value;
		}
	}

	public void SetOptions(OptionSet optionSet)
	{
		if (optionSet == null)
		{
			throw new ArgumentNullException("optionSet");
		}
		List<OptionChangedEventArgs> list = new List<OptionChangedEventArgs>();
		lock (gate)
		{
			foreach (OptionKey accessedOption in optionSet.GetAccessedOptions())
			{
				object option = optionSet.GetOption(accessedOption);
				if (!object.Equals(GetOption(accessedOption), option))
				{
					list.Add(new OptionChangedEventArgs(accessedOption, option));
					currentValues = currentValues.SetItem(accessedOption, option);
				}
			}
		}
		EventHandler<OptionChangedEventArgs> optionChanged = this.OptionChanged;
		if (optionChanged == null)
		{
			return;
		}
		foreach (OptionChangedEventArgs item in list)
		{
			optionChanged(this, item);
		}
	}
}
