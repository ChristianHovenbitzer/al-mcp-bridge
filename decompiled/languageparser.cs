using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

internal class LanguageParser : ParserBase
{
	protected delegate PostSkipAction SkipBadSeparatedListDelegate<T>(ref InternalSyntaxToken startToken, InternalSeparatedSyntaxListBuilder<T> list, SyntaxKind expected) where T : InternalSyntaxNode;

	internal class IndexerParselet : IInfixParselet
	{
		public int GetBindingPower()
		{
			return 10;
		}

		public InternalCodeExpressionSyntax Parse(LanguageParser parser, InternalCodeExpressionSyntax left, InternalSyntaxToken token)
		{
			InternalBracketedArgumentListSyntax argumentList = ParseBracketedArgumentList(parser);
			return InternalSyntaxFactory.ElementAccessExpression(left, argumentList);
		}

		private InternalBracketedArgumentListSyntax ParseBracketedArgumentList(LanguageParser parser)
		{
			InternalSyntaxToken startToken = parser.EatToken(SyntaxKind.OpenBracketToken);
			InternalSeparatedSyntaxList<InternalCodeExpressionSyntax> arguments = parser.ParseSeparatedList(ref startToken, SyntaxKind.IdentifierToken, (InternalSyntaxToken token) => true, SyntaxKind.CommaToken, SyntaxKind.CloseBracketToken, parser.SkipBadCommaSeparatedToken, parser.ParseExpression);
			InternalSyntaxToken internalSyntaxToken = parser.EatToken(SyntaxKind.CloseBracketToken);
			if (arguments.Count == 0)
			{
				internalSyntaxToken = parser.AddError(internalSyntaxToken, ErrorCode.ERR_IndexersMustHaveAtLeastOneValue);
			}
			return InternalSyntaxFactory.BracketedArgumentList(startToken, arguments, internalSyntaxToken);
		}
	}

	internal class BinaryOperatorParselet : IInfixParselet
	{
		private readonly int bindingPower;

		private readonly bool isRight;

		public BinaryOperatorParselet(BindingPower bindingPower, bool isRight = false)
		{
			this.bindingPower = (int)bindingPower;
			this.isRight = isRight;
		}

		public int GetBindingPower()
		{
			return bindingPower;
		}

		public InternalCodeExpressionSyntax Parse(LanguageParser parser, InternalCodeExpressionSyntax left, InternalSyntaxToken token)
		{
			token = parser.EatTokenOrKeyword();
			InternalCodeExpressionSyntax right = parser.ParseExpression(bindingPower - (isRight ? 1 : 0));
			return InternalSyntaxFactory.BinaryExpression(SyntaxFacts.GetBinaryExpression(token.Kind), left, token, right);
		}
	}

	internal class InListParselet : IInfixParselet
	{
		public int GetBindingPower()
		{
			return 6;
		}

		public InternalCodeExpressionSyntax Parse(LanguageParser parser, InternalCodeExpressionSyntax left, InternalSyntaxToken token)
		{
			InternalSyntaxToken inKeywordToken = parser.EatKeywordToken(SyntaxKind.InKeyword);
			InternalBracketedArgumentListSyntax listExpression = ParseBracketedArgumentList(parser);
			return InternalSyntaxFactory.InListExpression(left, inKeywordToken, listExpression);
		}

		internal InternalBracketedArgumentListSyntax ParseBracketedArgumentList(LanguageParser parser)
		{
			InternalSyntaxToken openBracketToken = parser.EatToken(SyntaxKind.OpenBracketToken);
			InternalSeparatedSyntaxListBuilder<InternalCodeExpressionSyntax> item = parser.Pool.AllocateSeparated<InternalCodeExpressionSyntax>();
			try
			{
				while (true)
				{
					InternalCodeExpressionSyntax internalCodeExpressionSyntax = parser.ParseExpression();
					if (parser.CurrentToken.Kind == SyntaxKind.DotDotToken)
					{
						internalCodeExpressionSyntax = InternalSyntaxFactory.BinaryExpression(SyntaxFacts.GetBinaryExpression(SyntaxKind.DotDotToken), internalCodeExpressionSyntax, parser.EatToken(SyntaxKind.DotDotToken), parser.ParseExpression());
					}
					item.Add(internalCodeExpressionSyntax);
					if (parser.CurrentToken.Kind != SyntaxKind.CommaToken)
					{
						break;
					}
					item.AddSeparator(parser.EatToken(SyntaxKind.CommaToken));
				}
				InternalSyntaxToken closeBracketToken = parser.EatToken(SyntaxKind.CloseBracketToken);
				return InternalSyntaxFactory.BracketedArgumentList(openBracketToken, item.ToList(), closeBracketToken);
			}
			finally
			{
				parser.Pool.Free(item);
			}
		}
	}

	internal class MemberParselet : IInfixParselet
	{
		public int GetBindingPower()
		{
			return 11;
		}

		public InternalCodeExpressionSyntax Parse(LanguageParser parser, InternalCodeExpressionSyntax left, InternalSyntaxToken token)
		{
			token = parser.EatToken(SyntaxKind.DotToken);
			InternalIdentifierNameSyntax name = parser.ParseIdentifierName();
			return InternalSyntaxFactory.MemberAccessExpression(left, token, name);
		}
	}

	internal class InvocationParselet : IInfixParselet
	{
		public int GetBindingPower()
		{
			return 11;
		}

		public InternalCodeExpressionSyntax Parse(LanguageParser parser, InternalCodeExpressionSyntax left, InternalSyntaxToken token)
		{
			InternalArgumentListSyntax argumentList = ParseParenthesizedArgumentList(parser);
			return InternalSyntaxFactory.InvocationExpression(left, argumentList);
		}

