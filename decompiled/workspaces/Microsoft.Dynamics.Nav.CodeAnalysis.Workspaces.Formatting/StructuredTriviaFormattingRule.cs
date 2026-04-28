using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Formatting.Rules;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Options;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Formatting;

internal class StructuredTriviaFormattingRule : BaseFormattingRule
{
	internal const string Name = "AL Structured Trivia Formatting Rule";

	public override AdjustNewLinesOperation? GetAdjustNewLinesOperation(SyntaxToken previousToken, SyntaxToken currentToken, OptionSet optionSet, NextOperation<AdjustNewLinesOperation> nextOperation)
	{
		if (previousToken.Parent is StructuredTriviaSyntax || currentToken.Parent is StructuredTriviaSyntax)
		{
			return null;
		}
		return nextOperation.Invoke();
	}

	public override AdjustSpacesOperation GetAdjustSpacesOperation(SyntaxToken previousToken, SyntaxToken currentToken, OptionSet optionSet, NextOperation<AdjustSpacesOperation> nextOperation)
	{
		if (previousToken.Parent is StructuredTriviaSyntax || currentToken.Parent is StructuredTriviaSyntax)
		{
			return GetAdjustSpacesOperation(previousToken, currentToken, in nextOperation);
		}
		return nextOperation.Invoke();
	}

	private AdjustSpacesOperation GetAdjustSpacesOperation(SyntaxToken previousToken, SyntaxToken currentToken, in NextOperation<AdjustSpacesOperation> nextOperation)
	{
		if (previousToken.Kind == SyntaxKind.HashToken && SyntaxFacts.IsPreprocessorKeyword(currentToken.Kind))
		{
			return CreateAdjustSpacesOperation(0, AdjustSpacesOption.ForceSpacesIfOnSingleLine);
		}
		if (previousToken.Kind == SyntaxKind.RegionKeyword && currentToken.Kind == SyntaxKind.EndOfDirectiveToken)
		{
			return CreateAdjustSpacesOperation(0, AdjustSpacesOption.PreserveSpaces);
		}
		if (currentToken.Kind == SyntaxKind.EndOfDirectiveToken)
		{
			return CreateAdjustSpacesOperation(0, AdjustSpacesOption.ForceSpacesIfOnSingleLine);
		}
		return nextOperation.Invoke();
	}
}
