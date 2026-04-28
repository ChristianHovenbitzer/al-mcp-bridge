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
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.CodeFixes.Providers.SuggestObjectTypeFromNamespace;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.CodeFixes.Providers;

[CodeFixProvider("SuggestObjectTypeFromNamespaceProvider")]
public class SuggestObjectTypeFromNamespaceProvider : CodeFixProvider
{
	private class SuggestObjectTypeFromNamespaceCodeAction : CodeAction.DocumentChangeAction
	{
		public override CodeActionKind Kind => CodeActionKind.QuickFix;

		public override bool SupportsFixAll { get; }

		public override string? FixAllTitle => string.Empty;

		public override string? FixAllSingleInstanceTitle => Title;

		public SuggestObjectTypeFromNamespaceCodeAction(string title, Func<CancellationToken, Task<Document>> createChangedDocument, string equivalenceKey, bool generateFixAll)
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
		return SuggestObjectTypeFromNamespaceFixAllProvider.Instance;
	}

	public override async Task RegisterCodeFixesAsync(CodeFixContext context)
	{
		Document document = context.Document;
		TextSpan span = context.Span;
		CancellationToken cancellationToken = context.CancellationToken;
		SyntaxNode syntaxNode = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		if (!VersionChecker.IsSupported(syntaxNode, Feature.Namespaces))
		{
			return;
		}
		SyntaxNode node = syntaxNode.FindNode(span);
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
		SemanticModel semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		ImmutableArray<ISymbol> candidateSymbolsFromIdentifier = NamespaceActionUtilities.GetCandidateSymbolsFromIdentifier(identifier, identifierAndKind.Value.Kind, semanticModel, context.CancellationToken);
		bool generateFixAll = candidateSymbolsFromIdentifier.Length == 1;
		foreach (ISymbol item in candidateSymbolsFromIdentifier.OrderBy((ISymbol ao) => ao.ToDisplayString()))
		{
			context.RegisterFixes(ImmutableArray.Create((CodeAction)CreateCodeAction(identifierAndKind.Value.IdentifierName, item, document, generateFixAll)), context.Diagnostics);
		}
	}

	private SuggestObjectTypeFromNamespaceCodeAction CreateCodeAction(SyntaxNode node, ISymbol symbol, Document document, bool generateFixAll)
	{
		Document document2 = document;
		SyntaxNode node2 = node;
		ISymbol symbol2 = symbol;
		string text = symbol2.ToDisplayString(SymbolDisplayFormat.QualifiedNameOnlyFormat);
		return new SuggestObjectTypeFromNamespaceCodeAction(string.Format(CultureInfo.CurrentCulture, WorkspacesResources.FullyQualifyObjectTypeSymbol, text), (CancellationToken c) => Update(document2, node2, symbol2, c), text, generateFixAll);
	}

	private async Task<Document> Update(Document document, SyntaxNode node, ISymbol symbol, CancellationToken cancellationToken)
	{
		SyntaxNode root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		NameSyntax newNode = symbol.GetNamespaceAndNameQualifiedNameSyntax().WithTriviaFrom(node);
		SyntaxNode root2 = root.ReplaceNode(node, newNode);
		return document.WithSyntaxRoot(root2);
	}
}
