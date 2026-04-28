using System.Collections.Immutable;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.CodeFixes;

internal class CodeFixCollection
{
	public object Provider { get; }

	public TextSpan TextSpan { get; }

	public ImmutableArray<CodeFix> Fixes { get; }

	public Diagnostic FirstDiagnostic { get; }

	public CodeFixCollection(object provider, TextSpan span, ImmutableArray<CodeFix> fixes, Diagnostic firstDiagnostic)
	{
		Provider = provider;
		TextSpan = span;
		Fixes = fixes.NullToEmpty();
		FirstDiagnostic = firstDiagnostic;
	}
}
