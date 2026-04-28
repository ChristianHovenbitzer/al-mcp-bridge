using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

internal class Lexer : IDisposable
{
	internal struct TokenInfo
	{
		internal SyntaxKind Kind;

		internal SyntaxKind ContextualKind;

		internal bool HasIdentifierEscapeSequence;

		internal string Text;

		internal string StringValue;

		internal int Int32Value;

		internal long Int64Value;

		internal decimal DecimalValue;

		internal DateTime DateTimeValue;

		internal bool RequiresTextForXmlEntity;
	}

	private enum QuickScanState : byte
	{
		Initial,
		FollowingWhite,
		FollowingCR,
		Ident,
		Number,
		Punctuation,
		Dot,
		CompoundPunctStart,
		QuotedIdent,
		QuotedIdentEnd,
		DoneAfterNext,
		Done,
		Bad
	}

	private enum CharFlags : byte
	{
		White,
		CR,
		LF,
		Letter,
		Digit,
		Punct,
		Dot,
		CompoundPunctStart,
		DoubleQuote,
		Slash,
		Complex,
		EndOfFile
	}

	internal const int MaxCachedTokenSize = 42;

	private const int TriviaListInitialCapacity = 8;

	private LexerMode mode;

	private readonly bool allowPreprocessorDirectives;

	private List<SyntaxDiagnosticInfo> errors;

	private readonly StringBuilder builder = new StringBuilder(255);

	private readonly LexerCache cache;

	private readonly SlidingTextWindow textWindow;

	private DirectiveStack directives;

	private InternalSyntaxListBuilder leadingTriviaCache = new InternalSyntaxListBuilder(10);

	private InternalSyntaxListBuilder trailingTriviaCache = new InternalSyntaxListBuilder(10);

	private DocumentationCommentParser xmlParser;

	private char[] identBuffer;

	private int identLen;

	private readonly bool verbatimStringLiteralSupport;

	private static readonly byte[,] StateTransitions = new byte[11, 12]
	{
		{
			0, 0, 0, 3, 4, 5, 6, 7, 8, 12,
			12, 12
		},
		{
			1, 2, 10, 11, 11, 11, 11, 11, 11, 12,
			12, 11
		},
		{
			11, 11, 10, 11, 11, 11, 11, 11, 11, 11,
			11, 11
		},
		{
			1, 2, 10, 3, 3, 11, 11, 11, 12, 12,
			12, 11
		},
		{
			1, 2, 10, 12, 4, 11, 12, 11, 12, 12,
			12, 11
		},
		{
			1, 2, 10, 11, 11, 11, 11, 11, 11, 12,
			12, 11
		},
		{
			1, 2, 10, 11, 4, 11, 12, 11, 11, 12,
			12, 11
		},
		{
			1, 2, 10, 11, 11, 12, 11, 12, 12, 12,
			12, 11
		},
		{
			8, 12, 12, 8, 8, 8, 8, 8, 9, 8,
			8, 12
		},
		{
			1, 2, 10, 12, 12, 11, 11, 11, 12, 12,
			12, 11
		},
		{
			11, 11, 11, 11, 11, 11, 11, 11, 11, 11,
			11, 11
		}
	};

	private static readonly byte[] CharProperties = new byte[384]
	{
		10, 10, 10, 10, 10, 10, 10, 10, 10, 0,
		2, 0, 0, 1, 10, 10, 10, 10, 10, 10,
		10, 10, 10, 10, 10, 10, 10, 10, 10, 10,
		10, 10, 0, 7, 8, 10, 10, 7, 7, 10,
		5, 5, 7, 7, 5, 7, 6, 9, 4, 4,
		4, 4, 4, 4, 4, 4, 4, 4, 7, 5,
		7, 7, 7, 7, 10, 3, 3, 3, 3, 3,
		3, 3, 3, 3, 3, 3, 3, 3, 3, 3,
		3, 3, 3, 3, 3, 3, 3, 3, 3, 3,
		3, 5, 10, 5, 7, 3, 10, 3, 3, 3,
		3, 3, 3, 3, 3, 3, 3, 3, 3, 3,
		3, 3, 3, 3, 3, 3, 3, 3, 3, 3,
		3, 3, 3, 5, 7, 5, 7, 10, 10, 10,
		10, 10, 10, 10, 10, 10, 10, 10, 10, 10,
		10, 10, 10, 10, 10, 10, 10, 10, 10, 10,
		10, 10, 10, 10, 10, 10, 10, 10, 10, 10,
		10, 10, 10, 10, 10, 10, 10, 10, 10, 10,
		3, 10, 10, 10, 10, 10, 10, 10, 10, 10,
		10, 3, 10, 10, 10, 10, 3, 10, 10, 10,
		10, 10, 3, 3, 3, 3, 3, 3, 3, 3,
		3, 3, 3, 3, 3, 3, 3, 3, 3, 3,
		3, 3, 3, 3, 3, 10, 3, 3, 3, 3,
		3, 3, 3, 3, 3, 3, 3, 3, 3, 3,
		3, 3, 3, 3, 3, 3, 3, 3, 3, 3,
		3, 3, 3, 3, 3, 3, 3, 10, 3, 3,
		3, 3, 3, 3, 3, 3, 3, 3, 3, 3,
		3, 3, 3, 3, 3, 3, 3, 3, 3, 3,
		3, 3, 3, 3, 3, 3, 3, 3, 3, 3,
		3, 3, 3, 3, 3, 3, 3, 3, 3, 3,
		3, 3, 3, 3, 3, 3, 3, 3, 3, 3,
		3, 3, 3, 3, 3, 3, 3, 3, 3, 3,
		3, 3, 3, 3, 3, 3, 3, 3, 3, 3,
		3, 3, 3, 3, 3, 3, 3, 3, 3, 3,
		3, 3, 3, 3, 3, 3, 3, 3, 3, 3,
		3, 3, 3, 3, 3, 3, 3, 3, 3, 3,
		3, 3, 3, 3, 3, 3, 3, 3, 3, 3,
		3, 3, 3, 3, 3, 3, 3, 3, 3, 3,
		3, 3, 3, 3, 3, 3, 3, 3, 3, 3,
		3, 3, 3, 3
	};

	public int LineNo { get; private set; }

	internal SlidingTextWindow TextWindow => textWindow;

	public DirectiveStack Directives => directives;

	public int Length => TextWindow.Width;

	public int LinePos { get; }

	public ParseOptions Options { get; }

	public bool SuppressDocumentationCommentParse => (int)Options.DocumentationMode < 1;

	private bool InXmlNameAttributeValue
	{
		get
		{
			LexerMode lexerMode = mode & LexerMode.MaskLexMode;
			if (lexerMode == LexerMode.XmlNameQuote || lexerMode == LexerMode.XmlNameDoubleQuote)
			{
				return true;
			}
			return false;
		}
	}

	protected bool HasErrors => errors != null;

	public Lexer(string sourceText)
		: this(SourceText.From(sourceText))
	{
	}

	public Lexer(SourceText sourceText, ParseOptions parseOptions = null, bool allowPreprocessorDirectives = true)
	{
		textWindow = new SlidingTextWindow(sourceText);
		LineNo = 0;
		LinePos = 0;
		mode = LexerMode.XmlDocCommentLocationStart;
		Options = parseOptions ?? ParseOptions.Default;
		cache = new LexerCache();
		identBuffer = new char[32];
		this.allowPreprocessorDirectives = allowPreprocessorDirectives;
		verbatimStringLiteralSupport = Options.IsFeatureEnabled(Feature.VerbatimStringLiterals);
	}

	public void Reset(int position, DirectiveStack directives)
	{
		TextWindow.Reset(position);
		this.directives = directives;
	}

	public InternalSyntaxToken Lex(ref LexerMode mode)
	{
		InternalSyntaxToken result = Lex(mode);
		mode = this.mode;
		return result;
	}

	public InternalSyntaxToken Lex(LexerMode lexerMode)
	{
		mode = lexerMode;
		switch (mode)
		{
		case LexerMode.Default:
		case LexerMode.Object:
		case LexerMode.Code:
		case LexerMode.Property:
		case LexerMode.Expression:
			return QuickScanSyntaxToken() ?? LexSyntaxToken();
		case LexerMode.Directive:
			return LexDirectiveToken();
		default:
			switch (ModeOf(mode))
			{
			case LexerMode.XmlDocComment:
				return LexXmlToken();
			case LexerMode.XmlElementTag:
				return LexXmlElementTagToken();
			case LexerMode.XmlAttributeTextQuote:
			case LexerMode.XmlAttributeTextDoubleQuote:
				return LexXmlAttributeTextToken();
			case LexerMode.XmlCDataSectionText:
				return LexXmlCDataSectionTextToken();
			case LexerMode.XmlCommentText:
				return LexXmlCommentTextToken();
			case LexerMode.XmlProcessingInstructionText:
				return LexXmlProcessingInstructionTextToken();
			case LexerMode.XmlNameQuote:
			case LexerMode.XmlNameDoubleQuote:
				return LexXmlCrefOrNameToken();
			case LexerMode.XmlCharacter:
				return LexXmlCharacter();
			default:
				throw ExceptionUtilities.UnexpectedValue(ModeOf(mode));
			}
		}
	}

	private static LexerMode ModeOf(LexerMode mode)
	{
		return mode & LexerMode.MaskLexMode;
	}

	private bool ModeIs(LexerMode mode)
	{
		return ModeOf(this.mode) == mode;
	}

	private static XmlDocCommentLocation LocationOf(LexerMode mode)
	{
		return (XmlDocCommentLocation)((int)(mode & LexerMode.MaskXmlDocCommentLocation) >> 20);
	}

	private bool LocationIs(XmlDocCommentLocation location)
	{
		return LocationOf(mode) == location;
	}

	private void MutateLocation(XmlDocCommentLocation location)
	{
		mode &= ~LexerMode.MaskXmlDocCommentLocation;
		mode |= (LexerMode)((int)location << 20);
	}

	private static XmlDocCommentStyle StyleOf(LexerMode mode)
	{
		return (XmlDocCommentStyle)((int)(mode & LexerMode.MaskXmlDocCommentStyle) >> 24);
	}

	private bool StyleIs(XmlDocCommentStyle style)
	{
		return StyleOf(mode) == style;
	}

	public void Dispose()
	{
		textWindow.Dispose();
		cache.Free();
		GC.SuppressFinalize(this);
	}

	private static int GetFullWidth(InternalSyntaxListBuilder builder)
	{
		int num = 0;
		if (builder != null)
		{
			for (int i = 0; i < builder.Count; i++)
			{
				num += builder[i].FullWidth;
			}
		}
		return num;
	}

	private InternalSyntaxToken LexSyntaxToken()
	{
		leadingTriviaCache.Clear();
		LexSyntaxTrivia(TextWindow.Position > 0, ref leadingTriviaCache, isTrailing: false);
		TokenInfo info = default(TokenInfo);
		ScanSyntaxToken(ref info);
		SyntaxDiagnosticInfo[] array = GetErrors(GetFullWidth(leadingTriviaCache));
		trailingTriviaCache.Clear();
		LexSyntaxTrivia(afterFirstToken: true, ref trailingTriviaCache, isTrailing: true);
		return Create(ref info, leadingTriviaCache, trailingTriviaCache, array);
	}

	private void LexDirectiveAndExcludedTrivia(bool afterFirstToken, bool afterNonWhitespaceOnLine, ref InternalSyntaxListBuilder triviaList)
	{
		if (LexSingleDirective(isActive: true, endIsActive: true, afterFirstToken, afterNonWhitespaceOnLine, ref triviaList) is InternalBranchingDirectiveTriviaSyntax { BranchTaken: false })
		{
			LexExcludedDirectivesAndTrivia(endIsActive: true, ref triviaList);
		}
	}

	private void LexExcludedDirectivesAndTrivia(bool endIsActive, ref InternalSyntaxListBuilder triviaList)
	{
		while (true)
		{
			bool followedByDirective;
			InternalSyntaxNode internalSyntaxNode = LexDisabledText(out followedByDirective);
			if (internalSyntaxNode != null)
			{
				AddTrivia(internalSyntaxNode, ref triviaList);
			}
			if (followedByDirective)
			{
				InternalSyntaxNode internalSyntaxNode2 = LexSingleDirective(isActive: false, endIsActive, afterFirstToken: false, afterNonWhitespaceOnLine: false, ref triviaList);
				InternalBranchingDirectiveTriviaSyntax internalBranchingDirectiveTriviaSyntax = internalSyntaxNode2 as InternalBranchingDirectiveTriviaSyntax;
				if (internalSyntaxNode2.Kind != SyntaxKind.EndIfDirectiveTrivia && (internalBranchingDirectiveTriviaSyntax == null || !internalBranchingDirectiveTriviaSyntax.BranchTaken))
				{
					if (internalSyntaxNode2.Kind == SyntaxKind.IfDirectiveTrivia)
					{
						LexExcludedDirectivesAndTrivia(endIsActive: false, ref triviaList);
					}
					continue;
				}
				break;
			}
			break;
		}
	}

