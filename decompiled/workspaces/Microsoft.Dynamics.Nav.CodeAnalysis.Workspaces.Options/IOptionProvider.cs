using System.Collections.Immutable;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Options;

internal interface IOptionProvider
{
	ImmutableArray<IOption> Options { get; }
}
