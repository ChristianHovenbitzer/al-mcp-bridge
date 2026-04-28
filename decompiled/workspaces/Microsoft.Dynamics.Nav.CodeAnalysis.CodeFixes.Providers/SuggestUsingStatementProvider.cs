using System;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.CodeAnalysis.CodeActions;
using Microsoft.Dynamics.Nav.CodeAnalysis.CodeActions.Mef;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.CodeFixes.Helpers;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.CodeFixes.Providers;

[CodeFixProvider("SuggestUsingStatementProvider")]
public class SuggestUsingStatementProvider : CodeFixProvider
{
	private class SuggestUsingStatementCodeAction : CodeAction.DocumentChangeAction
	{
		public override CodeActionKind Kind => CodeActionKind.QuickFix;

		public override bool SupportsFixAll { get; }

		public override string? FixAllSingleInstanceTitle => Title;

		public override string? FixAllTitle => string.Empty;

		public SuggestUsingStatementCodeAction(string title, Func<CancellationToken, Task<Document>> createChangedDocument, string equivalenceKey, bool generateFixAll)
			: base(title, createChangedDocument, equivalenceKey)
		{
			SupportsFixAll = generateFixAll;
		}
	}

	private static readonly ErrorCode[] fixableErrors = new ErrorCode[3]
	{
		ErrorCode.ERR_MissingApplicationObject,
		ErrorCode.ERR_ExtensionTargetNotFound,
		ErrorCode.ERR_NameNotInContext
	};

	public override ImmutableArray<string> FixableDiagnosticIds { get; } = fixableErrors.Select((ErrorCode t) => MessageProvider.Instance.GetIdForErrorCode((int)t)).ToImmutableArray();


	public override FixAllProvider? GetFixAllProvider()
	{
		return SuggestUsingStatementFixAllProvider.Instance;
	}

	public override async Task RegisterCodeFixesAsync(CodeFixContext context)
	{
		Document document = context.Document;
		TextSpan span = context.Span;
		CancellationToken cancellationToken = context.CancellationToken;
		SyntaxNode syntaxNode = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		if (syntaxNode.Kind == SyntaxKind.CompilationUnit)
		{
			CompilationUnitSyntax compilationUnitSyntax = (CompilationUnitSyntax)syntaxNode;
			if (VersionChecker.IsSupported(syntaxNode, Feature.Namespaces) && compilationUnitSyntax.NamespaceDeclaration != null)
			{
				await RegisterInstanceCodeFix(context, syntaxNode, span, document);
			}
		}
	}

	private async Task RegisterInstanceCodeFix(CodeFixContext context, SyntaxNode syntaxRoot, TextSpan span, Document document)
	{
		SyntaxNode node = syntaxRoot.FindNode(span);
		(SyntaxNode? IdentifierName, SymbolKind Kind)? identifierAndKind = NamespaceActionUtilities.GetIdentifierAndKind(node);
		if (!identifierAndKind.HasValue)
		{
			return;
		}
		string identifier = identifierAndKind.Value.IdentifierName?.GetIdentifierOrLiteralValue()?.UnquoteIdentifier();
		if (identifier == null)
		{
			return;
		}
		SemanticModel semanticModel = await document.GetSemanticModelAsync(context.CancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		ImmutableArray<ISymbol> candidateSymbolsFromIdentifier = NamespaceActionUtilities.GetCandidateSymbolsFromIdentifier(identifier, identifierAndKind.Value.Kind, semanticModel, context.CancellationToken);
		bool generateFixAll = candidateSymbolsFromIdentifier.Length == 1;
		foreach (ISymbol item in from s in candidateSymbolsFromIdentifier
			where s.ContainingNamespace != null
			select s into ao
			orderby ao.ToDisplayString()
			select ao)
		{
			context.RegisterFixes(ImmutableArray.Create((CodeAction)CreateCodeAction(item, document, generateFixAll)), context.Diagnostics);
		}
	}

	private SuggestUsingStatementCodeAction CreateCodeAction(ISymbol symbol, Document document, bool generateFixAll)
	{
		Document document2 = document;
		NameSyntax namespaceName = symbol.GetNamespacePartOfQualifiedNameSyntax();
		return new SuggestUsingStatementCodeAction(string.Format(CultureInfo.CurrentCulture, WorkspacesResources.AddUsingStatement, namespaceName), (CancellationToken c) => Update(document2, namespaceName, c), namespaceName.ToString(), generateFixAll);
	}

	private async Task<Document> Update(Document document, NameSyntax namespaceForUsing, CancellationToken cancellationToken)
	{
		SyntaxNode obj = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		CompilationUnitSyntax compilationUnitSyntax = (CompilationUnitSyntax)obj;
		UsingDirectiveSyntax usingDirective = NamespaceActionUtilities.CreateUsingStatement(namespaceForUsing);
		CompilationUnitSyntax newNode = NamespaceActionUtilities.AddUsing(compilationUnitSyntax, usingDirective);
		SyntaxNode root = obj.ReplaceNode(compilationUnitSyntax, newNode);
		return document.WithSyntaxRoot(root);
	}
}
