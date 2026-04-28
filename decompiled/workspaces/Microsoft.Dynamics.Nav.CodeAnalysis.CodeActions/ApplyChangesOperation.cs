using System;
using System.Threading;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.CodeActions;

public sealed class ApplyChangesOperation : CodeActionOperation
{
	public Solution ChangedSolution { get; }

	internal override bool ApplyDuringTests => true;

	public ApplyChangesOperation(Solution changedSolution)
	{
		ChangedSolution = changedSolution ?? throw new ArgumentNullException("changedSolution");
	}

	public override void Apply(Workspace workspace, CancellationToken cancellationToken)
	{
		TryApply(workspace, cancellationToken);
	}

	internal override bool TryApply(Workspace workspace, CancellationToken cancellationToken)
	{
		workspace.TryApplyChanges(ChangedSolution);
		return true;
	}
}
