using System.Threading;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Options;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Completion;

public abstract class CompletionProvider
{
	internal string Name { get; }

	internal virtual bool IsSnippetProvider => false;

	internal virtual bool IsDebuggerConsoleProvider => false;

	protected CompletionProvider()
	{
		Name = GetType().FullName;
	}

	public abstract Task ProvideCompletionsAsync(CompletionContext context, AbstractSyntaxContext memberSyntaxContext);

	public virtual bool ShouldTriggerCompletion(SourceText text, int position, CompletionTrigger trigger, OptionSet options)
	{
		if (trigger.Kind == CompletionTriggerKind.Insertion)
		{
			int insertedCharacterPosition = ((position > 0) ? (position - 1) : 0);
			return IsInsertionTrigger(text, insertedCharacterPosition, options);
		}
		return false;
	}

	internal virtual bool IsInsertionTrigger(SourceText text, int insertedCharacterPosition, OptionSet options)
	{
		return false;
	}

	public virtual async Task<CompletionChange> GetChangeAsync(Document document, CompletionItem item, char? commitKey = null, CancellationToken cancellationToken = default(CancellationToken))
	{
		return CompletionChange.Create((await GetTextChangeAsync(document, item, commitKey, cancellationToken).ConfigureAwait(continueOnCapturedContext: false)) ?? new TextChange(item.Span, item.DisplayText));
	}

	public virtual Task<TextChange?> GetTextChangeAsync(Document document, CompletionItem selectedItem, char? character, CancellationToken cancellationToken)
	{
		return Task.FromResult<TextChange?>(null);
	}

	protected internal virtual async Task<AbstractSyntaxContext> CreateContextAsync(Document document, int position, CancellationToken cancellationToken)
	{
		return await CreateMemberContextAsync(document, position, cancellationToken);
	}

	internal static async Task<AbstractSyntaxContext> CreateMemberContextAsync(Document document, int position, CancellationToken cancellationToken)
	{
		TextSpan span = new TextSpan(position, 0);
		SemanticModel semanticModel = await document.GetSemanticModelForSpanAsync(span, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		return MemberSyntaxContext.CreateContext(document.Project.Solution.Workspace, semanticModel, position, cancellationToken);
	}
}
