using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Formatting.Rules;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Options;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Formatting;

internal class TriviaFormatter
{
	private delegate LineColumnDelta Formatter<T>(LineColumn lineColumn, SyntaxTrivia trivia, List<T> changes, CancellationToken cancellationToken);

	private delegate void WhitespaceAppender<T>(LineColumn lineColumn, LineColumnDelta delta, TextSpan span, List<T> changes);

	private class DocumentationCommentExteriorCommentRewriter : SyntaxRewriter
	{
		private readonly bool forceIndentation;

		private readonly int indentation;

		private readonly int indentationDelta;

		private readonly OptionSet optionSet;

		public DocumentationCommentExteriorCommentRewriter(bool forceIndentation, int indentation, int indentationDelta, OptionSet optionSet, bool visitStructuredTrivia = true)
			: base(visitStructuredTrivia)
		{
			this.forceIndentation = forceIndentation;
			this.indentation = indentation;
			this.indentationDelta = indentationDelta;
			this.optionSet = optionSet;
		}

		public override SyntaxTrivia VisitTrivia(SyntaxTrivia trivia)
		{
			if (trivia.Kind == SyntaxKind.DocumentationCommentExteriorTrivia)
			{
				if (IsBeginningOrEndOfDocumentComment(trivia))
				{
					return base.VisitTrivia(trivia);
				}
				string text = trivia.ToFullString();
				string text2 = text.AdjustIndentForXmlDocExteriorTrivia(forceIndentation, indentation, indentationDelta, optionSet.GetOption(FormattingOptions.UseTabs, "AL"), optionSet.GetOption(FormattingOptions.TabSize, "AL"));
				if (text == text2)
				{
					return base.VisitTrivia(trivia);
				}
				return SyntaxFactory.DocumentationCommentExterior(text2);
			}
			return base.VisitTrivia(trivia);
		}

		private bool IsBeginningOrEndOfDocumentComment(SyntaxTrivia trivia)
		{
			for (SyntaxNode parent = trivia.Token.Parent; parent != null; parent = parent.Parent)
			{
				if (parent.Kind == SyntaxKind.SingleLineDocumentationCommentTrivia || parent.Kind == SyntaxKind.MultiLineDocumentationCommentTrivia)
				{
					if (trivia.Span.End == parent.SpanStart || trivia.Span.End == parent.Span.End)
					{
						return true;
					}
					return false;
				}
			}
			return false;
		}
	}

	private readonly bool firstLineBlank;

	private readonly int indentation;

	protected readonly FormattingContext Context;

	protected readonly ChainedFormattingRules FormattingRules;

	protected readonly LineColumn InitialLineColumn;

	protected readonly int LineBreaks;

	protected readonly string OriginalString;

	protected readonly int Spaces;

	protected readonly SyntaxToken Token1;

	protected readonly SyntaxToken Token2;

	private SyntaxTrivia newLine;

	private bool succeeded = true;

	private static readonly string[] s_spaceCache;

	protected int StartPosition
	{
		get
		{
			if (Token1.Kind == SyntaxKind.None)
			{
				return TreeInfo.StartPosition;
			}
			return Token1.Span.End;
		}
	}

	protected int EndPosition
	{
		get
		{
			if (Token2.Kind == SyntaxKind.None)
			{
				return TreeInfo.EndPosition;
			}
			return Token2.SpanStart;
		}
	}

	protected TreeData TreeInfo => Context.TreeData;

	protected OptionSet OptionSet => Context.OptionSet;

	protected string Language { get; } = "AL";


	protected TokenStream TokenStream => Context.TokenStream;

	public TriviaFormatter(FormattingContext context, ChainedFormattingRules formattingRules, SyntaxToken token1, SyntaxToken token2, string originalString, int lineBreaks, int spaces)
	{
		Contract.ThrowIfNull(context);
		Contract.ThrowIfNull(formattingRules);
		Contract.ThrowIfNull(originalString);
		Contract.ThrowIfFalse(lineBreaks >= 0);
		Contract.ThrowIfFalse(spaces >= 0);
		Contract.ThrowIfTrue(token1 == default(SyntaxToken) && token2 == default(SyntaxToken));
		Context = context;
		FormattingRules = formattingRules;
		OriginalString = originalString;
		Token1 = token1;
		Token2 = token2;
		LineBreaks = lineBreaks;
		Spaces = spaces;
		InitialLineColumn = GetInitialLineColumn();
		indentation = ((LineBreaks > 0) ? GetIndentation() : (-1));
		firstLineBlank = FirstLineBlank();
	}

