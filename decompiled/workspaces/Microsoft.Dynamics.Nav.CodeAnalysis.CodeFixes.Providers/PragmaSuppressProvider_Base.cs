using System;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.CodeAnalysis.CodeActions;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.CodeFixes.Providers;

public abstract class PragmaSuppressProvider_Base : CodeFixProvider
{
	private class PragmaSuppressCodeAction : CodeAction.DocumentChangeAction
	{
		public override CodeActionKind Kind => CodeActionKind.QuickFix;

		public PragmaSuppressCodeAction(string title, Func<CancellationToken, Task<Document>> createChangedDocument, string equivalenceKey)
			: base(title, createChangedDocument, equivalenceKey)
		{
		}
	}

	protected abstract string SuppressableDiagnosticId { get; }

	protected abstract SyntaxKind[] ExpectedNodeKinds { get; }

	protected abstract Func<SyntaxNode, SyntaxNode?> GetSuppressableParent { get; }

	public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(SuppressableDiagnosticId);

	protected virtual PragmaSuppressFixAllProviderKind FixAllProviderKind { get; } = PragmaSuppressFixAllProviderKind.Batch;


	public override FixAllProvider? GetFixAllProvider()
	{
		return FixAllProviderKind switch
		{
			PragmaSuppressFixAllProviderKind.Batch => WellKnownFixAllProviders.BatchFixer, 
			PragmaSuppressFixAllProviderKind.DocumentBased => new PragmaSuppressDocumentBasedFixAllDiagnosticProvider(SuppressableDiagnosticId, GetSuppressableParent, ExpectedNodeKinds), 
			_ => null, 
		};
	}

	public override async Task RegisterCodeFixesAsync(CodeFixContext context)
	{
		Document document = context.Document;
		SyntaxNode syntaxNode = (await document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(continueOnCapturedContext: false)).FindNode(context.Span, findInsideTrivia: false, getInnermostNodeForTie: true);
		if (ExpectedNodeKinds.Contains(syntaxNode.Kind))
		{
			SyntaxNode syntaxNode2 = GetSuppressableParent(syntaxNode);
			if (syntaxNode2 != null)
			{
				context.RegisterFixes(ImmutableArray.Create((CodeAction)CreateCodeAction(syntaxNode2, document)), context.Diagnostics);
			}
		}
	}

	protected SyntaxNode? GetAncestorOfKind(SyntaxNode syntaxNode, SyntaxKind ancestorKind)
	{
		if (syntaxNode.TryGetAncestorOfKind<SyntaxNode>(ancestorKind, out SyntaxNode ancestor) && ancestor != null)
		{
			return ancestor;
		}
		return null;
	}

	private async Task<Document> Update(Document document, SyntaxNode syntaxNode, CancellationToken cancellationToken)
	{
		SyntaxNode root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		SyntaxNode nodeWithPragmaSuppression = PragmaSuppressUtilities.GetNodeWithPragmaSuppression(syntaxNode, SuppressableDiagnosticId);
		SyntaxNode root2 = root.ReplaceNode(syntaxNode, nodeWithPragmaSuppression);
		return document.WithSyntaxRoot(root2);
	}

	private PragmaSuppressCodeAction CreateCodeAction(SyntaxNode syntaxNode, Document document)
	{
		Document document2 = document;
		SyntaxNode syntaxNode2 = syntaxNode;
		return new PragmaSuppressCodeAction(string.Format(CultureInfo.CurrentCulture, WorkspacesResources.SuppressDiagnostic, SuppressableDiagnosticId), (CancellationToken c) => Update(document2, syntaxNode2, c), "pragma_warning_disable_" + SuppressableDiagnosticId);
	}
}
