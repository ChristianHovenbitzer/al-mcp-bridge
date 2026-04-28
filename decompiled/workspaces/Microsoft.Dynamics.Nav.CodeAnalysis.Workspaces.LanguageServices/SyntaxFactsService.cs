using System;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Host;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Shared.Extensions;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.LanguageServices;

internal class SyntaxFactsService : ISyntaxFactsService, ILanguageService
{
	internal static readonly SyntaxFactsService Instance = new SyntaxFactsService();

	public bool IsCaseSensitive => false;

	private SyntaxFactsService()
	{
	}

	public SyntaxNode GetBindableParent(SyntaxToken token)
	{
		SyntaxNode syntaxNode = token.Parent;
		while (syntaxNode != null)
		{
			SyntaxNode parent = syntaxNode.Parent;
			if (parent.Kind == SyntaxKind.QualifiedName || (parent is MemberAccessExpressionSyntax memberAccessExpressionSyntax && memberAccessExpressionSyntax.Expression == syntaxNode) || !(parent is NameSyntax))
			{
				break;
			}
			syntaxNode = parent;
		}
		return syntaxNode;
	}

	public SyntaxNode GetContainingMemberDeclaration(SyntaxNode root, int position, bool useFullSpan = true)
	{
		throw new NotImplementedException();
	}

	public SyntaxNode GetMethodLevelMember(SyntaxNode root, int memberId)
	{
		throw new NotImplementedException();
	}

	public int GetMethodLevelMemberId(SyntaxNode root, SyntaxNode node)
	{
		throw new NotImplementedException();
	}

	public bool IsBindableToken(SyntaxToken token)
	{
		if (this.IsWord(token) || IsLiteral(token))
		{
			return !IsOperatorWordToken(token);
		}
		return false;
	}

	public bool IsIdentifier(SyntaxToken token)
	{
		if (!token.IsKind(SyntaxKind.IdentifierToken))
		{
			return token.Parent?.ParentIsKind(SyntaxKind.ObjectReference) ?? false;
		}
		return true;
	}

	public bool IsKeyword(SyntaxToken token)
	{
		return token.Kind.IsKeyword();
	}

	private static bool IsOperatorWordToken(SyntaxToken token)
	{
		SyntaxKind kind = token.Kind;
		if (kind - 20 <= SyntaxKind.EmptyToken || kind - 61 <= SyntaxKind.Int32LiteralToken || kind == SyntaxKind.InKeyword)
		{
			return true;
		}
		return false;
	}

	public bool IsLiteral(SyntaxToken token)
	{
		SyntaxKind kind = token.Kind;
		if (kind - 2 <= SyntaxKind.TimeLiteralToken || kind - 14 <= SyntaxKind.EmptyToken)
		{
			return true;
		}
		return false;
	}

	public bool IsMethodLevelMember(SyntaxNode node)
	{
		throw new NotImplementedException();
	}
}