	private InternalSyntaxNode LexSingleDirective(bool isActive, bool endIsActive, bool afterFirstToken, bool afterNonWhitespaceOnLine, ref InternalSyntaxListBuilder triviaList)
	{
		if (SyntaxFacts.IsWhitespace(TextWindow.PeekChar()))
		{
			Start();
			AddTrivia(ScanWhitespace(), ref triviaList);
		}
		LexerMode lexerMode = mode;
		InternalSyntaxNode internalSyntaxNode = new DirectiveParser(this, directives).ParseDirective(isActive, endIsActive, afterFirstToken, afterNonWhitespaceOnLine);
		AddTrivia(internalSyntaxNode, ref triviaList);
		directives = internalSyntaxNode.ApplyDirectives(directives);
		mode = lexerMode;
		return internalSyntaxNode;
	}

	private InternalSyntaxNode LexDisabledText(out bool followedByDirective)
	{
		Start();
		int position = TextWindow.Position;
		int num = 0;
		bool flag = true;
		while (true)
		{
			char c = TextWindow.PeekChar();
			if ((uint)c <= 13u)
			{
				if (c == '\n' || c == '\r')
				{
					goto IL_00cb;
				}
			}
			else
			{
				switch (c)
				{
				case '\uffff':
					if (TextWindow.IsReallyAtEnd())
					{
						followedByDirective = false;
						if (TextWindow.Width <= 0)
						{
							return null;
						}
						return InternalSyntaxFactory.DisabledText(TextWindow.GetText(intern: false));
					}
					break;
				case '#':
					if (!allowPreprocessorDirectives)
					{
						break;
					}
					followedByDirective = true;
					if (position >= TextWindow.Position || flag)
					{
						TextWindow.Reset(position);
						if (TextWindow.Width <= 0)
						{
							return null;
						}
						return InternalSyntaxFactory.DisabledText(TextWindow.GetText(intern: false));
					}
					break;
				}
			}
			if (!SyntaxFacts.IsNewLine(c))
			{
				flag = flag && SyntaxFacts.IsWhitespace(c);
				TextWindow.AdvanceChar();
				continue;
			}
			goto IL_00cb;
			IL_00cb:
			ScanEndOfLine();
			position = TextWindow.Position;
			flag = true;
			num++;
		}
	}

	private InternalSyntaxToken LexDirectiveToken()
	{
		Start();
		TokenInfo info = default(TokenInfo);
		ScanDirectiveToken(ref info);
		SyntaxDiagnosticInfo[] array = GetErrors(0);
		InternalSyntaxListBuilder trailingTrivia = LexDirectiveTrailingTrivia(info.Kind == SyntaxKind.EndOfDirectiveToken);
		return Create(ref info, null, trailingTrivia, array);
	}

	private bool ScanDirectiveToken(ref TokenInfo info)
	{
		bool flag = false;
		char c;
		char surrogateCharacter;
		switch (c = TextWindow.PeekChar())
		{
		case '\uffff':
			if (TextWindow.IsReallyAtEnd())
			{
				info.Kind = SyntaxKind.EndOfDirectiveToken;
				break;
			}
			goto default;
		case '\n':
		case '\r':
			info.Kind = SyntaxKind.EndOfDirectiveToken;
			break;
		case '#':
			TextWindow.AdvanceChar();
			info.Kind = SyntaxKind.HashToken;
			break;
		case '(':
			TextWindow.AdvanceChar();
			info.Kind = SyntaxKind.OpenParenToken;
			break;
		case ')':
			TextWindow.AdvanceChar();
			info.Kind = SyntaxKind.CloseParenToken;
			break;
		case ',':
			TextWindow.AdvanceChar();
			info.Kind = SyntaxKind.CommaToken;
			break;
		case '=':
			TextWindow.AdvanceChar();
			info.Kind = SyntaxKind.EqualsToken;
			break;
		case '<':
			if (TextWindow.PeekChar(1) == '>')
			{
				TextWindow.AdvanceChar(2);
				info.Kind = SyntaxKind.NotEqualsToken;
				break;
			}
			goto default;
		case '0':
		case '1':
		case '2':
		case '3':
		case '4':
		case '5':
		case '6':
		case '7':
		case '8':
		case '9':
			ScanInteger();
			info.Kind = SyntaxKind.Int32LiteralToken;
			info.Text = TextWindow.GetText(intern: true);
			if (!int.TryParse(info.Text, out info.Int32Value))
			{
				AddError(ErrorCode.ERR_InvalidInt32Literal, builder.ToString());
			}
			break;
		case '\'':
			ScanQuotedString(ref info, SyntaxKind.StringLiteralToken);
			break;
		case '\\':
			c = TextWindow.PeekCharOrUnicodeEscape(out surrogateCharacter);
			flag = true;
			if (SyntaxFacts.IsIdentifierStartCharacter(c))
			{
				ScanIdentifierOrKeyword(ref info);
				break;
			}
			goto default;
		default:
			if (flag || !SyntaxFacts.IsNewLine(c))
			{
				if (SyntaxFacts.IsIdentifierStartCharacter(c))
				{
					ScanIdentifierOrKeyword(ref info);
					break;
				}
				if (flag)
				{
					TextWindow.NextCharOrUnicodeEscape(out surrogateCharacter, out SyntaxDiagnosticInfo info2);
					AddError(info2);
				}
				else
				{
					TextWindow.AdvanceChar();
				}
				info.Kind = SyntaxKind.None;
				info.Text = TextWindow.GetText(intern: true);
				break;
			}
			goto case '\n';
		}
		DebugAssertHelper.Assert(info.Kind != 0 || info.Text != null);
		return info.Kind != SyntaxKind.None;
	}

	private bool ScanInteger()
	{
		int position = TextWindow.Position;
		char c;
		while ((c = TextWindow.PeekChar()) >= '0' && c <= '9')
		{
			TextWindow.AdvanceChar();
		}
		return position < TextWindow.Position;
	}

	private InternalSyntaxListBuilder LexDirectiveTrailingTrivia(bool includeEndOfLine)
	{
		InternalSyntaxListBuilder list = null;
		while (true)
		{
			int position = TextWindow.Position;
			InternalSyntaxNode internalSyntaxNode = LexDirectiveTrivia();
			if (internalSyntaxNode == null)
			{
				break;
			}
			if (internalSyntaxNode.Kind == SyntaxKind.EndOfLineTrivia)
			{
				if (includeEndOfLine)
				{
					AddTrivia(internalSyntaxNode, ref list);
				}
				else
				{
					TextWindow.Reset(position);
				}
				break;
			}
			AddTrivia(internalSyntaxNode, ref list);
		}
		return list;
	}

	private InternalSyntaxNode LexDirectiveTrivia()
	{
		InternalSyntaxNode result = null;
		Start();
		char c = TextWindow.PeekChar();
		switch (c)
		{
		case '/':
			if (TextWindow.PeekChar(1) == '/')
			{
				ScanToEndOfLine();
				result = InternalSyntaxFactory.Comment(TextWindow.GetText(intern: false));
			}
			break;
		case '\n':
		case '\r':
			result = ScanEndOfLine();
			break;
		case '\t':
		case '\v':
		case '\f':
		case ' ':
			result = ScanWhitespace();
			break;
		default:
			if (!SyntaxFacts.IsWhitespace(c))
			{
				if (!SyntaxFacts.IsNewLine(c))
				{
					break;
				}
				goto case '\n';
			}
			goto case '\t';
		}
		return result;
	}

	private static InternalSyntaxToken Create(ref TokenInfo tokenInfo, InternalSyntaxListBuilder leadingTrivia, InternalSyntaxListBuilder trailingTrivia, SyntaxDiagnosticInfo[] errors)
	{
		InternalSyntaxNode leading = InternalSyntaxList.List(leadingTrivia);
		InternalSyntaxNode trailing = InternalSyntaxList.List(trailingTrivia);
		InternalSyntaxToken internalSyntaxToken = tokenInfo.Kind switch
		{
			SyntaxKind.IdentifierToken => InternalSyntaxFactory.Identifier(tokenInfo.ContextualKind, leading, tokenInfo.Text, tokenInfo.Text, trailing), 
			SyntaxKind.StringLiteralToken => InternalSyntaxFactory.Literal(leading, tokenInfo.Text, tokenInfo.StringValue, trailing), 
			SyntaxKind.Int32LiteralToken => InternalSyntaxFactory.Literal(leading, tokenInfo.Text, tokenInfo.Int32Value, trailing), 
			SyntaxKind.Int64LiteralToken => InternalSyntaxFactory.Literal(leading, tokenInfo.Text, tokenInfo.Int64Value, trailing), 
			SyntaxKind.DecimalLiteralToken => InternalSyntaxFactory.Literal(leading, tokenInfo.Text, tokenInfo.DecimalValue, trailing), 
			SyntaxKind.TimeLiteralToken => InternalSyntaxFactory.Literal(tokenInfo.Kind, leading, tokenInfo.Text, tokenInfo.Int32Value, trailing), 
			SyntaxKind.DateLiteralToken => InternalSyntaxFactory.Literal(tokenInfo.Kind, leading, tokenInfo.Text, tokenInfo.Int32Value, trailing), 
			SyntaxKind.DateTimeLiteralToken => InternalSyntaxFactory.Literal(tokenInfo.Kind, leading, tokenInfo.Text, tokenInfo.DateTimeValue, trailing), 
			SyntaxKind.TrueKeyword => InternalSyntaxFactory.Literal(tokenInfo.Kind, leading, tokenInfo.Text, value: true, trailing), 
			SyntaxKind.FalseKeyword => InternalSyntaxFactory.Literal(tokenInfo.Kind, leading, tokenInfo.Text, value: false, trailing), 
			SyntaxKind.None => InternalSyntaxFactory.BadToken(leading, tokenInfo.Text, trailing), 
			SyntaxKind.XmlTextLiteralNewLineToken => InternalSyntaxFactory.XmlTextNewLine(leading, tokenInfo.Text, tokenInfo.StringValue, trailing), 
			SyntaxKind.XmlTextLiteralToken => InternalSyntaxFactory.XmlTextLiteral(leading, tokenInfo.Text, tokenInfo.StringValue, trailing), 
			SyntaxKind.XmlEntityLiteralToken => InternalSyntaxFactory.XmlEntity(leading, tokenInfo.Text, tokenInfo.StringValue, trailing), 
			_ => InternalSyntaxFactory.Token(leading, tokenInfo.Kind, trailing), 
		};
		if (errors != null)
		{
			internalSyntaxToken = internalSyntaxToken.WithInternalDiagnostics(errors);
		}
		return internalSyntaxToken;
	}

	private InternalSyntaxNode ScanEndOfLine()
	{
		char ch;
		switch (ch = TextWindow.PeekChar())
		{
		case '\r':
			TextWindow.AdvanceChar();
			if (TextWindow.PeekChar() == '\n')
			{
				TextWindow.AdvanceChar();
				return InternalSyntaxFactory.CarriageReturnLineFeed;
			}
			return InternalSyntaxFactory.CarriageReturn;
		case '\n':
			TextWindow.AdvanceChar();
			return InternalSyntaxFactory.LineFeed;
		default:
			if (SyntaxFacts.IsNewLine(ch))
			{
				TextWindow.AdvanceChar();
				return InternalSyntaxFactory.EndOfLine(ch.ToString());
			}
			return null;
		}
	}

	private InternalSyntaxTrivia ScanWhitespace()
	{
		bool flag = true;
		int hashCode = -2128831035;
		while (true)
		{
			char c = TextWindow.PeekChar();
			switch (c)
			{
			default:
				if (c != '\u001a')
				{
					if (c == ' ')
					{
						goto IL_003f;
					}
					if (c <= '\u007f' || !SyntaxFacts.IsWhitespace(c))
					{
						break;
					}
				}
				goto case '\t';
			case '\t':
			case '\v':
			case '\f':
				flag = false;
				goto IL_003f;
			case '\n':
			case '\r':
				break;
			}
			break;
			IL_003f:
			TextWindow.AdvanceChar();
			hashCode = Hash.CombineFNVHash(hashCode, c);
		}
		if (flag)
		{
			return InternalSyntaxFactory.Spaces(TextWindow.Width);
		}
		int width = TextWindow.Width;
		if (width < 42)
		{
			return cache.LookupTrivia(TextWindow.CharacterWindow, TextWindow.LexemeRelativeStart, width, hashCode, CreateWhitespaceTrivia);
		}
		return CreateWhitespaceTrivia();
	}

