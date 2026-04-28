using System;
using System.Collections.Immutable;
using System.Diagnostics;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Completion;

[DebuggerDisplay("{DisplayText}")]
public sealed class CompletionItem : IComparable<CompletionItem>
{
	public string DisplayText { get; }

	public string FilterText { get; }

	public string SortText { get; }

	public string? DescriptionValue { get; }

	public string? Documentation { get; }

	public string? ObsoleteInformation { get; }

	public string? DetailText { get; }

	public string InsertionText { get; }

	public bool IsSnippet { get; set; }

	public bool IsMarkdownDocs { get; set; }

	public bool IsDeprecated { get; set; }

	public TextSpan Span { get; }

	public ImmutableDictionary<string, string> Properties { get; }

	public ImmutableArray<string> Tags { get; }

	public CompletionItemRules Rules { get; }

	private CompletionItem(string displayText, string? filterText, string? sortText, string? descriptionValue, string? documentation, string? detailText, string? insertionText, string? obsoleteInformation, bool isSnippet, bool isMarkdownDocs, bool isDeprecated, TextSpan span, ImmutableDictionary<string, string>? properties, ImmutableArray<string> tags, CompletionItemRules? rules = null)
	{
		DisplayText = displayText ?? string.Empty;
		FilterText = filterText ?? DisplayText;
		SortText = sortText ?? DisplayText;
		DescriptionValue = descriptionValue;
		DetailText = detailText;
		InsertionText = insertionText ?? DisplayText;
		ObsoleteInformation = obsoleteInformation;
		Span = span;
		Properties = properties ?? ImmutableDictionary<string, string>.Empty;
		Tags = (tags.IsDefault ? ImmutableArray<string>.Empty : tags);
		Rules = rules ?? CompletionItemRules.Default;
		IsSnippet = isSnippet;
		IsMarkdownDocs = isMarkdownDocs;
		IsDeprecated = isDeprecated;
		Documentation = documentation;
	}

	public static CompletionItem Create(string displayText, string? filterText = null, string? sortText = null, string? descriptionText = null, string? documentation = null, string? detailText = null, string? insertionText = null, string? obsoleteInformation = null, bool isSnippet = false, bool isMarkdownDocs = false, bool isDeprecated = false, TextSpan span = default(TextSpan), ImmutableDictionary<string, string>? properties = null, ImmutableArray<string> tags = default(ImmutableArray<string>), CompletionItemRules? rules = null)
	{
		return new CompletionItem(displayText, filterText, sortText, descriptionText, documentation, detailText, insertionText, obsoleteInformation, isSnippet, isMarkdownDocs, isDeprecated, span, properties, tags, rules);
	}

	private CompletionItem With(Optional<TextSpan> span = default(Optional<TextSpan>), Optional<string> displayText = default(Optional<string>), Optional<string> detailText = default(Optional<string>), Optional<string> filterText = default(Optional<string>), Optional<string> descriptionText = default(Optional<string>), Optional<string> documentation = default(Optional<string>), Optional<string> sortText = default(Optional<string>), Optional<string> insertionText = default(Optional<string>), Optional<string> obsoleteInformation = default(Optional<string>), Optional<bool> isSnippet = default(Optional<bool>), Optional<bool> isMarkdownDocs = default(Optional<bool>), Optional<bool> isDeprecated = default(Optional<bool>), Optional<ImmutableDictionary<string, string>> properties = default(Optional<ImmutableDictionary<string, string>>), Optional<ImmutableArray<string>> tags = default(Optional<ImmutableArray<string>>), Optional<CompletionItemRules> rules = default(Optional<CompletionItemRules>))
	{
		TextSpan textSpan = (span.HasValue ? span.Value : Span);
		string text = (displayText.HasValue ? displayText.Value : DisplayText);
		string text2 = (detailText.HasValue ? detailText.Value : DetailText);
		string text3 = (filterText.HasValue ? filterText.Value : FilterText);
		string text4 = (sortText.HasValue ? sortText.Value : SortText);
		ImmutableDictionary<string, string> immutableDictionary = (properties.HasValue ? properties.Value : Properties);
		ImmutableArray<string> immutableArray = (tags.HasValue ? tags.Value : Tags);
		CompletionItemRules completionItemRules = (rules.HasValue ? rules.Value : Rules);
		string text5 = (insertionText.HasValue ? insertionText.Value : InsertionText);
		string text6 = (descriptionText.HasValue ? descriptionText.Value : DescriptionValue);
		string text7 = (documentation.HasValue ? documentation.Value : Documentation);
		string text8 = (obsoleteInformation.HasValue ? obsoleteInformation.Value : ObsoleteInformation);
		bool flag = (isSnippet.HasValue ? isSnippet.HasValue : IsSnippet);
		bool flag2 = (isMarkdownDocs.HasValue ? isMarkdownDocs.HasValue : IsMarkdownDocs);
		bool flag3 = (isDeprecated.HasValue ? isDeprecated.HasValue : IsDeprecated);
		if (textSpan == Span && text == DisplayText && text2 == DetailText && text3 == FilterText && text4 == SortText && immutableDictionary == Properties && immutableArray == Tags && completionItemRules == Rules && text5 == InsertionText && text6 == DescriptionValue && text7 == Documentation && text8 == ObsoleteInformation && flag == IsSnippet && flag2 == IsMarkdownDocs && flag3 == IsDeprecated)
		{
			return this;
		}
		TextSpan span2 = textSpan;
		ImmutableDictionary<string, string> properties2 = immutableDictionary;
		ImmutableArray<string> tags2 = immutableArray;
		CompletionItemRules rules2 = completionItemRules;
		return Create(text, text3, text4, text6, text7, text2, text5, text8, flag, flag2, flag3, span2, properties2, tags2, rules2);
	}

