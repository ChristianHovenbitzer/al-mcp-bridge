using System;
using System.Threading;
using Microsoft.Dynamics.Nav.CodeAnalysis.CodeActions;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.CodeRefactoring;

public struct CodeRefactoringContext
{
	private readonly Action<CodeAction> registerRefactoring;

	public Document Document { get; }

	public TextSpan Span { get; }

	public CancellationToken CancellationToken { get; }

	public CodeRefactoringContext(Document document, TextSpan span, Action<CodeAction> registerRefactoring, CancellationToken cancellationToken)
	{
		Document = document ?? throw new ArgumentNullException("document");
		Span = span;
		this.registerRefactoring = registerRefactoring ?? throw new ArgumentNullException("registerRefactoring");
		CancellationToken = cancellationToken;
	}

	public void RegisterRefactoring(CodeAction action)
	{
		if (action == null)
		{
			throw new ArgumentNullException("action");
		}
		registerRefactoring(action);
	}
}