	private InternalSyntaxTrivia CreateWhitespaceTrivia()
	{
		return InternalSyntaxFactory.Whitespace(TextWindow.GetText(intern: true));
	}

	private void Start()
	{
		errors = null;
		TextWindow.Start();
	}

	public static bool IsNewLine(char ch)
	{
		if (ch != '\n')
		{
			return ch == '\r';
		}
		return true;
	}

	private void ScanToEndOfLine()
	{
		char c;
		while (!IsNewLine(c = TextWindow.PeekChar()) && (c != '\uffff' || !TextWindow.IsReallyAtEnd()))
		{
			TextWindow.AdvanceChar();
		}
	}

	private bool ScanMultiLineComment(out bool isTerminated)
	{
		if (TextWindow.PeekChar() == '/' && TextWindow.PeekChar(1) == '*')
		{
			TextWindow.AdvanceChar(2);
			while (true)
			{
				char c;
				if ((c = TextWindow.PeekChar()) == '\uffff' && TextWindow.IsReallyAtEnd())
				{
					isTerminated = false;
					break;
				}
				if (c == '*' && TextWindow.PeekChar(1) == '/')
				{
					TextWindow.AdvanceChar(2);
					isTerminated = true;
					break;
				}
				TextWindow.AdvanceChar();
			}
			return true;
		}
		isTerminated = false;
		return false;
	}

	private void LexSyntaxTrivia(bool afterFirstToken, ref InternalSyntaxListBuilder triviaList, bool isTrailing)
	{
		bool flag = !isTrailing;
		while (true)
		{
			Start();
			char c = TextWindow.PeekChar();
			if (c == ' ')
			{
				AddTrivia(ScanWhitespace(), ref triviaList);
				continue;
			}
			if (c > '\u007f')
			{
				if (SyntaxFacts.IsWhitespace(c))
				{
					c = ' ';
				}
				else if (SyntaxFacts.IsNewLine(c))
				{
					c = '\n';
				}
			}
			switch (c)
			{
			default:
				return;
			case '\t':
			case '\v':
			case '\f':
			case '\u001a':
			case ' ':
				AddTrivia(ScanWhitespace(), ref triviaList);
				break;
			case '\n':
			case '\r':
				AddTrivia(ScanEndOfLine(), ref triviaList);
				if (isTrailing)
				{
					return;
				}
				break;
			case '/':
			{
				char c2 = TextWindow.PeekChar(1);
				if (c2 != '/' && c2 != '*')
				{
					return;
				}
				switch (c2)
				{
				case '/':
					if (!SuppressDocumentationCommentParse && TextWindow.PeekChar(2) == '/' && TextWindow.PeekChar(3) != '/')
					{
						if (isTrailing)
						{
							return;
						}
						AddTrivia(LexXmlDocComment(XmlDocCommentStyle.SingleLine), ref triviaList);
						break;
					}
					ScanToEndOfLine();
					AddTrivia(InternalSyntaxFactory.Comment(SyntaxKind.LineCommentTrivia, TextWindow.GetText(intern: false)), ref triviaList);
					if (isTrailing)
					{
						return;
					}
					break;
				case '*':
				{
					if (!SuppressDocumentationCommentParse && TextWindow.PeekChar(2) == '*' && TextWindow.PeekChar(3) != '*' && TextWindow.PeekChar(3) != '/')
					{
						if (isTrailing)
						{
							return;
						}
						AddTrivia(LexXmlDocComment(XmlDocCommentStyle.Delimited), ref triviaList);
						break;
					}
					ScanMultiLineComment(out var isTerminated);
					if (!isTerminated)
					{
						AddError(ErrorCode.ERR_InvalidMultilineComment);
					}
					AddTrivia(InternalSyntaxFactory.Comment(SyntaxKind.CommentTrivia, TextWindow.GetText(intern: false)), ref triviaList);
					break;
				}
				}
				break;
			}
			case '#':
				if (allowPreprocessorDirectives)
				{
					LexDirectiveAndExcludedTrivia(afterFirstToken, isTrailing || !flag, ref triviaList);
					break;
				}
				return;
			}
		}
	}

	private void ScanQuotedString(ref TokenInfo info, SyntaxKind stringKind)
	{
		char c = TextWindow.PeekChar();
		bool flag = c == '"';
		bool flag2 = flag;
		if (c == '\'' || flag)
		{
			TextWindow.AdvanceChar();
			builder.Length = 0;
			while (true)
			{
				char c2 = TextWindow.PeekChar();
				if (c2 == c)
				{
					TextWindow.AdvanceChar();
					if (TextWindow.PeekChar() != c)
					{
						break;
					}
				}
				else
				{
					if (IsNewLine(c2) || (c2 == '\uffff' && TextWindow.IsReallyAtEnd()))
					{
						if (c == '\'')
						{
							AddError(ErrorCode.ERR_TextLiteralNotProperlyTerminated, builder.ToString());
						}
						else
						{
							AddError(ErrorCode.ERR_IdentifierNotProperlyTerminated, builder.ToString());
						}
						break;
					}
					if (flag2)
					{
						flag2 = IsValidXmlChar(c2);
					}
				}
				TextWindow.AdvanceChar();
				builder.Append(c2);
			}
			if (flag && !flag2)
			{
				AddError(ErrorCode.WRN_ERR_IdentifierContainsInvalidCharacters, builder.ToString());
			}
			info.Text = TextWindow.GetText(intern: true);
			info.Kind = (info.ContextualKind = stringKind);
			if (builder.Length > 0)
			{
				info.StringValue = builder.ToString();
			}
			else
			{
				info.StringValue = string.Empty;
			}
		}
		else
		{
			info.Kind = SyntaxKind.None;
			info.Text = null;
		}
	}

	private bool IsValidXmlChar(char ch)
	{
		if (ch < ' ')
		{
			if (ch != '\t' && ch != '\n')
			{
				return ch == '\r';
			}
			return true;
		}
		if (ch < '\ue000')
		{
			return ch <= '\ud7ff';
		}
		if (ch < 65536)
		{
			return ch <= '\ud7ff';
		}
		return ch <= 1114111;
	}

	private bool TryScanAtStringToken(ref TokenInfo info)
	{
		DebugAssertHelper.Assert(TextWindow.PeekChar() == '@');
		int position = TextWindow.Position;
		int i;
		for (i = 0; TextWindow.PeekChar(i) == '@'; i++)
		{
		}
		if (TextWindow.PeekChar(i) == '\'')
		{
			ScanVerbatimStringLiteral(ref info);
			if (!verbatimStringLiteralSupport)
			{
				AddError(CreateFeatureNotAvailableError(Feature.VerbatimStringLiterals, position));
			}
			return true;
		}
		return false;
	}

	private SyntaxDiagnosticInfo CreateFeatureNotAvailableError(Feature feature, int start)
	{
		string text = feature.Localize();
		Version arg = feature.RequiredVersion();
		return new SyntaxDiagnosticInfo(start - TextWindow.LexemeStartPosition, TextWindow.Position - start, ErrorCode.ERR_FeatureNotAvailable, text, Options.RuntimeVersion, string.Format(CultureInfo.CurrentCulture, CodeAnalysisResources.VersionOrGreater, arg));
	}

	private void ScanVerbatimStringLiteral(ref TokenInfo info)
	{
		DebugAssertHelper.Assert(TextWindow.PeekChar() == '@');
		builder.Length = 0;
		int position = TextWindow.Position;
		while (TextWindow.PeekChar() == '@')
		{
			TextWindow.AdvanceChar();
		}
		if (TextWindow.Position - position >= 2)
		{
			AddError(position, TextWindow.Position - position, ErrorCode.ERR_IllegalAtSequence);
		}
		DebugAssertHelper.Assert(TextWindow.PeekChar() == '\'');
		TextWindow.AdvanceChar();
		while (true)
		{
			char c = TextWindow.PeekChar();
			if (c == '\'')
			{
				TextWindow.AdvanceChar();
				if (TextWindow.PeekChar() != '\'')
				{
					break;
				}
				TextWindow.AdvanceChar();
				builder.Append(c);
			}
			else
			{
				if (c == '\uffff' && TextWindow.IsReallyAtEnd())
				{
					AddError(ErrorCode.ERR_TextLiteralNotProperlyTerminated);
					break;
				}
				TextWindow.AdvanceChar();
				builder.Append(c);
			}
		}
		info.Kind = SyntaxKind.StringLiteralToken;
		info.Text = TextWindow.GetText(intern: false);
		info.StringValue = builder.ToString();
	}

	private void ScanNumericLiteral(ref TokenInfo info)
	{
		info.Kind = SyntaxKind.Int32LiteralToken;
		builder.Length = 0;
		char c;
		while (char.IsDigit(c = TextWindow.PeekChar()))
		{
			builder.Append(c);
			TextWindow.AdvanceChar();
		}
		if (c == '.' && char.IsDigit(TextWindow.PeekChar(1)))
		{
			info.Kind = SyntaxKind.DecimalLiteralToken;
			TextWindow.AdvanceChar();
			builder.Append(c);
			while (char.IsDigit(c = TextWindow.PeekChar()))
			{
				TextWindow.AdvanceChar();
				builder.Append(c);
			}
			if (c == 'T')
			{
				info.Kind = SyntaxKind.TimeLiteralToken;
				if (!DateTimeUtilities.TryParseTime(builder.ToString(), out info.Int32Value))
				{
					builder.Append(c);
					AddError(ErrorCode.ERR_InvalidTimeLiteral, builder.ToString());
				}
				TextWindow.AdvanceChar();
			}
			else if (!decimal.TryParse(builder.ToString(), NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out info.DecimalValue))
			{
				AddError(ErrorCode.ERR_InvalidDecimalLiteral, builder.ToString());
			}
		}
		else
		{
			switch (c)
			{
			case 'D':
				info.Kind = SyntaxKind.DateLiteralToken;
				builder.Append(c);
				TextWindow.AdvanceChar();
				if (TextWindow.PeekChar() == 'T')
				{
					builder.Append(TextWindow.PeekChar());
					TextWindow.AdvanceChar();
					if (ParseZeroDateTime(builder.ToString(), out info.Int32Value))
					{
						info.Kind = SyntaxKind.DateTimeLiteralToken;
						break;
					}
					AddError(ErrorCode.ERR_InvalidDateTimeLiteral, builder.ToString());
				}
				else if (!DateTimeUtilities.TryParseDate(builder.ToString(), out info.Int32Value))
				{
					AddError(ErrorCode.ERR_InvalidDateLiteral, builder.ToString());
				}
				break;
			case 'T':
				info.Kind = SyntaxKind.TimeLiteralToken;
				TextWindow.AdvanceChar();
				if (!DateTimeUtilities.TryParseTime(builder.ToString(), out info.Int32Value))
				{
					builder.Append(c);
					AddError(ErrorCode.ERR_InvalidTimeLiteral, builder.ToString());
				}
				break;
			case 'L':
				info.Kind = SyntaxKind.Int64LiteralToken;
				TextWindow.AdvanceChar();
				if (!long.TryParse(builder.ToString(), out info.Int64Value))
				{
					AddError(ErrorCode.ERR_InvalidInt64Literal, builder?.ToString() + "L");
				}
				break;
			default:
				if (!int.TryParse(builder.ToString(), out info.Int32Value))
				{
					AddError(ErrorCode.ERR_InvalidInt32Literal, builder.ToString());
				}
				break;
			}
		}
		info.Text = TextWindow.GetText(intern: true);
	}

	private static bool ParseZeroDateTime(string value, out int dateTime)
	{
		dateTime = 0;
		return value == "0DT";
	}

	internal SyntaxTriviaList ScanSyntaxLeadingTrivia()
	{
		leadingTriviaCache.Clear();
		LexSyntaxTrivia(TextWindow.Position > 0, ref leadingTriviaCache, isTrailing: false);
		return new SyntaxTriviaList(default(SyntaxToken), leadingTriviaCache.ToListNode(), 0);
	}

	internal SyntaxTriviaList ScanSyntaxTrailingTrivia()
	{
		trailingTriviaCache.Clear();
		LexSyntaxTrivia(afterFirstToken: true, ref trailingTriviaCache, isTrailing: true);
		return new SyntaxTriviaList(default(SyntaxToken), trailingTriviaCache.ToListNode(), 0);
	}

	private void ScanIdentifierOrKeyword(ref TokenInfo info)
	{
		info.ContextualKind = SyntaxKind.None;
		if (ScanIdentifier(ref info))
		{
			if (!cache.TryGetKeywordKind(info.Text, mode, out info.Kind))
			{
				info.ContextualKind = (info.Kind = SyntaxKind.IdentifierToken);
			}
			else if (SyntaxFacts.IsContextualKeyword(info.Kind))
			{
				info.ContextualKind = info.Kind;
				info.Kind = SyntaxKind.IdentifierToken;
			}
		}
		else
		{
			info.Kind = SyntaxKind.None;
		}
	}

