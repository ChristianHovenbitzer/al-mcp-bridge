using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Completion;

internal class NamespaceContextSymbolRecommender : ContextAwareSymbolRecommender
{
	internal NamespaceContextSymbolRecommender(MemberSyntaxContext context)
		: base(context)
	{
	}

	protected internal override async Task<IEnumerable<ISymbol>> RecommendSymbolsAsync(CancellationToken cancellationToken)
	{
		if (!base.Context.General.HasFlag(GeneralContexts.Namespace))
		{
			return await base.RecommendSymbolsAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		}
		if (base.Context.IsRightOfDot && base.Context.LeftToken.Parent is QualifiedNameSyntax qualifiedNameSyntax)
		{
			NamespaceSymbol nestedNamespace = base.Context.SemanticModel.Compilation.GlobalNamespace.GetNestedNamespace(qualifiedNameSyntax.Left);
			if (nestedNamespace == null)
			{
				return SpecializedCollections.EmptyEnumerable<ISymbol>();
			}
			return base.Context.SemanticModel.LookupSymbols(base.Context.LeftToken.SpanStart, LookupOptions.MustBeNamespaceSymbol, nestedNamespace, null, SymbolKind.Namespace, cancellationToken);
		}
		ImmutableArray<ISymbol> immutableArray = base.Context.SemanticModel.LookupSymbols(base.Context.LeftToken.SpanStart, LookupOptions.MustBeNamespaceSymbol, null, null, SymbolKind.Namespace, cancellationToken);
		if (base.Context.TargetToken.Kind == SyntaxKind.NamespaceKeyword)
		{
			string publisher = base.Context.SemanticModel.Compilation.ModuleInfo.Publisher;
			return immutableArray.WhereAsArray((ISymbol n) => n.ContainingModule?.Publisher == publisher);
		}
		return immutableArray;
	}
}
