using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Microsoft.Dynamics.Nav.CodeAnalysis.Shared.Collections;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Formatting.Rules;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Log;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Options;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Formatting;

internal class FormattingContext : IIntervalIntrospector<FormattingContext.AnchorData>, IIntervalIntrospector<FormattingContext.IndentationData>, IIntervalIntrospector<FormattingContext.RelativeIndentationData>
{
	private class AnchorData
	{
		private readonly AnchorIndentationOperation _operation;

		public TextSpan TextSpan => _operation.TextSpan;

		public SyntaxToken AnchorToken => _operation.AnchorToken;

		public SyntaxToken StartToken => _operation.StartToken;

		public SyntaxToken EndToken => _operation.EndToken;

		public int OriginalColumn { get; }

		public AnchorData(AnchorIndentationOperation operation, int originalColumn)
		{
			_operation = operation;
			OriginalColumn = originalColumn;
		}
	}

	private abstract class IndentationData
	{
		public TextSpan TextSpan { get; }

		public abstract int Indentation { get; }

		public IndentationData(TextSpan textSpan)
		{
			TextSpan = textSpan;
		}
	}

	private class RootIndentationData : SimpleIndentationData
	{
		public RootIndentationData(SyntaxNode rootNode)
			: base(rootNode.FullSpan, 0)
		{
		}
	}

	private class SimpleIndentationData : IndentationData
	{
		public override int Indentation { get; }

		public SimpleIndentationData(TextSpan textSpan, int indentation)
			: base(textSpan)
		{
			Indentation = indentation;
		}
	}

	private class LazyIndentationData : IndentationData
	{
		private readonly Lazy<int> _indentationGetter;

		public override int Indentation => _indentationGetter.Value;

		public LazyIndentationData(TextSpan textSpan, Lazy<int> indentationGetter)
			: base(textSpan)
		{
			_indentationGetter = indentationGetter;
		}
	}

	private class RelativeIndentationData : LazyIndentationData
	{
		public TextSpan InseparableRegionSpan { get; }

		public IndentBlockOperation Operation { get; }

		public SyntaxToken EndToken => Operation.EndToken;

		public RelativeIndentationData(int inseparableRegionSpanStart, TextSpan textSpan, IndentBlockOperation operation, Lazy<int> indentationGetter)
			: base(textSpan, indentationGetter)
		{
			Operation = operation;
			InseparableRegionSpan = TextSpan.FromBounds(inseparableRegionSpanStart, textSpan.End);
		}
	}

	private class InitialContextFinder
	{
		private readonly ChainedFormattingRules formattingRules;

		private readonly SyntaxToken lastToken;

		private readonly SyntaxNode rootNode;

		private readonly TokenStream tokenStream;

		public InitialContextFinder(TokenStream tokenStream, ChainedFormattingRules formattingRules, SyntaxNode rootNode, SyntaxToken lastToken)
		{
			Contract.ThrowIfNull(tokenStream);
			Contract.ThrowIfNull(formattingRules);
			Contract.ThrowIfNull(rootNode);
			this.tokenStream = tokenStream;
			this.formattingRules = formattingRules;
			this.rootNode = rootNode;
			this.lastToken = lastToken;
		}

		public (List<IndentBlockOperation>, List<SuppressOperation>) Do(SyntaxToken startToken, SyntaxToken endToken)
		{
			using (Logger.LogBlock(FunctionId.Formatting_ContextInitialization, CancellationToken.None))
			{
				List<IndentBlockOperation> initialIndentBlockOperations = GetInitialIndentBlockOperations(startToken, endToken);
				List<SuppressOperation> initialSuppressOperations = GetInitialSuppressOperations(startToken, endToken);
				return ValueTuple.Create(initialIndentBlockOperations, initialSuppressOperations);
			}
		}

