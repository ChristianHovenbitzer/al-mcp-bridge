using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Completion;

internal class QueryColumnFilterSymbolRecommender : ContextAwareSymbolRecommender
{
	private readonly PropertyValueSyntax propertyValue;

	internal QueryColumnFilterSymbolRecommender(MemberSyntaxContext context)
		: base(context)
	{
		PropertySyntax ancestor = Microsoft.Dynamics.Nav.CodeAnalysis.Syntax.SyntaxTokenExtensions.GetAncestor<PropertySyntax>(base.Context.TargetToken);
		if (ancestor != null)
		{
			propertyValue = ancestor.Value;
		}
	}

	protected internal override Task<IEnumerable<ISymbol>> RecommendSymbolsAsync(CancellationToken cancellationToken)
	{
		if (propertyValue == null)
		{
			return Task.FromResult(SpecializedCollections.EmptyEnumerable<ISymbol>());
		}
		if (propertyValue.Kind != SyntaxKind.TableFilterPropertyValue || !IsColumnSymbolSuggestion())
		{
			return Task.FromResult(SpecializedCollections.EmptyEnumerable<ISymbol>());
		}
		return Task.FromResult(GetRecommendation(cancellationToken));
	}

	private bool IsColumnSymbolSuggestion()
	{
		if (base.Context.LeftToken.Kind != SyntaxKind.EqualsToken)
		{
			return base.Context.LeftToken.Kind == SyntaxKind.CommaToken;
		}
		return true;
	}

	private IEnumerable<ISymbol> GetRecommendation(CancellationToken cancellationToken)
	{
		PropertySyntax ancestor = Microsoft.Dynamics.Nav.CodeAnalysis.Syntax.SyntaxTokenExtensions.GetAncestor<PropertySyntax>(base.Context.TargetToken);
		SyntaxNode parent = ancestor.Parent.Parent;
		switch (ancestor.Parent.Parent.Kind)
		{
		case SyntaxKind.QueryColumn:
		{
			string elementName2 = ((QueryColumnSyntax)parent)?.Name?.GetIdentifierOrLiteralValue();
			return GetQueryElementSymbol(SymbolKind.QueryColumn, elementName2, cancellationToken);
		}
		case SyntaxKind.QueryFilter:
		{
			string elementName = ((QueryFilterSyntax)parent)?.Name?.GetIdentifierOrLiteralValue();
			return GetQueryElementSymbol(SymbolKind.QueryFilter, elementName, cancellationToken);
		}
		default:
			return SpecializedCollections.EmptyEnumerable<ISymbol>();
		}
	}

	private IEnumerable<ISymbol> GetQueryElementSymbol(SymbolKind kind, string elementName, CancellationToken cancellationToken)
	{
		return from s in base.Context.SemanticModel.LookupSymbols(base.Context.LeftToken.SpanStart, LookupOptions.Default, null, elementName.UnquoteIdentifier(), SymbolKind.Undefined, cancellationToken)
			where s.Kind == kind
			select s;
	}
}