	private void ResetIdentBuffer()
	{
		identLen = 0;
	}

	private void AddIdentChar(char ch)
	{
		if (identLen >= identBuffer.Length)
		{
			GrowIdentBuffer();
		}
		identBuffer[identLen++] = ch;
	}

	private void GrowIdentBuffer()
	{
		char[] destinationArray = new char[identBuffer.Length * 2];
		Array.Copy(identBuffer, destinationArray, identBuffer.Length);
		identBuffer = destinationArray;
	}

	private bool ScanIdentifier(ref TokenInfo info)
	{
		if (!ScanIdentifier_FastPath(ref info))
		{
			return ScanIdentifier_SlowPath(ref info);
		}
		return true;
	}

	private bool ScanIdentifier_FastPath(ref TokenInfo info)
	{
		int i = TextWindow.Offset;
		char[] characterWindow = TextWindow.CharacterWindow;
		int characterWindowCount = TextWindow.CharacterWindowCount;
		int num = i;
		for (; i != characterWindowCount; i++)
		{
			switch (characterWindow[i])
			{
			case '\0':
			case '\t':
			case '\n':
			case '\r':
			case ' ':
			case '!':
			case '"':
			case '%':
			case '&':
			case '\'':
			case '(':
			case ')':
			case '*':
			case '+':
			case ',':
			case '-':
			case '.':
			case '/':
			case ':':
			case ';':
			case '<':
			case '=':
			case '>':
			case '?':
			case '[':
			case ']':
			case '^':
			case '{':
			case '|':
			case '}':
			case '~':
			{
				int num2 = i - num;
				TextWindow.AdvanceChar(num2);
				info.Text = (info.StringValue = TextWindow.Intern(characterWindow, num, num2));
				return true;
			}
			case '0':
			case '1':
			case '2':
			case '3':
			case '4':
			case '5':
			case '6':
			case '7':
			case '8':
			case '9':
				if (i == num)
				{
					return false;
				}
				break;
			case 'A':
			case 'B':
			case 'C':
			case 'D':
			case 'E':
			case 'F':
			case 'G':
			case 'H':
			case 'I':
			case 'J':
			case 'K':
			case 'L':
			case 'M':
			case 'N':
			case 'O':
			case 'P':
			case 'Q':
			case 'R':
			case 'S':
			case 'T':
			case 'U':
			case 'V':
			case 'W':
			case 'X':
			case 'Y':
			case 'Z':
			case '_':
			case 'a':
			case 'b':
			case 'c':
			case 'd':
			case 'e':
			case 'f':
			case 'g':
			case 'h':
			case 'i':
			case 'j':
			case 'k':
			case 'l':
			case 'm':
			case 'n':
			case 'o':
			case 'p':
			case 'q':
			case 'r':
			case 's':
			case 't':
			case 'u':
			case 'v':
			case 'w':
			case 'x':
			case 'y':
			case 'z':
				break;
			default:
				return false;
			}
		}
		return false;
	}

	private bool ScanIdentifier_SlowPath(ref TokenInfo info)
	{
		int position = TextWindow.Position;
		ResetIdentBuffer();
		while (true)
		{
			char surrogateCharacter = '\uffff';
			bool flag = false;
			char c = TextWindow.PeekChar();
			while (true)
			{
				switch (c)
				{
				case '\\':
					if (!flag && TextWindow.IsUnicodeEscape())
					{
						goto IL_0214;
					}
					goto default;
				case '\uffff':
					if (!TextWindow.IsReallyAtEnd())
					{
						goto default;
					}
					goto case '\t';
				case '0':
				case '1':
				case '2':
				case '3':
				case '4':
				case '5':
				case '6':
				case '7':
				case '8':
				case '9':
					if (identLen != 0)
					{
						goto case 'A';
					}
					goto case '\t';
				default:
					if (identLen != 0 || c <= '\u007f' || !SyntaxFacts.IsIdentifierStartCharacter(c))
					{
						if (identLen <= 0 || c <= '\u007f' || !SyntaxFacts.IsIdentifierPartCharacter(c))
						{
							goto case '\t';
						}
						if (UnicodeCharacterUtilities.IsFormattingChar(c))
						{
							if (flag)
							{
								TextWindow.NextCharOrUnicodeEscape(out surrogateCharacter, out SyntaxDiagnosticInfo info2);
								AddError(info2);
							}
							else
							{
								TextWindow.AdvanceChar();
							}
							break;
						}
					}
					goto case 'A';
				case 'A':
				case 'B':
				case 'C':
				case 'D':
				case 'E':
				case 'F':
				case 'G':
				case 'H':
				case 'I':
				case 'J':
				case 'K':
				case 'L':
				case 'M':
				case 'N':
				case 'O':
				case 'P':
				case 'Q':
				case 'R':
				case 'S':
				case 'T':
				case 'U':
				case 'V':
				case 'W':
				case 'X':
				case 'Y':
				case 'Z':
				case '_':
				case 'a':
				case 'b':
				case 'c':
				case 'd':
				case 'e':
				case 'f':
				case 'g':
				case 'h':
				case 'i':
				case 'j':
				case 'k':
				case 'l':
				case 'm':
				case 'n':
				case 'o':
				case 'p':
				case 'q':
				case 'r':
				case 's':
				case 't':
				case 'u':
				case 'v':
				case 'w':
				case 'x':
				case 'y':
				case 'z':
					if (flag)
					{
						TextWindow.NextCharOrUnicodeEscape(out surrogateCharacter, out SyntaxDiagnosticInfo info3);
						AddError(info3);
					}
					else
					{
						TextWindow.AdvanceChar();
					}
					AddIdentChar(c);
					if (surrogateCharacter != '\uffff')
					{
						AddIdentChar(surrogateCharacter);
					}
					break;
				case '\t':
				case ' ':
				case '$':
				case '(':
				case ')':
				case ',':
				case '.':
				case ';':
				case '<':
				{
					int width = TextWindow.Width;
					if (identLen > 0)
					{
						info.Text = TextWindow.GetInternedText();
						if (identLen == width)
						{
							info.StringValue = info.Text;
						}
						else
						{
							info.StringValue = TextWindow.Intern(identBuffer, 0, identLen);
						}
						return true;
					}
					info.Text = null;
					info.StringValue = null;
					TextWindow.Reset(position);
					return false;
				}
				}
				break;
				IL_0214:
				info.HasIdentifierEscapeSequence = true;
				flag = true;
				c = TextWindow.PeekUnicodeEscape(out surrogateCharacter);
			}
		}
	}

	private void ScanSyntaxToken(ref TokenInfo info)
	{
		info.Kind = SyntaxKind.None;
		info.ContextualKind = SyntaxKind.None;
		info.Text = null;
		char c = TextWindow.PeekChar();
		switch (c)
		{
		case '\'':
			ScanQuotedString(ref info, SyntaxKind.StringLiteralToken);
			break;
		case '"':
			ScanQuotedString(ref info, SyntaxKind.IdentifierToken);
			break;
		case '@':
			if (!TryScanAtStringToken(ref info))
			{
				DebugAssertHelper.Assert(TextWindow.PeekChar() == '@');
				TextWindow.AdvanceChar();
				info.Text = TextWindow.GetText(intern: true);
				AddError(ErrorCode.ERR_ExpectedVerbatimLiteral);
			}
			break;
		case '0':
		case '1':
		case '2':
		case '3':
		case '4':
		case '5':
		case '6':
		case '7':
		case '8':
		case '9':
			ScanNumericLiteral(ref info);
			break;
		case 'A':
		case 'B':
		case 'C':
		case 'D':
		case 'E':
		case 'F':
		case 'G':
		case 'H':
		case 'I':
		case 'J':
		case 'K':
		case 'L':
		case 'M':
		case 'N':
		case 'O':
		case 'P':
		case 'Q':
		case 'R':
		case 'S':
		case 'T':
		case 'U':
		case 'V':
		case 'W':
		case 'X':
		case 'Y':
		case 'Z':
		case '_':
		case 'a':
		case 'b':
		case 'c':
		case 'd':
		case 'e':
		case 'f':
		case 'g':
		case 'h':
		case 'i':
		case 'j':
		case 'k':
		case 'l':
		case 'm':
		case 'n':
		case 'o':
		case 'p':
		case 'q':
		case 'r':
		case 's':
		case 't':
		case 'u':
		case 'v':
		case 'w':
		case 'x':
		case 'y':
		case 'z':
			ScanIdentifierOrKeyword(ref info);
			break;
		case '+':
			TextWindow.AdvanceChar();
			if (TextWindow.PeekChar() == '=')
			{
				TextWindow.AdvanceChar();
				info.Kind = SyntaxKind.AssignPlusToken;
			}
			else
			{
				info.Kind = SyntaxKind.PlusToken;
			}
			break;
		case '-':
			TextWindow.AdvanceChar();
			if (TextWindow.PeekChar() == '=')
			{
				TextWindow.AdvanceChar();
				info.Kind = SyntaxKind.AssignMinusToken;
			}
			else
			{
				info.Kind = SyntaxKind.MinusToken;
			}
			break;
		case '*':
			TextWindow.AdvanceChar();
			if (TextWindow.PeekChar() == '=')
			{
				TextWindow.AdvanceChar();
				info.Kind = SyntaxKind.AssignMultiplyToken;
			}
			else
			{
				info.Kind = SyntaxKind.MultiplyToken;
			}
			break;
		case '/':
			TextWindow.AdvanceChar();
			if (TextWindow.PeekChar() == '=')
			{
				TextWindow.AdvanceChar();
				info.Kind = SyntaxKind.AssignRDivToken;
			}
			else
			{
				info.Kind = SyntaxKind.RDivToken;
			}
			break;
		case '(':
			TextWindow.AdvanceChar();
			info.Kind = SyntaxKind.OpenParenToken;
			break;
		case ')':
			TextWindow.AdvanceChar();
			info.Kind = SyntaxKind.CloseParenToken;
			break;
		case '{':
			TextWindow.AdvanceChar();
			info.Kind = SyntaxKind.OpenBraceToken;
			break;
		case '}':
			TextWindow.AdvanceChar();
			info.Kind = SyntaxKind.CloseBraceToken;
			break;
		case '[':
			TextWindow.AdvanceChar();
			info.Kind = SyntaxKind.OpenBracketToken;
			break;
		case ']':
			TextWindow.AdvanceChar();
			info.Kind = SyntaxKind.CloseBracketToken;
			break;
		case '<':
			TextWindow.AdvanceChar();
			info.Kind = SyntaxKind.LessThanToken;
			if ((c = TextWindow.PeekChar()) == '=')
			{
				TextWindow.AdvanceChar();
				info.Kind = SyntaxKind.LessThanEqualsToken;
			}
			else if (c == '>')
			{
				TextWindow.AdvanceChar();
				info.Kind = SyntaxKind.NotEqualsToken;
			}
			break;
		case '=':
			TextWindow.AdvanceChar();
			info.Kind = SyntaxKind.EqualsToken;
			break;
		case '>':
			TextWindow.AdvanceChar();
			info.Kind = SyntaxKind.GreaterThanToken;
			if (TextWindow.PeekChar() == '=')
			{
				TextWindow.AdvanceChar();
				info.Kind = SyntaxKind.GreaterThanEqualsToken;
			}
			break;
		case ':':
			TextWindow.AdvanceChar();
			info.Kind = SyntaxKind.ColonToken;
			if ((c = TextWindow.PeekChar()) == '=')
			{
				TextWindow.AdvanceChar();
				info.Kind = SyntaxKind.AssignToken;
			}
			else if (c == ':')
			{
				TextWindow.AdvanceChar();
				info.Kind = SyntaxKind.ColonColonToken;
			}
			break;
		case ',':
			TextWindow.AdvanceChar();
			info.Kind = SyntaxKind.CommaToken;
			break;
		case ';':
			TextWindow.AdvanceChar();
			info.Kind = SyntaxKind.SemicolonToken;
			break;
		case '.':
			TextWindow.AdvanceChar();
			info.Kind = SyntaxKind.DotToken;
			if (TextWindow.PeekChar() == '.')
			{
				TextWindow.AdvanceChar();
				info.Kind = SyntaxKind.DotDotToken;
			}
			break;
		case '&':
			if (mode == LexerMode.Property)
			{
				TextWindow.AdvanceChar();
				info.Kind = SyntaxKind.AndFilterKeyword;
				break;
			}
			goto default;
		case '#':
			TextWindow.AdvanceChar();
			info.Kind = SyntaxKind.HashToken;
			break;
		case '?':
			TextWindow.AdvanceChar();
			info.Kind = SyntaxKind.QuestionToken;
			break;
		case '|':
			if (mode == LexerMode.Property)
			{
				TextWindow.AdvanceChar();
				info.Kind = SyntaxKind.OrFilterKeyword;
				break;
			}
			goto default;
		case '\uffff':
			if (TextWindow.IsReallyAtEnd())
			{
				if (directives.HasUnfinishedIf())
				{
					AddError(ErrorCode.ERR_EndifDirectiveExpected);
				}
				if (directives.HasUnfinishedRegion())
				{
					AddError(ErrorCode.ERR_EndRegionDirectiveExpected);
				}
				info.Kind = SyntaxKind.EndOfFileToken;
				break;
			}
			goto default;
		default:
			if (UnicodeCharacterUtilities.IsIdentifierStartCharacter(c))
			{
				ScanIdentifierOrKeyword(ref info);
				break;
			}
			TextWindow.AdvanceChar();
			info.Text = TextWindow.GetText(intern: false);
			AddError(ErrorCode.ERR_UnexpectedCharacter, info.Text);
			break;
		}
		if (info.Kind == SyntaxKind.IdentifierToken && info.Text.Length > 120)
		{
			AddError(0, info.Text.Length, ErrorCode.ERR_IdentifierTooLong, info.Text, 120);
		}
		if (info.ContextualKind == SyntaxKind.None)
		{
			info.ContextualKind = info.Kind;
		}
	}

