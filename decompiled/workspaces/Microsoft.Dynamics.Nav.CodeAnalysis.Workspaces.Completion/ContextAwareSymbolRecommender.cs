using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Completion;

internal abstract class ContextAwareSymbolRecommender
{
	private ObjectTypeSymbol? declaringObject;

	protected internal MemberSyntaxContext Context { get; }

	protected ObjectTypeSymbol DeclaringObject => declaringObject ?? (declaringObject = Context.SemanticModel.GetDeclaredSymbol(Context.DeclaringObject) as ObjectTypeSymbol);

	protected internal virtual bool IsExclusive => false;

	protected ContextAwareSymbolRecommender(MemberSyntaxContext context)
	{
		Context = context;
	}

	protected internal virtual Task<IEnumerable<ISymbol>> RecommendSymbolsAsync(CancellationToken cancellationToken)
	{
		return Task.FromResult(SpecializedCollections.EmptyEnumerable<ISymbol>());
	}

	protected void GetMembersAndGroupsForPageChange(SyntaxToken token, ObjectTypeSymbol containingObject, CancellationToken cancellationToken, out IEnumerable<Symbol> members, out IEnumerable<Symbol> groupMembers, out IEnumerable<Symbol> areaMembers)
	{
		TypeSymbol targetPage = GetTargetPage();
		GetMembersAndGroupsForPageTarget(token, containingObject, targetPage, cancellationToken, out members, out groupMembers, out areaMembers);
	}

	private TypeSymbol? GetTargetPage()
	{
		if (Context.Page.HasFlag(PageContexts.View) && DeclaringObject.Kind == SymbolKind.Page)
		{
			return DeclaringObject;
		}
		ObjectTypeSymbol objectTypeSymbol = declaringObject;
		if (!(objectTypeSymbol is PageExtensionBaseTypeSymbol pageExtensionBaseTypeSymbol))
		{
			if (objectTypeSymbol is RequestPageExtensionTypeSymbol requestPageExtensionTypeSymbol)
			{
				return requestPageExtensionTypeSymbol?.TargetPage;
			}
			return null;
		}
		return pageExtensionBaseTypeSymbol?.Target;
	}

	private void GetMembersAndGroupsForPageTarget(SyntaxToken token, ObjectTypeSymbol containingObject, TypeSymbol? targetPage, CancellationToken cancellationToken, out IEnumerable<Symbol> members, out IEnumerable<Symbol> groupMembers, out IEnumerable<Symbol> areaMembers)
	{
		ObjectTypeSymbol containingObject2 = containingObject;
		if (targetPage == null)
		{
			members = SpecializedCollections.EmptyEnumerable<Symbol>();
			groupMembers = SpecializedCollections.EmptyEnumerable<Symbol>();
			areaMembers = SpecializedCollections.EmptyEnumerable<Symbol>();
		}
		else if (token.Parent.GetFirstParent(SyntaxKind.PageExtensionLayout) != null || token.Parent.GetFirstParent(SyntaxKind.RequestPageExtension) != null)
		{
			BuildMembersGroupsAndAreas(token, targetPage, cancellationToken, SymbolKind.Control, (ControlSymbol x) => x.ControlKind.IsGroup(), (ControlSymbol x) => x.ControlKind == ControlKind.Area, (ControlSymbol x) => ShouldExcludeSameModuleSymbolReference(containingObject2, x), out members, out groupMembers, out areaMembers);
		}
		else if (token.Parent.GetFirstParent(SyntaxKind.PageExtensionActionList) != null)
		{
			bool canUseActionV2 = !PageMemberSymbolHelpers.AreActionsV1Used(DeclaringObject.DeclaringSyntaxNode) && PageMemberSymbolHelpers.AreActionsV2Supported(DeclaringObject.DeclaringSyntaxNode);
			ActionAreaKind? changeAnchorAreaKind = GetChangeActionAreKind(token);
			BuildMembersGroupsAndAreas(token, targetPage, cancellationToken, SymbolKind.Action, (ActionSymbol x) => x.ActionKind.IsGroup(), (ActionSymbol x) => x.ActionKind == ActionKind.Area, (ActionSymbol x) => ShouldExcludeActionSymbols(x, changeAnchorAreaKind, canUseActionV2) || ShouldExcludeSameModuleSymbolReference(containingObject2, x), out members, out groupMembers, out areaMembers);
		}
		else if (token.Parent.GetFirstParent(SyntaxKind.PageExtensionViewList) != null)
		{
			BuildMembersGroupsAndAreas(token, targetPage, cancellationToken, SymbolKind.View, (ViewSymbol _) => false, (ViewSymbol _) => false, (ViewSymbol x) => ShouldExcludeSameModuleSymbolReference(containingObject2, x), out members, out groupMembers, out areaMembers);
		}
		else if (token.Parent.GetFirstParent(SyntaxKind.PageExtensionAnalysisViewList) != null)
		{
			BuildMembersGroupsAndAreas(token, targetPage, cancellationToken, SymbolKind.AnalysisView, (AnalysisViewSymbol _) => false, (AnalysisViewSymbol _) => false, (AnalysisViewSymbol x) => ShouldExcludeSameModuleSymbolReference(containingObject2, x), out members, out groupMembers, out areaMembers);
		}
		else
		{
			DebugAssertHelper.Fail("Couldn't resolve the member and groups for page extensions / customizations.");
			members = SpecializedCollections.EmptyEnumerable<Symbol>();
			groupMembers = SpecializedCollections.EmptyEnumerable<Symbol>();
			areaMembers = SpecializedCollections.EmptyEnumerable<Symbol>();
		}
	}

