using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Host;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.CodeActions;

internal abstract class CodeActionService : ILanguageService
{
	public static CodeActionService Create()
	{
		return new CodeActionServiceLoader().CreateCodeActionService();
	}

	public static CodeActionService GetService(Document document)
	{
		return document.Project.LanguageServices.GetService<CodeActionService>();
	}

	public abstract Task<ImmutableArray<CodeActionProxy>> GetCodeActionsAsync(Document document, TextSpan textSpan, CancellationToken cancellationToken);
}
