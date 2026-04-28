using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Completion;

internal class FieldListReferencePropertyValuesRecommender : PropertyValuesRecommender
{
	protected internal override bool IsExclusive => true;

	internal FieldListReferencePropertyValuesRecommender(MemberSyntaxContext context)
		: base(context)
	{
		base.Next = new ControlListReferencePropertyValuesRecommender(context);
	}

	protected internal override async Task<IEnumerable<PropertyValueRecommendation>> RecommendPropertyValuesAsync(CancellationToken cancellationToken)
	{
		if (base.PropertyTypeInfo == null || base.PropertyValue == null)
		{
			return SpecializedCollections.EmptyEnumerable<PropertyValueRecommendation>();
		}
		if (!base.PropertyValue.IsKind(SyntaxKind.CommaSeparatedPropertyValue) || !string.Equals(base.PropertyTypeInfo.ValueKind, SymbolKind.Field.ToString(), StringComparison.OrdinalIgnoreCase))
		{
			return await base.RecommendPropertyValuesAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		}
		IContainerSymbol tableTypeSymbol = FindRelatedTableSymbol();
		IEnumerable<ISymbol> fields = RecommendationHelper.RecommendFieldSymbolsForFieldGroup(base.Context, tableTypeSymbol, cancellationToken);
		fields = FilterResult(fields);
		return GetPropertyValueRecommendationsFromSymbols(fields);
	}

	private IContainerSymbol FindRelatedTableSymbol()
	{
		IContainerSymbol containerSymbol = null;
		ISymbol symbol = null;
		switch (base.PropertyTypeInfo.Kind)
		{
		case PropertyKind.RequestFilterFields:
		case PropertyKind.CalcFields:
			symbol = base.Context.SemanticModel.GetDeclaredSymbol(base.PropertyValue.Parent);
			if (symbol == null)
			{
				break;
			}
			if (symbol.ContainingType.IsKind(SymbolKind.XmlPortNode))
			{
				TypeSymbol type = ((XmlPortNodeSymbol)symbol.ContainingType).Type;
				if (type.IsKind(SymbolKind.Record))
				{
					containerSymbol = type.BaseType;
				}
			}
			else if (symbol.ContainingType.IsKind(SymbolKind.ReportDataItem))
			{
				TypeSymbol type2 = ((ReportDataItemSymbol)symbol.ContainingType).Type;
				if (type2.IsKind(SymbolKind.Record))
				{
					containerSymbol = type2.BaseType;
				}
			}
			else
			{
				if (!symbol.ContainingSymbol.Kind.IsKind(SymbolKind.Change))
				{
					break;
				}
				ChangeModifySymbol changeModifySymbol = (ChangeModifySymbol)symbol.ContainingSymbol;
				if (changeModifySymbol.ChangeTargetKind.IsKind(SymbolKind.ReportDataItem))
				{
					TypeSymbol type3 = ((ReportDataItemSymbol)changeModifySymbol.Target).Type;
					if (type3.IsKind(SymbolKind.Record))
					{
						containerSymbol = type3.BaseType;
					}
				}
			}
			break;
		case PropertyKind.DataCaptionFields:
		case PropertyKind.ODataKeyFields:
			symbol = base.Context.SemanticModel.GetDeclaredSymbol(base.PropertyValue.Parent);
			if (symbol != null)
			{
				containerSymbol = symbol.GetContainingPage()?.RelatedTable;
			}
			break;
		}
		return containerSymbol ?? base.DeclaringObject;
	}

	private IEnumerable<ISymbol> FilterResult(IEnumerable<ISymbol> fields)
	{
		if (base.PropertyTypeInfo.Kind == PropertyKind.SumIndexFields)
		{
			fields = fields.Where((ISymbol symbol) => symbol.Kind == SymbolKind.Field && ((FieldSymbol)symbol).Type.NavTypeKind.IsValidSumIndexFieldValue());
		}
		return fields;
	}
}