	private ActionAreaKind? GetChangeActionAreKind(SyntaxToken token)
	{
		SyntaxNode parentOfType = GetParentOfType(token, SyntaxKind.ActionMoveChange);
		if (parentOfType != null && Context.SemanticModel.GetDeclaredSymbol(parentOfType) is ChangeMoveSymbol changeMoveSymbol && (changeMoveSymbol.Anchor?.Kind).GetValueOrDefault() == SymbolKind.Action)
		{
			return ((ActionSymbol)changeMoveSymbol.Anchor).GetContainingActionAreaKind();
		}
		SyntaxNode parentOfType2 = GetParentOfType(token, SyntaxKind.ActionAddChange);
		if (parentOfType2 != null && Context.SemanticModel.GetDeclaredSymbol(parentOfType2) is ChangeAddSymbol changeAddSymbol && (changeAddSymbol.Anchor?.Kind).GetValueOrDefault() == SymbolKind.Action)
		{
			return ((ActionSymbol)changeAddSymbol.Anchor).GetContainingActionAreaKind();
		}
		SyntaxNode parentOfType3 = GetParentOfType(token, SyntaxKind.ActionModifyChange);
		if (parentOfType3 != null && Context.SemanticModel.GetDeclaredSymbol(parentOfType3) is ChangeModifySymbol changeModifySymbol && (changeModifySymbol.Target?.Kind).GetValueOrDefault() == SymbolKind.Action)
		{
			return ((ActionSymbol)changeModifySymbol.Target).GetContainingActionAreaKind();
		}
		return null;
	}

	private static SyntaxNode? GetParentOfType(SyntaxToken token, SyntaxKind parentKind)
	{
		if (!token.ParentIsKind(parentKind))
		{
			return token.Parent.GetFirstParent(parentKind);
		}
		return token.Parent;
	}

	private static bool ShouldExcludeActionSymbols(ActionSymbol candidate, ActionAreaKind? changeAnchorAreaKind, bool canUseActionV2)
	{
		ActionAreaKind? actionAreaKind = candidate.GetActionAreaKind();
		if (actionAreaKind.HasValue && actionAreaKind.Value.IsActionsV2ActionArea() && !canUseActionV2)
		{
			return true;
		}
		actionAreaKind = candidate.GetContainingActionAreaKind();
		if (!actionAreaKind.HasValue)
		{
			return false;
		}
		if (actionAreaKind.Value.IsActionsV2ActionArea() && !canUseActionV2)
		{
			return true;
		}
		if (!changeAnchorAreaKind.HasValue)
		{
			return false;
		}
		if (actionAreaKind.Value.IsActionsV2ActionArea() != changeAnchorAreaKind.Value.IsActionsV2ActionArea())
		{
			return true;
		}
		return false;
	}

	private void BuildMembersGroupsAndAreas<T>(SyntaxToken token, TypeSymbol target, CancellationToken cancellationToken, SymbolKind kind, Func<T, bool> isGroup, Func<T, bool> isArea, Func<T, bool> excludeSymbol, out IEnumerable<Symbol> members, out IEnumerable<Symbol> groupMembers, out IEnumerable<Symbol> areaMembers) where T : Symbol
	{
		ArrayBuilder<Symbol> arrayBuilder = new ArrayBuilder<Symbol>();
		ArrayBuilder<Symbol> arrayBuilder2 = new ArrayBuilder<Symbol>();
		ArrayBuilder<Symbol> arrayBuilder3 = new ArrayBuilder<Symbol>();
		try
		{
			ImmutableArray<ISymbol>.Enumerator enumerator = Context.SemanticModel.LookupSymbols(token.SpanStart, LookupOptions.MustBePageElement, target, null, SymbolKind.Undefined, cancellationToken).GetEnumerator();
			while (enumerator.MoveNext())
			{
				ISymbol current = enumerator.Current;
				if (!current.IsKind(kind))
				{
					continue;
				}
				T val = (T)current;
				if (excludeSymbol(val))
				{
					continue;
				}
				arrayBuilder.Add(val);
				if (isGroup(val))
				{
					if (isArea(val))
					{
						arrayBuilder3.Add(val);
					}
					else
					{
						arrayBuilder2.Add(val);
					}
				}
			}
			members = arrayBuilder.ToImmutable();
			groupMembers = arrayBuilder2.ToImmutable();
			areaMembers = arrayBuilder3.ToImmutable();
		}
		finally
		{
			arrayBuilder.Free();
			arrayBuilder2.Free();
			arrayBuilder3.Free();
		}
	}

	protected TableTypeSymbol GetRelatedTableSymbol(PropertyValueSyntax propertyValue)
	{
		return PropertyRecommendationHelper.GetRelatedTableSymbol(propertyValue, Context.SemanticModel.Compilation, DeclaringObject);
	}

	protected bool IsAssigment(bool hasFlagAnyExpression, bool hasFlagStatement, bool isRightOfNameSeparator)
	{
		SyntaxNode parent = Context.TargetToken.Parent;
		bool flag = parent != null && parent.Kind == SyntaxKind.AssignmentStatement;
		return !((!hasFlagAnyExpression && !hasFlagStatement) || isRightOfNameSeparator) && flag;
	}

	protected bool IsCaseStatement(bool hasFlagAnyExpression, bool hasFlagStatement, bool isRightOfNameSeparator)
	{
		SyntaxNode parent = Context.TargetToken.Parent;
		bool flag = parent != null && parent.Kind == SyntaxKind.CaseStatement;
		return !((!hasFlagAnyExpression && hasFlagStatement) || isRightOfNameSeparator) && flag;
	}

	protected bool ShouldExcludeSameModuleSymbolReference(IObjectTypeSymbol containingSymbol, ISymbol candidate)
	{
		return RecommendationHelper.ShouldExcludeSameModuleSymbolReference(containingSymbol, candidate);
	}
}
