using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Completion;

internal class ReportDefaultRenderingLayoutRecommender : PropertyValuesRecommender
{
	private readonly ApplicationObjectSyntax containingObjectSyntax;

	protected internal override bool IsExclusive => true;

	internal ReportDefaultRenderingLayoutRecommender(MemberSyntaxContext context)
		: base(context)
	{
		containingObjectSyntax = Microsoft.Dynamics.Nav.CodeAnalysis.Syntax.SyntaxTokenExtensions.GetAncestor<ApplicationObjectSyntax>(base.Context.TargetToken);
		base.Next = new InherentEntitlementsPropertyValuesRecommender(context);
	}

	protected internal override async Task<IEnumerable<PropertyValueRecommendation>> RecommendPropertyValuesAsync(CancellationToken cancellationToken)
	{
		if (base.PropertyTypeInfo.Kind != PropertyKind.DefaultRenderingLayout || containingObjectSyntax == null)
		{
			return await base.RecommendPropertyValuesAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		}
		switch (containingObjectSyntax.Kind)
		{
		case SyntaxKind.ReportObject:
		{
			ReportTypeSymbol reportTypeSymbol = (ReportTypeSymbol)base.Context.SemanticModel.GetDeclaredSymbolForNode(containingObjectSyntax, cancellationToken);
			return GetPropertyValueRecommendationsFromSymbols(reportTypeSymbol.Layouts, (ISymbol l) => l.Name.QuoteIdentifierIfNeeded(), matchDisplayTextToInsertionText: true);
		}
		case SyntaxKind.ReportExtension:
		{
			ReportExtensionTypeSymbol reportExtensionTypeSymbol = (ReportExtensionTypeSymbol)base.Context.SemanticModel.GetDeclaredSymbolForNode(containingObjectSyntax, cancellationToken);
			return GetPropertyValueRecommendationsFromSymbols(reportExtensionTypeSymbol.Layouts, (ISymbol l) => l.Name.QuoteIdentifierIfNeeded(), matchDisplayTextToInsertionText: true);
		}
		default:
			return await base.RecommendPropertyValuesAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		}
	}
}
