using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.CodeAnalysis.CodeActions;
using Microsoft.Dynamics.Nav.CodeAnalysis.CodeActions.Mef;
using Microsoft.Dynamics.Nav.CodeAnalysis.CodeFixes;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.QualifyImplicitWith;

[CodeFixProvider("QualifyImplicitWithCodeFixProvider")]
public class QualifyImplicitWithCodeFixProvider : CodeFixProvider
{
	private class QualifyImplicitWithCodeAction : CodeAction.DocumentChangeAction
	{
		public override CodeActionKind Kind => CodeActionKind.QuickFix;

		public override bool SupportsFixAll { get; }

		public override string? FixAllTitle => string.Empty;

		public override string? FixAllSingleInstanceTitle => Title;

		public QualifyImplicitWithCodeAction(string title, Func<CancellationToken, Task<Document>> createChangedDocument, string equivalenceKey, bool generateFixAll)
			: base(title, createChangedDocument, equivalenceKey)
		{
			SupportsFixAll = generateFixAll;
		}
	}

	internal class QualifyImplicitWithFixAllProvider : DocumentBasedFixAllByDiagnosticsProvider
	{
		private static readonly ImmutableArray<FixAllScope> supportedFixAllScopes = new FixAllScope[3]
		{
			FixAllScope.Document,
			FixAllScope.Project,
			FixAllScope.Workspace
		}.ToImmutableArray();

		public static QualifyImplicitWithFixAllProvider Instance { get; } = new QualifyImplicitWithFixAllProvider();


		public QualifyImplicitWithFixAllProvider()
			: base(supportedFixAllScopes)
		{
		}

		public override string? GetOverrideFixAllTitle(FixAllScope scope)
		{
			return string.Format(CultureInfo.CurrentCulture, WorkspacesResources.QualifyAllImplicitWith, scope.ToDisplayString().ToLower());
		}

		protected override async Task<Document?> FixAllAsync(FixAllContext fixAllContext, Document document, ImmutableArray<Diagnostic> diagnostics)
		{
			SyntaxNode syntaxNode = QualifyIdentifierNameSyntaxNodes(await document.GetSyntaxRootAsync(fixAllContext.CancellationToken).ConfigureAwait(continueOnCapturedContext: false), diagnostics, null);
			if (fixAllContext.Scope == FixAllScope.Document)
			{
				syntaxNode = syntaxNode.WithLeadingTrivia(syntaxNode.GetLeadingTrivia().Concat(SyntaxFactory.ParseLeadingTrivia("#pragma implicitwith disable" + Environment.NewLine))).WithTrailingTrivia(SyntaxFactory.ParseLeadingTrivia(Environment.NewLine + "#pragma implicitwith restore").Concat(syntaxNode.GetTrailingTrivia()));
			}
			return document.WithSyntaxRoot(syntaxNode);
		}
	}

	private static readonly ErrorCode[] fixableErrors = new ErrorCode[2]
	{
		ErrorCode.WRN_ERR_UseOfImplicitWith,
		ErrorCode.HDN_UseOfImplicitWith
	};

	public override ImmutableArray<string> FixableDiagnosticIds { get; } = fixableErrors.Select((ErrorCode t) => MessageProvider.Instance.GetIdForErrorCode((int)t)).ToImmutableArray();


	private static string ExtractQualifierFromDiagnostic(Diagnostic diagnostic)
	{
		return (string)diagnostic.Arguments[0];
	}

