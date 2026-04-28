using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.CodeAnalysis.CodeActions;
using Microsoft.Dynamics.Nav.CodeAnalysis.CodeActions.Mef;
using Microsoft.Dynamics.Nav.CodeAnalysis.CodeFixes;
using Microsoft.Dynamics.Nav.CodeAnalysis.PreviewReference;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InterfaceImplementer;

[CodeFixProvider("InterfaceImplementer")]
public sealed class InterfaceImplementerCodeFixProvider : CodeFixProvider
{
	private class InterfaceImplementerCodeAction : CodeAction.DocumentChangeAction
	{
		public override CodeActionKind Kind => CodeActionKind.QuickFix;

		public InterfaceImplementerCodeAction(string title, Func<CancellationToken, Task<Document>> createChangedDocument, string equivalenceKey)
			: base(title, createChangedDocument, equivalenceKey)
		{
		}
	}

	private class MyCodeAction : CodeAction.CodeActionWithNestedActions
	{
		public MyCodeAction(string title, ImmutableArray<CodeAction> nestedActions)
			: base(title, nestedActions)
		{
		}
	}

	private static readonly int[] fixableErrors = new int[1] { 582 };

	public override ImmutableArray<string> FixableDiagnosticIds { get; } = fixableErrors.Select((int t) => MessageProvider.Instance.GetIdForErrorCode(t)).ToImmutableArray();


	public override async Task RegisterCodeFixesAsync(CodeFixContext context)
	{
		await Task.Run(delegate
		{
			InterfaceImplementerCodeAction action = CreateCodeAction(context);
			context.RegisterCodeFix(action, context.Diagnostics);
		});
	}

	private InterfaceImplementerCodeAction CreateCodeAction(CodeFixContext context)
	{
		return new InterfaceImplementerCodeAction(WorkspacesResources.ImplementInterface, (CancellationToken c) => AddMissingMembers(context), null);
	}

	private async Task<Document> AddMissingMembers(CodeFixContext context)
	{
		Document document = context.Document;
		CancellationToken cancellationToken = context.CancellationToken;
		SyntaxNode root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		SyntaxNode syntaxNode = root.FindNode(context.Span);
		if (!(syntaxNode.GetContainingApplicationObjectSyntax() is CodeunitSyntax cuSyntax) || cuSyntax.Interfaces.IsEmpty())
		{
			return document;
		}
		if (syntaxNode.Kind != SyntaxKind.ObjectNameReference)
		{
			return document;
		}
		SyntaxNode syntaxNode2 = ((ObjectNameReferenceSyntax)syntaxNode).Identifier;
		if (syntaxNode2.Kind == SyntaxKind.QualifiedName)
		{
			syntaxNode2 = ((QualifiedNameSyntax)syntaxNode2).Right;
		}
		InterfaceTypeSymbol intf = document.GetSymbolFromNode(syntaxNode2, context.CancellationToken).Result as InterfaceTypeSymbol;
		if (intf == null)
		{
			return document;
		}
		CodeunitTypeSymbol symbol = document.GetSymbolAtPositionAsync(cuSyntax.GetIdentifierNameSyntax().SpanStart, context.CancellationToken).Result as CodeunitTypeSymbol;
		if (symbol == null)
		{
			return document;
		}
		Binder enclosingBinder = (await document.GetSemanticModelForNodeAsync(cuSyntax, cancellationToken)).GetEnclosingBinder(cuSyntax.GetIdentifierNameSyntax().SpanStart);
		MemberSyntax[] missingMembers = GetMissingMembers(intf, symbol, enclosingBinder);
		CodeunitSyntax newNode = cuSyntax.AddMembers(missingMembers);
		SyntaxNode root2 = root.ReplaceNode(cuSyntax, newNode);
		return document.WithSyntaxRoot(root2);
	}

	private static MethodOrTriggerDeclarationSyntax[] GetMissingMembers(InterfaceTypeSymbol intf, CodeunitTypeSymbol symbol, Binder binder)
	{
		using PooledList<MethodSymbol> pooledList = PooledList<MethodSymbol>.GetInstance();
		ImmutableArray<MethodSymbol> interfaceMembers = intf.GetAllMethods();
		List<MethodSymbol> list = (from m in symbol.GetMembersOfKind<MethodSymbol>(SymbolKind.Method)
			where !m.IsLocal && !m.IsTrigger()
			select m).ToList();
		int i;
		for (i = 0; i < interfaceMembers.Length; i++)
		{
			if (!list.Contains((MethodSymbol h) => h.HasSameSignature(interfaceMembers[i])))
			{
				pooledList.Add(interfaceMembers[i]);
				list.Add(interfaceMembers[i]);
			}
		}
		return ReferenceSymbolToSyntax.ConvertProceduresAndTriggers(pooledList, skipAttributes: true, signaturesOnly: false, binder);
	}
}
