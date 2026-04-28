using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Formatting.Rules;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.LanguageServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Options;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Formatting;

public static class Formatter
{
	public static SyntaxAnnotation Annotation { get; } = new SyntaxAnnotation();


	public static Task<Document> FormatAsync(Document document, OptionSet options = null, CancellationToken cancellationToken = default(CancellationToken))
	{
		return FormatAsync(document, (IEnumerable<TextSpan>)null, options, cancellationToken);
	}

	public static Task<Document> FormatAsync(Document document, TextSpan span, OptionSet options = null, CancellationToken cancellationToken = default(CancellationToken))
	{
		return FormatAsync(document, SpecializedCollections.SingletonEnumerable(span), options, cancellationToken);
	}

	public static Task<Document> FormatAsync(Document document, IEnumerable<TextSpan> spans, OptionSet options = null, CancellationToken cancellationToken = default(CancellationToken))
	{
		IFormattingService languageService = document.GetLanguageService<IFormattingService>();
		if (languageService != null)
		{
			return languageService.FormatAsync(document, spans, options, cancellationToken);
		}
		return SpecializedTasks.FromResult(document);
	}

	public static Task<Document> FormatAsync(Document document, SyntaxAnnotation annotation, OptionSet options = null, CancellationToken cancellationToken = default(CancellationToken))
	{
		return FormatAsync(document, annotation, options, null, cancellationToken);
	}

	public static SyntaxNode Format(SyntaxNode node, SyntaxAnnotation annotation, Workspace workspace, OptionSet options = null, CancellationToken cancellationToken = default(CancellationToken))
	{
		return FormatAsync(node, annotation, workspace, options, cancellationToken).WaitAndGetResult(cancellationToken);
	}

	public static SyntaxNode Format(SyntaxNode node, Workspace workspace, OptionSet options = null, CancellationToken cancellationToken = default(CancellationToken))
	{
		return FormatAsync(node, workspace, options, cancellationToken).WaitAndGetResult(cancellationToken);
	}

	public static SyntaxNode Format(SyntaxNode node, TextSpan span, Workspace workspace, OptionSet options = null, CancellationToken cancellationToken = default(CancellationToken))
	{
		return FormatAsync(node, span, workspace, options, cancellationToken).WaitAndGetResult(cancellationToken);
	}

	public static SyntaxNode Format(SyntaxNode node, IEnumerable<TextSpan> spans, Workspace workspace, OptionSet options = null, CancellationToken cancellationToken = default(CancellationToken))
	{
		return FormatAsync(node, spans, workspace, options, cancellationToken).WaitAndGetResult(cancellationToken);
	}

	public static IList<TextChange> GetFormattedTextChanges(SyntaxNode node, Workspace workspace, OptionSet options = null, CancellationToken cancellationToken = default(CancellationToken))
	{
		return GetFormattedTextChangesAsync(node, workspace, options, cancellationToken).WaitAndGetResult(cancellationToken);
	}

	public static IList<TextChange> GetFormattedTextChanges(SyntaxNode node, TextSpan span, Workspace workspace, OptionSet options = null, CancellationToken cancellationToken = default(CancellationToken))
	{
		return GetFormattedTextChangesAsync(node, span, workspace, options, cancellationToken).WaitAndGetResult(cancellationToken);
	}

	public static IList<TextChange> GetFormattedTextChanges(SyntaxNode node, IEnumerable<TextSpan> spans, Workspace workspace, OptionSet options = null, CancellationToken cancellationToken = default(CancellationToken))
	{
		return GetFormattedTextChangesAsync(node, spans, workspace, options, cancellationToken).WaitAndGetResult(cancellationToken);
	}

	internal static IEnumerable<IFormattingRule> GetDefaultFormattingRules(Document document)
	{
		if (document == null)
		{
			throw new ArgumentNullException("document");
		}
		ISyntaxFormattingService service = document.Project.LanguageServices.GetService<ISyntaxFormattingService>();
		if (service != null)
		{
			return service.GetDefaultFormattingRules();
		}
		return SpecializedCollections.EmptyEnumerable<IFormattingRule>();
	}

	internal static IEnumerable<IFormattingRule> GetDefaultFormattingRules(Workspace workspace, string language)
	{
		if (workspace == null)
		{
			throw new ArgumentNullException("workspace");
		}
		if (language == null)
		{
			throw new ArgumentNullException("language");
		}
		ISyntaxFormattingService service = workspace.Services.GetLanguageServices(language).GetService<ISyntaxFormattingService>();
		if (service != null)
		{
			return service.GetDefaultFormattingRules();
		}
		return SpecializedCollections.EmptyEnumerable<IFormattingRule>();
	}