	public override async Task RegisterCodeFixesAsync(CodeFixContext context)
	{
		Document document = context.Document;
		TextSpan span = context.Span;
		CancellationToken cancellationToken = context.CancellationToken;
		SyntaxNode syntaxNode = (await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false)).FindNode(span);
		if (syntaxNode.Kind == SyntaxKind.IdentifierName)
		{
			IdentifierNameSyntax identifierName = (IdentifierNameSyntax)syntaxNode;
			string qualifier = ExtractQualifierFromDiagnostic(context.Diagnostics[0]);
			context.RegisterFixes(ImmutableArray.Create((CodeAction)CreateThisInstanceAction(identifierName, syntaxNode.ToString(), qualifier, document), (CodeAction)CreateAllInstancesAction(identifierName, qualifier, document)), context.Diagnostics);
		}
	}

	public override FixAllProvider? GetFixAllProvider()
	{
		return QualifyImplicitWithFixAllProvider.Instance;
	}

	public ImmutableArray<int> FixableDiagnosticErrorCodes()
	{
		return fixableErrors.Cast<int>().ToImmutableArray();
	}

	private QualifyImplicitWithCodeAction CreateThisInstanceAction(IdentifierNameSyntax identifierName, string expression, string qualifier, Document document)
	{
		Document document2 = document;
		IdentifierNameSyntax identifierName2 = identifierName;
		string qualifier2 = qualifier;
		return new QualifyImplicitWithCodeAction(string.Format(CultureInfo.CurrentCulture, WorkspacesResources.QualifyWith_ThisInstance, qualifier2, expression), (CancellationToken c) => UpdateThisInstance(document2, identifierName2, qualifier2, c), qualifier2, generateFixAll: true);
	}

	private QualifyImplicitWithCodeAction CreateAllInstancesAction(IdentifierNameSyntax identifierName, string qualifier, Document document)
	{
		Document document2 = document;
		IdentifierNameSyntax identifierName2 = identifierName;
		string qualifier2 = qualifier;
		return new QualifyImplicitWithCodeAction(string.Format(CultureInfo.CurrentCulture, WorkspacesResources.QualifyWith_AllObjectInstances, qualifier2), (CancellationToken c) => UpdateAllInstances(document2, identifierName2, qualifier2, c), qualifier2 + "_all", generateFixAll: false);
	}

	private async Task<Document> UpdateThisInstance(Document document, IdentifierNameSyntax identifierName, string qualifier, CancellationToken cancellationToken)
	{
		SyntaxNode root = (await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false)).ReplaceNode(identifierName, SyntaxFactory.MemberAccessExpression(SyntaxFactory.IdentifierName(qualifier), identifierName.Identifier.Text).WithTriviaFrom(identifierName));
		return document.WithSyntaxRoot(root);
	}

	private async Task<Document> UpdateAllInstances(Document document, IdentifierNameSyntax identifierName, string qualifier, CancellationToken cancellationToken)
	{
		SyntaxNode root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		ObjectSyntax orgObjectSyntax = identifierName.GetAncestor<ObjectSyntax>();
		root = root.TrackNodes(orgObjectSyntax);
		IEnumerable<Diagnostic> diagnostics;
		if (orgObjectSyntax.Kind == SyntaxKind.CodeunitObject)
		{
			MethodOrTriggerDeclarationSyntax methodSyntax = identifierName.GetAncestor<MethodOrTriggerDeclarationSyntax>();
			diagnostics = from d in (await document.GetSemanticModelForNodeAsync(methodSyntax, cancellationToken)).GetMethodBodyDiagnostics(methodSyntax.Span)
				where fixableErrors.Contains((ErrorCode)d.Code)
				select d;
		}
		else
		{
			diagnostics = from d in (await document.GetSemanticModelForNodeAsync(orgObjectSyntax, cancellationToken)).GetDiagnostics(orgObjectSyntax.Span)
				where fixableErrors.Contains((ErrorCode)d.Code)
				select d;
		}
		SyntaxNode root3 = QualifyIdentifierNameSyntaxNodes(root, diagnostics, qualifier);
		ObjectSyntax currentNode = root3.GetCurrentNode(orgObjectSyntax);
		ObjectSyntax newNode = currentNode.WithLeadingTrivia(currentNode.GetLeadingTrivia().Concat(SyntaxFactory.ParseLeadingTrivia("#pragma implicitwith disable" + Environment.NewLine)));
		root3 = root3.ReplaceNode(currentNode, newNode);
		currentNode = root3.GetCurrentNode(orgObjectSyntax);
		SyntaxToken nextTokenOrEndOfFile = currentNode.GetLastToken().GetNextTokenOrEndOfFile(includeZeroWidth: true);
		root3 = root3.ReplaceToken(nextTokenOrEndOfFile, nextTokenOrEndOfFile.WithPrependedLeadingTrivia(SyntaxFactory.ParseLeadingTrivia(Environment.NewLine + "#pragma implicitwith restore" + Environment.NewLine)));
		return document.WithSyntaxRoot(root3);
	}

	private static SyntaxNode QualifyIdentifierNameSyntaxNodes(SyntaxNode root, IEnumerable<Diagnostic> diagnostics, string? qualifier)
	{
		PooledDictionary<SyntaxNode, SyntaxNode> nodeMap = PooledDictionary<SyntaxNode, SyntaxNode>.GetInstance();
		try
		{
			foreach (Diagnostic diagnostic in diagnostics)
			{
				SyntaxNode syntaxNode = root.FindNode(diagnostic.Location.SourceSpan);
				IdentifierNameSyntax identifierNameSyntax = null;
				if (qualifier == null)
				{
					qualifier = ExtractQualifierFromDiagnostic(diagnostic);
				}
				if (syntaxNode.Kind == SyntaxKind.IdentifierName)
				{
					identifierNameSyntax = (IdentifierNameSyntax)syntaxNode;
					nodeMap.Add(identifierNameSyntax, SyntaxFactory.MemberAccessExpression(SyntaxFactory.IdentifierName(qualifier), identifierNameSyntax.Identifier.Text).WithTriviaFrom(identifierNameSyntax));
				}
			}
			return root.ReplaceNodes(nodeMap.Keys, (SyntaxNode o, SyntaxNode n) => nodeMap[o]);
		}
		finally
		{
			if (nodeMap != null)
			{
				((IDisposable)nodeMap).Dispose();
			}
		}
	}
}
