namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.LanguageModelTools.SymbolSearch;

public sealed class SymbolInfo
{
	public string Id { get; set; } = string.Empty;


	public string Name { get; set; } = string.Empty;


	public string? FullName { get; set; }

	public string Kind { get; set; } = string.Empty;


	public string? Namespace { get; set; }

	public string? ContainerName { get; set; }

	public string? Signature { get; set; }

	public string? DocSummary { get; set; }

	public string? Path { get; set; }
}
