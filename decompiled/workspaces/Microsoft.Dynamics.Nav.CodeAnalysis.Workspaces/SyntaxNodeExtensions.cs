using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces;

internal static class SyntaxNodeExtensions
{
	private static readonly Func<SyntaxTriviaList, int, SyntaxToken> s_findSkippedTokenForward = FindSkippedTokenForward;

	private static readonly Func<SyntaxTriviaList, int, SyntaxToken> s_findSkippedTokenBackward = FindSkippedTokenBackward;

	public static IEnumerable<SyntaxNodeOrToken> DepthFirstTraversal(this SyntaxNode node)
	{
		return SyntaxNodeOrTokenExtensions.DepthFirstTraversal(node);
	}

	public static IEnumerable<SyntaxNode> GetAncestors(this SyntaxNode node)
	{
		for (SyntaxNode current = node.Parent; current != null; current = current.GetParent())
		{
			yield return current;
		}
	}

	public static bool IsAnyArgumentList(this SyntaxNode node)
	{
		if (!node.IsKind(SyntaxKind.ArgumentList) && !node.IsKind(SyntaxKind.AttributeArgumentList))
		{
			return node.IsKind(SyntaxKind.BracketedArgumentList);
		}
		return true;
	}

	public static IEnumerable<SyntaxTrivia> GetAllPrecedingTriviaToPreviousToken(this SyntaxToken token, SourceText sourceText = null, bool includePreviousTokenTrailingTriviaOnlyIfOnSameLine = false)
	{
		SyntaxToken previousToken = token.GetPreviousToken(includeZeroWidth: false, includeSkipped: true);
		if (previousToken.Kind == SyntaxKind.None)
		{
			return token.LeadingTrivia;
		}
		if (includePreviousTokenTrailingTriviaOnlyIfOnSameLine && !sourceText.AreOnSameLine(previousToken, token))
		{
			return token.LeadingTrivia;
		}
		return previousToken.TrailingTrivia.Concat(token.LeadingTrivia);
	}

	public static IEnumerable<SyntaxTrivia> GetAllTrailingTriviaToNextToken(this SyntaxToken token, SourceText sourceText = null, bool includeNextTokenLeadingTriviaOnlyIfOnSameLine = false)
	{
		SyntaxToken nextToken = token.GetNextToken(includeZeroWidth: false, includeSkipped: true);
		if (nextToken.Kind == SyntaxKind.None)
		{
			return token.TrailingTrivia;
		}
		if (includeNextTokenLeadingTriviaOnlyIfOnSameLine && !sourceText.AreOnSameLine(nextToken, token))
		{
			return token.TrailingTrivia;
		}
		return nextToken.LeadingTrivia.Concat(token.TrailingTrivia);
	}

	public static IEnumerable<TNode> GetAncestors<TNode>(this SyntaxNode node) where TNode : SyntaxNode
	{
		for (SyntaxNode current = node.Parent; current != null; current = current.GetParent())
		{
			if (current is TNode val)
			{
				yield return val;
			}
		}
	}

	private static TNode GetAncestor<TNode>(this SyntaxNode node) where TNode : SyntaxNode
	{
		for (SyntaxNode parent = node.Parent; parent != null; parent = parent.GetParent())
		{
			if (parent is TNode result)
			{
				return result;
			}
		}
		return null;
	}

	public static TNode GetAncestorOrThis<TNode>(this SyntaxNode node) where TNode : SyntaxNode
	{
		if (node == null)
		{
			return null;
		}
		return node.GetAncestorsOrThis<TNode>().FirstOrDefault();
	}

	public static IEnumerable<TNode> GetAncestorsOrThis<TNode>(this SyntaxNode node) where TNode : SyntaxNode
	{
		for (SyntaxNode current = node; current != null; current = current.GetParent())
		{
			if (current is TNode val)
			{
				yield return val;
			}
		}
	}

	public static bool HasAncestor<TNode>(this SyntaxNode node) where TNode : SyntaxNode
	{
		return node.GetAncestors<TNode>().Any();
	}

