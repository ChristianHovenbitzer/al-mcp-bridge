using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces;

internal static class StatementSyntaxExtensions
{
	public static StatementSyntax UpdateWithSemicolon(this StatementSyntax node)
	{
		return node.Kind switch
		{
			SyntaxKind.AssignmentStatement => ((AssignmentStatementSyntax)node).UpdateWithSemicolon(), 
			SyntaxKind.Block => ((BlockSyntax)node).UpdateWithSemicolon(), 
			SyntaxKind.IfStatement => ((IfStatementSyntax)node).UpdateWithSemicolon(), 
			SyntaxKind.ForStatement => ((ForStatementSyntax)node).UpdateWithSemicolon(), 
			SyntaxKind.WhileStatement => ((WhileStatementSyntax)node).UpdateWithSemicolon(), 
			SyntaxKind.AssertErrorStatement => ((AssertErrorStatementSyntax)node).UpdateWithSemicolon(), 
			SyntaxKind.EmptyStatement => ((EmptyStatementSyntax)node).UpdateWithSemicolon(), 
			SyntaxKind.BreakStatement => ((BreakStatementSyntax)node).UpdateWithSemicolon(), 
			SyntaxKind.ContinueStatement => ((ContinueStatementSyntax)node).UpdateWithSemicolon(), 
			SyntaxKind.ExitStatement => ((ExitStatementSyntax)node).UpdateWithSemicolon(), 
			SyntaxKind.CompoundAssignmentStatement => ((CompoundAssignmentStatementSyntax)node).UpdateWithSemicolon(), 
			SyntaxKind.CaseStatement => ((CaseStatementSyntax)node).UpdateWithSemicolon(), 
			SyntaxKind.WithStatement => ((WithStatementSyntax)node).UpdateWithSemicolon(), 
			SyntaxKind.ExpressionStatement => ((ExpressionStatementSyntax)node).UpdateWithSemicolon(), 
			SyntaxKind.RepeatStatement => ((RepeatStatementSyntax)node).UpdateWithSemicolon(), 
			SyntaxKind.ForEachStatement => ((ForEachStatementSyntax)node).UpdateWithSemicolon(), 
			SyntaxKind.OrphanedElseStatement => ((OrphanedElseStatementSyntax)node).UpdateWithSemicolon(), 
			_ => node, 
		};
	}

	public static StatementSyntax UpdateWithSemicolon(this AssignmentStatementSyntax node)
	{
		return node.Update(node.Target, node.AssignmentToken, node.Source, SyntaxFactory.Token(SyntaxKind.SemicolonToken));
	}

	public static StatementSyntax UpdateWithSemicolon(this BlockSyntax node)
	{
		return node.Update(node.BeginKeywordToken, node.Statements, node.EndKeywordToken, SyntaxFactory.Token(SyntaxKind.SemicolonToken));
	}

	public static StatementSyntax UpdateWithSemicolon(this IfStatementSyntax node)
	{
		return node.Update(node.IfKeywordToken, node.Condition, node.ThenKeywordToken, node.Statement, node.ElseKeywordToken, node.ElseStatement.UpdateWithSemicolon());
	}

	public static StatementSyntax UpdateWithSemicolon(this ForStatementSyntax node)
	{
		return node.Update(node.ForKeywordToken, node.LoopVariable, node.AssignToken, node.InitialValue, node.OperatorKeywordToken, node.EndValue, node.DoKeywordToken, node.Statement.UpdateWithSemicolon());
	}

	public static StatementSyntax UpdateWithSemicolon(this WhileStatementSyntax node)
	{
		return node.Update(node.WhileKeywordToken, node.Condition, node.DoKeywordToken, node.Statement.UpdateWithSemicolon());
	}

	public static StatementSyntax UpdateWithSemicolon(this AssertErrorStatementSyntax node)
	{
		return node.Update(node.AssertErrorKeywordToken, node.Statement.UpdateWithSemicolon());
	}

	public static StatementSyntax UpdateWithSemicolon(this EmptyStatementSyntax node)
	{
		return node.Update(SyntaxFactory.Token(SyntaxKind.SemicolonToken));
	}

	public static StatementSyntax UpdateWithSemicolon(this BreakStatementSyntax node)
	{
		return node.Update(node.BreakKeyword, SyntaxFactory.Token(SyntaxKind.SemicolonToken));
	}

	public static StatementSyntax UpdateWithSemicolon(this ContinueStatementSyntax node)
	{
		return node.Update(node.ContinueKeyword, SyntaxFactory.Token(SyntaxKind.SemicolonToken));
	}

	public static StatementSyntax UpdateWithSemicolon(this ExitStatementSyntax node)
	{
		return node.Update(node.ExitKeywordToken, node.OpenParenthesisToken, node.ExitValue, node.CloseParenthesisToken, SyntaxFactory.Token(SyntaxKind.SemicolonToken));
	}

	public static StatementSyntax UpdateWithSemicolon(this CompoundAssignmentStatementSyntax node)
	{
		return node.Update(node.Target, node.AssignmentToken, node.Source, SyntaxFactory.Token(SyntaxKind.SemicolonToken));
	}

	public static StatementSyntax UpdateWithSemicolon(this CaseStatementSyntax node)
	{
		return node.Update(node.CaseKeywordToken, node.Expression, node.OfKeywordToken, node.CaseLines, node.CaseElse, node.EndKeywordToken, SyntaxFactory.Token(SyntaxKind.SemicolonToken));
	}

	public static StatementSyntax UpdateWithSemicolon(this WithStatementSyntax node)
	{
		return node.Update(node.WithKeywordToken, node.WithId, node.DoKeywordToken, node.Statement.UpdateWithSemicolon());
	}

	public static StatementSyntax UpdateWithSemicolon(this ExpressionStatementSyntax node)
	{
		return node.Update(node.Expression, SyntaxFactory.Token(SyntaxKind.SemicolonToken));
	}

	public static StatementSyntax UpdateWithSemicolon(this RepeatStatementSyntax node)
	{
		return node.Update(node.RepeatKeywordToken, node.Statements, node.UntilKeywordToken, node.Condition, SyntaxFactory.Token(SyntaxKind.SemicolonToken));
	}

	public static StatementSyntax UpdateWithSemicolon(this ForEachStatementSyntax node)
	{
		return node.Update(node.ForEachKeywordToken, node.IterationVariable, node.InKeywordToken, node.Expression, node.DoKeywordToken, node.Statement.UpdateWithSemicolon());
	}

	public static StatementSyntax UpdateWithSemicolon(this OrphanedElseStatementSyntax node)
	{
		return node.Update(node.ElseKeywordToken, node.ElseStatement.UpdateWithSemicolon());
	}
}