		private InternalArgumentListSyntax ParseParenthesizedArgumentList(LanguageParser parser)
		{
			InternalSyntaxToken startToken = parser.EatToken(SyntaxKind.OpenParenToken);
			InternalSeparatedSyntaxList<InternalCodeExpressionSyntax> arguments = parser.ParseSeparatedList(ref startToken, SyntaxKind.IdentifierToken, parser.IsArgumentListMember, SyntaxKind.CommaToken, SyntaxKind.CloseParenToken, parser.SkipBadCommaSeparatedToken, parser.ParseExpression);
			InternalSyntaxToken closeParenthesisToken = parser.EatToken(SyntaxKind.CloseParenToken);
			return InternalSyntaxFactory.ArgumentList(startToken, arguments, closeParenthesisToken);
		}
	}

	internal class OptionParselet : IInfixParselet
	{
		public int GetBindingPower()
		{
			return 11;
		}

		public InternalCodeExpressionSyntax Parse(LanguageParser parser, InternalCodeExpressionSyntax left, InternalSyntaxToken token)
		{
			return parser.ParseOptionAccess(left);
		}
	}

	internal class IsAsParselet : IInfixParselet
	{
		public int GetBindingPower()
		{
			return 6;
		}

		public InternalCodeExpressionSyntax Parse(LanguageParser parser, InternalCodeExpressionSyntax left, InternalSyntaxToken token)
		{
			token = parser.EatKeywordToken();
			InternalNameSyntax right = parser.ParseQualifiedName();
			return parser.CheckFeatureAvailability(InternalSyntaxFactory.BinaryExpression(SyntaxFacts.GetBinaryExpression(token.Kind), left, token, right), Feature.InterfaceCasting);
		}
	}

	internal class ConditionalOperatorParselet : IInfixParselet
	{
		public int GetBindingPower()
		{
			return 2;
		}

		public InternalCodeExpressionSyntax Parse(LanguageParser parser, InternalCodeExpressionSyntax left, InternalSyntaxToken token)
		{
			token = parser.EatToken(SyntaxKind.QuestionToken);
			InternalCodeExpressionSyntax whenTrue = parser.ParseExpression();
			InternalSyntaxToken colonToken = parser.EatToken(SyntaxKind.ColonToken);
			InternalCodeExpressionSyntax whenFalse = parser.ParseExpression();
			return parser.CheckFeatureAvailability(InternalSyntaxFactory.ConditionalExpression(left, token, whenTrue, colonToken, whenFalse), Feature.ConditionalExpression);
		}
	}

	public class NameParselet : IPrefixParselet
	{
		InternalCodeExpressionSyntax IPrefixParselet.Parse(LanguageParser parser, InternalSyntaxToken token)
		{
			return parser.ParseIdentifierName();
		}
	}

	public class ThisParselet : IPrefixParselet
	{
		InternalCodeExpressionSyntax IPrefixParselet.Parse(LanguageParser parser, InternalSyntaxToken token)
		{
			if (!parser.ThisSupport)
			{
				return parser.AddError(parser.ParseIdentifierName(), ErrorCode.WRN_IdentifierIsKeywordFromVersion, SyntaxKind.ThisKeyword.GetText(), RuntimeVersion.Fall2024);
			}
			return InternalSyntaxFactory.ThisExpression(parser.EatKeywordToken(SyntaxKind.ThisKeyword));
		}
	}

	public class LiteralParselet : IPrefixParselet
	{
		InternalCodeExpressionSyntax IPrefixParselet.Parse(LanguageParser parser, InternalSyntaxToken token)
		{
			return InternalSyntaxFactory.LiteralExpression(parser.ParseLiteralValue());
		}
	}

	internal class GroupingParselet : IPrefixParselet
	{
		public InternalCodeExpressionSyntax Parse(LanguageParser parser, InternalSyntaxToken token)
		{
			return InternalSyntaxFactory.ParenthesizedExpression(parser.EatToken(SyntaxKind.OpenParenToken), parser.ParseExpression(), parser.EatToken(SyntaxKind.CloseParenToken));
		}
	}

	internal class PrefixOperatorParselet : IPrefixParselet
	{
		private readonly int bindingPower;

		public PrefixOperatorParselet(int bindingPower)
		{
			this.bindingPower = bindingPower;
		}

		public InternalCodeExpressionSyntax Parse(LanguageParser parser, InternalSyntaxToken token)
		{
			SyntaxKind unaryExpression = SyntaxFacts.GetUnaryExpression(parser.CurrentToken.ContextualKind);
			token = parser.EatTokenOrKeyword();
			InternalCodeExpressionSyntax expression = parser.ParseExpression(bindingPower);
			SyntaxKind contextualKind = token.ContextualKind;
			if (contextualKind - 17 <= SyntaxKind.EmptyToken || contextualKind == SyntaxKind.NotKeyword)
			{
				return InternalSyntaxFactory.UnaryExpression(unaryExpression, token, expression);
			}
			throw ExceptionUtilities.UnexpectedValueWithMessage(token.ContextualKind, token.Text);
		}
	}

	internal class BadTokenParselet : IPrefixParselet
	{
		public InternalCodeExpressionSyntax Parse(LanguageParser parser, InternalSyntaxToken token)
		{
			return parser.AddTrailingSkippedSyntax(ParserBase.CreateMissingIdentifierName(), SkipBadTokens(parser));
		}

		private InternalSyntaxNode SkipBadTokens(LanguageParser parser)
		{
			InternalSyntaxListBuilder<InternalSyntaxToken> internalSyntaxListBuilder = parser.Pool.Allocate<InternalSyntaxToken>();
			try
			{
				while (!parser.IsEndOfFile && !parser.CurrentToken.ContextualKind.IsObjectKeyword())
				{
					InternalSyntaxToken node = parser.EatToken();
					if (internalSyntaxListBuilder.Count == 0)
					{
						node = parser.AddError(node, ErrorCode.ERR_ExpressionExpected);
					}
					internalSyntaxListBuilder.Add(node);
				}
				return internalSyntaxListBuilder.ToListNode();
			}
			finally
			{
				parser.Pool.Free(internalSyntaxListBuilder);
			}
		}
	}

