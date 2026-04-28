using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Host;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.CodeFixes;

internal interface ICodeFixService : IWorkspaceService
{
	Task<ImmutableArray<CodeFixCollection>> GetFixesAsync(Document document, TextSpan textSpan, CancellationToken cancellationToken);
}
