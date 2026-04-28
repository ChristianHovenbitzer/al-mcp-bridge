using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Log;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Options;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Formatting;

internal class TokenStream
{
	private struct Changes
	{
		public const int BeginningOfTreeKey = -1;

		public const int EndOfTreeKey = -2;

		private ConcurrentDictionary<int, TriviaData> map;

		public bool TryRemove(int pairIndex)
		{
			TriviaData value;
			return map?.TryRemove(pairIndex, out value) ?? false;
		}

		public void AddOrReplace(int key, TriviaData triviaInfo)
		{
			LazyInitialization.EnsureInitialized(ref map, () => new ConcurrentDictionary<int, TriviaData>(1, 8))[key] = triviaInfo;
		}

		public bool TryGet(int key, out TriviaData triviaInfo)
		{
			triviaInfo = null;
			return map?.TryGetValue(key, out triviaInfo) ?? false;
		}
	}

	private class Iterator : IEnumerable<(int, SyntaxToken, SyntaxToken)>, IEnumerable
	{
		private struct Enumerator : IEnumerator<(int, SyntaxToken, SyntaxToken)>, IEnumerator, IDisposable
		{
			private readonly List<SyntaxToken> tokensIncludingZeroWidth;

			private readonly int maxCount;

			private int index;

			public (int, SyntaxToken, SyntaxToken) Current { get; private set; }

			object IEnumerator.Current
			{
				get
				{
					if (index == 0 || index == maxCount + 1)
					{
						throw new InvalidOperationException();
					}
					return Current;
				}
			}

			public Enumerator(List<SyntaxToken> tokensIncludingZeroWidth)
			{
				this.tokensIncludingZeroWidth = tokensIncludingZeroWidth;
				maxCount = this.tokensIncludingZeroWidth.Count - 1;
				index = 0;
				Current = default((int, SyntaxToken, SyntaxToken));
			}

			public void Dispose()
			{
			}

			public bool MoveNext()
			{
				if (index < maxCount)
				{
					Current = ValueTuple.Create(index, tokensIncludingZeroWidth[index], tokensIncludingZeroWidth[index + 1]);
					index++;
					return true;
				}
				return MoveNextRare();
			}

			private bool MoveNextRare()
			{
				index = maxCount + 1;
				Current = default((int, SyntaxToken, SyntaxToken));
				return false;
			}

			void IEnumerator.Reset()
			{
				index = 0;
				Current = default((int, SyntaxToken, SyntaxToken));
			}
		}

		private readonly List<SyntaxToken> tokensIncludingZeroWidth;

		public Iterator(List<SyntaxToken> tokensIncludingZeroWidth)
		{
			this.tokensIncludingZeroWidth = tokensIncludingZeroWidth;
		}

		public IEnumerator<(int, SyntaxToken, SyntaxToken)> GetEnumerator()
		{
			return new Enumerator(tokensIncludingZeroWidth);
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}

	private const int MagicTextLengthToTokensRatio = 10;

	private readonly TriviaData[] cachedOriginalTriviaInfo;

	private readonly AbstractTriviaDataFactory factory;

	private readonly Func<TokenData, TokenData, TriviaData> getOriginalTriviaData;

	private readonly Func<TokenData, TokenData, TriviaData> getTriviaData;

	private readonly OptionSet optionSet;

	private readonly List<SyntaxToken> tokens;

	private readonly Dictionary<SyntaxToken, int> tokenToIndexMap;

	private readonly TreeData treeData;

	private Changes changes;

	public bool FormatBeginningOfTree => treeData.IsFirstToken(FirstTokenInStream.Token);

	public bool FormatEndOfTree => treeData.IsLastToken(LastTokenInStream.Token);

	public bool IsFormattingWholeDocument
	{
		get
		{
			if (FormatBeginningOfTree)
			{
				return FormatEndOfTree;
			}
			return false;
		}
	}

	public TokenData FirstTokenInStream => new TokenData(this, 0, tokens[0]);

	public TokenData LastTokenInStream => new TokenData(this, TokenCount - 1, tokens[TokenCount - 1]);

	public int TokenCount => tokens.Count;

