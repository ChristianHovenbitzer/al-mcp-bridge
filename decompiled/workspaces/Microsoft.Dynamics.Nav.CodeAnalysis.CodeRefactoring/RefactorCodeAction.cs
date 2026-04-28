using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.CodeAnalysis.CodeActions;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.CodeRefactoring;

internal sealed class RefactorCodeAction : CodeAction.DocumentChangeAction
{
	public override CodeActionKind Kind => CodeActionKind.Refactor;

	public RefactorCodeAction(string title, Func<CancellationToken, Task<Document>> createChangedDocument)
		: base(title, createChangedDocument)
	{
	}
}