	internal static async Task<Document> FormatAsync(Document document, IEnumerable<TextSpan> spans, OptionSet options, IEnumerable<IFormattingRule> rules, CancellationToken cancellationToken)
	{
		if (document == null)
		{
			throw new ArgumentNullException("document");
		}
		return document.WithSyntaxRoot(await FormatAsync(await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false), spans, document.Project.Solution.Workspace, options, rules, cancellationToken).ConfigureAwait(continueOnCapturedContext: false));
	}

	internal static async Task<Document> FormatAsync(Document document, SyntaxAnnotation annotation, OptionSet options, IEnumerable<IFormattingRule> rules, CancellationToken cancellationToken)
	{
		if (document == null)
		{
			throw new ArgumentNullException("document");
		}
		if (annotation == null)
		{
			throw new ArgumentNullException("annotation");
		}
		return document.WithSyntaxRoot(await FormatAsync(await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false), annotation, document.Project.Solution.Workspace, options, rules, cancellationToken).ConfigureAwait(continueOnCapturedContext: false));
	}

	internal static Task<SyntaxNode> FormatAsync(SyntaxNode node, SyntaxAnnotation annotation, Workspace workspace, OptionSet options = null, CancellationToken cancellationToken = default(CancellationToken))
	{
		return FormatAsync(node, annotation, workspace, options, null, cancellationToken);
	}

	internal static Task<SyntaxNode> FormatAsync(SyntaxNode node, SyntaxAnnotation annotation, Workspace workspace, OptionSet options, IEnumerable<IFormattingRule> rules, CancellationToken cancellationToken)
	{
		if (workspace == null)
		{
			throw new ArgumentNullException("workspace");
		}
		if (node == null)
		{
			throw new ArgumentNullException("node");
		}
		if (annotation == null)
		{
			throw new ArgumentNullException("annotation");
		}
		IEnumerable<TextSpan> spans = ((annotation == SyntaxAnnotation.ElasticAnnotation) ? GetElasticSpans(node) : GetAnnotatedSpans(node, annotation));
		return FormatAsync(node, spans, workspace, options, rules, cancellationToken);
	}

	internal static Task<SyntaxNode> FormatAsync(SyntaxNode node, Workspace workspace, OptionSet options = null, CancellationToken cancellationToken = default(CancellationToken))
	{
		return FormatAsync(node, SpecializedCollections.SingletonEnumerable(node.FullSpan), workspace, options, null, cancellationToken);
	}

	internal static Task<SyntaxNode> FormatAsync(SyntaxNode node, TextSpan span, Workspace workspace, OptionSet options = null, CancellationToken cancellationToken = default(CancellationToken))
	{
		return FormatAsync(node, SpecializedCollections.SingletonEnumerable(span), workspace, options, null, cancellationToken);
	}

	internal static Task<SyntaxNode> FormatAsync(SyntaxNode node, IEnumerable<TextSpan> spans, Workspace workspace, OptionSet options = null, CancellationToken cancellationToken = default(CancellationToken))
	{
		return FormatAsync(node, spans, workspace, options, null, cancellationToken);
	}

	internal static SyntaxNode Format(SyntaxNode node, IEnumerable<TextSpan> spans, Workspace workspace, OptionSet options, IEnumerable<IFormattingRule> rules, CancellationToken cancellationToken)
	{
		return FormatAsync(node, spans, workspace, options, rules, cancellationToken).WaitAndGetResult(cancellationToken);
	}

	internal static async Task<SyntaxNode> FormatAsync(SyntaxNode node, IEnumerable<TextSpan> spans, Workspace workspace, OptionSet options, IEnumerable<IFormattingRule> rules, CancellationToken cancellationToken)
	{
		IFormattingResult formattingResult = await GetFormattingResult(node, spans, workspace, options, rules, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		return (formattingResult == null) ? node : formattingResult.GetFormattedRoot(cancellationToken);
	}

	internal static async Task<IList<TextChange>> GetFormattedTextChangesAsync(SyntaxNode node, IEnumerable<TextSpan> spans, Workspace workspace, OptionSet options, IEnumerable<IFormattingRule> rules, CancellationToken cancellationToken)
	{
		IFormattingResult formattingResult = await GetFormattingResult(node, spans, workspace, options, rules, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		return (formattingResult == null) ? SpecializedCollections.EmptyList<TextChange>() : formattingResult.GetTextChanges(cancellationToken);
	}

	internal static Task<IFormattingResult> GetFormattingResult(SyntaxNode node, IEnumerable<TextSpan> spans, Workspace workspace, OptionSet options, IEnumerable<IFormattingRule> rules, CancellationToken cancellationToken)
	{
		if (workspace == null)
		{
			throw new ArgumentNullException("workspace");
		}
		if (node == null)
		{
			throw new ArgumentNullException("node");
		}
		ISyntaxFormattingService service = workspace.Services.GetLanguageServices("AL").GetService<ISyntaxFormattingService>();
		if (service == null)
		{
			return SpecializedTasks.Default<IFormattingResult>();
		}
		options = options ?? workspace.Options;
		rules = rules ?? GetDefaultFormattingRules(workspace, "AL");
		spans = spans ?? SpecializedCollections.SingletonEnumerable(node.FullSpan);
		return service.FormatAsync(node, spans, options, rules, cancellationToken);
	}

	internal static Task<IList<TextChange>> GetFormattedTextChangesAsync(SyntaxNode node, Workspace workspace, OptionSet options = null, CancellationToken cancellationToken = default(CancellationToken))
	{
		return GetFormattedTextChangesAsync(node, SpecializedCollections.SingletonEnumerable(node.FullSpan), workspace, options, null, cancellationToken);
	}

	internal static Task<IList<TextChange>> GetFormattedTextChangesAsync(SyntaxNode node, TextSpan span, Workspace workspace, OptionSet options = null, CancellationToken cancellationToken = default(CancellationToken))
	{
		return GetFormattedTextChangesAsync(node, SpecializedCollections.SingletonEnumerable(span), workspace, options, null, cancellationToken);
	}

	internal static Task<IList<TextChange>> GetFormattedTextChangesAsync(SyntaxNode node, IEnumerable<TextSpan> spans, Workspace workspace, OptionSet options = null, CancellationToken cancellationToken = default(CancellationToken))
	{
		return GetFormattedTextChangesAsync(node, spans, workspace, options, null, cancellationToken);
	}

	internal static IList<TextChange> GetFormattedTextChanges(SyntaxNode node, IEnumerable<TextSpan> spans, Workspace workspace, OptionSet options, IEnumerable<IFormattingRule> rules, CancellationToken cancellationToken)
	{
		return GetFormattedTextChangesAsync(node, spans, workspace, options, rules, cancellationToken).WaitAndGetResult(cancellationToken);
	}

	private static IEnumerable<TextSpan> GetAnnotatedSpans(SyntaxNode node, SyntaxAnnotation annotation)
	{
		foreach (SyntaxNodeOrToken annotatedNodesAndToken in node.GetAnnotatedNodesAndTokens(annotation))
		{
			SyntaxToken firstToken = (annotatedNodesAndToken.IsNode ? annotatedNodesAndToken.AsNode().GetFirstToken(includeZeroWidth: true) : annotatedNodesAndToken.AsToken());
			SyntaxToken lastToken = (annotatedNodesAndToken.IsNode ? annotatedNodesAndToken.AsNode().GetLastToken(includeZeroWidth: true) : annotatedNodesAndToken.AsToken());
			yield return GetSpan(firstToken, lastToken);
		}
	}

	private static TextSpan GetSpan(SyntaxToken firstToken, SyntaxToken lastToken)
	{
		SyntaxToken previousToken = firstToken.GetPreviousToken();
		SyntaxToken nextToken = lastToken.GetNextToken();
		if (previousToken.Kind != 0)
		{
			firstToken = previousToken;
		}
		if (nextToken.Kind != 0)
		{
			lastToken = nextToken;
		}
		return TextSpan.FromBounds(firstToken.SpanStart, lastToken.Span.End);
	}

	private static IEnumerable<TextSpan> GetElasticSpans(SyntaxNode root)
	{
		return AggregateSpans(from t in (from tr in root.GetAnnotatedTrivia(SyntaxAnnotation.ElasticAnnotation)
				select tr.Token).Distinct()
			select GetElasticSpan(t));
	}

	private static TextSpan GetElasticSpan(SyntaxToken token)
	{
		return GetSpan(token, token);
	}

	private static IEnumerable<TextSpan> AggregateSpans(IEnumerable<TextSpan> spans)
	{
		List<TextSpan> list = new List<TextSpan>();
		TextSpan textSpan = default(TextSpan);
		foreach (TextSpan span in spans)
		{
			if (textSpan == default(TextSpan))
			{
				textSpan = span;
				continue;
			}
			if (span.IntersectsWith(textSpan))
			{
				textSpan = TextSpan.FromBounds(textSpan.Start, span.End);
				continue;
			}
			list.Add(textSpan);
			textSpan = span;
		}
		if (textSpan != default(TextSpan))
		{
			list.Add(textSpan);
		}
		return list;
	}
}
