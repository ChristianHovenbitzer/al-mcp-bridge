using System.Collections.Generic;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Formatting.Rules;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Options;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Formatting;

internal class AnchorIndentationFormattingRule : BaseFormattingRule
{
	internal const string Name = "AL Anchor Indentation Formatting Rule";

	public override void AddAnchorIndentationOperations(List<AnchorIndentationOperation> list, SyntaxNode node, OptionSet optionSet, NextAction<AnchorIndentationOperation> nextOperation)
	{
		nextOperation.Invoke(list);
		if (node is BlockSyntax blockSyntax)
		{
			if (blockSyntax.Parent == null || blockSyntax.Parent is BlockSyntax)
			{
				AddAnchorIndentationOperation(list, blockSyntax);
			}
			else
			{
				AddAnchorIndentationOperationBasedOnParent(list, blockSyntax);
			}
			return;
		}
		if (node.Kind.IsStatementSyntax())
		{
			AddAnchorIndentationOperation(list, node);
		}
		if (node.Kind == SyntaxKind.ParameterList)
		{
			AddAnchorIndentationOperationBasedOnParent(list, node);
		}
	}

	private void AddAnchorIndentationOperation(List<AnchorIndentationOperation> list, SyntaxNode node)
	{
		AddAnchorIndentationOperation(list, node.GetFirstToken(includeZeroWidth: true), node.GetLastToken(includeZeroWidth: true));
	}

	private void AddAnchorIndentationOperationBasedOnParent(List<AnchorIndentationOperation> list, SyntaxNode node)
	{
		AddAnchorIndentationOperation(list, node.Parent.GetFirstToken(includeZeroWidth: true), node.GetLastToken(includeZeroWidth: true));
	}
}
