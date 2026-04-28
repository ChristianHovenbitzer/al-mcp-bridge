using System.Collections.Generic;
using System.Threading;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Log;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Formatting;

internal abstract class AbstractFormattingResult : IFormattingResult
{
	private readonly CancellableLazy<IList<TextChange>> lazyChanges;

	private readonly CancellableLazy<SyntaxNode> lazyNode;

	public readonly TextSpan FormattedSpan;

	protected readonly TaskExecutor TaskExecutor;

	protected readonly TokenStream TokenStream;

	protected readonly TreeData TreeInfo;

	internal AbstractFormattingResult(TreeData treeInfo, TokenStream tokenStream, TextSpan formattedSpan, TaskExecutor taskExecutor)
	{
		TreeInfo = treeInfo;
		TokenStream = tokenStream;
		FormattedSpan = formattedSpan;
		TaskExecutor = taskExecutor;
		lazyChanges = new CancellableLazy<IList<TextChange>>(CreateTextChanges);
		lazyNode = new CancellableLazy<SyntaxNode>(CreateFormattedRoot);
	}

	protected abstract SyntaxNode Rewriter(Dictionary<(SyntaxToken, SyntaxToken), TriviaData> map, CancellationToken cancellationToken);

	public IList<TextChange> GetTextChanges(CancellationToken cancellationToken)
	{
		return lazyChanges.GetValue(cancellationToken);
	}

	public SyntaxNode GetFormattedRoot(CancellationToken cancellationToken)
	{
		return lazyNode.GetValue(cancellationToken);
	}

	private IList<TextChange> CreateTextChanges(CancellationToken cancellationToken)
	{
		using (Logger.LogBlock(FunctionId.Formatting_CreateTextChanges, cancellationToken))
		{
			IEnumerable<((SyntaxToken, SyntaxToken), TriviaData)> triviaDataWithTokenPair = TokenStream.GetTriviaDataWithTokenPair(cancellationToken);
			IEnumerable<((SyntaxToken, SyntaxToken), TriviaData)> enumerable = TaskExecutor.Filter(triviaDataWithTokenPair, (((SyntaxToken, SyntaxToken), TriviaData) d) => d.Item2.ContainsChanges, (((SyntaxToken, SyntaxToken), TriviaData) d) => d, cancellationToken);
			List<TextChange> list = new List<TextChange>();
			foreach (var item in enumerable)
			{
				AddTextChanges(list, item.Item1.Item1, item.Item1.Item2, item.Item2);
			}
			return list;
		}
	}

	private void AddTextChanges(List<TextChange> list, SyntaxToken token1, SyntaxToken token2, TriviaData data)
	{
		TextSpan textSpan = TextSpan.FromBounds((token1.Kind == SyntaxKind.None) ? TreeInfo.StartPosition : token1.Span.End, (token2.Kind == SyntaxKind.None) ? TreeInfo.EndPosition : token2.SpanStart);
		string textBetween = TreeInfo.GetTextBetween(token1, token2);
		foreach (TextChange textChange in data.GetTextChanges(textSpan))
		{
			string text = ((textChange.Span == textSpan) ? textBetween : textBetween.Substring(textChange.Span.Start - textSpan.Start, textChange.Span.Length));
			list.Add(textChange.SimpleDiff(text));
		}
	}

	private SyntaxNode CreateFormattedRoot(CancellationToken cancellationToken)
	{
		using (Logger.LogBlock(FunctionId.Formatting_CreateFormattedRoot, cancellationToken))
		{
			IEnumerable<((SyntaxToken, SyntaxToken), TriviaData)> changes = GetChanges(cancellationToken);
			using PooledObject<Dictionary<(SyntaxToken, SyntaxToken), TriviaData>> pooledObject = SharedPools.Default<Dictionary<(SyntaxToken, SyntaxToken), TriviaData>>().GetPooledObject();
			Dictionary<(SyntaxToken, SyntaxToken), TriviaData> map = pooledObject.Object;
			changes.Do(delegate(((SyntaxToken, SyntaxToken), TriviaData) change)
			{
				map.Add(change.Item1, change.Item2);
			});
			if (map.Count == 0)
			{
				return TreeInfo.Root;
			}
			return Rewriter(map, cancellationToken);
		}
	}

	internal IEnumerable<((SyntaxToken, SyntaxToken), TriviaData)> GetChanges(CancellationToken cancellationToken)
	{
		return TaskExecutor.Filter<((SyntaxToken, SyntaxToken), TriviaData), ((SyntaxToken, SyntaxToken), TriviaData)>(TokenStream.GetTriviaDataWithTokenPair(cancellationToken), (((SyntaxToken, SyntaxToken), TriviaData) d) => d.Item2.ContainsChanges, (((SyntaxToken, SyntaxToken), TriviaData) d) => d, cancellationToken);
	}
}
