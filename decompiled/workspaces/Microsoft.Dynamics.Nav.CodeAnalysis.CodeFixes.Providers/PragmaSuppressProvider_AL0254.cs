using System;
using Microsoft.Dynamics.Nav.CodeAnalysis.CodeActions.Mef;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.CodeFixes.Providers;

[CodeFixProvider("PragmaSuppressProvider_AL0254")]
public class PragmaSuppressProvider_AL0254 : PragmaSuppressProvider_Base
{
	protected override string SuppressableDiagnosticId { get; } = MessageProvider.Instance.GetIdForErrorCode(254);


	protected override SyntaxKind[] ExpectedNodeKinds { get; } = new SyntaxKind[3]
	{
		SyntaxKind.IdentifierName,
		SyntaxKind.SortingExpression,
		SyntaxKind.OrderByExpression
	};


	protected override Func<SyntaxNode, SyntaxNode?> GetSuppressableParent => (SyntaxNode x) => GetAncestorOfKind(x, SyntaxKind.Property);
}
