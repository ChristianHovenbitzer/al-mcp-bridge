using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Formatting.Rules;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Options;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Formatting;

internal class TokenBasedFormattingRule : BaseFormattingRule
{
	internal const string Name = "AL Token Based Formatting Rule";

	public override AdjustNewLinesOperation? GetAdjustNewLinesOperation(SyntaxToken previousToken, SyntaxToken currentToken, OptionSet optionSet, NextOperation<AdjustNewLinesOperation> nextOperation)
	{
		switch (currentToken.Kind)
		{
		case SyntaxKind.OpenBraceToken:
			if (!previousToken.IsParenInParenthesizedExpression())
			{
				return CreateAdjustNewLinesOperation(1, AdjustNewLinesOption.PreserveLines);
			}
			break;
		case SyntaxKind.CloseBraceToken:
			return CreateAdjustNewLinesOperation(1, AdjustNewLinesOption.PreserveLines);
		}
		if (previousToken.Kind == SyntaxKind.CloseBraceToken && currentToken.Kind == SyntaxKind.WhileKeyword)
		{
			return CreateAdjustNewLinesOperation(0, AdjustNewLinesOption.PreserveLines);
		}
		switch (previousToken.Kind)
		{
		case SyntaxKind.CloseBraceToken:
			if (!previousToken.IsCloseBraceOfExpression() && !currentToken.IsKind(SyntaxKind.SemicolonToken) && !currentToken.IsParenInParenthesizedExpression() && !currentToken.IsCommaInAnyArgumentsList() && !currentToken.IsParenInArgumentList() && !currentToken.IsDotInMemberAccess() && !currentToken.IsCloseParenInStatement())
			{
				return CreateAdjustNewLinesOperation(1, AdjustNewLinesOption.PreserveLines);
			}
			break;
		case SyntaxKind.OpenBraceToken:
			return CreateAdjustNewLinesOperation(1, AdjustNewLinesOption.PreserveLines);
		}
		if ((currentToken.IsGlobalVarSectionVarKeyword() && !previousToken.IsGlobalVarSectionAccessModifier()) || currentToken.IsVarSectionVarKeyword())
		{
			int line = 1;
			if (currentToken.IsGlobalVarSectionVarKeyword() && !previousToken.IsStartOfCurlyBraceBlock())
			{
				line = 2;
			}
			return CreateAdjustNewLinesOperation(line, AdjustNewLinesOption.PreserveLines);
		}
		if (previousToken.IsVarSectionVarKeyword() || previousToken.IsGlobalVarSectionVarKeyword())
		{
			return CreateAdjustNewLinesOperation(1, AdjustNewLinesOption.PreserveLines);
		}
		if (previousToken.Kind == SyntaxKind.RepeatKeyword && currentToken.Kind != SyntaxKind.BeginKeyword)
		{
			return CreateAdjustNewLinesOperation(1, AdjustNewLinesOption.PreserveLines);
		}
		if (previousToken.IsKind(SyntaxKind.SemicolonToken) && previousToken.Parent.Kind != SyntaxKind.ParameterList)
		{
			int line2 = 0;
			if (previousToken.Parent.Kind.IsStatementSyntax())
			{
				line2 = 1;
			}
			switch (previousToken.Parent.Kind)
			{
			case SyntaxKind.TriggerDeclaration:
			case SyntaxKind.MethodDeclaration:
			case SyntaxKind.VariableDeclaration:
			case SyntaxKind.Property:
				line2 = 1;
				if (previousToken.IsLastTokenOfGlobalVarSection())
				{
					line2 = 2;
				}
				break;
			case SyntaxKind.Block:
			{
				SyntaxNode parent = previousToken.Parent.Parent;
				line2 = 1;
				if (parent.IsScopeMember())
				{
					line2 = 2;
				}
				break;
			}
			}
			return CreateAdjustNewLinesOperation(line2, AdjustNewLinesOption.PreserveLines);
		}
		if (previousToken.IsKind(SyntaxKind.CloseParenToken) && (previousToken.IsParentPageMoveChange() || previousToken.IsParentLabelRename()))
		{
			return CreateAdjustNewLinesOperation(1, AdjustNewLinesOption.PreserveLines);
		}
		if (currentToken.IsKind(SyntaxKind.BeginKeyword))
		{
			if (!previousToken.IsKind(SyntaxKind.ThenKeyword, SyntaxKind.ElseKeyword, SyntaxKind.RepeatKeyword, SyntaxKind.DoKeyword))
			{
				return CreateAdjustNewLinesOperation(1, AdjustNewLinesOption.PreserveLines);
			}
			if (previousToken.IsKind(SyntaxKind.RepeatKeyword))
			{
				return CreateAdjustNewLinesOperation(0, AdjustNewLinesOption.PreserveLines);
			}
		}
		if (currentToken.IsKind(SyntaxKind.ElseKeyword))
		{
			return CreateAdjustNewLinesOperation(0, AdjustNewLinesOption.PreserveLines);
		}
		if (previousToken.IsKind(SyntaxKind.ElseKeyword) && currentToken.IsKind(SyntaxKind.IfKeyword))
		{
			if (previousToken.IsOnTheSameLineAs(currentToken))
			{
				return CreateAdjustNewLinesOperation(0, AdjustNewLinesOption.PreserveLines);
			}
			return CreateAdjustNewLinesOperation(1, AdjustNewLinesOption.PreserveLines);
		}
		if (previousToken.IsKind(SyntaxKind.ElseKeyword) && !currentToken.IsKind(SyntaxKind.BeginKeyword))
		{
			return CreateAdjustNewLinesOperation(1, AdjustNewLinesOption.ForceLines);
		}
		if (previousToken.IsKind(SyntaxKind.ThenKeyword) && !currentToken.IsKind(SyntaxKind.BeginKeyword) && !currentToken.IsKind(SyntaxKind.SemicolonToken))
		{
			return CreateAdjustNewLinesOperation(1, AdjustNewLinesOption.ForceLines);
		}
		if (previousToken.IsKind(SyntaxKind.DoKeyword) && !currentToken.IsKind(SyntaxKind.BeginKeyword))
		{
			return CreateAdjustNewLinesOperation(0, AdjustNewLinesOption.PreserveLines);
		}
		if (previousToken.IsKind(SyntaxKind.ColonToken) && previousToken.Parent.Kind == SyntaxKind.CaseLine)
		{
			return CreateAdjustNewLinesOperation(1, AdjustNewLinesOption.PreserveLines);
		}
		if (previousToken.IsKind(SyntaxKind.OfKeyword) && previousToken.Parent.IsKind(SyntaxKind.CaseStatement))
		{
			return CreateAdjustNewLinesOperation(1, AdjustNewLinesOption.PreserveLines);
		}
		if (previousToken.IsKind(SyntaxKind.BeginKeyword))
		{
			return CreateAdjustNewLinesOperation(1, AdjustNewLinesOption.PreserveLines);
		}
		if (currentToken.IsKind(SyntaxKind.EndKeyword))
		{
			return CreateAdjustNewLinesOperation(1, AdjustNewLinesOption.PreserveLines);
		}
		if (currentToken.IsKind(SyntaxKind.UntilKeyword) && previousToken.IsKind(SyntaxKind.EndKeyword))
		{
			return CreateAdjustNewLinesOperation(0, AdjustNewLinesOption.PreserveLines);
		}
		if (previousToken.IsAfterAttribute())
		{
			return CreateAdjustNewLinesOperation(1, AdjustNewLinesOption.PreserveLines);
		}
		return nextOperation.Invoke();
	}

