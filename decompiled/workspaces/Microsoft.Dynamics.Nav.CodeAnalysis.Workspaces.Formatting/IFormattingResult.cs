using System.Collections.Generic;
using System.Threading;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Formatting;

internal interface IFormattingResult
{
	IList<TextChange> GetTextChanges(CancellationToken cancellationToken = default(CancellationToken));

	SyntaxNode GetFormattedRoot(CancellationToken cancellationToken = default(CancellationToken));
}
