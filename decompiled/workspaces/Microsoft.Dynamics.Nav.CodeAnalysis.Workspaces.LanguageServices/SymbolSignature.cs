using System.Collections.Generic;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.LanguageServices;

internal class SymbolSignature
{
	public ISymbol Label { get; }

	public IEnumerable<ISymbol> Parameters { get; }

	public SymbolSignature(ISymbol label, IEnumerable<ISymbol> parameters)
	{
		Label = label;
		Parameters = parameters;
	}
}