		private List<IndentBlockOperation> GetInitialIndentBlockOperations(SyntaxToken startToken, SyntaxToken endToken)
		{
			TextSpan span = TextSpan.FromBounds(startToken.SpanStart, endToken.Span.End);
			SyntaxNode syntaxNode = startToken.GetCommonRoot(endToken).GetParentWithBiggerSpan();
			SyntaxNode previous = null;
			List<IndentBlockOperation> operations = new List<IndentBlockOperation>();
			List<IndentBlockOperation> list = new List<IndentBlockOperation>();
			while (syntaxNode != null)
			{
				syntaxNode.DescendantNodesAndSelf((SyntaxNode n) => n != previous && n.Span.IntersectsWith(span) && !span.Contains(n.Span)).Do(delegate(SyntaxNode n)
				{
					formattingRules.AddIndentBlockOperations(list, n, lastToken);
					foreach (IndentBlockOperation item in list)
					{
						if (item != null)
						{
							operations.Add(item);
						}
					}
					list.Clear();
				});
				if (operations.Any((IndentBlockOperation o) => o.TextSpan.Contains(span)))
				{
					break;
				}
				previous = syntaxNode;
				syntaxNode = syntaxNode.Parent;
			}
			operations.RemoveAll((IndentBlockOperation o) => o == null || !o.TextSpan.IntersectsWith(span));
			if (operations.Count == 0)
			{
				operations.Add(new IndentBlockOperation(rootNode.GetFirstToken(includeZeroWidth: true), rootNode.GetLastToken(includeZeroWidth: true), rootNode.FullSpan, 0, IndentBlockOption.AbsolutePosition));
				return operations;
			}
			operations.Sort(CommonFormattingHelpers.IndentBlockOperationComparer);
			return operations;
		}

		private List<SuppressOperation> GetInitialSuppressOperations(SyntaxToken startToken, SyntaxToken endToken)
		{
			List<SuppressOperation> initialSuppressOperations = GetInitialSuppressOperations(startToken, endToken, SuppressOption.NoWrapping);
			List<SuppressOperation> initialSuppressOperations2 = GetInitialSuppressOperations(startToken, endToken, SuppressOption.NoSpacing);
			List<SuppressOperation> list = initialSuppressOperations.Combine(initialSuppressOperations2);
			if (list == null)
			{
				return null;
			}
			list.Sort(CommonFormattingHelpers.SuppressOperationComparer);
			return list;
		}

		private List<SuppressOperation> GetInitialSuppressOperations(SyntaxToken startToken, SyntaxToken endToken, SuppressOption mask)
		{
			List<SuppressOperation> initialSuppressOperations = GetInitialSuppressOperations(startToken, mask);
			List<SuppressOperation> initialSuppressOperations2 = GetInitialSuppressOperations(endToken, mask);
			return initialSuppressOperations.Combine(initialSuppressOperations2);
		}

		private List<SuppressOperation> GetInitialSuppressOperations(SyntaxToken token, SuppressOption mask)
		{
			SyntaxNode parent = token.Parent;
			int startPosition = token.SpanStart;
			List<SuppressOperation> list = new List<SuppressOperation>();
			Predicate<SuppressOperation> match = delegate(SuppressOperation o)
			{
				if (o == null)
				{
					return true;
				}
				if (o.ContainsElasticTrivia(tokenStream) && !o.Option.IsOn(SuppressOption.IgnoreElastic))
				{
					return true;
				}
				if (!o.TextSpan.Contains(startPosition))
				{
					return true;
				}
				return !o.Option.IsMaskOn(mask);
			};
			for (SyntaxNode syntaxNode = parent; syntaxNode != null; syntaxNode = syntaxNode.Parent)
			{
				formattingRules.AddSuppressOperations(list, syntaxNode, lastToken);
				list.RemoveAll(match);
				if (list.Count > 0)
				{
					return list;
				}
			}
			return null;
		}
	}

	private readonly Dictionary<SyntaxToken, AnchorData> anchorBaseTokenMap;

	private readonly HashSet<TextSpan> anchorMap;

	private readonly ContextIntervalTree<AnchorData> anchorTree;

	private readonly AbstractFormatEngine engine;

	private readonly HashSet<TextSpan> indentationMap;

	private readonly ContextIntervalTree<IndentationData> indentationTree;

	private readonly string language;

	private readonly ContextIntervalTree<RelativeIndentationData> relativeIndentationTree;

	private readonly HashSet<TextSpan> suppressSpacingMap;

	private readonly ContextIntervalTree<SuppressSpacingData> suppressSpacingTree;

	private readonly HashSet<TextSpan> suppressWrappingMap;

	private readonly ContextIntervalTree<SuppressWrappingData> suppressWrappingTree;

	private List<IndentBlockOperation> initialIndentBlockOperations;

	public OptionSet OptionSet => engine.OptionSet;

