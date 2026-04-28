using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Completion;

internal class ObjectReferencePropertyValuesRecommender : PropertyValuesRecommender
{
	protected internal override bool IsExclusive => true;

	internal ObjectReferencePropertyValuesRecommender(MemberSyntaxContext context)
		: base(context)
	{
		base.Next = new FieldListReferencePropertyValuesRecommender(context);
	}

	protected internal override async Task<IEnumerable<PropertyValueRecommendation>> RecommendPropertyValuesAsync(CancellationToken cancellationToken)
	{
		if (base.PropertyTypeInfo == null || base.PropertyValue == null)
		{
			return SpecializedCollections.EmptyEnumerable<PropertyValueRecommendation>();
		}
		SymbolKind symbolKind;
		switch (base.PropertyValue.Kind)
		{
		case SyntaxKind.QualifiedObjectReferencePropertyValue:
			symbolKind = GetSymbolKindFromQualifiedObjectReferencePropertyValue();
			break;
		case SyntaxKind.ObjectReferencePropertyValue:
			symbolKind = GetSymbolKindFromObjectReferencePropertyValue();
			break;
		case SyntaxKind.CommaSeparatedPropertyValue:
		case SyntaxKind.CommaSeparatedObjectNameReferencesPropertyValue:
			symbolKind = GetSymbolKindFromCommaSeparatedObjectReferencePropertyValue();
			break;
		default:
			return await base.RecommendPropertyValuesAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		}
		if (symbolKind == SymbolKind.Undefined)
		{
			return await base.RecommendPropertyValuesAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		}
		switch (base.PropertyTypeInfo.Kind)
		{
		case PropertyKind.SumIndexFields:
			return await base.RecommendPropertyValuesAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		case PropertyKind.RoleCenter:
		{
			IEnumerable<ISymbol> source2 = base.Context.LookupSymbols(SymbolKind.Page, cancellationToken);
			source2 = source2.Where((ISymbol m) => m.Kind == SymbolKind.Namespace || (m.Kind == SymbolKind.Page && ((PageTypeSymbol)m).PageType == PageTypeKind.RoleCenter));
			return GetPropertyValueRecommendationsFromSymbols(source2);
		}
		case PropertyKind.Customizations:
		{
			ImmutableHashSet<string> existingMembers = ((CommaSeparatedObjectNameReferencesPropertyValueSyntax)base.PropertyValue).Values.Select((ObjectNameReferenceSyntax x) => x.ToString()).ToImmutableHashSet(SemanticFacts.NameEqualityComparer);
			IEnumerable<ISymbol> source = base.Context.LookupSymbols(SymbolKind.PageCustomization, cancellationToken);
			source = source.Where((ISymbol m) => !FilterOutPageCustomizationRecommendation(existingMembers, m));
			return GetPropertyValueRecommendationsFromSymbols(source);
		}
		default:
		{
			IEnumerable<ISymbol> symbols = base.Context.LookupSymbols(symbolKind, cancellationToken);
			return GetPropertyValueRecommendationsFromSymbols(symbols);
		}
		}
	}

	private SymbolKind GetSymbolKindFromQualifiedObjectReferencePropertyValue()
	{
		if (base.PropertyValue.Kind != SyntaxKind.QualifiedObjectReferencePropertyValue)
		{
			return SymbolKind.Undefined;
		}
		return ((QualifiedObjectReferencePropertyValueSyntax)base.PropertyValue).ObjectType.Kind.ToSymbolKind();
	}

	private SymbolKind GetSymbolKindFromObjectReferencePropertyValue()
	{
		if (base.PropertyValue.Kind != SyntaxKind.ObjectReferencePropertyValue)
		{
			return SymbolKind.Undefined;
		}
		return base.PropertyTypeInfo.Kind.GetSymbolKind();
	}

	private SymbolKind GetSymbolKindFromCommaSeparatedObjectReferencePropertyValue()
	{
		if (base.PropertyValue.Kind != SyntaxKind.CommaSeparatedPropertyValue && base.PropertyValue.Kind != SyntaxKind.CommaSeparatedObjectNameReferencesPropertyValue)
		{
			return SymbolKind.Undefined;
		}
		return base.PropertyTypeInfo.Kind.GetSymbolKind();
	}

	private bool FilterOutPageCustomizationRecommendation(ImmutableHashSet<string> existingMembers, ISymbol symbol)
	{
		if (symbol.Kind == SymbolKind.Namespace)
		{
			return false;
		}
		if (symbol.Kind == SymbolKind.PageCustomization)
		{
			if (existingMembers.Contains(symbol.Name))
			{
				return true;
			}
			if (base.DeclaringObject.Kind == SymbolKind.ProfileExtension && ((PageCustomizationTypeSymbol)symbol).UsesClearProperties())
			{
				return true;
			}
		}
		return false;
	}
}
