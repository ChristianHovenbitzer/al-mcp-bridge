using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.CodeAnalysis.CodeActions;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Formatting;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.CodeFixes;

internal sealed class BatchFixAllProvider : FixAllProvider
{
	public static readonly FixAllProvider Instance = new BatchFixAllProvider();

	private BatchFixAllProvider()
	{
	}

	public override IEnumerable<FixAllScope> GetSupportedFixAllScopes()
	{
		return ImmutableArray.Create(FixAllScope.Document, FixAllScope.Project, FixAllScope.Workspace);
	}

	public override Task<CodeAction?> GetFixAsync(FixAllContext fixAllContext)
	{
		return DefaultFixAllProviderHelpers.GetFixAsync(FixAllContextHelper.GetDefaultFixAllTitle(fixAllContext.Scope, fixAllContext.DiagnosticIds.FirstOrDefault()), fixAllContext, FixAllContextsAsync);
	}

	private async Task<Solution?> FixAllContextsAsync(FixAllContext originalFixAllContext, ImmutableArray<FixAllContext> fixAllContexts)
	{
		CancellationToken cancellationToken = originalFixAllContext.CancellationToken;
		Dictionary<DocumentId, TextChangeMerger> docIdToTextMerger = new Dictionary<DocumentId, TextChangeMerger>();
		ImmutableArray<FixAllContext>.Enumerator enumerator = fixAllContexts.GetEnumerator();
		while (enumerator.MoveNext())
		{
			FixAllContext current = enumerator.Current;
			Contract.ThrowIfFalse(current.Scope == FixAllScope.Document || current.Scope == FixAllScope.Project);
			await FixSingleContextAsync(current, docIdToTextMerger).ConfigureAwait(continueOnCapturedContext: false);
		}
		if (docIdToTextMerger.Count == 0)
		{
			return null;
		}
		Solution solution = originalFixAllContext.Solution;
		foreach (IGrouping<ProjectId, KeyValuePair<DocumentId, TextChangeMerger>> item in from kvp in docIdToTextMerger
			group kvp by kvp.Key.ProjectId)
		{
			solution = await ApplyChangesAsync(solution, item.SelectAsArray((KeyValuePair<DocumentId, TextChangeMerger> kvp) => (Key: kvp.Key, Value: kvp.Value)), cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		}
		return solution;
	}

	private static async Task FixSingleContextAsync(FixAllContext fixAllContext, Dictionary<DocumentId, TextChangeMerger> docIdToTextMerger)
	{
		await AddDocumentChangesAsync(fixAllContext, docIdToTextMerger, await DetermineDiagnosticsAsync(fixAllContext).ConfigureAwait(continueOnCapturedContext: false)).ConfigureAwait(continueOnCapturedContext: false);
	}

	private static async Task<ImmutableDictionary<Document, ImmutableArray<Diagnostic>>> DetermineDiagnosticsAsync(FixAllContext fixAllContext)
	{
		FixAllContext fixAllContext2 = fixAllContext;
		return (await fixAllContext2.GetDocumentDiagnosticsToFixAsync().ConfigureAwait(continueOnCapturedContext: false)).Where(delegate(KeyValuePair<Document, ImmutableArray<Diagnostic>> kvp)
		{
			if (kvp.Key.Project != fixAllContext2.Project)
			{
				return false;
			}
			return (IsProjectOrWorkspaceScope(fixAllContext2.Scope) || fixAllContext2.Document == null || fixAllContext2.Document == kvp.Key) ? true : false;
		}).ToImmutableDictionary();
	}

	private static bool IsProjectOrWorkspaceScope(FixAllScope scope)
	{
		if (scope != FixAllScope.Project)
		{
			return scope == FixAllScope.Workspace;
		}
		return true;
	}

	private static async Task AddDocumentChangesAsync(FixAllContext fixAllContext, Dictionary<DocumentId, TextChangeMerger> docIdToTextMerger, ImmutableDictionary<Document, ImmutableArray<Diagnostic>> documentToDiagnostics)
	{
		ImmutableArray<Diagnostic> orderedDiagnostics = (from d in documentToDiagnostics.SelectMany<KeyValuePair<Document, ImmutableArray<Diagnostic>>, Diagnostic>((KeyValuePair<Document, ImmutableArray<Diagnostic>> kvp) => kvp.Value)
			where d.Location.IsInSource
			orderby d.Location.SourceTree.FilePath, d.Location.SourceSpan.Start
			select d).ToImmutableArray();
		await MergeTextChangesAsync(fixAllContext, await GetAllChangedDocumentsInDiagnosticsOrderAsync(fixAllContext, orderedDiagnostics).ConfigureAwait(continueOnCapturedContext: false), docIdToTextMerger).ConfigureAwait(continueOnCapturedContext: false);
	}

	private static async Task<ImmutableArray<Document>> GetAllChangedDocumentsInDiagnosticsOrderAsync(FixAllContext fixAllContext, ImmutableArray<Diagnostic> orderedDiagnostics)
	{
		FixAllContext fixAllContext2 = fixAllContext;
		Solution solution = fixAllContext2.Solution;
		CancellationToken cancellationToken = fixAllContext2.CancellationToken;
		ArrayBuilder<Document> result = ArrayBuilder<Document>.GetInstance();
		try
		{
			TaskExecutor.GetSuitableInstance(orderedDiagnostics.Length).ForEach(orderedDiagnostics, async delegate(Diagnostic diagnostic)
			{
				Document requiredDocument = solution.GetRequiredDocument(diagnostic.Location.SourceTree);
				ArrayBuilder<CodeAction> codeActions = ArrayBuilder<CodeAction>.GetInstance();
				ArrayBuilder<Document> changedDocuments = ArrayBuilder<Document>.GetInstance();
				try
				{
					Action<CodeAction, ImmutableArray<Diagnostic>> registerCodeFixAction = GetRegisterCodeFixAction(fixAllContext2.CodeActionEquivalenceKey, codeActions);
					CodeFixContext context = new CodeFixContext(requiredDocument, diagnostic.Location.SourceSpan, ImmutableArray.Create(diagnostic), registerCodeFixAction, cancellationToken);
					await (((CodeFixProvider)fixAllContext2.CodeFixProvider).RegisterCodeFixesAsync(context) ?? Task.CompletedTask).ConfigureAwait(continueOnCapturedContext: false);
					foreach (CodeAction item in codeActions)
					{
						Solution changedSolution = await item.GetChangedSolutionInternalAsync(postProcessChanges: true, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
						if (changedSolution != null)
						{
							IEnumerable<DocumentId> source = new SolutionChanges(changedSolution, solution).GetProjectChanges().SelectMany((ProjectChanges p) => p.GetChangedDocuments());
							changedDocuments.AddRange(source.Select((DocumentId id) => changedSolution.GetRequiredDocument(id)));
						}
					}
					result.AddRange(changedDocuments.ToImmutable());
				}
				finally
				{
					codeActions.Free();
					changedDocuments.Free();
				}
			}, cancellationToken);
			return result.ToImmutable();
		}
		finally
		{
			result.Free();
		}
	}

	private static async Task MergeTextChangesAsync(FixAllContext fixAllContext, ImmutableArray<Document> allChangedDocumentsInDiagnosticsOrder, Dictionary<DocumentId, TextChangeMerger> docIdToTextMerger)
	{
		CancellationToken cancellationToken = fixAllContext.CancellationToken;
		ArrayBuilder<Task> tasks = ArrayBuilder<Task>.GetInstance();
		try
		{
			foreach (IGrouping<DocumentId, Document> item in from d in allChangedDocumentsInDiagnosticsOrder
				group d by d.Id)
			{
				DocumentId key = item.Key;
				ImmutableArray<Document> allDocChanges = item.ToImmutableArray();
				if (!docIdToTextMerger.TryGetValue(key, out TextChangeMerger textMerger))
				{
					Document requiredDocument = fixAllContext.Solution.GetRequiredDocument(key);
					textMerger = new TextChangeMerger(requiredDocument);
					docIdToTextMerger.Add(key, textMerger);
				}
				tasks.Add(Task.Run(async delegate
				{
					await textMerger.TryMergeChangesAsync(allDocChanges, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
				}, cancellationToken));
			}
			await Task.WhenAll(tasks).ConfigureAwait(continueOnCapturedContext: false);
		}
		finally
		{
			tasks.Free();
		}
	}

	private static Action<CodeAction, ImmutableArray<Diagnostic>> GetRegisterCodeFixAction(string? codeActionEquivalenceKey, ArrayBuilder<CodeAction> codeActions)
	{
		string codeActionEquivalenceKey2 = codeActionEquivalenceKey;
		ArrayBuilder<CodeAction> codeActions2 = codeActions;
		return delegate(CodeAction action, ImmutableArray<Diagnostic> diagnostics)
		{
			ArrayBuilder<CodeAction> instance = ArrayBuilder<CodeAction>.GetInstance();
			try
			{
				instance.Push(action);
				while (instance.Count > 0)
				{
					CodeAction codeAction = instance.Pop();
					if (codeAction != null)
					{
						string equivalenceKey = codeAction.EquivalenceKey;
						if (codeActionEquivalenceKey2 == equivalenceKey)
						{
							lock (codeActions2)
							{
								codeActions2.Add(codeAction);
							}
						}
					}
					ImmutableArray<CodeAction>.Enumerator enumerator = codeAction.NestedCodeActions.GetEnumerator();
					while (enumerator.MoveNext())
					{
						CodeAction current = enumerator.Current;
						instance.Push(current);
					}
				}
			}
			finally
			{
				instance.Free();
			}
		};
	}

	private static async Task<Solution> ApplyChangesAsync(Solution currentSolution, ImmutableArray<(DocumentId, TextChangeMerger)> docIdsAndMerger, CancellationToken cancellationToken)
	{
		ImmutableArray<(DocumentId, TextChangeMerger)>.Enumerator enumerator = docIdsAndMerger.GetEnumerator();
		while (enumerator.MoveNext())
		{
			(DocumentId, TextChangeMerger) current = enumerator.Current;
			DocumentId documentId = current.Item1;
			currentSolution = currentSolution.WithDocumentText(documentId, await current.Item2.GetFinalMergedTextAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false));
		}
		return currentSolution;
	}
}