	public TreeData TreeData => engine.TreeData;

	public TokenStream TokenStream { get; }

	int IIntervalIntrospector<AnchorData>.GetStart(AnchorData value)
	{
		return value.TextSpan.Start;
	}

	int IIntervalIntrospector<AnchorData>.GetLength(AnchorData value)
	{
		return value.TextSpan.Length;
	}

	public FormattingContext(AbstractFormatEngine engine, TokenStream tokenStream, string language)
	{
		Contract.ThrowIfNull(engine);
		Contract.ThrowIfNull(tokenStream);
		this.engine = engine;
		TokenStream = tokenStream;
		this.language = language;
		relativeIndentationTree = new ContextIntervalTree<RelativeIndentationData>(this);
		indentationTree = new ContextIntervalTree<IndentationData>(this);
		suppressWrappingTree = new ContextIntervalTree<SuppressWrappingData>(SuppressIntervalIntrospector.Instance);
		suppressSpacingTree = new ContextIntervalTree<SuppressSpacingData>(SuppressIntervalIntrospector.Instance);
		anchorTree = new ContextIntervalTree<AnchorData>(this);
		anchorBaseTokenMap = new Dictionary<SyntaxToken, AnchorData>();
		indentationMap = new HashSet<TextSpan>();
		suppressWrappingMap = new HashSet<TextSpan>();
		suppressSpacingMap = new HashSet<TextSpan>();
		anchorMap = new HashSet<TextSpan>();
		initialIndentBlockOperations = new List<IndentBlockOperation>();
	}

	public void Initialize(ChainedFormattingRules formattingRules, SyntaxToken startToken, SyntaxToken endToken, CancellationToken cancellationToken)
	{
		SyntaxNode root = TreeData.Root;
		if (TokenStream.IsFormattingWholeDocument)
		{
			RootIndentationData rootIndentationData = new RootIndentationData(root);
			indentationTree.AddIntervalInPlace(rootIndentationData);
			indentationMap.Add(rootIndentationData.TextSpan);
			return;
		}
		(List<IndentBlockOperation>, List<SuppressOperation>) tuple = new InitialContextFinder(TokenStream, formattingRules, root, endToken).Do(startToken, endToken);
		if (tuple.Item1 != null)
		{
			List<IndentBlockOperation> item = tuple.Item1;
			IndentBlockOperation indentBlockOperation = item[0];
			int indentationOfCurrentPosition = new BottomUpBaseIndentationFinder(formattingRules, OptionSet.GetOption(FormattingOptions.TabSize, language), OptionSet.GetOption(FormattingOptions.IndentationSize, language), TokenStream, endToken).GetIndentationOfCurrentPosition(root, indentBlockOperation, (SyntaxToken t) => TokenStream.GetCurrentColumn(t), cancellationToken);
			SimpleIndentationData simpleIndentationData = new SimpleIndentationData(indentBlockOperation.TextSpan, indentationOfCurrentPosition);
			indentationTree.AddIntervalInPlace(simpleIndentationData);
			indentationMap.Add(simpleIndentationData.TextSpan);
			initialIndentBlockOperations = item;
		}
		tuple.Item2?.Do(delegate(SuppressOperation o)
		{
			AddInitialSuppressOperation(o);
		});
	}

	public void AddIndentBlockOperations(List<IndentBlockOperation> operations, CancellationToken cancellationToken)
	{
		Contract.ThrowIfNull(operations);
		if (initialIndentBlockOperations.Count <= 0)
		{
			operations.Sort(CommonFormattingHelpers.IndentBlockOperationComparer);
			operations.Do(delegate(IndentBlockOperation o)
			{
				cancellationToken.ThrowIfCancellationRequested();
				AddIndentBlockOperation(o);
			});
			return;
		}
		TextSpan textSpan = initialIndentBlockOperations[0].TextSpan;
		List<IndentBlockOperation> list = new List<IndentBlockOperation>(initialIndentBlockOperations.Count - 1 + operations.Count);
		for (int i = 1; i < initialIndentBlockOperations.Count; i++)
		{
			list.Add(initialIndentBlockOperations[i]);
		}
		for (int j = 0; j < operations.Count; j++)
		{
			cancellationToken.ThrowIfCancellationRequested();
			TextSpan textSpan2 = operations[j].TextSpan;
			if (textSpan2.Start >= textSpan.Start && !textSpan2.Contains(textSpan))
			{
				list.Add(operations[j]);
			}
		}
		list.Sort(CommonFormattingHelpers.IndentBlockOperationComparer);
		list.Do(delegate(IndentBlockOperation o)
		{
			cancellationToken.ThrowIfCancellationRequested();
			AddIndentBlockOperation(o);
		});
	}

