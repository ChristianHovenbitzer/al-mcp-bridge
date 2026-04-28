using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.CodeAnalysis.CodeActions;
using Microsoft.Dynamics.Nav.CodeAnalysis.CodeRefactoring;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.CodeFixes;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.CodeFixes;

internal sealed class FixAllState
{
	internal sealed class FixMultipleDiagnosticProvider : FixAllContext.DiagnosticProvider
	{
		public ImmutableDictionary<Document, ImmutableArray<Diagnostic>> DocumentDiagnosticsMap { get; }

		public ImmutableDictionary<Project, ImmutableArray<Diagnostic>> ProjectDiagnosticsMap { get; }

		public FixMultipleDiagnosticProvider(ImmutableDictionary<Document, ImmutableArray<Diagnostic>> diagnosticsMap)
		{
			DocumentDiagnosticsMap = diagnosticsMap;
			ProjectDiagnosticsMap = ImmutableDictionary<Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Project, ImmutableArray<Diagnostic>>.Empty;
		}

		public FixMultipleDiagnosticProvider(ImmutableDictionary<Project, ImmutableArray<Diagnostic>> diagnosticsMap)
		{
			ProjectDiagnosticsMap = diagnosticsMap;
			DocumentDiagnosticsMap = ImmutableDictionary<Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Document, ImmutableArray<Diagnostic>>.Empty;
		}

		public override Task<IEnumerable<Diagnostic>> GetAllDiagnosticsAsync(Project project, ISet<string> diagnosticIdsWithFixes, CancellationToken cancellationToken)
		{
			ArrayBuilder<Diagnostic> instance = ArrayBuilder<Diagnostic>.GetInstance();
			try
			{
				ImmutableArray<Diagnostic> value;
				if (!DocumentDiagnosticsMap.IsEmpty)
				{
					foreach (Document document in project.Documents)
					{
						if (!DocumentDiagnosticsMap.TryGetValue(document, out value))
						{
							continue;
						}
						ImmutableArray<Diagnostic>.Enumerator enumerator2 = value.GetEnumerator();
						while (enumerator2.MoveNext())
						{
							Diagnostic current2 = enumerator2.Current;
							if (diagnosticIdsWithFixes.Contains(current2.Id))
							{
								instance.Add(current2);
							}
						}
					}
				}
				if (ProjectDiagnosticsMap.TryGetValue(project, out value))
				{
					ImmutableArray<Diagnostic>.Enumerator enumerator2 = value.GetEnumerator();
					while (enumerator2.MoveNext())
					{
						Diagnostic current3 = enumerator2.Current;
						if (diagnosticIdsWithFixes.Contains(current3.Id))
						{
							instance.Add(current3);
						}
					}
				}
				return Task.FromResult((IEnumerable<Diagnostic>)instance.ToImmutable());
			}
			finally
			{
				instance.Free();
			}
		}

		public override Task<IEnumerable<Diagnostic>> GetDocumentDiagnosticsAsync(Document document, ISet<string> diagnosticIdsWithFixes, CancellationToken cancellationToken)
		{
			if (DocumentDiagnosticsMap.TryGetValue(document, out ImmutableArray<Diagnostic> value))
			{
				ArrayBuilder<Diagnostic> instance = ArrayBuilder<Diagnostic>.GetInstance();
				try
				{
					ImmutableArray<Diagnostic>.Enumerator enumerator = value.GetEnumerator();
					while (enumerator.MoveNext())
					{
						Diagnostic current = enumerator.Current;
						if (diagnosticIdsWithFixes.Contains(current.Id))
						{
							instance.Add(current);
						}
					}
					return Task.FromResult((IEnumerable<Diagnostic>)instance.ToImmutableArray());
				}
				finally
				{
					instance.Free();
				}
			}
			return SpecializedTasks.EmptyEnumerable<Diagnostic>();
		}

		public override Task<IEnumerable<Diagnostic>> GetProjectDiagnosticsAsync(Project project, ISet<string> diagnosticIdsWithFixes, CancellationToken cancellationToken)
		{
			if (ProjectDiagnosticsMap.TryGetValue(project, out ImmutableArray<Diagnostic> value))
			{
				ArrayBuilder<Diagnostic> instance = ArrayBuilder<Diagnostic>.GetInstance();
				try
				{
					ImmutableArray<Diagnostic>.Enumerator enumerator = value.GetEnumerator();
					while (enumerator.MoveNext())
					{
						Diagnostic current = enumerator.Current;
						if (diagnosticIdsWithFixes.Contains(current.Id))
						{
							instance.Add(current);
						}
					}
					return Task.FromResult((IEnumerable<Diagnostic>)instance.ToImmutableArray());
				}
				finally
				{
					instance.Free();
				}
			}
			return SpecializedTasks.EmptyEnumerable<Diagnostic>();
		}
	}

	public readonly int CorrelationId;

	public FixAllContext.DiagnosticProvider DiagnosticProvider { get; }

	public FixAllProvider? FixAllProvider { get; }

	public string? CodeActionEquivalenceKey { get; }

