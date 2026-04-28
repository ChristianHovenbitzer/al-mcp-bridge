using System.Collections.Generic;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Options;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Formatting.Rules;

internal class NoOpFormattingRule : IFormattingRule
{
	public void AddSuppressOperations(List<SuppressOperation> list, SyntaxNode node, SyntaxToken lastToken, OptionSet optionSet, NextAction<SuppressOperation> nextOperation)
	{
		nextOperation.Invoke(list);
	}

	public void AddAnchorIndentationOperations(List<AnchorIndentationOperation> list, SyntaxNode node, OptionSet optionSet, NextAction<AnchorIndentationOperation> nextOperation)
	{
		nextOperation.Invoke(list);
	}

	public void AddIndentBlockOperations(List<IndentBlockOperation> list, SyntaxNode node, OptionSet optionSet, NextAction<IndentBlockOperation> nextOperation)
	{
		nextOperation.Invoke(list);
	}

	public void AddAlignTokensOperations(List<AlignTokensOperation> list, SyntaxNode node, OptionSet optionSet, NextAction<AlignTokensOperation> nextOperation)
	{
		nextOperation.Invoke(list);
	}

	public AdjustNewLinesOperation? GetAdjustNewLinesOperation(SyntaxToken previousToken, SyntaxToken currentToken, OptionSet optionSet, NextOperation<AdjustNewLinesOperation> nextOperation)
	{
		return nextOperation.Invoke();
	}

	public AdjustSpacesOperation GetAdjustSpacesOperation(SyntaxToken previousToken, SyntaxToken currentToken, OptionSet optionSet, NextOperation<AdjustSpacesOperation> nextOperation)
	{
		return nextOperation.Invoke();
	}
}
