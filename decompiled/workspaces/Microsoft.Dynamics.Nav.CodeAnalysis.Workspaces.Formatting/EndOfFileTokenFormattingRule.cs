using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Formatting.Rules;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Options;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Formatting;

internal class EndOfFileTokenFormattingRule : BaseFormattingRule
{
	internal const string Name = "AL End Of File Token Formatting Rule";

	public override AdjustNewLinesOperation? GetAdjustNewLinesOperation(SyntaxToken previousToken, SyntaxToken currentToken, OptionSet optionSet, NextOperation<AdjustNewLinesOperation> nextOperation)
	{
		if (currentToken.Kind == SyntaxKind.EndOfFileToken)
		{
			return CreateAdjustNewLinesOperation(0, AdjustNewLinesOption.PreserveLines);
		}
		return nextOperation.Invoke();
	}

	public override AdjustSpacesOperation GetAdjustSpacesOperation(SyntaxToken previousToken, SyntaxToken currentToken, OptionSet optionSet, NextOperation<AdjustSpacesOperation> nextOperation)
	{
		if (currentToken.Kind == SyntaxKind.EndOfFileToken)
		{
			return CreateAdjustSpacesOperation(0, AdjustSpacesOption.ForceSpacesIfOnSingleLine);
		}
		return nextOperation.Invoke();
	}
}
