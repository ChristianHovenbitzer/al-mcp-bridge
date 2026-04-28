using System.Collections.Generic;
using System.ComponentModel;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.LanguageModelTools.SymbolSearch;

public sealed class SymbolSearchFilters
{
	[Description("Object kinds to include (Table, Codeunit, Page, etc.).")]
	public IReadOnlyList<string>? Kinds { get; set; }

	[Description("Restrict results to a namespace.")]
	public string? Namespace { get; set; }

	[Description("Exact container object name for filtering members only (e.g., 'Customer' with memberKinds=['Method'] finds methods inside Customer table/codeunit). Do NOT use objectName to find the object itself—use query with kinds filter instead (e.g., query='Customer' kinds=['Table']). Must be combined with memberKinds or used when searching for any member type.")]
	public string? ObjectName { get; set; }

	[Description("Member kinds to include (Field, Key, Action, etc.).")]
	public IReadOnlyList<string>? MemberKinds { get; set; }

	[Description("Access modifiers to include.")]
	public IReadOnlyList<string>? Access { get; set; }

	[Description("Obsolete state filter.")]
	public IReadOnlyList<string>? ObsoleteState { get; set; }

	[Description("Match mode: name | doc | all.")]
	public string? Match { get; set; }

	[Description("Scope: project | dependencies | all.")]
	public string? Scope { get; set; }

	[Description("Max results (<=200).")]
	public int? Limit { get; set; }
}
