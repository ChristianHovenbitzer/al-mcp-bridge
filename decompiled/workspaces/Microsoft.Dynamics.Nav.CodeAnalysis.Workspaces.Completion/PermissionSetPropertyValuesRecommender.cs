using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Completion;

internal class PermissionSetPropertyValuesRecommender : PropertyValuesRecommender
{
	protected internal override bool IsExclusive => true;

	internal PermissionSetPropertyValuesRecommender(MemberSyntaxContext context)
		: base(context)
	{
		base.Next = new FileUploadActionRecommender(context);
	}

	protected internal override async Task<IEnumerable<PropertyValueRecommendation>> RecommendPropertyValuesAsync(CancellationToken cancellationToken)
	{
		if (base.PropertyTypeInfo == null || base.PropertyValue == null)
		{
			return SpecializedCollections.EmptyEnumerable<PropertyValueRecommendation>();
		}
		if (!base.PropertyValue.IsKind(SyntaxKind.CommaSeparatedObjectNameReferencesPropertyValue) || !string.Equals(base.PropertyTypeInfo.ValueKind, "PermissionSet", StringComparison.OrdinalIgnoreCase))
		{
			return await base.RecommendPropertyValuesAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		}
		IEnumerable<ISymbol> enumerable = base.Context.LookupSymbols(SymbolKind.PermissionSet, cancellationToken);
		PermissionSetSymbol permissionSetSymbol = base.Context.SemanticModel.GetEnclosingSymbol(base.Context.Position) as PermissionSetSymbol;
		if (permissionSetSymbol != null)
		{
			ArrayBuilder<ISymbol> instance = ArrayBuilder<ISymbol>.GetInstance();
			foreach (ISymbol item in enumerable)
			{
				if (item != permissionSetSymbol && !permissionSetSymbol.ExcludedPermissionSets.Contains(item) && !permissionSetSymbol.IncludedPermissionSets.Contains(item))
				{
					instance.Add(item);
				}
			}
			return GetPropertyValueRecommendationsFromSymbols(instance.ToArrayAndFree());
		}
		return GetPropertyValueRecommendationsFromSymbols(enumerable);
	}
}