	private static readonly IInfixParselet orBinaryOperatorParselet = new BinaryOperatorParselet(BindingPower.Or);

	private static readonly IInfixParselet andBinaryOperatorParselet = new BinaryOperatorParselet(BindingPower.And);

	private static readonly IInfixParselet equalityBinaryOperatorParselet = new BinaryOperatorParselet(BindingPower.Equality);

	private static readonly IInfixParselet comparisonBinaryOperatorParselet = new BinaryOperatorParselet(BindingPower.Equality);

	private static readonly IInfixParselet termBinaryOperatorParselet = new BinaryOperatorParselet(BindingPower.Term);

	private static readonly IInfixParselet factorBinaryOperatorParselet = new BinaryOperatorParselet(BindingPower.Factor);

	private static readonly IInfixParselet invocationParselet = new InvocationParselet();

	private static readonly IInfixParselet indexerParselet = new IndexerParselet();

	private static readonly IInfixParselet inListParselet = new InListParselet();

	private static readonly IInfixParselet memberParselet = new MemberParselet();

	private static readonly IInfixParselet optionParselet = new OptionParselet();

	private static readonly IInfixParselet isAsParselet = new IsAsParselet();

	private static readonly IInfixParselet conditionalOperatorParselet = new ConditionalOperatorParselet();

	private static readonly IPrefixParselet operatorPrefixParselet = new PrefixOperatorParselet(9);

	private static readonly IPrefixParselet nameParselet = new NameParselet();

	private static readonly IPrefixParselet literalParselet = new LiteralParselet();

	private static readonly IPrefixParselet groupingParselet = new GroupingParselet();

	private static readonly IPrefixParselet thisParselet = new ThisParselet();

	private static readonly IPrefixParselet badTokenParselet = new BadTokenParselet();

	public LanguageParser(Lexer lexer, LexerMode mode, SyntaxNode oldTree, IEnumerable<TextChangeRange> changes, CancellationToken cancellationToken = default(CancellationToken))
		: base(lexer, mode, oldTree, changes, allowModeReset: false, preLexIfNotIncremental: false, cancellationToken)
	{
	}

	protected InternalSeparatedSyntaxList<T> ParseSeparatedList<T>(ref InternalSyntaxToken startToken, SyntaxKind listRecoveryKind, Func<InternalSyntaxToken, bool> isListMember, SyntaxKind listSeparator, SyntaxKind closeTokenKind, SkipBadSeparatedListDelegate<T> skipBadTokens, Func<T> parseListMember, bool shouldParseMemberIfLastElementIsSeparator = false) where T : InternalSyntaxNode
	{
		InternalSeparatedSyntaxListBuilder<T> internalSeparatedSyntaxListBuilder = base.Pool.AllocateSeparated<T>();
		try
		{
			bool flag = true;
			while (base.CurrentToken.Kind != closeTokenKind && !IsTerminator())
			{
				if (flag)
				{
					if (isListMember(base.CurrentToken))
					{
						flag = false;
						internalSeparatedSyntaxListBuilder.Add(parseListMember());
					}
					else if (skipBadTokens(ref startToken, internalSeparatedSyntaxListBuilder, listRecoveryKind) == PostSkipAction.Abort)
					{
						break;
					}
				}
				else if (base.CurrentToken.Kind == listSeparator)
				{
					flag = true;
					internalSeparatedSyntaxListBuilder.AddSeparator(EatToken());
				}
				else if (skipBadTokens(ref startToken, internalSeparatedSyntaxListBuilder, listSeparator) == PostSkipAction.Abort)
				{
					break;
				}
			}
			if (flag && internalSeparatedSyntaxListBuilder.Count > 0)
			{
				if (shouldParseMemberIfLastElementIsSeparator)
				{
					internalSeparatedSyntaxListBuilder.Add(parseListMember());
				}
				else
				{
					InternalSyntaxNode node = internalSeparatedSyntaxListBuilder[internalSeparatedSyntaxListBuilder.Count - 1];
					internalSeparatedSyntaxListBuilder[internalSeparatedSyntaxListBuilder.Count - 1] = AddError(node, ErrorCode.ERR_ListCannotEndWithSeparator, listSeparator.GetText());
				}
			}
			return internalSeparatedSyntaxListBuilder.ToList();
		}
		finally
		{
			base.Pool.Free(internalSeparatedSyntaxListBuilder);
		}
	}

	protected InternalSeparatedSyntaxList<InternalIdentifierNameSyntax> ParseCommaSeparatedIdentifierNames(ref InternalSyntaxToken startToken, SyntaxKind closeTokenKind = SyntaxKind.SemicolonToken)
	{
		return ParseSeparatedList(ref startToken, SyntaxKind.IdentifierToken, (InternalSyntaxToken token) => token.IsTokenIdentifier(), SyntaxKind.CommaToken, closeTokenKind, SkipBadCommaSeparatedToken, ParseIdentifierName);
	}

	protected PostSkipAction SkipBadCommaSeparatedToken<T>(ref InternalSyntaxToken startToken, InternalSeparatedSyntaxListBuilder<T> list, SyntaxKind expected) where T : InternalSyntaxNode
	{
		return SkipBadSeparatedListTokensWithExpectedKind(ref startToken, list, (ParserBase p) => (expected == SyntaxKind.CommaToken && p.CurrentToken.Kind != SyntaxKind.CommaToken) || (expected.IsTokenIdentifier() && !p.CurrentToken.IsTokenIdentifier()), (ParserBase p) => p.CurrentToken.IsKind(SyntaxKind.SemicolonToken) || p.CurrentToken.IsKind(SyntaxKind.CloseParenToken) || p.CurrentToken.IsKeywordKind(SyntaxKind.BeginKeyword, SyntaxKind.VarKeyword, SyntaxKind.EndKeyword) || p.CurrentToken.IsKind(SyntaxKind.CloseBraceToken) || p.IsTerminator(), expected);
	}