	public List<SyntaxTrivia> FormatToSyntaxTrivia(CancellationToken cancellationToken)
	{
		List<SyntaxTrivia> list = ListPool<SyntaxTrivia>.Allocate();
		AddExtraLines(FormatTrivia(Format, AddWhitespaceTrivia, list, cancellationToken).Line, list);
		if (Succeeded())
		{
			List<SyntaxTrivia> result = new List<SyntaxTrivia>(list);
			ListPool<SyntaxTrivia>.Free(list);
			return result;
		}
		ListPool<SyntaxTrivia>.Free(list);
		return new List<SyntaxTrivia>(new TriviaList(Token1.TrailingTrivia, Token2.LeadingTrivia));
	}

	public List<TextChange> FormatToTextChanges(CancellationToken cancellationToken)
	{
		List<TextChange> list = ListPool<TextChange>.Allocate();
		AddExtraLines(FormatTrivia(Format, AddWhitespaceTextChange, list, cancellationToken).Line, list);
		if (Succeeded())
		{
			return ListPool<TextChange>.ReturnAndFree(list);
		}
		ListPool<TextChange>.Free(list);
		return new List<TextChange>();
	}

	protected bool Succeeded()
	{
		return succeeded;
	}

	protected bool IsWhitespace(SyntaxTrivia trivia)
	{
		return trivia.Kind == SyntaxKind.WhiteSpaceTrivia;
	}

	protected bool IsEndOfLine(SyntaxTrivia trivia)
	{
		return trivia.Kind == SyntaxKind.EndOfLineTrivia;
	}

	protected bool IsNullOrWhitespace(string text)
	{
		if (text == null)
		{
			return true;
		}
		for (int i = 0; i < text.Length; i++)
		{
			if (!IsWhitespace(text[i]) || !IsNewLine(text[i]))
			{
				return false;
			}
		}
		return true;
	}

	protected bool IsWhitespace(char ch)
	{
		return SyntaxFacts.IsWhitespace(ch);
	}

	protected bool IsNewLine(char ch)
	{
		return SyntaxFacts.IsNewLine(ch);
	}

	protected SyntaxTrivia CreateWhitespace(string text)
	{
		return SyntaxFactory.WhiteSpace(text);
	}

	protected SyntaxTrivia CreateEndOfLine()
	{
		if (newLine == default(SyntaxTrivia))
		{
			string option = Context.OptionSet.GetOption(FormattingOptions.NewLine, "AL");
			newLine = SyntaxFactory.EndOfLine(option);
		}
		return newLine;
	}

	protected LineColumnRule GetLineColumnRuleBetween(SyntaxTrivia trivia1, LineColumnDelta existingWhitespaceBetween, bool implicitLineBreak, SyntaxTrivia trivia2)
	{
		if (IsStartOrEndOfFile(trivia1, trivia2))
		{
			return LineColumnRule.PreserveLinesWithAbsoluteIndentation(0, 0);
		}
		if (trivia2.IsKind(SyntaxKind.None))
		{
			bool flag = FormattingRules.GetAdjustNewLinesOperation(Token1, Token2) != null;
			if (trivia1.Kind == SyntaxKind.CommentTrivia)
			{
				return LineColumnRule.PreserveLinesWithGivenIndentation(flag ? 1 : 0);
			}
			if (flag)
			{
				return LineColumnRule.PreserveLinesWithDefaultIndentation(0);
			}
			if (existingWhitespaceBetween.Lines > 0 && existingWhitespaceBetween.Spaces != Spaces)
			{
				return LineColumnRule.PreserveWithGivenSpaces(Spaces);
			}
			return LineColumnRule.Preserve();
		}
		if (SyntaxFacts.IsPreprocessorDirective(trivia2.Kind))
		{
			if (trivia2.IsKind(SyntaxKind.BadDirectiveTrivia) && existingWhitespaceBetween.Lines == 0 && !implicitLineBreak)
			{
				succeeded = false;
				return LineColumnRule.Preserve();
			}
			int lines = ((!trivia1.IsKind(SyntaxKind.None) || !Token1.IsKind(SyntaxKind.None)) ? 1 : 0);
			if (trivia2.IsKind(SyntaxKind.RegionDirectiveTrivia) || trivia2.IsKind(SyntaxKind.EndRegionDirectiveTrivia))
			{
				return LineColumnRule.PreserveLinesWithDefaultIndentation(lines);
			}
			return LineColumnRule.PreserveLinesWithAbsoluteIndentation(lines, 0);
		}
		if (trivia2.IsRegularOrDocComment())
		{
			if (!trivia1.IsRegularComment() || existingWhitespaceBetween.Lines > 1)
			{
				if (FormattingRules.GetAdjustNewLinesOperation(Token1, Token2) != null)
				{
					return LineColumnRule.PreserveLinesWithDefaultIndentation(0);
				}
				return LineColumnRule.PreserveLinesWithGivenIndentation(0);
			}
			if (existingWhitespaceBetween.Lines == 0)
			{
				return LineColumnRule.PreserveLinesWithGivenIndentation(0);
			}
			return LineColumnRule.PreserveLinesWithFollowingPrecedingIndentation();
		}
		if (trivia2.IsKind(SyntaxKind.SkippedTokensTrivia))
		{
			succeeded = false;
		}
		return LineColumnRule.Preserve();
	}

