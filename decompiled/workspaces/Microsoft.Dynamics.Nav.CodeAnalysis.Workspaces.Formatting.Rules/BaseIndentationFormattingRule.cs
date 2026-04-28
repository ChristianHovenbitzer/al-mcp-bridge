using System;
using System.Collections.Generic;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Options;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Formatting.Rules;

internal class BaseIndentationFormattingRule : AbstractFormattingRule
{
	private readonly int baseIndentation;

	private readonly SyntaxNode commonNode;

	private readonly TextSpan span;

	private readonly SyntaxToken token1;

	private readonly SyntaxToken token2;

	private readonly IFormattingRule vbHelperFormattingRule;

	public BaseIndentationFormattingRule(SyntaxNode root, TextSpan span, int baseIndentation, IFormattingRule vbHelperFormattingRule = null)
	{
		this.span = span;
		SetInnermostNodeForSpan(root, ref this.span, out token1, out token2, out commonNode);
		this.baseIndentation = baseIndentation;
		this.vbHelperFormattingRule = vbHelperFormattingRule;
	}

	public override void AddIndentBlockOperations(List<IndentBlockOperation> list, SyntaxNode node, OptionSet optionSet, NextAction<IndentBlockOperation> nextOperation)
	{
		if (commonNode == node)
		{
			list.Add(new IndentBlockOperation(token1, token2, span, baseIndentation, IndentBlockOption.AbsolutePosition));
		}
		else if (node.Span.Contains(span))
		{
			return;
		}
		AddNextIndentBlockOperations(list, node, optionSet, nextOperation);
		AdjustIndentBlockOperation(list);
	}

	private void AddNextIndentBlockOperations(List<IndentBlockOperation> list, SyntaxNode node, OptionSet optionSet, NextAction<IndentBlockOperation> nextOperation)
	{
		if (vbHelperFormattingRule == null)
		{
			base.AddIndentBlockOperations(list, node, optionSet, nextOperation);
		}
		else
		{
			vbHelperFormattingRule.AddIndentBlockOperations(list, node, optionSet, nextOperation);
		}
	}

	private void AdjustIndentBlockOperation(List<IndentBlockOperation> list)
	{
		for (int i = 0; i < list.Count; i++)
		{
			IndentBlockOperation indentBlockOperation = list[i];
			if (indentBlockOperation == null)
			{
				continue;
			}
			if (span == indentBlockOperation.TextSpan && !Myself(indentBlockOperation))
			{
				list[i] = null;
			}
			else if (!span.Contains(indentBlockOperation.TextSpan))
			{
				if (indentBlockOperation.TextSpan.Contains(span))
				{
					list[i] = null;
				}
				else if (indentBlockOperation.TextSpan.IntersectsWith(span))
				{
					list[i] = CloneAndAdjustFormattingOperation(indentBlockOperation);
				}
			}
		}
	}

	private bool Myself(IndentBlockOperation operation)
	{
		if (operation.TextSpan == span && operation.StartToken == token1 && operation.EndToken == token2 && operation.IndentationDeltaOrPosition == baseIndentation)
		{
			return operation.Option == IndentBlockOption.AbsolutePosition;
		}
		return false;
	}

	private IndentBlockOperation CloneAndAdjustFormattingOperation(IndentBlockOperation operation)
	{
		switch (operation.Option)
		{
		case IndentBlockOption.RelativeToFirstTokenOnBaseTokenLine:
			return FormattingOperations.CreateRelativeIndentBlockOperation(operation.BaseToken, operation.StartToken, operation.EndToken, AdjustTextSpan(operation.TextSpan), operation.IndentationDeltaOrPosition, operation.Option);
		case IndentBlockOption.RelativePosition:
		case IndentBlockOption.AbsolutePosition:
			return FormattingOperations.CreateIndentBlockOperation(operation.StartToken, operation.EndToken, AdjustTextSpan(operation.TextSpan), operation.IndentationDeltaOrPosition, operation.Option);
		default:
			throw ExceptionUtilities.UnexpectedValue(operation.Option);
		}
	}

	private TextSpan AdjustTextSpan(TextSpan textSpan)
	{
		return TextSpan.FromBounds(Math.Max(span.Start, textSpan.Start), Math.Min(span.End, textSpan.End));
	}

	private void SetInnermostNodeForSpan(SyntaxNode root, ref TextSpan span, out SyntaxToken token1, out SyntaxToken token2, out SyntaxNode commonNode)
	{
		commonNode = null;
		GetTokens(root, span, out token1, out token2);
		span = GetSpanFromTokens(span, token1, token2);
		if (token1.Kind != 0 && token2.Kind != 0)
		{
			commonNode = token1.GetCommonRoot(token2);
		}
	}

	private static void GetTokens(SyntaxNode root, TextSpan span, out SyntaxToken token1, out SyntaxToken token2)
	{
		token1 = root.FindToken(span.Start);
		token2 = root.FindTokenFromEnd(span.End);
		if (span.End < token1.Span.Start)
		{
			token1 = token1.GetPreviousToken();
		}
		if (token2.Span.End < span.Start)
		{
			token2 = token2.GetNextToken();
		}
	}

	private static TextSpan GetSpanFromTokens(TextSpan span, SyntaxToken token1, SyntaxToken token2)
	{
		SyntaxTree syntaxTree = token1.SyntaxTree;
		int num = token1.Span.End;
		if (span.Start <= token1.Span.Start)
		{
			token1 = token1.GetPreviousToken();
			num = token1.Span.End;
			if (token1.Kind == SyntaxKind.None)
			{
				num = 0;
			}
		}
		int num2 = token2.Span.Start;
		if (token2.Span.End <= span.End)
		{
			token2 = token2.GetNextToken();
			num2 = token2.Span.Start;
			if (token2.Kind == SyntaxKind.None)
			{
				num2 = syntaxTree.Length;
			}
		}
		if (token1.Equals(token2) && num2 < num)
		{
			int num3 = num2;
			num2 = num;
			num = num3;
		}
		return TextSpan.FromBounds(num, num2);
	}
}
