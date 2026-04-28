using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.CodeFixes.Helpers;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.CodeFixes.Providers;

internal class SuggestUsingStatementFixAllProvider : DocumentBasedFixAllByDiagnosticsProvider
{
	private static readonly ImmutableArray<FixAllScope> supportedFixAllScopes = new FixAllScope[3]
	{
		FixAllScope.Document,
		FixAllScope.Project,
		FixAllScope.Workspace
	}.ToImmutableArray();

	public static SuggestUsingStatementFixAllProvider Instance { get; } = new SuggestUsingStatementFixAllProvider();


	public SuggestUsingStatementFixAllProvider()
		: base(supportedFixAllScopes)
	{
	}

	public override string? GetOverrideFixAllTitle(FixAllScope scope)
	{
		return string.Format(CultureInfo.CurrentCulture, WorkspacesResources.SuggestUsingsForScope, scope.ToDisplayString().ToLower());
	}

	public override IEnumerable<string> GetSupportedFixAllDiagnosticIds(CodeFixProvider originalCodeFixProvider)
	{
		return base.GetSupportedFixAllDiagnosticIds(originalCodeFixProvider);
	}

	protected override async Task<Document?> FixAllAsync(FixAllContext fixAllContext, Document document, ImmutableArray<Diagnostic> diagnostics)
	{
		if (diagnostics.IsEmpty)
		{
			return document;
		}
		SyntaxNode syntaxRoot = await document.GetSyntaxRootAsync(fixAllContext.CancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		ImmutableArray<NameSyntax> immutableArray;
		using (PooledHashSet<SyntaxNode> nodesToFix = PooledHashSet<SyntaxNode>.GetInstance())
		{
			ImmutableArray<Diagnostic>.Enumerator enumerator = diagnostics.GetEnumerator();
			while (enumerator.MoveNext())
			{
				Diagnostic current = enumerator.Current;
				fixAllContext.CancellationToken.ThrowIfCancellationRequested();
				SyntaxNode item = syntaxRoot.FindNode(current.Location.SourceSpan);
				nodesToFix.Add(item);
			}
			immutableArray = await GetSuggestedNamespacesFromNodes(nodesToFix, document, fixAllContext.CancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		}
		if (immutableArray.IsEmpty)
		{
			return document;
		}
		CompilationUnitSyntax compilationUnitSyntax = (CompilationUnitSyntax)syntaxRoot;
		ImmutableArray<NameSyntax>.Enumerator enumerator2 = immutableArray.GetEnumerator();
		while (enumerator2.MoveNext())
		{
			UsingDirectiveSyntax usingDirective = NamespaceActionUtilities.CreateUsingStatement(enumerator2.Current);
			compilationUnitSyntax = NamespaceActionUtilities.AddUsing(compilationUnitSyntax, usingDirective);
		}
		return document.WithSyntaxRoot(compilationUnitSyntax);
	}

	private static async Task<ImmutableArray<NameSyntax>> GetSuggestedNamespacesFromNodes(PooledHashSet<SyntaxNode> nodes, Document document, CancellationToken cancellationToken)
	{
		if (nodes.Count == 0)
		{
			return ImmutableArray<NameSyntax>.Empty;
		}
		SemanticModel semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		Guid appId = semanticModel.Compilation.CompiledModule.AppId;
		PooledNameComparisonHashSet instance = PooledNameComparisonHashSet.GetInstance();
		ArrayBuilder<NameSyntax> instance2 = ArrayBuilder<NameSyntax>.GetInstance();
		try
		{
			foreach (SyntaxNode node in nodes)
			{
				cancellationToken.ThrowIfCancellationRequested();
				(SyntaxNode, SymbolKind)? identifierAndKind = NamespaceActionUtilities.GetIdentifierAndKind(node);
				if (!identifierAndKind.HasValue)
				{
					continue;
				}
				string text = identifierAndKind.Value.Item1?.GetIdentifierOrLiteralValue()?.UnquoteIdentifier();
				if (text == null)
				{
					continue;
				}
				ISymbol symbol = PickFirstCandidateFromSameModuleOrOther(NamespaceActionUtilities.GetCandidateSymbolsFromIdentifier(text, identifierAndKind.Value.Item2, semanticModel, cancellationToken), appId);
				if (symbol == null)
				{
					continue;
				}
				NameSyntax namespacePartOfQualifiedNameSyntax = symbol.GetNamespacePartOfQualifiedNameSyntax();
				if (namespacePartOfQualifiedNameSyntax != null)
				{
					string text2 = namespacePartOfQualifiedNameSyntax.ToFullString();
					if (!string.IsNullOrEmpty(text2) && instance.Add(text2))
					{
						instance2.Add(namespacePartOfQualifiedNameSyntax);
					}
				}
			}
			return instance2.ToImmutableArrayOrEmpty();
		}
		finally
		{
			instance.Free();
			instance2.Free();
		}
	}

	private static ISymbol? PickFirstCandidateFromSameModuleOrOther(IEnumerable<ISymbol> candidates, Guid documentModuleId)
	{
		ISymbol symbol = null;
		ISymbol symbol2 = null;
		foreach (ISymbol candidate in candidates)
		{
			IModuleSymbol? containingModule = candidate.ContainingModule;
			if (containingModule != null && containingModule.AppId == documentModuleId)
			{
				symbol2 = candidate;
				break;
			}
			if (symbol == null)
			{
				symbol = candidate;
			}
		}
		return symbol2 ?? symbol;
	}
}
