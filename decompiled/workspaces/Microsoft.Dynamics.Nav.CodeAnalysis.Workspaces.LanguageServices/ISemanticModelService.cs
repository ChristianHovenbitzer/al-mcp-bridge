using System.Threading;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Host;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.LanguageServices;

internal interface ISemanticModelService : IWorkspaceService
{
	Task<SemanticModel> GetSemanticModelForNodeAsync(Document document, SyntaxNode node, CancellationToken cancellationToken);
}
