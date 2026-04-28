using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Options;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Completion;

public sealed class CompletionContext
{
	private readonly List<CompletionItem> items;

	private CompletionItem suggestionModeItem;

	internal IReadOnlyList<CompletionItem> Items => items;

	internal CompletionProvider Provider { get; }

	public Document Document { get; }

	public int Position { get; }

	public TextSpan DefaultItemSpan { get; }

	public CompletionTrigger Trigger { get; }

	public OptionSet Options { get; }

	public CancellationToken CancellationToken { get; }

	public bool IsExclusive { get; set; }

	public CompletionItem SuggestionModeItem
	{
		get
		{
			return suggestionModeItem;
		}
		set
		{
			suggestionModeItem = value;
			if (suggestionModeItem != null)
			{
				suggestionModeItem = FixItem(suggestionModeItem);
			}
		}
	}

	public CompletionContext(CompletionProvider provider, Document document, int position, TextSpan defaultSpan, CompletionTrigger trigger, OptionSet options, CancellationToken cancellationToken)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		if (document == null)
		{
			throw new ArgumentNullException("document");
		}
		if (options == null)
		{
			throw new ArgumentNullException("options");
		}
		Provider = provider;
		Document = document;
		Position = position;
		DefaultItemSpan = defaultSpan;
		Trigger = trigger;
		Options = options;
		CancellationToken = cancellationToken;
		items = new List<CompletionItem>();
	}

	public void AddItem(CompletionItem item)
	{
		if (item == null)
		{
			throw new ArgumentNullException("item");
		}
		item = FixItem(item);
		items.Add(item);
	}

	public void AddItems(IEnumerable<CompletionItem> newItems)
	{
		if (newItems == null)
		{
			throw new ArgumentNullException("newItems");
		}
		foreach (CompletionItem newItem in newItems)
		{
			AddItem(newItem);
		}
	}

	private CompletionItem FixItem(CompletionItem item)
	{
		item = item.AddProperty("Provider", Provider.Name);
		if (item.Span == default(TextSpan) && DefaultItemSpan != default(TextSpan))
		{
			item = item.WithSpan(DefaultItemSpan);
		}
		return item;
	}
}
