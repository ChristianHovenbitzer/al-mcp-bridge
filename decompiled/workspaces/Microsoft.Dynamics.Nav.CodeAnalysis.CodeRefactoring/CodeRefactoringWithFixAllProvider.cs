using System;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.CodeAnalysis.CodeActions;
using Microsoft.Dynamics.Nav.CodeAnalysis.CodeFixes;
using Microsoft.Dynamics.Nav.CodeAnalysis.Editing;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.CodeRefactoring;

public abstract class CodeRefactoringWithFixAllProvider : CodeRefactoringProvider
{
	protected abstract ImmutableArray<FixAllScope> SupportedFixAllScopes { get; }

	internal sealed override FixAllProvider? GetFixAllProvider()
	{
		if (SupportedFixAllScopes.IsEmpty)
		{
			return null;
		}
		return FixAllProvider.Create(async (FixAllContext fixAllContext, Document document, Optional<ImmutableArray<TextSpan>> fixAllSpans) => await FixAllAsync(fixAllContext, document, fixAllSpans, null, fixAllContext.CodeActionEquivalenceKey, fixAllContext.CancellationToken).ConfigureAwait(continueOnCapturedContext: false), SupportedFixAllScopes);
	}

	protected Task<Document> FixAllAsync(FixAllContext fixAllContext, Document document, Optional<ImmutableArray<TextSpan>> fixAllSpans, CodeActionOptionsProvider optionsProvider, string? equivalenceKey, CancellationToken cancellationToken)
	{
		FixAllContext fixAllContext2 = fixAllContext;
		Document document2 = document;
		CodeActionOptionsProvider optionsProvider2 = optionsProvider;
		string equivalenceKey2 = equivalenceKey;
		return FixAllWithEditorAsync(document2, FixAllAsync, cancellationToken);
		Task FixAllAsync(SyntaxEditor editor)
		{
			ImmutableArray<TextSpan> fixAllSpans2 = (fixAllSpans.HasValue ? fixAllSpans.Value : ImmutableArray.Create(editor.OriginalRoot.FullSpan));
			return this.FixAllAsync(fixAllContext2, document2, fixAllSpans2, editor, optionsProvider2, equivalenceKey2, cancellationToken);
		}
	}

	internal static async Task<Document> FixAllWithEditorAsync(Document document, Func<SyntaxEditor, Task> editAsync, CancellationToken cancellationToken)
	{
		if (document == null)
		{
			throw new ArgumentNullException("document");
		}
		if (editAsync == null)
		{
			throw new ArgumentNullException("editAsync");
		}
		SyntaxEditor editor = new SyntaxEditor(await document.GetRequiredSyntaxRootAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false));
		await editAsync(editor).ConfigureAwait(continueOnCapturedContext: false);
		SyntaxNode changedRoot = editor.GetChangedRoot();
		return document.WithSyntaxRoot(changedRoot);
	}

	protected abstract Task FixAllAsync(FixAllContext fixAllContext, Document document, ImmutableArray<TextSpan> fixAllSpans, SyntaxEditor editor, CodeActionOptionsProvider optionsProvider, string? equivalenceKey, CancellationToken cancellationToken);
}
