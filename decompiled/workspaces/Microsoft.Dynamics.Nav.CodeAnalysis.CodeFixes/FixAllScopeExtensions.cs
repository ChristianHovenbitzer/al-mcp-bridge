using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.CodeFixes;

public static class FixAllScopeExtensions
{
	public static string ToDisplayString(this FixAllScope scope)
	{
		switch (scope)
		{
		case FixAllScope.Project:
			return WorkspacesResources.TermProject;
		case FixAllScope.Document:
			return WorkspacesResources.TermDocument;
		case FixAllScope.Workspace:
			return WorkspacesResources.TermWorkspace;
		default:
			DebugAssertHelper.Fail($"Unhandled case {scope}");
			return string.Empty;
		}
	}
}
