using System.Collections.Generic;
using System.Linq;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Formatting.Rules;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Options;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Formatting;

internal class ChainedFormattingRules
{
	private readonly ActionCache<AlignTokensOperation> alignFuncCache;

	private readonly ActionCache<AnchorIndentationOperation> anchorFuncCache;

	private readonly List<IFormattingRule> formattingRules;

	private readonly ActionCache<IndentBlockOperation> indentFuncCache;

	private readonly OperationCache<AdjustNewLinesOperation> newLinesFuncCache;

	private readonly OptionSet optionSet;

	private readonly OperationCache<AdjustSpacesOperation> spaceFuncCache;

	private readonly ActionCache<SuppressOperation> _suppressWrappingFuncCache;

	public ChainedFormattingRules(IEnumerable<IFormattingRule> formattingRules, OptionSet set)
	{
		Contract.ThrowIfNull(formattingRules);
		Contract.ThrowIfNull(set);
		this.formattingRules = formattingRules.ToList();
		optionSet = set;
		_suppressWrappingFuncCache = new ActionCache<SuppressOperation>(delegate(int index, List<SuppressOperation> list, SyntaxNode node, SyntaxToken lastToken, NextAction<SuppressOperation> next)
		{
			this.formattingRules[index].AddSuppressOperations(list, node, lastToken, optionSet, next);
		}, AddContinuedOperations);
		anchorFuncCache = new ActionCache<AnchorIndentationOperation>(delegate(int index, List<AnchorIndentationOperation> list, SyntaxNode node, SyntaxToken lastToken, NextAction<AnchorIndentationOperation> next)
		{
			this.formattingRules[index].AddAnchorIndentationOperations(list, node, optionSet, next);
		}, AddContinuedOperations);
		indentFuncCache = new ActionCache<IndentBlockOperation>(delegate(int index, List<IndentBlockOperation> list, SyntaxNode node, SyntaxToken lastToken, NextAction<IndentBlockOperation> next)
		{
			this.formattingRules[index].AddIndentBlockOperations(list, node, optionSet, next);
		}, AddContinuedOperations);
		alignFuncCache = new ActionCache<AlignTokensOperation>(delegate(int index, List<AlignTokensOperation> list, SyntaxNode node, SyntaxToken lastToken, NextAction<AlignTokensOperation> next)
		{
			this.formattingRules[index].AddAlignTokensOperations(list, node, optionSet, next);
		}, AddContinuedOperations);
		newLinesFuncCache = new OperationCache<AdjustNewLinesOperation>((int index, SyntaxToken token1, SyntaxToken token2, NextOperation<AdjustNewLinesOperation> next) => this.formattingRules[index].GetAdjustNewLinesOperation(token1, token2, optionSet, next), GetContinuedOperations);
		spaceFuncCache = new OperationCache<AdjustSpacesOperation>((int index, SyntaxToken token1, SyntaxToken token2, NextOperation<AdjustSpacesOperation> next) => this.formattingRules[index].GetAdjustSpacesOperation(token1, token2, optionSet, next), GetContinuedOperations);
	}

	public void AddSuppressOperations(List<SuppressOperation> list, SyntaxNode currentNode, SyntaxToken lastToken)
	{
		AddContinuedOperations(0, list, currentNode, lastToken, _suppressWrappingFuncCache);
	}

	public void AddAnchorIndentationOperations(List<AnchorIndentationOperation> list, SyntaxNode currentNode, SyntaxToken lastToken)
	{
		AddContinuedOperations(0, list, currentNode, lastToken, anchorFuncCache);
	}

	public void AddIndentBlockOperations(List<IndentBlockOperation> list, SyntaxNode currentNode, SyntaxToken lastToken)
	{
		AddContinuedOperations(0, list, currentNode, lastToken, indentFuncCache);
	}

	public void AddAlignTokensOperations(List<AlignTokensOperation> list, SyntaxNode currentNode, SyntaxToken lastToken)
	{
		AddContinuedOperations(0, list, currentNode, lastToken, alignFuncCache);
	}

	public AdjustNewLinesOperation GetAdjustNewLinesOperation(SyntaxToken previousToken, SyntaxToken currentToken)
	{
		return GetContinuedOperations(0, previousToken, currentToken, newLinesFuncCache);
	}

	public AdjustSpacesOperation GetAdjustSpacesOperation(SyntaxToken previousToken, SyntaxToken currentToken)
	{
		return GetContinuedOperations(0, previousToken, currentToken, spaceFuncCache);
	}

	private void AddContinuedOperations<TArg1>(int index, List<TArg1> arg1, SyntaxNode node, SyntaxToken lastToken, IActionHolder<TArg1> actionCache)
	{
		if (index < formattingRules.Count)
		{
			NextAction<TArg1> arg2 = new NextAction<TArg1>(index + 1, node, lastToken, actionCache);
			actionCache.NextOperation(index, arg1, node, lastToken, arg2);
		}
	}

	private TResult GetContinuedOperations<TResult>(int index, SyntaxToken token1, SyntaxToken token2, IOperationHolder<TResult> funcCache)
	{
		if (index >= formattingRules.Count)
		{
			return default(TResult);
		}
		NextOperation<TResult> arg = new NextOperation<TResult>(index + 1, token1, token2, funcCache);
		return funcCache.NextOperation(index, token1, token2, arg);
	}
}
