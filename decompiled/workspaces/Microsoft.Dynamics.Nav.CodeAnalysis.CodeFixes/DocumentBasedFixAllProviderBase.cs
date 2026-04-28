using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.CodeAnalysis.CodeActions;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Formatting;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.CodeFixes;

public abstract class DocumentBasedFixAllProviderBase<T> : FixAllProvider
{
	private readonly ImmutableArray<FixAllScope> supportedFixAllScopes;

	protected DocumentBasedFixAllProviderBase()
		: this(FixAllProvider.DefaultSupportedFixAllScopes)
	{
	}

	protected DocumentBasedFixAllProviderBase(ImmutableArray<FixAllScope> supportedFixAllScopes)
	{
		this.supportedFixAllScopes = supportedFixAllScopes;
	}

	protected abstract Task<Document?> FixAllAsync(FixAllContext fixAllContext, Document document, ImmutableArray<T> codeLocations);

	protected abstract Task<ImmutableDictionary<Document, ImmutableArray<T>>> GetCodeLocations(FixAllContext fixAllContext, CancellationToken token);

	protected virtual string GetFixAllTitle(FixAllContext fixAllContext)
	{
		return fixAllContext.GetDefaultFixAllTitle();
	}

	public sealed override IEnumerable<FixAllScope> GetSupportedFixAllScopes()
	{
		return supportedFixAllScopes;
	}

	public sealed override Task<CodeAction?> GetFixAsync(FixAllContext fixAllContext)
	{
		return DefaultFixAllProviderHelpers.GetFixAsync(fixAllContext.GetDefaultFixAllTitle(), fixAllContext, FixAllContextsHelperAsync);
	}

	private Task<Solution?> FixAllContextsHelperAsync(FixAllContext originalFixAllContext, ImmutableArray<FixAllContext> fixAllContexts)
	{
		return DocumentBasedFixAllProviderHelpers.FixAllContextsAsync(originalFixAllContext, fixAllContexts, GetFixAllTitle(originalFixAllContext), GetFixedDocumentsAsync);
	}

	private async Task<ImmutableDictionary<Document, ImmutableArray<T>>> GetFilteredCodeLocations(FixAllContext fixAllContext, CancellationToken token)
	{
		FixAllContext fixAllContext2 = fixAllContext;
		return (await GetCodeLocations(fixAllContext2, token).ConfigureAwait(continueOnCapturedContext: false)).Where(delegate(KeyValuePair<Document, ImmutableArray<T>> kvp)
		{
			if (kvp.Key.Project != fixAllContext2.Project)
			{
				return false;
			}
			return (IsProjectOrWorkspaceScope(fixAllContext2.Scope) || fixAllContext2.Document == null || fixAllContext2.Document == kvp.Key) ? true : false;
		}).ToImmutableDictionary();
	}

	private static bool IsProjectOrWorkspaceScope(FixAllScope scope)
	{
		if (scope != FixAllScope.Project)
		{
			return scope == FixAllScope.Workspace;
		}
		return true;
	}

	private async Task<IDictionary<DocumentId, (SyntaxNode? node, SourceText? text)>> GetFixedDocumentsAsync(FixAllContext fixAllContext)
	{
		FixAllContext fixAllContext2 = fixAllContext;
		Contract.ThrowIfFalse(fixAllContext2.Scope == FixAllScope.Document || fixAllContext2.Scope == FixAllScope.Project);
		CancellationToken cancellationToken = fixAllContext2.CancellationToken;
		ConcurrentDictionary<DocumentId, (SyntaxNode? node, SourceText? text)> docIdToNewRootOrText = new ConcurrentDictionary<DocumentId, (SyntaxNode, SourceText)>();
		ImmutableDictionary<Document, ImmutableArray<T>> immutableDictionary = await GetFilteredCodeLocations(fixAllContext2, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		TaskExecutor.GetSuitableInstance(immutableDictionary.Count).ForEach(immutableDictionary, async delegate(KeyValuePair<Document, ImmutableArray<T>> n)
		{
			Document document = n.Key;
			ImmutableArray<T> value = n.Value;
			Document newDocument = await FixAllAsync(fixAllContext2, document, value).ConfigureAwait(continueOnCapturedContext: false);
			if (newDocument != null && newDocument != document)
			{
				SourceText text = null;
				SyntaxNode node = await newDocument.GetRequiredSyntaxRootAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
				if (node == null)
				{
					SourceText sourceText = ((!newDocument.SupportsSyntaxTree) ? (await newDocument.GetTextAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false)) : null);
					text = sourceText;
				}
				if (document.Id != null)
				{
					docIdToNewRootOrText[document.Id] = (node, text);
				}
			}
		}, cancellationToken);
		return docIdToNewRootOrText;
	}
}