	public CompletionItem WithSpan(TextSpan span)
	{
		return With(span);
	}

	public CompletionItem WithDisplayText(string text)
	{
		Optional<string> displayText = text;
		return With(default(Optional<TextSpan>), displayText);
	}

	public CompletionItem WithDetailText(string text)
	{
		Optional<string> detailText = text;
		return With(default(Optional<TextSpan>), default(Optional<string>), detailText);
	}

	public CompletionItem WithFilterText(string text)
	{
		Optional<string> filterText = text;
		return With(default(Optional<TextSpan>), default(Optional<string>), default(Optional<string>), filterText);
	}

	public CompletionItem WithSortText(string text)
	{
		Optional<string> sortText = text;
		return With(default(Optional<TextSpan>), default(Optional<string>), default(Optional<string>), default(Optional<string>), default(Optional<string>), default(Optional<string>), sortText);
	}

	public CompletionItem WithInsertionText(string text)
	{
		Optional<string> insertionText = text;
		return With(default(Optional<TextSpan>), default(Optional<string>), default(Optional<string>), default(Optional<string>), default(Optional<string>), default(Optional<string>), default(Optional<string>), insertionText);
	}

	public CompletionItem WithProperties(ImmutableDictionary<string, string> properties)
	{
		Optional<ImmutableDictionary<string, string>> properties2 = properties;
		return With(default(Optional<TextSpan>), default(Optional<string>), default(Optional<string>), default(Optional<string>), default(Optional<string>), default(Optional<string>), default(Optional<string>), default(Optional<string>), default(Optional<string>), default(Optional<bool>), default(Optional<bool>), default(Optional<bool>), properties2);
	}

	public CompletionItem AddProperty(string name, string value)
	{
		Optional<ImmutableDictionary<string, string>> properties = Properties.Add(name, value);
		return With(default(Optional<TextSpan>), default(Optional<string>), default(Optional<string>), default(Optional<string>), default(Optional<string>), default(Optional<string>), default(Optional<string>), default(Optional<string>), default(Optional<string>), default(Optional<bool>), default(Optional<bool>), default(Optional<bool>), properties);
	}

	public CompletionItem WithTags(ImmutableArray<string> tags)
	{
		Optional<ImmutableArray<string>> tags2 = tags;
		return With(default(Optional<TextSpan>), default(Optional<string>), default(Optional<string>), default(Optional<string>), default(Optional<string>), default(Optional<string>), default(Optional<string>), default(Optional<string>), default(Optional<string>), default(Optional<bool>), default(Optional<bool>), default(Optional<bool>), default(Optional<ImmutableDictionary<string, string>>), tags2);
	}

	public CompletionItem AddTag(string tag)
	{
		if (tag == null)
		{
			throw new ArgumentNullException("tag");
		}
		if (Tags.Contains(tag))
		{
			return this;
		}
		Optional<ImmutableArray<string>> tags = Tags.Add(tag);
		return With(default(Optional<TextSpan>), default(Optional<string>), default(Optional<string>), default(Optional<string>), default(Optional<string>), default(Optional<string>), default(Optional<string>), default(Optional<string>), default(Optional<string>), default(Optional<bool>), default(Optional<bool>), default(Optional<bool>), default(Optional<ImmutableDictionary<string, string>>), tags);
	}

	public int CompareTo(CompletionItem other)
	{
		int num = StringComparer.OrdinalIgnoreCase.Compare(SortText, other.SortText);
		if (num == 0)
		{
			num = StringComparer.OrdinalIgnoreCase.Compare(DisplayText, other.DisplayText);
		}
		return num;
	}

	public override string ToString()
	{
		return DisplayText;
	}

	public bool Equals(CompletionItem other)
	{
		return this == other;
	}

	public override bool Equals(object obj)
	{
		CompletionItem completionItem = obj as CompletionItem;
		if (completionItem != null)
		{
			return Equals(completionItem);
		}
		return false;
	}

	public static bool operator ==(CompletionItem? left, CompletionItem? right)
	{
		if ((object)left == null && (object)right == null)
		{
			return true;
		}
		if ((object)left == null)
		{
			return false;
		}
		if ((object)right == null)
		{
			return false;
		}
		if (string.Compare(left.DisplayText, right.DisplayText, StringComparison.OrdinalIgnoreCase) == 0 && string.Compare(left.FilterText, right.FilterText, StringComparison.OrdinalIgnoreCase) == 0 && string.Compare(left.SortText, right.SortText, StringComparison.OrdinalIgnoreCase) == 0 && string.Compare(left.InsertionText, right.InsertionText, StringComparison.OrdinalIgnoreCase) == 0 && left.Rules == right.Rules && left.Tags.Equals(right.Tags) && left.Properties.Equals(right.Properties))
		{
			return left.Span == right.Span;
		}
		return false;
	}

	public static bool operator !=(CompletionItem? completionItem1, CompletionItem? completionItem2)
	{
		return !(completionItem1 == completionItem2);
	}

	public static bool operator >(CompletionItem completionItem1, CompletionItem completionItem2)
	{
		return completionItem1.CompareTo(completionItem2) > 0;
	}

	public static bool operator <(CompletionItem completionItem1, CompletionItem completionItem2)
	{
		return completionItem1.CompareTo(completionItem2) < 0;
	}

	public override int GetHashCode()
	{
		return DisplayText.GetHashCode() ^ FilterText.GetHashCode() ^ InsertionText.GetHashCode() ^ Properties.GetHashCode() ^ Rules.GetHashCode() ^ SortText.GetHashCode() ^ Span.GetHashCode() ^ Tags.GetHashCode();
	}
}
