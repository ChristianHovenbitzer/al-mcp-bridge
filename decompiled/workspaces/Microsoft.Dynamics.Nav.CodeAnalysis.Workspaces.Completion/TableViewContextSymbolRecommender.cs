using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Completion;

internal class TableViewContextSymbolRecommender : ContextAwareSymbolRecommender
{
	private readonly PropertySyntax propertySyntax;

	internal TableViewContextSymbolRecommender(MemberSyntaxContext context)
		: base(context)
	{
		propertySyntax = Microsoft.Dynamics.Nav.CodeAnalysis.Syntax.SyntaxTokenExtensions.GetAncestor<PropertySyntax>(base.Context.TargetToken);
	}

	protected internal override async Task<IEnumerable<ISymbol>> RecommendSymbolsAsync(CancellationToken cancellationToken)
	{
		if (base.Context.DeclaringObject.IsKind(SyntaxKind.ReportObject, SyntaxKind.ReportExtension, SyntaxKind.XmlPortObject))
		{
			PropertySyntax obj = propertySyntax;
			if (obj != null && obj.Value?.Kind == SyntaxKind.TableViewPropertyValue)
			{
				if (((TableViewPropertyValueSyntax)propertySyntax.Value).TableFilter != null)
				{
					return GetRecommendation(cancellationToken);
				}
				return await base.RecommendSymbolsAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
			}
		}
		return await base.RecommendSymbolsAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
	}

	private IEnumerable<ISymbol> GetRecommendation(CancellationToken cancellationToken)
	{
		ImmutableArray<FieldSymbol>? immutableArray = base.Context.SemanticModel.GetDeclaredSymbol(propertySyntax)?.ContainingType?.GetRelatedTableSymbol().GetFields();
		if (!immutableArray.HasValue)
		{
			return Enumerable.Empty<ISymbol>();
		}
		return immutableArray.GetValueOrDefault();
	}
}
