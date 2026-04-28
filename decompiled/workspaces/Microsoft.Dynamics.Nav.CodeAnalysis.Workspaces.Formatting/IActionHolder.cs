using System;
using System.Collections.Generic;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Formatting;

internal interface IActionHolder<TArgument>
{
	Action<int, List<TArgument>, SyntaxNode, SyntaxToken, NextAction<TArgument>> NextOperation { get; }

	Action<int, List<TArgument>, SyntaxNode, SyntaxToken, IActionHolder<TArgument>> Continuation { get; }
}
