using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.SignatureHelp;

internal static class OverloadRecommender
{
	internal static ImmutableArray<MethodSymbol> GetOverloadsInOrderOfRelevance(InvocationExpressionSyntax invocation, SemanticModel model, int activeParameterIndex, CancellationToken cancellationToken)
	{
		using ArrayBuilder<MethodSymbol> arrayBuilder = new ArrayBuilder<MethodSymbol>();
		DiagnosticBag instance = DiagnosticBag.GetInstance();
		_ = invocation.ArgumentList;
		Binder binder = model.Compilation.GetBinderFactory(invocation.SyntaxTree).GetBinder(invocation);
		try
		{
			GetRankedOverloads(binder, invocation, instance, out MethodSymbol validOverload, out IEnumerable<MethodSymbol> otherCandidates, cancellationToken);
			if (validOverload != null)
			{
				arrayBuilder.Add(validOverload);
			}
			if (otherCandidates.Any())
			{
				arrayBuilder.AddRange(otherCandidates);
			}
			if (activeParameterIndex > 0)
			{
				return PromoteOverloadsBasedOnActiveParameter(arrayBuilder, activeParameterIndex);
			}
			return arrayBuilder.ToImmutableArrayOrEmpty();
		}
		finally
		{
			instance.Free();
			arrayBuilder.Free();
		}
	}

	private static ImmutableArray<MethodSymbol> PromoteOverloadsBasedOnActiveParameter(IEnumerable<MethodSymbol> overloads, int activeParameterIndex)
	{
		ArrayBuilder<MethodSymbol> instance = ArrayBuilder<MethodSymbol>.GetInstance();
		ArrayBuilder<MethodSymbol> arrayBuilder = null;
		foreach (MethodSymbol overload in overloads)
		{
			if (overload.ParameterCount < activeParameterIndex + 1)
			{
				if (arrayBuilder == null)
				{
					arrayBuilder = ArrayBuilder<MethodSymbol>.GetInstance();
				}
				arrayBuilder.Add(overload);
			}
			else
			{
				instance.Add(overload);
			}
		}
		if (arrayBuilder != null && arrayBuilder.Count > 0)
		{
			instance.AddRange(arrayBuilder);
			arrayBuilder.Free();
		}
		return instance.ToImmutableAndFree();
	}

	private static void GetRankedOverloads(Binder binder, InvocationExpressionSyntax invocation, DiagnosticBag diagnostics, out MethodSymbol? validOverload, out IEnumerable<MethodSymbol> otherCandidates, CancellationToken cancellationToken)
	{
		validOverload = null;
		otherCandidates = Enumerable.Empty<MethodSymbol>();
		OverloadResolutionResult<MethodSymbol> overloadResolutionForInvocationExpression = binder.GetOverloadResolutionForInvocationExpression(invocation, diagnostics, isStatement: false);
		if (overloadResolutionForInvocationExpression != null)
		{
			cancellationToken.ThrowIfCancellationRequested();
			if (overloadResolutionForInvocationExpression.Succeeded)
			{
				validOverload = overloadResolutionForInvocationExpression.ValidResult.Member;
			}
			MethodSymbol validOverloadTemp = validOverload;
			otherCandidates = from o in GetMethodOverloadsOrderedByApplicability(overloadResolutionForInvocationExpression)
				where o != validOverloadTemp
				select o;
		}
	}

	private static ImmutableArray<MethodSymbol> GetMethodOverloadsOrderedByApplicability(OverloadResolutionResult<MethodSymbol> overloadResolutionResult)
	{
		ArrayBuilder<MethodSymbol> instance = ArrayBuilder<MethodSymbol>.GetInstance(overloadResolutionResult.Results.Length);
		ImmutableArray<MemberResolutionResult<MethodSymbol>>.Enumerator enumerator = overloadResolutionResult.Results.Sort(CompareApplicability).GetEnumerator();
		while (enumerator.MoveNext())
		{
			instance.Add(enumerator.Current.Member);
		}
		return instance.ToImmutableAndFree();
	}

	private static int CompareApplicability<TMember>(MemberResolutionResult<TMember> member, MemberResolutionResult<TMember> other) where TMember : Symbol
	{
		int num = ResolutionKindApplicabilityRanking(member.Result.Kind) - ResolutionKindApplicabilityRanking(other.Result.Kind);
		if (num != 0)
		{
			return Math.Sign(num);
		}
		int num2 = member.Result.BadArgumentsOpt.LengthOrDefault() - other.Result.BadArgumentsOpt.LengthOrDefault();
		if (num2 != 0)
		{
			return Math.Sign(num2);
		}
		int num3 = CalcArgumentConversionRankWithPotentialBadArguments(member) - CalcArgumentConversionRankWithPotentialBadArguments(other);
		if (num3 != 0)
		{
			return Math.Sign(num3);
		}
		int num4 = member.Result.EffectiveParameters.SafeLength() - other.Result.EffectiveParameters.SafeLength();
		if (num4 != 0)
		{
			return Math.Sign(num4);
		}
		return 0;
	}

	private static int CalcArgumentConversionRankWithPotentialBadArguments<TMember>(MemberResolutionResult<TMember> m) where TMember : Symbol
	{
		if (!m.Result.ConversionsOpt.HasValue)
		{
			return 0;
		}
		int num = 0;
		for (int i = 0; i < m.Result.ConversionsOpt.Value.Length; i++)
		{
			num += (int)m.Result.ConversionsOpt.Value[i].Kind;
		}
		return num;
	}

	private static int ResolutionKindApplicabilityRanking(MemberResolutionKind kind)
	{
		switch (kind)
		{
		case MemberResolutionKind.Applicable:
			return 0;
		case MemberResolutionKind.Worse:
		case MemberResolutionKind.Worst:
			return 1;
		case MemberResolutionKind.RequiredParameterMissing:
		case MemberResolutionKind.BadArguments:
		case MemberResolutionKind.BadTypeFromFirstArguments:
			return 2;
		default:
			return int.MaxValue;
		}
	}

	private static int SafeLength(this EffectiveParameters? effectiveParameters)
	{
		return effectiveParameters?.Parameters.Length ?? 0;
	}
}
