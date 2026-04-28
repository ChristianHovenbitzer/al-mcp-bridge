using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Completion;

internal class BooleanValuesRecommender : PropertyValuesRecommender
{
	private const string TrueString = "true";

	private const string FalseString = "false";

	protected internal override bool IsExclusive => true;

	internal BooleanValuesRecommender(MemberSyntaxContext context)
		: base(context)
	{
		base.Next = new EnumValuesRecommender(context);
	}

	protected internal override async Task<IEnumerable<PropertyValueRecommendation>> RecommendPropertyValuesAsync(CancellationToken cancellationToken)
	{
		if (base.PropertyTypeInfo == null || base.PropertyValue == null)
		{
			return SpecializedCollections.EmptyEnumerable<PropertyValueRecommendation>();
		}
		if (string.Compare(base.PropertyTypeInfo.TypeName, "Boolean", StringComparison.OrdinalIgnoreCase) != 0)
		{
			return await base.RecommendPropertyValuesAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		}
		return ImmutableArray.Create(CreateBooleanPropertyRecommendation(booleanValue: true, "true"), CreateBooleanPropertyRecommendation(booleanValue: false, "false"));
	}

	private PropertyValueRecommendation CreateBooleanPropertyRecommendation(bool booleanValue, string stringValue)
	{
		return new PropertyValueRecommendation(stringValue)
		{
			InsertionText = stringValue,
			IsMarkdownDocs = true,
			DetailText = base.PropertyTypeInfo.GetBooleanValuePropertyDetailText(booleanValue),
			DescriptionValue = string.Empty,
			IsDeprecated = (base.PropertyTypeInfo.Deprecated?.IsSupported(base.Context.RuntimeVersion) ?? false)
		};
	}
}
