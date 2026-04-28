using System;
using System.Collections.Immutable;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.LanguageServices;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.FindReferences;

internal static class BuiltInMethodReferenceHelper
{
	public static Func<SyntaxToken, bool> GetTokenMatchFunction(ISyntaxFactsService syntaxFacts, ISymbol symbol)
	{
		ISyntaxFactsService syntaxFacts2 = syntaxFacts;
		ISymbol symbol2 = symbol;
		return delegate(SyntaxToken token)
		{
			if (!syntaxFacts2.IsIdentifier(token))
			{
				return false;
			}
			string text = token.ValueText.UnquoteIdentifier();
			return SemanticFacts.IsSameName(text, symbol2.Name) || GetAllRelatedSymbolNames((IMethodSymbol)symbol2).Contains(text);
		};
	}

	public static ImmutableHashSet<string> GetAllRelatedSymbolNames(IMethodSymbol builtInMethodSymbol)
	{
		if (builtInMethodSymbol is SynthesizedObjectSpecificBuiltInMethodSymbol synthesizedObjectSpecificBuiltInMethodSymbol)
		{
			using PooledNameComparisonHashSet pooledNameComparisonHashSet = PooledNameComparisonHashSet.GetInstance();
			pooledNameComparisonHashSet.Add(synthesizedObjectSpecificBuiltInMethodSymbol.Name);
			pooledNameComparisonHashSet.AddAll(synthesizedObjectSpecificBuiltInMethodSymbol.RelatedTriggerNames);
			pooledNameComparisonHashSet.AddAll(synthesizedObjectSpecificBuiltInMethodSymbol.RelatedTriggerEventNames);
			pooledNameComparisonHashSet.AddAll(synthesizedObjectSpecificBuiltInMethodSymbol.RelatedBuiltInMethodNames);
			return pooledNameComparisonHashSet.ToImmutableHashSet(SemanticFacts.NameEqualityComparer);
		}
		return ImmutableHashSet<string>.Empty;
	}

	public static ImmutableHashSet<string> GetRelatedTriggerAndTriggerEventSymbolNames(IMethodSymbol builtInMethodSymbol)
	{
		if (builtInMethodSymbol is SynthesizedObjectSpecificBuiltInMethodSymbol synthesizedObjectSpecificBuiltInMethodSymbol)
		{
			using PooledNameComparisonHashSet pooledNameComparisonHashSet = PooledNameComparisonHashSet.GetInstance();
			pooledNameComparisonHashSet.AddAll(synthesizedObjectSpecificBuiltInMethodSymbol.RelatedTriggerNames);
			pooledNameComparisonHashSet.AddAll(synthesizedObjectSpecificBuiltInMethodSymbol.RelatedTriggerEventNames);
			return pooledNameComparisonHashSet.ToImmutableHashSet(SemanticFacts.NameEqualityComparer);
		}
		return ImmutableHashSet<string>.Empty;
	}

	public static ImmutableHashSet<string> GetRelatedBuiltInMethodNames(IMethodSymbol builtInMethodSymbol)
	{
		if (builtInMethodSymbol is SynthesizedObjectSpecificBuiltInMethodSymbol synthesizedObjectSpecificBuiltInMethodSymbol)
		{
			return synthesizedObjectSpecificBuiltInMethodSymbol.RelatedBuiltInMethodNames;
		}
		return ImmutableHashSet<string>.Empty;
	}
}
