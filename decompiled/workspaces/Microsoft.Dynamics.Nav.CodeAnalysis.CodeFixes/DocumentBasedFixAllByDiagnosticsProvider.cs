using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.CodeFixes;

public abstract class DocumentBasedFixAllByDiagnosticsProvider : DocumentBasedFixAllProviderBase<Diagnostic>
{
	protected DocumentBasedFixAllByDiagnosticsProvider()
		: this(FixAllProvider.DefaultSupportedFixAllScopes)
	{
	}

	protected DocumentBasedFixAllByDiagnosticsProvider(ImmutableArray<FixAllScope> supportedFixAllScopes)
		: base(supportedFixAllScopes)
	{
	}

	protected override Task<ImmutableDictionary<Document, ImmutableArray<Diagnostic>>> GetCodeLocations(FixAllContext fixAllContext, CancellationToken token)
	{
		return fixAllContext.GetDocumentDiagnosticsToFixAsync();
	}
}
