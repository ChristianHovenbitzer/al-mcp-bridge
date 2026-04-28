using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.CodeAnalysis.CodeActions;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.CodeFixes;

internal static class DocumentBasedFixAllProviderHelpers
{
	private class PostProcessCodeAction : CodeAction
	{
		public static readonly PostProcessCodeAction Instance = new PostProcessCodeAction();

		public override string Title => "";

		public new Task<Document> PostProcessChangesAsync(Document document, CancellationToken cancellationToken)
		{
			return base.PostProcessChangesAsync(document, cancellationToken);
		}
	}

	public static async Task<Solution?> FixAllContextsAsync<TFixAllContext>(TFixAllContext originalFixAllContext, ImmutableArray<TFixAllContext> fixAllContexts, string progressTrackerDescription, Func<TFixAllContext, Task<IDictionary<DocumentId, (SyntaxNode? node, SourceText? text)>>> getFixedDocumentsAsync) where TFixAllContext : IFixAllContext
	{
		Solution solution = originalFixAllContext.Solution;
		ImmutableArray<TFixAllContext>.Enumerator enumerator = fixAllContexts.GetEnumerator();
		while (enumerator.MoveNext())
		{
			TFixAllContext current = enumerator.Current;
			Contract.ThrowIfFalse(current.Scope == FixAllScope.Document || current.Scope == FixAllScope.Project || current.Scope == FixAllScope.Workspace);
			solution = await FixSingleContextAsync(solution, current, getFixedDocumentsAsync).ConfigureAwait(continueOnCapturedContext: false);
		}
		return solution;
	}

	private static async Task<Solution> FixSingleContextAsync<TFixAllContext>(Solution currentSolution, TFixAllContext fixAllContext, Func<TFixAllContext, Task<IDictionary<DocumentId, (SyntaxNode? node, SourceText? text)>>> getFixedDocumentsAsync) where TFixAllContext : IFixAllContext
	{
		currentSolution = await CleanupAndApplyChangesAsync(currentSolution, await getFixedDocumentsAsync(fixAllContext).ConfigureAwait(continueOnCapturedContext: false), fixAllContext.CancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		return currentSolution;
	}

	private static async Task<Solution> CleanupAndApplyChangesAsync(Solution currentSolution, IDictionary<DocumentId, (SyntaxNode? node, SourceText? text)> docIdToNewRootOrText, CancellationToken cancellationToken)
	{
		if (docIdToNewRootOrText.Count > 0)
		{
			DocumentId key;
			(SyntaxNode, SourceText) value;
			foreach (KeyValuePair<DocumentId, (SyntaxNode, SourceText)> item6 in docIdToNewRootOrText)
			{
				item6.Deconstruct(out key, out value);
				(SyntaxNode, SourceText) tuple = value;
				DocumentId documentId = key;
				SyntaxNode item = tuple.Item1;
				SourceText item2 = tuple.Item2;
				currentSolution = ((item != null) ? currentSolution.WithDocumentSyntaxRoot(documentId, item) : currentSolution.WithDocumentText(documentId, item2));
			}
			ArrayBuilder<Task<(DocumentId docId, SourceText sourceText)>> tasks = ArrayBuilder<Task<(DocumentId, SourceText)>>.GetInstance();
			try
			{
				foreach (KeyValuePair<DocumentId, (SyntaxNode, SourceText)> item7 in docIdToNewRootOrText)
				{
					item7.Deconstruct(out key, out value);
					(SyntaxNode, SourceText) tuple2 = value;
					DocumentId documentId2 = key;
					if (tuple2.Item1 != null)
					{
						Document dirtyDocument = currentSolution.GetRequiredDocument(documentId2);
						tasks.Add(Task.Run(async delegate
						{
							SourceText item5 = await (await PostProcessCodeAction.Instance.PostProcessChangesAsync(dirtyDocument, cancellationToken).ConfigureAwait(continueOnCapturedContext: false)).GetTextAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
							return (Id: dirtyDocument.Id, cleanedText: item5);
						}, cancellationToken));
					}
				}
				await Task.WhenAll(tasks).ConfigureAwait(continueOnCapturedContext: false);
				foreach (Task<(DocumentId, SourceText)> item8 in tasks)
				{
					(DocumentId, SourceText) obj = await item8.ConfigureAwait(continueOnCapturedContext: false);
					DocumentId item3 = obj.Item1;
					SourceText item4 = obj.Item2;
					currentSolution = currentSolution.WithDocumentText(item3, item4);
				}
			}
			finally
			{
				tasks.Free();
			}
		}
		return currentSolution;
	}
}
