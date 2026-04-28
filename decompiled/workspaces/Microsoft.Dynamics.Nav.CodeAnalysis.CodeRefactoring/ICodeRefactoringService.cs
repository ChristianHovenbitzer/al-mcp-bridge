using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.CodeRefactoring;

internal interface ICodeRefactoringService
{
	Task<ImmutableArray<CodeRefactoring>> GetRefactoringsAsync(Document document, TextSpan textSpan, CancellationToken cancellationToken);
}
