using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Completion;

internal class AttributeArgumentSymbolRecommender : ContextAwareSymbolRecommender
{
	public AttributeArgumentSymbolRecommender(MemberSyntaxContext context)
		: base(context)
	{
	}

	protected internal override Task<IEnumerable<ISymbol>> RecommendSymbolsAsync(CancellationToken cancellationToken)
	{
		if (!base.Context.General.HasFlag(GeneralContexts.AttributeArgumentList) || !base.Context.TargetToken.IsKind(SyntaxKind.OpenParenToken, SyntaxKind.CommaToken))
		{
			return base.RecommendSymbolsAsync(cancellationToken);
		}
		AttributeArgumentListSyntax attributeArgumentListSyntax = MemberSyntaxContext.FindAttributeArgumentListContext(base.Context.TargetToken);
		AttributeTypeInfo attributeTypeInfo = ((AttributeSymbol)base.Context.SemanticModel.GetDeclaredSymbol(attributeArgumentListSyntax.Parent))?.AttributeInfo;
		if (attributeTypeInfo == null || attributeTypeInfo.Kind == AttributeKind.EventSubscriber)
		{
			return base.RecommendSymbolsAsync(cancellationToken);
		}
		int num = attributeArgumentListSyntax.CalculateAttributeArgumentPosition(base.Context.Position);
		if (num < attributeTypeInfo.Parameters.Length)
		{
			AttributeParameterInfo attributeParameterInfo = attributeTypeInfo.Parameters[num];
			if (attributeParameterInfo.Type.Kind == SymbolKind.OptionType)
			{
				return Task.FromResult(SpecializedCollections.SingletonEnumerable(attributeParameterInfo.Type).Cast<ISymbol>());
			}
		}
		return base.RecommendSymbolsAsync(cancellationToken);
	}
}
