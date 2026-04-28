using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Completion;

internal class ExpressionAndStatementSymbolRecommender : ContextAwareSymbolRecommender
{
	internal ExpressionAndStatementSymbolRecommender(MemberSyntaxContext context)
		: base(context)
	{
	}

	protected internal override async Task<IEnumerable<ISymbol>> RecommendSymbolsAsync(CancellationToken cancellationToken)
	{
		if ((!base.Context.General.HasFlag(GeneralContexts.AnyExpression) && !base.Context.General.HasFlag(GeneralContexts.Statement)) || base.Context.IsRightOfNameSeparator)
		{
			return await base.RecommendSymbolsAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		}
		ImmutableArray<ISymbol> immutableArray = base.Context.SemanticModel.LookupSymbols(base.Context.LeftToken.EndPosition, LookupOptions.Default, null, null, SymbolKind.Undefined, cancellationToken);
		IEnumerable<ISymbol> result;
		if (!base.Context.General.HasFlag(GeneralContexts.AnyExpression))
		{
			IEnumerable<ISymbol> enumerable = immutableArray;
			result = enumerable;
		}
		else
		{
			result = immutableArray.Where(SemanticFacts.IsValidForExpression);
		}
		return result;
	}
}