	public ICodeActionProvider CodeActionProvider { get; }

	public ImmutableHashSet<string> DiagnosticIds { get; }

	public Document? Document { get; }

	public Project Project { get; }

	public FixAllScope Scope { get; }

	public FixAllKind FixAllKind { get; }

	public CodeActionOptionsProvider CodeActionOptionsProvider { get; }

	public TextSpan? Span { get; }

	public Solution Solution => Project.Solution;

	internal FixAllState(FixAllProvider? fixAllProvider, TextSpan? diagnosticSpan, Document? document, Project project, CodeFixProvider codeFixProvider, FixAllScope scope, string? codeActionEquivalenceKey, IEnumerable<string> diagnosticIds, FixAllContext.DiagnosticProvider fixAllDiagnosticProvider, CodeActionOptionsProvider codeActionOptionsProvider)
		: this(fixAllProvider, diagnosticSpan, document, project, (ICodeActionProvider)codeFixProvider, scope, codeActionEquivalenceKey, diagnosticIds, fixAllDiagnosticProvider, codeActionOptionsProvider)
	{
	}

	public FixAllState(FixAllProvider fixAllProvider, Document document, TextSpan selectionSpan, CodeRefactoringProvider codeRefactoringProvider, CodeActionOptionsProvider optionsProvider, FixAllScope fixAllScope, string CodeActionEquivalenceKey)
		: this(fixAllProvider, selectionSpan, document, document.Project, codeRefactoringProvider, fixAllScope, CodeActionEquivalenceKey, null, null, optionsProvider)
	{
	}

	public FixAllState(FixAllProvider? fixAllProvider, TextSpan? span, Document? document, Project project, ICodeActionProvider codeActionProvider, FixAllScope scope, string? codeActionEquivalenceKey, IEnumerable<string> diagnosticIds, FixAllContext.DiagnosticProvider fixAllDiagnosticProvider, CodeActionOptionsProvider codeActionOptionsProvider)
	{
		FixAllProvider = fixAllProvider;
		Span = span;
		Document = document;
		Project = project;
		CodeActionProvider = codeActionProvider;
		Scope = scope;
		CodeActionEquivalenceKey = codeActionEquivalenceKey;
		DiagnosticIds = ((diagnosticIds == null) ? ImmutableHashSet.Create<string>() : ImmutableHashSet.CreateRange(diagnosticIds));
		DiagnosticProvider = fixAllDiagnosticProvider;
		CodeActionOptionsProvider = codeActionOptionsProvider;
	}

	public FixAllState WithScope(FixAllScope scope)
	{
		Optional<FixAllScope> scope2 = scope;
		return With(default(Optional<(Document, Project)>), scope2);
	}

	public FixAllState WithCodeActionEquivalenceKey(string? codeActionEquivalenceKey)
	{
		Optional<string> codeActionEquivalenceKey2 = codeActionEquivalenceKey;
		return With(default(Optional<(Document, Project)>), default(Optional<FixAllScope>), codeActionEquivalenceKey2);
	}

	public FixAllState WithDocumentAndProject(Document? document, Project project)
	{
		return With((document, project));
	}

	public FixAllState With(Optional<(Document? document, Project project)> documentAndProject = default(Optional<(Document? document, Project project)>), Optional<FixAllScope> scope = default(Optional<FixAllScope>), Optional<string?> codeActionEquivalenceKey = default(Optional<string?>))
	{
		Document document;
		Project project;
		if (documentAndProject.HasValue)
		{
			(document, project) = documentAndProject.Value;
		}
		else
		{
			Document? document2 = Document;
			Project project2 = Project;
			project = project2;
			document = document2;
		}
		FixAllScope fixAllScope = (scope.HasValue ? scope.Value : Scope);
		string text = (codeActionEquivalenceKey.HasValue ? codeActionEquivalenceKey.Value : CodeActionEquivalenceKey);
		if (document == Document && project == Project && fixAllScope == Scope && text == CodeActionEquivalenceKey)
		{
			return this;
		}
		return new FixAllState(FixAllProvider, Span, document, project, CodeActionProvider, fixAllScope, text, DiagnosticIds, DiagnosticProvider, CodeActionOptionsProvider);
	}

	internal Task<ImmutableDictionary<Document, ImmutableArray<TextSpan>>> GetFixAllSpansAsync(CancellationToken cancellationToken)
	{
		IEnumerable<Document> enumerable = null;
		switch (Scope)
		{
		case FixAllScope.Document:
			Contract.ThrowIfNull(Document);
			enumerable = SpecializedCollections.SingletonEnumerable(Document);
			break;
		case FixAllScope.Project:
			enumerable = Project.Documents;
			break;
		case FixAllScope.Workspace:
			enumerable = Project.Solution.GetAllDocuments();
			break;
		default:
			return Task.FromResult(ImmutableDictionary<Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Document, ImmutableArray<TextSpan>>.Empty);
		}
		return Task.FromResult(enumerable.ToImmutableDictionary((Document d) => d, (Document _) => ImmutableArray<TextSpan>.Empty));
	}
}
