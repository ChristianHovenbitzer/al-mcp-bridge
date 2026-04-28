using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Completion;

internal abstract class PropertyValuesRecommender
{
	private ObjectTypeSymbol? declaringObject;

	protected internal PropertyValueSyntax? PropertyValue { get; private set; }

	protected internal PropertyTypeInfo? PropertyTypeInfo { get; private set; }

	protected internal ObjectTypeSymbol? DeclaringObject => declaringObject ?? (declaringObject = Context.SemanticModel.GetDeclaredSymbol(Context.DeclaringObject) as ObjectTypeSymbol);

	protected internal FieldSymbol? DeclaringField
	{
		get
		{
			FieldSyntax fieldSyntax = PropertyValue?.GetAncestor<FieldSyntax>();
			if (fieldSyntax == null)
			{
				return null;
			}
			return (DeclaringObject as TableTypeSymbol)?.GetCodeMembers(fieldSyntax.Name.Identifier.ValueText).FirstOrDefault() as FieldSymbol;
		}
	}

	protected internal MemberSyntaxContext Context { get; }

	protected internal PropertyValuesRecommender? Next { get; set; }

	protected internal virtual bool IsExclusive
	{
		get
		{
			if (Next != null)
			{
				return Next.IsExclusive;
			}
			return false;
		}
	}

	protected PropertyValuesRecommender(MemberSyntaxContext context)
	{
		Context = context;
		SetCurrentState();
	}

	protected internal virtual async Task<IEnumerable<PropertyValueRecommendation>> RecommendPropertyValuesAsync(CancellationToken cancellationToken)
	{
		if (Next == null)
		{
			return SpecializedCollections.EmptyEnumerable<PropertyValueRecommendation>();
		}
		cancellationToken.ThrowIfCancellationRequested();
		if (PropertyValue == null || PropertyTypeInfo == null)
		{
			return SpecializedCollections.EmptyEnumerable<PropertyValueRecommendation>();
		}
		return await Next.RecommendPropertyValuesAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
	}

	protected string? GetInsertionTextWithQualificationIfNeeded(ISymbol symbol)
	{
		if (symbol.ContainingNamespace == null || symbol.ContainingNamespace.IsGlobalNamespace)
		{
			return null;
		}
		if (!Context.EnclosingBinder.IsSymbolInScope(symbol))
		{
			return ((Symbol)symbol).ToDisplayString(SymbolDisplayFormat.QualifiedNameOnlyFormat);
		}
		return null;
	}

	protected IEnumerable<PropertyValueRecommendation> GetPropertyValueRecommendationsFromSymbols(IEnumerable<ISymbol> symbols, Func<ISymbol, string?>? getInsertionText = null, bool matchDisplayTextToInsertionText = false)
	{
		Func<ISymbol, string?> getInsertionText2 = getInsertionText;
		return symbols.Select((ISymbol s) => PropertyValueRecommendation.Create(s, getInsertionText2, matchDisplayTextToInsertionText, s.GetGlyph(), Context.EnclosingBinder, Context.ShouldAddUsingStatementWhenCompleting));
	}

	protected static IEnumerable<PropertyValueRecommendation> GetPropertyValueRecommendationsFromEnumPropertyTypeInfo(EnumPropertyTypeInfo info, Version runtimeVersion)
	{
		EnumPropertyTypeInfo info2 = info;
		Version runtimeVersion2 = runtimeVersion;
		return info2.Options.SelectAsArrayWhere((EnumPropertyMemberInfo x) => new PropertyValueRecommendation(x.Name)
		{
			DetailText = info2.GetEnumValuePropertyDetailText(x.Name),
			DescriptionValue = info2.GetEnumValuePropertyDocumentation(x.Name),
			IsDeprecated = (x.Deprecated?.IsSupported(runtimeVersion2) ?? false)
		}, (EnumPropertyMemberInfo x) => x.Compatibility == null || x.Compatibility.IsSupported(runtimeVersion2));
	}

	private void SetPropertyValueSyntax()
	{
		PropertyValue = Microsoft.Dynamics.Nav.CodeAnalysis.Syntax.SyntaxTokenExtensions.GetAncestor<PropertySyntax>(Context.LeftToken)?.Value;
	}

	protected void SetCurrentState()
	{
		SetPropertyValueSyntax();
		if (PropertyValue != null)
		{
			PropertyTypeInfo = PropertyValue.GetPropertyTypeInfo();
		}
	}
}
