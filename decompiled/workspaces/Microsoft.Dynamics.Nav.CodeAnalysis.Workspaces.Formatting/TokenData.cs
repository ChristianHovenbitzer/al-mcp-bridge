using System;
using System.Collections.Generic;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Formatting;

internal struct TokenData : IEqualityComparer<TokenData>, IEquatable<TokenData>, IComparable<TokenData>, IComparer<TokenData>
{
	public TokenStream TokenStream { get; }

	public int IndexInStream { get; }

	public SyntaxToken Token { get; }

	public TokenData(TokenStream tokenStream, int indexInStream, SyntaxToken token)
	{
		this = default(TokenData);
		Contract.ThrowIfNull(tokenStream);
		Contract.ThrowIfFalse(indexInStream == -1 || (0 <= indexInStream && indexInStream < tokenStream.TokenCount));
		TokenStream = tokenStream;
		IndexInStream = indexInStream;
		Token = token;
	}

	public TokenData GetPreviousTokenData()
	{
		return TokenStream.GetPreviousTokenData(this);
	}

	public TokenData GetNextTokenData()
	{
		return TokenStream.GetNextTokenData(this);
	}

	public bool Equals(TokenData x, TokenData y)
	{
		return x.Equals(y);
	}

	public int GetHashCode(TokenData obj)
	{
		return obj.GetHashCode();
	}

	public override int GetHashCode()
	{
		return Token.GetHashCode();
	}

	public override bool Equals(object obj)
	{
		if (obj is TokenData other)
		{
			return Equals(other);
		}
		return false;
	}

	public bool Equals(TokenData other)
	{
		if (TokenStream != other.TokenStream)
		{
			return false;
		}
		if (IndexInStream >= 0 && IndexInStream == other.IndexInStream)
		{
			return true;
		}
		return Token.Equals(other.Token);
	}

	public int Compare(TokenData x, TokenData y)
	{
		return x.CompareTo(y);
	}

	public int CompareTo(TokenData other)
	{
		Contract.ThrowIfFalse(TokenStream == other.TokenStream);
		if (IndexInStream >= 0 && other.IndexInStream >= 0)
		{
			return IndexInStream - other.IndexInStream;
		}
		int num = Token.SpanStart - other.Token.SpanStart;
		if (num != 0)
		{
			return num;
		}
		int num2 = Token.Span.End - other.Token.Span.End;
		if (num2 != 0)
		{
			return num2;
		}
		IEnumerable<SyntaxToken> tokens = Token.GetCommonRoot(other.Token).DescendantTokens();
		int num3 = Index(tokens, Token);
		int num4 = Index(tokens, other.Token);
		Contract.ThrowIfFalse(num3 >= 0 && num4 >= 0);
		return num3 - num4;
	}

	private int Index(IEnumerable<SyntaxToken> tokens, SyntaxToken token)
	{
		int num = 0;
		foreach (SyntaxToken token2 in tokens)
		{
			if (token2 == token)
			{
				return num;
			}
			num++;
		}
		return -1;
	}

	public static bool operator <(TokenData left, TokenData right)
	{
		return left.CompareTo(right) < 0;
	}

	public static bool operator >(TokenData left, TokenData right)
	{
		return left.CompareTo(right) > 0;
	}

	public static bool operator ==(TokenData left, TokenData right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(TokenData left, TokenData right)
	{
		return left.Equals(right);
	}
}
