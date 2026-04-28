using System.Collections.Immutable;
using System.Linq;
using System.Threading;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Completion;

public sealed class CompletionDescription
{
	public static readonly CompletionDescription Empty = new CompletionDescription(ImmutableArray<TaggedText>.Empty);

	private string text;

	public ImmutableArray<TaggedText> TaggedParts { get; }

	public string Text
	{
		get
		{
			if (text == null)
			{
				Interlocked.CompareExchange(ref text, string.Concat(TaggedParts.Select((TaggedText p) => p.Text)), null);
			}
			return text;
		}
	}

	private CompletionDescription(ImmutableArray<TaggedText> taggedParts)
	{
		TaggedParts = (taggedParts.IsDefault ? ImmutableArray<TaggedText>.Empty : taggedParts);
	}

	public static CompletionDescription Create(ImmutableArray<TaggedText> taggedParts)
	{
		return new CompletionDescription(taggedParts);
	}

	public static CompletionDescription FromText(string text)
	{
		return new CompletionDescription(ImmutableArray.Create(new TaggedText("Text", text)));
	}

	public CompletionDescription WithTaggedParts(ImmutableArray<TaggedText> taggedParts)
	{
		if (taggedParts != TaggedParts)
		{
			return new CompletionDescription(taggedParts);
		}
		return this;
	}
}
