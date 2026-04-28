using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Completion;

internal class InterfaceImplementationRecommender : PropertyValuesRecommender
{
	protected internal override bool IsExclusive => true;

	internal InterfaceImplementationRecommender(MemberSyntaxContext context)
		: base(context)
	{
		base.Next = new ReportDataItemLinkReferenceRecommender(context);
	}

	protected internal override async Task<IEnumerable<PropertyValueRecommendation>> RecommendPropertyValuesAsync(CancellationToken cancellationToken)
	{
		if (base.PropertyTypeInfo == null || base.PropertyValue == null)
		{
			return SpecializedCollections.EmptyEnumerable<PropertyValueRecommendation>();
		}
		if (base.PropertyTypeInfo.Kind != PropertyKind.DefaultImplementation && base.PropertyTypeInfo.Kind != PropertyKind.UnknownValueImplementation && base.PropertyTypeInfo.Kind != PropertyKind.Implementation)
		{
			return await base.RecommendPropertyValuesAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		}
		SpecializedCollections.EmptyEnumerable<ISymbol>();
		bool flag = base.Context.LeftToken.IsKind(SyntaxKind.EqualsToken) && base.Context.LeftToken.Parent.IsKind(SyntaxKind.Property);
		bool flag2 = base.Context.LeftToken.IsKind(SyntaxKind.CommaToken) && base.Context.LeftToken.Parent.IsKind(SyntaxKind.CommaSeparatedIdentifierEqualsIdentifierList);
		if (!base.Context.LeftToken.TryGetAncestorOfKind<IdentifierEqualsIdentifierSyntax>(SyntaxKind.IdentifierEqualsIdentifier, out IdentifierEqualsIdentifierSyntax ancestor) && !flag && !flag2)
		{
			return SpecializedCollections.EmptyEnumerable<PropertyValueRecommendation>();
		}
		NamespaceSymbol namespaceSymbol = null;
		if (ancestor != null)
		{
			NameSyntax nameSyntax = (ancestor.LeftIdentifier.FullSpan.Contains(base.Context.LeftToken.Position) ? ancestor.LeftIdentifier : ancestor.RightIdentifier);
			if (nameSyntax.IsKind(SyntaxKind.QualifiedName))
			{
				namespaceSymbol = base.Context.EnclosingBinder.BindNamespaceOrType(((QualifiedNameSyntax)nameSyntax).Left, SymbolKind.Namespace, null) as NamespaceSymbol;
			}
		}
		IEnumerable<ISymbol> symbols;
		if (flag || flag2 || (ancestor != null && ancestor.LeftIdentifier.Span.Contains(base.Context.Position)))
		{
			symbols = FindInterfaces(namespaceSymbol);
		}
		else
		{
			if (ancestor?.RightIdentifier == null || !(ancestor?.EqualsToken.Span.End <= base.Context.Position))
			{
				return SpecializedCollections.EmptyEnumerable<PropertyValueRecommendation>();
			}
			InterfaceTypeSymbol interfaceTypeSymbol = null;
			if (ancestor?.LeftIdentifier != null)
			{
				interfaceTypeSymbol = base.Context.EnclosingBinder.LookupNamespaceOrTypeSymbol(ancestor.LeftIdentifier, SymbolKind.Interface, DiagnosticBag.GetNullInstance()) as InterfaceTypeSymbol;
			}
			symbols = FindCodeunits(interfaceTypeSymbol, namespaceSymbol, cancellationToken);
		}
		return GetPropertyValueRecommendationsFromSymbols(symbols, (namespaceSymbol == null) ? new Func<ISymbol, string>(base.GetInsertionTextWithQualificationIfNeeded) : null);
	}

	private IEnumerable<ISymbol> FindInterfaces(NamespaceSymbol? qualifierOpt)
	{
		EnumTypeSymbol enumTypeSymbol = (EnumTypeSymbol)(((Symbol)base.Context.SemanticModel.GetDeclaredSymbol(base.PropertyValue.Parent))?.GetBaseApplicationObjectSymbol());
		if (enumTypeSymbol == null)
		{
			return SpecializedCollections.EmptyEnumerable<ISymbol>();
		}
		return enumTypeSymbol.ImplementedInterfaces.Where((InterfaceTypeSymbol i) => !i.IsUnresolvedInterface());
	}

	private IEnumerable<ISymbol> FindCodeunits(InterfaceTypeSymbol? interfaceTypeSymbol, NamespaceSymbol? qualifierOpt, CancellationToken cancellationToken)
	{
		InterfaceTypeSymbol interfaceTypeSymbol2 = interfaceTypeSymbol;
		ImmutableArray<ContainerSymbol>? immutableArray = qualifierOpt?.SymbolMap.GetSymbolsByKind(SymbolKind.Codeunit);
		IEnumerable<ISymbol> enumerable;
		if (!immutableArray.HasValue)
		{
			enumerable = base.Context.LookupSymbols(SymbolKind.Codeunit, cancellationToken);
		}
		else
		{
			IEnumerable<ISymbol> enumerable2 = immutableArray.GetValueOrDefault();
			enumerable = enumerable2;
		}
		IEnumerable<ISymbol> enumerable3 = enumerable;
		if (interfaceTypeSymbol2 == null)
		{
			return enumerable3;
		}
		return enumerable3.Where((ISymbol s) => s.IsKind(SymbolKind.Namespace) || (!interfaceTypeSymbol2.IsUnresolvedInterface() && ((CodeunitTypeSymbol)s).ImplementedInterfaces.Contains(interfaceTypeSymbol2)));
	}
}
