using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.Dynamics.Nav.CodeAnalysis.CodeActions;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.CodeFixes;

public struct CodeFixContext
{
	private readonly Action<CodeAction, ImmutableArray<Diagnostic>> registerCodeFix;

	public Document Document { get; }

	internal Project Project { get; }

	public TextSpan Span { get; }

	public ImmutableArray<Diagnostic> Diagnostics { get; }

	public CancellationToken CancellationToken { get; }

	public CodeFixContext(Document document, TextSpan span, ImmutableArray<Diagnostic> diagnostics, Action<CodeAction, ImmutableArray<Diagnostic>> registerCodeFix, CancellationToken cancellationToken)
		: this(document, span, diagnostics, registerCodeFix, verifyArguments: true, cancellationToken)
	{
	}

	public CodeFixContext(Document document, Diagnostic diagnostic, Action<CodeAction, ImmutableArray<Diagnostic>> registerCodeFix, CancellationToken cancellationToken)
		: this(document, diagnostic.Location.SourceSpan, ImmutableArray.Create(diagnostic), registerCodeFix, verifyArguments: true, cancellationToken)
	{
	}

	internal CodeFixContext(Document document, TextSpan span, ImmutableArray<Diagnostic> diagnostics, Action<CodeAction, ImmutableArray<Diagnostic>> registerCodeFix, bool verifyArguments, CancellationToken cancellationToken)
		: this(document, document.Project, span, diagnostics, registerCodeFix, verifyArguments, cancellationToken)
	{
	}

	internal CodeFixContext(Project project, ImmutableArray<Diagnostic> diagnostics, Action<CodeAction, ImmutableArray<Diagnostic>> registerCodeFix, CancellationToken cancellationToken)
		: this(null, project, default(TextSpan), diagnostics, registerCodeFix, verifyArguments: false, cancellationToken)
	{
	}

	private CodeFixContext(Document document, Project project, TextSpan span, ImmutableArray<Diagnostic> diagnostics, Action<CodeAction, ImmutableArray<Diagnostic>> registerCodeFix, bool verifyArguments, CancellationToken cancellationToken)
	{
		if (verifyArguments)
		{
			if (document == null)
			{
				throw new ArgumentNullException("document");
			}
			if (registerCodeFix == null)
			{
				throw new ArgumentNullException("registerCodeFix");
			}
			VerifyDiagnosticsArgument(diagnostics, span);
		}
		Document = document;
		Project = project;
		Span = span;
		Diagnostics = diagnostics;
		this.registerCodeFix = registerCodeFix;
		CancellationToken = cancellationToken;
	}

	internal CodeFixContext(Document document, Diagnostic diagnostic, Action<CodeAction, ImmutableArray<Diagnostic>> registerCodeFix, bool verifyArguments, CancellationToken cancellationToken)
		: this(document, diagnostic.Location.SourceSpan, ImmutableArray.Create(diagnostic), registerCodeFix, verifyArguments, cancellationToken)
	{
	}

	public void RegisterCodeFix(CodeAction action, Diagnostic diagnostic)
	{
		if (action == null)
		{
			throw new ArgumentNullException("action");
		}
		if (diagnostic == null)
		{
			throw new ArgumentNullException("diagnostic");
		}
		registerCodeFix(action, ImmutableArray.Create(diagnostic));
	}

	public void RegisterCodeFix(CodeAction action, IEnumerable<Diagnostic> diagnostics)
	{
		if (diagnostics == null)
		{
			throw new ArgumentNullException("diagnostics");
		}
		RegisterCodeFix(action, diagnostics.ToImmutableArray());
	}

	public void RegisterCodeFix(CodeAction action, ImmutableArray<Diagnostic> diagnostics)
	{
		if (action == null)
		{
			throw new ArgumentNullException("action");
		}
		VerifyDiagnosticsArgument(diagnostics, Span);
		registerCodeFix(action, diagnostics);
	}

	public void RegisterFixes(ImmutableArray<CodeAction> actions, ImmutableArray<Diagnostic> diagnostics)
	{
		if (!actions.IsDefaultOrEmpty && !diagnostics.IsDefaultOrEmpty)
		{
			ImmutableArray<CodeAction>.Enumerator enumerator = actions.GetEnumerator();
			while (enumerator.MoveNext())
			{
				CodeAction current = enumerator.Current;
				RegisterCodeFix(current, diagnostics);
			}
		}
	}

	private static void VerifyDiagnosticsArgument(ImmutableArray<Diagnostic> diagnostics, TextSpan span)
	{
		if (diagnostics.IsDefault)
		{
			throw new ArgumentException(WorkspacesResources.VerifyDiagnosticsArgumentIsNotInitialized, "diagnostics");
		}
		if (diagnostics.Length == 0)
		{
			throw new ArgumentException(WorkspacesResources.AtLeastOneDiagnosticMustBeSupplied, "diagnostics");
		}
		if (diagnostics.Any((Diagnostic d) => d == null))
		{
			throw new ArgumentException(WorkspacesResources.SuppliedDiagnosticCannotBeNull, "diagnostics");
		}
		if (!diagnostics.Any((Diagnostic d) => d.Location.SourceSpan.Intersection(span).HasValue))
		{
			throw new ArgumentException(string.Format(WorkspacesResources.DiagnosticMustHaveSpan, span.ToString()), "diagnostics");
		}
	}
}