	protected PostSkipAction SkipBadCommaSeparatedStringLiteralToken<T>(ref InternalSyntaxToken startToken, InternalSeparatedSyntaxListBuilder<T> list, SyntaxKind expected) where T : InternalSyntaxNode
	{
		return SkipBadSeparatedListTokensWithExpectedKind(ref startToken, list, (ParserBase p) => (expected == SyntaxKind.CommaToken && p.CurrentToken.Kind != SyntaxKind.CommaToken) || expected == SyntaxKind.StringLiteralToken, (ParserBase p) => p.CurrentToken.IsKind(SyntaxKind.SemicolonToken) || p.IsTerminator(), expected);
	}

	protected internal InternalIdentifierNameOrEmptySyntax ParseIdentifierNameOrEmptySyntax()
	{
		if (base.IsIncremental && base.CurrentNodeKind == SyntaxKind.IdentifierNameOrEmpty)
		{
			return (InternalIdentifierNameOrEmptySyntax)EatNode();
		}
		InternalSyntaxToken internalSyntaxToken = ParseIdentifierToken(allowMissingIdentifier: true);
		if (internalSyntaxToken == null)
		{
			InternalSyntaxToken empty = InternalSyntaxFactory.Token(SyntaxKind.EmptyToken);
			return InternalSyntaxFactory.IdentifierNameOrEmpty(null, empty);
		}
		return InternalSyntaxFactory.IdentifierNameOrEmpty(InternalSyntaxFactory.IdentifierName(internalSyntaxToken), null);
	}

	protected internal InternalIdentifierNameSyntax ParseIdentifierName()
	{
		if (base.IsIncremental && base.CurrentNodeKind == SyntaxKind.IdentifierName)
		{
			return (InternalIdentifierNameSyntax)EatNode();
		}
		return InternalSyntaxFactory.IdentifierName(ParseIdentifierToken());
	}

	internal InternalStatementSyntax ParseStatement()
	{
		if (base.IsIncremental && base.CurrentNode is StatementSyntax)
		{
			return (InternalStatementSyntax)EatNode();
		}
		return base.CurrentToken.ContextualKind switch
		{
			SyntaxKind.BeginKeyword => ParseBlock(), 
			SyntaxKind.IfKeyword => ParseIf(), 
			SyntaxKind.ElseKeyword => ParseOrphanedElse(), 
			SyntaxKind.CaseKeyword => ParseCase(), 
			SyntaxKind.WhileKeyword => ParseWhile(), 
			SyntaxKind.WithKeyword => ParseWith(), 
			SyntaxKind.ForKeyword => ParseFor(), 
			SyntaxKind.ForEachKeyword => ParseForeach(), 
			SyntaxKind.RepeatKeyword => ParseRepeat(), 
			SyntaxKind.ExitKeyword => ParseExit(), 
			SyntaxKind.SemicolonToken => InternalSyntaxFactory.EmptyStatement(EatToken(SyntaxKind.SemicolonToken)), 
			SyntaxKind.AssertErrorKeyword => ParseAssertError(), 
			SyntaxKind.BreakKeyword => ParseBreakStatement(), 
			_ => ParseAssign(), 
		};
	}

	private InternalStatementSyntax ParseBreakStatement()
	{
		return InternalSyntaxFactory.BreakStatement(EatKeywordToken(SyntaxKind.BreakKeyword), SemicolonOrNothing());
	}

	private InternalSyntaxList<InternalStatementSyntax> ParseStatementList(ref InternalSyntaxToken previousNode, SyntaxKind toKeywordToken)
	{
		InternalSyntaxListBuilder<InternalStatementSyntax> internalSyntaxListBuilder = base.Pool.Allocate<InternalStatementSyntax>();
		try
		{
			bool flag = true;
			while (!base.CurrentToken.IsKeywordKind(toKeywordToken) && base.CurrentToken.Kind != SyntaxKind.EndOfFileToken)
			{
				if (base.CurrentToken.IsPossibleStatement(base.ParenthesisAsStartOfStatement))
				{
					InternalStatementSyntax internalStatementSyntax = ParseStatement();
					if (!flag)
					{
						internalStatementSyntax = AddErrorToFirstToken(internalStatementSyntax, ErrorCode.ERR_SemicolonExpected);
					}
					flag = internalStatementSyntax.HasSemicolon;
					internalSyntaxListBuilder.Add(internalStatementSyntax);
					continue;
				}
				InternalSyntaxNode trailingTrivia;
				PostSkipAction num = SkipBadStatementListTokens(internalSyntaxListBuilder, SyntaxKind.EndKeyword, out trailingTrivia);
				if (trailingTrivia != null)
				{
					previousNode = AddTrailingSkippedSyntax(previousNode, trailingTrivia);
				}
				if (num == PostSkipAction.Abort)
				{
					break;
				}
			}
			return internalSyntaxListBuilder.ToList();
		}
		finally
		{
			base.Pool.Free(internalSyntaxListBuilder);
		}
	}

	private PostSkipAction SkipBadStatementListTokens(InternalSyntaxListBuilder<InternalStatementSyntax> statements, SyntaxKind expected, out InternalSyntaxNode trailingTrivia)
	{
		return SkipBadListTokensWithExpectedKindHelper(statements, (ParserBase p) => !p.CurrentToken.IsPossibleStatement(base.ParenthesisAsStartOfStatement), (ParserBase p) => p.CurrentToken.IsKeywordKind(SyntaxKind.EndKeyword, SyntaxKind.VarKeyword) || p.CurrentToken.Kind == SyntaxKind.CloseBraceToken || p.CurrentToken.Kind == SyntaxKind.OpenBracketToken || p.CurrentToken.IsPossibleMember() || p.IsTerminator(), expected, out trailingTrivia);
	}

