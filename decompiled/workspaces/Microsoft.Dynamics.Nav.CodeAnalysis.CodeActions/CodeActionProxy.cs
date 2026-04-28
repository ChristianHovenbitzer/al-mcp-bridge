using System;
using System.Collections.Immutable;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.CodeActions;

internal class CodeActionProxy
{
	public CodeAction CodeAction { get; }

	public CodeAction ParentCodeAction { get; }

	public CodeActionProxy(CodeAction codeAction, CodeAction parentCodeAction = null)
	{
		CodeAction = codeAction ?? throw new ArgumentNullException("codeAction");
		ParentCodeAction = parentCodeAction;
	}

	public string GetIdentifier()
	{
		return CodeAction.EquivalenceKey ?? GetTitle();
	}

	public string GetTitle()
	{
		if (ParentCodeAction != null)
		{
			return string.Format(CultureInfo.CurrentCulture, WorkspacesResources.CodeActionCommandTitle, ParentCodeAction.Title.TrimEnd('.'), CodeAction.Title);
		}
		return CodeAction.Title;
	}

	public bool IsPreferred()
	{
		return CodeAction.IsPreferred;
	}

	public CodeActionKind GetKind()
	{
		return CodeAction.Kind;
	}

	public Task<ImmutableArray<CodeActionOperation>> GetOperationsAsync(CancellationToken cancellationToken)
	{
		return CodeAction.GetOperationsAsync(cancellationToken);
	}
}
