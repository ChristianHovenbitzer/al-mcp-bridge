using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Formatting.Rules;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Log;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Options;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Formatting;

internal abstract class AbstractFormatEngine
{
	private class OperationApplier
	{
		private readonly FormattingContext context;

		private readonly ChainedFormattingRules formattingRules;

		private readonly TokenStream tokenStream;

		public OperationApplier(FormattingContext context, TokenStream tokenStream, ChainedFormattingRules formattingRules)
		{
			this.context = context;
			this.tokenStream = tokenStream;
			this.formattingRules = formattingRules;
		}

		public bool Apply(AdjustSpacesOperation operation, int pairIndex)
		{
			if (operation.Option == AdjustSpacesOption.PreserveSpaces)
			{
				return ApplyPreserveSpacesOperation(operation, pairIndex);
			}
			if (operation.Option == AdjustSpacesOption.ForceSpaces)
			{
				return ApplyForceSpacesOperation(operation, pairIndex);
			}
			if (operation.Option == AdjustSpacesOption.DynamicSpaceToIndentationIfOnSingleLine)
			{
				return ApplyDynamicSpacesOperation(operation, pairIndex);
			}
			return ApplySpaceIfSingleLine(operation, pairIndex);
		}

		public bool ApplyForceSpacesOperation(AdjustSpacesOperation operation, int pairIndex)
		{
			TriviaData triviaData = tokenStream.GetTriviaData(pairIndex);
			if (triviaData.LineBreaks == 0 && triviaData.Spaces == operation.Space)
			{
				return false;
			}
			tokenStream.ApplyChange(pairIndex, triviaData.WithSpace(operation.Space, context, formattingRules));
			return true;
		}

		public bool Apply(AdjustNewLinesOperation operation, int pairIndex, CancellationToken cancellationToken)
		{
			if (operation.Option == AdjustNewLinesOption.PreserveLines)
			{
				return ApplyPreserveLinesOperation(operation, pairIndex, cancellationToken);
			}
			if (operation.Option == AdjustNewLinesOption.ForceLines)
			{
				return ApplyForceLinesOperation(operation, pairIndex, cancellationToken);
			}
			if (tokenStream.TwoTokensOnSameLine(tokenStream.GetToken(pairIndex), tokenStream.GetToken(pairIndex + 1)))
			{
				return ApplyForceLinesOperation(operation, pairIndex, cancellationToken);
			}
			return false;
		}

		public bool ApplyPreserveLinesOperation(AdjustNewLinesOperation operation, int pairIndex, CancellationToken cancellationToken)
		{
			TriviaData triviaData = tokenStream.GetTriviaData(pairIndex);
			int baseIndentation = context.GetBaseIndentation(tokenStream.GetToken(pairIndex + 1));
			if (operation.Line > triviaData.LineBreaks)
			{
				tokenStream.ApplyChange(pairIndex, triviaData.WithLine(operation.Line, baseIndentation, context, formattingRules, cancellationToken));
				return true;
			}
			if (triviaData.SecondTokenIsFirstTokenOnLine && baseIndentation != triviaData.Spaces)
			{
				tokenStream.ApplyChange(pairIndex, triviaData.WithIndentation(baseIndentation, context, formattingRules, cancellationToken));
				return true;
			}
			return operation.Line > 0;
		}

		public bool ApplyAlignment(AlignTokensOperation operation, Dictionary<SyntaxToken, int> previousChangesMap, CancellationToken cancellationToken)
		{
			Contract.ThrowIfNull(previousChangesMap);
			IList<TokenData> tokenData;
			switch (operation.Option)
			{
			case AlignTokensOption.AlignIndentationOfTokensToBaseToken:
				if (!ApplyAlignment(operation.BaseToken, operation.Tokens, previousChangesMap, out tokenData, cancellationToken))
				{
					return false;
				}
				break;
			case AlignTokensOption.AlignIndentationOfTokensToFirstTokenOfBaseTokenLine:
				if (!ApplyAlignment(tokenStream.FirstTokenOfBaseTokenLine(operation.BaseToken), operation.Tokens, previousChangesMap, out tokenData, cancellationToken))
				{
					return false;
				}
				break;
			default:
				return Contract.FailWithReturn<bool>("Unknown option");
			}
			ApplyIndentationChangesToDependentTokens(tokenData, previousChangesMap, cancellationToken);
			return true;
		}

		public bool ApplyBaseTokenIndentationChangesFromTo(SyntaxToken baseToken, SyntaxToken startToken, SyntaxToken endToken, Dictionary<SyntaxToken, int> previousChangesMap, CancellationToken cancellationToken)
		{
			Contract.ThrowIfFalse(baseToken.Kind != 0 && startToken.Kind != 0 && endToken.Kind != SyntaxKind.None);
			TokenData tokenData = tokenStream.GetTokenData(baseToken);
			TokenData previousTokenData = tokenStream.GetTokenData(startToken).GetPreviousTokenData();
			TokenData tokenData2 = tokenStream.GetTokenData(endToken);
			return ApplyBaseTokenIndentationChangesFromTo(tokenData, previousTokenData, tokenData2, previousChangesMap, cancellationToken);
		}

