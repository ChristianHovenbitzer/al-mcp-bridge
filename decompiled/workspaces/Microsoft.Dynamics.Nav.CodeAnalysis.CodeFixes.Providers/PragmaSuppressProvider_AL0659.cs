using System;
using Microsoft.Dynamics.Nav.CodeAnalysis.CodeActions.Mef;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.CodeFixes.Providers;

[CodeFixProvider("PragmaSuppressProvider_AL0659")]
public class PragmaSuppressProvider_AL0659 : PragmaSuppressProvider_Base
{
	protected override string SuppressableDiagnosticId { get; } = MessageProvider.Instance.GetIdForErrorCode(659);


	protected override SyntaxKind[] ExpectedNodeKinds { get; } = new SyntaxKind[1] { SyntaxKind.IdentifierName };


	protected override Func<SyntaxNode, SyntaxNode?> GetSuppressableParent => (SyntaxNode x) => GetAncestorOfKind(x, SyntaxKind.EnumType);
}