	public static IEnumerable<TSyntaxNode> Traverse<TSyntaxNode>(this SyntaxNode node, TextSpan searchSpan, Func<SyntaxNode, bool> predicate) where TSyntaxNode : SyntaxNode
	{
		Contract.ThrowIfNull(node);
		LinkedList<SyntaxNode> nodes = new LinkedList<SyntaxNode>();
		nodes.AddFirst(node);
		while (nodes.Count > 0)
		{
			SyntaxNode currentNode = nodes.First.Value;
			nodes.RemoveFirst();
			if (currentNode != null && searchSpan.Contains(currentNode.FullSpan) && predicate(currentNode))
			{
				if (currentNode is TSyntaxNode val)
				{
					yield return val;
				}
				nodes.AddRangeAtHead(currentNode.ChildNodes());
			}
		}
	}

	public static bool CheckParent<T>(this SyntaxNode node, Func<T, bool> valueChecker) where T : SyntaxNode
	{
		if (!(node?.Parent is T arg))
		{
			return false;
		}
		return valueChecker(arg);
	}

	public static bool IsChildNode<TParent>(this SyntaxNode node, Func<TParent, SyntaxNode> childGetter) where TParent : SyntaxNode
	{
		TParent ancestor = node.GetAncestor<TParent>();
		if (ancestor == null)
		{
			return false;
		}
		SyntaxNode syntaxNode = childGetter(ancestor);
		return node == syntaxNode;
	}

	public static bool IsFoundUnder<TParent>(this SyntaxNode node, Func<TParent, SyntaxNode> childGetter) where TParent : SyntaxNode
	{
		TParent ancestor = node.GetAncestor<TParent>();
		if (ancestor == null)
		{
			return false;
		}
		SyntaxNode value = childGetter(ancestor);
		return node.GetAncestorsOrThis<SyntaxNode>().Contains(value);
	}

	public static SyntaxNode GetCommonRoot(this SyntaxNode node1, SyntaxNode node2)
	{
		Contract.ThrowIfTrue(node1.Kind == SyntaxKind.None || node2.Kind == SyntaxKind.None);
		IEnumerable<SyntaxNode> ancestorsOrThis = node1.GetAncestorsOrThis<SyntaxNode>();
		HashSet<SyntaxNode> hashSet = new HashSet<SyntaxNode>(node2.GetAncestorsOrThis<SyntaxNode>());
		return ancestorsOrThis.First(hashSet.Contains);
	}

	public static int Width(this SyntaxNode node)
	{
		return node.Span.Length;
	}

	public static int FullWidth(this SyntaxNode node)
	{
		return node.FullSpan.Length;
	}

	public static SyntaxNode FindInnermostCommonNode(this IEnumerable<SyntaxNode> nodes, Func<SyntaxNode, bool> predicate)
	{
		IEnumerable<SyntaxNode> enumerable = null;
		foreach (SyntaxNode node in nodes)
		{
			enumerable = ((enumerable == null) ? node.AncestorsAndSelf().Where(predicate) : enumerable.Intersect(node.AncestorsAndSelf().Where(predicate)));
		}
		return enumerable?.First();
	}

	public static TSyntaxNode FindInnermostCommonNode<TSyntaxNode>(this IEnumerable<SyntaxNode> nodes) where TSyntaxNode : SyntaxNode
	{
		return (TSyntaxNode)nodes.FindInnermostCommonNode((SyntaxNode n) => n is TSyntaxNode);
	}

	public static SyntaxNode AddAnnotations(this SyntaxNode root, IEnumerable<Tuple<SyntaxToken, SyntaxAnnotation>> pairs)
	{
		Contract.ThrowIfNull(root);
		Contract.ThrowIfNull(pairs);
		Dictionary<SyntaxToken, SyntaxAnnotation[]> tokenMap = (from p in pairs
			group p.Item2 by p.Item1).ToDictionary((IGrouping<SyntaxToken, SyntaxAnnotation> g) => g.Key, (IGrouping<SyntaxToken, SyntaxAnnotation> g) => g.ToArray());
		return root.ReplaceTokens(tokenMap.Keys, (SyntaxToken o, SyntaxToken n) => o.WithAdditionalAnnotations(tokenMap[o]));
	}