	protected LineColumnDelta Format(LineColumn lineColumn, SyntaxTrivia trivia, List<SyntaxTrivia> changes, CancellationToken cancellationToken)
	{
		if (trivia.HasStructure)
		{
			return FormatStructuredTrivia(lineColumn, trivia, changes, cancellationToken);
		}
		if (TryFormatMultiLineCommentTrivia(lineColumn, trivia, out var result))
		{
			changes.Add(result);
			return GetLineColumnDelta(lineColumn, result);
		}
		changes.Add(trivia);
		return GetLineColumnDelta(lineColumn, trivia);
	}

	private LineColumnDelta FormatStructuredTrivia(LineColumn lineColumn, SyntaxTrivia trivia, List<SyntaxTrivia> changes, CancellationToken cancellationToken)
	{
		if (trivia.Kind == SyntaxKind.SkippedTokensTrivia)
		{
			succeeded = false;
			changes.Add(trivia);
			return GetLineColumnDelta(lineColumn, trivia);
		}
		if (!trivia.IsDocComment())
		{
			SyntaxTrivia syntaxTrivia = SyntaxFactory.Trivia((StructuredTriviaSyntax)ALStructuredTriviaFormatEngine.Format(trivia, InitialLineColumn.Column, OptionSet, FormattingRules, cancellationToken).GetFormattedRoot(cancellationToken));
			changes.Add(syntaxTrivia);
			return GetLineColumnDelta(lineColumn, syntaxTrivia);
		}
		SyntaxTrivia syntaxTrivia2 = FormatDocumentComment(lineColumn, trivia);
		changes.Add(syntaxTrivia2);
		return GetLineColumnDelta(lineColumn, syntaxTrivia2);
	}

	private LineColumnDelta FormatStructuredTrivia(LineColumn lineColumn, SyntaxTrivia trivia, List<TextChange> changes, CancellationToken cancellationToken)
	{
		if (trivia.Kind == SyntaxKind.SkippedTokensTrivia)
		{
			succeeded = false;
			return GetLineColumnDelta(lineColumn, trivia);
		}
		if (!trivia.IsDocComment())
		{
			IFormattingResult formattingResult = ALStructuredTriviaFormatEngine.Format(trivia, InitialLineColumn.Column, OptionSet, FormattingRules, cancellationToken);
			if (formattingResult.GetTextChanges(cancellationToken).Count == 0)
			{
				return GetLineColumnDelta(lineColumn, trivia);
			}
			changes.AddRange(formattingResult.GetTextChanges(cancellationToken));
			SyntaxTrivia trivia2 = SyntaxFactory.Trivia((StructuredTriviaSyntax)formattingResult.GetFormattedRoot(cancellationToken));
			return GetLineColumnDelta(lineColumn, trivia2);
		}
		SyntaxTrivia syntaxTrivia = FormatDocumentComment(lineColumn, trivia);
		if (syntaxTrivia != trivia)
		{
			changes.Add(new TextChange(trivia.FullSpan, syntaxTrivia.ToFullString()));
		}
		return GetLineColumnDelta(lineColumn, syntaxTrivia);
	}

	private SyntaxTrivia FormatDocumentComment(LineColumn lineColumn, SyntaxTrivia trivia)
	{
		int column = lineColumn.Column;
		if (trivia.IsSingleLineDocComment())
		{
			if (!trivia.ToFullString().TrimEnd(null).ContainsLineBreak())
			{
				return trivia;
			}
			return new DocumentationCommentExteriorCommentRewriter(forceIndentation: true, column, 0, OptionSet).VisitTrivia(trivia);
		}
		int num = column - GetExistingIndentation(trivia);
		if (num == 0)
		{
			return trivia;
		}
		return new DocumentationCommentExteriorCommentRewriter(forceIndentation: false, column, num, OptionSet).VisitTrivia(trivia);
	}

	protected LineColumnDelta Format(LineColumn lineColumn, SyntaxTrivia trivia, List<TextChange> changes, CancellationToken cancellationToken)
	{
		if (trivia.HasStructure)
		{
			return FormatStructuredTrivia(lineColumn, trivia, changes, cancellationToken);
		}
		if (TryFormatMultiLineCommentTrivia(lineColumn, trivia, out var result))
		{
			changes.Add(new TextChange(trivia.FullSpan, result.ToFullString()));
			return GetLineColumnDelta(lineColumn, result);
		}
		return GetLineColumnDelta(lineColumn, trivia);
	}

