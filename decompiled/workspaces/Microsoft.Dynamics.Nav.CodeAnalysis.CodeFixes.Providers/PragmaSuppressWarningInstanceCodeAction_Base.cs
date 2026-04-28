using System;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.CodeAnalysis.CodeActions;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.CodeFixes.Providers;

public abstract class PragmaSuppressWarningInstanceCodeAction_Base : CodeFixProvider
{
	private class PragmaSuppressWarningInstanceCodeAction : CodeAction.DocumentChangeAction
	{
		public override CodeActionKind Kind => CodeActionKind.QuickFix;

		public PragmaSuppressWarningInstanceCodeAction(string title, Func<CancellationToken, Task<Document>> createChangedDocument, string equivalenceKey)
			: base(title, createChangedDocument, equivalenceKey)
		{
		}
	}

	public abstract ImmutableHashSet<string> warningIds { get; }

	public override FixAllProvider? GetFixAllProvider()
	{
		return null;
	}

	public override async Task RegisterCodeFixesAsync(CodeFixContext context)
	{
		Document document = context.Document;
		SyntaxNode syntaxNode = (await document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(continueOnCapturedContext: false)).FindNode(context.Span);
		ImmutableArray<string> diagnosticIds = (from d in context.Diagnostics
			select d.Id into diagnosticId
			where warningIds.Contains(diagnosticId)
			select diagnosticId).ToImmutableArray();
		if (!diagnosticIds.IsEmpty)
		{
			context.RegisterFixes(CreateCodeActions(syntaxNode, document, diagnosticIds), context.Diagnostics);
		}
	}

	private async Task<Document> Update(Document document, SyntaxNode syntaxNode, CancellationToken cancellationToken, string diagnosticId)
	{
		SyntaxNode root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		int line = syntaxNode.GetLocation().GetLineSpan().StartLinePosition.Line;
		int line2 = syntaxNode.GetLocation().GetLineSpan().EndLinePosition.Line;
		SyntaxNode ancesstorWrappingLine = PragmaSuppressUtilities.GetAncesstorWrappingLine(syntaxNode, line);
		SyntaxNode nodeWithPragmaSuppressWrappedLine = PragmaSuppressUtilities.GetNodeWithPragmaSuppressWrappedLine(ancesstorWrappingLine, line, line2, diagnosticId);
		SyntaxNode root2 = root.ReplaceNode(ancesstorWrappingLine, nodeWithPragmaSuppressWrappedLine);
		return document.WithSyntaxRoot(root2);
	}

	private ImmutableArray<CodeAction> CreateCodeActions(SyntaxNode syntaxNode, Document document, ImmutableArray<string> diagnosticIds)
	{
		Document document2 = document;
		SyntaxNode syntaxNode2 = syntaxNode;
		return diagnosticIds.Select((Func<string, CodeAction>)((string diagnosticId) => new PragmaSuppressWarningInstanceCodeAction(string.Format(CultureInfo.CurrentCulture, WorkspacesResources.SuppressDiagnostic, diagnosticId), (CancellationToken c) => Update(document2, syntaxNode2, c, diagnosticId), "pragma_warning_disable_" + diagnosticId))).ToImmutableArray();
	}
}
