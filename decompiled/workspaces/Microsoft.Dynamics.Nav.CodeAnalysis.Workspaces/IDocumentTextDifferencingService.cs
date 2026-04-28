using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Host;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces;

internal interface IDocumentTextDifferencingService : IWorkspaceService
{
	Task<ImmutableArray<TextChange>> GetTextChangesAsync(Document oldDocument, Document newDocument, CancellationToken cancellationToken);
}
