using System;
using System.Collections.Generic;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Host;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Options;

internal interface IOptionService : IWorkspaceService
{
	event EventHandler<OptionChangedEventArgs> OptionChanged;

	T GetOption<T>(Option<T> option);

	T GetOption<T>(PerLanguageOption<T> option, string languageName);

	object GetOption(OptionKey optionKey);

	OptionSet GetOptions();

	void SetOptions(OptionSet optionSet);

	IEnumerable<IOption> GetRegisteredOptions();
}
