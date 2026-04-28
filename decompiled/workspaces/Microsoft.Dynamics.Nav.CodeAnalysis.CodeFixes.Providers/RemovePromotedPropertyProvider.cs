using System;
using System.Collections.Immutable;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.CodeAnalysis.CodeActions;
using Microsoft.Dynamics.Nav.CodeAnalysis.CodeActions.Mef;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.CodeFixes.Providers;

[CodeFixProvider("RemovePromotedPropertyProvider")]
public class RemovePromotedPropertyProvider : CodeFixProvider
{
	private class RemovePromotedPropertyCodeAction : CodeAction.DocumentChangeAction
	{
		public override CodeActionKind Kind => CodeActionKind.QuickFix;

		public RemovePromotedPropertyCodeAction(string title, Func<CancellationToken, Task<Document>> createChangedDocument, string equivalenceKey)
			: base(title, createChangedDocument, equivalenceKey)
		{
		}
	}

	private const ErrorCode FixableDiagnosticCode = ErrorCode.WRN_ERR_MissingAssociatedPropertyMultipleValues;

	public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(MessageProvider.Instance.GetIdForErrorCode(729));


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
		if (syntaxNode.IsKind(SyntaxKind.Property))
		{
			PropertySyntax propertySyntax = (PropertySyntax)syntaxNode;
			PropertyKind propertyKind = SyntaxFacts.GetPropertyKind(propertySyntax.Name.Identifier.ValueText);
			if (propertyKind.IsPromotedActionProperty() || propertyKind.IsAnyPromotedActionCategoriesProperty())
			{
				context.RegisterCodeFix(CreateCodeAction(propertySyntax.Parent, propertySyntax, document), context.Diagnostics[0]);
			}
		}
	}

	private RemovePromotedPropertyCodeAction CreateCodeAction(SyntaxNode previousSyntax, PropertySyntax promotedProperty, Document document)
	{
		Document document2 = document;
		SyntaxNode previousSyntax2 = previousSyntax;
		PropertySyntax promotedProperty2 = promotedProperty;
		return new RemovePromotedPropertyCodeAction(string.Format(CultureInfo.CurrentCulture, WorkspacesResources.RemovePromotedProperty), (CancellationToken c) => Update(document2, previousSyntax2, promotedProperty2, c), "remove_promoted_property");
	}

	private async Task<Document> Update(Document document, SyntaxNode previousSyntax, PropertySyntax promotedProperty, CancellationToken cancellationToken)
	{
		SyntaxNode root = Microsoft.Dynamics.Nav.CodeAnalysis.Syntax.SyntaxNodeExtensions.ReplaceNode(newNode: previousSyntax.RemoveNode(promotedProperty, SyntaxRemoveOptions.KeepNoTrivia), root: await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false), oldNode: previousSyntax);
		return document.WithSyntaxRoot(root);
	}
}
