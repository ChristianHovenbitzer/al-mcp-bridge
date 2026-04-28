using System.Threading;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.CodeFixes;

internal interface IFixAllContext
{
	FixAllState State { get; }

	FixAllProvider FixAllProvider { get; }

	Solution Solution { get; }

	Project Project { get; }

	Document? Document { get; }

	object Provider { get; }

	FixAllScope Scope { get; }

	string? CodeActionEquivalenceKey { get; }

	CancellationToken CancellationToken { get; }

	string GetDefaultFixAllTitle();
}