	private InternalSyntaxNode LexXmlDocComment(XmlDocCommentStyle style)
	{
		LexerMode lexerMode = mode;
		LexerMode modeflags = ((style != 0) ? LexerMode.XmlDocCommentStyleDelimited : LexerMode.XmlDocCommentLocationStart);
		if (xmlParser == null)
		{
			xmlParser = new DocumentationCommentParser(this, modeflags);
		}
		else
		{
			xmlParser.ReInitialize(modeflags);
		}
		bool isTerminated;
		InternalDocumentationCommentTriviaSyntax result = xmlParser.ParseDocumentationComment(out isTerminated);
		DebugAssertHelper.Assert(LocationIs(XmlDocCommentLocation.End) || TextWindow.PeekChar() == '\uffff');
		mode = lexerMode;
		if (!isTerminated)
		{
			AddError(TextWindow.LexemeStartPosition, TextWindow.Width, ErrorCode.ERR_OpenEndedComment);
		}
		return result;
	}

	private InternalSyntaxToken LexXmlToken()
	{
		TokenInfo info = default(TokenInfo);
		InternalSyntaxListBuilder trivia = null;
		LexXmlDocCommentLeadingTrivia(ref trivia);
		Start();
		ScanXmlToken(ref info);
		SyntaxDiagnosticInfo[] array = GetErrors(GetFullWidth(trivia));
		return Create(ref info, trivia, null, array);
	}

	private bool ScanXmlToken(ref TokenInfo info)
	{
		DebugAssertHelper.Assert(!LocationIs(XmlDocCommentLocation.Start));
		DebugAssertHelper.Assert(!LocationIs(XmlDocCommentLocation.Exterior));
		if (LocationIs(XmlDocCommentLocation.End))
		{
			info.Kind = SyntaxKind.EndOfDocumentationCommentToken;
			return true;
		}
		char ch;
		char c = (ch = TextWindow.PeekChar());
		if ((uint)c <= 13u)
		{
			if (c == '\n' || c == '\r')
			{
				goto IL_008b;
			}
			goto IL_00ae;
		}
		if (c != '&')
		{
			if (c != '<')
			{
				if (c != '\uffff' || !TextWindow.IsReallyAtEnd())
				{
					goto IL_00ae;
				}
				info.Kind = SyntaxKind.EndOfDocumentationCommentToken;
			}
			else
			{
				ScanXmlTagStart(ref info);
			}
		}
		else
		{
			ScanXmlEntity(ref info);
			info.Kind = SyntaxKind.XmlEntityLiteralToken;
		}
		goto IL_00c5;
		IL_008b:
		ScanXmlTextLiteralNewLineToken(ref info);
		goto IL_00c5;
		IL_00ae:
		if (SyntaxFacts.IsNewLine(ch))
		{
			goto IL_008b;
		}
		ScanXmlText(ref info);
		info.Kind = SyntaxKind.XmlTextLiteralToken;
		goto IL_00c5;
		IL_00c5:
		DebugAssertHelper.Assert(info.Kind != 0 || info.Text != null);
		return info.Kind != SyntaxKind.None;
	}

	private void ScanXmlTextLiteralNewLineToken(ref TokenInfo info)
	{
		ScanEndOfLine();
		info.StringValue = (info.Text = TextWindow.GetText(intern: false));
		info.Kind = SyntaxKind.XmlTextLiteralNewLineToken;
		MutateLocation(XmlDocCommentLocation.Exterior);
	}

	private void ScanXmlTagStart(ref TokenInfo info)
	{
		DebugAssertHelper.Assert(TextWindow.PeekChar() == '<');
		if (TextWindow.PeekChar(1) == '!')
		{
			if (TextWindow.PeekChar(2) == '-' && TextWindow.PeekChar(3) == '-')
			{
				TextWindow.AdvanceChar(4);
				info.Kind = SyntaxKind.XmlCommentStartToken;
			}
			else if (TextWindow.PeekChar(2) == '[' && TextWindow.PeekChar(3) == 'C' && TextWindow.PeekChar(4) == 'D' && TextWindow.PeekChar(5) == 'A' && TextWindow.PeekChar(6) == 'T' && TextWindow.PeekChar(7) == 'A' && TextWindow.PeekChar(8) == '[')
			{
				TextWindow.AdvanceChar(9);
				info.Kind = SyntaxKind.XmlCDataStartToken;
			}
			else
			{
				TextWindow.AdvanceChar();
				info.Kind = SyntaxKind.LessThanToken;
			}
		}
		else if (TextWindow.PeekChar(1) == '/')
		{
			TextWindow.AdvanceChar(2);
			info.Kind = SyntaxKind.LessThanSlashToken;
		}
		else if (TextWindow.PeekChar(1) == '?')
		{
			TextWindow.AdvanceChar(2);
			info.Kind = SyntaxKind.XmlProcessingInstructionStartToken;
		}
		else
		{
			TextWindow.AdvanceChar();
			info.Kind = SyntaxKind.LessThanToken;
		}
	}

	private void ScanXmlEntity(ref TokenInfo info)
	{
		info.StringValue = null;
		DebugAssertHelper.Assert(TextWindow.PeekChar() == '&');
		TextWindow.AdvanceChar();
		builder.Clear();
		XmlParseErrorCode? xmlParseErrorCode = null;
		object[] array = null;
		char c;
		if (IsXmlNameStartChar(c = TextWindow.PeekChar()))
		{
			while (IsXmlNameChar(c = TextWindow.PeekChar()))
			{
				TextWindow.AdvanceChar();
				builder.Append(c);
			}
			switch (builder.ToString())
			{
			case "lt":
				info.StringValue = "<";
				break;
			case "gt":
				info.StringValue = ">";
				break;
			case "amp":
				info.StringValue = "&";
				break;
			case "apos":
				info.StringValue = "'";
				break;
			case "quot":
				info.StringValue = "\"";
				break;
			default:
			{
				xmlParseErrorCode = XmlParseErrorCode.XML_RefUndefinedEntity_1;
				object[] array2 = new string[1] { builder.ToString() };
				array = array2;
				break;
			}
			}
		}
		else if (c == '#')
		{
			TextWindow.AdvanceChar();
			bool num = TextWindow.PeekChar() == 'x';
			uint num2 = 0u;
			if (num)
			{
				TextWindow.AdvanceChar();
				while (SyntaxFacts.IsHexDigit(c = TextWindow.PeekChar()))
				{
					TextWindow.AdvanceChar();
					if (num2 <= 134217727)
					{
						num2 = (num2 << 4) + (uint)SyntaxFacts.HexValue(c);
					}
				}
			}
			else
			{
				while (SyntaxFacts.IsDecDigit(c = TextWindow.PeekChar()))
				{
					TextWindow.AdvanceChar();
					if (num2 <= 134217727)
					{
						num2 = (num2 << 3) + (num2 << 1) + (uint)SyntaxFacts.DecValue(c);
					}
				}
			}
			if (TextWindow.PeekChar() != ';')
			{
				xmlParseErrorCode = XmlParseErrorCode.XML_InvalidCharEntity;
			}
			if (MatchesProductionForXmlChar(num2))
			{
				char lowSurrogate;
				char charsFromUtf = SlidingTextWindow.GetCharsFromUtf32(num2, out lowSurrogate);
				builder.Append(charsFromUtf);
				if (lowSurrogate != '\uffff')
				{
					builder.Append(lowSurrogate);
				}
				info.StringValue = builder.ToString();
			}
			else if (!xmlParseErrorCode.HasValue)
			{
				xmlParseErrorCode = XmlParseErrorCode.XML_InvalidUnicodeChar;
			}
		}
		else if (SyntaxFacts.IsWhitespace(c) || SyntaxFacts.IsNewLine(c))
		{
			if (!xmlParseErrorCode.HasValue)
			{
				xmlParseErrorCode = XmlParseErrorCode.XML_InvalidWhitespace;
			}
		}
		else if (!xmlParseErrorCode.HasValue)
		{
			xmlParseErrorCode = XmlParseErrorCode.XML_InvalidToken;
			object[] array2 = new string[1] { c.ToString() };
			array = array2;
		}
		c = TextWindow.PeekChar();
		if (c == ';')
		{
			TextWindow.AdvanceChar();
		}
		else if (!xmlParseErrorCode.HasValue)
		{
			xmlParseErrorCode = XmlParseErrorCode.XML_InvalidToken;
			object[] array2 = new string[1] { c.ToString() };
			array = array2;
		}
		info.Text = TextWindow.GetText(intern: true);
		if (info.StringValue == null)
		{
			info.StringValue = info.Text;
		}
		if (xmlParseErrorCode.HasValue)
		{
			AddError(xmlParseErrorCode.Value, array ?? Array.Empty<object>());
		}
	}

	private static bool MatchesProductionForXmlChar(uint charValue)
	{
		if (charValue != 9 && charValue != 10 && charValue != 13 && (charValue < 32 || charValue > 55295) && (charValue < 57344 || charValue > 65533))
		{
			if (charValue >= 65536)
			{
				return charValue <= 1114111;
			}
			return false;
		}
		return true;
	}

	private void ScanXmlText(ref TokenInfo info)
	{
		if (TextWindow.PeekChar() == ']' && TextWindow.PeekChar(1) == ']' && TextWindow.PeekChar(2) == '>')
		{
			TextWindow.AdvanceChar(3);
			info.StringValue = (info.Text = TextWindow.GetText(intern: false));
			AddError(XmlParseErrorCode.XML_CDataEndTagNotAllowed);
			return;
		}
		while (true)
		{
			char c = TextWindow.PeekChar();
			if ((uint)c <= 38u)
			{
				if (c == '\n' || c == '\r' || c == '&')
				{
					goto IL_00d7;
				}
			}
			else if ((uint)c <= 60u)
			{
				if (c != '*')
				{
					if (c == '<')
					{
						goto IL_00d7;
					}
				}
				else if (StyleIs(XmlDocCommentStyle.Delimited) && TextWindow.PeekChar(1) == '/')
				{
					break;
				}
			}
			else
			{
				switch (c)
				{
				case '\uffff':
					if (TextWindow.IsReallyAtEnd())
					{
						info.StringValue = (info.Text = TextWindow.GetText(intern: false));
						return;
					}
					break;
				case ']':
					if (TextWindow.PeekChar(1) == ']' && TextWindow.PeekChar(2) == '>')
					{
						info.StringValue = (info.Text = TextWindow.GetText(intern: false));
						return;
					}
					break;
				}
			}
			if (!SyntaxFacts.IsNewLine(c))
			{
				TextWindow.AdvanceChar();
				continue;
			}
			goto IL_00d7;
			IL_00d7:
			info.StringValue = (info.Text = TextWindow.GetText(intern: false));
			return;
		}
		info.StringValue = (info.Text = TextWindow.GetText(intern: false));
	}

	private InternalSyntaxToken LexXmlElementTagToken()
	{
		TokenInfo info = default(TokenInfo);
		InternalSyntaxListBuilder trivia = null;
		LexXmlDocCommentLeadingTriviaWithWhitespace(ref trivia);
		Start();
		ScanXmlElementTagToken(ref info);
		SyntaxDiagnosticInfo[] array = GetErrors(GetFullWidth(trivia));
		if (array == null && info.Kind == SyntaxKind.IdentifierToken)
		{
			InternalSyntaxToken internalSyntaxToken = DocumentationCommentXmlTokens.LookupToken(info.Text, trivia);
			if (internalSyntaxToken != null)
			{
				return internalSyntaxToken;
			}
		}
		return Create(ref info, trivia, null, array);
	}

