using System;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces;

public sealed class WorkspaceRegistration
{
	public Workspace Workspace { get; private set; }

	public event EventHandler WorkspaceChanged;

	internal WorkspaceRegistration()
	{
	}

	internal void SetWorkspaceAndRaiseEvents(Workspace workspace)
	{
		Workspace = workspace;
		this.WorkspaceChanged?.Invoke(this, EventArgs.Empty);
	}
}
