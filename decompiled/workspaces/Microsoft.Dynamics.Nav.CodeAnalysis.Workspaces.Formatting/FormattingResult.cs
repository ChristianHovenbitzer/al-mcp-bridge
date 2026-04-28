using System.Collections.Generic;
using System.Threading;
using Microsoft.Dynamics.Nav.CodeAnalysis.Shared.Collections;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Formatting;

internal class FormattingResult : AbstractFormattingResult
{
	internal FormattingResult(TreeData treeInfo, TokenStream tokenStream, TextSpan spanToFormat, TaskExecutor taskExecutor)
		: base(treeInfo, tokenStream, spanToFormat, taskExecutor)
	{
	}

	protected override SyntaxNode Rewriter(Dictionary<(SyntaxToken, SyntaxToken), TriviaData> changeMap, CancellationToken cancellationToken)
	{
		return new TriviaRewriter(TreeInfo.Root, SimpleIntervalTree.Create(TextSpanIntervalIntrospector.Instance, FormattedSpan), changeMap, cancellationToken).Transform();
	}
}
