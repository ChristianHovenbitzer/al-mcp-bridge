using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Completion;

internal class EnumContextSymbolRecommender : ContextAwareSymbolRecommender
{
	internal EnumContextSymbolRecommender(MemberSyntaxContext context)
		: base(context)
	{
	}

	protected internal override async Task<IEnumerable<ISymbol>> RecommendSymbolsAsync(CancellationToken cancellationToken)
	{
		CodeExpressionSyntax codeExpressionSyntax = null;
		bool hasFlagAnyExpression = base.Context.General.HasFlag(GeneralContexts.AnyExpression);
		bool hasFlagStatement = base.Context.General.HasFlag(GeneralContexts.Statement);
		bool isRightOfNameSeparator = base.Context.IsRightOfNameSeparator;
		bool flag = false;
		SymbolInfo? symbolInfo;
		if (IsAssigment(hasFlagAnyExpression, hasFlagStatement, isRightOfNameSeparator))
		{
			StatementSyntax statementSyntax = (StatementSyntax)base.Context.TargetToken.Parent;
			symbolInfo = base.Context.SemanticModel.GetSymbolInfo(((AssignmentStatementSyntax)statementSyntax).Target);
		}
		else
		{
			if (!IsCaseStatement(hasFlagAnyExpression, hasFlagStatement, isRightOfNameSeparator))
			{
				if (IsNestedEnumValue(base.Context))
				{
					return SpecializedCollections.EmptyEnumerable<ISymbol>();
				}
				return await base.RecommendSymbolsAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
			}
			flag = true;
			StatementSyntax statementSyntax = (StatementSyntax)base.Context.TargetToken.Parent;
			codeExpressionSyntax = ((CaseStatementSyntax)statementSyntax).Expression;
			symbolInfo = base.Context.SemanticModel.GetSymbolInfo(codeExpressionSyntax);
		}
		PooledList<ISymbol> instance = PooledList<ISymbol>.GetInstance();
		ITypeSymbol typeSymbol = symbolInfo?.Symbol.GetTypeSymbol();
		try
		{
			if (typeSymbol != null && typeSymbol.Kind == SymbolKind.Enum)
			{
				instance.Add(typeSymbol);
				if (flag)
				{
					instance.Add(EnumClassTypeSymbol.Instance);
					if (codeExpressionSyntax != null && codeExpressionSyntax.Kind == SyntaxKind.IdentifierName)
					{
						ISymbol symbol = base.Context.SemanticModel.GetSymbolInfo(codeExpressionSyntax).Symbol;
						if (symbol != null)
						{
							instance.Add(symbol);
						}
					}
				}
				return instance.ToImmutableArrayOrEmpty();
			}
		}
		finally
		{
			instance.Free();
		}
		return await base.RecommendSymbolsAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
	}

	internal static bool IsNestedEnumValue(MemberSyntaxContext context)
	{
		if (context.TargetToken.ParentIsKind(SyntaxKind.OptionAccessExpression))
		{
			OptionAccessExpressionSyntax optionAccessExpressionSyntax = (OptionAccessExpressionSyntax)context.TargetToken.Parent;
			if (optionAccessExpressionSyntax.Expression.Kind == SyntaxKind.OptionAccessExpression)
			{
				OptionAccessExpressionSyntax optionAccessExpressionSyntax2 = (OptionAccessExpressionSyntax)optionAccessExpressionSyntax.Expression;
				if (!context.SemanticModel.GetSymbolInfo(optionAccessExpressionSyntax2.Expression).Symbol.IsKind(SymbolKind.Class))
				{
					return true;
				}
			}
		}
		return false;
	}
}
