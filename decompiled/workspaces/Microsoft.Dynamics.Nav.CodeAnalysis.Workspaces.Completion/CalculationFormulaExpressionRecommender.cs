using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Completion;

internal class CalculationFormulaExpressionRecommender : PropertyExpressionRecommender<CalculationFormulaPropertyValueSyntax, TableFilterExpressionSyntax>
{
	internal CalculationFormulaExpressionRecommender(MemberSyntaxContext context)
		: base(context)
	{
	}

	protected internal override async Task<IEnumerable<ISymbol>> RecommendSymbolsAsync(CancellationToken cancellationToken)
	{
		CalculationFormulaExpressionRecommender calculationFormulaExpressionRecommender = this;
		SeparatedSyntaxList<PropertyExpressionSyntax>? separatedSyntaxList = base.PropertyExpressionSyntax?.Conditions;
		IEnumerable<PropertyExpressionSyntax> conditions;
		if (!separatedSyntaxList.HasValue)
		{
			conditions = SpecializedCollections.EmptyEnumerable<PropertyExpressionSyntax>();
		}
		else
		{
			IEnumerable<PropertyExpressionSyntax> enumerable = separatedSyntaxList.GetValueOrDefault();
			conditions = enumerable;
		}
		return await calculationFormulaExpressionRecommender.RecommendSymbolsAsync(cancellationToken, conditions, shouldUseLhsFieldsFromSource: false);
	}

	protected override TableTypeSymbol? SetReferencedTable()
	{
		if (!base.Context.General.HasFlag(GeneralContexts.AnyComplexPropertyExpression))
		{
			return null;
		}
		ExpressionSyntax memberAccess = null;
		switch (base.DeclaringSyntax.Kind)
		{
		case SyntaxKind.ExistCalculationFormulaStatement:
		case SyntaxKind.CountCalculationFormulaStatement:
			memberAccess = ((TableCalculationFormulaSyntax)base.DeclaringSyntax).Table;
			break;
		case SyntaxKind.SumCalculationFormulaStatement:
		case SyntaxKind.AverageCalculationFormulaStatement:
		case SyntaxKind.MinCalculationFormulaStatement:
		case SyntaxKind.MaxCalculationFormulaStatement:
		case SyntaxKind.LookupCalculationFormulaStatement:
			memberAccess = ((FieldCalculationFormulaSyntax)base.DeclaringSyntax).Field;
			break;
		}
		return GetReferencedTable(memberAccess);
	}
}
