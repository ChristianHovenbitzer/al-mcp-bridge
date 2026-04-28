using System.Collections.Generic;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Formatting.Rules;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Options;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Formatting;

internal class WrappingFormattingRule : BaseFormattingRule
{
	public override void AddSuppressOperations(List<SuppressOperation> list, SyntaxNode node, SyntaxToken lastToken, OptionSet optionSet, NextAction<SuppressOperation> nextOperation)
	{
		nextOperation.Invoke(list);
		AddBraceSuppressOperations(list, node, lastToken);
		AddStatementExceptBlockSuppressOperations(list, node);
		if (!optionSet.GetOption(ALFormattingOptions.WrappingPreserveSingleLine))
		{
			RemoveSuppressOperationForBlock(list, node);
		}
		optionSet.GetOption(ALFormattingOptions.WrappingKeepStatementsOnSingleLine);
	}

	protected void RemoveSuppressOperation(List<SuppressOperation> list, SyntaxToken startToken, SyntaxToken endToken)
	{
		if (startToken.Kind == SyntaxKind.None || endToken.Kind == SyntaxKind.None)
		{
			return;
		}
		TextSpan textSpan = TextSpan.FromBounds(startToken.SpanStart, endToken.Span.End);
		for (int i = 0; i < list.Count; i++)
		{
			if (list[i] != null && list[i].TextSpan.Start >= textSpan.Start && list[i].TextSpan.End <= textSpan.End)
			{
				list[i] = null;
			}
		}
	}

	private void AddStatementExceptBlockSuppressOperations(List<SuppressOperation> list, SyntaxNode node)
	{
		if (node is StatementSyntax { Kind: not SyntaxKind.Block, Kind: not SyntaxKind.CaseStatement } statementSyntax && statementSyntax.Parent.Kind != SyntaxKind.CaseLine)
		{
			SyntaxToken firstToken = statementSyntax.GetFirstToken(includeZeroWidth: true);
			SyntaxToken lastToken = statementSyntax.GetLastToken(includeZeroWidth: true);
			AddSuppressWrappingIfOnSingleLineOperation(list, firstToken, lastToken);
		}
	}

	private void RemoveSuppressOperationForBlock(List<SuppressOperation> list, SyntaxNode node)
	{
		(SyntaxToken, SyntaxToken) scopeDelimiters = node.GetScopeDelimiters();
		if (scopeDelimiters.IsValidScopeDelimiterPair())
		{
			SyntaxToken firstToken = node.GetFirstToken(includeZeroWidth: true);
			RemoveSuppressOperation(list, firstToken, scopeDelimiters.Item2);
			RemoveSuppressOperation(list, scopeDelimiters.Item1, scopeDelimiters.Item2);
		}
	}
}
