using System;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces;

public class WorkspaceChangeEventArgs : EventArgs
{
	public WorkspaceChangeKind Kind { get; }

	public Solution OldSolution { get; }

	public Solution NewSolution { get; }

	public ProjectId ProjectId { get; }

	public DocumentId DocumentId { get; }

	public bool HandleRad { get; }

	public WorkspaceChangeEventArgs(WorkspaceChangeKind kind, Solution oldSolution, Solution newSolution, ProjectId projectId = null, DocumentId documentId = null, bool handleRad = false)
	{
		Kind = kind;
		OldSolution = oldSolution;
		NewSolution = newSolution;
		ProjectId = projectId;
		DocumentId = documentId;
		HandleRad = handleRad;
	}
}
