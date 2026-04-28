using System.Collections.Generic;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Formatting;

internal struct NextAction<TArgument>
{
	private readonly int _index;

	private readonly SyntaxNode _node;

	private readonly SyntaxToken _lastToken;

	private readonly IActionHolder<TArgument> _actionCache;

	public NextAction(int index, SyntaxNode node, SyntaxToken lastToken, IActionHolder<TArgument> actionCache)
	{
		_index = index;
		_node = node;
		_lastToken = lastToken;
		_actionCache = actionCache;
	}

	public void Invoke(List<TArgument> arguments)
	{
		_actionCache.Continuation(_index, arguments, _node, _lastToken, _actionCache);
	}
}
