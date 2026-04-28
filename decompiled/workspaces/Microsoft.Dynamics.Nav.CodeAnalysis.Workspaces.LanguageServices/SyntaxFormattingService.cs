using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.CodeAnalysis.Shared;
using Microsoft.Dynamics.Nav.CodeAnalysis.Shared.Collections;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Formatting;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Formatting.Rules;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Host;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Options;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.LanguageServices;

internal class SyntaxFormattingService : ISyntaxFormattingService, ILanguageService
{
	private static readonly Func<TextSpan, bool> s_notEmpty = (TextSpan s) => !s.IsEmpty;

	private static readonly Func<TextSpan, int> s_spanLength = (TextSpan s) => s.Length;

	internal static readonly SyntaxFormattingService Instance = new SyntaxFormattingService();

	private readonly Lazy<IEnumerable<IFormattingRule>> lazyExportedRules;

	public SyntaxFormattingService()
	{
		lazyExportedRules = new Lazy<IEnumerable<IFormattingRule>>(() => new IFormattingRule[1]
		{
			new DefaultOperationProvider()
		});
	}

	public IEnumerable<IFormattingRule> GetDefaultFormattingRules()
	{
		IEnumerable<IFormattingRule> value = lazyExportedRules.Value;
		return new IFormattingRule[11]
		{
			new WrappingFormattingRule(),
			new SpacingFormattingRule(),
			new NewLineUserSettingFormattingRule(),
			new IndentUserSettingsFormattingRule(),
			new ElasticTriviaFormattingRule(),
			new EndOfFileTokenFormattingRule(),
			new StructuredTriviaFormattingRule(),
			new IndentBlockFormattingRule(),
			new SuppressFormattingRule(),
			new AnchorIndentationFormattingRule(),
			new TokenBasedFormattingRule()
		}.Concat(value);
	}

	protected IFormattingResult CreateAggregatedFormattingResult(SyntaxNode node, IList<AbstractFormattingResult> results, SimpleIntervalTree<TextSpan> formattingSpans = null)
	{
		return new AggregatedFormattingResult(node, results, formattingSpans);
	}

	protected Task<AbstractFormattingResult> FormatAsync(SyntaxNode node, OptionSet optionSet, IEnumerable<IFormattingRule> formattingRules, SyntaxToken token1, SyntaxToken token2, CancellationToken cancellationToken)
	{
		return new ALFormatEngine(node, optionSet, formattingRules, token1, token2).FormatAsync(cancellationToken);
	}

	public async Task<IFormattingResult> FormatAsync(SyntaxNode node, IEnumerable<TextSpan> spans, OptionSet options, IEnumerable<IFormattingRule> rules, CancellationToken cancellationToken)
	{
		CheckArguments(node, spans, options, rules);
		NormalizedTextSpanCollection normalizedTextSpanCollection = new NormalizedTextSpanCollection(spans.Where(s_notEmpty));
		if (normalizedTextSpanCollection.Count == 0)
		{
			return CreateAggregatedFormattingResult(node, SpecializedCollections.EmptyList<AbstractFormattingResult>());
		}
		if (AllowDisjointSpanMerging(normalizedTextSpanCollection, options.GetOption(FormattingOptions.AllowDisjointSpanMerging)))
		{
			return await FormatMergedSpanAsync(node, options, rules, normalizedTextSpanCollection, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		}
		return await FormatIndividuallyAsync(node, options, rules, normalizedTextSpanCollection, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
	}

	private static void CheckArguments(SyntaxNode node, IEnumerable<TextSpan> spans, OptionSet options, IEnumerable<IFormattingRule> rules)
	{
		if (node == null)
		{
			throw new ArgumentNullException("node");
		}
		if (spans == null)
		{
			throw new ArgumentNullException("spans");
		}
		if (options == null)
		{
			throw new ArgumentNullException("options");
		}
		if (rules == null)
		{
			throw new ArgumentException("rules");
		}
	}

	private async Task<IFormattingResult> FormatMergedSpanAsync(SyntaxNode node, OptionSet options, IEnumerable<IFormattingRule> rules, IList<TextSpan> spansToFormat, CancellationToken cancellationToken)
	{
		TextSpan spanToFormat = TextSpan.FromBounds(spansToFormat[0].Start, spansToFormat[spansToFormat.Count - 1].End);
		(SyntaxToken, SyntaxToken) tuple = node.ConvertToTokenPair(spanToFormat);
		if (node.IsInvalidTokenRange(tuple.Item1, tuple.Item2))
		{
			return CreateAggregatedFormattingResult(node, SpecializedCollections.EmptyList<AbstractFormattingResult>());
		}
		AbstractFormattingResult item = await FormatAsync(node, options, rules, tuple.Item1, tuple.Item2, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		return CreateAggregatedFormattingResult(node, new List<AbstractFormattingResult>(1) { item }, SimpleIntervalTree.Create(TextSpanIntervalIntrospector.Instance, spanToFormat));
	}

	private async Task<IFormattingResult> FormatIndividuallyAsync(SyntaxNode node, OptionSet options, IEnumerable<IFormattingRule> rules, IList<TextSpan> spansToFormat, CancellationToken cancellationToken)
	{
		List<AbstractFormattingResult> results = null;
		foreach (var item in node.ConvertToTokenPairs(spansToFormat))
		{
			if (!node.IsInvalidTokenRange(item.Item1, item.Item2))
			{
				results = results ?? new List<AbstractFormattingResult>();
				List<AbstractFormattingResult> list = results;
				list.Add(await FormatAsync(node, options, rules, item.Item1, item.Item2, cancellationToken).ConfigureAwait(continueOnCapturedContext: false));
			}
		}
		if (results == null)
		{
			return CreateAggregatedFormattingResult(node, SpecializedCollections.EmptyList<AbstractFormattingResult>());
		}
		if (results.Count == 1)
		{
			return results[0];
		}
		return CreateAggregatedFormattingResult(node, results);
	}

	private bool AllowDisjointSpanMerging(IList<TextSpan> list, bool shouldUseFormattingSpanCollapse)
	{
		if (!shouldUseFormattingSpanCollapse)
		{
			return false;
		}
		if (list.Count <= 3)
		{
			return false;
		}
		if (list.Count > 30)
		{
			return true;
		}
		TextSpan textSpan = TextSpan.FromBounds(list[0].Start, list[list.Count - 1].End);
		int val = list.Sum(s_spanLength);
		return textSpan.Length / Math.Max(val, 1) < 2;
	}
}
