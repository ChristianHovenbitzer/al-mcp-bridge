using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Completion;

internal class ImageValuesRecommender : PropertyValuesRecommender
{
	private enum ImageTarget
	{
		Unknown,
		Field,
		Action,
		ActionGroup
	}

	protected internal override bool IsExclusive => true;

	internal ImageValuesRecommender(MemberSyntaxContext context)
		: base(context)
	{
		base.Next = new ObjectReferencePropertyValuesRecommender(context);
	}

	protected internal override async Task<IEnumerable<PropertyValueRecommendation>> RecommendPropertyValuesAsync(CancellationToken cancellationToken)
	{
		if (base.PropertyTypeInfo == null || base.PropertyValue == null)
		{
			return SpecializedCollections.EmptyEnumerable<PropertyValueRecommendation>();
		}
		if (base.PropertyTypeInfo.Kind != PropertyKind.Image)
		{
			return await base.RecommendPropertyValuesAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		}
		ImageTarget kind = ImageTarget.Unknown;
		SyntaxNode syntaxNode = FindDeclaringParent(base.Context.LeftToken.GetPreviousToken().Parent, ref kind);
		switch (kind)
		{
		case ImageTarget.Action:
		{
			ActionSymbol actionSymbol = null;
			if (syntaxNode is PageActionSyntax syntax)
			{
				actionSymbol = base.Context.SemanticModel.GetDeclaredSymbol(syntax);
			}
			else if (syntaxNode is PageFileUploadActionSyntax syntax2)
			{
				actionSymbol = base.Context.SemanticModel.GetDeclaredSymbol(syntax2);
			}
			if (actionSymbol == null)
			{
				throw ExceptionUtilities.Unreachable;
			}
			if (actionSymbol.IsCueAction())
			{
				return CreateImagePropertyRecommendation(ImageResources.GetActionCueGroupImageResources());
			}
			if (actionSymbol.GetContainingActionAreaKind() == ActionAreaKind.Prompting)
			{
				return CreateImagePropertyRecommendation(GetPromptActionImageResources());
			}
			return CreateImagePropertyRecommendation(ImageResources.GetActionImageResources());
		}
		case ImageTarget.ActionGroup:
		{
			ActionSymbol declaredSymbol = base.Context.SemanticModel.GetDeclaredSymbol((PageActionGroupSyntax)syntaxNode);
			if (PageMemberSymbolHelpers.IsDirectlyContainedInGivenActionArea(declaredSymbol, ActionAreaKind.Sections))
			{
				return CreateImagePropertyRecommendation(ImageResources.GetRoleCenterActionGroupImageResources());
			}
			if (!PageMemberSymbolHelpers.IsContainedInGroupInGivenActionArea(declaredSymbol, ActionAreaKind.Sections))
			{
				return CreateImagePropertyRecommendation(ImageResources.GetActionImageResources());
			}
			break;
		}
		case ImageTarget.Field:
			if (base.Context.SemanticModel.GetDeclaredSymbol((PageFieldSyntax)syntaxNode).IsCueControl())
			{
				return CreateImagePropertyRecommendation(ImageResources.GetFieldCueGroupImageResources());
			}
			break;
		}
		return await base.RecommendPropertyValuesAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
	}

	private static SyntaxNode FindDeclaringParent(SyntaxNode objectSyntax, ref ImageTarget kind)
	{
		if (objectSyntax == null)
		{
			return null;
		}
		if (IsValidDeclaringType(objectSyntax, ref kind))
		{
			return objectSyntax;
		}
		return FindDeclaringParent(objectSyntax.Parent, ref kind);
	}

	private static bool IsValidDeclaringType(SyntaxNode syntaxNode, ref ImageTarget kind)
	{
		switch (syntaxNode.Kind)
		{
		case SyntaxKind.PageAction:
		case SyntaxKind.PageFileUploadAction:
			kind = ImageTarget.Action;
			return true;
		case SyntaxKind.PageActionGroup:
			kind = ImageTarget.ActionGroup;
			return true;
		case SyntaxKind.PageField:
			kind = ImageTarget.Field;
			return true;
		default:
			return false;
		}
	}

	private static IEnumerable<PropertyValueRecommendation> CreateImagePropertyRecommendation(IDictionary<string, string> resource)
	{
		List<PropertyValueRecommendation> list = new List<PropertyValueRecommendation>();
		foreach (KeyValuePair<string, string> item in resource)
		{
			string key = item.Key;
			string text = ((item.Value != null) ? Uri.UnescapeDataString(item.Value) : string.Empty);
			DebugAssertHelper.Assert(!string.IsNullOrEmpty(text));
			list.Add(new PropertyValueRecommendation(key)
			{
				DetailText = key + " Image",
				DocumentationText = MarkdownHelper.CreateBase64Image(key, text),
				IsMarkdownDocs = true
			});
		}
		return list;
	}

	private static IDictionary<string, string> GetPromptActionImageResources()
	{
		IDictionary<string, string> imageResources = ImageResources.GetActionImageResources();
		return new List<string> { "Sparkle", "SparkleFilled" }.ToDictionary((string key) => key, (string key) => imageResources[key]);
	}
}
