using System.Collections.Immutable;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Completion;

internal abstract class CommonCompletionService : CompletionServiceWithProviders
{
	protected CommonCompletionService(Workspace workspace, ImmutableArray<CompletionProvider>? exclusiveProviders)
		: base(workspace, exclusiveProviders)
	{
	}

	protected override CompletionItem GetBetterItem(CompletionItem item, CompletionItem existingItem)
	{
		if (existingItem.Rules.Preselect && IsSnippetItem(item))
		{
			return existingItem;
		}
		return item;
	}

	protected static bool IsKeywordItem(CompletionItem item)
	{
		return item.Tags.Contains("Keyword");
	}

	protected static bool IsSnippetItem(CompletionItem item)
	{
		return item.Tags.Contains("Snippet");
	}
}
