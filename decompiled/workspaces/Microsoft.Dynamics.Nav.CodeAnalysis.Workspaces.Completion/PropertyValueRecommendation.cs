using System;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Completion;

internal class PropertyValueRecommendation : IEquatable<PropertyValueRecommendation>
{
	public string DisplayText { get; private set; }

	public string? DetailText { get; set; }

	public string? DescriptionValue { get; set; }

	public string? DocumentationText { get; set; }

	public string? InsertionText { get; set; }

	public string? ObsoleteInformation { get; set; }

	public bool IsMarkdownDocs { get; set; }

	public bool IsDeprecated { get; set; }

	public Glyph Glyph { get; set; }

	public ISymbol? Symbol { get; private set; }

	public bool AddUsingStatementWhenCompleting { get; private set; }

	public static PropertyValueRecommendation Create(ISymbol symbol, Func<ISymbol, string>? getInsertionText, bool matchDisplayTextToInsertionText = false, Glyph glyph = Glyph.None, Binder enclosingBinder = null, bool addUsingStatementWhenCompleting = true)
	{
		string displayText = (matchDisplayTextToInsertionText ? symbol.Name.QuoteIdentifierIfNeeded() : symbol.Name);
		string obsoleteInformationMessage = ObsoleteSymbolHelper.GetObsoleteInformationMessage(symbol);
		return new PropertyValueRecommendation(displayText)
		{
			InsertionText = getInsertionText?.Invoke(symbol),
			ObsoleteInformation = obsoleteInformationMessage,
			Glyph = glyph,
			Symbol = symbol,
			DocumentationText = symbol.GetDocumentationText(obsoleteInformationMessage, isMarkdownDocs: true),
			DescriptionValue = GetDescriptionValue(symbol, enclosingBinder),
			AddUsingStatementWhenCompleting = addUsingStatementWhenCompleting
		};
	}

	private static string? GetDescriptionValue(ISymbol symbol, Binder? binder)
	{
		if (binder == null)
		{
			return null;
		}
		if (binder.IsSymbolInScope(symbol))
		{
			return symbol.Name;
		}
		return symbol.ToDisplayString(SymbolDisplayFormat.QualifiedNameOnlyFormat);
	}

	internal PropertyValueRecommendation(string displayText)
	{
		DisplayText = displayText;
	}

	public bool Equals(PropertyValueRecommendation other)
	{
		if (other == null)
		{
			return false;
		}
		if (string.Equals(DisplayText, other.DisplayText, StringComparison.OrdinalIgnoreCase) && string.Equals(DetailText, other.DetailText, StringComparison.OrdinalIgnoreCase) && string.Equals(DescriptionValue, other.DescriptionValue, StringComparison.OrdinalIgnoreCase) && string.Equals(ObsoleteInformation, other.ObsoleteInformation, StringComparison.OrdinalIgnoreCase) && string.Equals(InsertionText, other.InsertionText, StringComparison.OrdinalIgnoreCase) && string.Equals(DocumentationText, other.DocumentationText, StringComparison.OrdinalIgnoreCase) && IsMarkdownDocs == other.IsMarkdownDocs && IsDeprecated == other.IsDeprecated)
		{
			return Glyph == other.Glyph;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return Hash.Combine(Hash.Combine(DisplayText.GetHashCode(), DetailText?.GetHashCode() ?? 0, DescriptionValue?.GetHashCode() ?? 0, ObsoleteInformation?.GetHashCode() ?? 0, InsertionText?.GetHashCode() ?? 0, IsMarkdownDocs.GetHashCode()), DocumentationText?.GetHashCode() ?? 0, IsDeprecated.GetHashCode(), Glyph.GetHashCode());
	}
}