	private InternalStatementSyntax ParseAssign()
	{
		InternalCodeExpressionSyntax internalCodeExpressionSyntax = ParseExpression();
		switch (base.CurrentToken.Kind)
		{
		case SyntaxKind.AssignToken:
		{
			InternalSyntaxToken assignmentToken2 = base.CurrentToken;
			EatToken();
			InternalCodeExpressionSyntax source2 = ParseValue();
			return InternalSyntaxFactory.AssignmentStatement(internalCodeExpressionSyntax, assignmentToken2, source2, SemicolonOrNothing());
		}
		case SyntaxKind.AssignRDivToken:
		case SyntaxKind.AssignPlusToken:
		case SyntaxKind.AssignMinusToken:
		case SyntaxKind.AssignMultiplyToken:
		{
			InternalSyntaxToken assignmentToken = base.CurrentToken;
			EatToken();
			InternalCodeExpressionSyntax source = ParseValue();
			return InternalSyntaxFactory.CompoundAssignmentStatement(internalCodeExpressionSyntax, assignmentToken, source, SemicolonOrNothing());
		}
		default:
			return InternalSyntaxFactory.ExpressionStatement(internalCodeExpressionSyntax, SemicolonOrNothing());
		}
	}

	private InternalIfStatementSyntax ParseIf()
	{
		InternalSyntaxToken ifKeywordToken = EatKeywordToken();
		InternalSyntaxToken internalSyntaxToken = null;
		InternalSyntaxToken elseKeywordToken = null;
		InternalCodeExpressionSyntax condition = ParseValue();
		internalSyntaxToken = EatKeywordToken(SyntaxKind.ThenKeyword);
		InternalStatementSyntax internalStatementSyntax = null;
		InternalStatementSyntax elseStatement = null;
		if (base.CurrentToken.Kind != SyntaxKind.EndOfFileToken)
		{
			internalStatementSyntax = ParseStatement();
			if (base.CurrentToken.IsKeywordKind(SyntaxKind.ElseKeyword) && !internalStatementSyntax.HasSemicolon)
			{
				elseKeywordToken = EatKeywordToken();
				elseStatement = ParseStatement();
			}
		}
		else
		{
			internalStatementSyntax = InternalSyntaxFactory.EmptyStatement(ParserBase.MissingToken(SyntaxKind.SemicolonToken));
		}
		return InternalSyntaxFactory.IfStatement(ifKeywordToken, condition, internalSyntaxToken, internalStatementSyntax, elseKeywordToken, elseStatement);
	}

	private InternalOrphanedElseStatementSyntax ParseOrphanedElse()
	{
		InternalSyntaxToken node = EatKeywordToken(SyntaxKind.ElseKeyword);
		node = AddError(node, ErrorCode.ERR_OrphanedElseStatement);
		InternalStatementSyntax elseStatement = ParseStatement();
		return InternalSyntaxFactory.OrphanedElseStatement(node, elseStatement);
	}

	private InternalCaseStatementSyntax ParseCase()
	{
		InternalSyntaxToken caseKeywordToken = EatKeywordToken();
		InternalCodeExpressionSyntax expression = ParseValue();
		InternalSyntaxToken ofKeywordToken = EatKeywordToken(SyntaxKind.OfKeyword);
		InternalSyntaxListBuilder<InternalCaseLineSyntax> internalSyntaxListBuilder = base.Pool.Allocate<InternalCaseLineSyntax>();
		try
		{
			bool flag = true;
			while (!IsEndOfCaseLines())
			{
				InternalSeparatedSyntaxListBuilder<InternalCodeExpressionSyntax> item = base.Pool.AllocateSeparated<InternalCodeExpressionSyntax>();
				try
				{
					while (true)
					{
						InternalCodeExpressionSyntax internalCodeExpressionSyntax = ParseValue();
						if (!flag)
						{
							internalCodeExpressionSyntax = AddErrorToFirstToken(internalCodeExpressionSyntax, ErrorCode.ERR_SemicolonExpected);
						}
						if (base.CurrentToken.Kind == SyntaxKind.DotDotToken)
						{
							InternalSyntaxToken internalSyntaxToken = base.CurrentToken;
							EatToken();
							InternalCodeExpressionSyntax right = ParseValue();
							internalCodeExpressionSyntax = InternalSyntaxFactory.BinaryExpression(SyntaxFacts.GetBinaryExpression(internalSyntaxToken.Kind), internalCodeExpressionSyntax, internalSyntaxToken, right);
						}
						item.Add(internalCodeExpressionSyntax);
						if (base.CurrentToken.Kind != SyntaxKind.CommaToken)
						{
							break;
						}
						item.AddSeparator(EatToken());
					}
					InternalSyntaxToken colonToken = EatToken(SyntaxKind.ColonToken);
					InternalStatementSyntax internalStatementSyntax = ParseStatement();
					flag = internalStatementSyntax.HasSemicolon;
					internalSyntaxListBuilder.Add(InternalSyntaxFactory.CaseLine(item.ToList(), colonToken, internalStatementSyntax));
				}
				finally
				{
					base.Pool.Free(item);
				}
			}
			InternalCaseElseSyntax caseElse = null;
			if (base.CurrentToken.IsKeywordKind(SyntaxKind.ElseKeyword))
			{
				InternalSyntaxToken previousNode = EatKeywordToken();
				InternalSyntaxList<InternalStatementSyntax> elseStatements = ParseStatementList(ref previousNode, SyntaxKind.EndKeyword);
				caseElse = InternalSyntaxFactory.CaseElse(previousNode, elseStatements);
			}
			InternalSyntaxToken endKeywordToken = EatKeywordToken(SyntaxKind.EndKeyword);
			return InternalSyntaxFactory.CaseStatement(caseKeywordToken, expression, ofKeywordToken, internalSyntaxListBuilder, caseElse, endKeywordToken, SemicolonOrNothing());
		}
		finally
		{
			base.Pool.Free(internalSyntaxListBuilder);
		}
	}

