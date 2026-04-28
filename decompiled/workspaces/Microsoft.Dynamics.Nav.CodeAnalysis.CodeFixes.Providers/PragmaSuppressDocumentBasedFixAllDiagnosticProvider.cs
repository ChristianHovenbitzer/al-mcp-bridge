using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.CodeFixes.Providers;

internal class PragmaSuppressDocumentBasedFixAllDiagnosticProvider : DocumentBasedFixAllByDiagnosticsProvider
{
	private readonly string DiagnosticId;

	private readonly SyntaxKind[] ExpectedKinds;

	private readonly Func<SyntaxNode, SyntaxNode?> GetSuppressableParent;

	private static readonly ImmutableArray<FixAllScope> supportedFixAllScopes = new FixAllScope[3]
	{
		FixAllScope.Document,
		FixAllScope.Project,
		FixAllScope.Workspace
	}.ToImmutableArray();

	public PragmaSuppressDocumentBasedFixAllDiagnosticProvider(string diagnosticId, Func<SyntaxNode, SyntaxNode?> getSuppressableParent, params SyntaxKind[] expectedKinds)
		: base(supportedFixAllScopes)
	{
		DiagnosticId = diagnosticId;
		GetSuppressableParent = getSuppressableParent;
		ExpectedKinds = expectedKinds;
	}

	public override string? GetOverrideFixAllTitle(FixAllScope scope)
	{
		return string.Format(CultureInfo.CurrentCulture, WorkspacesResources.SuppressDiagnosticForScope, DiagnosticId, scope.ToDisplayString().ToLower());
	}

	protected override async Task<Document?> FixAllAsync(FixAllContext fixAllContext, Document document, ImmutableArray<Diagnostic> diagnostics)
	{
		if (diagnostics.IsEmpty)
		{
			return document;
		}
		SyntaxNode syntaxNode = await document.GetSyntaxRootAsync(fixAllContext.CancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		PooledHashSet<SyntaxToken> tokensWithPragmaSuppress = PooledHashSet<SyntaxToken>.GetInstance();
		try
		{
			using PooledHashSet<SyntaxToken> pooledHashSet = PooledHashSet<SyntaxToken>.GetInstance();
			using PooledHashSet<SyntaxNode> pooledHashSet2 = PooledHashSet<SyntaxNode>.GetInstance();
			ImmutableArray<Diagnostic>.Enumerator enumerator = diagnostics.GetEnumerator();
			while (enumerator.MoveNext())
			{
				Diagnostic current = enumerator.Current;
				fixAllContext.CancellationToken.ThrowIfCancellationRequested();
				if (current.Id != DiagnosticId)
				{
					continue;
				}
				SyntaxNode syntaxNode2 = syntaxNode.FindNode(current.Location.SourceSpan);
				if (ExpectedKinds.Contains(syntaxNode2.Kind))
				{
					SyntaxNode syntaxNode3 = GetSuppressableParent(syntaxNode2);
					if (syntaxNode3 != null)
					{
						AddFirstAndLastTokenToCollections(tokensWithPragmaSuppress, pooledHashSet, pooledHashSet2, syntaxNode3);
					}
				}
			}
			if (tokensWithPragmaSuppress.Count == 0)
			{
				return document;
			}
			(PragmaWarningDirectiveTriviaSyntax, PragmaWarningDirectiveTriviaSyntax) suppressionTrivias = PragmaSuppressUtilities.GetSuppressionTrivias(DiagnosticId);
			PragmaWarningDirectiveTriviaSyntax suppress = suppressionTrivias.Item1;
			PragmaWarningDirectiveTriviaSyntax restore = suppressionTrivias.Item2;
			SyntaxNode root = syntaxNode.ReplaceCore(tokens: tokensWithPragmaSuppress.Union(pooledHashSet), nodes: pooledHashSet2, computeReplacementNode: (SyntaxNode o, SyntaxNode r) => o.WithTrailingTrivia(PragmaSuppressUtilities.GetTrailingTriviaWithPragmaRestore(o, restore)), computeReplacementToken: (SyntaxToken o, SyntaxToken r) => tokensWithPragmaSuppress.Contains(o) ? o.WithLeadingTrivia(PragmaSuppressUtilities.GetLeadingTriviaWithPragmaSuppress(o, suppress)) : o.WithTrailingTrivia(PragmaSuppressUtilities.GetTrailingTriviaWithPragmaRestore(o, restore)));
			return document.WithSyntaxRoot(root);
		}
		finally
		{
			if (tokensWithPragmaSuppress != null)
			{
				((IDisposable)tokensWithPragmaSuppress).Dispose();
			}
		}
	}

	private static void AddFirstAndLastTokenToCollections(HashSet<SyntaxToken> tokensWithPragmaSuppress, HashSet<SyntaxToken> tokensWithPragmaRestore, HashSet<SyntaxNode> nodesWithPragmaRestore, SyntaxNode suppressableParent)
	{
		var (item, syntaxNodeOrToken) = PragmaSuppressUtilities.GetFirstAndLastTokenOrNode(suppressableParent);
		tokensWithPragmaSuppress.Add(item);
		if (syntaxNodeOrToken.IsToken)
		{
			tokensWithPragmaRestore.Add(syntaxNodeOrToken.AsToken());
		}
		else
		{
			nodesWithPragmaRestore.Add(syntaxNodeOrToken.AsNode());
		}
	}
}
