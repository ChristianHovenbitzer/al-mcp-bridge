using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.CodeAnalysis.CodeActions;
using Microsoft.Dynamics.Nav.CodeAnalysis.CodeActions.Mef;
using Microsoft.Dynamics.Nav.CodeAnalysis.CodeFixes;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Completion;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.LanguageServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Options;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Shared.Extensions;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.SpellCheck;

[CodeFixProvider("SpellCheck")]
public class SpellCheckCodeFixProvider : CodeFixProvider
{
	private class SpellCheckCodeAction : CodeAction.DocumentChangeAction
	{
		public override CodeActionKind Kind => CodeActionKind.QuickFix;

		public SpellCheckCodeAction(string title, Func<CancellationToken, Task<Document>> createChangedDocument, string equivalenceKey)
			: base(title, createChangedDocument, equivalenceKey)
		{
		}
	}

	private class MyCodeAction : CodeAction.CodeActionWithNestedActions
	{
		public MyCodeAction(string title, ImmutableArray<CodeAction> nestedActions)
			: base(title, nestedActions)
		{
		}
	}

	private static readonly int[] fixableErrors = new int[7] { 104, 105, 106, 107, 118, 169, 185 };

	private const int MinTokenLength = 3;

	public override ImmutableArray<string> FixableDiagnosticIds { get; } = fixableErrors.Select((int t) => MessageProvider.Instance.GetIdForErrorCode(t)).ToImmutableArray();


	private SyntaxToken CreateIdentifier(SyntaxToken nameToken, string newName)
	{
		return SyntaxFactory.IdentifierNoQuotes(newName).WithTriviaFrom(nameToken);
	}

	public override async Task RegisterCodeFixesAsync(CodeFixContext context)
	{
		Document document = context.Document;
		TextSpan span = context.Span;
		CancellationToken cancellationToken = context.CancellationToken;
		SyntaxNode syntaxNode = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		SyntaxNode syntaxNode2 = syntaxNode.FindNode(span);
		if (syntaxNode2 != null && syntaxNode2.Span == span)
		{
			await CheckNodeAsync(context, document, syntaxNode2, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
			return;
		}
		SyntaxToken token = syntaxNode.FindToken(span.Start);
		if (token.Span == span)
		{
			await CheckTokenAsync(context, document, token, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		}
	}

	private async Task CheckNodeAsync(CodeFixContext context, Document document, SyntaxNode node, CancellationToken cancellationToken)
	{
		SemanticModel semanticModel = null;
		foreach (SimpleNameSyntax name in node.DescendantNodesAndSelf(DescendIntoChildren).OfType<SimpleNameSyntax>())
		{
			SyntaxToken token = name.GetFirstToken();
			string? valueText = token.ValueText;
			if (valueText != null && valueText.Length >= 3)
			{
				SemanticModel semanticModel2 = semanticModel;
				if (semanticModel2 == null)
				{
					semanticModel2 = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
				}
				semanticModel = semanticModel2;
				if (semanticModel.GetSymbolInfo(name, cancellationToken).Symbol == null)
				{
					await CreateSpellCheckCodeIssueAsync(context, token, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
				}
			}
		}
	}

	private async Task CheckTokenAsync(CodeFixContext context, Document document, SyntaxToken token, CancellationToken cancellationToken)
	{
		if (document.GetLanguageService<ISyntaxFactsService>().IsWord(token))
		{
			string valueText = token.ValueText;
			if (!IsALKeyword(valueText) && valueText != null && valueText.Length >= 3)
			{
				await CreateSpellCheckCodeIssueAsync(context, token, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
			}
		}
	}

	private static bool IsALKeyword(string nameText)
	{
		return SyntaxFacts.GetALKeywordKind(nameText.ToUpper()) != SyntaxKind.None;
	}

	private bool DescendIntoChildren(SyntaxNode arg)
	{
		return true;
	}

	private async Task CreateSpellCheckCodeIssueAsync(CodeFixContext context, SyntaxToken nameToken, CancellationToken cancellationToken)
	{
		Document document = context.Document;
		CompletionService service = CompletionService.GetService(document);
		OptionSet optionSet = document.Project.Solution.Workspace.Options;
		if (optionSet != null)
		{
			optionSet = optionSet.WithChangedOption(CompletionOptions.SerializeCompletionResult, value: true);
		}
		int spanStart = nameToken.SpanStart;
		OptionSet options = optionSet;
		CompletionList completionList = await service.GetCompletionsAsync(document, spanStart, default(CompletionTrigger), null, options, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		if (completionList == null)
		{
			return;
		}
		string valueText = nameToken.ValueText;
		WordSimilarityChecker similarityChecker = WordSimilarityChecker.Allocate(valueText, substringsAreSimilar: true);
		try
		{
			await CheckItemsAsync(context, nameToken, completionList, similarityChecker).ConfigureAwait(continueOnCapturedContext: false);
		}
		finally
		{
			similarityChecker.Free();
		}
	}

	private async Task CheckItemsAsync(CodeFixContext context, SyntaxToken nameToken, CompletionList completionList, WordSimilarityChecker similarityChecker)
	{
		Document document = context.Document;
		CancellationToken cancellationToken = context.CancellationToken;
		MultiDictionary<double, string> results = new MultiDictionary<double, string>();
		ImmutableArray<CompletionItem>.Enumerator enumerator = completionList.Items.GetEnumerator();
		while (enumerator.MoveNext())
		{
			CompletionItem current = enumerator.Current;
			string filterText = current.FilterText;
			if (similarityChecker.AreSimilar(filterText, out var matchCost))
			{
				string v = await GetInsertionTextAsync(document, current, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
				results.Add(matchCost, v);
			}
		}
		string nameText = nameToken.ValueText;
		ImmutableArray<CodeAction> immutableArray = ((IEnumerable<CodeAction>?)(from newName in (from t in results.OrderBy<KeyValuePair<double, MultiDictionary<double, string>.ValueSet>, double>((KeyValuePair<double, MultiDictionary<double, string>.ValueSet> kvp) => kvp.Key).SelectMany((KeyValuePair<double, MultiDictionary<double, string>.ValueSet> kvp) => kvp.Value.Order())
				where t != nameText
				select t).Take(3)
			select CreateCodeAction(nameToken, nameText, newName, document))).ToImmutableArrayOrEmpty();
		if (immutableArray.Length > 1)
		{
			context.RegisterCodeFix(new MyCodeAction(string.Format(WorkspacesResources.FixTypo, nameText), immutableArray), context.Diagnostics);
		}
		else
		{
			context.RegisterFixes(immutableArray, context.Diagnostics);
		}
	}

	private async Task<string> GetInsertionTextAsync(Document document, CompletionItem item, CancellationToken cancellationToken)
	{
		return (await CompletionService.GetService(document).GetChangeAsync(document, item, null, cancellationToken).ConfigureAwait(continueOnCapturedContext: false)).TextChange.NewText;
	}

	private SpellCheckCodeAction CreateCodeAction(SyntaxToken nameToken, string oldName, string newName, Document document)
	{
		Document document2 = document;
		string newName2 = newName;
		return new SpellCheckCodeAction(string.Format(CultureInfo.CurrentCulture, WorkspacesResources.ChangeTo, oldName, newName2), (CancellationToken c) => Update(document2, nameToken, newName2, c), newName2);
	}

	private async Task<Document> Update(Document document, SyntaxToken nameToken, string newName, CancellationToken cancellationToken)
	{
		SyntaxNode root = (await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false)).ReplaceToken(nameToken, CreateIdentifier(nameToken, newName));
		return document.WithSyntaxRoot(root);
	}
}
