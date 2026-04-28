using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Completion;

internal static class SymbolCompletionItem
{
	private const char SymbolSplitter = '|';

	private static readonly char[] SymbolSplitters = new char[1] { '|' };

	private static readonly char[] ProjectSeperators = new char[1] { ';' };

	public static CompletionItem Create(string displayText, TextSpan span, IReadOnlyList<ISymbol> symbols, int contextPosition = -1, int descriptionPosition = -1, string? sortText = null, string? insertionText = null, Glyph? glyph = null, string? filterText = null, bool preselect = false, SupportedPlatformData? supportedPlatforms = null, bool isArgumentName = false, ImmutableDictionary<string, string>? properties = null, ImmutableArray<string> tags = default(ImmutableArray<string>), CompletionItemRules? rules = null, bool isSnippet = false, bool shouldSerializeItem = false, string? obsoleteInformation = null, string? description = null, bool addUsingStatementWhenCompleting = true)
	{
		ImmutableDictionary<string, string> immutableDictionary = properties ?? ImmutableDictionary<string, string>.Empty;
		if (shouldSerializeItem)
		{
			immutableDictionary = immutableDictionary.Add("Symbols", EncodeSymbols(symbols));
		}
		if (contextPosition >= 0)
		{
			immutableDictionary = immutableDictionary.Add("ContextPosition", contextPosition.ToString(CultureInfo.InvariantCulture));
		}
		if (descriptionPosition >= 0)
		{
			immutableDictionary = immutableDictionary.Add("DescriptionPosition", descriptionPosition.ToString(CultureInfo.InvariantCulture));
		}
		if (addUsingStatementWhenCompleting)
		{
			immutableDictionary = AddNamespaceProperty(symbols, immutableDictionary);
		}
		string text = symbols[0].ToDisplayParts(SymbolDisplayFormat.SimpleHoverFormat).ToDisplayString();
		bool flag = false;
		if (symbols.Count > 1)
		{
			text += $" (+ {symbols.Count - 1} overload(s))";
			flag = symbols.All((ISymbol s) => s.IsObsoletePending || s.IsObsoletePendingMove);
		}
		else
		{
			flag = symbols[0].IsObsoletePending || symbols[0].IsObsoletePendingMove;
		}
		if (flag && symbols.Select((ISymbol s) => ObsoleteSymbolHelper.GetObsoleteInformationMessage(s)).Distinct().Count() > 1)
		{
			obsoleteInformation = null;
		}
		string documentationText = symbols[0].GetDocumentationText(obsoleteInformation, isMarkdownDocs: true);
		string filterText2 = filterText ?? ((displayText.Length > 0 && displayText[0] == '@') ? displayText : symbols[0].Name);
		string sortText2 = sortText ?? symbols[0].Name;
		Glyph? glyph2 = glyph ?? symbols[0].GetGlyph();
		string detailText = text;
		string obsoleteInformation2 = obsoleteInformation;
		bool isArgumentName2 = isArgumentName;
		bool isDeprecated = flag;
		bool showsWarningIcon = supportedPlatforms != null;
		ImmutableDictionary<string, string> properties2 = immutableDictionary;
		return WithSupportedPlatforms(CommonCompletionItem.Create(displayText, span, glyph2, description, documentationText, detailText, sortText2, filterText2, insertionText, obsoleteInformation2, preselect, showsWarningIcon, shouldFormatOnCommit: false, isArgumentName2, isSnippet, isMarkdownDocs: true, isDeprecated, properties2, tags, rules), supportedPlatforms);
	}

	private static ImmutableDictionary<string, string> AddNamespaceProperty(IReadOnlyList<ISymbol> symbols, ImmutableDictionary<string, string> props)
	{
		ISymbol symbol = symbols[0];
		if (symbol.Kind.IsObjectTypeSymbol())
		{
			string value = symbol.ContainingNamespace?.QualifiedName ?? string.Empty;
			props = props.Add("Namespace", value);
		}
		return props;
	}

	public static string EncodeSymbols(IReadOnlyList<ISymbol> symbols)
	{
		if (symbols.Count > 1)
		{
			return string.Join('|'.ToString(), symbols.Select(EncodeSymbol));
		}
		if (symbols.Count == 1)
		{
			return EncodeSymbol(symbols[0]);
		}
		return string.Empty;
	}

	public static string EncodeSymbol(ISymbol symbol)
	{
		return SymbolId.CreateId(symbol);
	}

	public static bool HasSymbols(CompletionItem item)
	{
		return item.Properties.ContainsKey("Symbols");
	}

