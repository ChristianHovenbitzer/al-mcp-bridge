using System;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Formatting;

internal class OperationCache<TResult> : IOperationHolder<TResult>
{
	public Func<int, SyntaxToken, SyntaxToken, NextOperation<TResult>, TResult> NextOperation { get; }

	public Func<int, SyntaxToken, SyntaxToken, IOperationHolder<TResult>, TResult> Continuation { get; }

	public OperationCache(Func<int, SyntaxToken, SyntaxToken, NextOperation<TResult>, TResult> nextOperation, Func<int, SyntaxToken, SyntaxToken, IOperationHolder<TResult>, TResult> continuation)
	{
		NextOperation = nextOperation;
		Continuation = continuation;
	}
}
