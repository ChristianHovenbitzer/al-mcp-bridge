namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

internal static class InternalSyntaxTokenExtensions
{
	internal static bool IsKeywordKind(this InternalSyntaxToken token, SyntaxKind kind)
	{
		return token.ContextualKind == kind;
	}

	internal static bool IsKeywordKind(this InternalSyntaxToken token, SyntaxKind kind1, SyntaxKind kind2)
	{
		DebugAssertHelper.Assert(kind1.IsKeyword() && kind2.IsKeyword());
		if (token.ContextualKind != kind1)
		{
			return token.ContextualKind == kind2;
		}
		return true;
	}

	internal static bool IsKeywordKind(this InternalSyntaxToken token, SyntaxKind kind1, SyntaxKind kind2, SyntaxKind kind3)
	{
		DebugAssertHelper.Assert(kind1.IsKeyword() && kind2.IsKeyword() && kind3.IsKeyword());
		if (token.ContextualKind != kind1 && token.ContextualKind != kind2)
		{
			return token.ContextualKind == kind3;
		}
		return true;
	}

	internal static bool IsKeywordKind(this InternalSyntaxToken token, SyntaxKind kind1, SyntaxKind kind2, SyntaxKind kind3, SyntaxKind kind4)
	{
		DebugAssertHelper.Assert(kind1.IsKeyword() && kind2.IsKeyword() && kind3.IsKeyword() && kind4.IsKeyword());
		if (token.ContextualKind != kind1 && token.ContextualKind != kind2 && token.ContextualKind != kind3)
		{
			return token.ContextualKind == kind4;
		}
		return true;
	}

	internal static bool IsKeywordKind(this InternalSyntaxToken token, SyntaxKind kind1, SyntaxKind kind2, SyntaxKind kind3, SyntaxKind kind4, SyntaxKind kind5)
	{
		DebugAssertHelper.Assert(kind1.IsKeyword() && kind2.IsKeyword() && kind3.IsKeyword() && kind4.IsKeyword() && kind5.IsKeyword());
		if (token.ContextualKind != kind1 && token.ContextualKind != kind2 && token.ContextualKind != kind3 && token.ContextualKind != kind4)
		{
			return token.ContextualKind == kind5;
		}
		return true;
	}

	internal static bool IsKeywordKind(this InternalSyntaxToken token, SyntaxKind kind1, SyntaxKind kind2, SyntaxKind kind3, SyntaxKind kind4, SyntaxKind kind5, SyntaxKind kind6)
	{
		DebugAssertHelper.Assert(kind1.IsKeyword() && kind2.IsKeyword() && kind3.IsKeyword() && kind4.IsKeyword() && kind5.IsKeyword() && kind6.IsKeyword());
		if (token.ContextualKind != kind1 && token.ContextualKind != kind2 && token.ContextualKind != kind3 && token.ContextualKind != kind4 && token.ContextualKind != kind5)
		{
			return token.ContextualKind == kind6;
		}
		return true;
	}

	public static bool IsAllowedVariableName(this InternalSyntaxToken token)
	{
		if (token.ContextualKind != SyntaxKind.IdentifierToken)
		{
			if (token.ContextualKind.IsKeywordAllowedIdentifier())
			{
				return !token.IsKeywordKind(SyntaxKind.LocalKeyword, SyntaxKind.InternalKeyword, SyntaxKind.ProtectedKeyword);
			}
			return false;
		}
		return true;
	}

	public static bool IsMemberStart(this InternalSyntaxToken token)
	{
		return token.ContextualKind.IsMemberStartKeyword();
	}

	internal static bool IsPageChangeKeyword(this InternalSyntaxToken token)
	{
		return token.ContextualKind.IsPageChangeKeyword();
	}

	internal static bool IsPageActionOrGroupKeyword(this InternalSyntaxToken token)
	{
		return token.ContextualKind.IsPageActionOrGroupKeyword();
	}

	internal static bool IsPageControlOrGroupKeyword(this InternalSyntaxToken token)
	{
		return token.ContextualKind.IsPageControlOrGroupKeyword();
	}

	public static bool IsXmlPortNodeKeyword(this InternalSyntaxToken token)
	{
		return token.ContextualKind.IsXmlPortNodeKeyword();
	}

	public static bool IsObjectKeyword(this InternalSyntaxToken token)
	{
		return token.ContextualKind.IsObjectKeyword();
	}

	internal static bool IsKeywordAllowedIdentifier(this InternalSyntaxToken token)
	{
		return token.ContextualKind.IsKeywordAllowedIdentifier();
	}

	public static bool IsTokenIdentifier(this InternalSyntaxToken token)
	{
		if (token.ContextualKind != SyntaxKind.IdentifierToken)
		{
			return token.IsKeywordAllowedIdentifier();
		}
		return true;
	}

	internal static bool IsPossibleSignedLiteralToken(this InternalSyntaxToken token)
	{
		return token.ContextualKind.IsPossibleSignedLiteralToken();
	}

	internal static bool IsPossibleStatement(this InternalSyntaxToken token, bool parenthesisAsStartOfStatement)
	{
		switch (token.ContextualKind)
		{
		case SyntaxKind.IdentifierToken:
		case SyntaxKind.SemicolonToken:
		case SyntaxKind.ExitKeyword:
		case SyntaxKind.BeginKeyword:
		case SyntaxKind.CaseKeyword:
		case SyntaxKind.ElseKeyword:
		case SyntaxKind.ForKeyword:
		case SyntaxKind.IfKeyword:
		case SyntaxKind.RepeatKeyword:
		case SyntaxKind.UntilKeyword:
		case SyntaxKind.WithKeyword:
		case SyntaxKind.WhileKeyword:
		case SyntaxKind.AssertErrorKeyword:
		case SyntaxKind.ForEachKeyword:
		case SyntaxKind.BreakKeyword:
		case SyntaxKind.ThisKeyword:
			return true;
		case SyntaxKind.OpenParenToken:
			return parenthesisAsStartOfStatement;
		default:
			return token.ContextualKind.IsKeywordAllowedIdentifier();
		}
	}

	internal static bool IsPossibleMember(this InternalSyntaxToken token)
	{
		if (!token.IsPossibleProcedure())
		{
			return token.IsPossibleTrigger();
		}
		return true;
	}

	internal static bool IsPossibleTrigger(this InternalSyntaxToken token)
	{
		return token.IsKeywordKind(SyntaxKind.TriggerKeyword);
	}

	internal static bool IsPossibleProcedure(this InternalSyntaxToken token)
	{
		return token.IsKeywordKind(SyntaxKind.LocalKeyword, SyntaxKind.InternalKeyword, SyntaxKind.ProtectedKeyword, SyntaxKind.ProcedureKeyword);
	}
}
