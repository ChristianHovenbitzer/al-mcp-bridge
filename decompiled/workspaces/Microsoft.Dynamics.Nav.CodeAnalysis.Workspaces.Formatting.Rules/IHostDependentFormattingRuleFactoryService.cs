using System.Collections.Generic;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Host;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Formatting.Rules;

internal interface IHostDependentFormattingRuleFactoryService : IWorkspaceService
{
	bool ShouldNotFormatOrCommitOnPaste(Document document);

	bool ShouldUseBaseIndentation(Document document);

	IFormattingRule CreateRule(Document document, int position);

	IEnumerable<TextChange> FilterFormattedChanges(Document document, TextSpan span, IList<TextChange> changes);
}
