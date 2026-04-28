using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Completion;

internal class ReportExtensionDataItemChangeAnchorSymbolRecommender : ContextAwareSymbolRecommender
{
	internal ReportExtensionDataItemChangeAnchorSymbolRecommender(MemberSyntaxContext context)
		: base(context)
	{
	}

	protected internal override async Task<IEnumerable<ISymbol>> RecommendSymbolsAsync(CancellationToken cancellationToken)
	{
		if (base.DeclaringObject == null || !base.DeclaringObject.IsKind(SymbolKind.ReportExtension))
		{
			return SpecializedCollections.EmptyEnumerable<ISymbol>();
		}
		SyntaxToken previousToken = base.Context.LeftToken.GetPreviousToken();
		if (!previousToken.Kind.IsReportDatasetChangeKeyword())
		{
			return SpecializedCollections.EmptyEnumerable<ISymbol>();
		}
		ReportTypeSymbol reportTypeSymbol = (ReportTypeSymbol)((base.DeclaringObject as ReportExtensionTypeSymbol)?.Target);
		if (reportTypeSymbol == null)
		{
			return SpecializedCollections.EmptyEnumerable<ISymbol>();
		}
		bool includeColumns = previousToken.IsKind(SyntaxKind.ModifyKeyword);
		return GetDatasetMembers(previousToken, cancellationToken, reportTypeSymbol, includeColumns);
	}

	private IEnumerable<ISymbol> GetDatasetMembers(SyntaxToken token, CancellationToken cancellationToken, ReportTypeSymbol target, bool includeColumns)
	{
		ArrayBuilder<ISymbol> arrayBuilder = new ArrayBuilder<ISymbol>();
		try
		{
			ImmutableArray<ISymbol>.Enumerator enumerator = base.Context.SemanticModel.LookupSymbols(token.SpanStart, LookupOptions.MustBeReportDataItemElement, target, null, SymbolKind.Undefined, cancellationToken).GetEnumerator();
			while (enumerator.MoveNext())
			{
				ISymbol current = enumerator.Current;
				if ((includeColumns && current.IsKind(SymbolKind.ReportDataItem, SymbolKind.ReportColumn)) || current.IsKind(SymbolKind.ReportDataItem))
				{
					arrayBuilder.Add(current);
				}
			}
			return arrayBuilder.ToImmutable();
		}
		finally
		{
			arrayBuilder.Free();
		}
	}
}
