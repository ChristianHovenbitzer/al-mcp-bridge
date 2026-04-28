using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.CodeAnalysis.CodeActions;
using Microsoft.Dynamics.Nav.CodeAnalysis.CodeActions.Mef;
using Microsoft.Dynamics.Nav.CodeAnalysis.CodeFixes;
using Microsoft.Dynamics.Nav.CodeAnalysis.CodeRefactoring;
using Microsoft.Dynamics.Nav.CodeAnalysis.Editing;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.ActionV1Converter;

[CodeRefactoringProvider("ActionV1Converter")]
public sealed class ActionV1ConverterCodeFixProvider : CodeRefactoringWithFixAllProvider
{
	private sealed class Analyser
	{
		private sealed class MyCodeAction : CodeAction.DocumentChangeAction
		{
			public override CodeActionKind Kind => CodeActionKind.Refactor;

			public MyCodeAction(string title, Func<CancellationToken, Task<Document>> createChangedDocument)
				: base(title, createChangedDocument)
			{
			}
		}

		public void ComputeRefactorings(Document document, ApplicationObjectSyntax applicationObjectSyntax, CodeRefactoringContext context)
		{
			Document document2 = document;
			ApplicationObjectSyntax applicationObjectSyntax2 = applicationObjectSyntax;
			context.RegisterRefactoring(new MyCodeAction(WorkspacesResources.ConvertPageToActionV2, (CancellationToken c) => ConvertAsync(document2, ImmutableArray.Create(applicationObjectSyntax2), context.CancellationToken)));
		}

		public static async Task<Document> ConvertAsync(Document document, ImmutableArray<ApplicationObjectSyntax> applicationObjectSyntaxes, CancellationToken cancellationToken)
		{
			return document.WithSyntaxRoot(await GetConvertedRootAsync(document, applicationObjectSyntaxes, cancellationToken).ConfigureAwait(continueOnCapturedContext: false));
		}