	public static async Task<ImmutableArray<ISymbol>> GetSymbolsAsync(CompletionItem item, Document document, CancellationToken cancellationToken)
	{
		if (item.Properties.TryGetValue("Symbols", out string value))
		{
			List<string> idList = value.Split(SymbolSplitters, StringSplitOptions.RemoveEmptyEntries).ToList();
			List<ISymbol> symbols = new List<ISymbol>();
			DecodeSymbols(idList, await document.Project.GetCompilationAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false), symbols);
			if (idList.Count > 0)
			{
				ImmutableArray<DocumentId> linkedDocumentIds = document.GetLinkedDocumentIds();
				if (linkedDocumentIds.Length > 0)
				{
					ImmutableArray<DocumentId>.Enumerator enumerator = linkedDocumentIds.GetEnumerator();
					while (enumerator.MoveNext())
					{
						DocumentId current = enumerator.Current;
						DecodeSymbols(idList, await document.Project.Solution.GetDocument(current).Project.GetCompilationAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false), symbols);
					}
				}
			}
			return symbols.ToImmutableArray();
		}
		return ImmutableArray<ISymbol>.Empty;
	}

	private static void DecodeSymbols(List<string> ids, Compilation compilation, List<ISymbol> symbols)
	{
		int num = 0;
		while (num < ids.Count)
		{
			ISymbol symbol = DecodeSymbol(ids[num], compilation);
			if (symbol != null)
			{
				ids.RemoveAt(num);
				symbols.Add(symbol);
			}
			else
			{
				num++;
			}
		}
	}

	private static ISymbol? DecodeSymbol(string id, Compilation compilation)
	{
		return SymbolId.GetFirstSymbolForId(id, compilation);
	}

	public static async Task<CompletionDescription> GetDescriptionAsync(CompletionItem item, Document document, CancellationToken cancellationToken)
	{
		Workspace workspace = document.Project.Solution.Workspace;
		int position = GetDescriptionPosition(item);
		if (position == -1)
		{
			position = item.Span.Start;
		}
		SupportedPlatformData supportedPlatforms = GetSupportedPlatforms(item, workspace);
		Document document2 = document;
		if (supportedPlatforms != null && supportedPlatforms.InvalidProjects.Contains(document.Id.ProjectId))
		{
			DocumentId documentId = document.GetLinkedDocumentIds().FirstOrDefault((DocumentId id) => !supportedPlatforms.InvalidProjects.Contains(id.ProjectId));
			if (documentId != null)
			{
				document2 = document.Project.Solution.GetDocument(documentId);
			}
		}
		SemanticModel semanticModel = await document2.GetSemanticModelAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		ImmutableArray<ISymbol> immutableArray = await GetSymbolsAsync(item, document, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		if (immutableArray.Length > 0)
		{
			return await CommonCompletionUtilities.CreateDescriptionAsync(workspace, semanticModel, position, immutableArray, supportedPlatforms, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		}
		return CompletionDescription.Empty;
	}

	private static CompletionItem WithSupportedPlatforms(CompletionItem completionItem, SupportedPlatformData supportedPlatforms)
	{
		if (supportedPlatforms != null)
		{
			return completionItem.AddProperty("InvalidProjects", string.Join(";", supportedPlatforms.InvalidProjects.Select((ProjectId id) => id.Id))).AddProperty("CandidateProjects", string.Join(";", supportedPlatforms.CandidateProjects.Select((ProjectId id) => id.Id)));
		}
		return completionItem;
	}

	public static SupportedPlatformData GetSupportedPlatforms(CompletionItem item, Workspace workspace)
	{
		if (item.Properties.TryGetValue("InvalidProjects", out string value) && item.Properties.TryGetValue("CandidateProjects", out string value2))
		{
			return new SupportedPlatformData((from s in value.Split(ProjectSeperators)
				select ProjectId.CreateFromSerialized(Guid.Parse(s))).ToList(), (from s in value2.Split(ProjectSeperators)
				select ProjectId.CreateFromSerialized(Guid.Parse(s))).ToList(), workspace);
		}
		return null;
	}

	public static int GetContextPosition(CompletionItem item)
	{
		if (item.Properties.TryGetValue("ContextPosition", out string value) && int.TryParse(value, out var result))
		{
			return result;
		}
		return -1;
	}

	public static int GetDescriptionPosition(CompletionItem item)
	{
		if (item.Properties.TryGetValue("DescriptionPosition", out string value) && int.TryParse(value, out var result))
		{
			return result;
		}
		return -1;
	}

	public static string GetInsertionText(CompletionItem item)
	{
		item.Properties.TryGetValue("InsertionText", out string value);
		return value;
	}
}
