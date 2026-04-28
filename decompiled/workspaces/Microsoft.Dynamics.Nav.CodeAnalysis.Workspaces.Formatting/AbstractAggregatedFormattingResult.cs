using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.Dynamics.Nav.CodeAnalysis.Shared.Collections;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Log;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Formatting;

internal abstract class AbstractAggregatedFormattingResult : IFormattingResult
{
	private readonly IList<AbstractFormattingResult> formattingResults;

	private readonly SimpleIntervalTree<TextSpan> formattingSpans;

	private readonly CancellableLazy<SyntaxNode> lazyNode;

	private readonly CancellableLazy<IList<TextChange>> lazyTextChanges;

	protected readonly SyntaxNode Node;

	public bool ContainsChanges => GetTextChanges(CancellationToken.None).Count > 0;

	protected AbstractAggregatedFormattingResult(SyntaxNode node, IList<AbstractFormattingResult> formattingResults, SimpleIntervalTree<TextSpan> formattingSpans)
	{
		Contract.ThrowIfNull(node);
		Contract.ThrowIfNull(formattingResults);
		Node = node;
		this.formattingResults = formattingResults;
		this.formattingSpans = formattingSpans;
		lazyTextChanges = new CancellableLazy<IList<TextChange>>(CreateTextChanges);
		lazyNode = new CancellableLazy<SyntaxNode>(CreateFormattedRoot);
	}

	protected abstract SyntaxNode Rewriter(Dictionary<(SyntaxToken, SyntaxToken), TriviaData> changeMap, CancellationToken cancellationToken);

	protected SimpleIntervalTree<TextSpan> GetFormattingSpans()
	{
		return formattingSpans ?? SimpleIntervalTree.Create(TextSpanIntervalIntrospector.Instance, formattingResults.Select((AbstractFormattingResult r) => r.FormattedSpan));
	}

	public IList<TextChange> GetTextChanges(CancellationToken cancellationToken)
	{
		return lazyTextChanges.GetValue(cancellationToken);
	}

	public SyntaxNode GetFormattedRoot(CancellationToken cancellationToken)
	{
		return lazyNode.GetValue(cancellationToken);
	}

	private IList<TextChange> CreateTextChanges(CancellationToken cancellationToken)
	{
		using (Logger.LogBlock(FunctionId.Formatting_AggregateCreateTextChanges, cancellationToken))
		{
			IList<TextChange> list = CreateTextChangesWorker(cancellationToken);
			IList<TextChange> result;
			if (formattingSpans != null)
			{
				IList<TextChange> list2 = list.Where((TextChange s) => formattingSpans.HasIntervalThatIntersectsWith(s.Span)).ToList();
				result = list2;
			}
			else
			{
				result = list;
			}
			return result;
		}
	}

	private IList<TextChange> CreateTextChangesWorker(CancellationToken cancellationToken)
	{
		if (formattingResults.Count == 1)
		{
			return formattingResults[0].GetTextChanges(cancellationToken);
		}
		List<TextChange> list = new List<TextChange>(formattingResults.Sum((AbstractFormattingResult r) => r.GetTextChanges(cancellationToken).Count));
		foreach (AbstractFormattingResult formattingResult in formattingResults)
		{
			list.AddRange(formattingResult.GetTextChanges(cancellationToken));
		}
		return list;
	}

	private SyntaxNode CreateFormattedRoot(CancellationToken cancellationToken)
	{
		using (Logger.LogBlock(FunctionId.Formatting_AggregateCreateFormattedRoot, cancellationToken))
		{
			Dictionary<(SyntaxToken, SyntaxToken), TriviaData> map = new Dictionary<(SyntaxToken, SyntaxToken), TriviaData>();
			formattingResults.Do(delegate(AbstractFormattingResult result)
			{
				result.GetChanges(cancellationToken).Do<((SyntaxToken, SyntaxToken), TriviaData)>(delegate(((SyntaxToken, SyntaxToken), TriviaData) change)
				{
					map.Add(change.Item1, change.Item2);
				});
			});
			return Rewriter(map, cancellationToken);
		}
	}
}