	private bool ScanXmlElementTagToken(ref TokenInfo info)
	{
		DebugAssertHelper.Assert(!LocationIs(XmlDocCommentLocation.Start));
		DebugAssertHelper.Assert(!LocationIs(XmlDocCommentLocation.Exterior));
		if (LocationIs(XmlDocCommentLocation.End))
		{
			info.Kind = SyntaxKind.EndOfDocumentationCommentToken;
			return true;
		}
		char ch;
		switch (ch = TextWindow.PeekChar())
		{
		case '<':
			ScanXmlTagStart(ref info);
			break;
		case '>':
			TextWindow.AdvanceChar();
			info.Kind = SyntaxKind.GreaterThanToken;
			break;
		case '/':
			if (TextWindow.PeekChar(1) == '>')
			{
				TextWindow.AdvanceChar(2);
				info.Kind = SyntaxKind.SlashGreaterThanToken;
				break;
			}
			goto default;
		case '"':
			TextWindow.AdvanceChar();
			info.Kind = SyntaxKind.DoubleQuoteToken;
			break;
		case '\'':
			TextWindow.AdvanceChar();
			info.Kind = SyntaxKind.SingleQuoteToken;
			break;
		case '=':
			TextWindow.AdvanceChar();
			info.Kind = SyntaxKind.EqualsToken;
			break;
		case ':':
			TextWindow.AdvanceChar();
			info.Kind = SyntaxKind.ColonToken;
			break;
		case '\uffff':
			if (TextWindow.IsReallyAtEnd())
			{
				info.Kind = SyntaxKind.EndOfDocumentationCommentToken;
				break;
			}
			goto default;
		case '*':
			if (StyleIs(XmlDocCommentStyle.Delimited) && TextWindow.PeekChar(1) == '/')
			{
				DebugAssertHelper.Assert(condition: false, "Should have picked up leading indentationTrivia, but didn't.");
				break;
			}
			goto default;
		default:
			if (IsXmlNameStartChar(ch))
			{
				ScanXmlName(ref info);
				info.StringValue = info.Text;
				info.Kind = SyntaxKind.IdentifierToken;
			}
			else if (SyntaxFacts.IsWhitespace(ch) || SyntaxFacts.IsNewLine(ch))
			{
				DebugAssertHelper.Assert(condition: false, "Should have picked up leading indentationTrivia, but didn't.");
			}
			else
			{
				TextWindow.AdvanceChar();
				info.Kind = SyntaxKind.None;
				info.StringValue = (info.Text = TextWindow.GetText(intern: false));
			}
			break;
		case '\n':
		case '\r':
			break;
		}
		DebugAssertHelper.Assert(info.Kind != 0 || info.Text != null);
		return info.Kind != SyntaxKind.None;
	}

	private void ScanXmlName(ref TokenInfo info)
	{
		int position = TextWindow.Position;
		while (true)
		{
			char c = TextWindow.PeekChar();
			if (c == ':' || !IsXmlNameChar(c))
			{
				break;
			}
			TextWindow.AdvanceChar();
		}
		info.Text = TextWindow.GetText(position, TextWindow.Position - position, intern: true);
	}

	private static bool IsXmlNameStartChar(char ch)
	{
		return XmlCharType.IsStartNCNameCharXml4e(ch);
	}

	private static bool IsXmlNameChar(char ch)
	{
		return XmlCharType.IsNCNameCharXml4e(ch);
	}

	private InternalSyntaxToken LexXmlAttributeTextToken()
	{
		TokenInfo info = default(TokenInfo);
		InternalSyntaxListBuilder trivia = null;
		LexXmlDocCommentLeadingTrivia(ref trivia);
		Start();
		ScanXmlAttributeTextToken(ref info);
		SyntaxDiagnosticInfo[] array = GetErrors(GetFullWidth(trivia));
		return Create(ref info, trivia, null, array);
	}

	private bool ScanXmlAttributeTextToken(ref TokenInfo info)
	{
		DebugAssertHelper.Assert(!LocationIs(XmlDocCommentLocation.Start));
		DebugAssertHelper.Assert(!LocationIs(XmlDocCommentLocation.Exterior));
		if (LocationIs(XmlDocCommentLocation.End))
		{
			info.Kind = SyntaxKind.EndOfDocumentationCommentToken;
			return true;
		}
		char ch;
		char c = (ch = TextWindow.PeekChar());
		if ((uint)c <= 34u)
		{
			if (c == '\n' || c == '\r')
			{
				goto IL_0101;
			}
			if (c != '"' || !ModeIs(LexerMode.XmlAttributeTextDoubleQuote))
			{
				goto IL_0124;
			}
			TextWindow.AdvanceChar();
			info.Kind = SyntaxKind.DoubleQuoteToken;
		}
		else if ((uint)c <= 39u)
		{
			if (c != '&')
			{
				if (c != '\'' || !ModeIs(LexerMode.XmlAttributeTextQuote))
				{
					goto IL_0124;
				}
				TextWindow.AdvanceChar();
				info.Kind = SyntaxKind.SingleQuoteToken;
			}
			else
			{
				ScanXmlEntity(ref info);
				info.Kind = SyntaxKind.XmlEntityLiteralToken;
			}
		}
		else if (c != '<')
		{
			if (c != '\uffff' || !TextWindow.IsReallyAtEnd())
			{
				goto IL_0124;
			}
			info.Kind = SyntaxKind.EndOfDocumentationCommentToken;
		}
		else
		{
			TextWindow.AdvanceChar();
			info.Kind = SyntaxKind.LessThanToken;
		}
		goto IL_013b;
		IL_0124:
		if (SyntaxFacts.IsNewLine(ch))
		{
			goto IL_0101;
		}
		ScanXmlAttributeText(ref info);
		info.Kind = SyntaxKind.XmlTextLiteralToken;
		goto IL_013b;
		IL_013b:
		DebugAssertHelper.Assert(info.Kind != 0 || info.Text != null);
		return info.Kind != SyntaxKind.None;
		IL_0101:
		ScanXmlTextLiteralNewLineToken(ref info);
		goto IL_013b;
	}

	private void ScanXmlAttributeText(ref TokenInfo info)
	{
		while (true)
		{
			char c = TextWindow.PeekChar();
			switch (c)
			{
			case '"':
				if (ModeIs(LexerMode.XmlAttributeTextDoubleQuote))
				{
					info.StringValue = (info.Text = TextWindow.GetText(intern: false));
					return;
				}
				goto default;
			case '\'':
				if (ModeIs(LexerMode.XmlAttributeTextQuote))
				{
					info.StringValue = (info.Text = TextWindow.GetText(intern: false));
					return;
				}
				goto default;
			case '\n':
			case '\r':
			case '&':
			case '<':
				info.StringValue = (info.Text = TextWindow.GetText(intern: false));
				return;
			case '\uffff':
				if (TextWindow.IsReallyAtEnd())
				{
					info.StringValue = (info.Text = TextWindow.GetText(intern: false));
					return;
				}
				goto default;
			case '*':
				if (StyleIs(XmlDocCommentStyle.Delimited) && TextWindow.PeekChar(1) == '/')
				{
					info.StringValue = (info.Text = TextWindow.GetText(intern: false));
					return;
				}
				goto default;
			default:
				if (!SyntaxFacts.IsNewLine(c))
				{
					break;
				}
				goto case '\n';
			}
			TextWindow.AdvanceChar();
		}
	}

	private InternalSyntaxToken LexXmlCharacter()
	{
		TokenInfo info = default(TokenInfo);
		InternalSyntaxListBuilder trivia = null;
		LexXmlDocCommentLeadingTriviaWithWhitespace(ref trivia);
		Start();
		ScanXmlCharacter(ref info);
		SyntaxDiagnosticInfo[] array = GetErrors(GetFullWidth(trivia));
		return Create(ref info, trivia, null, array);
	}

	private bool ScanXmlCharacter(ref TokenInfo info)
	{
		DebugAssertHelper.Assert(!LocationIs(XmlDocCommentLocation.Start));
		DebugAssertHelper.Assert(!LocationIs(XmlDocCommentLocation.Exterior));
		if (LocationIs(XmlDocCommentLocation.End))
		{
			info.Kind = SyntaxKind.EndOfDocumentationCommentToken;
			return true;
		}
		char c = TextWindow.PeekChar();
		if (c != '&')
		{
			if (c == '\uffff' && TextWindow.IsReallyAtEnd())
			{
				info.Kind = SyntaxKind.EndOfFileToken;
			}
			else
			{
				info.Kind = SyntaxKind.XmlTextLiteralToken;
				info.Text = (info.StringValue = TextWindow.NextChar().ToString());
			}
		}
		else
		{
			ScanXmlEntity(ref info);
			info.Kind = SyntaxKind.XmlEntityLiteralToken;
		}
		return true;
	}

	private InternalSyntaxToken LexXmlCrefOrNameToken()
	{
		TokenInfo info = default(TokenInfo);
		InternalSyntaxListBuilder trivia = null;
		LexXmlDocCommentLeadingTriviaWithWhitespace(ref trivia);
		Start();
		ScanXmlCrefToken(ref info);
		SyntaxDiagnosticInfo[] array = GetErrors(GetFullWidth(trivia));
		return Create(ref info, trivia, null, array);
	}

