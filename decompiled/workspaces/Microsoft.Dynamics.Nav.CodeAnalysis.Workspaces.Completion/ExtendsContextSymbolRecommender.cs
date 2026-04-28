using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Completion;

internal class ExtendsContextSymbolRecommender : AbstractQualifiedObjectTypeRecommender
{
	internal ExtendsContextSymbolRecommender(MemberSyntaxContext context)
		: base(context)
	{
	}

	protected internal override async Task<IEnumerable<ISymbol>> RecommendSymbolsAsync(CancellationToken cancellationToken)
	{
		if (!base.Context.General.HasFlag(GeneralContexts.Extends))
		{
			return await base.RecommendSymbolsAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		}
		return from s in base.Context.LookupSymbols(GetTargetSymbolKind(), cancellationToken)
			where (s.Kind.IsExtensibleObject() && ((ApplicationObjectTypeSymbol)s).IsExtensible) || s.Kind == SymbolKind.Namespace
			select s;
	}

	private SymbolKind GetTargetSymbolKind()
	{
		SyntaxNode syntaxNode = base.Context.DeclaringObject;
		if (syntaxNode != null)
		{
			SymbolKind symbolKind = syntaxNode.Kind.ToSymbolKind();
			if (symbolKind.IsExtensionOrCustomizationObject())
			{
				SymbolKind baseSymbolKind = symbolKind.GetBaseSymbolKind();
				if (symbolKind != baseSymbolKind)
				{
					return baseSymbolKind;
				}
			}
		}
		return SymbolKind.Undefined;
	}
}