	public static SyntaxNode AddAnnotations(this SyntaxNode root, IEnumerable<Tuple<SyntaxNode, SyntaxAnnotation>> pairs)
	{
		Contract.ThrowIfNull(root);
		Contract.ThrowIfNull(pairs);
		Dictionary<SyntaxNode, SyntaxAnnotation[]> tokenMap = (from p in pairs
			group p.Item2 by p.Item1).ToDictionary((IGrouping<SyntaxNode, SyntaxAnnotation> g) => g.Key, (IGrouping<SyntaxNode, SyntaxAnnotation> g) => g.ToArray());
		return root.ReplaceNodes(tokenMap.Keys, (SyntaxNode o, SyntaxNode n) => o.WithAdditionalAnnotations(tokenMap[o]));
	}

	public static TextSpan GetContainedSpan(this IEnumerable<SyntaxNode> nodes)
	{
		Contract.ThrowIfNull(nodes);
		Contract.ThrowIfFalse(nodes.Any());
		TextSpan result = nodes.First().Span;
		foreach (SyntaxNode node in nodes)
		{
			result = TextSpan.FromBounds(Math.Min(result.Start, node.SpanStart), Math.Max(result.End, node.Span.End));
		}
		return result;
	}

	public static IEnumerable<TextSpan> GetContiguousSpans(this IEnumerable<SyntaxNode> nodes, Func<SyntaxNode, SyntaxToken> getLastToken = null)
	{
		SyntaxNode syntaxNode = null;
		TextSpan? textSpan = null;
		foreach (SyntaxNode node in nodes.OrderBy((SyntaxNode n) => n.SpanStart))
		{
			if (syntaxNode == null)
			{
				textSpan = node.Span;
			}
			else if ((getLastToken?.Invoke(syntaxNode) ?? syntaxNode.GetLastToken()).GetNextToken() == node.GetFirstToken())
			{
				textSpan = TextSpan.FromBounds(textSpan.Value.Start, node.Span.End);
			}
			else
			{
				yield return textSpan.Value;
				textSpan = node.Span;
			}
			syntaxNode = node;
		}
		if (textSpan.HasValue)
		{
			yield return textSpan.Value;
		}
	}

	public static bool OverlapsHiddenPosition(this SyntaxNode node, CancellationToken cancellationToken)
	{
		return node.OverlapsHiddenPosition(node.Span, cancellationToken);
	}

	public static bool OverlapsHiddenPosition(this SyntaxNode node, TextSpan span, CancellationToken cancellationToken)
	{
		return node.SyntaxTree.OverlapsHiddenPosition(span, cancellationToken);
	}

	public static bool OverlapsHiddenPosition(this SyntaxNode declaration, SyntaxNode startNode, SyntaxNode endNode, CancellationToken cancellationToken)
	{
		int end = startNode.Span.End;
		int spanStart = endNode.SpanStart;
		TextSpan span = TextSpan.FromBounds(end, spanStart);
		return declaration.OverlapsHiddenPosition(span, cancellationToken);
	}

	public static IEnumerable<T> GetAnnotatedNodes<T>(this SyntaxNode node, SyntaxAnnotation syntaxAnnotation) where T : SyntaxNode
	{
		return (from n in node.GetAnnotatedNodesAndTokens(syntaxAnnotation)
			select n.AsNode()).OfType<T>();
	}

	public static Task<TRootNode> ReplaceNodesAsync<TRootNode>(this TRootNode root, IEnumerable<SyntaxNode> nodes, Func<SyntaxNode, SyntaxNode, CancellationToken, Task<SyntaxNode>> computeReplacementAsync, CancellationToken cancellationToken) where TRootNode : SyntaxNode
	{
		return root.ReplaceSyntaxAsync(nodes, computeReplacementAsync, null, null, null, null, cancellationToken);
	}

