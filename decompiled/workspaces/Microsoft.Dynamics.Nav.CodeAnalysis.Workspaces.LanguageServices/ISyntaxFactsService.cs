using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Host;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.LanguageServices;

internal interface ISyntaxFactsService : ILanguageService
{
	bool IsCaseSensitive { get; }

	bool IsIdentifier(SyntaxToken token);

	bool IsKeyword(SyntaxToken token);

	bool IsLiteral(SyntaxToken token);

	bool IsBindableToken(SyntaxToken token);

	bool IsMethodLevelMember(SyntaxNode node);

	SyntaxNode GetContainingMemberDeclaration(SyntaxNode root, int position, bool useFullSpan = true);

	int GetMethodLevelMemberId(SyntaxNode root, SyntaxNode node);

	SyntaxNode GetMethodLevelMember(SyntaxNode root, int memberId);

	SyntaxNode GetBindableParent(SyntaxToken token);
}
