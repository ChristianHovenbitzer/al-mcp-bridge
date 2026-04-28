using System.Collections.Generic;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Formatting.Rules;

internal sealed class AlignTokensOperation
{
	public SyntaxToken BaseToken { get; }

	public IEnumerable<SyntaxToken> Tokens { get; }

	public AlignTokensOption Option { get; }

	internal AlignTokensOperation(SyntaxToken baseToken, IEnumerable<SyntaxToken> tokens, AlignTokensOption option)
	{
		Contract.ThrowIfNull(tokens);
		Option = option;
		BaseToken = baseToken;
		Tokens = tokens;
	}
}
