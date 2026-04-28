using Microsoft.Dynamics.Nav.CodeAnalysis.Shared.Collections;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces;

internal static class SimpleIntervalTreeExtensions
{
	public static bool HasIntervalThatIntersectsWith(this SimpleIntervalTree<TextSpan> tree, TextSpan span)
	{
		return tree.HasIntervalThatIntersectsWith(span.Start, span.Length);
	}
}