	protected bool ContainsImplicitLineBreak(SyntaxTrivia trivia)
	{
		if (!trivia.HasStructure)
		{
			return false;
		}
		SyntaxNode structure = trivia.GetStructure();
		if (structure != null && structure.HasTrailingTrivia)
		{
			return structure.GetTrailingTrivia().IndexOf(SyntaxKind.EndOfLineTrivia) >= 0;
		}
		return false;
	}

	protected LineColumn GetLineColumn(LineColumn lineColumn, SyntaxTrivia trivia)
	{
		string text = trivia.ToFullString();
		return lineColumn.With(GetLineColumnDelta(lineColumn.Column, text));
	}

	protected LineColumnDelta GetLineColumnDelta(LineColumn lineColumn, SyntaxTrivia trivia)
	{
		string text = trivia.ToFullString();
		return GetLineColumnDelta(lineColumn.Column, text);
	}

	protected LineColumnDelta GetLineColumnDelta(int initialColumn, string text)
	{
		string lastLineText = text.GetLastLineText();
		if (text != lastLineText)
		{
			return new LineColumnDelta(text.GetNumberOfLineBreaks(), lastLineText.GetColumnFromLineOffset(lastLineText.Length, OptionSet.GetOption(FormattingOptions.TabSize, Language)), IsNullOrWhitespace(lastLineText));
		}
		return new LineColumnDelta(0, text.ConvertTabToSpace(OptionSet.GetOption(FormattingOptions.TabSize, Language), initialColumn, text.Length), IsNullOrWhitespace(lastLineText));
	}

	protected int GetExistingIndentation(SyntaxTrivia trivia)
	{
		int length = trivia.FullSpan.Start - StartPosition;
		string text = OriginalString.Substring(0, length);
		LineColumnDelta lineColumnDelta = GetLineColumnDelta(InitialLineColumn.Column, text);
		return InitialLineColumn.With(lineColumnDelta).Column;
	}

	private LineColumn FormatTrivia<T>(Formatter<T> formatter, WhitespaceAppender<T> whitespaceAdder, List<T> changes, CancellationToken cancellationToken)
	{
		LineColumn lineColumn = InitialLineColumn;
		LineColumnDelta lineColumnDelta = LineColumnDelta.Default;
		SyntaxTrivia trivia = default(SyntaxTrivia);
		SyntaxTrivia syntaxTrivia = default(SyntaxTrivia);
		bool flag = false;
		TriviaList triviaList = new TriviaList(Token1.TrailingTrivia, Token2.LeadingTrivia);
		foreach (SyntaxTrivia item in triviaList)
		{
			if (item.Kind == SyntaxKind.None)
			{
				continue;
			}
			if (IsWhitespaceOrEndOfLine(item))
			{
				if (IsEndOfLine(item))
				{
					flag = false;
				}
				lineColumnDelta = lineColumnDelta.With(GetLineColumnOfWhitespace(lineColumn, syntaxTrivia, trivia, lineColumnDelta, item));
				trivia = item;
			}
			else
			{
				trivia = default(SyntaxTrivia);
				lineColumn = FormatFirstTriviaAndWhitespaceAfter(lineColumn, syntaxTrivia, lineColumnDelta, item, formatter, whitespaceAdder, changes, flag, cancellationToken);
				flag = flag || ContainsImplicitLineBreak(item);
				lineColumnDelta = LineColumnDelta.Default;
				syntaxTrivia = item;
			}
		}
		return FormatFirstTriviaAndWhitespaceAfter(lineColumn, syntaxTrivia, lineColumnDelta, default(SyntaxTrivia), formatter, whitespaceAdder, changes, flag, cancellationToken);
	}

	private LineColumn FormatFirstTriviaAndWhitespaceAfter<T>(LineColumn lineColumnBeforeTrivia1, SyntaxTrivia trivia1, LineColumnDelta existingWhitespaceBetween, SyntaxTrivia trivia2, Formatter<T> format, WhitespaceAppender<T> addWhitespaceTrivia, List<T> changes, bool implicitLineBreak, CancellationToken cancellationToken)
	{
		LineColumn lineColumn = ((trivia1.Kind == SyntaxKind.None) ? lineColumnBeforeTrivia1 : lineColumnBeforeTrivia1.With(format(lineColumnBeforeTrivia1, trivia1, changes, cancellationToken)));
		LineColumnRule overallLineColumnRuleBetween = GetOverallLineColumnRuleBetween(trivia1, existingWhitespaceBetween, implicitLineBreak, trivia2);
		LineColumnDelta delta = Apply(lineColumnBeforeTrivia1, trivia1, lineColumn, existingWhitespaceBetween, trivia2, overallLineColumnRuleBetween);
		TextSpan textSpan = GetTextSpan(trivia1, trivia2);
		addWhitespaceTrivia(lineColumn, delta, textSpan, changes);
		return lineColumn.With(delta);
	}

