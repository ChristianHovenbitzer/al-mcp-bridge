using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Host;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Options;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Completion;

public abstract class CompletionService : ILanguageService
{
	public static CompletionService GetService(Document document)
	{
		return document.Project.LanguageServices.GetService<CompletionService>();
	}

	public virtual CompletionRules GetRules()
	{
		return CompletionRules.Default;
	}

	public virtual bool ShouldTriggerCompletion(SourceText text, int caretPosition, CompletionTrigger trigger, ImmutableHashSet<string> roles = null, OptionSet options = null)
	{
		return false;
	}

	public virtual Task<TextSpan> GetDefaultItemSpanAsync(Document document, SourceText text, int caretPosition, CancellationToken cancellationToken)
	{
		return Task.FromResult(CommonCompletionUtilities.GetWordSpan(text, caretPosition, (char c) => char.IsLetter(c), (char c) => char.IsLetterOrDigit(c)));
	}

	public abstract Task<CompletionList> GetCompletionsAsync(Document document, int caretPosition, CompletionTrigger trigger = default(CompletionTrigger), ImmutableHashSet<string> roles = null, OptionSet options = null, CancellationToken cancellationToken = default(CancellationToken));

	public virtual Task<CompletionChange> GetChangeAsync(Document document, CompletionItem item, char? commitCharacter = null, CancellationToken cancellationToken = default(CancellationToken))
	{
		return Task.FromResult(CompletionChange.Create(new TextChange(item.Span, item.DisplayText)));
	}
}
