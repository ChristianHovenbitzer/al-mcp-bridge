using System.Threading;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Host;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.LanguageServices;

internal interface ISemanticFactsService : ILanguageService
{
	ISymbol GetDeclaredSymbol(SemanticModel semanticModel, SyntaxToken token, CancellationToken cancellationToken);
}