	public void AddIndentBlockOperation(IndentBlockOperation operation)
	{
		IndentBlockOperation operation2 = operation;
		TextSpan textSpan = operation2.TextSpan;
		if (textSpan.IsEmpty || indentationMap.Contains(textSpan))
		{
			return;
		}
		if (operation2.IsRelativeIndentation)
		{
			RelativeIndentationData value = new RelativeIndentationData(operation2.Option.IsOn(IndentBlockOption.RelativeToFirstTokenOnBaseTokenLine) ? TokenStream.FirstTokenOfBaseTokenLine(operation2.BaseToken).FullSpan.Start : operation2.BaseToken.FullSpan.Start, indentationGetter: new Lazy<int>(delegate
			{
				int num2 = operation2.IndentationDeltaOrPosition * OptionSet.GetOption(FormattingOptions.IndentationSize, language);
				return TokenStream.GetCurrentColumn(operation2.Option.IsOn(IndentBlockOption.RelativeToFirstTokenOnBaseTokenLine) ? TokenStream.FirstTokenOfBaseTokenLine(operation2.BaseToken) : operation2.BaseToken) + num2;
			}, isThreadSafe: true), textSpan: textSpan, operation: operation2);
			indentationTree.AddIntervalInPlace(value);
			relativeIndentationTree.AddIntervalInPlace(value);
			indentationMap.Add(textSpan);
			return;
		}
		if (operation2.Option.IsOn(IndentBlockOption.AbsolutePosition))
		{
			indentationTree.AddIntervalInPlace(new SimpleIndentationData(textSpan, operation2.IndentationDeltaOrPosition));
			indentationMap.Add(textSpan);
			return;
		}
		IndentationData indentationData = indentationTree.GetSmallestContainingInterval(operation2.TextSpan.Start, 0);
		if (indentationData == null)
		{
			int indentation = operation2.IndentationDeltaOrPosition * OptionSet.GetOption(FormattingOptions.IndentationSize, language);
			indentationTree.AddIntervalInPlace(new SimpleIndentationData(textSpan, indentation));
			indentationMap.Add(textSpan);
			return;
		}
		Lazy<int> indentationGetter2 = new Lazy<int>(delegate
		{
			int num = operation2.IndentationDeltaOrPosition * OptionSet.GetOption(FormattingOptions.IndentationSize, language);
			return indentationData.Indentation + num;
		}, isThreadSafe: true);
		indentationTree.AddIntervalInPlace(new LazyIndentationData(textSpan, indentationGetter2));
		indentationMap.Add(textSpan);
	}

	public void AddInitialSuppressOperation(SuppressOperation operation)
	{
		if (operation != null && !operation.TextSpan.IsEmpty)
		{
			bool onSameLine = TokenStream.TwoTokensOriginallyOnSameLine(operation.StartToken, operation.EndToken);
			AddSuppressOperation(operation, onSameLine);
		}
	}

	public void AddSuppressOperations(List<SuppressOperation> operations, CancellationToken cancellationToken)
	{
		List<SuppressOperation> operations2 = operations;
		(SuppressOperation, bool, bool)[] valuePairs = new(SuppressOperation, bool, bool)[operations2.Count];
		engine.TaskExecutor.For(0, operations2.Count, delegate(int i)
		{
			SuppressOperation suppressOperation = operations2[i];
			if (suppressOperation.ContainsElasticTrivia(TokenStream) && !suppressOperation.Option.IsOn(SuppressOption.IgnoreElastic))
			{
				valuePairs[i] = ValueTuple.Create(suppressOperation, item2: false, item3: false);
			}
			else
			{
				bool item = TokenStream.TwoTokensOriginallyOnSameLine(suppressOperation.StartToken, suppressOperation.EndToken);
				valuePairs[i] = ValueTuple.Create(suppressOperation, item2: true, item);
			}
		}, cancellationToken);
		valuePairs.Do(delegate((SuppressOperation, bool, bool) v)
		{
			cancellationToken.ThrowIfCancellationRequested();
			if (v.Item2)
			{
				AddSuppressOperation(v.Item1, v.Item3);
			}
		});
	}

