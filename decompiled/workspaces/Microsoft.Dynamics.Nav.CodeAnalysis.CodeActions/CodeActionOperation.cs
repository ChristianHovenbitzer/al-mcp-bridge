using System.Threading;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.CodeActions;

public abstract class CodeActionOperation
{
	public virtual string Title => null;

	internal virtual bool ApplyDuringTests => false;

	public virtual void Apply(Workspace workspace, CancellationToken cancellationToken)
	{
	}

	internal virtual bool TryApply(Workspace workspace, CancellationToken cancellationToken)
	{
		Apply(workspace, cancellationToken);
		return true;
	}
}
