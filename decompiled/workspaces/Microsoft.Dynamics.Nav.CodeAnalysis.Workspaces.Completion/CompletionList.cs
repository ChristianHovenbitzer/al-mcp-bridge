using System.Collections.Immutable;
using System.Linq;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Completion;

public sealed class CompletionList
{
	public static readonly CompletionList Empty = new CompletionList(default(TextSpan), default(ImmutableArray<CompletionItem>), null, isExclusive: false);

	public ImmutableArray<CompletionItem> Items { get; }

	public TextSpan DefaultSpan { get; }

	public CompletionItem SuggestionModeItem { get; }

	internal bool IsExclusive { get; }

	private CompletionList(TextSpan defaultSpan, ImmutableArray<CompletionItem> items, CompletionItem suggestionModeItem, bool isExclusive)
	{
		DefaultSpan = defaultSpan;
		Items = (items.IsDefault ? ImmutableArray<CompletionItem>.Empty : items);
		SuggestionModeItem = suggestionModeItem;
		IsExclusive = isExclusive;
	}

	public static CompletionList Create(TextSpan defaultSpan, ImmutableArray<CompletionItem> items, CompletionItem suggestionModeItem = null)
	{
		return Create(defaultSpan, items, suggestionModeItem, isExclusive: false);
	}

	internal static CompletionList Create(TextSpan defaultSpan, ImmutableArray<CompletionItem> items, CompletionItem suggestionModeItem, bool isExclusive)
	{
		return new CompletionList(defaultSpan, FixItemSpans(items, defaultSpan), suggestionModeItem, isExclusive);
	}

	private static ImmutableArray<CompletionItem> FixItemSpans(ImmutableArray<CompletionItem> items, TextSpan defaultSpan)
	{
		if (defaultSpan != default(TextSpan) && items.Any((CompletionItem i) => i.Span == default(TextSpan)))
		{
			items = items.Select((CompletionItem i) => (!(i.Span == default(TextSpan))) ? i : i.WithSpan(defaultSpan)).ToImmutableArray();
		}
		return items;
	}

	private CompletionList With(Optional<TextSpan> span = default(Optional<TextSpan>), Optional<ImmutableArray<CompletionItem>> items = default(Optional<ImmutableArray<CompletionItem>>), Optional<CompletionItem> suggestionModeItem = default(Optional<CompletionItem>))
	{
		TextSpan textSpan = (span.HasValue ? span.Value : DefaultSpan);
		ImmutableArray<CompletionItem> immutableArray = (items.HasValue ? items.Value : Items);
		CompletionItem completionItem = (suggestionModeItem.HasValue ? suggestionModeItem.Value : SuggestionModeItem);
		if (textSpan == DefaultSpan && immutableArray == Items && completionItem == SuggestionModeItem)
		{
			return this;
		}
		return Create(textSpan, immutableArray, completionItem);
	}

	public CompletionList WithDefaultSpan(TextSpan span)
	{
		return With(span);
	}

	public CompletionList WithItems(ImmutableArray<CompletionItem> items)
	{
		Optional<ImmutableArray<CompletionItem>> items2 = items;
		return With(default(Optional<TextSpan>), items2);
	}

	public CompletionList WithSuggestionModeItem(CompletionItem suggestionModeItem)
	{
		Optional<CompletionItem> suggestionModeItem2 = suggestionModeItem;
		return With(default(Optional<TextSpan>), default(Optional<ImmutableArray<CompletionItem>>), suggestionModeItem2);
	}
}
