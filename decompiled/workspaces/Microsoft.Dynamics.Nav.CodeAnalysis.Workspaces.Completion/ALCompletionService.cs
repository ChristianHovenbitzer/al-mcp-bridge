using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Completion;

internal class ALCompletionService : CommonCompletionService
{
	private readonly ImmutableArray<CompletionProvider> defaultCompletionProviders = ImmutableArray.Create(new CompletionProvider[5]
	{
		new TextCompletionProvider(),
		new PropertyDeclarationCompletionProvider(),
		new PropertyValueCompletionProvider(),
		new SymbolCompletionProvider(),
		new XmlDocCommentCompletionProvider()
	});

	private readonly Workspace workspace;

	private CompletionRules latestRules = CompletionRules.Default;

	public ALCompletionService(Workspace workspace, ImmutableArray<CompletionProvider>? exclusiveProviders = null)
		: base(workspace, exclusiveProviders)
	{
		this.workspace = workspace;
	}

	protected override ImmutableArray<CompletionProvider> GetBuiltInProviders()
	{
		return defaultCompletionProviders;
	}

	public override async Task<TextSpan> GetDefaultItemSpanAsync(Document document, SourceText text, int caretPosition, CancellationToken cancellationToken)
	{
		SyntaxToken token = (await document.GetSyntaxRootAsync(cancellationToken)).FindToken(caretPosition);
		if ((token.IsKind(SyntaxKind.IdentifierToken) || token.IsKind(SyntaxKind.StringLiteralToken)) && token.Span.IntersectsWith(caretPosition))
		{
			return GetTextSpanDependingOnSurroundingQuotes(token, caretPosition);
		}
		return CompletionUtilities.GetCompletionItemSpan(text, caretPosition);
	}

	private static TextSpan GetTextSpanDependingOnSurroundingQuotes(SyntaxToken token, int caretPosition)
	{
		if (token.Text.Length > 0 && token.Text[0] == '"')
		{
			int num = caretPosition - token.SpanStart;
			if (num > 0 && Microsoft.Dynamics.Nav.CodeAnalysis.Utilities.StringExtensions.Last(token.Text) == '"' && token.Text.Length > 1)
			{
				num++;
			}
			return new TextSpan(token.SpanStart, num);
		}
		return token.Span;
	}

	public override CompletionRules GetRules()
	{
		EnterKeyRule defaultEnterKeyRule = ((!workspace.Options.GetOption(CompletionOptions.AddNewLineOnEnterAfterFullyTypedWord)) ? EnterKeyRule.Never : EnterKeyRule.AfterFullyTypedWord);
		CompletionRules completionRules = latestRules.WithDefaultEnterKeyRule(defaultEnterKeyRule);
		Interlocked.Exchange(ref latestRules, completionRules);
		return completionRules;
	}
}
