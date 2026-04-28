using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Completion;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Utilities;

public static class SymbolExtensions
{
	internal static Glyph GetGlyph(this ISymbol symbol)
	{
		switch (symbol.Kind)
		{
		case SymbolKind.ArrayType:
			return ((ArrayTypeSymbol)symbol).ElementType.GetGlyph();
		case SymbolKind.Field:
		{
			ITypeSymbol containingType = symbol.ContainingType;
			if (containingType != null && containingType.Kind == SymbolKind.Option)
			{
				return Glyph.Option;
			}
			return Glyph.Field;
		}
		case SymbolKind.Option:
		case SymbolKind.OptionType:
			return Glyph.Option;
		case SymbolKind.GlobalVariable:
			return Glyph.GlobalVariable;
		case SymbolKind.LocalVariable:
			return Glyph.LocalVariable;
		case SymbolKind.Class:
			return Glyph.Builtin;
		case SymbolKind.Table:
			return Glyph.Table;
		case SymbolKind.Codeunit:
			return Glyph.Codeunit;
		case SymbolKind.Page:
			return Glyph.Page;
		case SymbolKind.Report:
			return Glyph.Report;
		case SymbolKind.Query:
			return Glyph.Query;
		case SymbolKind.XmlPort:
			return Glyph.XmlPort;
		case SymbolKind.PageCustomization:
			return Glyph.PageCustomization;
		case SymbolKind.Profile:
			return Glyph.Profile;
		case SymbolKind.PermissionSet:
			return Glyph.PermissionSet;
		case SymbolKind.ControlAddIn:
			return Glyph.ControlAddIn;
		case SymbolKind.Control:
			return Glyph.Control;
		case SymbolKind.Action:
			return Glyph.Action;
		case SymbolKind.Method:
			switch (((IMethodSymbol)symbol).MethodKind)
			{
			case MethodKind.Trigger:
				return Glyph.Trigger;
			case MethodKind.BuiltInMethod:
			case MethodKind.BuiltInOperator:
				return Glyph.Builtin;
			default:
				return Glyph.Method;
			}
		case SymbolKind.Parameter:
			return Glyph.Parameter;
		case SymbolKind.Property:
			return Glyph.Property;
		case SymbolKind.Key:
			return Glyph.Key;
		case SymbolKind.Enum:
			return Glyph.Enum;
		case SymbolKind.EnumValue:
			return Glyph.EnumValue;
		case SymbolKind.Interface:
			return Glyph.Interface;
		case SymbolKind.Namespace:
			return Glyph.Namespace;
		default:
			return Glyph.None;
		}
	}

	internal static NameSyntax GetQualifiedNameSyntax(this ISymbol symbol)
	{
		if (symbol.ContainingSymbol == null || symbol.ContainingSymbol.Kind == SymbolKind.Module || string.IsNullOrEmpty(symbol.ContainingSymbol.Name))
		{
			return SyntaxFactory.IdentifierName(symbol.Name.QuoteIdentifierIfNeeded());
		}
		return SyntaxFactory.QualifiedName(symbol.ContainingSymbol.GetQualifiedNameSyntax(), SyntaxFactory.IdentifierName(symbol.Name.QuoteIdentifierIfNeeded()));
	}

	internal static NameSyntax? GetNamespacePartOfQualifiedNameSyntax(this ISymbol symbol)
	{
		if (symbol.ContainingSymbol == null || symbol.ContainingSymbol.Kind == SymbolKind.Module || string.IsNullOrEmpty(symbol.ContainingSymbol.Name))
		{
			return null;
		}
		while (symbol.ContainingSymbol.Kind != SymbolKind.Namespace)
		{
			symbol = symbol.ContainingSymbol;
		}
		return symbol.ContainingSymbol.GetQualifiedNameSyntax();
	}

	internal static NameSyntax GetNamespaceAndNameQualifiedNameSyntax(this ISymbol symbol)
	{
		NameSyntax namespacePartOfQualifiedNameSyntax = symbol.GetNamespacePartOfQualifiedNameSyntax();
		SimpleNameSyntax simpleNameSyntax = SyntaxFactory.IdentifierName((symbol.Kind == SymbolKind.DotNet) ? symbol.ContainingSymbol.Name : symbol.Name);
		if (namespacePartOfQualifiedNameSyntax == null)
		{
			return simpleNameSyntax;
		}
		return SyntaxFactory.QualifiedName(namespacePartOfQualifiedNameSyntax, simpleNameSyntax);
	}
}
