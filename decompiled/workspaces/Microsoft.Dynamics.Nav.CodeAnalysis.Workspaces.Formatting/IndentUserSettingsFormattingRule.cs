using System.Collections.Generic;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Formatting.Rules;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Options;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Formatting;

internal class IndentUserSettingsFormattingRule : BaseFormattingRule
{
	public override void AddIndentBlockOperations(List<IndentBlockOperation> list, SyntaxNode node, OptionSet optionSet, NextAction<IndentBlockOperation> nextOperation)
	{
		nextOperation.Invoke(list);
		(SyntaxToken, SyntaxToken) bracePair = node.GetBracePair();
		if (bracePair.IsValidScopeDelimiterPair() && optionSet.GetOption(ALFormattingOptions.IndentBraces))
		{
			AddIndentBlockOperation(list, bracePair.Item1, bracePair.Item1, bracePair.Item1.Span);
			AddIndentBlockOperation(list, bracePair.Item2, bracePair.Item2, bracePair.Item2.Span);
		}
	}
}
