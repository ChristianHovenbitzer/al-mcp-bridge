using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.CodeFixes;

internal abstract class DocumentBasedFixAllSpansProvider : DocumentBasedFixAllProviderBase<TextSpan>
{
	private readonly ImmutableArray<FixAllScope> supportedFixAllScopes;

	protected DocumentBasedFixAllSpansProvider()
		: this(FixAllProvider.DefaultSupportedFixAllScopes)
	{
	}

	protected DocumentBasedFixAllSpansProvider(ImmutableArray<FixAllScope> supportedFixAllScopes)
		: base(supportedFixAllScopes)
	{
	}

	protected override Task<ImmutableDictionary<Document, ImmutableArray<TextSpan>>> GetCodeLocations(FixAllContext fixAllContext, CancellationToken token)
	{
		return fixAllContext.GetFixAllSpansAsync(token);
	}
}
