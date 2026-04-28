using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.CodeAnalysis.Analyzers;
using Microsoft.Dynamics.Nav.CodeAnalysis.CodeActions;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.CodeFixes.Helpers;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Host;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Utilities;
using Microsoft.Dynamics.Nav.Deployment.Telemetry;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.CodeFixes;

internal class CodeFixService : ICodeFixService, IWorkspaceService
{
	internal class FixAllCachedDocumentDiagnosticsProvider : FixAllContext.DiagnosticProvider
	{
		private readonly ImmutableArray<Diagnostic> diagnostics;

		public FixAllCachedDocumentDiagnosticsProvider(ImmutableArray<Diagnostic> documentDiagnostics)
		{
			diagnostics = documentDiagnostics;
		}

		public override Task<IEnumerable<Diagnostic>> GetAllDiagnosticsAsync(Project project, ISet<string> diagnosticIdsWithFixes, CancellationToken cancellationToken)
		{
			return Task.FromResult((IEnumerable<Diagnostic>)GetFilteredDiagnostics(diagnostics, diagnosticIdsWithFixes));
		}

		public override Task<IEnumerable<Diagnostic>> GetDocumentDiagnosticsAsync(Document document, ISet<string> diagnosticIdsWithFixes, CancellationToken cancellationToken)
		{
			return Task.FromResult((IEnumerable<Diagnostic>)GetFilteredDiagnostics(diagnostics, diagnosticIdsWithFixes));
		}

		public override Task<IEnumerable<Diagnostic>> GetProjectDiagnosticsAsync(Project project, ISet<string> diagnosticIdsWithFixes, CancellationToken cancellationToken)
		{
			return SpecializedTasks.EmptyEnumerable<Diagnostic>();
		}

		private static ImmutableArray<Diagnostic> GetFilteredDiagnostics(ImmutableArray<Diagnostic> diagnostics, ISet<string> diagnosticIdsWithFixes)
		{
			ArrayBuilder<Diagnostic> instance = ArrayBuilder<Diagnostic>.GetInstance();
			try
			{
				ImmutableArray<Diagnostic>.Enumerator enumerator = diagnostics.GetEnumerator();
				while (enumerator.MoveNext())
				{
					Diagnostic current = enumerator.Current;
					if (diagnosticIdsWithFixes.Contains(current.Id))
					{
						instance.Add(current);
					}
				}
				return instance.ToImmutableArray();
			}
			finally
			{
				instance.Free();
			}
		}
	}

