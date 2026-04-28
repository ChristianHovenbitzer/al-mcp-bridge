using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Completion;

internal abstract class PropertyExpressionRecommender<T, U> : ContextAwareSymbolRecommender where T : PropertyValueSyntax where U : PropertyExpressionSyntax
{
	private TableTypeSymbol referencedTable;

	protected T DeclaringSyntax { get; }

	protected U PropertyExpressionSyntax { get; }

	protected virtual bool HasContext
	{
		get
		{
			if (PropertyExpressionSyntax != null || base.Context.TargetToken.IsKind(SyntaxKind.CommaToken))
			{
				return DeclaringSyntax != null;
			}
			return false;
		}
	}

	protected TableTypeSymbol? ReferencedTable
	{
		get
		{
			if (referencedTable != null)
			{
				return referencedTable;
			}
			referencedTable = SetReferencedTable();
			return referencedTable;
		}
	}

	protected PropertyExpressionRecommender(MemberSyntaxContext context)
		: base(context)
	{
		PropertyExpressionSyntax = Microsoft.Dynamics.Nav.CodeAnalysis.Syntax.SyntaxTokenExtensions.GetAncestor<U>(base.Context.TargetToken);
		DeclaringSyntax = Microsoft.Dynamics.Nav.CodeAnalysis.Syntax.SyntaxTokenExtensions.GetAncestor<T>(base.Context.TargetToken);
	}

	protected virtual TableTypeSymbol? GetSourceTable()
	{
		return base.DeclaringObject?.GetRelatedTableSymbol();
	}

	protected abstract TableTypeSymbol SetReferencedTable();

	protected TableTypeSymbol? GetReferencedTable(SyntaxNode memberAccess)
	{
		if (memberAccess == null || memberAccess.IsMissing)
		{
			return null;
		}
		memberAccess.SplitMemberAccess(out NameSyntax lhs, out string _);
		if (lhs == null)
		{
			return null;
		}
		return base.Context.EnclosingBinder.LookupNamespaceOrTypeSymbol(lhs, SymbolKind.Table, DiagnosticBag.GetNullInstance()) as TableTypeSymbol;
	}

	protected async Task<IEnumerable<ISymbol>> RecommendSymbolsAsync(CancellationToken cancellationToken, IEnumerable<PropertyExpressionSyntax> conditions, bool shouldUseLhsFieldsFromSource = true)
	{
		if (!HasContext || !base.Context.General.HasFlag(GeneralContexts.AnyComplexPropertyExpression))
		{
			return await base.RecommendSymbolsAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		}
		SyntaxToken targetToken = base.Context.TargetToken;
		if (targetToken.IsKind(SyntaxKind.ColonColonToken) && targetToken.ParentIsKind(SyntaxKind.OptionAccessExpression))
		{
			ISymbol declaredSymbol = base.Context.SemanticModel.GetDeclaredSymbol(targetToken.Parent, cancellationToken);
			if (declaredSymbol != null && declaredSymbol.Kind == SymbolKind.Enum)
			{
				return base.Context.SemanticModel.Compilation.GetEnumValues((EnumTypeSymbol)declaredSymbol);
			}
			return RecommendationHelper.GetObjects(base.Context, (OptionAccessExpressionSyntax)targetToken.Parent, cancellationToken);
		}
		if ((targetToken.IsKind(SyntaxKind.OpenParenToken, SyntaxKind.CommaToken) && targetToken.ParentIsKind(SyntaxKind.TableFilterExpression)) || (targetToken.IsKind(SyntaxKind.CommaToken) && targetToken.ParentIsKind(SyntaxKind.TableFilterPropertyValue)))
		{
			TableTypeSymbol tableFilterExpressionReferencedTable = GetTableFilterExpressionReferencedTable(targetToken.Parent);
			PooledNameComparisonHashSet instance = PooledNameComparisonHashSet.GetInstance();
			try
			{
				GetUsedFieldsInSyntax(instance, conditions);
				IEnumerable<ISymbol> fields = RecommendationHelper.GetFields(base.Context, shouldUseLhsFieldsFromSource ? tableFilterExpressionReferencedTable : ReferencedTable, cancellationToken);
				List<ISymbol> list = new List<ISymbol>();
				foreach (ISymbol item in fields)
				{
					if (!instance.Contains(item.Name))
					{
						list.Add(item);
					}
				}
				return list;
			}
			finally
			{
				instance.Free();
			}
		}
		if (targetToken.IsKind(SyntaxKind.OpenParenToken) && targetToken.ParentIsKind(SyntaxKind.SimpleFieldExpression, SyntaxKind.FieldUpperLimitExpression, SyntaxKind.FieldUpperLimitExpression))
		{
			return RecommendationHelper.GetFields(base.Context, (!shouldUseLhsFieldsFromSource) ? GetSourceTable() : ReferencedTable, cancellationToken);
		}
		if (RecommendationHelper.IsConstExpressionContext(targetToken))
		{
			return GetPossibleFieldValues(((ConstExpressionSyntax)targetToken.Parent).LeftHandSide, shouldUseLhsFieldsFromSource);
		}
		if (RecommendationHelper.IsFilterExpressionContext(targetToken, out FilterExpressionSyntax filterExpressionSyntax))
		{
			return GetPossibleFieldValues(filterExpressionSyntax.LeftHandSide, shouldUseLhsFieldsFromSource);
		}
		return SpecializedCollections.EmptyEnumerable<ISymbol>();
	}