	private bool ScanXmlCrefToken(ref TokenInfo info)
	{
		DebugAssertHelper.Assert(!LocationIs(XmlDocCommentLocation.Start));
		DebugAssertHelper.Assert(!LocationIs(XmlDocCommentLocation.Exterior));
		if (LocationIs(XmlDocCommentLocation.End))
		{
			info.Kind = SyntaxKind.EndOfDocumentationCommentToken;
			return true;
		}
		int position = TextWindow.Position;
		char ch = TextWindow.NextChar();
		char surrogate = '\uffff';
		if ((uint)ch <= 38u)
		{
			if ((uint)ch <= 13u)
			{
				if (ch == '\n' || ch == '\r')
				{
					goto IL_0136;
				}
				goto IL_018f;
			}
			if (ch != '"')
			{
				if (ch != '&')
				{
					goto IL_018f;
				}
				TextWindow.Reset(position);
				if (!TextWindow.TryScanXmlEntity(out ch, out surrogate))
				{
					TextWindow.Reset(position);
					ScanXmlEntity(ref info);
					info.Kind = SyntaxKind.XmlEntityLiteralToken;
					return true;
				}
			}
			else if (ModeIs(LexerMode.XmlNameDoubleQuote))
			{
				info.Kind = SyntaxKind.DoubleQuoteToken;
				return true;
			}
		}
		else if ((uint)ch <= 60u)
		{
			if (ch != '\'')
			{
				if (ch != '<')
				{
					goto IL_018f;
				}
				info.Text = TextWindow.GetText(intern: false);
				AddError(XmlParseErrorCode.XML_LessThanInAttributeValue, info.Text);
				return true;
			}
			if (ModeIs(LexerMode.XmlNameQuote))
			{
				info.Kind = SyntaxKind.SingleQuoteToken;
				return true;
			}
		}
		else if (ch != '{')
		{
			if (ch != '}')
			{
				if (ch != '\uffff' || !TextWindow.IsReallyAtEnd())
				{
					goto IL_018f;
				}
				info.Kind = SyntaxKind.EndOfDocumentationCommentToken;
				return true;
			}
			ch = '>';
		}
		else
		{
			ch = '<';
		}
		goto IL_0197;
		IL_0197:
		DebugAssertHelper.Assert(TextWindow.Position > position, "First character or entity has been consumed.");
		switch (ch)
		{
		case '(':
			info.Kind = SyntaxKind.OpenParenToken;
			break;
		case ')':
			info.Kind = SyntaxKind.CloseParenToken;
			break;
		case '[':
			info.Kind = SyntaxKind.OpenBracketToken;
			break;
		case ']':
			info.Kind = SyntaxKind.CloseBracketToken;
			break;
		case ',':
			info.Kind = SyntaxKind.CommaToken;
			break;
		case '.':
			if (AdvanceIfMatches('.'))
			{
				if (TextWindow.PeekChar() == '.')
				{
					AddCrefError(ErrorCode.ERR_UnexpectedCharacter, ".");
				}
				info.Kind = SyntaxKind.DotDotToken;
			}
			else
			{
				info.Kind = SyntaxKind.DotToken;
			}
			break;
		case '?':
			info.Kind = SyntaxKind.QuestionToken;
			break;
		case '&':
			info.Kind = SyntaxKind.AmpersandToken;
			break;
		case '*':
			info.Kind = SyntaxKind.MultiplyToken;
			break;
		case '|':
			info.Kind = SyntaxKind.BarToken;
			break;
		case '^':
			info.Kind = SyntaxKind.CaretToken;
			break;
		case '%':
			info.Kind = SyntaxKind.PercentToken;
			break;
		case '/':
			info.Kind = SyntaxKind.RDivToken;
			break;
		case '~':
			info.Kind = SyntaxKind.TildeToken;
			break;
		case '{':
			info.Kind = SyntaxKind.LessThanToken;
			break;
		case '}':
			info.Kind = SyntaxKind.GreaterThanToken;
			break;
		case ':':
			if (AdvanceIfMatches(':'))
			{
				info.Kind = SyntaxKind.ColonColonToken;
			}
			else
			{
				info.Kind = SyntaxKind.ColonToken;
			}
			break;
		case '=':
			if (AdvanceIfMatches('='))
			{
				info.Kind = SyntaxKind.EqualsEqualsToken;
			}
			else
			{
				info.Kind = SyntaxKind.EqualsToken;
			}
			break;
		case '!':
			if (AdvanceIfMatches('='))
			{
				info.Kind = SyntaxKind.ExclamationEqualsToken;
			}
			else
			{
				info.Kind = SyntaxKind.ExclamationToken;
			}
			break;
		case '>':
			if (AdvanceIfMatches('='))
			{
				info.Kind = SyntaxKind.GreaterThanEqualsToken;
			}
			else
			{
				info.Kind = SyntaxKind.GreaterThanToken;
			}
			break;
		case '<':
			if (AdvanceIfMatches('='))
			{
				info.Kind = SyntaxKind.LessThanEqualsToken;
			}
			else if (AdvanceIfMatches('<'))
			{
				info.Kind = SyntaxKind.LessThanLessThanToken;
			}
			else
			{
				info.Kind = SyntaxKind.LessThanToken;
			}
			break;
		case '+':
			if (AdvanceIfMatches('+'))
			{
				info.Kind = SyntaxKind.PlusPlusToken;
			}
			else
			{
				info.Kind = SyntaxKind.PlusToken;
			}
			break;
		case '-':
			if (AdvanceIfMatches('-'))
			{
				info.Kind = SyntaxKind.MinusMinusToken;
			}
			else
			{
				info.Kind = SyntaxKind.MinusToken;
			}
			break;
		}
		if (info.Kind != 0)
		{
			DebugAssertHelper.Assert(info.Text == null, "Haven't tried to set it yet.");
			DebugAssertHelper.Assert(info.StringValue == null, "Haven't tried to set it yet.");
			string text = info.Kind.GetText();
			string text2 = TextWindow.GetText(intern: false);
			if (!string.IsNullOrEmpty(text) && text2 != text)
			{
				info.RequiresTextForXmlEntity = true;
				info.Text = text2;
				info.StringValue = text;
			}
		}
		else
		{
			TextWindow.Reset(position);
			if (ScanIdentifier(ref info) && info.Text.Length > 0)
			{
				if (!InXmlNameAttributeValue && cache.TryGetKeywordKind(info.StringValue, mode, out var kind))
				{
					info.Kind = kind;
					info.RequiresTextForXmlEntity = info.Text != info.StringValue;
				}
				else
				{
					info.Kind = SyntaxKind.IdentifierToken;
				}
			}
			else if (TextWindow.PeekChar() == '&')
			{
				ScanXmlEntity(ref info);
				info.Kind = SyntaxKind.XmlEntityLiteralToken;
				AddCrefError(ErrorCode.ERR_UnexpectedCharacter, info.Text);
			}
			else
			{
				char charValue = TextWindow.NextChar();
				info.Text = TextWindow.GetText(intern: false);
				if (MatchesProductionForXmlChar(charValue))
				{
					AddCrefError(ErrorCode.ERR_UnexpectedCharacter, info.Text);
				}
				else
				{
					AddError(XmlParseErrorCode.XML_InvalidUnicodeChar);
				}
			}
		}
		DebugAssertHelper.Assert(info.Kind != 0 || info.Text != null);
		return info.Kind != SyntaxKind.None;
		IL_018f:
		if (SyntaxFacts.IsNewLine(ch))
		{
			goto IL_0136;
		}
		goto IL_0197;
		IL_0136:
		TextWindow.Reset(position);
		ScanXmlTextLiteralNewLineToken(ref info);
		goto IL_0197;
	}

	private bool AdvanceIfMatches(char ch)
	{
		char c = TextWindow.PeekChar();
		if (c == ch || (c == '{' && ch == '<') || (c == '}' && ch == '>'))
		{
			TextWindow.AdvanceChar();
			return true;
		}
		if (c == '&')
		{
			int position = TextWindow.Position;
			if (TextWindow.TryScanXmlEntity(out var ch2, out var surrogate) && ch2 == ch && surrogate == '\uffff')
			{
				return true;
			}
			TextWindow.Reset(position);
		}
		return false;
	}

	private void AddCrefError(ErrorCode code, params object[] args)
	{
		AddCrefError(MakeError(code, args));
	}

	private void AddCrefError(DiagnosticInfo info)
	{
		if (info != null)
		{
			AddError(ErrorCode.WRN_ErrorOverride, info, info.Code);
		}
	}

	private InternalSyntaxToken LexXmlCDataSectionTextToken()
	{
		TokenInfo info = default(TokenInfo);
		InternalSyntaxListBuilder trivia = null;
		LexXmlDocCommentLeadingTrivia(ref trivia);
		Start();
		ScanXmlCDataSectionTextToken(ref info);
		SyntaxDiagnosticInfo[] array = GetErrors(GetFullWidth(trivia));
		return Create(ref info, trivia, null, array);
	}

	private bool ScanXmlCDataSectionTextToken(ref TokenInfo info)
	{
		DebugAssertHelper.Assert(!LocationIs(XmlDocCommentLocation.Start));
		DebugAssertHelper.Assert(!LocationIs(XmlDocCommentLocation.Exterior));
		if (LocationIs(XmlDocCommentLocation.End))
		{
			info.Kind = SyntaxKind.EndOfDocumentationCommentToken;
			return true;
		}
		char ch;
		char c = (ch = TextWindow.PeekChar());
		if ((uint)c <= 13u)
		{
			if (c == '\n' || c == '\r')
			{
				goto IL_00a5;
			}
			goto IL_00c8;
		}
		if (c != ']')
		{
			if (c != '\uffff' || !TextWindow.IsReallyAtEnd())
			{
				goto IL_00c8;
			}
			info.Kind = SyntaxKind.EndOfDocumentationCommentToken;
		}
		else
		{
			if (TextWindow.PeekChar(1) != ']' || TextWindow.PeekChar(2) != '>')
			{
				goto IL_00c8;
			}
			TextWindow.AdvanceChar(3);
			info.Kind = SyntaxKind.XmlCDataEndToken;
		}
		goto IL_00df;
		IL_00df:
		return true;
		IL_00c8:
		if (SyntaxFacts.IsNewLine(ch))
		{
			goto IL_00a5;
		}
		ScanXmlCDataSectionText(ref info);
		info.Kind = SyntaxKind.XmlTextLiteralToken;
		goto IL_00df;
		IL_00a5:
		ScanXmlTextLiteralNewLineToken(ref info);
		goto IL_00df;
	}

	private void ScanXmlCDataSectionText(ref TokenInfo info)
	{
		while (true)
		{
			char c = TextWindow.PeekChar();
			if ((uint)c <= 13u)
			{
				if (c == '\n' || c == '\r')
				{
					break;
				}
			}
			else
			{
				switch (c)
				{
				case ']':
					if (TextWindow.PeekChar(1) == ']' && TextWindow.PeekChar(2) == '>')
					{
						info.StringValue = (info.Text = TextWindow.GetText(intern: false));
						return;
					}
					break;
				case '\uffff':
					if (TextWindow.IsReallyAtEnd())
					{
						info.StringValue = (info.Text = TextWindow.GetText(intern: false));
						return;
					}
					break;
				case '*':
					if (StyleIs(XmlDocCommentStyle.Delimited) && TextWindow.PeekChar(1) == '/')
					{
						info.StringValue = (info.Text = TextWindow.GetText(intern: false));
						return;
					}
					break;
				}
			}
			if (SyntaxFacts.IsNewLine(c))
			{
				break;
			}
			TextWindow.AdvanceChar();
		}
		info.StringValue = (info.Text = TextWindow.GetText(intern: false));
	}

	private InternalSyntaxToken LexXmlCommentTextToken()
	{
		TokenInfo info = default(TokenInfo);
		InternalSyntaxListBuilder trivia = null;
		LexXmlDocCommentLeadingTrivia(ref trivia);
		Start();
		ScanXmlCommentTextToken(ref info);
		SyntaxDiagnosticInfo[] array = GetErrors(GetFullWidth(trivia));
		return Create(ref info, trivia, null, array);
	}

	private bool ScanXmlCommentTextToken(ref TokenInfo info)
	{
		DebugAssertHelper.Assert(!LocationIs(XmlDocCommentLocation.Start));
		DebugAssertHelper.Assert(!LocationIs(XmlDocCommentLocation.Exterior));
		if (LocationIs(XmlDocCommentLocation.End))
		{
			info.Kind = SyntaxKind.EndOfDocumentationCommentToken;
			return true;
		}
		char ch;
		char c = (ch = TextWindow.PeekChar());
		if ((uint)c <= 13u)
		{
			if (c == '\n' || c == '\r')
			{
				goto IL_00be;
			}
			goto IL_00e1;
		}
		if (c != '-')
		{
			if (c != '\uffff' || !TextWindow.IsReallyAtEnd())
			{
				goto IL_00e1;
			}
			info.Kind = SyntaxKind.EndOfDocumentationCommentToken;
		}
		else
		{
			if (TextWindow.PeekChar(1) != '-')
			{
				goto IL_00e1;
			}
			if (TextWindow.PeekChar(2) == '>')
			{
				TextWindow.AdvanceChar(3);
				info.Kind = SyntaxKind.XmlCommentEndToken;
			}
			else
			{
				TextWindow.AdvanceChar(2);
				info.Kind = SyntaxKind.MinusMinusToken;
			}
		}
		goto IL_00f8;
		IL_00f8:
		return true;
		IL_00e1:
		if (SyntaxFacts.IsNewLine(ch))
		{
			goto IL_00be;
		}
		ScanXmlCommentText(ref info);
		info.Kind = SyntaxKind.XmlTextLiteralToken;
		goto IL_00f8;
		IL_00be:
		ScanXmlTextLiteralNewLineToken(ref info);
		goto IL_00f8;
	}

	private void ScanXmlCommentText(ref TokenInfo info)
	{
		while (true)
		{
			char c = TextWindow.PeekChar();
			if ((uint)c <= 13u)
			{
				if (c == '\n' || c == '\r')
				{
					break;
				}
			}
			else
			{
				switch (c)
				{
				case '-':
					if (TextWindow.PeekChar(1) == '-')
					{
						info.StringValue = (info.Text = TextWindow.GetText(intern: false));
						return;
					}
					break;
				case '\uffff':
					if (TextWindow.IsReallyAtEnd())
					{
						info.StringValue = (info.Text = TextWindow.GetText(intern: false));
						return;
					}
					break;
				case '*':
					if (StyleIs(XmlDocCommentStyle.Delimited) && TextWindow.PeekChar(1) == '/')
					{
						info.StringValue = (info.Text = TextWindow.GetText(intern: false));
						return;
					}
					break;
				}
			}
			if (SyntaxFacts.IsNewLine(c))
			{
				break;
			}
			TextWindow.AdvanceChar();
		}
		info.StringValue = (info.Text = TextWindow.GetText(intern: false));
	}

	private InternalSyntaxToken LexXmlProcessingInstructionTextToken()
	{
		TokenInfo info = default(TokenInfo);
		InternalSyntaxListBuilder trivia = null;
		LexXmlDocCommentLeadingTrivia(ref trivia);
		Start();
		ScanXmlProcessingInstructionTextToken(ref info);
		SyntaxDiagnosticInfo[] array = GetErrors(GetFullWidth(trivia));
		return Create(ref info, trivia, null, array);
	}

