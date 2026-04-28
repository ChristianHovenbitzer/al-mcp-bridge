using System;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Formatting;

internal interface IOperationHolder<TResult>
{
	Func<int, SyntaxToken, SyntaxToken, NextOperation<TResult>, TResult> NextOperation { get; }

	Func<int, SyntaxToken, SyntaxToken, IOperationHolder<TResult>, TResult> Continuation { get; }
}
