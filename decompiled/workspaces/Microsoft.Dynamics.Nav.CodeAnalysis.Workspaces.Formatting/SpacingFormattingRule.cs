using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Formatting.Rules;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Options;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Formatting;

internal class SpacingFormattingRule : BaseFormattingRule
{
	public override AdjustSpacesOperation GetAdjustSpacesOperation(SyntaxToken previousToken, SyntaxToken currentToken, OptionSet optionSet, NextOperation<AdjustSpacesOperation> nextOperation)
	{
		if (optionSet == null)
		{
			return nextOperation.Invoke();
		}
		SyntaxKind kind = previousToken.Kind;
		if (currentToken.IsOpenParenInParameterList() && kind == SyntaxKind.IdentifierToken)
		{
			return AdjustSpacesOperationZeroOrOne(optionSet, ALFormattingOptions.SpacingAfterMethodDeclarationName);
		}
		return nextOperation.Invoke();
	}

	private AdjustSpacesOperation AdjustSpacesOperationZeroOrOne(OptionSet optionSet, Option<bool> option, AdjustSpacesOption explicitOption = AdjustSpacesOption.ForceSpacesIfOnSingleLine)
	{
		if (optionSet.GetOption(option))
		{
			return CreateAdjustSpacesOperation(1, explicitOption);
		}
		return CreateAdjustSpacesOperation(0, explicitOption);
	}
}
