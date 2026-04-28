using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces;

internal static class ExpressionSyntaxExtensions
{
	internal static ExpressionSyntax WalkUpParentheses(this ExpressionSyntax expression)
	{
		while (expression.ParentIsKind(SyntaxKind.ParenthesizedExpression))
		{
			expression = (ExpressionSyntax)expression.Parent;
		}
		return expression;
	}

	internal static ExpressionSyntax WalkDownParentheses(this ExpressionSyntax expression)
	{
		while (expression.IsKind(SyntaxKind.ParenthesizedExpression))
		{
			expression = ((ParenthesizedExpressionSyntax)expression).Expression;
		}
		return expression;
	}

	public static bool IsRightSideOfDotOrColonColon(this ExpressionSyntax name)
	{
		if (!name.IsRightSideOfDot())
		{
			return name.IsRightSideOfColonColon();
		}
		return true;
	}

	public static bool IsRightSideOfDot(this ExpressionSyntax name)
	{
		return name.IsMemberAccessExpressionName();
	}

	public static bool IsMemberAccessExpressionName(this ExpressionSyntax expression)
	{
		if (expression.ParentIsKind(SyntaxKind.MemberAccessExpression))
		{
			return ((MemberAccessExpressionSyntax)expression.Parent).Name == expression;
		}
		return false;
	}

	public static bool IsRightSideOfColonColon(this ExpressionSyntax expression)
	{
		if (expression.ParentIsKind(SyntaxKind.OptionAccessExpression))
		{
			return ((OptionAccessExpressionSyntax)expression.Parent).Name == expression;
		}
		return false;
	}
}
