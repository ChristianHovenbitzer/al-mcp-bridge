using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.CodeAnalysis.CodeActions;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.CodeFixes;

internal static class DefaultFixAllProviderHelpers
{
	public static async Task<CodeAction?> GetFixAsync(string title, FixAllContext fixAllContext, Func<FixAllContext, ImmutableArray<FixAllContext>, Task<Solution?>> fixAllContextsAsync)
	{
		Solution solution = fixAllContext.Scope switch
		{
			FixAllScope.Document => await GetDocumentFixesAsync(fixAllContext, fixAllContextsAsync).ConfigureAwait(continueOnCapturedContext: false), 
			FixAllScope.Project => await GetProjectFixesAsync(fixAllContext, fixAllContextsAsync).ConfigureAwait(continueOnCapturedContext: false), 
			FixAllScope.Workspace => await GetSolutionFixesAsync(fixAllContext, fixAllContextsAsync).ConfigureAwait(continueOnCapturedContext: false), 
			_ => throw ExceptionUtilities.UnexpectedValue(fixAllContext.Scope), 
		};
		if (solution == null)
		{
			return null;
		}
		return CodeAction.Create(title, (CancellationToken c) => Task.FromResult(solution));
	}

	private static Task<Solution?> GetDocumentFixesAsync(FixAllContext fixAllContext, Func<FixAllContext, ImmutableArray<FixAllContext>, Task<Solution?>> fixAllContextsAsync)
	{
		return fixAllContextsAsync(fixAllContext, ImmutableArray.Create(fixAllContext));
	}

	private static Task<Solution?> GetProjectFixesAsync(FixAllContext fixAllContext, Func<FixAllContext, ImmutableArray<FixAllContext>, Task<Solution?>> fixAllContextsAsync)
	{
		return fixAllContextsAsync(fixAllContext, ImmutableArray.Create(fixAllContext));
	}

	private static Task<Solution?> GetSolutionFixesAsync(FixAllContext fixAllContext, Func<FixAllContext, ImmutableArray<FixAllContext>, Task<Solution?>> fixAllContextsAsync)
	{
		FixAllContext fixAllContext2 = fixAllContext;
		Solution solution = fixAllContext2.Solution;
		IEnumerable<Project> source = from id in solution.GetProjectDependencyGraph().GetTopologicallySortedProjects()
			select solution.GetProject(id);
		return fixAllContextsAsync(fixAllContext2, source.SelectAsArray((Project p) => fixAllContext2.WithDocumentAndProject((fixAllContext2.Project == p) ? fixAllContext2.Document : p.Documents.FirstOrDefault(), p).WithScope(FixAllScope.Project)));
	}
}
