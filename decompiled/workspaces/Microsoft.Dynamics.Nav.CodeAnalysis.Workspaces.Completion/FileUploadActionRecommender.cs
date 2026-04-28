using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Completion;

internal class FileUploadActionRecommender : PropertyValuesRecommender
{
	private readonly ApplicationObjectSyntax containingObjectSyntax;

	protected internal override bool IsExclusive => true;

	public FileUploadActionRecommender(MemberSyntaxContext context)
		: base(context)
	{
		containingObjectSyntax = Microsoft.Dynamics.Nav.CodeAnalysis.Syntax.SyntaxTokenExtensions.GetAncestor<ApplicationObjectSyntax>(base.Context.TargetToken);
		base.Next = new PermissionPropertyValuesRecommender(context);
	}

	protected internal override async Task<IEnumerable<PropertyValueRecommendation>> RecommendPropertyValuesAsync(CancellationToken cancellationToken)
	{
		if ((base.PropertyTypeInfo.Kind != PropertyKind.FileUploadAction && base.PropertyTypeInfo.Kind != PropertyKind.FileUploadRowAction) || containingObjectSyntax == null)
		{
			return await base.RecommendPropertyValuesAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		}
		if (containingObjectSyntax.Kind == SyntaxKind.PageObject)
		{
			PageTypeSymbol pageTypeSymbol = (PageTypeSymbol)base.Context.SemanticModel.GetDeclaredSymbolForNode(containingObjectSyntax, cancellationToken);
			return GetPropertyValueRecommendationsFromSymbols(pageTypeSymbol.FlattenedNonCueActions.Where((ActionSymbol action) => action.ActionKind == ActionKind.FileUploadAction), (ISymbol l) => l.Name.QuoteIdentifierIfNeeded(), matchDisplayTextToInsertionText: true);
		}
		return await base.RecommendPropertyValuesAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
	}
}
