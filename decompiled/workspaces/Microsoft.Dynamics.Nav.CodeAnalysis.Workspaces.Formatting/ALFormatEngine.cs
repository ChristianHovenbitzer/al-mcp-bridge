using System.Collections.Generic;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Formatting.Rules;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Options;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Formatting;

internal class ALFormatEngine : AbstractFormatEngine
{
	public ALFormatEngine(SyntaxNode node, OptionSet optionSet, IEnumerable<IFormattingRule> formattingRules, SyntaxToken token1, SyntaxToken token2)
		: base(Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Formatting.TreeData.Create(node), optionSet, formattingRules, token1, token2, Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Formatting.TaskExecutor.Concurrent)
	{
	}

	protected override AbstractTriviaDataFactory CreateTriviaFactory()
	{
		return new TriviaDataFactory(TreeData, OptionSet);
	}

	protected override AbstractFormattingResult CreateFormattingResult(TokenStream tokenStream)
	{
		return new FormattingResult(TreeData, tokenStream, SpanToFormat, TaskExecutor);
	}
}
