using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Completion;

internal abstract class PermissionsObjectIdSymbolRecommender : ContextAwareSymbolRecommender
{
	protected abstract AttributeKind PermissionsAttributeKind { get; }

	protected internal override bool IsExclusive => true;

	public PermissionsObjectIdSymbolRecommender(MemberSyntaxContext context)
		: base(context)
	{
	}

	protected internal override Task<IEnumerable<ISymbol>> RecommendSymbolsAsync(CancellationToken cancellationToken)
	{
		if (!base.Context.General.HasFlag(GeneralContexts.AttributeArgumentList) || !base.Context.TargetToken.IsKind(SyntaxKind.ColonColonToken))
		{
			return base.RecommendSymbolsAsync(cancellationToken);
		}
		AttributeArgumentListSyntax attributeArgumentListSyntax = MemberSyntaxContext.FindAttributeArgumentListContext(base.Context.TargetToken);
		if ((((AttributeSymbol)base.Context.SemanticModel.GetDeclaredSymbol(attributeArgumentListSyntax.Parent))?.AttributeInfo)?.Kind == PermissionsAttributeKind)
		{
			SyntaxNode parent = base.Context.TargetToken.Parent;
			if (parent.Kind == SyntaxKind.OptionAccessExpression)
			{
				ExpressionSyntax expression = ((OptionAccessExpressionSyntax)parent).Expression;
				if (expression != null)
				{
					TypeInfo typeInfo = base.Context.SemanticModel.GetTypeInfo(expression, cancellationToken);
					IContainerSymbol containerSymbol = ((typeInfo != TypeInfo.None) ? typeInfo.Type : null);
					if (containerSymbol.Kind == SymbolKind.Class)
					{
						int spanStart = expression.SpanStart;
						if (containerSymbol.TryGetSymbolsFromOptionType(base.Context.SemanticModel.Compilation, spanStart, cancellationToken, out IEnumerable<ISymbol> symbols, searchAllModules: false))
						{
							return Task.FromResult(symbols);
						}
					}
				}
			}
		}
		return base.RecommendSymbolsAsync(cancellationToken);
	}
}
