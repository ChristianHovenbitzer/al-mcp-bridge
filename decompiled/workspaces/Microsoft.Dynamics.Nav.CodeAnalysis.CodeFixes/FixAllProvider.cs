using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.CodeAnalysis.CodeActions;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.CodeFixes;

public abstract class FixAllProvider
{
	private class CallbackDocumentBasedFixAllProvider : DocumentBasedFixAllSpansProvider
	{
		private readonly Func<FixAllContext, Document, Optional<ImmutableArray<TextSpan>>, Task<Document?>> fixAllAsync;

		public CallbackDocumentBasedFixAllProvider(Func<FixAllContext, Document, Optional<ImmutableArray<TextSpan>>, Task<Document?>> fixAllAsync, ImmutableArray<FixAllScope> supportedFixAllScopes)
			: base(supportedFixAllScopes)
		{
			this.fixAllAsync = fixAllAsync;
		}

		protected override Task<Document?> FixAllAsync(FixAllContext fixAllContext, Document document, ImmutableArray<TextSpan> fixAllSpans)
		{
			return fixAllAsync(fixAllContext, document, fixAllSpans);
		}
	}

	private protected static ImmutableArray<FixAllScope> DefaultSupportedFixAllScopes = ImmutableArray.Create(FixAllScope.Document, FixAllScope.Project, FixAllScope.Workspace);

	public virtual IEnumerable<FixAllScope> GetSupportedFixAllScopes()
	{
		return DefaultSupportedFixAllScopes;
	}

	public virtual IEnumerable<string> GetSupportedFixAllDiagnosticIds(CodeFixProvider originalCodeFixProvider)
	{
		return originalCodeFixProvider.FixableDiagnosticIds;
	}

	public virtual string? GetOverrideFixAllTitle(FixAllScope scope)
	{
		return null;
	}

	public abstract Task<CodeAction?> GetFixAsync(FixAllContext fixAllContext);

	public static FixAllProvider Create(Func<FixAllContext, Document, Optional<ImmutableArray<TextSpan>>, Task<Document?>> fixAllAsync)
	{
		return Create(fixAllAsync, DefaultSupportedFixAllScopes);
	}

	public static FixAllProvider Create(Func<FixAllContext, Document, Optional<ImmutableArray<TextSpan>>, Task<Document?>> fixAllAsync, ImmutableArray<FixAllScope> supportedFixAllScopes)
	{
		if (fixAllAsync == null)
		{
			throw new ArgumentNullException("fixAllAsync");
		}
		if (supportedFixAllScopes.IsDefault)
		{
			throw new ArgumentNullException("supportedFixAllScopes");
		}
		return new CallbackDocumentBasedFixAllProvider(fixAllAsync, supportedFixAllScopes);
	}
}
