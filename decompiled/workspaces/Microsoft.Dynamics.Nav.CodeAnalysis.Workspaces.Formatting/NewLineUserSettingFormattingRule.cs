using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Formatting.Rules;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Options;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Formatting;

internal class NewLineUserSettingFormattingRule : BaseFormattingRule
{
	private bool IsControlBlock(SyntaxNode node)
	{
		if (node.Kind == SyntaxKind.CaseStatement)
		{
			return true;
		}
		SyntaxKind valueOrDefault = (node?.Parent.Kind).GetValueOrDefault();
		if (valueOrDefault == SyntaxKind.IfStatement || valueOrDefault - 251 <= SyntaxKind.Int32LiteralToken)
		{
			return true;
		}
		return false;
	}

	public override AdjustSpacesOperation GetAdjustSpacesOperation(SyntaxToken previousToken, SyntaxToken currentToken, OptionSet optionSet, NextOperation<AdjustSpacesOperation> nextOperation)
	{
		AdjustSpacesOperation result = nextOperation.Invoke();
		if (currentToken.Kind == SyntaxKind.OpenBraceToken && currentToken.Parent is ObjectSyntax && !optionSet.GetOption(ALFormattingOptions.NewLinesForBracesInTypes))
		{
			result = CreateAdjustSpacesOperation(1, AdjustSpacesOption.ForceSpaces);
		}
		return result;
	}

	public override AdjustNewLinesOperation? GetAdjustNewLinesOperation(SyntaxToken previousToken, SyntaxToken currentToken, OptionSet optionSet, NextOperation<AdjustNewLinesOperation> nextOperation)
	{
		AdjustNewLinesOperation result = nextOperation.Invoke();
		if (currentToken.Kind == SyntaxKind.OpenBraceToken && currentToken.Parent is ObjectSyntax)
		{
			if (optionSet.GetOption(ALFormattingOptions.NewLinesForBracesInTypes))
			{
				return CreateAdjustNewLinesOperation(1, AdjustNewLinesOption.PreserveLines);
			}
			return null;
		}
		return result;
	}
}
