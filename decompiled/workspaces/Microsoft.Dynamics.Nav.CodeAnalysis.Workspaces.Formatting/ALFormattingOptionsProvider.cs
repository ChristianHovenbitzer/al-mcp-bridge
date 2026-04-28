using System.Collections.Immutable;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Options;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Formatting;

internal class ALFormattingOptionsProvider : IOptionProvider
{
	public ImmutableArray<IOption> Options { get; } = ImmutableArray.Create((IOption)ALFormattingOptions.SpacingAfterMethodDeclarationName, (IOption)ALFormattingOptions.WrappingPreserveSingleLine, (IOption)ALFormattingOptions.WrappingKeepStatementsOnSingleLine, (IOption)ALFormattingOptions.NewLinesForBracesInTypes);

}
