using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.FindSymbols;

[DebuggerDisplay("{GetDebuggerDisplay(),nq}")]
public class ReferencedSymbol
{
	public ISymbol Definition => DefinitionAndProjectId.Symbol;

	internal SymbolAndProjectId DefinitionAndProjectId { get; }

	public IEnumerable<ReferenceLocation> Locations { get; }

	internal ReferencedSymbol(SymbolAndProjectId definitionAndProjectId, IEnumerable<ReferenceLocation> locations)
	{
		DefinitionAndProjectId = definitionAndProjectId;
		Locations = (locations ?? SpecializedCollections.EmptyEnumerable<ReferenceLocation>()).ToReadOnlyCollection();
	}

	internal string GetDebuggerDisplay()
	{
		int num = Locations.Count();
		return string.Format(CultureInfo.InvariantCulture, "{0}, {1} {2}", Definition.Name, num, (num == 1) ? "ref" : "refs");
	}
}