	private InternalRepeatStatementSyntax ParseRepeat()
	{
		InternalSyntaxToken previousNode = EatKeywordToken(SyntaxKind.RepeatKeyword);
		InternalSyntaxList<InternalStatementSyntax> statements = ParseStatementList(ref previousNode, SyntaxKind.UntilKeyword);
		return InternalSyntaxFactory.RepeatStatement(previousNode, statements, EatKeywordToken(SyntaxKind.UntilKeyword), ParseValue(), SemicolonOrNothing());
	}

	private InternalWhileStatementSyntax ParseWhile()
	{
		return InternalSyntaxFactory.WhileStatement(EatKeywordToken(), ParseValue(), EatKeywordToken(SyntaxKind.DoKeyword), ParseStatement());
	}

	private InternalForStatementSyntax ParseFor()
	{
		InternalSyntaxToken forKeywordToken = EatKeywordToken();
		InternalCodeExpressionSyntax loopVariable = ParseExpression();
		InternalSyntaxToken assignToken = EatToken(SyntaxKind.AssignToken);
		InternalCodeExpressionSyntax initialValue = ParseValue();
		InternalSyntaxToken operatorKeywordToken = (base.CurrentToken.IsKeywordKind(SyntaxKind.ToKeyword, SyntaxKind.DownToKeyword) ? EatKeywordToken() : EatToken(SyntaxKind.ToKeyword, ErrorCode.ERR_ForStatementToOrDownToExpected));
		InternalCodeExpressionSyntax endValue = ParseValue();
		InternalSyntaxToken doKeywordToken = EatKeywordToken(SyntaxKind.DoKeyword);
		InternalStatementSyntax statement = ParseStatement();
		return InternalSyntaxFactory.ForStatement(forKeywordToken, loopVariable, assignToken, initialValue, operatorKeywordToken, endValue, doKeywordToken, statement);
	}

	private InternalForEachStatementSyntax ParseForeach()
	{
		return InternalSyntaxFactory.ForEachStatement(EatKeywordToken(SyntaxKind.ForEachKeyword), ParseIdentifierName(), EatKeywordToken(SyntaxKind.InKeyword), ParseValue(), EatKeywordToken(SyntaxKind.DoKeyword), ParseStatement());
	}

	private InternalExitStatementSyntax ParseExit()
	{
		InternalSyntaxToken exitKeywordToken = EatKeywordToken(SyntaxKind.ExitKeyword);
		if (base.CurrentToken.Kind == SyntaxKind.OpenParenToken)
		{
			InternalSyntaxToken openParenthesisToken = EatToken();
			InternalCodeExpressionSyntax exitValue = ParseValue();
			InternalSyntaxToken closeParenthesisToken = EatToken(SyntaxKind.CloseParenToken);
			return InternalSyntaxFactory.ExitStatement(exitKeywordToken, openParenthesisToken, exitValue, closeParenthesisToken, SemicolonOrNothing());
		}
		return InternalSyntaxFactory.ExitStatement(exitKeywordToken, null, null, null, SemicolonOrNothing());
	}

	internal InternalBlockSyntax ParseBlock(bool semicolonRequired = false)
	{
		if (base.IsIncremental && base.CurrentNodeKind == SyntaxKind.Block)
		{
			return (InternalBlockSyntax)EatNode();
		}
		InternalSyntaxToken previousNode = EatKeywordToken(SyntaxKind.BeginKeyword);
		InternalSyntaxList<InternalStatementSyntax> statements = ParseStatementList(ref previousNode, SyntaxKind.EndKeyword);
		InternalSyntaxToken endKeywordToken = EatKeywordToken(SyntaxKind.EndKeyword);
		return InternalSyntaxFactory.Block(previousNode, statements, endKeywordToken, semicolonRequired ? EatToken(SyntaxKind.SemicolonToken) : SemicolonOrNothing());
	}

	private InternalWithStatementSyntax ParseWith()
	{
		InternalSyntaxToken withKeywordToken = EatKeywordToken();
		InternalCodeExpressionSyntax withId = ParseExpression();
		InternalSyntaxToken doKeywordToken = EatKeywordToken(SyntaxKind.DoKeyword);
		InternalStatementSyntax statement = ParseStatement();
		return InternalSyntaxFactory.WithStatement(withKeywordToken, withId, doKeywordToken, statement);
	}

	private InternalStatementSyntax ParseAssertError()
	{
		return InternalSyntaxFactory.AssertErrorStatement(EatKeywordToken(), ParseStatement());
	}

	private bool IsArgumentListMember(InternalSyntaxToken token)
	{
		return !token.IsKeywordKind(SyntaxKind.BeginKeyword, SyntaxKind.VarKeyword, SyntaxKind.EndKeyword);
	}

	private InternalCodeExpressionSyntax ParseValue()
	{
		return ParseExpression();
	}

	protected InternalOptionAccessExpressionSyntax ParseOptionAccess(InternalCodeExpressionSyntax lhsExpression)
	{
		InternalSyntaxToken colonColonToken = EatToken();
		InternalNameSyntax name = (base.CurrentToken.Kind.IsTokenIdentifier() ? ParseObjectTypeEnumMember() : AddError(ParserBase.CreateMissingIdentifierName(), ErrorCode.ERR_IdentifierExpected));
		return InternalSyntaxFactory.OptionAccessExpression(lhsExpression, colonColonToken, name);
	}

	protected InternalObjectNameReferenceSyntax ParseObjectNameReference()
	{
		return InternalSyntaxFactory.ObjectNameReference(ParseQualifiedName(!IsFeatureEnabled(Feature.Namespaces)));
	}

