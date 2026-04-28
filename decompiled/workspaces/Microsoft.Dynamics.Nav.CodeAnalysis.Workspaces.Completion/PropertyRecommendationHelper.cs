using System.Collections.Immutable;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Completion;

internal static class PropertyRecommendationHelper
{
	internal static TableTypeSymbol GetRelatedTableSymbol(PropertyValueSyntax propertyValue, Compilation compilation, ObjectTypeSymbol declaringObject)
	{
		PropertyTypeInfo propertyTypeInfo = propertyValue.GetPropertyTypeInfo();
		if (propertyTypeInfo == null)
		{
			return null;
		}
		switch (propertyTypeInfo.Kind)
		{
		case PropertyKind.SubPageView:
		case PropertyKind.SubPageLink:
			return GetSubPageViewAndLinkRelatedTable(propertyValue, compilation);
		case PropertyKind.RunPageView:
		case PropertyKind.RunPageLink:
			return GetRunPageViewAndLinkRelatedTable(propertyValue, declaringObject);
		case PropertyKind.LinkFields:
		case PropertyKind.DataItemTableFilter:
			return GetPropertyValueRelatedTable(propertyValue, compilation);
		default:
			return null;
		}
	}

	private static TableTypeSymbol GetSubPageViewAndLinkRelatedTable(PropertyValueSyntax propertyValue, Compilation compilation)
	{
		PagePartSyntax pagePartSyntax = propertyValue.Parent.Parent.Parent as PagePartSyntax;
		if (pagePartSyntax?.PartName == null)
		{
			return null;
		}
		string identifierOrLiteralValue = pagePartSyntax.PartName.Identifier.GetIdentifierOrLiteralValue();
		ImmutableArray<ISymbol> objectSymbolsByNameAcrossModules = compilation.GetObjectSymbolsByNameAcrossModules(SymbolKind.Page, identifierOrLiteralValue);
		if (objectSymbolsByNameAcrossModules.Length != 1)
		{
			return null;
		}
		return (objectSymbolsByNameAcrossModules[0] as PageTypeSymbol)?.RelatedTable;
	}

	private static TableTypeSymbol GetRunPageViewAndLinkRelatedTable(PropertyValueSyntax propertyValue, ObjectTypeSymbol declaringObject)
	{
		if (!(propertyValue.Parent.Parent.Parent is PageActionSyntax pageActionSyntax))
		{
			return null;
		}
		string identifierOrLiteralValue = pageActionSyntax.Name.GetIdentifierOrLiteralValue();
		ActionSymbol actionSymbol = null;
		ImmutableArray<Symbol>.Enumerator enumerator = declaringObject.GetCodeMembers(identifierOrLiteralValue).GetEnumerator();
		while (enumerator.MoveNext())
		{
			Symbol current = enumerator.Current;
			if (current.Kind == SymbolKind.Action)
			{
				actionSymbol = (ActionSymbol)current;
				break;
			}
		}
		if (actionSymbol == null)
		{
			return null;
		}
		PropertySymbol property = actionSymbol.GetProperty(PropertyKind.RunObject);
		if (property?.Property?.PropertyValue == null || property.Property.PropertyValue.Kind != BoundKind.ApplicationObjectReferencePropertyValue)
		{
			return null;
		}
		return ((BoundApplicationObjectReferencePropertyValue)property.Property.PropertyValue).Value.GetRelatedTableSymbol();
	}

	private static TableTypeSymbol GetPropertyValueRelatedTable(PropertyValueSyntax propertyValue, Compilation compilation)
	{
		SyntaxNode parent = propertyValue.Parent.Parent.Parent;
		if (parent == null)
		{
			return null;
		}
		return parent.Kind switch
		{
			SyntaxKind.QueryDataItem => GetTableTypeSymbol(((QueryDataItemSyntax)parent).DataItemTable, compilation), 
			SyntaxKind.XmlPortTableElement => GetTableTypeSymbol(((XmlPortTableElementSyntax)parent).SourceTable, compilation), 
			_ => null, 
		};
	}

	private static TableTypeSymbol GetTableTypeSymbol(ObjectNameOrIdSyntax tableIdentifierSyntax, Compilation compilation)
	{
		if (tableIdentifierSyntax == null)
		{
			return null;
		}
		string identifierOrLiteralValue = tableIdentifierSyntax.Identifier.GetIdentifierOrLiteralValue();
		ImmutableArray<ISymbol> objectSymbolsByNameAcrossModules = compilation.GetObjectSymbolsByNameAcrossModules(SymbolKind.Table, identifierOrLiteralValue);
		if (objectSymbolsByNameAcrossModules.Length != 1)
		{
			return null;
		}
		return objectSymbolsByNameAcrossModules[0] as TableTypeSymbol;
	}
}
