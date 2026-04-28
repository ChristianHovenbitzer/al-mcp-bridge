using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Formatting;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Formatting.Rules;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Host;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Options;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.LanguageServices;

internal interface ISyntaxFormattingService : ILanguageService
{
	IEnumerable<IFormattingRule> GetDefaultFormattingRules();

	Task<IFormattingResult> FormatAsync(SyntaxNode node, IEnumerable<TextSpan> spans, OptionSet options, IEnumerable<IFormattingRule> rules, CancellationToken cancellationToken);
}
