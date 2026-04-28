using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.Dynamics.Nav.CodeAnalysis.Shared.Collections;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Formatting;

internal class TriviaRewriter : SyntaxRewriter
{
	private readonly CancellationToken cancellationToken;

	private readonly Dictionary<SyntaxToken, SyntaxTriviaList> leadingTriviaMap;

	private readonly SyntaxNode node;

	private readonly SimpleIntervalTree<TextSpan> spans;

	private readonly Dictionary<SyntaxToken, SyntaxTriviaList> trailingTriviaMap;

	public TriviaRewriter(SyntaxNode node, SimpleIntervalTree<TextSpan> spanToFormat, Dictionary<(SyntaxToken, SyntaxToken), TriviaData> map, CancellationToken cancellationToken)
	{
		Contract.ThrowIfNull(node);
		Contract.ThrowIfNull(map);
		this.node = node;
		spans = spanToFormat;
		this.cancellationToken = cancellationToken;
		trailingTriviaMap = new Dictionary<SyntaxToken, SyntaxTriviaList>();
		leadingTriviaMap = new Dictionary<SyntaxToken, SyntaxTriviaList>();
		PreprocessTriviaListMap(map, cancellationToken);
	}

	public SyntaxNode Transform()
	{
		return Visit(node);
	}

	public override SyntaxNode Visit(SyntaxNode node)
	{
		cancellationToken.ThrowIfCancellationRequested();
		if (node == null || !spans.HasIntervalThatIntersectsWith(node.FullSpan))
		{
			return node;
		}
		return base.Visit(node);
	}

	public override SyntaxToken VisitToken(SyntaxToken token)
	{
		cancellationToken.ThrowIfCancellationRequested();
		if (!spans.HasIntervalThatIntersectsWith(token.FullSpan))
		{
			return token;
		}
		bool flag = false;
		if (trailingTriviaMap.TryGetValue(token, out var value))
		{
			flag = true;
		}
		else
		{
			value = token.TrailingTrivia;
		}
		if (leadingTriviaMap.TryGetValue(token, out var value2))
		{
			flag = true;
		}
		else
		{
			value2 = token.LeadingTrivia;
		}
		if (flag)
		{
			return CreateNewToken(value2, token, value);
		}
		return token;
	}

	private void PreprocessTriviaListMap(Dictionary<(SyntaxToken, SyntaxToken), TriviaData> map, CancellationToken cancellationToken)
	{
		foreach (KeyValuePair<(SyntaxToken, SyntaxToken), TriviaData> item in map)
		{
			cancellationToken.ThrowIfCancellationRequested();
			(SyntaxTriviaList, SyntaxTriviaList) trailingAndLeadingTrivia = GetTrailingAndLeadingTrivia(item, cancellationToken);
			if (item.Key.Item1.Kind != 0)
			{
				trailingTriviaMap.Add(item.Key.Item1, trailingAndLeadingTrivia.Item1);
			}
			if (item.Key.Item2.Kind != 0)
			{
				leadingTriviaMap.Add(item.Key.Item2, trailingAndLeadingTrivia.Item2);
			}
		}
	}

	private (SyntaxTriviaList, SyntaxTriviaList) GetTrailingAndLeadingTrivia(KeyValuePair<(SyntaxToken, SyntaxToken), TriviaData> pair, CancellationToken cancellationToken)
	{
		if (pair.Key.Item1.Kind == SyntaxKind.None)
		{
			return ValueTuple.Create(default(SyntaxTriviaList), GetLeadingTriviaAtBeginningOfTree(pair.Key, pair.Value, cancellationToken));
		}
		if (pair.Value is TriviaDataWithList triviaDataWithList)
		{
			List<SyntaxTrivia> triviaList = triviaDataWithList.GetTriviaList(cancellationToken);
			int firstEndOfLineIndexOrRightBeforeComment = GetFirstEndOfLineIndexOrRightBeforeComment(triviaList);
			return ValueTuple.Create(SyntaxFactory.TriviaList(CreateTriviaListFromTo(triviaList, 0, firstEndOfLineIndexOrRightBeforeComment)), SyntaxFactory.TriviaList(CreateTriviaListFromTo(triviaList, firstEndOfLineIndexOrRightBeforeComment + 1, triviaList.Count - 1)));
		}
		string newText = pair.Value.GetTextChanges(GetTextSpan(pair.Key)).Single().NewText;
		SyntaxTriviaList syntaxTriviaList = SyntaxFactory.ParseTrailingTrivia(newText);
		int fullWidth = syntaxTriviaList.GetFullWidth();
		SyntaxTriviaList item = SyntaxFactory.ParseLeadingTrivia(newText.Substring(fullWidth));
		return ValueTuple.Create(syntaxTriviaList, item);
	}

	private TextSpan GetTextSpan((SyntaxToken, SyntaxToken) pair)
	{
		if (pair.Item1.Kind == SyntaxKind.None)
		{
			return TextSpan.FromBounds(node.FullSpan.Start, pair.Item2.SpanStart);
		}
		if (pair.Item2.Kind == SyntaxKind.None)
		{
			return TextSpan.FromBounds(pair.Item1.Span.End, node.FullSpan.End);
		}
		return TextSpan.FromBounds(pair.Item1.Span.End, pair.Item2.SpanStart);
	}

	private IEnumerable<SyntaxTrivia> CreateTriviaListFromTo(List<SyntaxTrivia> triviaList, int startIndex, int endIndex)
	{
		if (startIndex <= endIndex)
		{
			for (int i = startIndex; i <= endIndex; i++)
			{
				yield return triviaList[i];
			}
		}
	}

	private int GetFirstEndOfLineIndexOrRightBeforeComment(List<SyntaxTrivia> triviaList)
	{
		for (int i = 0; i < triviaList.Count; i++)
		{
			if (triviaList[i].Kind == SyntaxKind.EndOfLineTrivia)
			{
				return i;
			}
		}
		return triviaList.Count - 1;
	}

	private SyntaxTriviaList GetLeadingTriviaAtBeginningOfTree((SyntaxToken, SyntaxToken) pair, TriviaData triviaData, CancellationToken cancellationToken)
	{
		if (triviaData is TriviaDataWithList triviaDataWithList)
		{
			return SyntaxFactory.TriviaList(triviaDataWithList.GetTriviaList(cancellationToken));
		}
		return SyntaxFactory.ParseLeadingTrivia(triviaData.GetTextChanges(GetTextSpan(pair)).Single().NewText);
	}

	private static SyntaxToken CreateNewToken(SyntaxTriviaList leadingTrivia, SyntaxToken token, SyntaxTriviaList trailingTrivia)
	{
		return token.With(leadingTrivia, trailingTrivia);
	}
}