	public void AddAnchorIndentationOperation(AnchorIndentationOperation operation)
	{
		if (!operation.TextSpan.IsEmpty && !anchorMap.Contains(operation.TextSpan) && !anchorBaseTokenMap.ContainsKey(operation.AnchorToken))
		{
			int originalColumn = TokenStream.GetOriginalColumn(operation.StartToken);
			AnchorData value = new AnchorData(operation, originalColumn);
			anchorTree.AddIntervalInPlace(value);
			anchorBaseTokenMap.Add(operation.AnchorToken, value);
			anchorMap.Add(operation.TextSpan);
		}
	}

	public int GetBaseIndentation(SyntaxToken token)
	{
		return GetBaseIndentation(token.SpanStart);
	}

	public int GetBaseIndentation(int position)
	{
		return indentationTree.GetSmallestContainingInterval(position, 0)?.Indentation ?? 0;
	}

	public IEnumerable<IndentBlockOperation> GetAllRelativeIndentBlockOperations()
	{
		return from i in relativeIndentationTree.GetIntervalsThatIntersectWith(TreeData.StartPosition, TreeData.EndPosition, this)
			select i.Operation;
	}

	public bool TryGetEndTokenForRelativeIndentationSpan(SyntaxToken token, int maxChainDepth, out SyntaxToken endToken, CancellationToken cancellationToken)
	{
		endToken = default(SyntaxToken);
		int num = 0;
		do
		{
			cancellationToken.ThrowIfCancellationRequested();
			if (num++ > maxChainDepth)
			{
				return false;
			}
			TextSpan span = token.Span;
			RelativeIndentationData smallestContainingInterval = relativeIndentationTree.GetSmallestContainingInterval(span.Start, 0);
			if (smallestContainingInterval == null)
			{
				endToken = token;
				return true;
			}
			token = smallestContainingInterval.EndToken.GetNextToken(includeZeroWidth: true);
		}
		while (token.Kind != 0);
		return true;
	}

	public int GetAnchorDeltaFromOriginalColumn(SyntaxToken token)
	{
		AnchorData anchorData = GetAnchorData(token);
		if (anchorData == null)
		{
			return 0;
		}
		return TokenStream.GetCurrentColumn(anchorData.AnchorToken) - anchorData.OriginalColumn;
	}

	public SyntaxToken GetAnchorToken(SyntaxToken token)
	{
		return GetAnchorData(token)?.AnchorToken ?? default(SyntaxToken);
	}

	public int GetDeltaFromPreviousChangesMap(SyntaxToken token, Dictionary<SyntaxToken, int> previousChangesMap)
	{
		if (!previousChangesMap.ContainsKey(token))
		{
			return 0;
		}
		return TokenStream.GetCurrentColumn(token) - previousChangesMap[token];
	}

	public SyntaxToken GetEndTokenForAnchorSpan(TokenData tokenData)
	{
		AnchorData anchorData = FindAnchorSpanOnSameLineAfterToken(tokenData);
		if (anchorData == null)
		{
			return default(SyntaxToken);
		}
		ImmutableArray<AnchorData> intervalsThatOverlapWith = anchorTree.GetIntervalsThatOverlapWith(anchorData.TextSpan.Start, anchorData.TextSpan.Length);
		SyntaxToken endToken = anchorData.EndToken;
		ImmutableArray<AnchorData>.Enumerator enumerator = intervalsThatOverlapWith.GetEnumerator();
		while (enumerator.MoveNext())
		{
			AnchorData current = enumerator.Current;
			if (anchorData.TextSpan.IntersectsWith(current.AnchorToken.Span) && current.EndToken.Span.End >= endToken.Span.End)
			{
				endToken = current.EndToken;
			}
		}
		return endToken;
	}

	public bool IsWrappingSuppressed(TextSpan textSpan)
	{
		return suppressWrappingTree.GetSmallestEdgeExclusivelyContainingInterval(textSpan.Start, textSpan.Length)?.NoWrapping ?? false;
	}

