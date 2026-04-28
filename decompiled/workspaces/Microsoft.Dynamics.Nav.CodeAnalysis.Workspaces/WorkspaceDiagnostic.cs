using System.Diagnostics;
using System.Globalization;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces;

[DebuggerDisplay("{GetDebuggerDisplay(),nq}")]
public class WorkspaceDiagnostic
{
	public WorkspaceDiagnosticKind Kind { get; }

	public string Message { get; }

	public WorkspaceDiagnostic(WorkspaceDiagnosticKind kind, string message)
	{
		Kind = kind;
		Message = message;
	}

	public override string ToString()
	{
		return GetDebuggerDisplay();
	}

	internal string GetDebuggerDisplay()
	{
		return string.Format(CultureInfo.InvariantCulture, "[{0}] {1}", Kind.ToString(), Message);
	}
}
