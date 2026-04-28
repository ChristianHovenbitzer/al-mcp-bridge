using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Completion;

internal class InherentEntitlementsPropertyValuesRecommender : PropertyValuesRecommender
{
	private readonly ApplicationObjectSyntax containingObjectSyntax;

	protected internal override bool IsExclusive => true;

	internal InherentEntitlementsPropertyValuesRecommender(MemberSyntaxContext context)
		: base(context)
	{
		containingObjectSyntax = Microsoft.Dynamics.Nav.CodeAnalysis.Syntax.SyntaxTokenExtensions.GetAncestor<ApplicationObjectSyntax>(base.Context.TargetToken);
		base.Next = new MovedFromToPropertyValueRecommender(context);
	}

	protected internal override async Task<IEnumerable<PropertyValueRecommendation>> RecommendPropertyValuesAsync(CancellationToken cancellationToken)
	{
		if (base.PropertyTypeInfo == null || base.PropertyValue == null)
		{
			return SpecializedCollections.EmptyEnumerable<PropertyValueRecommendation>();
		}
		if ((!base.PropertyValue.IsKind(SyntaxKind.InherentEntitlementsPropertyValue) && !base.PropertyValue.IsKind(SyntaxKind.InherentPermissionsPropertyValue)) || containingObjectSyntax == null)
		{
			return await base.RecommendPropertyValuesAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		}
		switch (containingObjectSyntax.Kind)
		{
		case SyntaxKind.TableObject:
			return PermissionPropertyRecommendationHelper.RimdxRecommendations;
		case SyntaxKind.CodeunitObject:
		case SyntaxKind.PageObject:
		case SyntaxKind.ReportObject:
		case SyntaxKind.XmlPortObject:
		case SyntaxKind.QueryObject:
			return PermissionPropertyRecommendationHelper.ExecuteRecommendations;
		default:
			return await base.RecommendPropertyValuesAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		}
	}
}