	public bool IsSpacingSuppressed(TextSpan textSpan)
	{
		return suppressSpacingTree.GetSmallestEdgeExclusivelyContainingInterval(textSpan.Start, textSpan.Length)?.NoSpacing ?? false;
	}

	public bool IsSpacingSuppressed(int pairIndex)
	{
		SyntaxToken token = TokenStream.GetToken(pairIndex);
		SyntaxToken token2 = TokenStream.GetToken(pairIndex + 1);
		TextSpan textSpan = TextSpan.FromBounds(token.SpanStart, token2.Span.End);
		return IsSpacingSuppressed(textSpan);
	}

	private void AddSuppressOperation(SuppressOperation operation, bool onSameLine)
	{
		AddSpacingSuppressOperation(operation, onSameLine);
		AddWrappingSuppressOperation(operation, onSameLine);
	}

	private void AddSpacingSuppressOperation(SuppressOperation operation, bool twoTokensOnSameLine)
	{
		if (operation != null && !operation.TextSpan.IsEmpty)
		{
			SuppressOption option = operation.Option;
			if (option.IsMaskOn(SuppressOption.NoSpacing) && !suppressSpacingMap.Contains(operation.TextSpan) && ((option.IsOn(SuppressOption.NoSpacingIfOnSingleLine) && twoTokensOnSameLine) || (option.IsOn(SuppressOption.NoSpacingIfOnMultipleLine) && !twoTokensOnSameLine)))
			{
				SuppressSpacingData value = new SuppressSpacingData(operation.TextSpan, noSpacing: true);
				suppressSpacingMap.Add(operation.TextSpan);
				suppressSpacingTree.AddIntervalInPlace(value);
			}
		}
	}

	private void AddWrappingSuppressOperation(SuppressOperation operation, bool twoTokensOnSameLine)
	{
		if (operation != null && !operation.TextSpan.IsEmpty)
		{
			SuppressOption option = operation.Option;
			if (option.IsMaskOn(SuppressOption.NoWrapping) && !suppressWrappingMap.Contains(operation.TextSpan) && ((option.IsOn(SuppressOption.NoWrappingIfOnSingleLine) && twoTokensOnSameLine) || (option.IsOn(SuppressOption.NoWrappingIfOnMultipleLine) && !twoTokensOnSameLine)))
			{
				SuppressWrappingData value = new SuppressWrappingData(operation.TextSpan, noWrapping: true);
				suppressWrappingMap.Add(operation.TextSpan);
				suppressWrappingTree.AddIntervalInPlace(value);
			}
		}
	}

	[Conditional("DEBUG")]
	private void DebugCheckEmpty<T>(ContextIntervalTree<T> tree, TextSpan textSpan)
	{
		Contract.ThrowIfFalse(tree.GetIntervalsThatContain(textSpan.Start, textSpan.Length).Length == 0);
	}

	private AnchorData GetAnchorData(SyntaxToken token)
	{
		TextSpan span = token.Span;
		AnchorData smallestContainingInterval = anchorTree.GetSmallestContainingInterval(span.Start, 0);
		if (smallestContainingInterval == null)
		{
			return null;
		}
		return smallestContainingInterval;
	}

	private AnchorData FindAnchorSpanOnSameLineAfterToken(TokenData tokenData)
	{
		AnchorData result = null;
		while (tokenData.IndexInStream >= 0)
		{
			if (anchorBaseTokenMap.TryGetValue(tokenData.Token, out AnchorData value))
			{
				result = value;
			}
			int indexInStream = tokenData.IndexInStream;
			if (TokenStream.TokenCount - 1 <= indexInStream || TokenStream.GetTriviaData(indexInStream).SecondTokenIsFirstTokenOnLine)
			{
				return result;
			}
			tokenData = tokenData.GetNextTokenData();
		}
		return result;
	}

	int IIntervalIntrospector<IndentationData>.GetStart(IndentationData value)
	{
		return value.TextSpan.Start;
	}

	int IIntervalIntrospector<IndentationData>.GetLength(IndentationData value)
	{
		return value.TextSpan.Length;
	}

	int IIntervalIntrospector<RelativeIndentationData>.GetStart(RelativeIndentationData value)
	{
		return value.InseparableRegionSpan.Start;
	}

	int IIntervalIntrospector<RelativeIndentationData>.GetLength(RelativeIndentationData value)
	{
		return value.InseparableRegionSpan.Length;
	}
}
