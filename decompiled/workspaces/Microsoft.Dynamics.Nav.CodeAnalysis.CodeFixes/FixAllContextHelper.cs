using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.CodeFixes;

internal static class FixAllContextHelper
{
	public static async Task<ImmutableDictionary<Document, ImmutableArray<Diagnostic>>> GetDocumentDiagnosticsToFixAsync(FixAllContext fixAllContext)
	{
		FixAllContext fixAllContext2 = fixAllContext;
		CancellationToken cancellationToken = fixAllContext2.CancellationToken;
		ImmutableArray<Diagnostic> allDiagnostics = ImmutableArray<Diagnostic>.Empty;
		Document document = fixAllContext2.Document;
		Project project = fixAllContext2.Project;
		switch (fixAllContext2.Scope)
		{
		case FixAllScope.Document:
		{
			bool flag = document != null;
			if (flag)
			{
				flag = !(await document.IsGeneratedCodeAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false));
			}
			if (flag)
			{
				ImmutableArray<Diagnostic> value = await fixAllContext2.GetDocumentDiagnosticsAsync(document).ConfigureAwait(continueOnCapturedContext: false);
				return ImmutableDictionary<Document, ImmutableArray<Diagnostic>>.Empty.SetItem(document, value);
			}
			break;
		}
		case FixAllScope.Project:
			allDiagnostics = await fixAllContext2.GetAllDiagnosticsAsync(project).ConfigureAwait(continueOnCapturedContext: false);
			break;
		case FixAllScope.Workspace:
		{
			ImmutableArray<Project> immutableArray = project.Solution.Projects.Where((Project p) => p.Language == project.Language).ToImmutableArray();
			ConcurrentDictionary<ProjectId, ImmutableArray<Diagnostic>> diagnostics2 = new ConcurrentDictionary<ProjectId, ImmutableArray<Diagnostic>>();
			ArrayBuilder<Task> tasks = ArrayBuilder<Task>.GetInstance(immutableArray.Length);
			try
			{
				ImmutableArray<Project>.Enumerator enumerator = immutableArray.GetEnumerator();
				while (enumerator.MoveNext())
				{
					Project projectToFix2 = enumerator.Current;
					tasks.Add(Task.Run(async delegate
					{
						await AddDocumentDiagnosticsAsync(diagnostics2, projectToFix2).ConfigureAwait(continueOnCapturedContext: false);
					}, cancellationToken));
				}
				await Task.WhenAll(tasks).ConfigureAwait(continueOnCapturedContext: false);
				allDiagnostics = allDiagnostics.AddRange(diagnostics2.SelectMany<KeyValuePair<ProjectId, ImmutableArray<Diagnostic>>, Diagnostic>((KeyValuePair<ProjectId, ImmutableArray<Diagnostic>> i) => i.Value));
			}
			finally
			{
				tasks.Free();
			}
			break;
		}
		}
		if (allDiagnostics.IsEmpty)
		{
			return ImmutableDictionary<Document, ImmutableArray<Diagnostic>>.Empty;
		}
		return await GetDocumentDiagnosticsToFixAsync(fixAllContext2.Solution, allDiagnostics, fixAllContext2.CancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		async Task AddDocumentDiagnosticsAsync(ConcurrentDictionary<ProjectId, ImmutableArray<Diagnostic>> diagnostics, Project projectToFix)
		{
			ImmutableArray<Diagnostic> value2 = await fixAllContext2.GetAllDiagnosticsAsync(projectToFix).ConfigureAwait(continueOnCapturedContext: false);
			diagnostics.TryAdd(projectToFix.Id, value2);
		}
	}

	private static async Task<ImmutableDictionary<Document, ImmutableArray<Diagnostic>>> GetDocumentDiagnosticsToFixAsync(Solution solution, ImmutableArray<Diagnostic> diagnostics, CancellationToken cancellationToken)
	{
		Solution solution2 = solution;
		ImmutableDictionary<Document, ImmutableArray<Diagnostic>>.Builder builder = ImmutableDictionary.CreateBuilder<Document, ImmutableArray<Diagnostic>>();
		foreach (IGrouping<Document, Diagnostic> diagnosticsForDocument in from d in diagnostics
			group d by solution2.GetDocument(d.Location.SourceTree))
		{
			Document document = diagnosticsForDocument.Key;
			if (document != null)
			{
				cancellationToken.ThrowIfCancellationRequested();
				if (!(await document.IsGeneratedCodeAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false)))
				{
					builder.Add(document, diagnosticsForDocument.ToImmutableArray());
				}
			}
		}
		return builder.ToImmutable();
	}

	public static string GetDefaultFixAllTitle(FixAllScope fixAllScope, string diagnosticId)
	{
		return fixAllScope switch
		{
			FixAllScope.Document => string.Format(WorkspacesResources.FixAllDiagnosticOccurrencesInScope, diagnosticId, FixAllScope.Document.ToString().ToLowerInvariant()), 
			FixAllScope.Project => string.Format(WorkspacesResources.FixAllDiagnosticOccurrencesInScope, diagnosticId, FixAllScope.Project.ToString().ToLowerInvariant()), 
			FixAllScope.Workspace => string.Format(WorkspacesResources.FixAllDiagnosticOccurrencesInScope, diagnosticId, FixAllScope.Workspace.ToString().ToLowerInvariant()), 
			_ => throw ExceptionUtilities.UnexpectedValue(fixAllScope), 
		};
	}

	public static string GetDefaultFixAllTitle(FixAllScope fixAllScope)
	{
		return string.Format(WorkspacesResources.ApplyToAllOccurrencesInScope, fixAllScope.ToString().ToLowerInvariant());
	}
}
