using System.Collections.Generic;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces;

internal static class SyntaxNodeOrTokenExtensions
{
	public static IEnumerable<SyntaxNodeOrToken> DepthFirstTraversal(this SyntaxNodeOrToken node)
	{
		Stack<SyntaxNodeOrToken> stack = new Stack<SyntaxNodeOrToken>();
		stack.Push(node);
		while (!stack.IsEmpty())
		{
			SyntaxNodeOrToken current = stack.Pop();
			yield return current;
			if (current.IsNode)
			{
				ChildSyntaxList.Reversed.Enumerator enumerator = current.ChildNodesAndTokens().Reverse().GetEnumerator();
				while (enumerator.MoveNext())
				{
					SyntaxNodeOrToken current2 = enumerator.Current;
					stack.Push(current2);
				}
			}
		}
	}
}
