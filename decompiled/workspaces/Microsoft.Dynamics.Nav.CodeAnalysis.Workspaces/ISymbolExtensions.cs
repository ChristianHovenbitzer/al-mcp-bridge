using System.Collections.Immutable;
using System.Globalization;
using System.Threading;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces;

internal static class ISymbolExtensions
{
	public static string? GetDocumentationComment(this ISymbol symbol, CancellationToken cancellationToken = default(CancellationToken))
	{
		string text = symbol.GetDocumentation();
		if (string.IsNullOrEmpty(text))
		{
			if (symbol.Kind == SymbolKind.Parameter)
			{
				return symbol.ContainingSymbol?.GetDocumentationComment(null, cancellationToken)?.GetParameterText(symbol.Name);
			}
			text = symbol.GetDocumentationComment(null, cancellationToken)?.SummaryText ?? string.Empty;
		}
		return text;
	}

	public static DocumentationComment GetDocumentationComment(this ISymbol symbol, CultureInfo? preferredCulture = null, CancellationToken cancellationToken = default(CancellationToken))
	{
		string documentationCommentXml = symbol.GetDocumentationCommentXml(preferredCulture, cancellationToken);
		if (!string.IsNullOrEmpty(documentationCommentXml))
		{
			return DocumentationComment.FromXmlFragmentAsMarkdown(documentationCommentXml);
		}
		return DocumentationComment.Empty;
	}

	public static ImmutableArray<IParameterSymbol> GetParameters(this ISymbol? symbol)
	{
		if (symbol is IMethodSymbol methodSymbol)
		{
			return methodSymbol.Parameters;
		}
		return ImmutableArray<IParameterSymbol>.Empty;
	}

	public static IReturnValueSymbol? GetReturnValue(this ISymbol? symbol)
	{
		if (symbol is IMethodSymbol methodSymbol)
		{
			return methodSymbol.ReturnValueSymbol;
		}
		return null;
	}

	public static string? GetDocumentationText(this ISymbol symbol, string? obsoletionText, bool isMarkdownDocs)
	{
		string text = symbol.GetDocumentationComment(default(CancellationToken));
		if (string.IsNullOrEmpty(text))
		{
			text = DocumentationFacts.GetDocumentation(symbol);
		}
		return text?.GetDocumentationText(obsoletionText, isMarkdownDocs);
	}
}
