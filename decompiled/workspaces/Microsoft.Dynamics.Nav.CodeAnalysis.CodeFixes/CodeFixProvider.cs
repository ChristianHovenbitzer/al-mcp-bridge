using System.Collections.Immutable;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.CodeAnalysis.CodeActions;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.CodeFixes;

public abstract class CodeFixProvider : ICodeFixProvider, ICodeActionProvider
{
	public abstract ImmutableArray<string> FixableDiagnosticIds { get; }

	internal CodeActionRequestPriority RequestPriority
	{
		get
		{
			CodeActionRequestPriority codeActionRequestPriority = ComputeRequestPriority();
			Contract.ThrowIfFalse(codeActionRequestPriority == CodeActionRequestPriority.Low || codeActionRequestPriority == CodeActionRequestPriority.Normal || codeActionRequestPriority == CodeActionRequestPriority.High);
			return codeActionRequestPriority;
		}
	}

	public abstract Task RegisterCodeFixesAsync(CodeFixContext context);

	public virtual FixAllProvider? GetFixAllProvider()
	{
		return null;
	}

	private protected virtual CodeActionRequestPriority ComputeRequestPriority()
	{
		return CodeActionRequestPriority.Normal;
	}
}
