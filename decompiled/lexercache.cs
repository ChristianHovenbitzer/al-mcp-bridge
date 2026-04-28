using System;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

internal class LexerCache
{
	private static readonly ObjectPool<CachingIdentityFactory<string, SyntaxKind>> ALKeywordKindPool = CachingIdentityFactory<string, SyntaxKind>.CreatePool(512, (string key) => SyntaxFacts.GetALKeywordKind(key.ToUpperInvariant()));

	private static readonly ObjectPool<CachingIdentityFactory<string, SyntaxKind>> ObjectKeywordKindPool = CachingIdentityFactory<string, SyntaxKind>.CreatePool(512, (string key) => SyntaxFacts.GetObjectKeywordKind(key.ToUpperInvariant()));

	private static readonly ObjectPool<CachingIdentityFactory<string, SyntaxKind>> PropertyKeywordKindPool = CachingIdentityFactory<string, SyntaxKind>.CreatePool(512, (string key) => SyntaxFacts.GetPropertyKeywordKind(key.ToUpperInvariant()));

	private static readonly ObjectPool<CachingIdentityFactory<string, SyntaxKind>> DirectiveKeywordKindPool = CachingIdentityFactory<string, SyntaxKind>.CreatePool(512, (string key) => SyntaxFacts.GetDirectiveKeywordKind(key.ToUpperInvariant()));

	private readonly TextKeyedCache<InternalSyntaxTrivia> triviaMap;

	private readonly TextKeyedCache<InternalSyntaxToken> tokenMap;

	private readonly CachingIdentityFactory<string, SyntaxKind> alKeywordKindMap;

	private readonly CachingIdentityFactory<string, SyntaxKind> objectKeywordKindMap;

	private readonly CachingIdentityFactory<string, SyntaxKind> propertyKeywordKindMap;

	private readonly CachingIdentityFactory<string, SyntaxKind> directiveKeywordKindMap;

	internal const int MaxKeywordLength = 22;

	internal LexerCache()
	{
		triviaMap = TextKeyedCache<InternalSyntaxTrivia>.GetInstance();
		tokenMap = TextKeyedCache<InternalSyntaxToken>.GetInstance();
		alKeywordKindMap = ALKeywordKindPool.Allocate();
		objectKeywordKindMap = ObjectKeywordKindPool.Allocate();
		propertyKeywordKindMap = PropertyKeywordKindPool.Allocate();
		directiveKeywordKindMap = DirectiveKeywordKindPool.Allocate();
	}

	internal void Free()
	{
		alKeywordKindMap.Free();
		objectKeywordKindMap.Free();
		propertyKeywordKindMap.Free();
		directiveKeywordKindMap.Free();
		triviaMap.Free();
		tokenMap.Free();
	}

	internal bool TryGetKeywordKind(string key, LexerMode mode, out SyntaxKind kind)
	{
		if (key.Length > 22)
		{
			kind = SyntaxKind.None;
			return false;
		}
		switch (mode)
		{
		default:
			throw new InvalidOperationException("Unrecognized lexer mode");
		case LexerMode.Code:
		case LexerMode.Expression:
			kind = alKeywordKindMap.GetOrMakeValue(key);
			break;
		case LexerMode.Object:
			kind = objectKeywordKindMap.GetOrMakeValue(key);
			break;
		case LexerMode.Property:
			kind = propertyKeywordKindMap.GetOrMakeValue(key);
			break;
		case LexerMode.Directive:
			kind = directiveKeywordKindMap.GetOrMakeValue(key);
			break;
		}
		return kind != SyntaxKind.None;
	}

	internal InternalSyntaxTrivia LookupTrivia(char[] textBuffer, int keyStart, int keyLength, int hashCode, Func<InternalSyntaxTrivia> createTriviaFunction)
	{
		InternalSyntaxTrivia internalSyntaxTrivia = triviaMap.FindItem(textBuffer, keyStart, keyLength, hashCode);
		if (internalSyntaxTrivia == null)
		{
			internalSyntaxTrivia = createTriviaFunction();
			triviaMap.AddItem(textBuffer, keyStart, keyLength, hashCode, internalSyntaxTrivia);
		}
		return internalSyntaxTrivia;
	}

	internal InternalSyntaxToken LookupToken(char[] textBuffer, int keyStart, int keyLength, int hashCode, Func<InternalSyntaxToken> createTokenFunction)
	{
		InternalSyntaxToken internalSyntaxToken = tokenMap.FindItem(textBuffer, keyStart, keyLength, hashCode);
		if (internalSyntaxToken == null)
		{
			internalSyntaxToken = createTokenFunction();
			tokenMap.AddItem(textBuffer, keyStart, keyLength, hashCode, internalSyntaxToken);
		}
		return internalSyntaxToken;
	}
}
