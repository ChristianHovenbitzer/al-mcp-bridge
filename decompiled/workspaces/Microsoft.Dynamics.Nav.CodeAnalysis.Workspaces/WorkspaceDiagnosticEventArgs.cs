using System;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces;

public class WorkspaceDiagnosticEventArgs : EventArgs
{
	public WorkspaceDiagnostic Diagnostic { get; }

	public WorkspaceDiagnosticEventArgs(WorkspaceDiagnostic diagnostic)
	{
		Diagnostic = diagnostic;
	}
}
