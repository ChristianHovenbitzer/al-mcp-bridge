using System.Collections.Generic;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Formatting.Rules;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Formatting;

internal abstract class BaseFormattingRule : AbstractFormattingRule
{
	protected void AddUnindentBlockOperation(List<IndentBlockOperation> list, SyntaxToken startToken, SyntaxToken endToken, TextSpan textSpan, IndentBlockOption option = IndentBlockOption.RelativePosition)
	{
		if (startToken.Kind != 0 && endToken.Kind != 0)
		{
			list.Add(FormattingOperations.CreateIndentBlockOperation(startToken, endToken, textSpan, -1, option));
		}
	}

	protected void AddUnindentBlockOperation(List<IndentBlockOperation> list, SyntaxToken startToken, SyntaxToken endToken, bool includeTriviaAtEnd = false, IndentBlockOption option = IndentBlockOption.RelativePosition)
	{
		if (startToken.Kind != 0 && endToken.Kind != 0)
		{
			if (includeTriviaAtEnd)
			{
				list.Add(FormattingOperations.CreateIndentBlockOperation(startToken, endToken, -1, option));
				return;
			}
			int startPositionOfSpan = CommonFormattingHelpers.GetStartPositionOfSpan(startToken);
			int end = endToken.Span.End;
			list.Add(FormattingOperations.CreateIndentBlockOperation(startToken, endToken, TextSpan.FromBounds(startPositionOfSpan, end), -1, option));
		}
	}

	protected void AddAbsoluteZeroIndentBlockOperation(List<IndentBlockOperation> list, SyntaxToken startToken, SyntaxToken endToken, IndentBlockOption option = IndentBlockOption.AbsolutePosition)
	{
		if (startToken.Kind != 0 && endToken.Kind != 0)
		{
			list.Add(FormattingOperations.CreateIndentBlockOperation(startToken, endToken, 0, option));
		}
	}

	protected void AddIndentBlockOperation(List<IndentBlockOperation> list, SyntaxToken startToken, SyntaxToken endToken, IndentBlockOption option = IndentBlockOption.RelativePosition)
	{
		if (startToken.Kind != 0 && endToken.Kind != 0)
		{
			list.Add(FormattingOperations.CreateIndentBlockOperation(startToken, endToken, 1, option));
		}
	}

	protected void AddIndentBlockOperation(List<IndentBlockOperation> list, SyntaxToken startToken, SyntaxToken endToken, TextSpan textSpan, IndentBlockOption option = IndentBlockOption.RelativePosition)
	{
		if (startToken.Kind != 0 && endToken.Kind != 0)
		{
			list.Add(FormattingOperations.CreateIndentBlockOperation(startToken, endToken, textSpan, 1, option));
		}
	}

	protected void AddIndentBlockOperation(List<IndentBlockOperation> list, SyntaxToken baseToken, SyntaxToken startToken, SyntaxToken endToken, IndentBlockOption option = IndentBlockOption.RelativePosition)
	{
		list.Add(FormattingOperations.CreateRelativeIndentBlockOperation(baseToken, startToken, endToken, 1, option));
	}

	protected void AddIndentBlockOperation(List<IndentBlockOperation> list, SyntaxToken baseToken, SyntaxToken startToken, SyntaxToken endToken, TextSpan textSpan, IndentBlockOption option = IndentBlockOption.RelativePosition)
	{
		list.Add(FormattingOperations.CreateRelativeIndentBlockOperation(baseToken, startToken, endToken, textSpan, 1, option));
	}

	protected void SetAlignmentBlockOperation(List<IndentBlockOperation> list, SyntaxToken baseToken, SyntaxToken startToken, SyntaxToken endToken, IndentBlockOption option = IndentBlockOption.RelativePosition)
	{
		list.Add(FormattingOperations.CreateRelativeIndentBlockOperation(baseToken, startToken, endToken, 0, option));
	}

	protected void AddSuppressWrappingIfOnSingleLineOperation(List<SuppressOperation> list, SyntaxToken startToken, SyntaxToken endToken, SuppressOption extraOption = SuppressOption.None)
	{
		AddSuppressOperation(list, startToken, endToken, SuppressOption.NoWrappingIfOnSingleLine | extraOption);
	}

	protected void AddSuppressAllOperationIfOnMultipleLine(List<SuppressOperation> list, SyntaxToken startToken, SyntaxToken endToken, SuppressOption extraOption = SuppressOption.None)
	{
		AddSuppressOperation(list, startToken, endToken, SuppressOption.NoWrapping | SuppressOption.NoSpacingIfOnMultipleLine | extraOption);
	}

	protected void AddSuppressOperation(List<SuppressOperation> list, SyntaxToken startToken, SyntaxToken endToken, SuppressOption option)
	{
		if (startToken.Kind != 0 && endToken.Kind != 0)
		{
			list.Add(FormattingOperations.CreateSuppressOperation(startToken, endToken, option));
		}
	}

	protected void AddAnchorIndentationOperation(List<AnchorIndentationOperation> list, SyntaxToken anchorToken, SyntaxToken endToken)
	{
		if (anchorToken.Kind != 0 && endToken.Kind != 0)
		{
			list.Add(FormattingOperations.CreateAnchorIndentationOperation(anchorToken, endToken));
		}
	}

	protected void AddAlignIndentationOfTokensToBaseTokenOperation(List<AlignTokensOperation> list, SyntaxNode containingNode, SyntaxToken baseNode, IEnumerable<SyntaxToken> tokens, AlignTokensOption option = AlignTokensOption.AlignIndentationOfTokensToBaseToken)
	{
		if (containingNode != null && tokens != null)
		{
			list.Add(FormattingOperations.CreateAlignTokensOperation(baseNode, tokens, option));
		}
	}

	protected AdjustNewLinesOperation CreateAdjustNewLinesOperation(int line, AdjustNewLinesOption option)
	{
		return FormattingOperations.CreateAdjustNewLinesOperation(line, option);
	}

	protected AdjustSpacesOperation CreateAdjustSpacesOperation(int space, AdjustSpacesOption option)
	{
		return FormattingOperations.CreateAdjustSpacesOperation(space, option);
	}

	protected void AddBraceSuppressOperations(List<SuppressOperation> list, SyntaxNode node, SyntaxToken lastToken)
	{
		(SyntaxToken, SyntaxToken) bracePair = node.GetBracePair();
		if (!bracePair.IsValidScopeDelimiterPair())
		{
			return;
		}
		SyntaxToken firstToken = node.GetFirstToken(includeZeroWidth: true);
		SyntaxToken endToken = bracePair.Item2;
		if (lastToken.Kind != SyntaxKind.CloseBraceToken && lastToken.Kind != SyntaxKind.EndOfFileToken && !endToken.IsMissing && SomeParentHasMissingCloseBrace(node.Parent))
		{
			if (node.IsKind(SyntaxKind.Block) && ((BlockSyntax)node).Statements.Count >= 1)
			{
				_ = ((BlockSyntax)node).Statements[0];
			}
			else
			{
				endToken = endToken.GetPreviousToken();
			}
		}
		AddSuppressWrappingIfOnSingleLineOperation(list, firstToken, endToken);
		AddSuppressWrappingIfOnSingleLineOperation(list, bracePair.Item1, endToken);
	}

	private bool SomeParentHasMissingCloseBrace(SyntaxNode node)
	{
		while (node != null && node.Kind != SyntaxKind.CompilationUnit)
		{
			if (node.GetBracePair().Close.IsMissing)
			{
				return true;
			}
			node = node.Parent;
		}
		return false;
	}
}