	internal class FixAllEagerDiagnosticProvider : FixAllContext.DiagnosticProvider
	{
		public override async Task<IEnumerable<Diagnostic>> GetAllDiagnosticsAsync(Project project, ISet<string> diagnosticIdsWithFixes, CancellationToken cancellationToken)
		{
			return await GetProjectDiagnosticsAsync(project, diagnosticIdsWithFixes, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		}

		public override async Task<IEnumerable<Diagnostic>> GetDocumentDiagnosticsAsync(Document document, ISet<string> diagnosticIdsWithFixes, CancellationToken cancellationToken)
		{
			return FilterDiagnostics(await GetProjectDiagnosticsAsync(document.Project, diagnosticIdsWithFixes, cancellationToken).ConfigureAwait(continueOnCapturedContext: false), diagnosticIdsWithFixes, document);
		}

		public override async Task<IEnumerable<Diagnostic>> GetProjectDiagnosticsAsync(Project project, ISet<string> diagnosticIdsWithFixes, CancellationToken cancellationToken)
		{
			Compilation compilation = await project.GetCompilationAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
			ImmutableArray<Diagnostic> immutableArray = ImmutableArray<Diagnostic>.Empty;
			bool analyzerDiag = false;
			if (project.AnalyzerReferences.Count > 0)
			{
				ImmutableArray<DiagnosticAnalyzer> analyzersWithFixes = GetAnalyzersWithFixes(diagnosticIdsWithFixes, project.DiagnosticAnalyzers);
				if (analyzersWithFixes.Length > 0)
				{
					analyzerDiag = true;
					(ImmutableArray<Diagnostic>, ImmutableArray<Diagnostic>) obj = await AnalyzersHelper.GetAnalyzerDiagnostics(compilation, analyzersWithFixes, AnalyzerOptions.Empty, null, logAnalyzerExceptionAsDiagnostics: true, reportSuppressedDiagnostics: false, TelemetryServiceManager.CurrentTelemetryService, "CodeFixService", cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
					ImmutableArray<Diagnostic> item = obj.Item1;
					ImmutableArray<Diagnostic> item2 = obj.Item2;
					immutableArray = item.AddRange(item2);
					cancellationToken.ThrowIfCancellationRequested();
				}
			}
			if (!analyzerDiag)
			{
				immutableArray = compilation.GetDiagnostics(cancellationToken);
				cancellationToken.ThrowIfCancellationRequested();
			}
			return FilterDiagnostics(immutableArray, diagnosticIdsWithFixes);
		}

		private static ImmutableArray<Diagnostic> FilterDiagnostics(IEnumerable<Diagnostic> diagnostics, ISet<string> diagnosticIdsFilter, Document? document = null)
		{
			ArrayBuilder<Diagnostic> instance = ArrayBuilder<Diagnostic>.GetInstance();
			string text = document?.FilePath;
			try
			{
				foreach (Diagnostic diagnostic in diagnostics)
				{
					if (diagnosticIdsFilter.Contains(diagnostic.Id) && (text == null || diagnostic.Location?.SourceTree?.FilePath == text))
					{
						instance.Add(diagnostic);
					}
				}
				return instance.ToImmutableArray();
			}
			finally
			{
				instance.Free();
			}
		}

		private static ImmutableArray<DiagnosticAnalyzer> GetAnalyzersWithFixes(ISet<string> diagnosticIdsWithFixes, IEnumerable<DiagnosticAnalyzer> allAnalyzers)
		{
			ImmutableArray<DiagnosticAnalyzer>.Builder builder = ImmutableArray.CreateBuilder<DiagnosticAnalyzer>();
			foreach (DiagnosticAnalyzer allAnalyzer in allAnalyzers)
			{
				ImmutableArray<DiagnosticDescriptor>.Enumerator enumerator2 = allAnalyzer.SupportedDiagnostics.GetEnumerator();
				while (enumerator2.MoveNext())
				{
					DiagnosticDescriptor current2 = enumerator2.Current;
					if (diagnosticIdsWithFixes.Contains(current2.Id))
					{
						builder.Add(allAnalyzer);
						break;
					}
				}
			}
			return builder.ToImmutableArray();
		}
	}

	private const string TelemetryContext = "CodeFixService";

	private readonly Lazy<ImmutableArray<CodeFixProvider>> lazyCodeFixProviders;

	private readonly Lazy<ImmutableHashSet<string>> lazyDiagnosticIdsWithFixes;

	private ImmutableDictionary<CodeFixProvider, ImmutableArray<string>> fixerToFixableIdsMap = ImmutableDictionary<CodeFixProvider, ImmutableArray<string>>.Empty;

	private ImmutableDictionary<object, FixAllProviderInfo?> fixAllProviderMap = ImmutableDictionary<object, FixAllProviderInfo>.Empty;

	private int lastCompilationHash;

	private Tuple<DocumentId?, ImmutableArray<Diagnostic>> cachedDiagnosticsForDocument;

	private ProjectAnalyzerReference? projectAnalyzers;

	private readonly object cacheLock = new object();

	private ImmutableArray<CodeFixProvider> CodeFixProviders => lazyCodeFixProviders.Value;

	private ImmutableHashSet<string> DiagnosticIdsWithFixes => lazyDiagnosticIdsWithFixes.Value;

	private ImmutableArray<Diagnostic> CachedDiagnostics => cachedDiagnosticsForDocument.Item2;

	private DocumentId? CachedDocumentId => cachedDiagnosticsForDocument.Item1;

	public CodeFixService(IEnumerable<CodeFixProvider> providers)
	{
		IEnumerable<CodeFixProvider> providers2 = providers;
		base._002Ector();
		lazyCodeFixProviders = new Lazy<ImmutableArray<CodeFixProvider>>(() => (providers2 != null) ? providers2.Distinct().ToImmutableArray() : ImmutableArray<CodeFixProvider>.Empty);
		lazyDiagnosticIdsWithFixes = new Lazy<ImmutableHashSet<string>>(BuildDiagnosticIdWithFixesSet);
		cachedDiagnosticsForDocument = new Tuple<DocumentId, ImmutableArray<Diagnostic>>(null, ImmutableArray<Diagnostic>.Empty);
	}

	public async Task<ImmutableArray<CodeFixCollection>> GetFixesAsync(Document document, TextSpan textSpan, CancellationToken cancellationToken)
	{
		if (document == null)
		{
			return ImmutableArray<CodeFixCollection>.Empty;
		}
		PooledDictionary<TextSpan, List<Diagnostic>> aggregatedDiagnostics = null;
		try
		{
			ImmutableArray<Diagnostic> allDiagnostics = await GetDiagnosticsAsync(document, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
			ImmutableArray<Diagnostic>.Enumerator enumerator = allDiagnostics.GetEnumerator();
			while (enumerator.MoveNext())
			{
				Diagnostic current = enumerator.Current;
				if (current.IsSuppressed)
				{
					continue;
				}
				Location location = current.Location;
				if ((object)location != null && location.IsInSource && !(document.FilePath != current.Location?.SourceTree?.FilePath) && textSpan.IntersectsWith(current.Location.SourceSpan))
				{
					cancellationToken.ThrowIfCancellationRequested();
					if (aggregatedDiagnostics == null)
					{
						aggregatedDiagnostics = PooledDictionary<TextSpan, List<Diagnostic>>.GetInstance();
					}
					aggregatedDiagnostics.GetOrAdd(current.Location.SourceSpan, (TextSpan _) => new List<Diagnostic>()).Add(current);
				}
			}
			if (aggregatedDiagnostics == null)
			{
				return ImmutableArray<CodeFixCollection>.Empty;
			}
			ArrayBuilder<CodeFixCollection> result = ArrayBuilder<CodeFixCollection>.GetInstance();
			foreach (KeyValuePair<TextSpan, List<Diagnostic>> item in aggregatedDiagnostics)
			{
				TextSpan key = item.Key;
				IEnumerable<Diagnostic> diagnostics = item.Value.OrderByDescending((Diagnostic d) => d.Severity);
				await AppendFixesAsync(document, key, diagnostics, result, allDiagnostics, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
			}
			return result.ToImmutableAndFree();
		}
		finally
		{
			aggregatedDiagnostics?.Free();
		}
	}

	private async Task AppendFixesAsync(Document document, TextSpan span, IEnumerable<Diagnostic> diagnostics, ArrayBuilder<CodeFixCollection> result, ImmutableArray<Diagnostic> allDiagnostics, CancellationToken cancellationToken)
	{
		Document document2 = document;
		ImmutableArray<CodeFixProvider>.Enumerator enumerator = CodeFixProviders.GetEnumerator();
		while (enumerator.MoveNext())
		{
			CodeFixProvider fixer = enumerator.Current;
			cancellationToken.ThrowIfCancellationRequested();
			await AppendFixesAsync(document2, span, diagnostics, result, fixer, (Diagnostic d) => GetFixableDiagnosticIds(fixer).Contains(d.Id), (ImmutableArray<Diagnostic> ds) => GetCodeFixesAsync(document2, span, fixer, ds, cancellationToken), allDiagnostics).ConfigureAwait(continueOnCapturedContext: false);
		}
	}

	private async Task AppendFixesAsync(Document document, TextSpan fixesSpan, IEnumerable<Diagnostic> diagnosticsWithSameSpan, ArrayBuilder<CodeFixCollection> result, CodeFixProvider fixer, Func<Diagnostic, bool> hasFix, Func<ImmutableArray<Diagnostic>, Task<ImmutableArray<CodeFix>>> getFixes, ImmutableArray<Diagnostic> allDiagnostics)
	{
		ImmutableArray<Diagnostic> diagnostics = diagnosticsWithSameSpan.Where(hasFix).ToImmutableArray();
		if (diagnostics.Length <= 0)
		{
			return;
		}
		ImmutableArray<CodeFix> fixes = await getFixes(diagnostics);
		if (fixes.IsDefaultOrEmpty)
		{
			return;
		}
		FixAllProviderInfo orAdd = ImmutableInterlocked.GetOrAdd(ref fixAllProviderMap, fixer, FixAllProviderInfo.Create);
		_ = ImmutableArray<FixAllScope>.Empty;
		if (orAdd != null)
		{
			ImmutableHashSet<string> diagnosticIds = (from d in diagnostics.Where(orAdd.CanBeFixed)
				select d.Id).ToImmutableHashSet();
			FixAllCachedDocumentDiagnosticsProvider fixAllCachedDocumentDiagnosticsProvider = new FixAllCachedDocumentDiagnosticsProvider(allDiagnostics);
			FixAllEagerDiagnosticProvider fixAllEagerDiagnosticProvider = new FixAllEagerDiagnosticProvider();
			ImmutableArray<CodeFix>.Enumerator enumerator = fixes.GetEnumerator();
			while (enumerator.MoveNext())
			{
				CodeFix current = enumerator.Current;
				if (current.GetFixAllStates().IsEmpty)
				{
					ImmutableArray<FixAllState>.Builder builder = ImmutableArray.CreateBuilder<FixAllState>();
					ImmutableArray<FixAllScope>.Enumerator enumerator2 = orAdd.SupportedScopes.GetEnumerator();
					while (enumerator2.MoveNext())
					{
						FixAllScope current2 = enumerator2.Current;
						FixAllContext.DiagnosticProvider fixAllDiagnosticProvider = ((current2 == FixAllScope.Document) ? ((FixAllContext.DiagnosticProvider)fixAllCachedDocumentDiagnosticsProvider) : ((FixAllContext.DiagnosticProvider)fixAllEagerDiagnosticProvider));
						FixAllState item = new FixAllState(orAdd.FixAllProvider, fixesSpan, document, document.Project, fixer, current2, current.GetCodeAction().EquivalenceKey, diagnosticIds, fixAllDiagnosticProvider, null);
						builder.Add(item);
					}
					current.WithFixAllStates(builder.ToImmutableArray());
				}
			}
			_ = orAdd.SupportedScopes;
		}
		CodeFixCollection item2 = new CodeFixCollection(fixer, fixesSpan, fixes, diagnostics.First());
		result.Add(item2);
	}

	private async Task<ImmutableArray<CodeFix>> GetCodeFixesAsync(Document document, TextSpan span, CodeFixProvider fixer, ImmutableArray<Diagnostic> diagnostics, CancellationToken cancellationToken)
	{
		Document document2 = document;
		ArrayBuilder<CodeFix> fixes = ArrayBuilder<CodeFix>.GetInstance();
		try
		{
			CodeFixContext context = new CodeFixContext(document2, span, diagnostics, delegate(CodeAction action, ImmutableArray<Diagnostic> applicableDiagnostics)
			{
				fixes.Add(new CodeFix(document2.Project, action, applicableDiagnostics));
			}, verifyArguments: false, cancellationToken);
			await (fixer.RegisterCodeFixesAsync(context) ?? Task.CompletedTask).ConfigureAwait(continueOnCapturedContext: false);
			return fixes.ToImmutable();
		}
		finally
		{
			fixes.Free();
		}
	}

	private ImmutableArray<string> GetFixableDiagnosticIds(CodeFixProvider fixer)
	{
		try
		{
			return ImmutableInterlocked.GetOrAdd(ref fixerToFixableIdsMap, fixer, (CodeFixProvider f) => GetAndTestFixableDiagnosticIds(f));
		}
		catch (OperationCanceledException)
		{
			throw;
		}
		catch (Exception)
		{
			return ImmutableArray<string>.Empty;
		}
	}

	private static ImmutableArray<string> GetAndTestFixableDiagnosticIds(CodeFixProvider codeFixProvider)
	{
		ImmutableArray<string> fixableDiagnosticIds = codeFixProvider.FixableDiagnosticIds;
		if (fixableDiagnosticIds.IsDefault)
		{
			throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, WorkspacesResources.ReturnedUninitializedImmutableArray, codeFixProvider.GetType().Name + ".FixableDiagnosticIds"));
		}
		return fixableDiagnosticIds;
	}

	private async Task<ImmutableArray<Diagnostic>> GetDiagnosticsAsync(Document document, CancellationToken cancellationToken)
	{
		Project project = document.Project;
		SemanticModel semanticModel = await document.GetSemanticModelAsync().ConfigureAwait(continueOnCapturedContext: false);
		int compilationHash = semanticModel.Compilation.GetHashCode();
		if (!DocumentDiagnosticCacheNeedsInvalidation(compilationHash, document.Id))
		{
			return CachedDiagnostics;
		}
		cancellationToken.ThrowIfCancellationRequested();
		_ = ImmutableArray<Diagnostic>.Empty;
		ImmutableArray<Diagnostic> immutableArray;
		if (project.BackgroundCodeAnalysisScope.IsEnabled() && project.AnalyzerReferences.Count > 0)
		{
			ImmutableArray<DiagnosticAnalyzer> analyzersWithAvailableCodeFixes = GetAnalyzersWithAvailableCodeFixes(project);
			(ImmutableArray<Diagnostic>, ImmutableArray<Diagnostic>) tuple = await AnalyzersHelper.GetAnalyzerDiagnosticsForDocument(semanticModel.SyntaxTree, semanticModel.Compilation, analyzersWithAvailableCodeFixes, TelemetryServiceManager.CurrentTelemetryService, "CodeFixService", cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
			immutableArray = tuple.Item1.AddRange(tuple.Item2);
		}
		else
		{
			CancellationToken cancellationToken2 = cancellationToken;
			immutableArray = semanticModel.GetDiagnostics(null, cancellationToken2);
		}
		UpdateDocumentDiagnosticsCacheIfNeeded(compilationHash, document.Id, immutableArray);
		return immutableArray;
	}

	private ImmutableArray<DiagnosticAnalyzer> GetAnalyzersWithAvailableCodeFixes(Project project)
	{
		ProjectAnalyzerReference? projectAnalyzerReference = projectAnalyzers;
		if (projectAnalyzerReference != null && projectAnalyzerReference.IsValidForProject(project))
		{
			return projectAnalyzers.Analyzers;
		}
		ImmutableArray<DiagnosticAnalyzer>.Builder builder = ImmutableArray.CreateBuilder<DiagnosticAnalyzer>();
		ImmutableArray<DiagnosticAnalyzer>.Enumerator enumerator = project.DiagnosticAnalyzers.GetEnumerator();
		while (enumerator.MoveNext())
		{
			DiagnosticAnalyzer current = enumerator.Current;
			ImmutableArray<DiagnosticDescriptor>.Enumerator enumerator2 = current.SupportedDiagnostics.GetEnumerator();
			while (enumerator2.MoveNext())
			{
				DiagnosticDescriptor current2 = enumerator2.Current;
				if (DiagnosticIdsWithFixes.Contains(current2.Id))
				{
					builder.Add(current);
					break;
				}
			}
		}
		projectAnalyzers = new ProjectAnalyzerReference(builder.ToImmutable(), project);
		return projectAnalyzers.Analyzers;
	}

	private ImmutableHashSet<string> BuildDiagnosticIdWithFixesSet()
	{
		ImmutableHashSet<string>.Builder builder = ImmutableHashSet.CreateBuilder<string>();
		ImmutableArray<CodeFixProvider>.Enumerator enumerator = CodeFixProviders.GetEnumerator();
		while (enumerator.MoveNext())
		{
			ImmutableArray<string>.Enumerator enumerator2 = enumerator.Current.FixableDiagnosticIds.GetEnumerator();
			while (enumerator2.MoveNext())
			{
				string current = enumerator2.Current;
				builder.Add(current);
			}
		}
		return builder.ToImmutable();
	}

	internal void UpdateDocumentDiagnosticsCacheIfNeeded(int compilationHash, DocumentId documentId, ImmutableArray<Diagnostic> diagnostics)
	{
		lock (cacheLock)
		{
			if (lastCompilationHash != compilationHash)
			{
				cachedDiagnosticsForDocument = new Tuple<DocumentId, ImmutableArray<Diagnostic>>(documentId, diagnostics);
				lastCompilationHash = compilationHash;
			}
		}
	}

	private bool DocumentDiagnosticCacheNeedsInvalidation(int compilationHash, DocumentId currentDocument)
	{
		if (object.Equals(compilationHash, lastCompilationHash))
		{
			if (!(CachedDocumentId == null))
			{
				return CachedDocumentId != currentDocument;
			}
			return true;
		}
		return true;
	}
}
