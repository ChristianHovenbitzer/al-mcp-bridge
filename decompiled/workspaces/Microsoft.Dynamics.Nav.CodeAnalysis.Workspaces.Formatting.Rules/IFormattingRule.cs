using System.Collections.Generic;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Options;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Formatting.Rules;

internal interface IFormattingRule
{
	void AddSuppressOperations(List<SuppressOperation> list, SyntaxNode node, SyntaxToken lastToken, OptionSet optionSet, NextAction<SuppressOperation> nextOperation);

	void AddAnchorIndentationOperations(List<AnchorIndentationOperation> list, SyntaxNode node, OptionSet optionSet, NextAction<AnchorIndentationOperation> nextOperation);

	void AddIndentBlockOperations(List<IndentBlockOperation> list, SyntaxNode node, OptionSet optionSet, NextAction<IndentBlockOperation> nextOperation);

	void AddAlignTokensOperations(List<AlignTokensOperation> list, SyntaxNode node, OptionSet optionSet, NextAction<AlignTokensOperation> nextOperation);

	AdjustNewLinesOperation? GetAdjustNewLinesOperation(SyntaxToken previousToken, SyntaxToken currentToken, OptionSet optionSet, NextOperation<AdjustNewLinesOperation> nextOperation);

	AdjustSpacesOperation GetAdjustSpacesOperation(SyntaxToken previousToken, SyntaxToken currentToken, OptionSet optionSet, NextOperation<AdjustSpacesOperation> nextOperation);
}
