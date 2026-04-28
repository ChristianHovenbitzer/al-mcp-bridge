using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Formatting.Rules;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Formatting;

internal class BottomUpBaseIndentationFinder
{
	private readonly ChainedFormattingRules formattingRules;

	private readonly int indentationSize;

	private readonly SyntaxToken lastToken;

	private readonly int tabSize;

	private readonly TokenStream tokenStream;

	public BottomUpBaseIndentationFinder(ChainedFormattingRules formattingRules, int tabSize, int indentationSize, TokenStream tokenStream, SyntaxToken lastToken)
	{
		Contract.ThrowIfNull(formattingRules);
		this.formattingRules = formattingRules;
		this.tabSize = tabSize;
		this.indentationSize = indentationSize;
		this.tokenStream = tokenStream;
		this.lastToken = lastToken;
	}

	public int? FromIndentBlockOperations(SyntaxTree tree, SyntaxToken token, int position, CancellationToken cancellationToken)
	{
		IndentBlockOperation indentationDataFor = GetIndentationDataFor(tree.GetRoot(cancellationToken), token, position);
		if (indentationDataFor != null && token.Span.End <= indentationDataFor.TextSpan.Start && indentationDataFor.TextSpan.IntersectsWith(position) && position <= token.GetNextToken(includeZeroWidth: true).SpanStart)
		{
			return GetIndentationOfCurrentPosition(tree, token, position, cancellationToken);
		}
		return null;
	}

	public int? FromAlignTokensOperations(SyntaxTree tree, SyntaxToken token)
	{
		SyntaxToken nextToken = token.GetNextToken(includeZeroWidth: true);
		if (nextToken.Kind != 0 && nextToken.Width() <= 0)
		{
			SyntaxToken alignmentBaseTokenFor = GetAlignmentBaseTokenFor(nextToken);
			if (alignmentBaseTokenFor.Kind != 0)
			{
				return tree.GetTokenColumn(alignmentBaseTokenFor, tabSize);
			}
		}
		return null;
	}

	public int GetIndentationOfCurrentPosition(SyntaxTree tree, SyntaxToken token, int position, CancellationToken cancellationToken)
	{
		return GetIndentationOfCurrentPosition(tree, token, position, 0, cancellationToken);
	}

	public int GetIndentationOfCurrentPosition(SyntaxTree tree, SyntaxToken token, int position, int extraSpaces, CancellationToken cancellationToken)
	{
		SyntaxTree tree2 = tree;
		List<IndentBlockOperation> parentIndentBlockOperations = GetParentIndentBlockOperations(token);
		return GetIndentationOfCurrentPosition(tree2.GetRoot(cancellationToken), token, parentIndentBlockOperations, position, extraSpaces, (SyntaxToken t) => tree2.GetTokenColumn(t, tabSize), cancellationToken);
	}

	public int GetIndentationOfCurrentPosition(SyntaxNode root, IndentBlockOperation startingOperation, Func<SyntaxToken, int> tokenColumnGetter, CancellationToken cancellationToken)
	{
		SyntaxToken startToken = startingOperation.StartToken;
		List<IndentBlockOperation> parentIndentBlockOperations = GetParentIndentBlockOperations(startToken);
		int num = parentIndentBlockOperations.Count - 1;
		while (num >= 0 && CommonFormattingHelpers.IndentBlockOperationComparer(startingOperation, parentIndentBlockOperations[num]) < 0)
		{
			parentIndentBlockOperations.RemoveAt(num);
			num--;
		}
		return GetIndentationOfCurrentPosition(root, startToken, parentIndentBlockOperations, startToken.SpanStart, 0, tokenColumnGetter, cancellationToken);
	}

	private int GetIndentationOfCurrentPosition(SyntaxNode root, SyntaxToken token, List<IndentBlockOperation> list, int position, int extraSpaces, Func<SyntaxToken, int> tokenColumnGetter, CancellationToken cancellationToken)
	{
		var (num, indentBlockOperation) = GetIndentationRuleOfCurrentPosition(root, token, list, position);
		if (indentBlockOperation == null)
		{
			return num * indentationSize + extraSpaces;
		}
		if (indentBlockOperation.IsRelativeIndentation)
		{
			SyntaxToken syntaxToken = indentBlockOperation.BaseToken;
			if (indentBlockOperation.Option.IsOn(IndentBlockOption.RelativeToFirstTokenOnBaseTokenLine))
			{
				if (tokenStream != null)
				{
					syntaxToken = tokenStream.FirstTokenOfBaseTokenLine(syntaxToken);
				}
				else
				{
					TextLine lineFromPosition = syntaxToken.SyntaxTree.GetText(cancellationToken).Lines.GetLineFromPosition(syntaxToken.SpanStart);
					syntaxToken = syntaxToken.SyntaxTree.GetRoot(cancellationToken).FindToken(lineFromPosition.Start);
				}
			}
			int num2 = tokenColumnGetter(syntaxToken);
			return Math.Max(0, num2 + (num + indentBlockOperation.IndentationDeltaOrPosition) * indentationSize);
		}
		if (indentBlockOperation.Option.IsOn(IndentBlockOption.AbsolutePosition))
		{
			return Math.Max(0, num + extraSpaces);
		}
		throw ExceptionUtilities.Unreachable;
	}