	private LineColumnRule GetOverallLineColumnRuleBetween(SyntaxTrivia trivia1, LineColumnDelta existingWhitespaceBetween, bool implicitLineBreak, SyntaxTrivia trivia2)
	{
		LineColumnRule lineColumnRuleBetween = GetLineColumnRuleBetween(trivia1, existingWhitespaceBetween, implicitLineBreak, trivia2);
		GetTokensAtEdgeOfStructureTrivia(trivia1, trivia2, out var token, out var token2);
		if (token.Kind == SyntaxKind.None || token2.Kind == SyntaxKind.None)
		{
			return lineColumnRuleBetween;
		}
		AdjustNewLinesOperation adjustNewLinesOperation = FormattingRules.GetAdjustNewLinesOperation(token, token2);
		if (existingWhitespaceBetween.Lines != 0 && adjustNewLinesOperation == null)
		{
			return lineColumnRuleBetween;
		}
		if (adjustNewLinesOperation != null)
		{
			switch (adjustNewLinesOperation.Option)
			{
			case AdjustNewLinesOption.PreserveLines:
				if (existingWhitespaceBetween.Lines != 0)
				{
					int? lines2 = adjustNewLinesOperation.Line;
					LineColumnRule.LineOperations? lineOperation = LineColumnRule.LineOperations.Preserve;
					return lineColumnRuleBetween.With(lines2, null, null, lineOperation);
				}
				break;
			case AdjustNewLinesOption.ForceLines:
			{
				int? lines3 = adjustNewLinesOperation.Line;
				LineColumnRule.LineOperations? lineOperation = LineColumnRule.LineOperations.Force;
				return lineColumnRuleBetween.With(lines3, null, null, lineOperation);
			}
			case AdjustNewLinesOption.ForceLinesIfOnSingleLine:
				if (Context.TokenStream.TwoTokensOnSameLine(token, token2))
				{
					int? lines = adjustNewLinesOperation.Line;
					LineColumnRule.LineOperations? lineOperation = LineColumnRule.LineOperations.Force;
					return lineColumnRuleBetween.With(lines, null, null, lineOperation);
				}
				break;
			default:
				throw ExceptionUtilities.UnexpectedValue(adjustNewLinesOperation.Option);
			}
		}
		AdjustSpacesOperation adjustSpacesOperation = FormattingRules.GetAdjustSpacesOperation(token, token2);
		if (adjustSpacesOperation == null)
		{
			return lineColumnRuleBetween;
		}
		if (adjustSpacesOperation != null && adjustSpacesOperation.Option == AdjustSpacesOption.DefaultSpacesIfOnSingleLine && adjustSpacesOperation.Space == 1)
		{
			return lineColumnRuleBetween;
		}
		int? spaces = adjustSpacesOperation.Space;
		return lineColumnRuleBetween.With(null, spaces);
	}

	private void GetTokensAtEdgeOfStructureTrivia(SyntaxTrivia trivia1, SyntaxTrivia trivia2, out SyntaxToken token1, out SyntaxToken token2)
	{
		token1 = default(SyntaxToken);
		if (trivia1.Kind == SyntaxKind.None)
		{
			token1 = Token1;
		}
		else if (trivia1.HasStructure)
		{
			SyntaxToken lastToken = trivia1.GetStructure().GetLastToken(includeZeroWidth: true);
			if (ContainsOnlyWhitespace(lastToken.Span.End, lastToken.FullSpan.End))
			{
				token1 = lastToken;
			}
		}
		token2 = default(SyntaxToken);
		if (trivia2.Kind == SyntaxKind.None)
		{
			token2 = Token2;
		}
		else if (trivia2.HasStructure)
		{
			SyntaxToken firstToken = trivia2.GetStructure().GetFirstToken(includeZeroWidth: true);
			if (ContainsOnlyWhitespace(firstToken.FullSpan.Start, firstToken.SpanStart))
			{
				token2 = firstToken;
			}
		}
	}

	private bool ContainsOnlyWhitespace(int start, int end)
	{
		TextSpan textSpan = TextSpan.FromBounds(start, end);
		for (int i = textSpan.Start - Token1.Span.End; i < textSpan.Length; i++)
		{
			if (!char.IsWhiteSpace(OriginalString[i]))
			{
				return false;
			}
		}
		return true;
	}

	private bool FirstLineBlank()
	{
		if (Token1.TrailingTrivia.Count > 0 && Token1.TrailingTrivia[0].IsElastic())
		{
			return true;
		}
		int num = OriginalString.IndexOf(IsNewLine);
		if (num < 0)
		{
			return IsNullOrWhitespace(OriginalString);
		}
		for (int i = 0; i < num; i++)
		{
			if (!IsWhitespace(OriginalString[i]))
			{
				return false;
			}
		}
		return true;
	}

