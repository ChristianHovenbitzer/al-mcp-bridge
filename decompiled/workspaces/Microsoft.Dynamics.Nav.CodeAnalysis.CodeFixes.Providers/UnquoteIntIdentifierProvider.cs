using System;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.CodeAnalysis.CodeActions;
using Microsoft.Dynamics.Nav.CodeAnalysis.CodeActions.Mef;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.CodeFixes.Providers;

[CodeFixProvider("UnquoteIntIdentifierProvider")]
public class UnquoteIntIdentifierProvider : CodeFixProvider
{
	private class RemoveQuotesCodeAction : CodeAction.DocumentChangeAction
	{
		public override CodeActionKind Kind => CodeActionKind.QuickFix;

		public RemoveQuotesCodeAction(string title, Func<CancellationToken, Task<Document>> createChangedDocument, string equivalenceKey)
			: base(title, createChangedDocument, equivalenceKey)
		{
		}
	}

	private static readonly ErrorCode[] fixableErrors = new ErrorCode[2]
	{
		ErrorCode.ERR_ObjectIdWithQuotesNotSupported,
		ErrorCode.WRN_ERR_ObjectIdWithQuotesNotSupported
	};

	public override ImmutableArray<string> FixableDiagnosticIds { get; } = fixableErrors.Select((ErrorCode t) => MessageProvider.Instance.GetIdForErrorCode((int)t)).ToImmutableArray();


	public override FixAllProvider? GetFixAllProvider()
	{
		return WellKnownFixAllProviders.BatchFixer;
	}

	public override async Task RegisterCodeFixesAsync(CodeFixContext context)
	{
		Document document = context.Document;
		TextSpan span = context.Span;
		CancellationToken cancellationToken = context.CancellationToken;
		SyntaxNode syntaxRoot = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		RegisterInstanceCodeFix(context, syntaxRoot, span, document);
	}

	private void RegisterInstanceCodeFix(CodeFixContext context, SyntaxNode syntaxRoot, TextSpan span, Document document)
	{
		SyntaxNode syntaxNode = syntaxRoot.FindNode(span, findInsideTrivia: false, getInnermostNodeForTie: true);
		if (syntaxNode.IsKind(SyntaxKind.IdentifierName) && syntaxNode.TryGetAncestorOfKind<ObjectNameOrIdSyntax>(SyntaxKind.ObjectReference, out ObjectNameOrIdSyntax ancestor) && int.TryParse(((IdentifierNameSyntax)ancestor.Identifier).Unquoted(), out var result))
		{
			context.RegisterCodeFix(CreateCodeAction(result, ancestor, document), context.Diagnostics[0]);
		}
	}

	private RemoveQuotesCodeAction CreateCodeAction(int id, ObjectNameOrIdSyntax previousSyntax, Document document)
	{
		Document document2 = document;
		ObjectNameOrIdSyntax previousSyntax2 = previousSyntax;
		return new RemoveQuotesCodeAction(string.Format(CultureInfo.CurrentCulture, WorkspacesResources.RemoveQuotesFromIdentifier), (CancellationToken c) => Update(document2, id, previousSyntax2, c), "remove_quotes_" + id);
	}

	private async Task<Document> Update(Document document, int id, ObjectNameOrIdSyntax previousIdentifier, CancellationToken cancellationToken)
	{
		SyntaxNode root = (await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false)).ReplaceNode(previousIdentifier, SyntaxFactory.ObjectNameOrId(SyntaxFactory.ObjectId(id)));
		return document.WithSyntaxRoot(root);
	}
}
