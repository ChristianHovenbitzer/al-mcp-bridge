using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Completion;

internal class FieldParameterSymbolContextRecommender : ContextAwareSymbolRecommender
{
	protected internal override bool IsExclusive => true;

	internal FieldParameterSymbolContextRecommender(MemberSyntaxContext context)
		: base(context)
	{
	}

	protected internal override async Task<IEnumerable<ISymbol>> RecommendSymbolsAsync(CancellationToken cancellationToken)
	{
		if (!base.Context.IsArgumentExpression())
		{
			return await base.RecommendSymbolsAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		}
		InvocationExpressionSyntax node = base.Context.LeftToken.Parent.FirstAncestorOrSelf<InvocationExpressionSyntax>();
		if (node == null || node.Expression == null)
		{
			return await base.RecommendSymbolsAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		}
		string memberName = string.Empty;
		string lhs = string.Empty;
		((SyntaxNode)node.Expression).SplitMemberAccess(out lhs, out memberName);
		Tuple<ITypeSymbol, ImmutableArray<ISymbol>> tuple = await base.Context.GetSymbolInfoAtPositionAsync(node.SpanStart, cancellationToken);
		if (tuple == null)
		{
			return await base.RecommendSymbolsAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		}
		IContainerSymbol containerSymbol = tuple.Item1;
		if (containerSymbol == null)
		{
			return await base.RecommendSymbolsAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		}
		BuiltInMethodTypeSymbol builtInMethodTypeSymbol = null;
		bool flag = false;
		if (tuple.Item2.Length == 1 && tuple.Item2[0].Kind == SymbolKind.Method)
		{
			MethodSymbol methodSymbol = (MethodSymbol)tuple.Item2[0];
			if (methodSymbol.MethodKind == MethodKind.BuiltInMethod)
			{
				builtInMethodTypeSymbol = (BuiltInMethodTypeSymbol)methodSymbol;
				containerSymbol = GetBuiltinMethodOwnerSymbol(builtInMethodTypeSymbol, cancellationToken);
				flag = true;
			}
		}
		else
		{
			builtInMethodTypeSymbol = base.Context.SemanticModel.LookupSymbols(node.SpanStart, LookupOptions.Default, containerSymbol, null, SymbolKind.Undefined, cancellationToken).FirstOrDefault((ISymbol s) => s.Kind == SymbolKind.Method && string.Compare(s.Name, memberName, StringComparison.OrdinalIgnoreCase) == 0) as BuiltInMethodTypeSymbol;
		}
		if (builtInMethodTypeSymbol == null || builtInMethodTypeSymbol.ParameterTypes.Length == 0)
		{
			return await base.RecommendSymbolsAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		}
		int argumentOrdinal = GetArgumentOrdinal(node);
		bool flag2 = false;
		if (argumentOrdinal > builtInMethodTypeSymbol.Parameters.Length - 1)
		{
			if (!builtInMethodTypeSymbol.Parameters[builtInMethodTypeSymbol.Parameters.Length - 1].IsParams)
			{
				return await base.RecommendSymbolsAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
			}
			flag2 = true;
		}
		BuiltInParameterSymbol builtInParameterSymbol = (flag2 ? (builtInMethodTypeSymbol.Parameters[builtInMethodTypeSymbol.Parameters.Length - 1] as BuiltInParameterSymbol) : (builtInMethodTypeSymbol.Parameters[argumentOrdinal] as BuiltInParameterSymbol));
		if (builtInParameterSymbol == null || !builtInParameterSymbol.IsMemberReference || !builtInParameterSymbol.MemberMustBeOnSame)
		{
			return await base.RecommendSymbolsAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		}
		IEnumerable<ISymbol> first = Enumerable.Empty<ISymbol>();
		if (builtInParameterSymbol.IsKeyField)
		{
			first = first.Concat(RecommendationHelper.GetKeys(base.Context, containerSymbol, cancellationToken));
		}
		first = first.Concat(RecommendationHelper.GetMembers(base.Context, containerSymbol, flag, cancellationToken));
		if (!flag)
		{
			first = first.Concat(tuple.Item2);
		}
		return first;
	}

	private int GetArgumentOrdinal(InvocationExpressionSyntax invocation)
	{
		int num = 0;
		using (IEnumerator<SyntaxToken> enumerator = invocation.ArgumentList.Arguments.GetSeparators().GetEnumerator())
		{
			while (enumerator.MoveNext() && enumerator.Current.Span.Start <= base.Context.Position)
			{
				num++;
			}
		}
		return num;
	}

	private IContainerSymbol GetBuiltinMethodOwnerSymbol(BuiltInMethodTypeSymbol methodSymbol, CancellationToken cancellationToken)
	{
		if (base.DeclaringObject.Kind == SymbolKind.Codeunit)
		{
			ISymbol enclosingSymbol = base.Context.SemanticModel.GetEnclosingSymbol(base.Context.Position, cancellationToken);
			if (enclosingSymbol == null || enclosingSymbol.Kind != SymbolKind.Method)
			{
				return null;
			}
			TableTypeSymbol relatedTable = ((CodeunitTypeSymbol)base.DeclaringObject).RelatedTable;
			if (relatedTable != null)
			{
				return relatedTable;
			}
		}
		foreach (WithStatementSyntax ancestor in Microsoft.Dynamics.Nav.CodeAnalysis.Syntax.SyntaxNodeExtensions.GetAncestors<WithStatementSyntax>(base.Context.LeftToken.Parent))
		{
			CodeExpressionSyntax withId = ancestor.WithId;
			TypeSymbol typeSymbol = base.Context.SemanticModel.GetSymbolInfo(withId, cancellationToken).GetBestOrAllSymbols().FirstOrDefault()?.GetTypeSymbol() as TypeSymbol;
			TypeSymbol typeSymbol2 = typeSymbol?.BaseType;
			while (typeSymbol2 != null)
			{
				if (typeSymbol2 == methodSymbol.ContainingType)
				{
					return typeSymbol;
				}
				typeSymbol2 = typeSymbol2?.BaseType;
			}
		}
		return (from SynthesizedGlobalVariableSymbol s in from m in base.DeclaringObject.GetMembers()
				where m.Kind == SymbolKind.GlobalVariable && ((VariableSymbol)m).VariableKind == VariableKind.Synthesized && ((SynthesizedGlobalVariableSymbol)m).Type.Kind == SymbolKind.Record
				select m
			select s.Type).FirstOrDefault();
	}
}
