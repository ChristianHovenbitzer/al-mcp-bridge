using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Completion;

internal abstract class AbstractDocCommentCompletionProvider<TSyntax> : CompletionProvider where TSyntax : SyntaxNode
{
	private static readonly ImmutableArray<string> s_listTagNames = ImmutableArray.Create("listheader", "term", "item", "description");

	private static readonly ImmutableArray<string> s_listHeaderTagNames = ImmutableArray.Create("term", "description");

	private static readonly ImmutableArray<string> s_nestedTagNames = ImmutableArray.Create("c", "code", "para", "list");

	private static readonly ImmutableArray<string> s_topLevelSingleUseTagNames = ImmutableArray.Create("summary", "remarks", "example");

	private static readonly Dictionary<string, (string tagOpen, string textBeforeCaret, string textAfterCaret, string? tagClose)> s_tagMap = new Dictionary<string, (string, string, string, string)>
	{
		{
			"see",
			("<see", " cref=\"", "\"", "/>")
		},
		{
			"seealso",
			("<seealso", " cref=\"", "\"", "/>")
		},
		{
			"list",
			("<list", " type=\"", "\"", null)
		},
		{
			"paramref",
			("<paramref", " name=\"", "\"", "/>")
		}
	};

	private static readonly string[][] s_attributeMap = new string[6][]
	{
		new string[4] { "see", "cref", "cref=\"", "\"" },
		new string[4] { "see", "langword", "langword=\"", "\"" },
		new string[4] { "seealso", "cref", "cref=\"", "\"" },
		new string[4] { "list", "type", "type=\"", "\"" },
		new string[4] { "param", "name", "name=\"", "\"" },
		new string[4] { "paramref", "name", "name=\"", "\"" }
	};

	private static readonly ImmutableArray<string> s_listTypeValues = ImmutableArray.Create("bullet", "number", "table");

	private readonly CompletionItemRules defaultRules;

	private static readonly CharacterSetModificationRule WithoutQuoteRule = CharacterSetModificationRule.Create(CharacterSetModificationKind.Remove, '"');

	private static readonly CharacterSetModificationRule WithoutSpaceRule = CharacterSetModificationRule.Create(CharacterSetModificationKind.Remove, ' ');

	internal static readonly ImmutableArray<CharacterSetModificationRule> FilterRules = ImmutableArray.Create(CharacterSetModificationRule.Create(CharacterSetModificationKind.Add, '!', '-', '['));

	protected AbstractDocCommentCompletionProvider(CompletionItemRules defaultRules)
	{
		this.defaultRules = defaultRules ?? throw new ArgumentNullException("defaultRules");
	}

	public override async Task ProvideCompletionsAsync(CompletionContext context, AbstractSyntaxContext memberSyntaxContext)
	{
		if (context.Options.GetOption(CompletionOptions.ShowXmlDocCommentCompletion))
		{
			IEnumerable<CompletionItem> enumerable = await GetItemsWorkerAsync(context.Document, context.Position, context.Trigger, context.CancellationToken).ConfigureAwait(continueOnCapturedContext: false);
			if (enumerable != null)
			{
				context.AddItems(enumerable);
			}
		}
	}

	protected abstract Task<IEnumerable<CompletionItem>?> GetItemsWorkerAsync(Document document, int position, CompletionTrigger trigger, CancellationToken cancellationToken);

	protected abstract IEnumerable<string> GetExistingTopLevelElementNames(TSyntax syntax);

	protected abstract IEnumerable<string> GetExistingTopLevelAttributeValues(TSyntax syntax, string tagName, string attributeName);

	private CompletionItem GetItem(string name, char triggerChar)
	{
		if (s_tagMap.TryGetValue(name, out (string, string, string, string) value))
		{
			if (triggerChar == '<')
			{
				value.Item1 = value.Item1.Substring(1);
			}
			return CreateCompletionItem(name, value.Item1 + value.Item2, value.Item3 + value.Item4);
		}
		string text = ((triggerChar == '<') ? "" : "<");
		return CreateCompletionItem(name, text + name + ">", "</" + name + ">");
	}

