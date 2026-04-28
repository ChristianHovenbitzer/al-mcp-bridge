using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Options;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Formatting;

public static class ALFormattingOptions
{
	public static Option<bool> SpacingAfterMethodDeclarationName { get; } = new Option<bool>("ALFormattingOptions", "SpacingAfterMethodDeclarationName", defaultValue: false);


	public static Option<bool> IndentBraces { get; } = new Option<bool>("ALFormattingOptions", "IndentBraces", defaultValue: false);


	public static Option<bool> WrappingPreserveSingleLine { get; } = new Option<bool>("ALFormattingOptions", "WrappingPreserveSingleLine", defaultValue: true);


	public static Option<bool> WrappingKeepStatementsOnSingleLine { get; } = new Option<bool>("ALFormattingOptions", "WrappingKeepStatementsOnSingleLine", defaultValue: true);


	public static Option<bool> NewLinesForBracesInTypes { get; } = new Option<bool>("ALFormattingOptions", "NewLinesForBracesInTypes", defaultValue: true);

}
