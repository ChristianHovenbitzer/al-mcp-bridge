using System.Collections.Generic;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Options;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Formatting.Rules;

internal abstract class AbstractFormattingRule : IFormattingRule
{
	public virtual void AddSuppressOperations(List<SuppressOperation> list, SyntaxNode node, SyntaxToken lastToken, OptionSet optionSet, NextAction<SuppressOperation> nextOperation)
	{
		nextOperation.Invoke(list);
	}

	public virtual void AddAnchorIndentationOperations(List<AnchorIndentationOperation> list, SyntaxNode node, OptionSet optionSet, NextAction<AnchorIndentationOperation> nextOperation)
	{
		nextOperation.Invoke(list);
	}

	public virtual void AddIndentBlockOperations(List<IndentBlockOperation> list, SyntaxNode node, OptionSet optionSet, NextAction<IndentBlockOperation> nextOperation)
	{
		nextOperation.Invoke(list);
	}

	public virtual void AddAlignTokensOperations(List<AlignTokensOperation> list, SyntaxNode node, OptionSet optionSet, NextAction<AlignTokensOperation> nextOperation)
	{
		nextOperation.Invoke(list);
	}

	public virtual AdjustNewLinesOperation? GetAdjustNewLinesOperation(SyntaxToken previousToken, SyntaxToken currentToken, OptionSet optionSet, NextOperation<AdjustNewLinesOperation> nextOperation)
	{
		return nextOperation.Invoke();
	}

	public virtual AdjustSpacesOperation GetAdjustSpacesOperation(SyntaxToken previousToken, SyntaxToken currentToken, OptionSet optionSet, NextOperation<AdjustSpacesOperation> nextOperation)
	{
		return nextOperation.Invoke();
	}
}
