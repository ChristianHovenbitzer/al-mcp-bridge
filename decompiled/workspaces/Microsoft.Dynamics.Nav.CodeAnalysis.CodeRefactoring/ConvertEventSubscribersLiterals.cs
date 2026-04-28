using System;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.CodeAnalysis.CodeActions;
using Microsoft.Dynamics.Nav.CodeAnalysis.CodeActions.Mef;
using Microsoft.Dynamics.Nav.CodeAnalysis.CodeFixes;
using Microsoft.Dynamics.Nav.CodeAnalysis.Editing;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.CodeRefactoring;

[CodeRefactoringProvider("EventSubscriberConvert")]
public sealed class ConvertEventSubscribersLiterals : CodeRefactoringWithFixAllProvider
{
	private class ConvertEventSubscriberLiterals : CodeAction.DocumentChangeAction
	{
		public override CodeActionKind Kind => CodeActionKind.QuickFix;

		public ConvertEventSubscriberLiterals(string title, Func<CancellationToken, Task<Document>> createChangedDocument, string equivalenceKey)
			: base(title, createChangedDocument, equivalenceKey)
		{
		}
	}

	protected override ImmutableArray<FixAllScope> SupportedFixAllScopes => ImmutableArray.Create(FixAllScope.Document, FixAllScope.Project, FixAllScope.Workspace);

	public sealed override async Task ComputeRefactoringsAsync(CodeRefactoringContext context)
	{
		Document document = context.Document;
		SyntaxNode syntaxNode = await document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		if (!VersionChecker.IsSupported(syntaxNode, Feature.IdentifiersInEventSubscribers))
		{
			return;
		}
		LiteralAttributeArgumentSyntax attributeArgumentSyntax = syntaxNode.FindNode(context.Span).FirstAncestorOrSelf<LiteralAttributeArgumentSyntax>();
		if (await ShouldConvertAttributeArgument(document, attributeArgumentSyntax, context.CancellationToken) && !string.IsNullOrEmpty(attributeArgumentSyntax.GetIdentifierOrLiteralValue()))
		{
			context.RegisterRefactoring(new RefactorCodeAction(WorkspacesResources.ConvertToIdentifier, (CancellationToken c) => GetConvertedRootAsync(document, attributeArgumentSyntax, context.CancellationToken)));
		}
	}

	private async Task<bool> ShouldConvertAttributeArgument(Document document, LiteralAttributeArgumentSyntax attributeArgumentSyntax, CancellationToken cancellationToken)
	{
		if (attributeArgumentSyntax == null || attributeArgumentSyntax.Kind != SyntaxKind.LiteralAttributeArgument)
		{
			return false;
		}
		LiteralExpressionSyntax literal = attributeArgumentSyntax.Literal;
		if (literal == null || literal.Literal?.Kind != SyntaxKind.StringLiteralValue)
		{
			return false;
		}
		if (string.IsNullOrEmpty(attributeArgumentSyntax.GetIdentifierOrLiteralValue()))
		{
			return false;
		}
		MemberAttributeSyntax memberAttributeSyntax = (MemberAttributeSyntax)attributeArgumentSyntax.GetFirstParent(SyntaxKind.MemberAttribute);
		ISymbol symbol = await document.GetSymbolAtPositionAsync(memberAttributeSyntax.Name.Position, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		if (symbol == null || symbol.Kind != SymbolKind.Attribute)
		{
			return false;
		}
		AttributeTypeInfo attributeInfo = ((AttributeSymbol)symbol).AttributeInfo;
		return attributeInfo != null && attributeInfo.Category == AttributeCategory.EventSubscriber;
	}

	protected override async Task FixAllAsync(FixAllContext fixAllContext, Document document, ImmutableArray<TextSpan> fixAllSpans, SyntaxEditor editor, CodeActionOptionsProvider optionsProvider, string? equivalenceKey, CancellationToken cancellationToken)
	{
		SyntaxNode syntaxNode = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		if (syntaxNode == null)
		{
			return;
		}
		ArrayBuilder<LiteralAttributeArgumentSyntax> builder = ArrayBuilder<LiteralAttributeArgumentSyntax>.GetInstance();
		try
		{
			foreach (SyntaxNode item in syntaxNode.ChildNodes())
			{
				if (item.Kind != SyntaxKind.CodeunitObject)
				{
					continue;
				}
				foreach (SyntaxNode item2 in item.DescendantNodes())
				{
					if (item2.Kind == SyntaxKind.LiteralAttributeArgument)
					{
						LiteralAttributeArgumentSyntax literalAttributeArgument = (LiteralAttributeArgumentSyntax)item2;
						if (await ShouldConvertAttributeArgument(document, literalAttributeArgument, cancellationToken))
						{
							builder.Add(literalAttributeArgument);
						}
					}
				}
			}
			SyntaxNode newNode = await GetConvertedRootAsync(document, builder.ToImmutable(), cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
			editor.ReplaceNode(editor.OriginalRoot, newNode);
		}
		finally
		{
			builder.Free();
		}
	}

	private static async Task<Document> GetConvertedRootAsync(Document document, LiteralAttributeArgumentSyntax literalAttributeArgumentSyntax, CancellationToken cancellationToken)
	{
		return document.WithSyntaxRoot(await GetConvertedRootAsync(document, ImmutableArray.Create(literalAttributeArgumentSyntax), cancellationToken).ConfigureAwait(continueOnCapturedContext: false));
	}

	private static async Task<SyntaxNode> GetConvertedRootAsync(Document document, ImmutableArray<LiteralAttributeArgumentSyntax> literalAttributeArgumentSyntaxes, CancellationToken cancellationToken)
	{
		PooledDictionary<SyntaxNode, SyntaxNode> nodeMap = PooledDictionary<SyntaxNode, SyntaxNode>.GetInstance();
		try
		{
			ImmutableArray<LiteralAttributeArgumentSyntax>.Enumerator enumerator = literalAttributeArgumentSyntaxes.GetEnumerator();
			while (enumerator.MoveNext())
			{
				LiteralAttributeArgumentSyntax current = enumerator.Current;
				IdentifierAttributeArgumentSyntax value = SyntaxFactory.IdentifierAttributeArgument(SyntaxFactory.ObjectNameReference(SyntaxFactory.IdentifierName(current.GetIdentifierOrLiteralValue())));
				nodeMap.Add(current, value);
			}
			return (await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false)).ReplaceNodes(nodeMap.Keys, (SyntaxNode o, SyntaxNode n) => nodeMap[o]);
		}
		finally
		{
			nodeMap.Free();
		}
	}
}
