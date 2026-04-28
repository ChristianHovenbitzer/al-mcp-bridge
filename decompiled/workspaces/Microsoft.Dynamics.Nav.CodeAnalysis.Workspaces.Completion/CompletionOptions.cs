using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Options;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Completion;

internal static class CompletionOptions
{
	internal const string FeatureName = "Completion";

	internal const string ControllerFeatureName = "CompletionController";

	public static readonly PerLanguageOption<bool> HideAdvancedMembers = new PerLanguageOption<bool>("Completion", "HideAdvancedMembers", defaultValue: false);

	public static readonly PerLanguageOption<bool> IncludeKeywords = new PerLanguageOption<bool>("Completion", "IncludeKeywords", defaultValue: true);

	public static readonly PerLanguageOption<bool> TriggerOnTyping = new PerLanguageOption<bool>("Completion", "TriggerOnTyping", defaultValue: true);

	public static readonly PerLanguageOption<bool> TriggerOnTypingLetters = new PerLanguageOption<bool>("Completion", "TriggerOnTypingLetters", defaultValue: true);

	public static readonly Option<bool> AlwaysShowBuilder = new Option<bool>("CompletionController", "AlwaysShowBuilder", defaultValue: false);

	public static readonly Option<bool> FilterOutOfScopeLocals = new Option<bool>("CompletionController", "FilterOutOfScopeLocals", defaultValue: true);

	public static readonly Option<bool> ShowXmlDocCommentCompletion = new Option<bool>("CompletionController", "ShowXmlDocCommentCompletion", defaultValue: true);

	public static readonly Option<bool> AddNewLineOnEnterAfterFullyTypedWord = new Option<bool>("Completion", "Add New Line On Enter After Fully Typed Word", defaultValue: false);

	public static readonly Option<bool> SerializeCompletionResult = new Option<bool>("CompletionController", "Serialize", defaultValue: false);

	public static readonly Option<bool> IncludeSnippets = new Option<bool>("Completion", "Include Code Snippets", defaultValue: true);
}
