using System;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Options;

internal sealed class OptionChangedEventArgs : EventArgs
{
	private readonly OptionKey optionKey;

	public IOption Option => optionKey.Option;

	public string Language => optionKey.Language;

	public object Value { get; }

	internal OptionChangedEventArgs(OptionKey optionKey, object value)
	{
		this.optionKey = optionKey;
		Value = value;
	}
}
