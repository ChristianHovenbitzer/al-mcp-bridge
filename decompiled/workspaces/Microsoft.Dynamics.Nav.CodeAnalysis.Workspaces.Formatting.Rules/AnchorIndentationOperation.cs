using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Formatting.Rules;

internal sealed class AnchorIndentationOperation
{
	public SyntaxToken AnchorToken { get; }

	public TextSpan TextSpan { get; }

	public SyntaxToken StartToken { get; }

	public SyntaxToken EndToken { get; }

	internal AnchorIndentationOperation(SyntaxToken anchorToken, SyntaxToken startToken, SyntaxToken endToken, TextSpan textSpan)
	{
		Contract.ThrowIfTrue(anchorToken.Kind == SyntaxKind.None);
		Contract.ThrowIfTrue(textSpan.Start < 0 || textSpan.Length < 0);
		Contract.ThrowIfTrue(startToken.Kind == SyntaxKind.None);
		Contract.ThrowIfTrue(endToken.Kind == SyntaxKind.None);
		AnchorToken = anchorToken;
		TextSpan = textSpan;
		StartToken = startToken;
		EndToken = endToken;
	}
}
