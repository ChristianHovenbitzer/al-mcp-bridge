using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Formatting.Rules;

internal sealed class IndentBlockOperation
{
	public SyntaxToken BaseToken { get; }

	public TextSpan TextSpan { get; }

	public IndentBlockOption Option { get; }

	public SyntaxToken StartToken { get; }

	public SyntaxToken EndToken { get; }

	public bool IsRelativeIndentation { get; }

	public int IndentationDeltaOrPosition { get; }

	internal IndentBlockOperation(SyntaxToken startToken, SyntaxToken endToken, TextSpan textSpan, int indentationDelta, IndentBlockOption option)
	{
		Contract.ThrowIfFalse(option.IsMaskOn(IndentBlockOption.PositionMask));
		Contract.ThrowIfTrue(textSpan.Start < 0 || textSpan.Length < 0);
		Contract.ThrowIfTrue(startToken.Kind == SyntaxKind.None);
		Contract.ThrowIfTrue(endToken.Kind == SyntaxKind.None);
		BaseToken = default(SyntaxToken);
		TextSpan = textSpan;
		Option = option;
		StartToken = startToken;
		EndToken = endToken;
		IsRelativeIndentation = false;
		IndentationDeltaOrPosition = indentationDelta;
	}

	internal IndentBlockOperation(SyntaxToken baseToken, SyntaxToken startToken, SyntaxToken endToken, TextSpan textSpan, int indentationDelta, IndentBlockOption option)
	{
		Contract.ThrowIfFalse(option.IsMaskOn(IndentBlockOption.PositionMask));
		Contract.ThrowIfFalse(option.IsMaskOn(IndentBlockOption.RelativePositionMask));
		Contract.ThrowIfFalse(baseToken.Span.End <= textSpan.Start);
		Contract.ThrowIfTrue(textSpan.Start < 0 || textSpan.Length < 0);
		Contract.ThrowIfTrue(startToken.Kind == SyntaxKind.None);
		Contract.ThrowIfTrue(endToken.Kind == SyntaxKind.None);
		BaseToken = baseToken;
		TextSpan = textSpan;
		Option = option;
		StartToken = startToken;
		EndToken = endToken;
		IsRelativeIndentation = true;
		IndentationDeltaOrPosition = indentationDelta;
	}
}