	private bool ScanXmlProcessingInstructionTextToken(ref TokenInfo info)
	{
		DebugAssertHelper.Assert(!LocationIs(XmlDocCommentLocation.Start));
		DebugAssertHelper.Assert(!LocationIs(XmlDocCommentLocation.Exterior));
		if (LocationIs(XmlDocCommentLocation.End))
		{
			info.Kind = SyntaxKind.EndOfDocumentationCommentToken;
			return true;
		}
		char ch;
		char c = (ch = TextWindow.PeekChar());
		if ((uint)c <= 13u)
		{
			if (c == '\n' || c == '\r')
			{
				goto IL_0095;
			}
			goto IL_00b8;
		}
		if (c != '?')
		{
			if (c != '\uffff' || !TextWindow.IsReallyAtEnd())
			{
				goto IL_00b8;
			}
			info.Kind = SyntaxKind.EndOfDocumentationCommentToken;
		}
		else
		{
			if (TextWindow.PeekChar(1) != '>')
			{
				goto IL_00b8;
			}
			TextWindow.AdvanceChar(2);
			info.Kind = SyntaxKind.XmlProcessingInstructionEndToken;
		}
		goto IL_00cf;
		IL_0095:
		ScanXmlTextLiteralNewLineToken(ref info);
		goto IL_00cf;
		IL_00cf:
		return true;
		IL_00b8:
		if (SyntaxFacts.IsNewLine(ch))
		{
			goto IL_0095;
		}
		ScanXmlProcessingInstructionText(ref info);
		info.Kind = SyntaxKind.XmlTextLiteralToken;
		goto IL_00cf;
	}

	private void ScanXmlProcessingInstructionText(ref TokenInfo info)
	{
		while (true)
		{
			char c = TextWindow.PeekChar();
			if ((uint)c <= 13u)
			{
				if (c == '\n' || c == '\r')
				{
					break;
				}
			}
			else
			{
				switch (c)
				{
				case '?':
					if (TextWindow.PeekChar(1) == '>')
					{
						info.StringValue = (info.Text = TextWindow.GetText(intern: false));
						return;
					}
					break;
				case '\uffff':
					if (TextWindow.IsReallyAtEnd())
					{
						info.StringValue = (info.Text = TextWindow.GetText(intern: false));
						return;
					}
					break;
				case '*':
					if (StyleIs(XmlDocCommentStyle.Delimited) && TextWindow.PeekChar(1) == '/')
					{
						info.StringValue = (info.Text = TextWindow.GetText(intern: false));
						return;
					}
					break;
				}
			}
			if (SyntaxFacts.IsNewLine(c))
			{
				break;
			}
			TextWindow.AdvanceChar();
		}
		info.StringValue = (info.Text = TextWindow.GetText(intern: false));
	}

	private void LexXmlDocCommentLeadingTrivia(ref InternalSyntaxListBuilder trivia)
	{
		int position = TextWindow.Position;
		Start();
		if (LocationIs(XmlDocCommentLocation.Start) && StyleIs(XmlDocCommentStyle.Delimited))
		{
			if (TextWindow.PeekChar() == '/' && TextWindow.PeekChar(1) == '*' && TextWindow.PeekChar(2) == '*' && TextWindow.PeekChar(3) != '*')
			{
				TextWindow.AdvanceChar(3);
				string text = TextWindow.GetText(intern: true);
				AddTrivia(InternalSyntaxFactory.DocumentationCommentExteriorTrivia(text), ref trivia);
				MutateLocation(XmlDocCommentLocation.Interior);
			}
		}
		else if (LocationIs(XmlDocCommentLocation.Start) || LocationIs(XmlDocCommentLocation.Exterior))
		{
			while (true)
			{
				char c = TextWindow.PeekChar();
				switch (c)
				{
				case '\t':
				case '\v':
				case '\f':
				case ' ':
					goto IL_00fb;
				case '/':
					if (StyleIs(XmlDocCommentStyle.SingleLine) && TextWindow.PeekChar(1) == '/' && TextWindow.PeekChar(2) == '/' && TextWindow.PeekChar(3) != '/')
					{
						TextWindow.AdvanceChar(3);
						string text3 = TextWindow.GetText(intern: true);
						AddTrivia(InternalSyntaxFactory.DocumentationCommentExteriorTrivia(text3), ref trivia);
						MutateLocation(XmlDocCommentLocation.Interior);
						return;
					}
					break;
				case '*':
					if (StyleIs(XmlDocCommentStyle.Delimited))
					{
						while (TextWindow.PeekChar() == '*' && TextWindow.PeekChar(1) != '/')
						{
							TextWindow.AdvanceChar();
						}
						string text2 = TextWindow.GetText(intern: true);
						if (!string.IsNullOrEmpty(text2))
						{
							AddTrivia(InternalSyntaxFactory.DocumentationCommentExteriorTrivia(text2), ref trivia);
						}
						if (TextWindow.PeekChar() == '*' && TextWindow.PeekChar(1) == '/')
						{
							TextWindow.AdvanceChar(2);
							AddTrivia(InternalSyntaxFactory.DocumentationCommentExteriorTrivia("*/"), ref trivia);
							MutateLocation(XmlDocCommentLocation.End);
						}
						else
						{
							MutateLocation(XmlDocCommentLocation.Interior);
						}
						return;
					}
					break;
				}
				if (!SyntaxFacts.IsWhitespace(c))
				{
					break;
				}
				goto IL_00fb;
				IL_00fb:
				TextWindow.AdvanceChar();
			}
			if (StyleIs(XmlDocCommentStyle.SingleLine))
			{
				TextWindow.Reset(position);
				MutateLocation(XmlDocCommentLocation.End);
				return;
			}
			DebugAssertHelper.Assert(StyleIs(XmlDocCommentStyle.Delimited));
			string text4 = TextWindow.GetText(intern: true);
			if (!string.IsNullOrEmpty(text4))
			{
				AddTrivia(InternalSyntaxFactory.DocumentationCommentExteriorTrivia(text4), ref trivia);
			}
			MutateLocation(XmlDocCommentLocation.Interior);
		}
		else if (!LocationIs(XmlDocCommentLocation.End) && StyleIs(XmlDocCommentStyle.Delimited) && TextWindow.PeekChar() == '*' && TextWindow.PeekChar(1) == '/')
		{
			TextWindow.AdvanceChar(2);
			string text5 = TextWindow.GetText(intern: true);
			AddTrivia(InternalSyntaxFactory.DocumentationCommentExteriorTrivia(text5), ref trivia);
			MutateLocation(XmlDocCommentLocation.End);
		}
	}

	private void LexXmlDocCommentLeadingTriviaWithWhitespace(ref InternalSyntaxListBuilder trivia)
	{
		while (true)
		{
			LexXmlDocCommentLeadingTrivia(ref trivia);
			char ch = TextWindow.PeekChar();
			if (LocationIs(XmlDocCommentLocation.Interior) && (SyntaxFacts.IsWhitespace(ch) || SyntaxFacts.IsNewLine(ch)))
			{
				LexXmlWhitespaceAndNewLineTrivia(ref trivia);
				continue;
			}
			break;
		}
	}

	private void LexXmlWhitespaceAndNewLineTrivia(ref InternalSyntaxListBuilder trivia)
	{
		Start();
		if (!LocationIs(XmlDocCommentLocation.Interior))
		{
			return;
		}
		char c = TextWindow.PeekChar();
		switch (c)
		{
		case '\t':
		case '\v':
		case '\f':
		case ' ':
			AddTrivia(ScanWhitespace(), ref trivia);
			break;
		case '\n':
		case '\r':
			AddTrivia(ScanEndOfLine(), ref trivia);
			MutateLocation(XmlDocCommentLocation.Exterior);
			break;
		case '*':
			if (StyleIs(XmlDocCommentStyle.Delimited) && TextWindow.PeekChar(1) == '/')
			{
				break;
			}
			goto default;
		default:
			if (SyntaxFacts.IsWhitespace(c))
			{
				goto case '\t';
			}
			if (!SyntaxFacts.IsNewLine(c))
			{
				break;
			}
			goto case '\n';
		}
	}

	protected SyntaxDiagnosticInfo[] GetErrors(int leadingTriviaWidth)
	{
		if (errors != null)
		{
			if (leadingTriviaWidth > 0)
			{
				SyntaxDiagnosticInfo[] array = new SyntaxDiagnosticInfo[errors.Count];
				for (int i = 0; i < errors.Count; i++)
				{
					array[i] = errors[i].WithOffset(errors[i].Offset + leadingTriviaWidth);
				}
				return array;
			}
			return errors.ToArray();
		}
		return null;
	}

	protected void AddError(int position, int width, ErrorCode code)
	{
		AddError(MakeError(position, width, code));
	}

	protected void AddError(int position, int width, ErrorCode code, params object[] args)
	{
		AddError(MakeError(position, width, code, args));
	}

	protected void AddError(ErrorCode code)
	{
		AddError(MakeError(code));
	}

	protected void AddError(ErrorCode code, params object[] args)
	{
		AddError(MakeError(code, args));
	}

	protected void AddError(XmlParseErrorCode code)
	{
		AddError(MakeError(code));
	}

	protected void AddError(XmlParseErrorCode code, params object[] args)
	{
		AddError(MakeError(code, args));
	}

	protected void AddError(SyntaxDiagnosticInfo error)
	{
		if (error != null)
		{
			if (errors == null)
			{
				errors = new List<SyntaxDiagnosticInfo>(8);
			}
			errors.Add(error);
		}
	}

	protected SyntaxDiagnosticInfo MakeError(int position, int width, ErrorCode code)
	{
		return new SyntaxDiagnosticInfo(GetLexemeOffsetFromPosition(position), width, code);
	}

	protected SyntaxDiagnosticInfo MakeError(int position, int width, ErrorCode code, params object[] args)
	{
		return new SyntaxDiagnosticInfo(GetLexemeOffsetFromPosition(position), width, code, args);
	}

	protected static XmlSyntaxDiagnosticInfo MakeError(XmlParseErrorCode code)
	{
		return new XmlSyntaxDiagnosticInfo(0, 0, code);
	}

	protected static XmlSyntaxDiagnosticInfo MakeError(XmlParseErrorCode code, params object[] args)
	{
		return new XmlSyntaxDiagnosticInfo(0, 0, code, args);
	}

	private int GetLexemeOffsetFromPosition(int position)
	{
		if (position < TextWindow.LexemeStartPosition)
		{
			return position;
		}
		return position - TextWindow.LexemeStartPosition;
	}

	protected static SyntaxDiagnosticInfo MakeError(ErrorCode code)
	{
		return new SyntaxDiagnosticInfo(code);
	}

	protected static SyntaxDiagnosticInfo MakeError(ErrorCode code, params object[] args)
	{
		return new SyntaxDiagnosticInfo(code, args);
	}

	private void AddTrivia(InternalSyntaxNode trivia, ref InternalSyntaxListBuilder list)
	{
		if (HasErrors)
		{
			DiagnosticInfo[] array = GetErrors(0);
			DiagnosticInfo[] diagnostics = array;
			trivia = trivia.WithInternalDiagnostics(diagnostics);
		}
		if (list == null)
		{
			list = new InternalSyntaxListBuilder(8);
		}
		if (list == null)
		{
			list = new InternalSyntaxListBuilder(8);
		}
		list.Add(trivia);
	}

	private InternalSyntaxToken QuickScanSyntaxToken()
	{
		Start();
		QuickScanState quickScanState = QuickScanState.Initial;
		int num = TextWindow.Offset;
		int characterWindowCount = TextWindow.CharacterWindowCount;
		characterWindowCount = Math.Min(characterWindowCount, num + 42);
		int num2 = -2128831035;
		num2 = (int)(((uint)num2 ^ (uint)mode) * 16777619);
		char[] characterWindow = TextWindow.CharacterWindow;
		int num3 = CharProperties.Length;
		while (true)
		{
			if (num < characterWindowCount)
			{
				int num4 = characterWindow[num];
				CharFlags charFlags = (CharFlags)((num4 < num3) ? CharProperties[num4] : 10);
				quickScanState = (QuickScanState)StateTransitions[(uint)quickScanState, (uint)charFlags];
				if ((int)quickScanState >= 11)
				{
					break;
				}
				num2 = (num2 ^ num4) * 16777619;
				num++;
				continue;
			}
			quickScanState = QuickScanState.Bad;
			break;
		}
		TextWindow.AdvanceChar(num - TextWindow.Offset);
		DebugAssertHelper.Assert(quickScanState == QuickScanState.Bad || quickScanState == QuickScanState.Done, "can only exit with Bad or Done");
		if (quickScanState == QuickScanState.Done)
		{
			return cache.LookupToken(TextWindow.CharacterWindow, TextWindow.LexemeRelativeStart, num - TextWindow.LexemeRelativeStart, num2, CreateQuickToken);
		}
		TextWindow.Reset(TextWindow.LexemeStartPosition);
		return null;
	}

	private InternalSyntaxToken CreateQuickToken()
	{
		TextWindow.Reset(TextWindow.LexemeStartPosition);
		return LexSyntaxToken();
	}
}
