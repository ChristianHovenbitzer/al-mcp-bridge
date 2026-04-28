using System.Collections.Immutable;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Completion;

internal static class CommonCompletionItem
{
	public static CompletionItem Create(string displayText, TextSpan span = default(TextSpan), Glyph? glyph = null, string? descriptionText = null, string? documentation = null, string? detailText = null, string? sortText = null, string? filterText = null, string? insertionText = null, string? obsoleteInformation = null, bool preselect = false, bool showsWarningIcon = false, bool shouldFormatOnCommit = false, bool isArgumentName = false, bool isSnippet = false, bool isMarkdownDocs = false, bool isDeprecated = false, ImmutableDictionary<string, string>? properties = null, ImmutableArray<string> tags = default(ImmutableArray<string>), CompletionItemRules? rules = null)
	{
		tags = (tags.IsDefault ? ImmutableArray<string>.Empty : tags);
		if (glyph.HasValue)
		{
			tags = GlyphTags.GetTags(glyph.Value).AddRange(tags);
		}
		if (showsWarningIcon)
		{
			tags = tags.Add("Warning");
		}
		if (isArgumentName)
		{
			tags = tags.Add("ArgumentName");
		}
		if (isDeprecated)
		{
			tags = tags.Add("Deprecated");
		}
		rules = rules ?? CompletionItemRules.Default;
		rules = rules.WithPreselect(preselect).WithFormatOnCommit(shouldFormatOnCommit);
		ImmutableArray<string> tags2 = tags;
		CompletionItemRules rules2 = rules;
		return CompletionItem.Create(displayText, filterText, sortText, descriptionText, documentation, detailText, insertionText, obsoleteInformation, isSnippet, isMarkdownDocs, isDeprecated, span, properties, tags2, rules2);
	}
}
