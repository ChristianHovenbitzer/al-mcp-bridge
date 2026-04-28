using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Options;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Formatting;

internal abstract class TriviaData
{
	protected const int TokenPairIndexNotNeeded = int.MinValue;

	public int LineBreaks { get; protected set; }

	public int Spaces { get; protected set; }

	public bool SecondTokenIsFirstTokenOnLine => LineBreaks > 0;

	public abstract bool TreatAsElastic { get; }

	public abstract bool IsWhitespaceOnlyTrivia { get; }

	public abstract bool ContainsChanges { get; }

	protected OptionSet OptionSet { get; }

	protected string Language { get; }

	protected TriviaData(OptionSet optionSet, string language)
	{
		Contract.ThrowIfNull(optionSet);
		OptionSet = optionSet;
		Language = language;
	}

	public abstract IEnumerable<TextChange> GetTextChanges(TextSpan span);

	public abstract TriviaData WithSpace(int space, FormattingContext context, ChainedFormattingRules formattingRules);

	public abstract TriviaData WithLine(int line, int indentation, FormattingContext context, ChainedFormattingRules formattingRules, CancellationToken cancellationToken);

	public abstract TriviaData WithIndentation(int indentation, FormattingContext context, ChainedFormattingRules formattingRules, CancellationToken cancellationToken);

	public abstract void Format(FormattingContext context, ChainedFormattingRules formattingRules, Action<int, TriviaData> formattingResultApplier, CancellationToken cancellationToken, int tokenPairIndex = int.MinValue);
}
