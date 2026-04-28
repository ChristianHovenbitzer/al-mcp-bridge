using System.Collections.Immutable;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Options;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Completion;

internal class CompletionOptionsProvider : IOptionProvider
{
	public ImmutableArray<IOption> Options { get; } = ImmutableArray<IOption>.Empty;

}