		public bool ApplyAnchorIndentation(int pairIndex, Dictionary<SyntaxToken, int> previousChangesMap, CancellationToken cancellationToken)
		{
			TriviaData triviaData = tokenStream.GetTriviaData(pairIndex);
			if (!triviaData.SecondTokenIsFirstTokenOnLine)
			{
				return false;
			}
			if (context.IsSpacingSuppressed(pairIndex))
			{
				return false;
			}
			SyntaxToken token = tokenStream.GetToken(pairIndex + 1);
			int num = triviaData.Spaces + context.GetAnchorDeltaFromOriginalColumn(token);
			if (triviaData.Spaces != num)
			{
				previousChangesMap.Add(token, triviaData.Spaces);
				tokenStream.ApplyChange(pairIndex, triviaData.WithIndentation(num, context, formattingRules, cancellationToken));
				return true;
			}
			return false;
		}

		private bool ApplyDynamicSpacesOperation(AdjustSpacesOperation operation, int pairIndex)
		{
			TriviaData triviaData = tokenStream.GetTriviaData(pairIndex);
			if (triviaData.SecondTokenIsFirstTokenOnLine)
			{
				return false;
			}
			Contract.ThrowIfFalse(triviaData.LineBreaks == 0);
			int baseIndentation = context.GetBaseIndentation(tokenStream.GetToken(pairIndex + 1));
			SyntaxToken token = tokenStream.GetToken(pairIndex);
			tokenStream.GetTokenLength(token, out var length, out var onMultipleLines);
			int num = (onMultipleLines ? length : (tokenStream.GetCurrentColumn(token) + length));
			if (num < baseIndentation)
			{
				tokenStream.ApplyChange(pairIndex, triviaData.WithSpace(baseIndentation - num, context, formattingRules));
				return true;
			}
			return ApplySpaceIfSingleLine(operation, pairIndex);
		}

		private bool ApplyPreserveSpacesOperation(AdjustSpacesOperation operation, int pairIndex)
		{
			TriviaData triviaData = tokenStream.GetTriviaData(pairIndex);
			int space = operation.Space;
			if (triviaData.SecondTokenIsFirstTokenOnLine)
			{
				return false;
			}
			Contract.ThrowIfFalse(triviaData.LineBreaks == 0);
			if (space <= triviaData.Spaces)
			{
				return false;
			}
			tokenStream.ApplyChange(pairIndex, triviaData.WithSpace(space, context, formattingRules));
			return true;
		}

		private bool ApplySpaceIfSingleLine(AdjustSpacesOperation operation, int pairIndex)
		{
			TriviaData triviaData = tokenStream.GetTriviaData(pairIndex);
			int space = operation.Space;
			if (triviaData.SecondTokenIsFirstTokenOnLine)
			{
				return false;
			}
			Contract.ThrowIfFalse(triviaData.LineBreaks == 0);
			if (triviaData.Spaces == space)
			{
				return false;
			}
			tokenStream.ApplyChange(pairIndex, triviaData.WithSpace(space, context, formattingRules));
			return true;
		}

		private bool ApplyForceLinesOperation(AdjustNewLinesOperation operation, int pairIndex, CancellationToken cancellationToken)
		{
			TriviaData triviaData = tokenStream.GetTriviaData(pairIndex);
			int baseIndentation = context.GetBaseIndentation(tokenStream.GetToken(pairIndex + 1));
			if (triviaData.LineBreaks == operation.Line && triviaData.Spaces == baseIndentation && !triviaData.TreatAsElastic)
			{
				return true;
			}
			tokenStream.ApplyChange(pairIndex, triviaData.WithLine(operation.Line, baseIndentation, context, formattingRules, cancellationToken));
			return true;
		}

		private bool CanAlignBeApplied(SyntaxToken token, IEnumerable<SyntaxToken> operationTokens, out IList<TokenData> tokenData)
		{
			if (token.Width() <= 0 || operationTokens.IsEmpty())
			{
				tokenData = null;
				return false;
			}
			tokenData = GetTokenWithIndices(operationTokens);
			if (tokenData.Count == 0)
			{
				return false;
			}
			return true;
		}

		private bool ApplyAlignment(SyntaxToken token, IEnumerable<SyntaxToken> tokens, Dictionary<SyntaxToken, int> previousChangesMap, out IList<TokenData> tokenData, CancellationToken cancellationToken)
		{
			if (!CanAlignBeApplied(token, tokens, out tokenData))
			{
				return false;
			}
			ApplyIndentationToAlignWithGivenToken(token, tokenData, previousChangesMap, cancellationToken);
			return true;
		}

