using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Formatting.Rules;

internal sealed class SuppressOperation
{
	public TextSpan TextSpan { get; }

	public SuppressOption Option { get; }

	public SyntaxToken StartToken { get; }

	public SyntaxToken EndToken { get; }

	internal SuppressOperation(SyntaxToken startToken, SyntaxToken endToken, TextSpan textSpan, SuppressOption option)
	{
		Contract.ThrowIfTrue(textSpan.Start < 0 || textSpan.Length < 0);
		Contract.ThrowIfTrue(startToken.Kind == SyntaxKind.None);
		Contract.ThrowIfTrue(endToken.Kind == SyntaxKind.None);
		TextSpan = textSpan;
		Option = option;
		StartToken = startToken;
		EndToken = endToken;
	}
}
