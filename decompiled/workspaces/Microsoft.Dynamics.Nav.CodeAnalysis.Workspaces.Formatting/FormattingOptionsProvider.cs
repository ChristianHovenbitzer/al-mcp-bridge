using System.Collections.Immutable;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Options;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Formatting;

internal class FormattingOptionsProvider : IOptionProvider
{
	public ImmutableArray<IOption> Options { get; } = ImmutableArray.Create<IOption>(FormattingOptions.UseTabs, FormattingOptions.TabSize, FormattingOptions.IndentationSize, FormattingOptions.SmartIndent);

}
