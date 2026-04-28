using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Options;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Formatting;

public static class FormattingOptions
{
	public enum IndentStyle
	{
		None,
		Block,
		Smart
	}

	public static PerLanguageOption<bool> UseTabs { get; } = new PerLanguageOption<bool>("FormattingOptions", "UseTabs", defaultValue: false);


	public static PerLanguageOption<int> TabSize { get; } = new PerLanguageOption<int>("FormattingOptions", "TabSize", 4);


	public static PerLanguageOption<int> IndentationSize { get; } = new PerLanguageOption<int>("FormattingOptions", "IndentationSize", 4);


	public static PerLanguageOption<IndentStyle> SmartIndent { get; } = new PerLanguageOption<IndentStyle>("FormattingOptions", "SmartIndent", IndentStyle.Smart);


	public static PerLanguageOption<string> NewLine { get; } = new PerLanguageOption<string>("FormattingOptions", "NewLine", "\r\n");


	internal static PerLanguageOption<bool> DebugMode { get; } = new PerLanguageOption<bool>("FormattingOptions", "DebugMode", defaultValue: false);


	internal static Option<bool> AllowDisjointSpanMerging { get; } = new Option<bool>("FormattingOptions", "AllowDisjointSpanMerging", defaultValue: false);

}
