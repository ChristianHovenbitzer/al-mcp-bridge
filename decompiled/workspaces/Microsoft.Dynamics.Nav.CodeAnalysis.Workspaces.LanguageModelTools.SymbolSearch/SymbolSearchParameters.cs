using System.ComponentModel;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.LanguageModelTools.SymbolSearch;

public sealed class SymbolSearchParameters
{
	[Description("Required text to search in symbol names and documentation. Use '*' to list all symbols matching filters without name filtering. Examples: query='Customer' kinds=['Table'] finds Customer table; query='*' objectName='Customer' memberKinds=['Method'] lists ALL methods in Customer; query='Process' objectName='Customer' memberKinds=['Method'] finds methods containing 'Process' in Customer.")]
	public string Query { get; set; } = string.Empty;


	[Description("Optional filters that narrow results by kind, scope, container, etc. Use objectName to find members of a specific object (e.g., fields of the 'Customer' table).")]
	public SymbolSearchFilters? Filters { get; set; }
}
