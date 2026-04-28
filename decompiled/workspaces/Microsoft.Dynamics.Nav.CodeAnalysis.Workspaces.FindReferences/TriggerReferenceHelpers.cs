using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.LanguageServices;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.FindReferences;

internal static class TriggerReferenceHelpers
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

	public static IEnumerable<IMethodSymbol> GetTriggerEventSymbols(IMethodSymbol triggerSymbol)
	{
		ISymbol adjustedContainingSymbol = GetAdjustedContainingSymbol(triggerSymbol);
		if (!(adjustedContainingSymbol is ISymbolWithTriggerEvents symbolWithTriggerEvents))
		{
			yield break;
		}
		foreach (string triggerEventSymbolName in GetTriggerEventSymbolNames(triggerSymbol))
		{
			yield return symbolWithTriggerEvents.GetTriggerEvent(triggerEventSymbolName);
		}
	}

	public static ImmutableHashSet<string> GetAllRelatedSymbolNames(IMethodSymbol triggerSymbol)
	{
		TriggerTypeInfo triggerTypeInfo = GetTriggerTypeInfo(triggerSymbol);
		if (triggerTypeInfo == null)
		{
			return ImmutableHashSet<string>.Empty;
		}
		using PooledNameComparisonHashSet pooledNameComparisonHashSet = PooledNameComparisonHashSet.GetInstance();
		pooledNameComparisonHashSet.AddAll(triggerTypeInfo.RelatedTriggerNames);
		pooledNameComparisonHashSet.AddAll(triggerTypeInfo.RelatedBuiltInMethodNames);
		pooledNameComparisonHashSet.AddAll(triggerTypeInfo.RelatedTriggerEventNames);
		return pooledNameComparisonHashSet.ToImmutableHashSet(SemanticFacts.NameEqualityComparer);
	}

	public static ImmutableHashSet<string> GetRelatedTriggerAndBuiltInMethodSymbolNames(IMethodSymbol triggerSymbol)
	{
		TriggerTypeInfo triggerTypeInfo = GetTriggerTypeInfo(triggerSymbol);
		if (triggerTypeInfo == null)
		{
			return ImmutableHashSet<string>.Empty;
		}
		using PooledNameComparisonHashSet pooledNameComparisonHashSet = PooledNameComparisonHashSet.GetInstance();
		pooledNameComparisonHashSet.AddAll(triggerTypeInfo.RelatedTriggerNames);
		pooledNameComparisonHashSet.AddAll(triggerTypeInfo.RelatedBuiltInMethodNames);
		return pooledNameComparisonHashSet.ToImmutableHashSet(SemanticFacts.NameEqualityComparer);
	}

	public static ImmutableHashSet<string> GetTriggerEventSymbolNames(IMethodSymbol triggerSymbol)
	{
		TriggerTypeInfo triggerTypeInfo = GetTriggerTypeInfo(triggerSymbol);
		if (triggerTypeInfo != null)
		{
			return triggerTypeInfo.RelatedTriggerEventNames;
		}
		return ImmutableHashSet<string>.Empty;
	}

	public static ImmutableHashSet<string> GetRelatedTriggerSymbolNames(IMethodSymbol triggerSymbol)
	{
		TriggerTypeInfo triggerTypeInfo = GetTriggerTypeInfo(triggerSymbol);
		if (triggerTypeInfo != null)
		{
			return triggerTypeInfo.RelatedTriggerNames;
		}
		return ImmutableHashSet<string>.Empty;
	}

	public static ImmutableHashSet<string> GetRelatedBuiltInMethodNames(IMethodSymbol triggerSymbol)
	{
		TriggerTypeInfo triggerTypeInfo = GetTriggerTypeInfo(triggerSymbol);
		if (triggerTypeInfo != null)
		{
			return triggerTypeInfo.RelatedBuiltInMethodNames;
		}
		return ImmutableHashSet<string>.Empty;
	}

	private static TriggerTypeInfo? GetTriggerTypeInfo(IMethodSymbol triggerSymbol)
	{
		if (triggerSymbol is SourceTriggerSymbol sourceTriggerSymbol)
		{
			return sourceTriggerSymbol.TriggerInfo;
		}
		if (triggerSymbol is SynthesizedTriggerSymbol synthesizedTriggerSymbol)
		{
			return synthesizedTriggerSymbol.TriggerInfo;
		}
		return null;
	}

	public static ISymbol? GetAdjustedContainingSymbol(ISymbol symbol)
	{
		if (symbol.ContainingSymbol == null)
		{
			return null;
		}
		if (symbol.ContainingSymbol.IsChangeModifySymbol())
		{
			return ((ChangeModifySymbol)symbol.ContainingSymbol).Target;
		}
		if (symbol.ContainingSymbol.Kind.IsExtensionOrCustomizationObject())
		{
			return symbol.ContainingSymbol.TryGetExtensionTarget();
		}
		return symbol.ContainingSymbol;
	}
}
