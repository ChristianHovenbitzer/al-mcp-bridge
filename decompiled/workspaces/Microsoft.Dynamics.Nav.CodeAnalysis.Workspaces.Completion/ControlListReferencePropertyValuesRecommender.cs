using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Completion;

internal class ControlListReferencePropertyValuesRecommender : PropertyValuesRecommender
{
	protected internal override bool IsExclusive => true;

	internal ControlListReferencePropertyValuesRecommender(MemberSyntaxContext context)
		: base(context)
	{
		base.Next = new PermissionSetPropertyValuesRecommender(context);
	}

	protected internal override async Task<IEnumerable<PropertyValueRecommendation>> RecommendPropertyValuesAsync(CancellationToken cancellationToken)
	{
		if (base.PropertyTypeInfo == null || base.PropertyValue == null)
		{
			return SpecializedCollections.EmptyEnumerable<PropertyValueRecommendation>();
		}
		bool num = base.PropertyValue.IsKind(SyntaxKind.CommaSeparatedPropertyValue) && string.Equals(base.PropertyTypeInfo.ValueKind, SymbolKind.Control.ToString(), StringComparison.OrdinalIgnoreCase);
		bool flag = base.PropertyValue.IsKind(SyntaxKind.PageFieldReferencePropertyValue);
		if (!num && !flag)
		{
			return await base.RecommendPropertyValuesAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		}
		IEnumerable<ISymbol> symbols = SpecializedCollections.EmptyEnumerable<ISymbol>();
		if (base.PropertyTypeInfo.Kind == PropertyKind.IndentationControls || base.PropertyTypeInfo.Kind == PropertyKind.FreezeColumn)
		{
			symbols = FindRepeaterControls();
		}
		else if (base.PropertyTypeInfo.Kind == PropertyKind.Provider)
		{
			symbols = FindParts();
		}
		return GetPropertyValueRecommendationsFromSymbols(symbols);
	}

	private IEnumerable<ISymbol> FindRepeaterControls()
	{
		ISymbol declaredSymbol = base.Context.SemanticModel.GetDeclaredSymbol(base.PropertyValue.Parent);
		DebugAssertHelper.Assert(declaredSymbol != null);
		if (declaredSymbol == null)
		{
			return SpecializedCollections.EmptyEnumerable<ISymbol>();
		}
		ControlSymbol repeater = declaredSymbol.ContainingSymbol.GetExtendedPageControlOrSelf();
		ControlSymbol controlSymbol = repeater;
		if ((object)controlSymbol == null || controlSymbol.ControlKind != ControlKind.Repeater)
		{
			return SpecializedCollections.EmptyEnumerable<ISymbol>();
		}
		if (base.Context.DeclaringObject.IsKind(SyntaxKind.PageObject))
		{
			return repeater.Controls;
		}
		return from x in base.Context.SemanticModel.LookupSymbols(base.Context.Position, LookupOptions.MustBePageElement)
			where x.GetContainingSymbol() == repeater
			select x;
	}

	private IEnumerable<ISymbol> FindParts()
	{
		IPageBaseTypeSymbol containingPage = base.Context.SemanticModel.GetDeclaredSymbol(base.PropertyValue.Parent).ContainingType.GetContainingPage();
		IEnumerable<IControlSymbol> source = (IEnumerable<IControlSymbol>)((!base.Context.DeclaringObject.IsKind(SyntaxKind.PageObject)) ? base.Context.SemanticModel.LookupSymbols(base.Context.Position, LookupOptions.MustBePageElement).OfType<IControlSymbol>() : ((object)containingPage.FlattenedControls));
		return source.Where((IControlSymbol x) => x.ControlKind == ControlKind.Part);
	}
}
