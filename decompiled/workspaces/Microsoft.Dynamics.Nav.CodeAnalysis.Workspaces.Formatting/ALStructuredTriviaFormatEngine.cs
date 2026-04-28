using System.Threading;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Options;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Formatting;

internal class ALStructuredTriviaFormatEngine : AbstractFormatEngine
{
	private ALStructuredTriviaFormatEngine(SyntaxTrivia trivia, int initialColumn, OptionSet optionSet, ChainedFormattingRules formattingRules, SyntaxToken token1, SyntaxToken token2)
		: base(Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Formatting.TreeData.Create(trivia, initialColumn), optionSet, formattingRules, token1, token2, Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Formatting.TaskExecutor.Synchronous)
	{
	}

	public static IFormattingResult Format(SyntaxTrivia trivia, int initialColumn, OptionSet optionSet, ChainedFormattingRules formattingRules, CancellationToken cancellationToken)
	{
		SyntaxNode structure = trivia.GetStructure();
		return new ALStructuredTriviaFormatEngine(trivia, initialColumn, optionSet, formattingRules, structure.GetFirstToken(includeZeroWidth: true), structure.GetLastToken(includeZeroWidth: true)).FormatAsync(cancellationToken).WaitAndGetResult_CanCallOnBackground(cancellationToken);
	}

	protected override AbstractTriviaDataFactory CreateTriviaFactory()
	{
		return new TriviaDataFactory(TreeData, OptionSet);
	}

	protected override FormattingContext CreateFormattingContext(TokenStream tokenStream, CancellationToken cancellationToken)
	{
		return new FormattingContext(this, tokenStream, "AL");
	}

	protected override NodeOperations CreateNodeOperationTasks(CancellationToken cancellationToken)
	{
		return NodeOperations.Empty;
	}

	protected override AbstractFormattingResult CreateFormattingResult(TokenStream tokenStream)
	{
		return new FormattingResult(TreeData, tokenStream, SpanToFormat, TaskExecutor);
	}
}
