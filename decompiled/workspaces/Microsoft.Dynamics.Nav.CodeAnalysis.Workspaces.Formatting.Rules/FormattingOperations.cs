using System.Collections.Generic;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Options;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Formatting.Rules;

internal static class FormattingOperations
{
	private static readonly AdjustNewLinesOperation s_preserveZeroLine = new AdjustNewLinesOperation(0, AdjustNewLinesOption.PreserveLines);

	private static readonly AdjustNewLinesOperation s_preserveOneLine = new AdjustNewLinesOperation(1, AdjustNewLinesOption.PreserveLines);

	private static readonly AdjustNewLinesOperation s_forceOneLine = new AdjustNewLinesOperation(1, AdjustNewLinesOption.ForceLines);

	private static readonly AdjustNewLinesOperation s_forceIfSameLine = new AdjustNewLinesOperation(1, AdjustNewLinesOption.ForceLinesIfOnSingleLine);

	private static readonly AdjustSpacesOperation s_defaultOneSpaceIfOnSingleLine = new AdjustSpacesOperation(1, AdjustSpacesOption.DefaultSpacesIfOnSingleLine);

	private static readonly AdjustSpacesOperation s_forceOneSpaceIfOnSingleLine = new AdjustSpacesOperation(1, AdjustSpacesOption.ForceSpacesIfOnSingleLine);

	private static readonly AdjustSpacesOperation s_forceZeroSpaceIfOnSingleLine = new AdjustSpacesOperation(0, AdjustSpacesOption.ForceSpacesIfOnSingleLine);

	private static readonly AdjustSpacesOperation s_forceZeroLineUsingSpaceForce = new AdjustSpacesOperation(1, AdjustSpacesOption.ForceSpaces);

	public static AnchorIndentationOperation CreateAnchorIndentationOperation(SyntaxToken startToken, SyntaxToken endToken)
	{
		return CreateAnchorIndentationOperation(startToken, startToken, endToken, TextSpan.FromBounds(startToken.Span.End, endToken.Span.End));
	}

	public static AnchorIndentationOperation CreateAnchorIndentationOperation(SyntaxToken anchorToken, SyntaxToken startToken, SyntaxToken endToken, TextSpan textSpan)
	{
		return new AnchorIndentationOperation(anchorToken, startToken, endToken, textSpan);
	}

	public static SuppressOperation CreateSuppressOperation(SyntaxToken startToken, SyntaxToken endToken, SuppressOption option)
	{
		return CreateSuppressOperation(startToken, endToken, TextSpan.FromBounds(startToken.SpanStart, endToken.Span.End), option);
	}

	public static IndentBlockOperation CreateIndentBlockOperation(SyntaxToken startToken, SyntaxToken endToken, int indentationDelta, IndentBlockOption option)
	{
		TextSpan spanIncludingTrailingAndLeadingTriviaOfAdjacentTokens = CommonFormattingHelpers.GetSpanIncludingTrailingAndLeadingTriviaOfAdjacentTokens(startToken, endToken);
		return CreateIndentBlockOperation(startToken, endToken, spanIncludingTrailingAndLeadingTriviaOfAdjacentTokens, indentationDelta, option);
	}

	public static IndentBlockOperation CreateIndentBlockOperation(SyntaxToken startToken, SyntaxToken endToken, TextSpan textSpan, int indentationDelta, IndentBlockOption option)
	{
		return new IndentBlockOperation(startToken, endToken, textSpan, indentationDelta, option);
	}

	public static IndentBlockOperation CreateRelativeIndentBlockOperation(SyntaxToken baseToken, SyntaxToken startToken, SyntaxToken endToken, int indentationDelta, IndentBlockOption option)
	{
		TextSpan textSpan = TextSpan.FromBounds(CommonFormattingHelpers.GetStartPositionOfSpan(startToken), endToken.SpanEnd);
		return CreateRelativeIndentBlockOperation(baseToken, startToken, endToken, textSpan, indentationDelta, option);
	}

	public static IndentBlockOperation CreateRelativeIndentBlockOperation(SyntaxToken baseToken, SyntaxToken startToken, SyntaxToken endToken, TextSpan textSpan, int indentationDelta, IndentBlockOption option)
	{
		return new IndentBlockOperation(baseToken, startToken, endToken, textSpan, indentationDelta, option);
	}

	public static AlignTokensOperation CreateAlignTokensOperation(SyntaxToken baseToken, IEnumerable<SyntaxToken> tokens, AlignTokensOption option)
	{
		return new AlignTokensOperation(baseToken, tokens, option);
	}