	public override AdjustSpacesOperation GetAdjustSpacesOperation(SyntaxToken previousToken, SyntaxToken currentToken, OptionSet optionSet, NextOperation<AdjustSpacesOperation> nextOperation)
	{
		if (currentToken.Kind == SyntaxKind.SemicolonToken)
		{
			if (previousToken.Kind == SyntaxKind.SemicolonToken)
			{
				return CreateAdjustSpacesOperation(1, AdjustSpacesOption.ForceSpacesIfOnSingleLine);
			}
			return CreateAdjustSpacesOperation(0, AdjustSpacesOption.ForceSpacesIfOnSingleLine);
		}
		if (currentToken.Kind == SyntaxKind.OpenParenToken && IsMethodIdentifier(previousToken, currentToken))
		{
			return CreateAdjustSpacesOperation(0, AdjustSpacesOption.ForceSpacesIfOnSingleLine);
		}
		if (previousToken.ParenOrBracketContainsNothing(currentToken))
		{
			return CreateAdjustSpacesOperation(0, AdjustSpacesOption.ForceSpacesIfOnSingleLine);
		}
		SyntaxKind kind = currentToken.Kind;
		if (kind - 33 <= SyntaxKind.EmptyToken || kind == SyntaxKind.CloseParenToken || kind == SyntaxKind.CloseBracketToken)
		{
			return CreateAdjustSpacesOperation(0, AdjustSpacesOption.ForceSpacesIfOnSingleLine);
		}
		if (IsCommaInOptionValuesList(previousToken))
		{
			return CreateAdjustSpacesOperation(0, AdjustSpacesOption.ForceSpaces);
		}
		if (currentToken.IsKind(SyntaxKind.OpenBracketToken))
		{
			if (previousToken.IsKind(SyntaxKind.ArrayKeyword))
			{
				return CreateAdjustSpacesOperation(0, AdjustSpacesOption.ForceSpaces);
			}
			if (previousToken.IsKind(SyntaxKind.IdentifierToken))
			{
				return CreateAdjustSpacesOperation(0, AdjustSpacesOption.ForceSpaces);
			}
		}
		if (currentToken.IsKind(SyntaxKind.ColonToken))
		{
			kind = currentToken.Parent.Kind;
			if (kind == SyntaxKind.CaseLine || kind - 315 <= SyntaxKind.EmptyToken || kind == SyntaxKind.ReturnValue)
			{
				return CreateAdjustSpacesOperation(0, AdjustSpacesOption.ForceSpacesIfOnSingleLine);
			}
		}
		switch (previousToken.Kind)
		{
		case SyntaxKind.DotToken:
		case SyntaxKind.OpenParenToken:
		case SyntaxKind.OpenBracketToken:
			return CreateAdjustSpacesOperation(0, AdjustSpacesOption.ForceSpacesIfOnSingleLine);
		case SyntaxKind.CloseParenToken:
		case SyntaxKind.CloseBracketToken:
		{
			int space = ((previousToken.Kind != currentToken.Kind) ? 1 : 0);
			return CreateAdjustSpacesOperation(space, AdjustSpacesOption.ForceSpacesIfOnSingleLine);
		}
		default:
			if (previousToken.IsPlusOrMinusExpression() && !currentToken.IsPlusOrMinusExpression())
			{
				return CreateAdjustSpacesOperation(0, AdjustSpacesOption.ForceSpacesIfOnSingleLine);
			}
			if (previousToken.IsPlusOrMinusExpression() && currentToken.IsPlusOrMinusExpression() && previousToken.Kind != currentToken.Kind)
			{
				return CreateAdjustSpacesOperation(0, AdjustSpacesOption.ForceSpacesIfOnSingleLine);
			}
			if (previousToken.IsPlusOrMinusLiteralValue())
			{
				return CreateAdjustSpacesOperation(0, AdjustSpacesOption.ForceSpacesIfOnSingleLine);
			}
			if (previousToken.Kind == SyntaxKind.NotKeyword)
			{
				return CreateAdjustSpacesOperation(1, AdjustSpacesOption.ForceSpacesIfOnSingleLine);
			}
			if (previousToken.Kind == SyntaxKind.ColonColonToken || currentToken.Kind == SyntaxKind.ColonColonToken)
			{
				return CreateAdjustSpacesOperation(0, AdjustSpacesOption.ForceSpacesIfOnSingleLine);
			}
			if (previousToken.IsKind(SyntaxKind.ThenKeyword) && currentToken.IsKind(SyntaxKind.BeginKeyword))
			{
				return CreateAdjustSpacesOperation(1, AdjustSpacesOption.ForceSpaces);
			}
			if (previousToken.IsKind(SyntaxKind.ProtectedKeyword) && currentToken.IsKind(SyntaxKind.VarKeyword))
			{
				return CreateAdjustSpacesOperation(1, AdjustSpacesOption.ForceSpaces);
			}
			if (previousToken.IsKind(SyntaxKind.ElseKeyword) && currentToken.IsKind(SyntaxKind.IfKeyword, SyntaxKind.BeginKeyword))
			{
				return CreateAdjustSpacesOperation(1, AdjustSpacesOption.ForceSpaces);
			}
			if (previousToken.IsKind(SyntaxKind.DoKeyword) && currentToken.IsKind(SyntaxKind.BeginKeyword))
			{
				return CreateAdjustSpacesOperation(1, AdjustSpacesOption.ForceSpaces);
			}
			if (previousToken.Kind.IsControlKeyword() || previousToken.Kind.IsLogicalOperator())
			{
				return CreateAdjustSpacesOperation(1, AdjustSpacesOption.ForceSpacesIfOnSingleLine);
			}
			return nextOperation.Invoke();
		}
	}

	private static bool IsCommaInOptionValuesList(SyntaxToken previousToken)
	{
		if (previousToken.Kind == SyntaxKind.CommaToken)
		{
			return previousToken.ParentIsKind(SyntaxKind.OptionValues);
		}
		return false;
	}

	private static bool IsMethodIdentifier(SyntaxToken previousToken, SyntaxToken currentToken)
	{
		if (previousToken.Kind != SyntaxKind.IdentifierToken && !currentToken.IsParenInArgumentList())
		{
			if (previousToken.Kind.IsKeyword() && !previousToken.Kind.IsControlKeyword())
			{
				return !previousToken.IsOperatorToken();
			}
			return false;
		}
		return true;
	}
}
