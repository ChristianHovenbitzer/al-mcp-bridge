using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.LanguageServices;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Shared.Extensions;

internal static class ISyntaxFactsServiceExtensions
{
	public static bool IsWord(this ISyntaxFactsService syntaxFacts, SyntaxToken token)
	{
		if (!syntaxFacts.IsIdentifier(token))
		{
			return syntaxFacts.IsKeyword(token);
		}
		return true;
	}
}
