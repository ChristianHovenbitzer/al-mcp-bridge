using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Completion;

internal class EnumValuesRecommender : PropertyValuesRecommender
{
	protected internal override bool IsExclusive => true;

	internal EnumValuesRecommender(MemberSyntaxContext context)
		: base(context)
	{
		base.Next = new ImageValuesRecommender(context);
	}

	protected internal override async Task<IEnumerable<PropertyValueRecommendation>> RecommendPropertyValuesAsync(CancellationToken cancellationToken)
	{
		if (base.PropertyTypeInfo == null || base.PropertyValue == null)
		{
			return SpecializedCollections.EmptyEnumerable<PropertyValueRecommendation>();
		}
		if (base.PropertyTypeInfo.Kind == PropertyKind.InitValue)
		{
			FieldSymbol declaringField = base.DeclaringField;
			if (declaringField?.Type != null)
			{
				switch (declaringField.Type?.Kind)
				{
				case SymbolKind.Enum:
					if (declaringField.DeclaringCompilation != null)
					{
						ImmutableArray<EnumValueSymbol> enumValues = declaringField.DeclaringCompilation.GetEnumValues((EnumTypeSymbol)declaringField.Type);
						return GetPropertyValueRecommendationsFromSymbols(enumValues, GetInitValuePropertyInsertionText);
					}
					break;
				case SymbolKind.OptionType:
				{
					ImmutableArray<IOptionSymbol> values = ((OptionTypeSymbol)declaringField.Type).Values;
					return GetPropertyValueRecommendationsFromSymbols(values, GetInitValuePropertyInsertionText);
				}
				}
			}
		}
		if (base.PropertyTypeInfo is EnumPropertyTypeInfo info)
		{
			return PropertyValuesRecommender.GetPropertyValueRecommendationsFromEnumPropertyTypeInfo(info, base.Context.RuntimeVersion);
		}
		return await base.RecommendPropertyValuesAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
	}

	private string GetInitValuePropertyInsertionText(ISymbol enumSymbol)
	{
		if (SyntaxFacts.GetPropertyKeywordKind(enumSymbol.Name.ToUpperInvariant()) == SyntaxKind.None)
		{
			return enumSymbol.Name;
		}
		return enumSymbol.Name.QuoteIdentifier();
	}
}