	internal InternalNameSyntax ParseQualifiedName(bool disallowQualified = false, bool mustBeValidClsIdentifier = false)
	{
		InternalNameSyntax internalNameSyntax = ParseIdentifierName();
		if (mustBeValidClsIdentifier && !((InternalIdentifierNameSyntax)internalNameSyntax).Identifier.ValueText.IsValidClsIdentifier())
		{
			internalNameSyntax = AddError(internalNameSyntax, ErrorCode.ERR_NameIsNotValidClsIdentifier, ((InternalIdentifierNameSyntax)internalNameSyntax).Identifier.ValueText);
		}
		bool flag = false;
		while (base.CurrentToken.Kind == SyntaxKind.DotToken)
		{
			InternalSyntaxToken separator = EatToken();
			internalNameSyntax = ParseQualifiedNameRight(internalNameSyntax, separator, mustBeValidClsIdentifier);
			flag = true;
		}
		if (disallowQualified && flag)
		{
			return CheckFeatureAvailability(internalNameSyntax, Feature.Namespaces);
		}
		return internalNameSyntax;
	}

	internal InternalNameSyntax ParseObjectTypeEnumMember(bool disallowQualified = false)
	{
		InternalNameSyntax internalNameSyntax = ParseIdentifierName();
		bool flag = false;
		while (base.CurrentToken.Kind == SyntaxKind.DotToken)
		{
			ResetPoint point = GetResetPoint();
			InternalSyntaxToken dotToken = EatToken();
			InternalIdentifierNameSyntax right = ParseIdentifierName();
			if (base.CurrentToken.Kind == SyntaxKind.OpenParenToken)
			{
				Reset(ref point);
				Release(ref point);
				return internalNameSyntax;
			}
			Release(ref point);
			internalNameSyntax = InternalSyntaxFactory.QualifiedName(internalNameSyntax, dotToken, right);
			flag = true;
		}
		if (disallowQualified && flag)
		{
			return CheckFeatureAvailability(internalNameSyntax, Feature.Namespaces);
		}
		return internalNameSyntax;
	}

	private InternalNameSyntax ParseQualifiedNameRight(InternalNameSyntax left, InternalSyntaxToken separator, bool mustBeValidClsIdentifier)
	{
		InternalIdentifierNameSyntax internalIdentifierNameSyntax = ParseIdentifierName();
		if (mustBeValidClsIdentifier && !internalIdentifierNameSyntax.Identifier.ValueText.IsValidClsIdentifier())
		{
			internalIdentifierNameSyntax = AddError(internalIdentifierNameSyntax, ErrorCode.ERR_NameIsNotValidClsIdentifier, internalIdentifierNameSyntax.Identifier.ValueText);
		}
		if (separator.Kind == SyntaxKind.DotToken)
		{
			return InternalSyntaxFactory.QualifiedName(left, separator, internalIdentifierNameSyntax);
		}
		return left;
	}

	private InternalSyntaxToken? SemicolonOrNothing()
	{
		if (base.CurrentToken.Kind != SyntaxKind.SemicolonToken)
		{
			return null;
		}
		return EatToken();
	}

	private bool IsEndOfCaseLines()
	{
		if (!base.CurrentToken.IsKeywordKind(SyntaxKind.ElseKeyword, SyntaxKind.EndKeyword))
		{
			return base.CurrentToken.Kind == SyntaxKind.EndOfFileToken;
		}
		return true;
	}

	internal override bool IsTokenIdentifier(SyntaxKind kind)
	{
		return kind.IsTokenIdentifier();
	}

	public InternalCodeExpressionSyntax ParseExpression()
	{
		return ParseExpression(0);
	}

	public InternalCodeExpressionSyntax ParseExpression(int bindingPower)
	{
		InternalSyntaxToken token = base.CurrentToken;
		IPrefixParselet prefixParselet = GetPrefixParselet(token);
		if (prefixParselet == null)
		{
			return AddTrailingSkippedSyntax(ParserBase.CreateMissingIdentifierName(), AddError(EatToken(), ErrorCode.ERR_ExpressionExpected));
		}
		InternalCodeExpressionSyntax internalCodeExpressionSyntax = prefixParselet.Parse(this, token);
		IInfixParselet infixParselet;
		while (bindingPower < ((infixParselet = GetInfixParselet(base.CurrentToken))?.GetBindingPower() ?? 0))
		{
			internalCodeExpressionSyntax = infixParselet.Parse(this, internalCodeExpressionSyntax, base.CurrentToken);
		}
		return internalCodeExpressionSyntax;
	}

	private IInfixParselet? GetInfixParselet(InternalSyntaxToken token)
	{
		switch (token.ContextualKind)
		{
		case SyntaxKind.OrKeyword:
		case SyntaxKind.XorKeyword:
			return orBinaryOperatorParselet;
		case SyntaxKind.AndKeyword:
			return andBinaryOperatorParselet;
		case SyntaxKind.PlusToken:
		case SyntaxKind.MinusToken:
			return termBinaryOperatorParselet;
		case SyntaxKind.RDivToken:
		case SyntaxKind.MultiplyToken:
		case SyntaxKind.IDivKeyword:
		case SyntaxKind.ModuloKeyword:
			return factorBinaryOperatorParselet;
		case SyntaxKind.InKeyword:
			return inListParselet;
		case SyntaxKind.AsKeyword:
		case SyntaxKind.IsKeyword:
			return isAsParselet;
		case SyntaxKind.OpenParenToken:
			return invocationParselet;
		case SyntaxKind.OpenBracketToken:
			return indexerParselet;
		case SyntaxKind.LessThanToken:
		case SyntaxKind.LessThanEqualsToken:
		case SyntaxKind.GreaterThanToken:
		case SyntaxKind.GreaterThanEqualsToken:
			return comparisonBinaryOperatorParselet;
		case SyntaxKind.NotEqualsToken:
		case SyntaxKind.EqualsToken:
			return equalityBinaryOperatorParselet;
		case SyntaxKind.DotToken:
			return memberParselet;
		case SyntaxKind.ColonColonToken:
			return optionParselet;
		case SyntaxKind.QuestionToken:
			return conditionalOperatorParselet;
		default:
			return null;
		}
	}