	private LineColumnDelta Apply(LineColumn lineColumnBeforeTrivia1, SyntaxTrivia trivia1, LineColumn lineColumnAfterTrivia1, LineColumnDelta existingWhitespaceBetween, SyntaxTrivia trivia2, LineColumnRule rule)
	{
		if ((Token1.IsMissing && trivia1.Kind == SyntaxKind.None) || (trivia2.Kind == SyntaxKind.None && Token2.IsMissing))
		{
			return existingWhitespaceBetween;
		}
		int ruleLines = GetRuleLines(rule, lineColumnAfterTrivia1, existingWhitespaceBetween);
		int ruleSpacesOrIndentation = GetRuleSpacesOrIndentation(lineColumnBeforeTrivia1, trivia1, lineColumnAfterTrivia1, existingWhitespaceBetween, trivia2, rule);
		return new LineColumnDelta(ruleLines, ruleSpacesOrIndentation, whitespaceOnly: true, existingWhitespaceBetween.ForceUpdate);
	}

	private int GetRuleSpacesOrIndentation(LineColumn lineColumnBeforeTrivia1, SyntaxTrivia trivia1, LineColumn lineColumnAfterTrivia1, LineColumnDelta existingWhitespaceBetween, SyntaxTrivia trivia2, LineColumnRule rule)
	{
		LineColumn lineColumn = lineColumnAfterTrivia1.With(existingWhitespaceBetween);
		if (rule.Lines > 0 || lineColumn.WhitespaceOnly)
		{
			switch (rule.IndentationOperation)
			{
			case LineColumnRule.IndentationOperations.Absolute:
				return Math.Max(0, rule.Indentation);
			case LineColumnRule.IndentationOperations.Default:
				return Context.GetBaseIndentation((trivia2.Kind == SyntaxKind.None) ? EndPosition : trivia2.SpanStart);
			case LineColumnRule.IndentationOperations.Given:
				if (trivia2.Kind != 0)
				{
					return Math.Max(0, indentation);
				}
				return Spaces;
			case LineColumnRule.IndentationOperations.Follow:
				return Math.Max(0, lineColumnBeforeTrivia1.Column);
			case LineColumnRule.IndentationOperations.Preserve:
				return existingWhitespaceBetween.Spaces;
			default:
				throw ExceptionUtilities.UnexpectedValue(rule.IndentationOperation);
			}
		}
		return rule.SpaceOperation switch
		{
			LineColumnRule.SpaceOperations.Preserve => Math.Max(rule.Spaces, existingWhitespaceBetween.Spaces), 
			LineColumnRule.SpaceOperations.Force => Math.Max(rule.Spaces, 0), 
			_ => throw ExceptionUtilities.UnexpectedValue(rule.SpaceOperation), 
		};
	}

	private int GetRuleLines(LineColumnRule rule, LineColumn lineColumnAfterTrivia1, LineColumnDelta existingWhitespaceBetween)
	{
		int num = Math.Max(0, rule.Lines - GetTrailingLinesAtEndOfTrivia1(lineColumnAfterTrivia1));
		if (rule.LineOperation != 0)
		{
			return num;
		}
		return Math.Max(num, existingWhitespaceBetween.Lines);
	}

	private int GetIndentation()
	{
		string lastLineText = OriginalString.GetLastLineText();
		int initialColumn = ((lastLineText == OriginalString) ? InitialLineColumn.Column : 0);
		int firstNonWhitespaceIndexInString = lastLineText.GetFirstNonWhitespaceIndexInString();
		if (firstNonWhitespaceIndexInString < 0)
		{
			return Spaces;
		}
		int num = lastLineText.ConvertTabToSpace(OptionSet.GetOption(FormattingOptions.TabSize, Language), initialColumn, firstNonWhitespaceIndexInString);
		int num2 = lastLineText.ConvertTabToSpace(OptionSet.GetOption(FormattingOptions.TabSize, Language), initialColumn, lastLineText.Length);
		return Spaces - (num2 - num);
	}

	private int GetTrailingLinesAtEndOfTrivia1(LineColumn lineColumnAfterTrivia1)
	{
		if (lineColumnAfterTrivia1.Column != 0 || lineColumnAfterTrivia1.Line <= 0)
		{
			return 0;
		}
		return 1;
	}

	private void AddExtraLines(int linesBetweenTokens, List<SyntaxTrivia> changes)
	{
		if (linesBetweenTokens < LineBreaks)
		{
			List<SyntaxTrivia> list = new List<SyntaxTrivia>();
			AddWhitespaceTrivia(LineColumn.Default, new LineColumnDelta(LineBreaks - linesBetweenTokens, 0), list);
			changes.InsertRange(GetInsertionIndex(changes), list);
		}
	}

