namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces;

public class ProjectDiagnostic : WorkspaceDiagnostic
{
	public ProjectId ProjectId { get; }

	public ProjectDiagnostic(WorkspaceDiagnosticKind kind, string message, ProjectId projectId)
		: base(kind, message)
	{
		ProjectId = projectId;
	}
}
