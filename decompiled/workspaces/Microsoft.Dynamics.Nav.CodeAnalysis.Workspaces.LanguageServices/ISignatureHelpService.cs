using System.Threading;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Host;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.SignatureHelp;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.LanguageServices;

internal interface ISignatureHelpService : ILanguageService
{
	Task<SignatureHelpResult<SyntaxSignature>> ProvideSyntacticHelpAsync(Document document, int position, CancellationToken cancellationToken);

	Task<SignatureHelpResult<SymbolSignature>> ProvideSymbolicHelpAsync(Document document, int position, CancellationToken cancellationToken);

	Task<int?> GetActiveParameterAsync(Document document, int position, CancellationToken cancellationToken);
}
