using System;
using System.Collections.Generic;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Formatting;

internal static class FormattingHelpers
{
	public const string NewLine = "\r\n";

	public static string GetIndent(this SyntaxToken token)
	{
		token.GetAllPrecedingTriviaToPreviousToken();
		string text = string.Empty;
		int num = text.LastIndexOf("\r\n", StringComparison.Ordinal);
		if (num != -1)
		{
			int num2 = num + "\r\n".Length;
			text = text.Substring(num2, text.Length - num2);
		}
		return text;
	}

	public static string ContentBeforeLastNewLine(this IEnumerable<SyntaxTrivia> trivia)
	{
		string text = trivia.AsString();
		int num = text.LastIndexOf("\r\n", StringComparison.Ordinal);
		if (num == -1)
		{
			return string.Empty;
		}
		return text.Substring(0, num);
	}

	public static (SyntaxToken Open, SyntaxToken Close) GetBracePair(this SyntaxNode node)
	{
		return node.GetBraces();
	}

	public static (SyntaxToken Open, SyntaxToken Close) GetScopeDelimiters(this SyntaxNode node)
	{
		if (node.IsKind(SyntaxKind.Block))
		{
			BlockSyntax blockSyntax = (BlockSyntax)node;
			return (Open: blockSyntax.BeginKeywordToken, Close: blockSyntax.EndKeywordToken);
		}
		return node.GetBracePair();
	}

	public static bool IsValidScopeDelimiterPair(this (SyntaxToken, SyntaxToken) bracePair)
	{
		if (bracePair.Item1.IsKind(SyntaxKind.None) || bracePair.Item1.IsMissing || bracePair.Item2.IsKind(SyntaxKind.None))
		{
			return false;
		}
		return true;
	}

	public static bool IsOpenParenInParameterList(this SyntaxToken token)
	{
		if (token.Kind == SyntaxKind.OpenParenToken)
		{
			return token.Parent.Kind == SyntaxKind.ParameterList;
		}
		return false;
	}

	public static bool IsCloseBraceOfExpression(this SyntaxToken token)
	{
		if (token.Kind != SyntaxKind.CloseBraceToken)
		{
			return false;
		}
		return token.Parent is ExpressionSyntax;
	}

	public static bool IsDotInMemberAccess(this SyntaxToken token)
	{
		if (!(token.Parent is MemberAccessExpressionSyntax memberAccessExpressionSyntax))
		{
			return false;
		}
		if (token.Kind == SyntaxKind.DotToken)
		{
			return memberAccessExpressionSyntax.DotToken.Equals(token);
		}
		return false;
	}

	public static bool IsCommaInAnyArgumentsList(this SyntaxToken token)
	{
		if (token.Kind == SyntaxKind.CommaToken)
		{
			return token.Parent.IsAnyArgumentList();
		}
		return false;
	}

	public static bool IsParenInParenthesizedExpression(this SyntaxToken token)
	{
		if (!(token.Parent is ParenthesizedExpressionSyntax { OpenParenthesisToken: var openParenthesisToken } parenthesizedExpressionSyntax))
		{
			return false;
		}
		if (!openParenthesisToken.Equals(token))
		{
			return parenthesizedExpressionSyntax.CloseParenthesisToken.Equals(token);
		}
		return true;
	}

	public static bool IsParenInArgumentList(this SyntaxToken token)
	{
		SyntaxNode parent = token.Parent;
		switch (parent.Kind)
		{
		case SyntaxKind.ExitStatement:
		{
			ExitStatementSyntax exitStatementSyntax = (ExitStatementSyntax)parent;
			if (!(exitStatementSyntax.OpenParenthesisToken == token))
			{
				return exitStatementSyntax.CloseParenthesisToken == token;
			}
			return true;
		}
		case SyntaxKind.ArgumentList:
		{
			ArgumentListSyntax argumentListSyntax = (ArgumentListSyntax)parent;
			if (!(argumentListSyntax.OpenParenthesisToken == token))
			{
				return argumentListSyntax.CloseParenthesisToken == token;
			}
			return true;
		}
		case SyntaxKind.AttributeArgumentList:
		{
			AttributeArgumentListSyntax attributeArgumentListSyntax = (AttributeArgumentListSyntax)parent;
			if (!(attributeArgumentListSyntax.OpenParenthesisToken == token))
			{
				return attributeArgumentListSyntax.CloseParenthesisToken == token;
			}
			return true;
		}
		default:
			return false;
		}
	}

	public static bool IsCloseParenInStatement(this SyntaxToken token)
	{
		_ = token.Parent is StatementSyntax;
		return false;
	}

	public static bool ParenOrBracketContainsNothing(this SyntaxToken token1, SyntaxToken token2)
	{
		if (token1.Kind != SyntaxKind.OpenParenToken || token2.Kind != SyntaxKind.CloseParenToken)
		{
			if (token1.Kind == SyntaxKind.OpenBracketToken)
			{
				return token2.Kind == SyntaxKind.CloseBracketToken;
			}
			return false;
		}
		return true;
	}

	public static bool IsPlusOrMinusExpression(this SyntaxToken token)
	{
		SyntaxKind kind = token.Kind;
		if (kind - 17 <= SyntaxKind.EmptyToken)
		{
			return token.Parent is UnaryExpressionSyntax;
		}
		return false;
	}

	public static bool IsPlusOrMinusLiteralValue(this SyntaxToken token)
	{
		SyntaxKind kind = token.Kind;
		if (kind - 17 <= SyntaxKind.EmptyToken)
		{
			if (token.Parent.Kind != SyntaxKind.Int32SignedLiteralValue && token.Parent.Kind != SyntaxKind.Int64SignedLiteralValue)
			{
				return token.Parent.Kind == SyntaxKind.DecimalSignedLiteralValue;
			}
			return true;
		}
		return false;
	}
}