	protected IEnumerable<ISymbol> GetPossibleFieldValues(IdentifierNameSyntax lhs, bool shouldUseLhsFieldsFromSource)
	{
		FieldSymbol fieldSymbol = (shouldUseLhsFieldsFromSource ? GetSourceTable() : ReferencedTable)?.GetCodeMembers(lhs.Unquoted()).FirstOrDefault((Symbol m) => m.Kind == SymbolKind.Field) as FieldSymbol;
		if (fieldSymbol != null)
		{
			switch (fieldSymbol.Type.Kind)
			{
			case SymbolKind.Enum:
			{
				EnumTypeSymbol container = (EnumTypeSymbol)fieldSymbol.Type;
				return base.Context.SemanticModel.LookupSymbols(base.Context.Position, LookupOptions.MustBeInstance | LookupOptions.MustBeOption, container);
			}
			case SymbolKind.OptionType:
				return ((OptionTypeSymbol)fieldSymbol.Type).GetCodeMembers();
			}
		}
		return SpecializedCollections.EmptyEnumerable<ISymbol>();
	}

	protected static void GetUsedFieldsInSyntax(PooledNameComparisonHashSet set, IEnumerable<PropertyExpressionSyntax> conditions)
	{
		foreach (PropertyExpressionSyntax condition in conditions)
		{
			IdentifierNameSyntax identifierNameSyntax = null;
			switch (condition.Kind)
			{
			case SyntaxKind.ConstExpression:
				identifierNameSyntax = ((ConstExpressionSyntax)condition).LeftHandSide;
				break;
			case SyntaxKind.FilterExpression:
				identifierNameSyntax = ((FilterExpressionSyntax)condition).LeftHandSide;
				break;
			case SyntaxKind.SimpleFieldExpression:
				identifierNameSyntax = ((SimpleFieldExpressionSyntax)condition).LeftHandSide;
				break;
			case SyntaxKind.FieldFilterExpression:
				identifierNameSyntax = ((FieldFilterExpressionSyntax)condition).LeftHandSide;
				break;
			case SyntaxKind.FieldUpperLimitFilterExpression:
				identifierNameSyntax = ((FieldUpperLimitFilterExpressionSyntax)condition).LeftHandSide;
				break;
			case SyntaxKind.FieldUpperLimitExpression:
				identifierNameSyntax = ((FieldUpperLimitExpressionSyntax)condition).LeftHandSide;
				break;
			}
			if (identifierNameSyntax == null)
			{
				continue;
			}
			string valueText = identifierNameSyntax.Identifier.ValueText;
			if (!string.IsNullOrEmpty(valueText))
			{
				string item = valueText.ToUpperInvariant();
				if (!set.Contains(item))
				{
					set.Add(item);
				}
			}
		}
	}

	private TableTypeSymbol? GetTableFilterExpressionReferencedTable(SyntaxNode node)
	{
		PropertySyntax propertySyntax = (PropertySyntax)node.GetFirstParent(SyntaxKind.Property);
		PropertyKind? propertyKind = propertySyntax?.GetPropertyTypeInfo()?.Kind;
		if (propertyKind.HasValue && (propertyKind.GetValueOrDefault() == PropertyKind.RunPageView || propertyKind.GetValueOrDefault() == PropertyKind.SubPageView))
		{
			return GetRelatedTableSymbol(propertySyntax.Value);
		}
		return GetSourceTable();
	}
}
