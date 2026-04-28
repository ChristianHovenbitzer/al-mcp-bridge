using System.Collections.Generic;
using System.Threading;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Completion;

internal abstract class AbstractQualifiedObjectTypeRecommender : ContextAwareSymbolRecommender
{
	internal AbstractQualifiedObjectTypeRecommender(MemberSyntaxContext context)
		: base(context)
	{
	}

	protected internal IEnumerable<ISymbol> LookupSymbols(SymbolKind? kind, CancellationToken cancellationToken)
	{
		if (!kind.HasValue)
		{
			return SpecializedCollections.EmptyEnumerable<ISymbol>();
		}
		if (kind.GetValueOrDefault() == SymbolKind.Undefined)
		{
			return base.Context.SemanticModel.LookupSymbols(base.Context.LeftToken.SpanStart, LookupOptions.Default, null, null, SymbolKind.Undefined, cancellationToken);
		}
		if (base.Context.IsRightOfDot && base.Context.LeftToken.Parent is QualifiedNameSyntax qualifiedNameSyntax)
		{
			NamespaceSymbol nestedNamespace = base.Context.SemanticModel.Compilation.GlobalNamespace.GetNestedNamespace(qualifiedNameSyntax.Left);
			if (nestedNamespace == null)
			{
				return SpecializedCollections.EmptyEnumerable<ISymbol>();
			}
			return base.Context.SemanticModel.LookupSymbols(base.Context.LeftToken.SpanStart, LookupOptions.MustBeObjectTypeOrNamespaceSymbol, nestedNamespace, null, kind.Value, cancellationToken);
		}
		return base.Context.SemanticModel.LookupSymbols(base.Context.LeftToken.SpanStart, LookupOptions.MustBeObjectTypeOrNamespaceSymbol, null, null, kind.Value, cancellationToken);
	}
}
