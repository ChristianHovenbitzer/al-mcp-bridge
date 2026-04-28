using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Completion;

internal class ExternalBusinessEventCategoryRecommender : TextRecommender
{
	protected internal override async Task<IEnumerable<CompletionItem>> RecommendTextAsync(CompletionContext context, MemberSyntaxContext syntaxContext, CancellationToken cancellationToken)
	{
		if (!IsEligablePlace(syntaxContext))
		{
			return await base.RecommendTextAsync(context, syntaxContext, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		}
		AttributeArgumentListSyntax attributeArgumentListSyntax = MemberSyntaxContext.FindAttributeArgumentListContext(syntaxContext.TargetToken);
		AttributeSymbol obj = (AttributeSymbol)syntaxContext.SemanticModel.GetDeclaredSymbol(attributeArgumentListSyntax.Parent);
		if ((object)obj == null || (obj.AttributeInfo?.Kind).GetValueOrDefault() != AttributeKind.ExternalBusinessEvent)
		{
			return await base.RecommendTextAsync(context, syntaxContext, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		}
		if (attributeArgumentListSyntax.CalculateAttributeArgumentPosition(syntaxContext.Position) == 3)
		{
			EnumTypeSymbol enumTypeSymbol = syntaxContext.SemanticModel.Compilation.CompiledModule.GetObjectSymbolsByNameAcrossModules(SymbolKind.Enum, "EventCategory").FirstOrDefault() as EnumTypeSymbol;
			if (enumTypeSymbol == null)
			{
				return await base.RecommendTextAsync(context, syntaxContext, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
			}
			List<CompletionItem> list = new List<CompletionItem>();
			ImmutableArray<EnumValueSymbol>.Enumerator enumerator = syntaxContext.SemanticModel.Compilation.GetEnumValues(enumTypeSymbol).GetEnumerator();
			while (enumerator.MoveNext())
			{
				EnumValueSymbol current = enumerator.Current;
				string metadataName = current.MetadataName;
				string enumMemberAccessExpression = GetEnumMemberAccessExpression(current);
				list.Add(TextRecommender.CreateItem(metadataName, Glyph.Option, default(TextSpan), null, enumMemberAccessExpression));
			}
			return list;
		}
		return await base.RecommendTextAsync(context, syntaxContext, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
	}

	private static bool IsEligablePlace(MemberSyntaxContext syntaxContext)
	{
		if (syntaxContext.General.HasFlag(GeneralContexts.AttributeArgumentList))
		{
			return syntaxContext.TargetToken.IsKind(SyntaxKind.OpenParenToken, SyntaxKind.CommaToken, SyntaxKind.ColonColonToken);
		}
		return false;
	}

	private static string GetEnumMemberAccessExpression(EnumValueSymbol value)
	{
		return "EventCategory::" + value.MetadataName;
	}
}
