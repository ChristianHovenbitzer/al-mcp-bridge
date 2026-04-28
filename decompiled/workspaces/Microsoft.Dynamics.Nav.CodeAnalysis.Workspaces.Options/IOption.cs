using System;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Options;

public interface IOption
{
	string Feature { get; }

	string Name { get; }

	Type Type { get; }

	object DefaultValue { get; }

	bool IsPerLanguage { get; }
}
