using System;
using System.Collections.Immutable;
using Microsoft.Dynamics.Nav.CodeAnalysis.CodeActions;
using Microsoft.Dynamics.Nav.CodeAnalysis.CodeFixes;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.CodeRefactoring;

internal class CodeRefactoring
{
	private ImmutableArray<FixAllState> fixAllStates;

	public CodeRefactoringProvider Provider { get; }

	public ImmutableArray<(CodeAction action, TextSpan? applicableToSpan)> CodeActions { get; }

	public CodeRefactoring(CodeRefactoringProvider provider, ImmutableArray<(CodeAction, TextSpan?)> actions, ImmutableArray<FixAllState> fixAllStates = default(ImmutableArray<FixAllState>))
	{
		Provider = provider;
		this.fixAllStates = fixAllStates.NullToEmpty();
		CodeActions = actions.NullToEmpty();
		if (CodeActions.Length == 0)
		{
			throw new ArgumentException(WorkspacesResources.ActionsCanNotBeEmpty, "actions");
		}
	}

	internal ImmutableArray<(CodeAction action, TextSpan? applicableToSpan)> GetCodeActions()
	{
		if (fixAllStates.IsEmpty)
		{
			return CodeActions;
		}
		ArrayBuilder<(CodeAction, TextSpan?)> instance = ArrayBuilder<(CodeAction, TextSpan?)>.GetInstance();
		try
		{
			ImmutableArray<(CodeAction, TextSpan?)>.Enumerator enumerator = CodeActions.GetEnumerator();
			while (enumerator.MoveNext())
			{
				(CodeAction, TextSpan?) current = enumerator.Current;
				CodeAction item = current.Item1;
				TextSpan? item2 = current.Item2;
				ImmutableArray<CodeAction>.Builder builder = ImmutableArray.CreateBuilder<CodeAction>();
				builder.Add(new CodeAction.CodeActionWrapper(WorkspacesResources.ApplyToSingleInstance, item.EquivalenceKey, item));
				ImmutableArray<FixAllState>.Enumerator enumerator2 = fixAllStates.GetEnumerator();
				while (enumerator2.MoveNext())
				{
					FixAllState current2 = enumerator2.Current;
					builder.Add(new CodeAction.CodeActionWithFixAll(FixAllContextHelper.GetDefaultFixAllTitle(current2.Scope), item.EquivalenceKey, current2));
				}
				instance.Add((new CodeAction.CodeActionWithNestedActions(item.Title, builder.ToImmutable()), item2));
			}
			return instance.ToImmutableArray();
		}
		finally
		{
			instance.Free();
		}
	}

	internal void WithFixAllStates(ImmutableArray<FixAllState> fixAllStates)
	{
		this.fixAllStates = fixAllStates;
	}

	internal ImmutableArray<FixAllState> GetFixAllStates()
	{
		return fixAllStates;
	}
}