	private int GetInsertionIndex(List<SyntaxTrivia> changes)
	{
		if (firstLineBlank || changes.Count == 0)
		{
			return 0;
		}
		for (int num = changes.Count - 1; num >= 0; num--)
		{
			if (IsEndOfLine(changes[num]))
			{
				return num + 1;
			}
		}
		for (int num2 = changes.Count - 1; num2 >= 0; num2--)
		{
			if (changes[num2].ToFullString().ContainsLineBreak())
			{
				return num2 + 1;
			}
		}
		return 0;
	}

	private void AddExtraLines(int linesBetweenTokens, List<TextChange> changes)
	{
		if (linesBetweenTokens < LineBreaks)
		{
			int index;
			if (changes.Count == 0)
			{
				AddWhitespaceTextChange(LineColumn.Default, new LineColumnDelta(LineBreaks - linesBetweenTokens, 0), GetInsertionSpan(changes), changes);
			}
			else if (TryGetMatchingChangeIndex(changes, out index))
			{
				LineColumnDelta lineColumnDelta = GetLineColumnDelta(0, changes[index].NewText);
				changes[index] = GetWhitespaceTextChange(LineColumn.Default, new LineColumnDelta(LineBreaks + lineColumnDelta.Lines - linesBetweenTokens, lineColumnDelta.Spaces), changes[index].Span);
			}
			else
			{
				TextChange whitespaceTextChange = GetWhitespaceTextChange(LineColumn.Default, new LineColumnDelta(LineBreaks - linesBetweenTokens, 0), GetInsertionSpan(changes));
				changes.Insert(0, whitespaceTextChange);
			}
		}
	}

	private bool TryGetMatchingChangeIndex(List<TextChange> changes, out int index)
	{
		index = -1;
		TextSpan insertionSpan = GetInsertionSpan(changes);
		for (int i = 0; i < changes.Count; i++)
		{
			TextChange textChange = changes[i];
			if (textChange.Span.Contains(insertionSpan) && IsNullOrWhitespace(textChange.NewText))
			{
				index = i;
				return true;
			}
		}
		return false;
	}

	private TextSpan GetInsertionSpan(List<TextChange> changes)
	{
		if (firstLineBlank || changes.Count == 0)
		{
			return new TextSpan(StartPosition, 0);
		}
		for (int num = OriginalString.Length - 1; num >= 0; num--)
		{
			if (OriginalString[num] == '\n')
			{
				return new TextSpan(Math.Min(StartPosition + num + 1, EndPosition), 0);
			}
		}
		return new TextSpan(EndPosition, 0);
	}

	private void AddWhitespaceTrivia(LineColumn lineColumn, LineColumnDelta delta, List<SyntaxTrivia> changes)
	{
		AddWhitespaceTrivia(lineColumn, delta, default(TextSpan), changes);
	}

	private void AddWhitespaceTrivia(LineColumn lineColumn, LineColumnDelta delta, TextSpan notUsed, List<SyntaxTrivia> changes)
	{
		if (delta.Lines == 0 && delta.Spaces == 0)
		{
			return;
		}
		for (int i = 0; i < delta.Lines; i++)
		{
			changes.Add(CreateEndOfLine());
		}
		if (delta.Spaces != 0)
		{
			bool option = OptionSet.GetOption(FormattingOptions.UseTabs, Language);
			int option2 = OptionSet.GetOption(FormattingOptions.TabSize, Language);
			if (delta.Lines > 0 || lineColumn.Column == 0)
			{
				changes.Add(CreateWhitespace(delta.Spaces.CreateIndentationString(option, option2)));
			}
			else
			{
				changes.Add(CreateWhitespace(GetSpaces(delta.Spaces)));
			}
		}
	}

	private string GetWhitespaceString(LineColumn lineColumn, LineColumnDelta delta)
	{
		StringBuilder stringBuilder = StringBuilderPool.Allocate();
		string option = OptionSet.GetOption(FormattingOptions.NewLine, Language);
		for (int i = 0; i < delta.Lines; i++)
		{
			stringBuilder.Append(option);
		}
		if (delta.Spaces == 0)
		{
			return StringBuilderPool.ReturnAndFree(stringBuilder);
		}
		bool option2 = OptionSet.GetOption(FormattingOptions.UseTabs, Language);
		int option3 = OptionSet.GetOption(FormattingOptions.TabSize, Language);
		if (delta.Lines > 0 || lineColumn.Column == 0)
		{
			stringBuilder.AppendIndentationString(delta.Spaces, option2, option3);
			return StringBuilderPool.ReturnAndFree(stringBuilder);
		}
		stringBuilder.Append(' ', delta.Spaces);
		return StringBuilderPool.ReturnAndFree(stringBuilder);
	}

