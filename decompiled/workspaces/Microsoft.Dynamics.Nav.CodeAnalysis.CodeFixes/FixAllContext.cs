using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.CodeAnalysis.CodeActions;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Log;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.CodeFixes;

public class FixAllContext : IFixAllContext
{
	public abstract class DiagnosticProvider
	{
		public abstract Task<IEnumerable<Diagnostic>> GetDocumentDiagnosticsAsync(Document document, ISet<string> diagnosticIdsWithFixes, CancellationToken cancellationToken);

		public abstract Task<IEnumerable<Diagnostic>> GetProjectDiagnosticsAsync(Project project, ISet<string> diagnosticIdsWithFixes, CancellationToken cancellationToken);

		public abstract Task<IEnumerable<Diagnostic>> GetAllDiagnosticsAsync(Project project, ISet<string> diagnosticIdsWithFixes, CancellationToken cancellationToken);

		internal static async Task<ImmutableDictionary<Document, ImmutableArray<Diagnostic>>> GetDocumentDiagnosticsToFixAsync(FixAllContext fixAllContext)
		{
			return (await GetDocumentDiagnosticsToFixWorkerAsync(fixAllContext).ConfigureAwait(continueOnCapturedContext: false)).Where((KeyValuePair<Document, ImmutableArray<Diagnostic>> kvp) => !kvp.Value.IsDefaultOrEmpty).ToImmutableDictionary();
			static async Task<ImmutableDictionary<Document, ImmutableArray<Diagnostic>>> GetDocumentDiagnosticsToFixWorkerAsync(FixAllContext fixAllContext)
			{
				if (fixAllContext.State.DiagnosticProvider is FixAllState.FixMultipleDiagnosticProvider fixMultipleDiagnosticProvider)
				{
					return fixMultipleDiagnosticProvider.DocumentDiagnosticsMap;
				}
				return await FixAllContextHelper.GetDocumentDiagnosticsToFixAsync(fixAllContext).ConfigureAwait(continueOnCapturedContext: false);
			}
		}

		internal static async Task<ImmutableDictionary<Project, ImmutableArray<Diagnostic>>> GetProjectDiagnosticsToFixAsync(FixAllContext fixAllContext)
		{
			FixAllContext fixAllContext2 = fixAllContext;
			using (Logger.LogBlock(FunctionId.CodeFixes_FixAllOccurrencesComputation_Project_Diagnostics, fixAllContext2.CancellationToken))
			{
				Project project = fixAllContext2.Project;
				if (project != null)
				{
					switch (fixAllContext2.Scope)
					{
					case FixAllScope.Project:
						return ImmutableDictionary.CreateRange(SpecializedCollections.SingletonEnumerable(KeyValuePairUtil.Create(project, await fixAllContext2.GetProjectDiagnosticsAsync(project).ConfigureAwait(continueOnCapturedContext: false))));
					case FixAllScope.Workspace:
					{
						ImmutableDictionary<Project, ImmutableArray<Diagnostic>>.Builder projectsAndDiagnostics = ImmutableDictionary.CreateBuilder<Project, ImmutableArray<Diagnostic>>();
						var tasks = project.Solution.Projects.Select(async (Project p) => new
						{
							Project = p,
							Diagnostics = await fixAllContext2.GetProjectDiagnosticsAsync(p).ConfigureAwait(continueOnCapturedContext: false)
						}).ToArray();
						await Task.WhenAll(tasks).ConfigureAwait(continueOnCapturedContext: false);
						var array = tasks;
						for (int i = 0; i < array.Length; i++)
						{
							var anon = await array[i].ConfigureAwait(continueOnCapturedContext: false);
							if (anon.Diagnostics.Any())
							{
								projectsAndDiagnostics[anon.Project] = anon.Diagnostics;
							}
						}
						return projectsAndDiagnostics.ToImmutable();
					}
					}
				}
				return ImmutableDictionary<Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Project, ImmutableArray<Diagnostic>>.Empty;
			}
		}
	}

	internal abstract class SpanBasedDiagnosticProvider : DiagnosticProvider
	{
		public abstract Task<IEnumerable<Diagnostic>> GetDocumentSpanDiagnosticsAsync(Document document, ISet<string> diagnosticIds, TextSpan fixAllSpan, CancellationToken cancellationToken);
	}

	internal FixAllState State { get; }

	internal FixAllProvider? FixAllProvider => State.FixAllProvider;

	public Solution Solution => State.Solution;

	public Project Project => State.Project;

	public Document? Document => State.Document;

	public ICodeActionProvider CodeFixProvider => State.CodeActionProvider;

	public FixAllScope Scope => State.Scope;

	public ImmutableHashSet<string> DiagnosticIds => State.DiagnosticIds;

	public string? CodeActionEquivalenceKey => State.CodeActionEquivalenceKey;

	public CancellationToken CancellationToken { get; }

	FixAllState IFixAllContext.State
	{
		get
		{
			throw new NotImplementedException();
		}
	}

	FixAllProvider IFixAllContext.FixAllProvider
	{
		get
		{
			throw new NotImplementedException();
		}
	}

	public object Provider
	{
		get
		{
			throw new NotImplementedException();
		}
	}

	public FixAllContext(Document document, CodeFixProvider codeFixProvider, FixAllScope scope, string? codeActionEquivalenceKey, IEnumerable<string> diagnosticIds, DiagnosticProvider fixAllDiagnosticProvider, CancellationToken cancellationToken)
		: this(document, null, codeFixProvider, scope, codeActionEquivalenceKey, diagnosticIds, fixAllDiagnosticProvider, cancellationToken)
	{
	}

