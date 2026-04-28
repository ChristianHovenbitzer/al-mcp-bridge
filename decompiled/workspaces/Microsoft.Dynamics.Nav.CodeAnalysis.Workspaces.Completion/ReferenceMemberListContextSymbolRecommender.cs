using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Completion;

internal class ReferenceMemberListContextSymbolRecommender : ContextAwareSymbolRecommender
{
	internal ReferenceMemberListContextSymbolRecommender(MemberSyntaxContext context)
		: base(context)
	{
	}

	protected internal override async Task<IEnumerable<ISymbol>> RecommendSymbolsAsync(CancellationToken cancellationToken)
	{
		if (!base.Context.General.HasFlag(GeneralContexts.ReferenceMemberList))
		{
			return await base.RecommendSymbolsAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		}
		if (base.DeclaringObject == null)
		{
			return SpecializedCollections.EmptyEnumerable<ISymbol>();
		}
		SyntaxToken tokenBefore = base.Context.SyntaxTree.GetTokenBefore(base.Context.Position, cancellationToken);
		if (tokenBefore.Parent.Kind.IsMoveChange())
		{
			GetMembersAndGroupsForPageChange(tokenBefore, base.DeclaringObject, cancellationToken, out IEnumerable<Symbol> members, out IEnumerable<Symbol> _, out IEnumerable<Symbol> areaMembers);
			return Enumerable.Except(members, areaMembers);
		}
		if (tokenBefore.ParentIsKind(SyntaxKind.Key))
		{
			return RecommendationHelper.RecommendFieldSymbolsForKey(base.Context, base.DeclaringObject, cancellationToken);
		}
		if (tokenBefore.ParentIsKind(SyntaxKind.FieldGroup) || tokenBefore.ParentIsKind(SyntaxKind.FieldGroupAddChange))
		{
			return RecommendationHelper.RecommendFieldSymbolsForFieldGroup(base.Context, base.DeclaringObject, cancellationToken);
		}
		return SpecializedCollections.EmptyEnumerable<ISymbol>();
	}
}
