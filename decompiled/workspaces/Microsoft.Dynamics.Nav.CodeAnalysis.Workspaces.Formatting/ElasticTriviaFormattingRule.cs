using System;
using System.Collections.Generic;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Formatting.Rules;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Options;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Formatting;

internal class ElasticTriviaFormattingRule : BaseFormattingRule
{
	internal const string Name = "AL Elastic trivia Formatting Rule";

	public override void AddSuppressOperations(List<SuppressOperation> list, SyntaxNode node, SyntaxToken lastToken, OptionSet optionSet, NextAction<SuppressOperation> nextOperation)
	{
		nextOperation.Invoke(list);
		_ = node.ContainsAnnotations;
	}

	public override AdjustNewLinesOperation? GetAdjustNewLinesOperation(SyntaxToken previousToken, SyntaxToken currentToken, OptionSet optionSet, NextOperation<AdjustNewLinesOperation> nextOperation)
	{
		AdjustNewLinesOperation adjustNewLinesOperation = nextOperation.Invoke();
		if (adjustNewLinesOperation == null)
		{
			return null;
		}
		if (adjustNewLinesOperation.Option == AdjustNewLinesOption.ForceLines)
		{
			return adjustNewLinesOperation;
		}
		if (!CommonFormattingHelpers.HasAnyWhitespaceElasticTrivia(previousToken, currentToken))
		{
			return adjustNewLinesOperation;
		}
		if (currentToken.Kind == SyntaxKind.EndKeyword)
		{
			AdjustNewLinesOperation adjustNewLinesOperationBetweenMembers = GetAdjustNewLinesOperationBetweenMembers(previousToken, currentToken);
			if (adjustNewLinesOperationBetweenMembers != null)
			{
				return adjustNewLinesOperationBetweenMembers;
			}
		}
		int num = Math.Max(LineBreaksAfter(previousToken, currentToken), adjustNewLinesOperation.Line);
		if (num == 0)
		{
			return CreateAdjustNewLinesOperation(0, AdjustNewLinesOption.PreserveLines);
		}
		return CreateAdjustNewLinesOperation(num, AdjustNewLinesOption.ForceLines);
	}

	private AdjustNewLinesOperation GetAdjustNewLinesOperationBetweenMembers(SyntaxToken previousToken, SyntaxToken currentToken)
	{
		return FormattingOperations.CreateAdjustNewLinesOperation(2, AdjustNewLinesOption.ForceLines);
	}

	public override AdjustSpacesOperation GetAdjustSpacesOperation(SyntaxToken previousToken, SyntaxToken currentToken, OptionSet optionSet, NextOperation<AdjustSpacesOperation> nextOperation)
	{
		AdjustSpacesOperation adjustSpacesOperation = nextOperation.Invoke();
		if (adjustSpacesOperation == null)
		{
			return null;
		}
		if (adjustSpacesOperation.Option == AdjustSpacesOption.ForceSpaces)
		{
			return adjustSpacesOperation;
		}
		if (CommonFormattingHelpers.HasAnyWhitespaceElasticTrivia(previousToken, currentToken))
		{
			return CreateAdjustSpacesOperation(Math.Max(0, adjustSpacesOperation.Space), AdjustSpacesOption.ForceSpaces);
		}
		return adjustSpacesOperation;
	}

	private int LineBreaksAfter(SyntaxToken previousToken, SyntaxToken currentToken)
	{
		if (currentToken.Kind == SyntaxKind.None)
		{
			return 0;
		}
		switch (previousToken.Kind)
		{
		case SyntaxKind.None:
			return 0;
		case SyntaxKind.OpenBraceToken:
			return 1;
		case SyntaxKind.CloseBraceToken:
			return LineBreaksAfterCloseBrace(currentToken);
		case SyntaxKind.CloseParenToken:
			if ((!(previousToken.Parent is StatementSyntax) || currentToken.Parent == previousToken.Parent) && currentToken.Kind != SyntaxKind.OpenBraceToken)
			{
				return 0;
			}
			return 1;
		case SyntaxKind.SemicolonToken:
			return LineBreaksAfterSemicolon(previousToken, currentToken);
		default:
			return 0;
		}
	}

	private static int LineBreaksAfterCloseBrace(SyntaxToken nextToken)
	{
		if (nextToken.Kind == SyntaxKind.EndOfFileToken || nextToken.Kind == SyntaxKind.CloseBraceToken)
		{
			return 0;
		}
		return 2;
	}

	private static int LineBreaksAfterSemicolon(SyntaxToken previousToken, SyntaxToken currentToken)
	{
		if (previousToken.Parent is ForStatementSyntax || currentToken.Kind == SyntaxKind.IdentifierToken)
		{
			return 0;
		}
		_ = currentToken.Kind;
		_ = 60;
		return 1;
	}
}