	public FixAllContext(Document document, TextSpan? diagnosticSpan, CodeFixProvider codeFixProvider, FixAllScope scope, string? codeActionEquivalenceKey, IEnumerable<string> diagnosticIds, DiagnosticProvider fixAllDiagnosticProvider, CancellationToken cancellationToken)
		: this(new FixAllState(null, diagnosticSpan, document ?? throw new ArgumentNullException("document"), document.Project, codeFixProvider ?? throw new ArgumentNullException("codeFixProvider"), scope, codeActionEquivalenceKey, PublicContract.RequireNonNullItems(diagnosticIds, "diagnosticIds"), fixAllDiagnosticProvider ?? throw new ArgumentNullException("fixAllDiagnosticProvider"), (string a) => CodeActionOptions.Default), cancellationToken)
	{
	}

	public FixAllContext(Project project, CodeFixProvider codeFixProvider, FixAllScope scope, string? codeActionEquivalenceKey, IEnumerable<string> diagnosticIds, DiagnosticProvider fixAllDiagnosticProvider, CancellationToken cancellationToken)
		: this(new FixAllState(null, null, null, project ?? throw new ArgumentNullException("project"), codeFixProvider ?? throw new ArgumentNullException("codeFixProvider"), scope, codeActionEquivalenceKey, PublicContract.RequireNonNullItems(diagnosticIds, "diagnosticIds"), fixAllDiagnosticProvider ?? throw new ArgumentNullException("fixAllDiagnosticProvider"), (string _) => CodeActionOptions.Default), cancellationToken)
	{
	}

	internal FixAllContext(FixAllState state, CancellationToken cancellationToken)
	{
		State = state;
		CancellationToken = cancellationToken;
	}

	public async Task<ImmutableArray<Diagnostic>> GetDocumentDiagnosticsAsync(Document document)
	{
		if (document == null)
		{
			throw new ArgumentNullException("document");
		}
		if (Project.Language != document.Project.Language)
		{
			return ImmutableArray<Diagnostic>.Empty;
		}
		return (await State.DiagnosticProvider.GetDocumentDiagnosticsAsync(document, DiagnosticIds, CancellationToken).ConfigureAwait(continueOnCapturedContext: false)).ToImmutableArray();
	}

	internal async Task<ImmutableArray<Diagnostic>> GetDocumentSpanDiagnosticsAsync(Document document, TextSpan filterSpan)
	{
		if (document == null)
		{
			throw new ArgumentNullException("document");
		}
		if (Project.Language != document.Project.Language)
		{
			return ImmutableArray<Diagnostic>.Empty;
		}
		return (await ((State.DiagnosticProvider is SpanBasedDiagnosticProvider spanBasedDiagnosticProvider) ? spanBasedDiagnosticProvider.GetDocumentSpanDiagnosticsAsync(document, DiagnosticIds, filterSpan, CancellationToken) : State.DiagnosticProvider.GetDocumentDiagnosticsAsync(document, DiagnosticIds, CancellationToken)).ConfigureAwait(continueOnCapturedContext: false)).ToImmutableArray();
	}

	public Task<ImmutableArray<Diagnostic>> GetProjectDiagnosticsAsync(Project project)
	{
		if (project == null)
		{
			throw new ArgumentNullException("project");
		}
		return GetProjectDiagnosticsAsync(project, includeAllDocumentDiagnostics: false);
	}

	public Task<ImmutableArray<Diagnostic>> GetAllDiagnosticsAsync(Project project)
	{
		if (project == null)
		{
			throw new ArgumentNullException("project");
		}
		return GetProjectDiagnosticsAsync(project, includeAllDocumentDiagnostics: true);
	}

	private async Task<ImmutableArray<Diagnostic>> GetProjectDiagnosticsAsync(Project project, bool includeAllDocumentDiagnostics)
	{
		Contract.ThrowIfNull(project);
		if (Project.Language != project.Language)
		{
			return ImmutableArray<Diagnostic>.Empty;
		}
		return (await (includeAllDocumentDiagnostics ? State.DiagnosticProvider.GetAllDiagnosticsAsync(project, DiagnosticIds, CancellationToken) : State.DiagnosticProvider.GetProjectDiagnosticsAsync(project, DiagnosticIds, CancellationToken)).ConfigureAwait(continueOnCapturedContext: false)).ToImmutableArray();
	}

	public Task<ImmutableDictionary<Document, ImmutableArray<TextSpan>>> GetFixAllSpansAsync(CancellationToken cancellationToken)
	{
		return State.GetFixAllSpansAsync(cancellationToken);
	}

	internal FixAllContext WithScope(FixAllScope scope)
	{
		return WithState(State.WithScope(scope));
	}

	internal FixAllContext WithDocumentAndProject(Document? document, Project project)
	{
		return WithState(State.WithDocumentAndProject(document, project));
	}

	private FixAllContext WithState(FixAllState state)
	{
		if (State != state)
		{
			return new FixAllContext(state, CancellationToken);
		}
		return this;
	}

	internal Task<ImmutableDictionary<Document, ImmutableArray<Diagnostic>>> GetDocumentDiagnosticsToFixAsync()
	{
		return DiagnosticProvider.GetDocumentDiagnosticsToFixAsync(this);
	}

	internal Task<ImmutableDictionary<Project, ImmutableArray<Diagnostic>>> GetProjectDiagnosticsToFixAsync()
	{
		return DiagnosticProvider.GetProjectDiagnosticsToFixAsync(this);
	}

	internal string GetDefaultFixAllTitle()
	{
		return FixAllContextHelper.GetDefaultFixAllTitle(Scope, State.DiagnosticIds.FirstOrDefault());
	}

	string IFixAllContext.GetDefaultFixAllTitle()
	{
		throw new NotImplementedException();
	}
}