	protected IEnumerable<CompletionItem> GetAttributeItems(string? tagName, ISet<string> existingAttributes)
	{
		string tagName2 = tagName;
		ISet<string> existingAttributes2 = existingAttributes;
		return from x in s_attributeMap
			where x[0] == tagName2 && !existingAttributes2.Contains(x[1])
			select CreateCompletionItem(x[1], x[2], x[3]);
	}

	protected IEnumerable<CompletionItem> GetAlwaysVisibleItems(char triggerChar)
	{
		return new CompletionItem[4]
		{
			GetCDataItem(triggerChar),
			GetCommentItem(triggerChar),
			GetItem("see", triggerChar),
			GetItem("seealso", triggerChar)
		};
	}

	private CompletionItem GetCommentItem(char triggerChar)
	{
		return CreateCompletionItem("!--", "!--", "-->");
	}

	private CompletionItem GetCDataItem(char triggerChar)
	{
		return CreateCompletionItem("![CDATA[", "![CDATA[", "]]>");
	}

	protected IEnumerable<CompletionItem> GetNestedItems(ISymbol? symbol, bool includeKeywords, char triggerChar)
	{
		IEnumerable<CompletionItem> enumerable = s_nestedTagNames.Select((string n) => GetItem(n, triggerChar));
		if (symbol != null)
		{
			enumerable = enumerable.Concat(GetParamRefItems(symbol, triggerChar));
		}
		if (includeKeywords)
		{
			enumerable = enumerable.Concat(GetKeywordNames().Select(CreateLangwordCompletionItem));
		}
		return enumerable;
	}

	private IEnumerable<CompletionItem> GetParamRefItems(ISymbol symbol, char triggerChar)
	{
		return from p in symbol.GetParameters()
			select p.Name into p
			select CreateCompletionItem(FormatParameter("paramref", p), FormatParameterRefTag("paramref", p, triggerChar), string.Empty);
	}

	protected IEnumerable<CompletionItem> GetAttributeValueItems(ISymbol? symbol, string? tagName, string? attributeName)
	{
		if (attributeName == "name" && symbol != null)
		{
			if (tagName == "param" || tagName == "paramref")
			{
				return from parameter in symbol.GetParameters()
					select CreateCompletionItem(parameter.Name);
			}
		}
		else
		{
			if (attributeName == "langword" && tagName == "see")
			{
				return GetKeywordNames().Select(CreateCompletionItem);
			}
			if (attributeName == "type" && tagName == "list")
			{
				return s_listTypeValues.Select(CreateCompletionItem);
			}
		}
		return SpecializedCollections.EmptyEnumerable<CompletionItem>();
	}

	protected abstract IEnumerable<string> GetKeywordNames();

	protected IEnumerable<CompletionItem> GetTopLevelItems(ISymbol? symbol, TSyntax syntax, char triggerChar)
	{
		List<CompletionItem> list = new List<CompletionItem>();
		HashSet<string> hashSet = new HashSet<string>(GetExistingTopLevelElementNames(syntax));
		list.AddRange(from n in Enumerable.Except(s_topLevelSingleUseTagNames, hashSet)
			select GetItem(n, triggerChar));
		if (symbol != null)
		{
			list.AddRange(GetParameterItems(symbol.GetParameters(), syntax, "param", triggerChar));
			if (symbol is IMethodSymbol { ReturnValueSymbol: not null } && !hashSet.Contains("returns"))
			{
				list.Add(GetItem("returns", triggerChar));
			}
		}
		return list;
	}

	protected IEnumerable<CompletionItem> GetItemTagItems(char triggerChar)
	{
		return new string[2] { "term", "description" }.Select((string n) => GetItem(n, triggerChar));
	}

	protected IEnumerable<CompletionItem> GetListItems(char triggerChar)
	{
		return s_listTagNames.Select((string n) => GetItem(n, triggerChar));
	}

