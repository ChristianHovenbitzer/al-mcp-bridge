using System;
using System.Collections.Generic;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.LanguageModelTools.SymbolSearch;

public sealed class SymbolSearchResult
{
	public bool Succeeded { get; set; }

	public string? Message { get; set; }

	public IReadOnlyList<SymbolInfo> Symbols { get; set; } = Array.Empty<SymbolInfo>();

}
