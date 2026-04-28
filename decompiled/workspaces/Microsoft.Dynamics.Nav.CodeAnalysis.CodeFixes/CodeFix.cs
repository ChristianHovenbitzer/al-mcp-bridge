using System.Collections.Immutable;
using Microsoft.Dynamics.Nav.CodeAnalysis.CodeActions;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.CodeFixes;

internal sealed class CodeFix
{
	internal readonly Project Project;

	private readonly CodeAction Action;

	internal readonly ImmutableArray<Diagnostic> Diagnostics;

	private ImmutableArray<FixAllState> fixAllStates;

	internal Diagnostic PrimaryDiagnostic => Diagnostics[0];

	internal CodeFix(Project project, CodeAction action, ImmutableArray<Diagnostic> diagnostics, ImmutableArray<FixAllState> fixAllStates = default(ImmutableArray<FixAllState>))
	{
		Project = project;
		Action = action;
		Diagnostics = diagnostics;
		this.fixAllStates = fixAllStates.NullToEmpty();
	}

	internal CodeAction GetCodeAction()
	{
		if (fixAllStates.IsEmpty)
		{
			return Action;
		}
		ImmutableArray<CodeAction>.Builder builder = ImmutableArray.CreateBuilder<CodeAction>();
		builder.Add(new CodeAction.CodeActionWrapper(Action.FixAllSingleInstanceTitle ?? WorkspacesResources.FixSingleInstance, Action.EquivalenceKey, Action));
		if (Action.SupportsFixAll)
		{
			ImmutableArray<FixAllState>.Enumerator enumerator = fixAllStates.GetEnumerator();
			while (enumerator.MoveNext())
			{
				FixAllState current = enumerator.Current;
				builder.Add(new CodeAction.CodeActionWithFixAll(current.FixAllProvider?.GetOverrideFixAllTitle(current.Scope) ?? FixAllContextHelper.GetDefaultFixAllTitle(current.Scope, PrimaryDiagnostic.Id), Action.EquivalenceKey, current));
			}
		}
		return new CodeAction.CodeActionWithNestedActions(Action.FixAllTitle ?? Action.Title, builder.ToImmutable());
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
