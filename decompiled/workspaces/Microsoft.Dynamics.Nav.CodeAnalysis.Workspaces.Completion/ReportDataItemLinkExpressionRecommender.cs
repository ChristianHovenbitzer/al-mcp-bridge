using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Completion;

internal class ReportDataItemLinkExpressionRecommender : ContextAwareSymbolRecommender
{
	private readonly ReportDataItemSyntax reportDataItemSyntax;

	private readonly PropertySyntax propertySyntax;

	internal ReportDataItemLinkExpressionRecommender(MemberSyntaxContext context)
		: base(context)
	{
		propertySyntax = Microsoft.Dynamics.Nav.CodeAnalysis.Syntax.SyntaxTokenExtensions.GetAncestor<PropertySyntax>(base.Context.TargetToken);
		reportDataItemSyntax = Microsoft.Dynamics.Nav.CodeAnalysis.Syntax.SyntaxTokenExtensions.GetAncestor<ReportDataItemSyntax>(base.Context.TargetToken);
	}

	protected internal override Task<IEnumerable<ISymbol>> RecommendSymbolsAsync(CancellationToken cancellationToken)
	{
		if (reportDataItemSyntax != null)
		{
			PropertySyntax obj = propertySyntax;
			if (obj != null && obj.Value?.Kind == SyntaxKind.ReportDataItemLinkPropertyValue)
			{
				return Task.FromResult(GetRecommendation(cancellationToken));
			}
		}
		return Task.FromResult(SpecializedCollections.EmptyEnumerable<ISymbol>());
	}

	private IEnumerable<ISymbol> GetRecommendation(CancellationToken cancellationToken)
	{
		PropertySyntax ancestor = Microsoft.Dynamics.Nav.CodeAnalysis.Syntax.SyntaxTokenExtensions.GetAncestor<PropertySyntax>(base.Context.TargetToken);
		switch (base.Context.LeftToken.Kind)
		{
		case SyntaxKind.EqualsToken:
			if (ancestor.EqualsToken == base.Context.LeftToken)
			{
				return GetSourceFieldRecommendation(cancellationToken);
			}
			return SpecializedCollections.EmptyEnumerable<ISymbol>();
		case SyntaxKind.CommaToken:
			return GetSourceFieldRecommendation(cancellationToken);
		case SyntaxKind.OpenParenToken:
			return GetRelatedFieldRecommendation(cancellationToken);
		default:
			return SpecializedCollections.EmptyEnumerable<ISymbol>();
		}
	}

	private IEnumerable<ISymbol> GetRelatedFieldRecommendation(CancellationToken cancellationToken)
	{
		SourceReportDataItemSymbol sourceReportDataItemSymbol = base.Context.SemanticModel.GetDeclaredSymbolForNode(reportDataItemSyntax, cancellationToken) as SourceReportDataItemSymbol;
		if (sourceReportDataItemSymbol == null)
		{
			return SpecializedCollections.EmptyEnumerable<ISymbol>();
		}
		ReportDataItemSymbol dataItemLinkReferencedDataItem = sourceReportDataItemSymbol.GetDataItemLinkReferencedDataItem();
		if (dataItemLinkReferencedDataItem?.RelatedTable == null)
		{
			return SpecializedCollections.EmptyEnumerable<ISymbol>();
		}
		return from s in base.Context.SemanticModel.LookupSymbols(base.Context.LeftToken.SpanStart, LookupOptions.Default, dataItemLinkReferencedDataItem.RelatedTable, null, SymbolKind.Undefined, cancellationToken)
			where s.Kind == SymbolKind.Field
			select s;
	}

	private IEnumerable<ISymbol> GetSourceFieldRecommendation(CancellationToken cancellationToken)
	{
		return from s in base.Context.SemanticModel.LookupSymbols(base.Context.LeftToken.SpanStart, LookupOptions.Default, null, null, SymbolKind.Undefined, cancellationToken)
			where s.Kind == SymbolKind.Field
			select s;
	}
}