	private (int, IndentBlockOperation) GetIndentationRuleOfCurrentPosition(SyntaxNode root, SyntaxToken token, List<IndentBlockOperation> list, int position)
	{
		int num = 0;
		foreach (IndentBlockOperation item in GetIndentBlockOperationsFromSmallestSpan(root, list, position))
		{
			if (item.Option.IsOn(IndentBlockOption.AbsolutePosition))
			{
				return ValueTuple.Create(item.IndentationDeltaOrPosition + indentationSize * num, item);
			}
			if (item.Option == IndentBlockOption.RelativeToFirstTokenOnBaseTokenLine)
			{
				return ValueTuple.Create(num, item);
			}
			if (item.IsRelativeIndentation)
			{
				return ValueTuple.Create(num, item);
			}
			num += item.IndentationDeltaOrPosition;
		}
		return ValueTuple.Create<int, IndentBlockOperation>(num, null);
	}

	private List<IndentBlockOperation> GetParentIndentBlockOperations(SyntaxToken token)
	{
		IEnumerable<SyntaxNode> parentNodes = GetParentNodes(token);
		List<IndentBlockOperation> list = new List<IndentBlockOperation>();
		parentNodes.Do(delegate(SyntaxNode n)
		{
			formattingRules.AddIndentBlockOperations(list, n, lastToken);
		});
		list.RemoveAll(CommonFormattingHelpers.IsNull);
		list.Sort(CommonFormattingHelpers.IndentBlockOperationComparer);
		return list;
	}

	private IEnumerable<SyntaxNode> GetParentNodes(SyntaxToken token)
	{
		for (SyntaxNode current = token.Parent; current != null; current = ((!current.IsStructuredTrivia) ? current.Parent : ((IStructuredTriviaSyntax)current).ParentTrivia.Token.Parent))
		{
			yield return current;
		}
	}

	private SyntaxToken GetAlignmentBaseTokenFor(SyntaxToken token)
	{
		SyntaxNode parent = token.Parent;
		List<AlignTokensOperation> list = new List<AlignTokensOperation>();
		SyntaxNode syntaxNode = parent;
		while (syntaxNode != null)
		{
			list.Clear();
			formattingRules.AddAlignTokensOperations(list, syntaxNode, lastToken);
			if (list.Count == 0)
			{
				syntaxNode = syntaxNode.Parent;
				continue;
			}
			AlignTokensOperation alignTokensOperation = list.FirstOrDefault((AlignTokensOperation o) => o?.Tokens.Contains(token) ?? false);
			if (alignTokensOperation != null)
			{
				return alignTokensOperation.BaseToken;
			}
			syntaxNode = syntaxNode.Parent;
		}
		return default(SyntaxToken);
	}

	private IndentBlockOperation GetIndentationDataFor(SyntaxNode root, SyntaxToken token, int position)
	{
		SyntaxNode parent = token.Parent;
		List<IndentBlockOperation> list = new List<IndentBlockOperation>();
		for (SyntaxNode syntaxNode = parent; syntaxNode != null; syntaxNode = syntaxNode.Parent)
		{
			formattingRules.AddIndentBlockOperations(list, syntaxNode, lastToken);
			if (list.Any((IndentBlockOperation o) => o?.TextSpan.Contains(position) ?? false))
			{
				break;
			}
		}
		list.RemoveAll(CommonFormattingHelpers.IsNull);
		if (list.Count == 0)
		{
			return null;
		}
		list.Sort(CommonFormattingHelpers.IndentBlockOperationComparer);
		return GetIndentBlockOperationsFromSmallestSpan(root, list, position).FirstOrDefault();
	}

	private static IEnumerable<IndentBlockOperation> GetIndentBlockOperationsFromSmallestSpan(SyntaxNode root, List<IndentBlockOperation> list, int position)
	{
		SyntaxToken lastVisibleToken = default(SyntaxToken);
		HashSet<TextSpan> map = new HashSet<TextSpan>();
		for (int i = list.Count - 1; i >= 0; i--)
		{
			IndentBlockOperation indentBlockOperation = list[i];
			if (!map.Contains(indentBlockOperation.TextSpan))
			{
				map.Add(indentBlockOperation.TextSpan);
				if (indentBlockOperation.TextSpan.Contains(position))
				{
					yield return indentBlockOperation;
				}
				else if (indentBlockOperation.TextSpan.IsEmpty && indentBlockOperation.TextSpan.Start == position)
				{
					yield return indentBlockOperation;
				}
				else
				{
					SyntaxToken nextToken = indentBlockOperation.EndToken.GetNextToken(includeZeroWidth: true);
					if (indentBlockOperation.TextSpan.End == position && nextToken.IsMissing)
					{
						yield return indentBlockOperation;
					}
					else if (indentBlockOperation.TextSpan.End == position && position == nextToken.SpanStart)
					{
						yield return indentBlockOperation;
					}
					else if (root.FullSpan.End == position && indentBlockOperation.TextSpan.End == position)
					{
						yield return indentBlockOperation;
					}
					else
					{
						lastVisibleToken = ((lastVisibleToken.Kind == SyntaxKind.None) ? root.GetLastToken() : lastVisibleToken);
						if (lastVisibleToken.Span.End <= position && indentBlockOperation.TextSpan.End == position)
						{
							yield return indentBlockOperation;
						}
					}
				}
			}
		}
	}
}