	public static AdjustNewLinesOperation CreateAdjustNewLinesOperation(int line, AdjustNewLinesOption option)
	{
		switch (line)
		{
		case 0:
			if (option == AdjustNewLinesOption.PreserveLines)
			{
				return s_preserveZeroLine;
			}
			break;
		case 1:
			switch (option)
			{
			case AdjustNewLinesOption.PreserveLines:
				return s_preserveOneLine;
			case AdjustNewLinesOption.ForceLines:
				return s_forceOneLine;
			case AdjustNewLinesOption.ForceLinesIfOnSingleLine:
				return s_forceIfSameLine;
			}
			break;
		}
		return new AdjustNewLinesOperation(line, option);
	}

	public static AdjustSpacesOperation CreateAdjustSpacesOperation(int space, AdjustSpacesOption option)
	{
		if (space == 1 && option == AdjustSpacesOption.DefaultSpacesIfOnSingleLine)
		{
			return s_defaultOneSpaceIfOnSingleLine;
		}
		if (space == 0 && option == AdjustSpacesOption.ForceSpacesIfOnSingleLine)
		{
			return s_forceZeroSpaceIfOnSingleLine;
		}
		if (space == 1 && option == AdjustSpacesOption.ForceSpacesIfOnSingleLine)
		{
			return s_forceOneSpaceIfOnSingleLine;
		}
		if (space == 1 && option == AdjustSpacesOption.ForceSpaces)
		{
			return s_forceZeroLineUsingSpaceForce;
		}
		return new AdjustSpacesOperation(space, option);
	}

	internal static IEnumerable<SuppressOperation> GetSuppressOperations(IEnumerable<IFormattingRule> formattingRules, SyntaxNode node, SyntaxToken lastToken, OptionSet optionSet)
	{
		ChainedFormattingRules chainedFormattingRules = new ChainedFormattingRules(formattingRules, optionSet);
		List<SuppressOperation> list = new List<SuppressOperation>();
		chainedFormattingRules.AddSuppressOperations(list, node, lastToken);
		return list;
	}

	internal static IEnumerable<AnchorIndentationOperation> GetAnchorIndentationOperations(IEnumerable<IFormattingRule> formattingRules, SyntaxNode node, SyntaxToken lastToken, OptionSet optionSet)
	{
		ChainedFormattingRules chainedFormattingRules = new ChainedFormattingRules(formattingRules, optionSet);
		List<AnchorIndentationOperation> list = new List<AnchorIndentationOperation>();
		chainedFormattingRules.AddAnchorIndentationOperations(list, node, lastToken);
		return list;
	}

	internal static IEnumerable<IndentBlockOperation> GetIndentBlockOperations(IEnumerable<IFormattingRule> formattingRules, SyntaxNode node, SyntaxToken lastToken, OptionSet optionSet)
	{
		ChainedFormattingRules chainedFormattingRules = new ChainedFormattingRules(formattingRules, optionSet);
		List<IndentBlockOperation> list = new List<IndentBlockOperation>();
		chainedFormattingRules.AddIndentBlockOperations(list, node, lastToken);
		return list;
	}

	internal static IEnumerable<AlignTokensOperation> GetAlignTokensOperations(IEnumerable<IFormattingRule> formattingRules, SyntaxNode node, SyntaxToken lastToken, OptionSet optionSet)
	{
		ChainedFormattingRules chainedFormattingRules = new ChainedFormattingRules(formattingRules, optionSet);
		List<AlignTokensOperation> list = new List<AlignTokensOperation>();
		chainedFormattingRules.AddAlignTokensOperations(list, node, lastToken);
		return list;
	}

	internal static AdjustNewLinesOperation GetAdjustNewLinesOperation(IEnumerable<IFormattingRule> formattingRules, SyntaxToken previousToken, SyntaxToken currentToken, OptionSet optionSet)
	{
		return new ChainedFormattingRules(formattingRules, optionSet).GetAdjustNewLinesOperation(previousToken, currentToken);
	}

	internal static AdjustSpacesOperation GetAdjustSpacesOperation(IEnumerable<IFormattingRule> formattingRules, SyntaxToken previousToken, SyntaxToken currentToken, OptionSet optionSet)
	{
		return new ChainedFormattingRules(formattingRules, optionSet).GetAdjustSpacesOperation(previousToken, currentToken);
	}

	private static SuppressOperation CreateSuppressOperation(SyntaxToken startToken, SyntaxToken endToken, TextSpan textSpan, SuppressOption option)
	{
		return new SuppressOperation(startToken, endToken, textSpan, option);
	}
}
