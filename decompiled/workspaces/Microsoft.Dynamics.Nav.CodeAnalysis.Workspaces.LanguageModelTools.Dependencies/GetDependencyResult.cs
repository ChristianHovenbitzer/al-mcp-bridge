using System;
using System.Collections.Generic;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.LanguageModelTools.Dependencies;

public sealed class GetDependencyResult
{
	public bool Succeeded { get; set; }

	public string? ErrorMessage { get; set; }

	public string? ModuleName { get; set; }

	public IReadOnlyList<DependencyInfo> Dependencies { get; set; } = Array.Empty<DependencyInfo>();

}
