using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.CodeAnalysis.CodeFixes;
using Microsoft.Dynamics.Nav.CodeAnalysis.CodeRefactoring;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.CodeActions;

internal class ALCodeActionService : CodeActionService
{
	private static class CodeActionHelper
	{
		internal static async void GetCodeActions(IEnumerable<CodeFixCollection> codeFixCollections, Action<CodeActionProxy> action)
		{
			foreach (CodeFixCollection codeFixCollection in codeFixCollections)
			{
				ImmutableArray<CodeFix>.Enumerator enumerator2 = codeFixCollection.Fixes.GetEnumerator();
				while (enumerator2.MoveNext())
				{
					GetCodeActions(enumerator2.Current.GetCodeAction(), action);
				}
			}
		}

		internal static void GetCodeActions(ImmutableArray<Microsoft.Dynamics.Nav.CodeAnalysis.CodeRefactoring.CodeRefactoring> codeRefactorings, Action<CodeActionProxy> action)
		{
			ImmutableArray<Microsoft.Dynamics.Nav.CodeAnalysis.CodeRefactoring.CodeRefactoring>.Enumerator enumerator = codeRefactorings.GetEnumerator();
			while (enumerator.MoveNext())
			{
				ImmutableArray<(CodeAction, TextSpan?)>.Enumerator enumerator2 = enumerator.Current.GetCodeActions().GetEnumerator();
				while (enumerator2.MoveNext())
				{
					CodeAction item = enumerator2.Current.Item1;
					GetCodeActions(item, action);
				}
			}
		}

		private static void GetCodeActions(CodeAction codeAction, Action<CodeActionProxy> action)
		{
			if (codeAction.NestedCodeActions.Length == 0)
			{
				action(new CodeActionProxy(codeAction));
			}
			ImmutableArray<CodeAction>.Enumerator enumerator = codeAction.NestedCodeActions.GetEnumerator();
			while (enumerator.MoveNext())
			{
				CodeAction current = enumerator.Current;
				action(new CodeActionProxy(current, codeAction));
			}
		}
	}

	protected readonly ICodeFixService codeFixService;

	protected readonly ICodeRefactoringService codeRefactoringService;

	public ALCodeActionService(ICodeFixService codeFixService, ICodeRefactoringService codeRefactoringService)
	{
		this.codeFixService = codeFixService;
		this.codeRefactoringService = codeRefactoringService;
	}

	public override async Task<ImmutableArray<CodeActionProxy>> GetCodeActionsAsync(Document document, TextSpan textSpan, CancellationToken cancellationToken)
	{
		if (document == null)
		{
			return await Task.FromResult(ImmutableArray<CodeActionProxy>.Empty);
		}
		if (AreCodeActionsDisabledInProject(document.Project))
		{
			return await Task.FromResult(ImmutableArray<CodeActionProxy>.Empty);
		}
		ArrayBuilder<CodeActionProxy> builder = ArrayBuilder<CodeActionProxy>.GetInstance();
		try
		{
			CodeActionHelper.GetCodeActions(await codeFixService.GetFixesAsync(document, textSpan, cancellationToken).ConfigureAwait(continueOnCapturedContext: false), delegate(CodeActionProxy a)
			{
				builder.Add(a);
			});
			if (builder.IsEmpty())
			{
				CodeActionHelper.GetCodeActions(await codeRefactoringService.GetRefactoringsAsync(document, textSpan, cancellationToken).ConfigureAwait(continueOnCapturedContext: false), delegate(CodeActionProxy a)
				{
					builder.Add(a);
				});
			}
			return builder.ToImmutable();
		}
		finally
		{
			builder.Free();
		}
	}

	private bool AreCodeActionsDisabledInProject(Project project)
	{
		if (project?.State != null)
		{
			return !project.State.EnableCodeActions;
		}
		return true;
	}
}