		private void ApplyIndentationToAlignWithGivenToken(SyntaxToken token, IList<TokenData> list, Dictionary<SyntaxToken, int> previousChangesMap, CancellationToken cancellationToken)
		{
			int currentColumn = tokenStream.GetCurrentColumn(token);
			for (int i = 0; i < list.Count; i++)
			{
				TokenData tokenData = list[i];
				TokenData previousTokenData = tokenStream.GetPreviousTokenData(tokenData);
				TriviaData triviaData = tokenStream.GetTriviaData(previousTokenData, tokenData);
				if (triviaData.SecondTokenIsFirstTokenOnLine)
				{
					ApplyIndentationToGivenPosition(previousTokenData, tokenData, triviaData, currentColumn, previousChangesMap, cancellationToken);
				}
			}
		}

		private void ApplyIndentationToGivenPosition(TokenData previousToken, TokenData currentToken, TriviaData triviaInfo, int baseSpaceOrIndentation, Dictionary<SyntaxToken, int> previousChangesMap, CancellationToken cancellationToken)
		{
			previousChangesMap[currentToken.Token] = triviaInfo.Spaces;
			if (previousToken.IndexInStream >= 0 && triviaInfo.Spaces != baseSpaceOrIndentation)
			{
				TextSpan textSpan = TextSpan.FromBounds(previousToken.Token.Span.End, currentToken.Token.SpanStart);
				if (!context.IsSpacingSuppressed(textSpan))
				{
					tokenStream.ApplyChange(previousToken.IndexInStream, triviaInfo.WithIndentation(baseSpaceOrIndentation, context, formattingRules, cancellationToken));
				}
			}
		}

		private IList<TokenData> GetTokenWithIndices(IEnumerable<SyntaxToken> tokens)
		{
			List<TokenData> list = new List<TokenData>();
			foreach (SyntaxToken token in tokens)
			{
				if (token.Kind != 0 && token.Width() > 0)
				{
					TokenData tokenData = tokenStream.GetTokenData(token);
					if (tokenData.IndexInStream >= 0)
					{
						list.Add(tokenData);
					}
				}
			}
			list.Sort((TokenData t1, TokenData t2) => t1.IndexInStream - t2.IndexInStream);
			return list;
		}

		private bool ApplyIndentationChangesToDependentTokens(IList<TokenData> tokenWithIndices, Dictionary<SyntaxToken, int> newChangesMap, CancellationToken cancellationToken)
		{
			for (int i = 0; i < tokenWithIndices.Count; i++)
			{
				TokenData tokenData = tokenWithIndices[i];
				SyntaxToken endTokenForAnchorSpan = context.GetEndTokenForAnchorSpan(tokenData);
				if (endTokenForAnchorSpan.Kind != 0)
				{
					TokenData endToken = tokenStream.GetTokenData(endTokenForAnchorSpan);
					if (endToken.IndexInStream < 0)
					{
						endToken = tokenStream.LastTokenInStream;
					}
					ApplyBaseTokenIndentationChangesFromTo(tokenData, tokenData, endToken, newChangesMap, cancellationToken);
				}
			}
			return true;
		}

		private void ApplyIndentationDeltaFromTo(TokenData firstToken, TokenData lastToken, int indentationDelta, Dictionary<SyntaxToken, int> previousChangesMap, CancellationToken cancellationToken)
		{
			for (int i = firstToken.IndexInStream; i < lastToken.IndexInStream; i++)
			{
				TriviaData triviaData = tokenStream.GetTriviaData(i);
				if (triviaData.SecondTokenIsFirstTokenOnLine && !context.IsSpacingSuppressed(i))
				{
					SyntaxToken token = tokenStream.GetToken(i + 1);
					if (!previousChangesMap.ContainsKey(token))
					{
						ApplyIndentationDelta(i, token, indentationDelta, triviaData, previousChangesMap, cancellationToken);
					}
				}
			}
		}

		private void ApplyIndentationDelta(int pairIndex, SyntaxToken currentToken, int indentationDelta, TriviaData triviaInfo, Dictionary<SyntaxToken, int> previousChangesMap, CancellationToken cancellationToken)
		{
			Contract.ThrowIfFalse(triviaInfo.SecondTokenIsFirstTokenOnLine);
			int num = triviaInfo.Spaces + indentationDelta;
			if (triviaInfo.Spaces != num)
			{
				previousChangesMap.Add(currentToken, triviaInfo.Spaces);
				tokenStream.ApplyChange(pairIndex, triviaInfo.WithIndentation(num, context, formattingRules, cancellationToken));
			}
		}

