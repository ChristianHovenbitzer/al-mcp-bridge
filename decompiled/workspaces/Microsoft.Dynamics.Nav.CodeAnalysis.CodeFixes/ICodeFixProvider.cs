using System.Collections.Immutable;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.CodeAnalysis.CodeActions;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.CodeFixes;

public interface ICodeFixProvider : ICodeActionProvider
{
	ImmutableArray<string> FixableDiagnosticIds { get; }

	Task RegisterCodeFixesAsync(CodeFixContext context);
}
