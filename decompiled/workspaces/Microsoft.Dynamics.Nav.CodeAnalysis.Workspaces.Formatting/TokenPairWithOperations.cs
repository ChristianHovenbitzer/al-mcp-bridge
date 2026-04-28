using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Formatting.Rules;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Formatting;

internal struct TokenPairWithOperations
{
	public TokenStream TokenStream { get; }

	public AdjustSpacesOperation SpaceOperation { get; }

	public AdjustNewLinesOperation LineOperation { get; }

	public int PairIndex { get; }

	public SyntaxToken Token1 => TokenStream.GetToken(PairIndex);

	public SyntaxToken Token2 => TokenStream.GetToken(PairIndex + 1);

	public TokenPairWithOperations(TokenStream tokenStream, int tokenPairIndex, AdjustSpacesOperation spaceOperations, AdjustNewLinesOperation lineOperations)
	{
		this = default(TokenPairWithOperations);
		Contract.ThrowIfNull(tokenStream);
		Contract.ThrowIfFalse(0 <= tokenPairIndex && tokenPairIndex < tokenStream.TokenCount - 1);
		TokenStream = tokenStream;
		PairIndex = tokenPairIndex;
		SpaceOperation = spaceOperations;
		LineOperation = lineOperations;
	}
}