		private bool ApplyBaseTokenIndentationChangesFromTo(TokenData baseToken, TokenData startToken, TokenData endToken, Dictionary<SyntaxToken, int> previousChangesMap, CancellationToken cancellationToken)
		{
			TokenData tokenData = baseToken;
			if (tokenData.IndexInStream < 0)
			{
				return false;
			}
			while (tokenData.IndexInStream >= 0 && !previousChangesMap.ContainsKey(tokenData.Token))
			{
				int num = tokenData.IndexInStream - 1;
				if (num < 0 || tokenStream.GetTriviaData(num).SecondTokenIsFirstTokenOnLine)
				{
					return false;
				}
				tokenData = tokenData.GetPreviousTokenData();
			}
			if (tokenData.IndexInStream < 0)
			{
				return false;
			}
			int deltaFromPreviousChangesMap = context.GetDeltaFromPreviousChangesMap(tokenData.Token, previousChangesMap);
			if (deltaFromPreviousChangesMap == 0)
			{
				return false;
			}
			startToken = ((startToken.IndexInStream < 0) ? tokenStream.FirstTokenInStream : startToken);
			endToken = ((endToken.IndexInStream < 0) ? tokenStream.LastTokenInStream : endToken);
			ApplyIndentationDeltaFromTo(startToken, endToken, deltaFromPreviousChangesMap, previousChangesMap, cancellationToken);
			return true;
		}
	}

	private class Partitioner
	{
		private const int MinimumItemsPerPartition = 30000;

		private readonly FormattingContext context;

		private readonly TokenPairWithOperations[] operationPairs;

		private readonly TokenStream tokenStream;

		public Partitioner(FormattingContext context, TokenStream tokenStream, TokenPairWithOperations[] operationPairs)
		{
			Contract.ThrowIfNull(context);
			Contract.ThrowIfNull(tokenStream);
			Contract.ThrowIfNull(operationPairs);
			this.context = context;
			this.tokenStream = tokenStream;
			this.operationPairs = operationPairs;
		}

		public List<IEnumerable<TokenPairWithOperations>> GetPartitions(int partitionCount, CancellationToken cancellationToken)
		{
			using (Logger.LogBlock(FunctionId.Formatting_Partitions, cancellationToken))
			{
				Contract.ThrowIfFalse(partitionCount > 0);
				List<IEnumerable<TokenPairWithOperations>> list = new List<IEnumerable<TokenPairWithOperations>>();
				int num = operationPairs.Length / partitionCount;
				if (num < 10 || partitionCount <= 1 || operationPairs.Length < 30000)
				{
					list.Add(GetOperationPairsFromTo(0, operationPairs.Length));
					return list;
				}
				int num2 = 0;
				while (num2 < operationPairs.Length)
				{
					if (!TryGetNextPartitionIndex(num2, num, out var nextIndex))
					{
						list.Add(GetOperationPairsFromTo(num2, operationPairs.Length));
						break;
					}
					SyntaxToken nextPartitionToken = GetNextPartitionToken(nextIndex, num, cancellationToken);
					if (nextPartitionToken.Kind == SyntaxKind.None)
					{
						list.Add(GetOperationPairsFromTo(num2, operationPairs.Length));
						break;
					}
					TokenData tokenData = tokenStream.GetTokenData(nextPartitionToken);
					if (tokenData.IndexInStream < 0)
					{
						list.Add(GetOperationPairsFromTo(num2, operationPairs.Length));
						break;
					}
					Contract.ThrowIfFalse(num2 < tokenData.IndexInStream);
					Contract.ThrowIfFalse(tokenData.IndexInStream <= operationPairs.Length);
					list.Add(GetOperationPairsFromTo(num2, tokenData.IndexInStream));
					num2 = tokenData.IndexInStream;
				}
				return list;
			}
		}

		private SyntaxToken GetNextPartitionToken(int index, int perPartition, CancellationToken cancellationToken)
		{
			do
			{
				if (context.TryGetEndTokenForRelativeIndentationSpan(operationPairs[index].Token1, 10, out var endToken, cancellationToken))
				{
					return endToken;
				}
			}
			while (TryGetNextPartitionIndex(index, perPartition, out index));
			return default(SyntaxToken);
		}

		private bool TryGetNextPartitionIndex(int index, int perPartition, out int nextIndex)
		{
			nextIndex = Math.Min(index + perPartition, operationPairs.Length);
			return nextIndex < operationPairs.Length;
		}

		private IEnumerable<TokenPairWithOperations> GetOperationPairsFromTo(int from, int to)
		{
			for (int i = from; i < to; i++)
			{
				yield return operationPairs[i];
			}
		}
	}

	private const int ConcurrentThreshold = 30000;

	private readonly SyntaxNode commonRoot;

	private readonly ChainedFormattingRules formattingRules;

	private readonly string language = "AL";

	private readonly SyntaxToken token1;

	private readonly SyntaxToken token2;

	internal readonly OptionSet OptionSet;

	protected readonly TextSpan SpanToFormat;

	internal readonly TaskExecutor TaskExecutor;

	internal readonly TreeData TreeData;

	protected AbstractFormatEngine(TreeData treeData, OptionSet optionSet, IEnumerable<IFormattingRule> formattingRules, SyntaxToken token1, SyntaxToken token2, TaskExecutor executor)
		: this(treeData, optionSet, new ChainedFormattingRules(formattingRules, optionSet), token1, token2, executor)
	{
	}

