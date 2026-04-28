using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Formatting;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Host;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Options;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.LanguageServices;

internal class FormattingService : IFormattingService, ILanguageService
{
	internal static readonly FormattingService Instance = new FormattingService();

	public Task<Document> FormatAsync(Document document, IEnumerable<TextSpan> spans, OptionSet options, CancellationToken cancellationToken)
	{
		return Formatter.FormatAsync(document, spans, options, null, cancellationToken);
	}
}
