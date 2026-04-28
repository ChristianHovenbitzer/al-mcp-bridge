using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Utilities;

internal static class SymbolDisplayPartExtensions
{
	public static string GetFullText(this ImmutableArray<SymbolDisplayPart> parts)
	{
		return parts.AsEnumerable().GetFullText();
	}

	public static string GetFullText(this IEnumerable<SymbolDisplayPart> parts)
	{
		return string.Join(string.Empty, parts.Select((SymbolDisplayPart p) => p.ToString()));
	}

	public static void AddApplicationObjectNameName(this IList<SymbolDisplayPart> parts, string text)
	{
		parts.Add(new SymbolDisplayPart(SymbolDisplayPartKind.ApplicationObjectName, null, text));
	}

	public static void AddOptionName(this IList<SymbolDisplayPart> parts, string text)
	{
		parts.Add(new SymbolDisplayPart(SymbolDisplayPartKind.OptionName, null, text));
	}

	public static void AddErrorTypeName(this IList<SymbolDisplayPart> parts, string text)
	{
		parts.Add(new SymbolDisplayPart(SymbolDisplayPartKind.ErrorTypeName, null, text));
	}

	public static void AddEventName(this IList<SymbolDisplayPart> parts, string text)
	{
		parts.Add(new SymbolDisplayPart(SymbolDisplayPartKind.EventName, null, text));
	}

	public static void AddFieldName(this IList<SymbolDisplayPart> parts, string text)
	{
		parts.Add(new SymbolDisplayPart(SymbolDisplayPartKind.FieldName, null, text));
	}

	public static void AddKeyword(this IList<SymbolDisplayPart> parts, string text)
	{
		parts.Add(new SymbolDisplayPart(SymbolDisplayPartKind.Keyword, null, text));
	}

	public static void AddGlobalName(this IList<SymbolDisplayPart> parts, string text)
	{
		parts.Add(new SymbolDisplayPart(SymbolDisplayPartKind.GlobalName, null, text));
	}

	public static void AddLocalName(this IList<SymbolDisplayPart> parts, string text)
	{
		parts.Add(new SymbolDisplayPart(SymbolDisplayPartKind.LocalName, null, text));
	}

	public static void AddLineBreak(this IList<SymbolDisplayPart> parts, string text = "\r\n")
	{
		parts.Add(new SymbolDisplayPart(SymbolDisplayPartKind.LineBreak, null, text));
	}

	public static void AddNumericLiteral(this IList<SymbolDisplayPart> parts, string text)
	{
		parts.Add(new SymbolDisplayPart(SymbolDisplayPartKind.NumericLiteral, null, text));
	}

	public static void AddStringLiteral(this IList<SymbolDisplayPart> parts, string text)
	{
		parts.Add(new SymbolDisplayPart(SymbolDisplayPartKind.StringLiteral, null, text));
	}

	public static void AddMethodName(this IList<SymbolDisplayPart> parts, string text)
	{
		parts.Add(new SymbolDisplayPart(SymbolDisplayPartKind.MethodName, null, text));
	}

	public static void AddModuleName(this IList<SymbolDisplayPart> parts, string text)
	{
		parts.Add(new SymbolDisplayPart(SymbolDisplayPartKind.ModuleName, null, text));
	}

	public static void AddControlName(this IList<SymbolDisplayPart> parts, string text)
	{
		parts.Add(new SymbolDisplayPart(SymbolDisplayPartKind.ControlName, null, text));
	}

	public static void AddOperator(this IList<SymbolDisplayPart> parts, string text)
	{
		parts.Add(new SymbolDisplayPart(SymbolDisplayPartKind.Operator, null, text));
	}

	public static void AddParameterName(this IList<SymbolDisplayPart> parts, string text)
	{
		parts.Add(new SymbolDisplayPart(SymbolDisplayPartKind.ParameterName, null, text));
	}

	public static void AddPropertyName(this IList<SymbolDisplayPart> parts, string text)
	{
		parts.Add(new SymbolDisplayPart(SymbolDisplayPartKind.PropertyName, null, text));
	}

	public static void AddPunctuation(this IList<SymbolDisplayPart> parts, string text)
	{
		parts.Add(new SymbolDisplayPart(SymbolDisplayPartKind.Punctuation, null, text));
	}

	public static void AddSpace(this IList<SymbolDisplayPart> parts, string text = " ")
	{
		parts.Add(new SymbolDisplayPart(SymbolDisplayPartKind.Space, null, text));
	}

	public static void AddText(this IList<SymbolDisplayPart> parts, string text)
	{
		parts.Add(new SymbolDisplayPart(SymbolDisplayPartKind.Text, null, text));
	}
}
