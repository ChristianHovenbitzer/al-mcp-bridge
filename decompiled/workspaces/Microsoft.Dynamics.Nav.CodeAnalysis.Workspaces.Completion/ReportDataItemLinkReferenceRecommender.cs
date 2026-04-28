using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Completion;

internal class ReportDataItemLinkReferenceRecommender : PropertyValuesRecommender
{
	private readonly ReportDataItemSyntax reportDataItemSyntax;

	protected internal override bool IsExclusive => true;

	internal ReportDataItemLinkReferenceRecommender(MemberSyntaxContext context)
		: base(context)
	{
		reportDataItemSyntax = Microsoft.Dynamics.Nav.CodeAnalysis.Syntax.SyntaxTokenExtensions.GetAncestor<ReportDataItemSyntax>(base.Context.TargetToken);
		base.Next = new ReportWordMergeDataItemRecommender(context);
	}

	protected internal override async Task<IEnumerable<PropertyValueRecommendation>> RecommendPropertyValuesAsync(CancellationToken cancellationToken)
	{
		if (base.PropertyTypeInfo.Kind != PropertyKind.DataItemLinkReference || reportDataItemSyntax == null)
		{
			return await base.RecommendPropertyValuesAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		}
		IEnumerable<IReportDataItemSymbol> parentDataItems = GetParentDataItems(base.Context.SemanticModel.GetDeclaredSymbolForNode(reportDataItemSyntax, cancellationToken) as SourceReportDataItemSymbol);
		return GetPropertyValueRecommendationsFromSymbols(parentDataItems, (ISymbol s) => s.Name.QuoteIdentifierIfNeeded(), matchDisplayTextToInsertionText: true);
	}

	private static IEnumerable<IReportDataItemSymbol> GetParentDataItems(ReportDataItemSymbol? currentDataItem)
	{
		if (currentDataItem == null)
		{
			return SpecializedCollections.EmptyEnumerable<IReportDataItemSymbol>();
		}
		ArrayBuilder<IReportDataItemSymbol> instance = ArrayBuilder<IReportDataItemSymbol>.GetInstance();
		while (currentDataItem.ParentDataItem != null)
		{
			instance.Add(currentDataItem.ParentDataItem);
			currentDataItem = currentDataItem.ParentDataItem;
		}
		return instance.ToImmutableAndFree();
	}
}