	public IEnumerable<(int, SyntaxToken, SyntaxToken)> TokenIterator => new Iterator(tokens);

	public TokenStream(TreeData treeData, OptionSet optionSet, TextSpan spanToFormat, AbstractTriviaDataFactory factory)
	{
		using (Logger.LogBlock(FunctionId.Formatting_TokenStreamConstruction, CancellationToken.None))
		{
			this.factory = factory;
			this.treeData = treeData;
			this.optionSet = optionSet;
			int capacity = spanToFormat.Length / 10;
			tokens = new List<SyntaxToken>(capacity);
			tokens.AddRange(this.treeData.GetApplicableTokens(spanToFormat));
			cachedOriginalTriviaInfo = new TriviaData[TokenCount - 1];
			tokenToIndexMap = new Dictionary<SyntaxToken, int>(TokenCount);
			for (int i = 0; i < TokenCount; i++)
			{
				tokenToIndexMap.Add(tokens[i], i);
			}
			getTriviaData = GetTriviaData;
			getOriginalTriviaData = GetOriginalTriviaData;
		}
	}

	public SyntaxToken GetToken(int index)
	{
		Contract.ThrowIfFalse(0 <= index && index < TokenCount);
		return tokens[index];
	}

	public TokenData GetTokenData(SyntaxToken token)
	{
		int tokenIndexInStream = GetTokenIndexInStream(token);
		return new TokenData(this, tokenIndexInStream, token);
	}

	public TokenData GetPreviousTokenData(TokenData tokenData)
	{
		if (tokenData.IndexInStream > 0 && tokenData.IndexInStream < TokenCount)
		{
			return new TokenData(this, tokenData.IndexInStream - 1, tokens[tokenData.IndexInStream - 1]);
		}
		SyntaxToken previousToken = tokenData.Token.GetPreviousToken(includeZeroWidth: true);
		int num = TokenCount - 1;
		if (tokens[num].Equals(previousToken))
		{
			return new TokenData(this, num, tokens[num]);
		}
		return new TokenData(this, -1, previousToken);
	}

	public TokenData GetNextTokenData(TokenData tokenData)
	{
		if (tokenData.IndexInStream >= 0 && tokenData.IndexInStream < TokenCount - 1)
		{
			return new TokenData(this, tokenData.IndexInStream + 1, tokens[tokenData.IndexInStream + 1]);
		}
		SyntaxToken nextToken = tokenData.Token.GetNextToken(includeZeroWidth: true);
		if (tokens[0].Equals(nextToken))
		{
			return new TokenData(this, 0, tokens[0]);
		}
		return new TokenData(this, -1, nextToken);
	}

	public bool TwoTokensOriginallyOnSameLine(SyntaxToken token1, SyntaxToken token2)
	{
		return TwoTokensOnSameLineWorker(token1, token2, getOriginalTriviaData);
	}

	public bool TwoTokensOnSameLine(SyntaxToken token1, SyntaxToken token2)
	{
		return TwoTokensOnSameLineWorker(token1, token2, getTriviaData);
	}

	public void ApplyBeginningOfTreeChange(TriviaData data)
	{
		Contract.ThrowIfNull(data);
		changes.AddOrReplace(-1, data);
	}

	public void ApplyEndOfTreeChange(TriviaData data)
	{
		Contract.ThrowIfNull(data);
		changes.AddOrReplace(-2, data);
	}

	public void ApplyChange(int pairIndex, TriviaData data)
	{
		Contract.ThrowIfNull(data);
		Contract.ThrowIfFalse(0 <= pairIndex && pairIndex < TokenCount - 1);
		if (GetOriginalTriviaData(pairIndex) == data)
		{
			changes.TryRemove(pairIndex);
		}
		else
		{
			changes.AddOrReplace(pairIndex, data);
		}
	}

	public int GetCurrentColumn(SyntaxToken token)
	{
		TokenData tokenData = GetTokenData(token);
		return GetCurrentColumn(tokenData);
	}

	public int GetCurrentColumn(TokenData tokenData)
	{
		return GetColumn(tokenData, getTriviaData);
	}

