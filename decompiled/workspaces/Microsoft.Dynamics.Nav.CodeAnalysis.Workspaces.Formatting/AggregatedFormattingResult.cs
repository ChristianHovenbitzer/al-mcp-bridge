using System.Collections.Generic;
using System.Threading;
using Microsoft.Dynamics.Nav.CodeAnalysis.Shared.Collections;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Formatting;

internal class AggregatedFormattingResult : AbstractAggregatedFormattingResult
{
	public AggregatedFormattingResult(SyntaxNode node, IList<AbstractFormattingResult> results, SimpleIntervalTree<TextSpan> formattingSpans)
		: base(node, results, formattingSpans)
	{
	}

	protected override SyntaxNode Rewriter(Dictionary<(SyntaxToken, SyntaxToken), TriviaData> map, CancellationToken cancellationToken)
	{
		return new TriviaRewriter(Node, GetFormattingSpans(), map, cancellationToken).Transform();
	}
}