	public static Task<TRootNode> ReplaceTokensAsync<TRootNode>(this TRootNode root, IEnumerable<SyntaxToken> tokens, Func<SyntaxToken, SyntaxToken, CancellationToken, Task<SyntaxToken>> computeReplacementAsync, CancellationToken cancellationToken) where TRootNode : SyntaxNode
	{
		return root.ReplaceSyntaxAsync(null, null, tokens, computeReplacementAsync, null, null, cancellationToken);
	}

	public static Task<TRoot> ReplaceTriviaAsync<TRoot>(this TRoot root, IEnumerable<SyntaxTrivia> trivia, Func<SyntaxTrivia, SyntaxTrivia, CancellationToken, Task<SyntaxTrivia>> computeReplacementAsync, CancellationToken cancellationToken) where TRoot : SyntaxNode
	{
		return root.ReplaceSyntaxAsync(null, null, null, null, trivia, computeReplacementAsync, cancellationToken);
	}

	public static int IndexOf(this SyntaxTriviaList list, SyntaxKind kind)
	{
		return list.IndexOf((int)kind);
	}

	public static bool Any(this SyntaxTriviaList list, SyntaxKind kind)
	{
		return list.IndexOf(kind) >= 0;
	}

	public static async Task<TRoot> ReplaceSyntaxAsync<TRoot>(this TRoot root, IEnumerable<SyntaxNode> nodes, Func<SyntaxNode, SyntaxNode, CancellationToken, Task<SyntaxNode>> computeReplacementNodeAsync, IEnumerable<SyntaxToken> tokens, Func<SyntaxToken, SyntaxToken, CancellationToken, Task<SyntaxToken>> computeReplacementTokenAsync, IEnumerable<SyntaxTrivia> trivia, Func<SyntaxTrivia, SyntaxTrivia, CancellationToken, Task<SyntaxTrivia>> computeReplacementTriviaAsync, CancellationToken cancellationToken) where TRoot : SyntaxNode
	{
		Dictionary<TextSpan, SyntaxNode> nodesToReplace = ((nodes != null) ? nodes.ToDictionary((SyntaxNode n) => n.FullSpan) : new Dictionary<TextSpan, SyntaxNode>());
		Dictionary<TextSpan, SyntaxToken> tokensToReplace = ((tokens != null) ? tokens.ToDictionary((SyntaxToken t) => t.FullSpan) : new Dictionary<TextSpan, SyntaxToken>());
		Dictionary<TextSpan, SyntaxTrivia> triviaToReplace = ((trivia != null) ? trivia.ToDictionary((SyntaxTrivia t) => t.FullSpan) : new Dictionary<TextSpan, SyntaxTrivia>());
		Dictionary<SyntaxNode, SyntaxNode> nodeReplacements = new Dictionary<SyntaxNode, SyntaxNode>();
		Dictionary<SyntaxToken, SyntaxToken> tokenReplacements = new Dictionary<SyntaxToken, SyntaxToken>();
		Dictionary<SyntaxTrivia, SyntaxTrivia> triviaReplacements = new Dictionary<SyntaxTrivia, SyntaxTrivia>();
		AnnotationTable<object> retryAnnotations = new AnnotationTable<object>(AnnotationKind.RetryReplace);
		List<TextSpan> spans = new List<TextSpan>(nodesToReplace.Count + tokensToReplace.Count + triviaToReplace.Count);
		spans.AddRange(nodesToReplace.Keys);
		spans.AddRange(tokensToReplace.Keys);
		spans.AddRange(triviaToReplace.Keys);
		while (spans.Count > 0)
		{
			spans.Sort(delegate(TextSpan x, TextSpan y)
			{
				int num = x.End - y.End;
				if (num == 0)
				{
					num = x.Length - y.Length;
				}
				return num;
			});
			TextSpan textSpan = default(TextSpan);
			foreach (TextSpan span in spans)
			{
				if (textSpan == default(TextSpan) || !textSpan.IntersectsWith(span))
				{
					if (nodesToReplace.TryGetValue(span, out SyntaxNode currentNode))
					{
						SyntaxNode arg = ((SyntaxNode)retryAnnotations.GetAnnotations(currentNode).SingleOrDefault()) ?? currentNode;
						SyntaxNode value = await computeReplacementNodeAsync(arg, currentNode, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
						nodeReplacements[currentNode] = value;
					}
					else
					{
						if (tokensToReplace.TryGetValue(span, out var currentToken))
						{
							SyntaxToken syntaxToken = (SyntaxToken)retryAnnotations.GetAnnotations(currentToken).SingleOrDefault();
							if (syntaxToken == default(SyntaxToken))
							{
								syntaxToken = currentToken;
							}
							SyntaxToken value2 = await computeReplacementTokenAsync(syntaxToken, currentToken, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
							tokenReplacements[currentToken] = value2;
						}
						else
						{
							if (triviaToReplace.TryGetValue(span, out var currentTrivia))
							{
								SyntaxTrivia syntaxTrivia = (SyntaxTrivia)retryAnnotations.GetAnnotations(currentTrivia).SingleOrDefault();
								if (syntaxTrivia == default(SyntaxTrivia))
								{
									syntaxTrivia = currentTrivia;
								}
								SyntaxTrivia value3 = await computeReplacementTriviaAsync(syntaxTrivia, currentTrivia, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
								triviaReplacements[currentTrivia] = value3;
							}
							currentTrivia = default(SyntaxTrivia);
						}
						currentToken = default(SyntaxToken);
					}
					currentNode = null;
				}
				textSpan = span;
			}
			bool retryNodes = false;
			bool retryTokens = false;
			bool retryTrivia = false;
			root = root.ReplaceSyntax(nodesToReplace.Values, delegate(SyntaxNode original, SyntaxNode rewritten)
			{
				if (rewritten != original || !nodeReplacements.TryGetValue(original, out SyntaxNode value6))
				{
					value6 = retryAnnotations.WithAdditionalAnnotations(rewritten, original);
					retryNodes = true;
				}
				return value6;
			}, tokensToReplace.Values, delegate(SyntaxToken original, SyntaxToken rewritten)
			{
				if (rewritten != original || !tokenReplacements.TryGetValue(original, out var value5))
				{
					value5 = retryAnnotations.WithAdditionalAnnotations(rewritten, original);
					retryTokens = true;
				}
				return value5;
			}, triviaToReplace.Values, delegate(SyntaxTrivia original, SyntaxTrivia rewritten)
			{
				if (!triviaReplacements.TryGetValue(original, out var value4))
				{
					value4 = retryAnnotations.WithAdditionalAnnotations(rewritten, original);
					retryTrivia = true;
				}
				return value4;
			});
			nodesToReplace.Clear();
			tokensToReplace.Clear();
			triviaToReplace.Clear();
			spans.Clear();
			if (retryNodes)
			{
				nodesToReplace = retryAnnotations.GetAnnotatedNodes(root).ToDictionary((SyntaxNode n) => n.FullSpan);
				spans.AddRange(nodesToReplace.Keys);
			}
			if (retryTokens)
			{
				tokensToReplace = retryAnnotations.GetAnnotatedTokens(root).ToDictionary((SyntaxToken t) => t.FullSpan);
				spans.AddRange(tokensToReplace.Keys);
			}
			if (retryTrivia)
			{
				triviaToReplace = retryAnnotations.GetAnnotatedTrivia(root).ToDictionary((SyntaxTrivia t) => t.FullSpan);
				spans.AddRange(triviaToReplace.Keys);
			}
		}
		return root;
	}

	private static SyntaxToken FindSkippedTokenForward(SyntaxTriviaList triviaList, int position)
	{
		SyntaxTriviaList.Enumerator enumerator = triviaList.GetEnumerator();
		while (enumerator.MoveNext())
		{
			SyntaxTrivia current = enumerator.Current;
			if (!current.HasStructure || !(current.GetStructure() is SkippedTokensTriviaSyntax { Tokens: var tokens }))
			{
				continue;
			}
			SyntaxTokenList.Enumerator enumerator2 = tokens.GetEnumerator();
			while (enumerator2.MoveNext())
			{
				SyntaxToken current2 = enumerator2.Current;
				if (current2.Span.Length > 0 && position <= current2.Span.End)
				{
					return current2;
				}
			}
		}
		return default(SyntaxToken);
	}

	private static SyntaxToken FindSkippedTokenBackward(SyntaxTriviaList triviaList, int position)
	{
		SyntaxTriviaList.Reversed.Enumerator enumerator = triviaList.Reverse().GetEnumerator();
		while (enumerator.MoveNext())
		{
			SyntaxTrivia current = enumerator.Current;
			if (!current.HasStructure || !(current.GetStructure() is SkippedTokensTriviaSyntax { Tokens: var tokens }))
			{
				continue;
			}
			SyntaxTokenList.Enumerator enumerator2 = tokens.GetEnumerator();
			while (enumerator2.MoveNext())
			{
				SyntaxToken current2 = enumerator2.Current;
				if (current2.Span.Length > 0 && current2.SpanStart <= position)
				{
					return current2;
				}
			}
		}
		return default(SyntaxToken);
	}

	private static SyntaxToken GetInitialToken(SyntaxNode root, int position, bool includeSkipped = false, bool includeDirectives = false, bool includeDocumentationComments = false)
	{
		if (position >= root.FullSpan.End && root is CompilationUnitSyntax)
		{
			return root.GetLastToken(includeZeroWidth: true, includeSkipped: true).GetPreviousToken(includeZeroWidth: false, includeSkipped);
		}
		return root.FindToken(position, includeSkipped || includeDirectives || includeDocumentationComments);
	}

	public static SyntaxToken FindTokenOnRightOfPosition(this SyntaxNode root, int position, bool includeSkipped = false, bool includeDirectives = false, bool includeDocumentationComments = false)
	{
		Func<SyntaxTriviaList, int, SyntaxToken> func = (includeSkipped ? s_findSkippedTokenForward : ((Func<SyntaxTriviaList, int, SyntaxToken>)((SyntaxTriviaList l, int p) => default(SyntaxToken))));
		SyntaxToken syntaxToken = GetInitialToken(root, position, includeSkipped, includeDirectives, includeDocumentationComments);
		if (position < syntaxToken.SpanStart)
		{
			SyntaxToken syntaxToken2 = func(syntaxToken.LeadingTrivia, position);
			syntaxToken = ((syntaxToken2.Kind != 0) ? syntaxToken2 : syntaxToken);
		}
		else if (syntaxToken.Span.End <= position)
		{
			do
			{
				SyntaxToken syntaxToken3 = func(syntaxToken.TrailingTrivia, position);
				syntaxToken = ((syntaxToken3.Kind != 0) ? syntaxToken3 : syntaxToken.GetNextToken(includeZeroWidth: false, includeSkipped));
			}
			while (syntaxToken.Kind != 0 && syntaxToken.Span.End <= position && syntaxToken.Span.End <= root.FullSpan.End);
		}
		if (syntaxToken.Span.Length == 0)
		{
			syntaxToken = syntaxToken.GetNextToken();
		}
		return syntaxToken;
	}

	public static SyntaxToken FindTokenOnLeftOfPosition(this SyntaxNode root, int position, bool includeSkipped = false, bool includeDirectives = false, bool includeDocumentationComments = false)
	{
		Func<SyntaxTriviaList, int, SyntaxToken> func = (includeSkipped ? s_findSkippedTokenBackward : ((Func<SyntaxTriviaList, int, SyntaxToken>)((SyntaxTriviaList l, int p) => default(SyntaxToken))));
		SyntaxToken syntaxToken = GetInitialToken(root, position, includeSkipped, includeDirectives, includeDocumentationComments);
		if (position <= syntaxToken.SpanStart)
		{
			do
			{
				SyntaxToken syntaxToken2 = func(syntaxToken.LeadingTrivia, position);
				syntaxToken = ((syntaxToken2.Kind != 0) ? syntaxToken2 : syntaxToken.GetPreviousToken(includeZeroWidth: false, includeSkipped, includeDirectives, includeDocumentationComments));
			}
			while (position <= syntaxToken.SpanStart && root.FullSpan.Start < syntaxToken.SpanStart);
		}
		else if (syntaxToken.Span.End < position)
		{
			SyntaxToken syntaxToken3 = func(syntaxToken.TrailingTrivia, position);
			syntaxToken = ((syntaxToken3.Kind != 0) ? syntaxToken3 : syntaxToken);
		}
		if (syntaxToken.Span.Length == 0)
		{
			syntaxToken = syntaxToken.GetPreviousToken();
		}
		return syntaxToken;
	}

	public static T WithPrependedLeadingTrivia<T>(this T node, params SyntaxTrivia[] trivia) where T : SyntaxNode
	{
		if (trivia.Length == 0)
		{
			return node;
		}
		return node.WithPrependedLeadingTrivia((IEnumerable<SyntaxTrivia>)trivia);
	}

	public static T WithPrependedLeadingTrivia<T>(this T node, SyntaxTriviaList trivia) where T : SyntaxNode
	{
		if (trivia.Count == 0)
		{
			return node;
		}
		return node.WithLeadingTrivia(trivia.Concat(node.GetLeadingTrivia()));
	}

	public static T WithPrependedLeadingTrivia<T>(this T node, IEnumerable<SyntaxTrivia> trivia) where T : SyntaxNode
	{
		SyntaxTriviaList trivia2 = default(SyntaxTriviaList).AddRange(trivia);
		return node.WithPrependedLeadingTrivia(trivia2);
	}

	public static T WithAppendedTrailingTrivia<T>(this T node, params SyntaxTrivia[] trivia) where T : SyntaxNode
	{
		if (trivia.Length == 0)
		{
			return node;
		}
		return node.WithAppendedTrailingTrivia((IEnumerable<SyntaxTrivia>)trivia);
	}

	public static T WithAppendedTrailingTrivia<T>(this T node, SyntaxTriviaList trivia) where T : SyntaxNode
	{
		if (trivia.Count == 0)
		{
			return node;
		}
		return node.WithTrailingTrivia(node.GetTrailingTrivia().Concat(trivia));
	}

	public static T WithAppendedTrailingTrivia<T>(this T node, IEnumerable<SyntaxTrivia> trivia) where T : SyntaxNode
	{
		SyntaxTriviaList trivia2 = default(SyntaxTriviaList).AddRange(trivia);
		return node.WithAppendedTrailingTrivia(trivia2);
	}

	public static T With<T>(this T node, IEnumerable<SyntaxTrivia> leadingTrivia, IEnumerable<SyntaxTrivia> trailingTrivia) where T : SyntaxNode
	{
		return node.WithLeadingTrivia(leadingTrivia).WithTrailingTrivia(trailingTrivia);
	}

	private static SyntaxNode GetParent(this SyntaxNode node)
	{
		if (!(node is IStructuredTriviaSyntax { ParentTrivia: { Token: var token } }))
		{
			return node.Parent;
		}
		return token.Parent;
	}

	public static bool IsParentKind(this SyntaxNode node, SyntaxKind kind)
	{
		return Microsoft.Dynamics.Nav.CodeAnalysis.Syntax.SyntaxNodeExtensions.IsKind(node?.Parent, kind);
	}

	public static TNode FirstAncestorOrSelfUntil<TNode>(this SyntaxNode node, Func<SyntaxNode, bool> predicate) where TNode : SyntaxNode
	{
		for (SyntaxNode syntaxNode = node; syntaxNode != null; syntaxNode = syntaxNode.GetParent())
		{
			if (syntaxNode is TNode result)
			{
				return result;
			}
			if (predicate(syntaxNode))
			{
				break;
			}
		}
		return null;
	}
}