	public int GetOriginalColumn(SyntaxToken token)
	{
		TokenData tokenData = GetTokenData(token);
		return GetColumn(tokenData, getOriginalTriviaData);
	}

	public void GetTokenLength(SyntaxToken token, out int length, out bool onMultipleLines)
	{
		string text = token.ToString();
		if (text.ContainsLineBreak())
		{
			onMultipleLines = true;
			length = text.GetTextColumn(optionSet.GetOption(FormattingOptions.TabSize, "AL"), 0);
			return;
		}
		onMultipleLines = false;
		if (text.ContainsTab())
		{
			int originalColumn = treeData.GetOriginalColumn(optionSet.GetOption(FormattingOptions.TabSize, "AL"), token);
			length = text.ConvertTabToSpace(optionSet.GetOption(FormattingOptions.TabSize, "AL"), originalColumn, text.Length);
		}
		else
		{
			length = text.Length;
		}
	}

	public IEnumerable<((SyntaxToken, SyntaxToken), TriviaData)> GetTriviaDataWithTokenPair(CancellationToken cancellationToken)
	{
		if (FormatBeginningOfTree)
		{
			SyntaxToken token = FirstTokenInStream.Token;
			TriviaData triviaDataAtBeginningOfTree = GetTriviaDataAtBeginningOfTree();
			yield return ValueTuple.Create(ValueTuple.Create(default(SyntaxToken), token), triviaDataAtBeginningOfTree);
		}
		for (int pairIndex = 0; pairIndex < TokenCount - 1; pairIndex++)
		{
			cancellationToken.ThrowIfCancellationRequested();
			TriviaData triviaData = GetTriviaData(pairIndex);
			yield return ValueTuple.Create(ValueTuple.Create(tokens[pairIndex], tokens[pairIndex + 1]), triviaData);
		}
		if (FormatEndOfTree)
		{
			SyntaxToken token2 = LastTokenInStream.Token;
			TriviaData triviaDataAtEndOfTree = GetTriviaDataAtEndOfTree();
			yield return ValueTuple.Create(ValueTuple.Create(token2, default(SyntaxToken)), triviaDataAtEndOfTree);
		}
	}

	public TriviaData GetTriviaData(TokenData token1, TokenData token2)
	{
		if (treeData.IsFirstToken(token2.Token))
		{
			if (!FormatBeginningOfTree)
			{
				return GetOriginalTriviaData(token1, token2);
			}
			return GetTriviaDataAtBeginningOfTree();
		}
		if (treeData.IsLastToken(token1.Token))
		{
			if (!FormatEndOfTree)
			{
				return GetOriginalTriviaData(token1, token2);
			}
			return GetTriviaDataAtEndOfTree();
		}
		if (token1.IndexInStream < 0 || token2.IndexInStream < 0)
		{
			return GetOriginalTriviaData(token1, token2);
		}
		return GetTriviaData(token1.IndexInStream);
	}

	public TriviaData GetTriviaDataAtBeginningOfTree()
	{
		Contract.ThrowIfFalse(FormatBeginningOfTree);
		if (changes.TryGet(-1, out TriviaData triviaInfo))
		{
			return triviaInfo;
		}
		return GetOriginalTriviaData(default(TokenData), FirstTokenInStream);
	}

	public TriviaData GetTriviaDataAtEndOfTree()
	{
		Contract.ThrowIfFalse(FormatEndOfTree);
		if (changes.TryGet(-2, out TriviaData triviaInfo))
		{
			return triviaInfo;
		}
		return GetOriginalTriviaData(LastTokenInStream, default(TokenData));
	}

	public TriviaData GetTriviaData(int pairIndex)
	{
		Contract.ThrowIfFalse(0 <= pairIndex && pairIndex < TokenCount - 1);
		if (changes.TryGet(pairIndex, out TriviaData triviaInfo))
		{
			return triviaInfo;
		}
		return GetOriginalTriviaData(pairIndex);
	}

	public bool IsFirstTokenOnLine(SyntaxToken token)
	{
		Contract.ThrowIfTrue(token.Kind == SyntaxKind.None);
		TokenData tokenData = GetTokenData(token);
		TokenData previousTokenData = tokenData.GetPreviousTokenData();
		return IsFirstTokenOnLine(previousTokenData, tokenData);
	}