	internal AbstractFormatEngine(TreeData treeData, OptionSet optionSet, ChainedFormattingRules formattingRules, SyntaxToken token1, SyntaxToken token2, TaskExecutor executor)
	{
		Contract.ThrowIfNull(optionSet);
		Contract.ThrowIfNull(treeData);
		Contract.ThrowIfNull(formattingRules);
		Contract.ThrowIfNull(executor);
		Contract.ThrowIfTrue(treeData.Root.IsInvalidTokenRange(token1, token2));
		OptionSet = optionSet;
		TreeData = treeData;
		this.formattingRules = formattingRules;
		this.token1 = token1;
		this.token2 = token2;
		SpanToFormat = GetSpanToFormat();
		commonRoot = token1.GetCommonRoot(token2);
		TaskExecutor = (optionSet.GetOption(FormattingOptions.DebugMode, language) ? Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Formatting.TaskExecutor.Synchronous : ((SpanToFormat.Length < 30000) ? Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Formatting.TaskExecutor.Synchronous : executor));
	}

	public async Task<AbstractFormattingResult> FormatAsync(CancellationToken cancellationToken)
	{
		using (Logger.LogBlock(FunctionId.Formatting_Format, FormatSummary, cancellationToken))
		{
			NodeOperations nodeOperations = CreateNodeOperationTasks(cancellationToken);
			TokenStream tokenStream = new TokenStream(TreeData, OptionSet, SpanToFormat, CreateTriviaFactory());
			Task<TokenPairWithOperations[]> task2 = CreateTokenOperationTask(tokenStream, cancellationToken);
			FormattingContext context = CreateFormattingContext(tokenStream, cancellationToken);
			Task<IEnumerable<AnchorIndentationOperation>> task3 = TaskExecutor.ContinueWith<List<AnchorIndentationOperation>, IEnumerable<AnchorIndentationOperation>>(nodeOperations.AnchorIndentationOperationsTask, (Task<List<AnchorIndentationOperation>> task) => task.Result.Do(context.AddAnchorIndentationOperation), cancellationToken);
			BuildContext(context, tokenStream, nodeOperations, cancellationToken);
			ApplyBeginningOfTreeTriviaOperation(context, tokenStream, cancellationToken);
			FormattingContext context2 = context;
			TokenStream tokenStream2 = tokenStream;
			Task anchorContextTask = task3;
			NodeOperations nodeOperations2 = nodeOperations;
			await ApplyTokenOperationsAsync(context2, tokenStream2, anchorContextTask, nodeOperations2, await task2.ConfigureAwait(continueOnCapturedContext: false), cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
			ApplyTriviaOperations(context, tokenStream, cancellationToken);
			ApplyEndOfTreeTriviaOperation(context, tokenStream, cancellationToken);
			return CreateFormattingResult(tokenStream);
		}
	}

	protected abstract AbstractTriviaDataFactory CreateTriviaFactory();

	protected abstract AbstractFormattingResult CreateFormattingResult(TokenStream tokenStream);

	protected virtual FormattingContext CreateFormattingContext(TokenStream tokenStream, CancellationToken cancellationToken)
	{
		FormattingContext formattingContext = new FormattingContext(this, tokenStream, language);
		formattingContext.Initialize(formattingRules, token1, token2, cancellationToken);
		return formattingContext;
	}

	protected virtual NodeOperations CreateNodeOperationTasks(CancellationToken cancellationToken)
	{
		Task<List<SyntaxNode>> previousTask = TaskExecutor.StartNew(delegate
		{
			using (Logger.LogBlock(FunctionId.Formatting_IterateNodes, cancellationToken))
			{
				List<SyntaxNode> list2 = new List<SyntaxNode>(Math.Max(SpanToFormat.Length / 5, 4));
				foreach (SyntaxNode item in commonRoot.DescendantNodesAndSelf(SpanToFormat))
				{
					cancellationToken.ThrowIfCancellationRequested();
					list2.Add(item);
				}
				return list2;
			}
		}, cancellationToken);
		Task<List<IndentBlockOperation>> indentBlockOperationTask = TaskExecutor.ContinueWith(previousTask, delegate(Task<List<SyntaxNode>> task)
		{
			using (Logger.LogBlock(FunctionId.Formatting_CollectIndentBlock, cancellationToken))
			{
				return AddOperations(task.Result, delegate(List<IndentBlockOperation> l, SyntaxNode n)
				{
					formattingRules.AddIndentBlockOperations(l, n, token2);
				}, cancellationToken);
			}
		}, cancellationToken);
		Task<List<SuppressOperation>> suppressOperationTask = TaskExecutor.ContinueWith(previousTask, delegate(Task<List<SyntaxNode>> task)
		{
			using (Logger.LogBlock(FunctionId.Formatting_CollectSuppressOperation, cancellationToken))
			{
				return AddOperations(task.Result, delegate(List<SuppressOperation> l, SyntaxNode n)
				{
					formattingRules.AddSuppressOperations(l, n, token2);
				}, cancellationToken);
			}
		}, cancellationToken);
		Task<List<AlignTokensOperation>> alignmentOperationTask = TaskExecutor.ContinueWith(previousTask, delegate(Task<List<SyntaxNode>> task)
		{
			using (Logger.LogBlock(FunctionId.Formatting_CollectAlignOperation, cancellationToken))
			{
				List<AlignTokensOperation> list = AddOperations(task.Result, delegate(List<AlignTokensOperation> l, SyntaxNode n)
				{
					formattingRules.AddAlignTokensOperations(l, n, token2);
				}, cancellationToken);
				list.Sort((AlignTokensOperation o1, AlignTokensOperation o2) => o1.BaseToken.Span.CompareTo(o2.BaseToken.Span));
				return list;
			}
		}, cancellationToken);
		Task<List<AnchorIndentationOperation>> anchorIndentationOperationsTask = TaskExecutor.ContinueWith(previousTask, delegate(Task<List<SyntaxNode>> task)
		{
			using (Logger.LogBlock(FunctionId.Formatting_CollectAnchorOperation, cancellationToken))
			{
				return AddOperations(task.Result, delegate(List<AnchorIndentationOperation> l, SyntaxNode n)
				{
					formattingRules.AddAnchorIndentationOperations(l, n, token2);
				}, cancellationToken);
			}
		}, cancellationToken);
		return new NodeOperations(indentBlockOperationTask, suppressOperationTask, anchorIndentationOperationsTask, alignmentOperationTask);
	}

	private List<T> AddOperations<T>(List<SyntaxNode> nodes, Action<List<T>, SyntaxNode> addOperations, CancellationToken cancellationToken)
	{
		Action<List<T>, SyntaxNode> addOperations2 = addOperations;
		ThreadLocal<List<T>> localOperations = new ThreadLocal<List<T>>(() => new List<T>(), trackAllValues: true);
		try
		{
			ThreadLocal<List<T>> localList = new ThreadLocal<List<T>>(() => new List<T>(), trackAllValues: false);
			try
			{
				((nodes.Count > 1000 * Environment.ProcessorCount) ? Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Formatting.TaskExecutor.Concurrent : Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Formatting.TaskExecutor.Synchronous).ForEach(nodes, delegate(SyntaxNode n)
				{
					cancellationToken.ThrowIfCancellationRequested();
					List<T> value = localList.Value;
					addOperations2(value, n);
					foreach (T item in value)
					{
						if (item != null)
						{
							localOperations.Value.Add(item);
						}
					}
					value.Clear();
				}, cancellationToken);
				List<T> list = new List<T>(localOperations.Values.Sum((List<T> v) => v.Count));
				list.AddRange(localOperations.Values.SelectMany((List<T> v) => v));
				return list;
			}
			finally
			{
				if (localList != null)
				{
					((IDisposable)localList).Dispose();
				}
			}
		}
		finally
		{
			if (localOperations != null)
			{
				((IDisposable)localOperations).Dispose();
			}
		}
	}

	private Task<TokenPairWithOperations[]> CreateTokenOperationTask(TokenStream tokenStream, CancellationToken cancellationToken)
	{
		TokenStream tokenStream2 = tokenStream;
		return TaskExecutor.StartNew(delegate
		{
			using (Logger.LogBlock(FunctionId.Formatting_CollectTokenOperation, cancellationToken))
			{
				TokenPairWithOperations[] list = new TokenPairWithOperations[tokenStream2.TokenCount - 1];
				TaskExecutor.ForEach(tokenStream2.TokenIterator, delegate((int, SyntaxToken, SyntaxToken) pair)
				{
					AdjustSpacesOperation adjustSpacesOperation = formattingRules.GetAdjustSpacesOperation(pair.Item2, pair.Item3);
					AdjustNewLinesOperation adjustNewLinesOperation = formattingRules.GetAdjustNewLinesOperation(pair.Item2, pair.Item3);
					list[pair.Item1] = new TokenPairWithOperations(tokenStream2, pair.Item1, adjustSpacesOperation, adjustNewLinesOperation);
				}, cancellationToken);
				return list;
			}
		}, cancellationToken);
	}

	private async Task ApplyTokenOperationsAsync(FormattingContext context, TokenStream tokenStream, Task anchorContextTask, NodeOperations nodeOperations, TokenPairWithOperations[] tokenOperations, CancellationToken cancellationToken)
	{
		OperationApplier applier = new OperationApplier(context, tokenStream, formattingRules);
		ApplySpaceAndWrappingOperations(context, tokenStream, tokenOperations, applier, cancellationToken);
		await anchorContextTask.ConfigureAwait(continueOnCapturedContext: false);
		ApplyAnchorOperations(context, tokenStream, tokenOperations, applier, cancellationToken);
		await ApplySpecialOperationsAsync(context, tokenStream, nodeOperations, applier, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
	}

	private void ApplyBeginningOfTreeTriviaOperation(FormattingContext context, TokenStream tokenStream, CancellationToken cancellationToken)
	{
		TokenStream tokenStream2 = tokenStream;
		if (tokenStream2.FormatBeginningOfTree)
		{
			Action<int, TriviaData> formattingResultApplier = delegate(int i, TriviaData info)
			{
				tokenStream2.ApplyBeginningOfTreeChange(info);
			};
			tokenStream2.GetTriviaDataAtBeginningOfTree().WithIndentation(0, context, formattingRules, cancellationToken).Format(context, formattingRules, formattingResultApplier, cancellationToken);
		}
	}

	private void ApplyEndOfTreeTriviaOperation(FormattingContext context, TokenStream tokenStream, CancellationToken cancellationToken)
	{
		TokenStream tokenStream2 = tokenStream;
		if (tokenStream2.FormatEndOfTree)
		{
			Action<int, TriviaData> formattingResultApplier = delegate(int i, TriviaData info)
			{
				tokenStream2.ApplyEndOfTreeChange(info);
			};
			tokenStream2.GetTriviaDataAtEndOfTree().WithIndentation(0, context, formattingRules, cancellationToken).Format(context, formattingRules, formattingResultApplier, cancellationToken);
		}
	}

	private void ApplyTriviaOperations(FormattingContext context, TokenStream tokenStream, CancellationToken cancellationToken)
	{
		TokenStream tokenStream2 = tokenStream;
		FormattingContext context2 = context;
		Action<int, TriviaData> regularApplier = delegate(int tokenPairIndex, TriviaData info)
		{
			tokenStream2.ApplyChange(tokenPairIndex, info);
		};
		Action<int> body = delegate(int tokenPairIndex)
		{
			tokenStream2.GetTriviaData(tokenPairIndex).Format(context2, formattingRules, regularApplier, cancellationToken, tokenPairIndex);
		};
		TaskExecutor.For(0, tokenStream2.TokenCount - 1, body, cancellationToken);
	}

	private TextSpan GetSpanToFormat()
	{
		int start = (TreeData.IsFirstToken(token1) ? TreeData.StartPosition : token1.SpanStart);
		int end = (TreeData.IsLastToken(token2) ? TreeData.EndPosition : token2.Span.End);
		return TextSpan.FromBounds(start, end);
	}

	private async Task ApplySpecialOperationsAsync(FormattingContext context, TokenStream tokenStream, NodeOperations nodeOperationsCollector, OperationApplier applier, CancellationToken cancellationToken)
	{
		OperationApplier applier2 = applier;
		TokenStream tokenStream2 = tokenStream;
		using (Logger.LogBlock(FunctionId.Formatting_CollectAlignOperation, cancellationToken))
		{
			cancellationToken.ThrowIfCancellationRequested();
			Dictionary<SyntaxToken, int> previousChangesMap = new Dictionary<SyntaxToken, int>();
			(await nodeOperationsCollector.AlignmentOperationTask.ConfigureAwait(continueOnCapturedContext: false)).Do(delegate(AlignTokensOperation operation)
			{
				cancellationToken.ThrowIfCancellationRequested();
				applier2.ApplyAlignment(operation, previousChangesMap, cancellationToken);
			});
			context.GetAllRelativeIndentBlockOperations().Do(delegate(IndentBlockOperation o)
			{
				cancellationToken.ThrowIfCancellationRequested();
				applier2.ApplyBaseTokenIndentationChangesFromTo(FindCorrectBaseTokenOfRelativeIndentBlockOperation(o, tokenStream2), o.StartToken, o.EndToken, previousChangesMap, cancellationToken);
			});
		}
	}

	private void ApplyAnchorOperations(FormattingContext context, TokenStream tokenStream, TokenPairWithOperations[] tokenOperations, OperationApplier applier, CancellationToken cancellationToken)
	{
		OperationApplier applier2 = applier;
		TokenStream tokenStream2 = tokenStream;
		using (Logger.LogBlock(FunctionId.Formatting_ApplyAnchorOperation, cancellationToken))
		{
			IEnumerable<int> source = TaskExecutor.Filter(tokenOperations, (TokenPairWithOperations p) => AnchorOperationCandidate(p), (TokenPairWithOperations p) => p.PairIndex, cancellationToken);
			cancellationToken.ThrowIfCancellationRequested();
			Dictionary<SyntaxToken, int> previousChangesMap = new Dictionary<SyntaxToken, int>();
			source.Do(delegate(int pairIndex)
			{
				cancellationToken.ThrowIfCancellationRequested();
				applier2.ApplyAnchorIndentation(pairIndex, previousChangesMap, cancellationToken);
			});
			context.GetAllRelativeIndentBlockOperations().Do(delegate(IndentBlockOperation o)
			{
				cancellationToken.ThrowIfCancellationRequested();
				applier2.ApplyBaseTokenIndentationChangesFromTo(FindCorrectBaseTokenOfRelativeIndentBlockOperation(o, tokenStream2), o.StartToken, o.EndToken, previousChangesMap, cancellationToken);
			});
		}
	}

	private static bool AnchorOperationCandidate(TokenPairWithOperations pair)
	{
		if (pair.LineOperation == null)
		{
			return pair.TokenStream.GetTriviaData(pair.PairIndex).SecondTokenIsFirstTokenOnLine;
		}
		if (pair.LineOperation.Option == AdjustNewLinesOption.ForceLinesIfOnSingleLine)
		{
			if (!pair.TokenStream.TwoTokensOriginallyOnSameLine(pair.Token1, pair.Token2))
			{
				return pair.TokenStream.GetTriviaData(pair.PairIndex).SecondTokenIsFirstTokenOnLine;
			}
			return false;
		}
		return false;
	}

	private SyntaxToken FindCorrectBaseTokenOfRelativeIndentBlockOperation(IndentBlockOperation operation, TokenStream tokenStream)
	{
		if (operation.Option.IsOn(IndentBlockOption.RelativeToFirstTokenOnBaseTokenLine))
		{
			return tokenStream.FirstTokenOfBaseTokenLine(operation.BaseToken);
		}
		return operation.BaseToken;
	}

	private void ApplySpaceAndWrappingOperations(FormattingContext context, TokenStream tokenStream, TokenPairWithOperations[] tokenOperations, OperationApplier applier, CancellationToken cancellationToken)
	{
		FormattingContext context2 = context;
		TokenStream tokenStream2 = tokenStream;
		OperationApplier applier2 = applier;
		using (Logger.LogBlock(FunctionId.Formatting_ApplySpaceAndLine, cancellationToken))
		{
			List<IEnumerable<TokenPairWithOperations>> partitions = new Partitioner(context2, tokenStream2, tokenOperations).GetPartitions((TaskExecutor == Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Formatting.TaskExecutor.Synchronous) ? 1 : (Environment.ProcessorCount + 1), cancellationToken);
			cancellationToken.ThrowIfCancellationRequested();
			Task[] array = new Task[partitions.Count];
			for (int i = 0; i < partitions.Count; i++)
			{
				IEnumerable<TokenPairWithOperations> partition = partitions[i];
				array[i] = TaskExecutor.StartNew(delegate
				{
					cancellationToken.ThrowIfCancellationRequested();
					partition.Do(delegate(TokenPairWithOperations operationPair)
					{
						ApplySpaceAndWrappingOperationsBody(context2, tokenStream2, operationPair, applier2, cancellationToken);
					});
				}, cancellationToken);
			}
			Task.WaitAll(array, cancellationToken);
		}
	}

	private static void ApplySpaceAndWrappingOperationsBody(FormattingContext context, TokenStream tokenStream, TokenPairWithOperations operation, OperationApplier applier, CancellationToken cancellationToken)
	{
		SyntaxToken syntaxToken = operation.Token1;
		SyntaxToken syntaxToken2 = operation.Token2;
		if (!syntaxToken.IsMissing && !syntaxToken2.IsMissing)
		{
			tokenStream.GetTriviaData(operation.PairIndex);
			TextSpan textSpan = TextSpan.FromBounds(syntaxToken.Span.End, syntaxToken2.SpanStart);
			if ((operation.LineOperation == null || context.IsWrappingSuppressed(textSpan) || !applier.Apply(operation.LineOperation, operation.PairIndex, cancellationToken)) && operation.SpaceOperation != null && !context.IsSpacingSuppressed(textSpan))
			{
				applier.Apply(operation.SpaceOperation, operation.PairIndex);
			}
		}
	}

	private void BuildContext(FormattingContext context, TokenStream tokenStream, NodeOperations nodeOperations, CancellationToken cancellationToken)
	{
		FormattingContext context2 = context;
		using (Logger.LogBlock(FunctionId.Formatting_BuildContext, cancellationToken))
		{
			Task task2 = TaskExecutor.ContinueWith<List<IndentBlockOperation>>(nodeOperations.IndentBlockOperationTask, delegate(Task<List<IndentBlockOperation>> task)
			{
				context2.AddIndentBlockOperations(task.Result, cancellationToken);
			}, cancellationToken);
			Task task3 = TaskExecutor.ContinueWith<List<SuppressOperation>>(nodeOperations.SuppressOperationTask, delegate(Task<List<SuppressOperation>> task)
			{
				context2.AddSuppressOperations(task.Result, cancellationToken);
			}, cancellationToken);
			Task.WaitAll(new Task[2] { task2, task3 }, cancellationToken);
		}
	}

	private string FormatSummary()
	{
		return string.Format("({0}) ({1} - {2}) {3}", SpanToFormat, token1.ToString().Replace("\r\n", "\\r\\n"), token2.ToString().Replace("\r\n", "\\r\\n"), TaskExecutor);
	}
}