	protected IEnumerable<CompletionItem> GetListHeaderItems(char triggerChar)
	{
		return s_listHeaderTagNames.Select((string n) => GetItem(n, triggerChar));
	}

	private IEnumerable<CompletionItem> GetParameterItems<TSymbol>(ImmutableArray<TSymbol> symbols, TSyntax syntax, string tagName, char triggerChar) where TSymbol : ISymbol
	{
		string tagName2 = tagName;
		ISet<string> set = symbols.Select((TSymbol p) => p.Name).ToSet();
		set.RemoveAll(GetExistingTopLevelAttributeValues(syntax, tagName2, "name").WhereNotNull());
		string prefix = ((triggerChar == '<') ? "" : "<");
		return set.Select((string name) => CreateCompletionItem($"{tagName2} {"name"}=\"{name}\"", $"{prefix}{tagName2} {"name"}=\"{name}\">", "</" + tagName2 + ">"));
	}

	private string FormatParameter(string kind, string name)
	{
		return $"{kind} {"name"}=\"{name}\"";
	}

	private string FormatParameterRefTag(string kind, string name, char triggerChar)
	{
		string value = ((triggerChar == '<') ? "" : "<");
		return $"{value}{kind} {"name"}=\"{name}\"/>";
	}

	public override async Task<CompletionChange> GetChangeAsync(Document document, CompletionItem item, char? commitChar = null, CancellationToken cancellationToken = default(CancellationToken))
	{
		bool includesCommitCharacter = true;
		if (commitChar == ' ' && XmlDocCommentCompletionItem.TryGetInsertionTextOnSpace(item, out string beforeCaretText, out string afterCaretText))
		{
			includesCommitCharacter = false;
		}
		else
		{
			beforeCaretText = XmlDocCommentCompletionItem.GetBeforeCaretText(item);
			afterCaretText = XmlDocCommentCompletionItem.GetAfterCaretText(item);
		}
		SourceText obj = await document.GetTextAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		TextSpan span = item.Span;
		TextSpan span2 = TextSpan.FromBounds((obj[span.Start - 1] == '<' && beforeCaretText[0] == '<') ? (span.Start - 1) : span.Start, span.End);
		string text = beforeCaretText;
		int num = span2.Start + beforeCaretText.Length;
		if (commitChar.HasValue && !char.IsWhiteSpace(commitChar.Value) && commitChar.Value != text[text.Length - 1])
		{
			ReadOnlySpan<char> readOnlySpan = text;
			char reference = commitChar.Value;
			text = string.Concat(readOnlySpan, new ReadOnlySpan<char>(ref reference));
			num++;
		}
		text += afterCaretText;
		return CompletionChange.Create(new TextChange(span2, text), num, includesCommitCharacter);
	}

	private CompletionItem CreateCompletionItem(string displayText)
	{
		return CreateCompletionItem(displayText, displayText, string.Empty);
	}

	private CompletionItem CreateLangwordCompletionItem(string displayText)
	{
		return CreateCompletionItem(displayText, "<see langword=\"" + displayText + "\"/>", string.Empty);
	}

	protected CompletionItem CreateCompletionItem(string displayText, string beforeCaretText, string afterCaretText, string? beforeCaretTextOnSpace = null, string? afterCaretTextOnSpace = null)
	{
		return XmlDocCommentCompletionItem.Create(displayText, beforeCaretText, afterCaretText, beforeCaretTextOnSpace, afterCaretTextOnSpace, GetCompletionItemRules(displayText));
	}

	private CompletionItemRules GetCompletionItemRules(string displayText)
	{
		ImmutableArray<CharacterSetModificationRule> commitCharacterRules = defaultRules.CommitCharacterRules;
		if (displayText.Contains("\""))
		{
			commitCharacterRules = commitCharacterRules.Add(WithoutQuoteRule);
		}
		if (displayText.Contains(" "))
		{
			commitCharacterRules = commitCharacterRules.Add(WithoutSpaceRule);
		}
		return defaultRules.WithCommitCharacterRules(commitCharacterRules);
	}
}