	internal SyntaxToken FirstTokenOfBaseTokenLine(SyntaxToken token)
	{
		TokenData tokenData = GetTokenData(token);
		while (!IsFirstTokenOnLine(token))
		{
			TokenData previousTokenData = GetPreviousTokenData(tokenData);
			token = previousTokenData.Token;
			tokenData = previousTokenData;
		}
		return token;
	}

	[Conditional("DEBUG")]
	private void DebugCheckTokenOrder()
	{
		_ = tokens[0];
		for (int i = 1; i < tokens.Count; i++)
		{
			_ = tokens[i];
		}
	}

	private bool TwoTokensOnSameLineWorker(SyntaxToken token1, SyntaxToken token2, Func<TokenData, TokenData, TriviaData> triviaDataGetter)
	{
		if (token1 == token2)
		{
			return true;
		}
		if (token1.Span.End > token2.SpanStart)
		{
			return false;
		}
		TokenData tokenData = GetTokenData(token1);
		TokenData tokenData2 = GetTokenData(token2);
		TokenData arg = tokenData;
		TokenData nextTokenData = tokenData.GetNextTokenData();
		while (nextTokenData < tokenData2)
		{
			if (triviaDataGetter(arg, nextTokenData).SecondTokenIsFirstTokenOnLine)
			{
				return false;
			}
			arg = nextTokenData;
			nextTokenData = nextTokenData.GetNextTokenData();
		}
		return !triviaDataGetter(arg, tokenData2).SecondTokenIsFirstTokenOnLine;
	}

	private int GetColumn(TokenData tokenData, Func<TokenData, TokenData, TriviaData> triviaDataGetter)
	{
		TokenData previousTokenData = tokenData.GetPreviousTokenData();
		int num = 0;
		while (previousTokenData.Token.Kind != 0)
		{
			TriviaData triviaData = triviaDataGetter(previousTokenData, tokenData);
			if (triviaData.SecondTokenIsFirstTokenOnLine)
			{
				return num + triviaData.Spaces;
			}
			num += triviaData.Spaces;
			GetTokenLength(previousTokenData.Token, out var length, out var onMultipleLines);
			if (onMultipleLines)
			{
				return num + length;
			}
			num += length;
			tokenData = previousTokenData;
			previousTokenData = previousTokenData.GetPreviousTokenData();
		}
		return num + triviaDataGetter(previousTokenData, tokenData).Spaces;
	}

	private TriviaData GetOriginalTriviaData(TokenData token1, TokenData token2)
	{
		if (treeData.IsFirstToken(token2.Token))
		{
			return factory.CreateLeadingTrivia(token2.Token);
		}
		if (treeData.IsLastToken(token1.Token))
		{
			return factory.CreateTrailingTrivia(token1.Token);
		}
		if (token1.IndexInStream < 0 || token2.IndexInStream < 0)
		{
			return factory.Create(token1.Token, token2.Token);
		}
		return GetOriginalTriviaData(token1.IndexInStream);
	}

	private TriviaData GetOriginalTriviaData(int pairIndex)
	{
		Contract.ThrowIfFalse(0 <= pairIndex && pairIndex < TokenCount - 1);
		if (cachedOriginalTriviaInfo[pairIndex] == null)
		{
			TriviaData triviaData = factory.Create(tokens[pairIndex], tokens[pairIndex + 1]);
			cachedOriginalTriviaInfo[pairIndex] = triviaData;
		}
		return cachedOriginalTriviaInfo[pairIndex];
	}

	private bool IsFirstTokenOnLine(TokenData tokenData1, TokenData tokenData2)
	{
		if (tokenData1.Token.Kind == SyntaxKind.None)
		{
			return true;
		}
		return GetTriviaData(tokenData1, tokenData2).SecondTokenIsFirstTokenOnLine;
	}

	private int GetTokenIndexInStream(SyntaxToken token)
	{
		if (tokenToIndexMap.TryGetValue(token, out var value))
		{
			return value;
		}
		return -1;
	}
}