		public static async Task<SyntaxNode> GetConvertedRootAsync(Document document, ImmutableArray<ApplicationObjectSyntax> applicationObjectSyntaxes, CancellationToken cancellationToken)
		{
			PooledDictionary<SyntaxNode, SyntaxNode> nodeMap = PooledDictionary<SyntaxNode, SyntaxNode>.GetInstance();
			try
			{
				ImmutableArray<ApplicationObjectSyntax>.Enumerator enumerator = applicationObjectSyntaxes.GetEnumerator();
				while (enumerator.MoveNext())
				{
					ApplicationObjectSyntax applicationObjectSyntax = enumerator.Current;
					ApplicationObjectSyntax applicationObjectSyntax2 = null;
					if (applicationObjectSyntax.Kind == SyntaxKind.PageObject)
					{
						PageSyntax pageSyntax = (PageSyntax)applicationObjectSyntax;
						applicationObjectSyntax2 = ActionBarSyntaxConverter.ConvertPage((await document.GetSymbolAtPositionAsync(pageSyntax.GetIdentifierNameSyntax().SpanStart, cancellationToken).ConfigureAwait(continueOnCapturedContext: false)) as PageTypeSymbol, pageSyntax);
					}
					else if (applicationObjectSyntax.Kind == SyntaxKind.PageExtensionObject || applicationObjectSyntax.Kind == SyntaxKind.PageCustomizationObject)
					{
						AbstractPageExtensionSyntax pageExtSyntax = (AbstractPageExtensionSyntax)applicationObjectSyntax;
						applicationObjectSyntax2 = ActionBarSyntaxConverter.ConvertPageExt((await document.GetSymbolAtPositionAsync(pageExtSyntax.GetIdentifierNameSyntax().SpanStart, cancellationToken).ConfigureAwait(continueOnCapturedContext: false)) as PageExtensionBaseTypeSymbol, pageExtSyntax);
					}
					if (applicationObjectSyntax2 != null)
					{
						nodeMap.Add(applicationObjectSyntax, applicationObjectSyntax2);
					}
				}
				return (await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false)).ReplaceNodes(nodeMap.Keys, (SyntaxNode o, SyntaxNode n) => nodeMap[o]);
			}
			finally
			{
				nodeMap.Free();
			}
		}
	}

	private static readonly HashSet<SyntaxKind> parentNodesContainingActionsSet = new HashSet<SyntaxKind>
	{
		SyntaxKind.PageActionList,
		SyntaxKind.PageActionArea,
		SyntaxKind.ActionModifyChange,
		SyntaxKind.ActionMoveChange,
		SyntaxKind.ActionAddChange,
		SyntaxKind.PageExtensionActionList
	};

	protected sealed override ImmutableArray<FixAllScope> SupportedFixAllScopes => ImmutableArray.Create(FixAllScope.Document, FixAllScope.Project, FixAllScope.Workspace);

	public sealed override async Task ComputeRefactoringsAsync(CodeRefactoringContext context)
	{
		Document document = context.Document;
		SyntaxNode syntaxNode = await document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		if (!VersionChecker.IsSupported(syntaxNode, Feature.ActionsV2))
		{
			return;
		}
		SyntaxNode node = syntaxNode.FindNode(context.Span);
		if (ShouldShowActionOnNode(node))
		{
			ApplicationObjectSyntax containingApplicationObjectSyntax = node.GetContainingApplicationObjectSyntax();
			if (containingApplicationObjectSyntax != null && IsValidKindForConversion(containingApplicationObjectSyntax) && ShouldBeConverted(containingApplicationObjectSyntax))
			{
				CreateAnalyser().ComputeRefactorings(document, containingApplicationObjectSyntax, context);
			}
		}
	}

	protected sealed override async Task FixAllAsync(FixAllContext fixAllContext, Document document, ImmutableArray<TextSpan> fixAllSpans, SyntaxEditor editor, CodeActionOptionsProvider optionsProvider, string? equivalenceKey, CancellationToken cancellationToken)
	{
		SyntaxNode syntaxNode = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		if (syntaxNode == null)
		{
			return;
		}
		ArrayBuilder<ApplicationObjectSyntax> builder = ArrayBuilder<ApplicationObjectSyntax>.GetInstance();
		try
		{
			foreach (SyntaxNode item in syntaxNode.ChildNodes())
			{
				if (item is ApplicationObjectSyntax applicationObjectSyntax && IsValidKindForConversion(applicationObjectSyntax) && ShouldBeConverted(applicationObjectSyntax))
				{
					builder.Add(applicationObjectSyntax);
				}
			}
			SyntaxNode newNode = await Analyser.GetConvertedRootAsync(document, builder.ToImmutable(), cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
			editor.ReplaceNode(editor.OriginalRoot, newNode);
		}
		finally
		{
			builder.Free();
		}
	}

	private static bool IsValidKindForConversion(ApplicationObjectSyntax applicationObjectSyntax)
	{
		if ((applicationObjectSyntax.Kind != SyntaxKind.PageObject || !IsActionsV2Allowed((PageSyntax)applicationObjectSyntax)) && applicationObjectSyntax.Kind != SyntaxKind.PageExtensionObject)
		{
			return applicationObjectSyntax.Kind == SyntaxKind.PageCustomizationObject;
		}
		return true;
	}

	private static bool IsValidKindForConversion(SyntaxKind syntaxKind)
	{
		if (syntaxKind != SyntaxKind.PageObject && syntaxKind != SyntaxKind.PageExtensionObject)
		{
			return syntaxKind == SyntaxKind.PageCustomizationObject;
		}
		return true;
	}

	private static bool ShouldBeConverted(ApplicationObjectSyntax applicationObjectSyntax)
	{
		if (applicationObjectSyntax != null && !ActionBarSyntaxConverter.IsUsingActionRefSyntax(applicationObjectSyntax))
		{
			return ActionBarSyntaxConverter.IsUsingPromotedActionSyntax(applicationObjectSyntax);
		}
		return false;
	}

	private static bool ShouldShowActionOnNode(SyntaxNode node)
	{
		if (node == null)
		{
			return false;
		}
		if (node.IsAnyParentKind(parentNodesContainingActionsSet))
		{
			return true;
		}
		if (node.IsKind(SyntaxKind.IdentifierName) && IsValidKindForConversion(node.Parent.Kind))
		{
			return true;
		}
		SyntaxNode firstParent = node.GetFirstParent(SyntaxKind.Property);
		if (firstParent == null)
		{
			return false;
		}
		if (SyntaxFacts.GetPagePropertyKind(((PropertySyntax)firstParent).Name?.Identifier.ValueText).IsAnyPromotedActionCategoriesProperty())
		{
			return true;
		}
		return false;
	}

	private static bool IsActionsV2Allowed(PageSyntax pageObject)
	{
		return SemanticFacts.IsPromotedAreaAllowed(pageObject.GetEnumPropertyValue(PropertyKind.PageType, PageTypeKind.Card));
	}

	private Analyser CreateAnalyser()
	{
		return new Analyser();
	}
}
