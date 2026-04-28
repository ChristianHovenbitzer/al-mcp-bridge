using System;
using System.Collections.Generic;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Formatting;

internal class ActionCache<TArgument> : IActionHolder<TArgument>
{
	public Action<int, List<TArgument>, SyntaxNode, SyntaxToken, NextAction<TArgument>> NextOperation { get; }

	public Action<int, List<TArgument>, SyntaxNode, SyntaxToken, IActionHolder<TArgument>> Continuation { get; }

	public ActionCache(Action<int, List<TArgument>, SyntaxNode, SyntaxToken, NextAction<TArgument>> nextOperation, Action<int, List<TArgument>, SyntaxNode, SyntaxToken, IActionHolder<TArgument>> continuation)
	{
		NextOperation = nextOperation;
		Continuation = continuation;
	}
}
