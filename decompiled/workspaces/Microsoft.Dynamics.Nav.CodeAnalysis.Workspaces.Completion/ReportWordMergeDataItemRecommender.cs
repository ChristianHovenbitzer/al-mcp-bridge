using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Completion;

internal class ReportWordMergeDataItemRecommender : PropertyValuesRecommender
{
	private readonly ReportSyntax reportSyntax;

	internal ReportWordMergeDataItemRecommender(MemberSyntaxContext context)
		: base(context)
	{
		reportSyntax = Microsoft.Dynamics.Nav.CodeAnalysis.Syntax.SyntaxTokenExtensions.GetAncestor<ReportSyntax>(base.Context.TargetToken);
		base.Next = new ReportDefaultRenderingLayoutRecommender(context);
	}

	protected internal override async Task<IEnumerable<PropertyValueRecommendation>> RecommendPropertyValuesAsync(CancellationToken cancellationToken)
	{
		PropertyTypeInfo? propertyTypeInfo = base.PropertyTypeInfo;
		if (propertyTypeInfo == null || propertyTypeInfo.Kind != PropertyKind.WordMergeDataItem || reportSyntax == null)
		{
			return await base.RecommendPropertyValuesAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		}
		IEnumerable<IReportDataItemSymbol> rootDataItems = GetRootDataItems(base.Context.SemanticModel.GetDeclaredSymbolForNode(reportSyntax, cancellationToken) as SourceReportTypeSymbol);
		return GetPropertyValueRecommendationsFromSymbols(rootDataItems, (ISymbol s) => s.Name.QuoteIdentifierIfNeeded(), matchDisplayTextToInsertionText: true);
	}

	private static IEnumerable<IReportDataItemSymbol> GetRootDataItems(SourceReportTypeSymbol? sourceReportSymbol)
	{
		if (sourceReportSymbol == null)
		{
			return SpecializedCollections.EmptyEnumerable<IReportDataItemSymbol>();
		}
		return sourceReportSymbol.FlattenedDataItems.Where((ReportDataItemSymbol d) => d.ParentDataItem == null).ToImmutableArray();
	}
}
