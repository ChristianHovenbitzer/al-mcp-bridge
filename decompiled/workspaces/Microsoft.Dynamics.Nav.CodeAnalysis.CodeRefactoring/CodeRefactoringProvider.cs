using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.CodeAnalysis.CodeActions;
using Microsoft.Dynamics.Nav.CodeAnalysis.CodeFixes;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.CodeRefactoring;

public abstract class CodeRefactoringProvider : ICodeActionProvider
{
	public abstract Task ComputeRefactoringsAsync(CodeRefactoringContext context);

	internal virtual FixAllProvider? GetFixAllProvider()
	{
		return null;
	}
}