	private TextChange GetWhitespaceTextChange(LineColumn lineColumn, LineColumnDelta delta, TextSpan span)
	{
		return new TextChange(span, GetWhitespaceString(lineColumn, delta));
	}

	private void AddWhitespaceTextChange(LineColumn lineColumn, LineColumnDelta delta, TextSpan span, List<TextChange> changes)
	{
		string whitespaceString = GetWhitespaceString(lineColumn, delta);
		changes.Add(new TextChange(span, whitespaceString));
	}

	private TextSpan GetTextSpan(SyntaxTrivia trivia1, SyntaxTrivia trivia2)
	{
		if (trivia1.Kind == SyntaxKind.None)
		{
			return TextSpan.FromBounds(StartPosition, trivia2.FullSpan.Start);
		}
		if (trivia2.Kind == SyntaxKind.None)
		{
			return TextSpan.FromBounds(trivia1.FullSpan.End, EndPosition);
		}
		return TextSpan.FromBounds(trivia1.FullSpan.End, trivia2.FullSpan.Start);
	}

	private bool IsWhitespaceOrEndOfLine(SyntaxTrivia trivia)
	{
		if (!IsWhitespace(trivia))
		{
			return IsEndOfLine(trivia);
		}
		return true;
	}

	private LineColumnDelta GetLineColumnOfWhitespace(LineColumn lineColumn, SyntaxTrivia previousTrivia, SyntaxTrivia trivia1, LineColumnDelta whitespaceBetween, SyntaxTrivia trivia2)
	{
		if (trivia2.IsElastic())
		{
			if (trivia1.IsElastic() || IsEndOfLine(trivia1))
			{
				return LineColumnDelta.Default;
			}
			LineColumn lineColumn2 = GetLineColumn(lineColumn, previousTrivia);
			if ((whitespaceBetween.Lines > 0 || (lineColumn2.Line > 0 && lineColumn2.Column == 0)) && whitespaceBetween.WhitespaceOnly)
			{
				return LineColumnDelta.Default;
			}
			return new LineColumnDelta(1, 0, whitespaceOnly: true, forceUpdate: true);
		}
		if (IsEndOfLine(trivia2))
		{
			return new LineColumnDelta(1, 0, whitespaceOnly: true, forceUpdate: false);
		}
		string text = trivia2.ToFullString();
		return new LineColumnDelta(0, text.ConvertTabToSpace(OptionSet.GetOption(FormattingOptions.TabSize, Language), lineColumn.With(whitespaceBetween).Column, text.Length), whitespaceOnly: true, forceUpdate: false);
	}

	private bool IsStartOrEndOfFile(SyntaxTrivia trivia1, SyntaxTrivia trivia2)
	{
		if (Token1.Kind == SyntaxKind.None || Token2.Kind == SyntaxKind.None)
		{
			if (trivia1.Kind != 0)
			{
				return trivia2.Kind == SyntaxKind.None;
			}
			return true;
		}
		return false;
	}

	private bool TryFormatMultiLineCommentTrivia(LineColumn lineColumn, SyntaxTrivia trivia, out SyntaxTrivia result)
	{
		result = default(SyntaxTrivia);
		if (trivia.Kind != SyntaxKind.CommentTrivia)
		{
			return false;
		}
		int column = lineColumn.Column;
		int num = column - GetExistingIndentation(trivia);
		if (num != 0)
		{
			result = SyntaxFactory.ParseLeadingTrivia(trivia.ToFullString().ReindentStartOfXmlDocumentationComment(forceIndentation: false, column, num, OptionSet.GetOption(FormattingOptions.UseTabs, "AL"), OptionSet.GetOption(FormattingOptions.TabSize, "AL"), OptionSet.GetOption(FormattingOptions.NewLine, "AL"))).ElementAt(0);
			return true;
		}
		return false;
	}

	private LineColumn GetInitialLineColumn()
	{
		string text = Token1.ToString();
		int num = ((Token1.Kind != 0) ? TokenStream.GetCurrentColumn(Token1) : 0);
		LineColumnDelta lineColumnDelta = GetLineColumnDelta(num, text);
		return new LineColumn(0, num + lineColumnDelta.Spaces, lineColumnDelta.WhitespaceOnly);
	}

	private static string GetSpaces(int space)
	{
		if (space >= 0 && space < 20)
		{
			return s_spaceCache[space];
		}
		return new string(' ', space);
	}

	static TriviaFormatter()
	{
		s_spaceCache = new string[20];
		for (int i = 0; i < 20; i++)
		{
			s_spaceCache[i] = new string(' ', i);
		}
	}
}