	protected InternalInt32SignedLiteralValueSyntax ParseInt32LiteralValue(InternalSyntaxToken signToken = null)
	{
		signToken = ParseMinusSignIfNotParsed(signToken);
		return InternalSyntaxFactory.Int32SignedLiteralValue(signToken, EatToken(SyntaxKind.Int32LiteralToken));
	}

	protected InternalInt64SignedLiteralValueSyntax ParseInt64LiteralValue(InternalSyntaxToken signToken = null)
	{
		signToken = ParseMinusSignIfNotParsed(signToken);
		return InternalSyntaxFactory.Int64SignedLiteralValue(signToken, EatToken(SyntaxKind.Int64LiteralToken));
	}

	protected InternalDecimalSignedLiteralValueSyntax ParseDecimalLiteralValue(InternalSyntaxToken signToken = null)
	{
		signToken = ParseMinusSignIfNotParsed(signToken);
		return InternalSyntaxFactory.DecimalSignedLiteralValue(signToken, EatToken(SyntaxKind.DecimalLiteralToken));
	}

	protected InternalStringLiteralValueSyntax ParseStringLiteralValue()
	{
		return InternalSyntaxFactory.StringLiteralValue(EatToken(SyntaxKind.StringLiteralToken));
	}

	protected InternalBooleanLiteralValueSyntax ParseBooleanLiteralValue()
	{
		SyntaxKind contextualKind = base.CurrentToken.ContextualKind;
		if (contextualKind - 14 <= SyntaxKind.EmptyToken)
		{
			return InternalSyntaxFactory.BooleanLiteralValue(EatKeywordToken());
		}
		return InternalSyntaxFactory.BooleanLiteralValue(EatToken(SyntaxKind.TrueKeyword, ErrorCode.ERR_ExpectedBooleanLiteral));
	}

	protected InternalDateLiteralValueSyntax ParseDateLiteralValue()
	{
		return InternalSyntaxFactory.DateLiteralValue(EatToken(SyntaxKind.DateLiteralToken));
	}

	protected InternalTimeLiteralValueSyntax ParseTimeLiteralValue()
	{
		return InternalSyntaxFactory.TimeLiteralValue(EatToken(SyntaxKind.TimeLiteralToken));
	}

	protected InternalDateTimeLiteralValueSyntax ParseDateTimeLiteralValue()
	{
		return InternalSyntaxFactory.DateTimeLiteralValue(EatToken(SyntaxKind.DateTimeLiteralToken));
	}

	protected InternalLiteralValueSyntax ParseNumericLiteralValue()
	{
		InternalSyntaxToken internalSyntaxToken = ((base.CurrentToken.Kind == SyntaxKind.MinusToken) ? EatToken() : null);
		switch (base.CurrentToken.Kind)
		{
		case SyntaxKind.Int32LiteralToken:
			return ParseInt32LiteralValue(internalSyntaxToken);
		case SyntaxKind.Int64LiteralToken:
			return ParseInt64LiteralValue(internalSyntaxToken);
		case SyntaxKind.DecimalLiteralToken:
			return ParseDecimalLiteralValue(internalSyntaxToken);
		default:
		{
			DebugAssertHelper.Assert(internalSyntaxToken != null, "This switch should only be reached if we have a minus sign and no number after it: " + base.CurrentToken.Kind);
			InternalSyntaxToken number = EatToken(SyntaxKind.Int32LiteralToken, ErrorCode.ERR_ExpectedNumericLiteral);
			return InternalSyntaxFactory.Int32SignedLiteralValue(internalSyntaxToken, number);
		}
		}
	}

	internal InternalLiteralValueSyntax ParseLiteralValue()
	{
		switch (base.CurrentToken.ContextualKind)
		{
		case SyntaxKind.Int32LiteralToken:
		case SyntaxKind.Int64LiteralToken:
		case SyntaxKind.DecimalLiteralToken:
		case SyntaxKind.MinusToken:
			return ParseNumericLiteralValue();
		case SyntaxKind.StringLiteralToken:
			return ParseStringLiteralValue();
		case SyntaxKind.TimeLiteralToken:
			return ParseTimeLiteralValue();
		case SyntaxKind.DateLiteralToken:
			return ParseDateLiteralValue();
		case SyntaxKind.DateTimeLiteralToken:
			return ParseDateTimeLiteralValue();
		case SyntaxKind.FalseKeyword:
		case SyntaxKind.TrueKeyword:
			return ParseBooleanLiteralValue();
		default:
			return InternalSyntaxFactory.StringLiteralValue(EatToken(SyntaxKind.StringLiteralToken, ErrorCode.ERR_ExpectedLiteral));
		}
	}

	private InternalSyntaxToken ParseMinusSignIfNotParsed(InternalSyntaxToken signToken)
	{
		if (signToken == null)
		{
			signToken = ((base.CurrentToken.Kind != SyntaxKind.MinusToken) ? null : EatToken());
		}
		return signToken;
	}

	private IPrefixParselet? GetPrefixParselet(InternalSyntaxToken token)
	{
		SyntaxKind contextualKind = token.ContextualKind;
		switch (contextualKind)
		{
		case SyntaxKind.PlusToken:
		case SyntaxKind.MinusToken:
		case SyntaxKind.NotKeyword:
			return operatorPrefixParselet;
		case SyntaxKind.IdentifierToken:
			return nameParselet;
		case SyntaxKind.OpenParenToken:
			return groupingParselet;
		default:
			if (!contextualKind.IsSignedOrSignInvariantLiteralToken())
			{
				switch (contextualKind)
				{
				case SyntaxKind.ThisKeyword:
					return thisParselet;
				case SyntaxKind.OpenBracketToken:
					return badTokenParselet;
				default:
					if (base.CurrentToken.IsKeywordAllowedIdentifier())
					{
						return nameParselet;
					}
					return null;
				}
			}
			return literalParselet;
		}
	}
}
