using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Completion;

internal class FieldModifySymbolRecommmender : ContextAwareSymbolRecommender
{
	internal FieldModifySymbolRecommmender(MemberSyntaxContext context)
		: base(context)
	{
	}

	protected internal override async Task<IEnumerable<ISymbol>> RecommendSymbolsAsync(CancellationToken cancellationToken)
	{
		if (!base.Context.Table.HasFlag(TableContexts.ModifyContext))
		{
			return await base.RecommendSymbolsAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		}
		if (base.DeclaringObject == null || base.DeclaringObject.Kind != SymbolKind.TableExtension)
		{
			return SpecializedCollections.EmptyEnumerable<ISymbol>();
		}
		TableExtensionTypeSymbol tableExtension = (TableExtensionTypeSymbol)base.DeclaringObject;
		TableTypeSymbol tableTypeSymbol = (TableTypeSymbol)tableExtension.Target;
		if (tableTypeSymbol == null)
		{
			return SpecializedCollections.EmptyEnumerable<ISymbol>();
		}
		SyntaxToken previousToken = base.Context.LeftToken.GetPreviousToken();
		int position = (tableTypeSymbol.IsSourceSymbol() ? tableTypeSymbol.GetLocation().SourceSpan.Start : 0);
		Enumerable.Empty<ISymbol>();
		if (previousToken.Parent.GetFirstParent(SyntaxKind.FieldGroupExtensionList) != null)
		{
			return tableTypeSymbol.GetFieldGroups();
		}
		return from s in base.Context.SemanticModel.LookupSymbols(position, LookupOptions.Default, tableTypeSymbol, null, SymbolKind.Undefined, cancellationToken)
			where s.IsKind(SymbolKind.Field) && tableExtension != s.ContainingSymbol && !ShouldExcludeSameModuleSymbolReference(tableExtension, s)
			select s;
	}
}
