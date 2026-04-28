using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.CodeAnalysis.CodeActions;
using Microsoft.Dynamics.Nav.CodeAnalysis.CodeActions.Mef;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Semantics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Formatting;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.CodeFixes.Providers;

[CodeFixProvider("ExplicitWithConverterCodeFixProvider")]
public class ExplicitWithConverterCodeFixProvider : CodeFixProvider
{
	private class WithStatementVisitor : OperationWalker
	{
		private readonly IWithStatement withStatement;

		public List<SyntaxNode> Nodes = new List<SyntaxNode>();

		public WithStatementVisitor(IWithStatement withStatement)
		{
			this.withStatement = withStatement;
		}

		public override void VisitArgument(IArgument operation)
		{
			if (operation.Parameter == null || !operation.Parameter.IsMemberReference)
			{
				base.VisitArgument(operation);
			}
		}

		public override void VisitFieldAccess(IFieldAccess operation)
		{
			base.VisitFieldAccess(operation);
			AddIfInstanceIsFromWithStatement(operation, operation.Instance);
		}

		public override void VisitTestFieldAccess(ITestFieldAccess operation)
		{
			base.VisitTestFieldAccess(operation);
			AddIfInstanceIsFromWithStatement(operation, operation.Instance);
		}

		public override void VisitTestFilterAccess(ITestFilterAccess operation)
		{
			base.VisitTestFilterAccess(operation);
			AddIfInstanceIsFromWithStatement(operation, operation.Instance);
		}

		public override void VisitTestFilterFieldAccess(ITestFilterFieldAccess operation)
		{
			base.VisitTestFilterFieldAccess(operation);
			if (operation.Instance is ITestFilterAccess operation2)
			{
				VisitTestFilterAccess(operation2);
			}
		}

		public override void VisitInvocationExpression(IInvocationExpression operation)
		{
			base.VisitInvocationExpression(operation);
			if (operation.Instance == withStatement.Value)
			{
				SyntaxNode item;
				switch (operation.Syntax.Kind)
				{
				case SyntaxKind.InvocationExpression:
					item = ((InvocationExpressionSyntax)operation.Syntax).Expression;
					break;
				case SyntaxKind.IdentifierName:
					item = (ExpressionSyntax)operation.Syntax;
					break;
				case SyntaxKind.AssignmentStatement:
					item = ((AssignmentStatementSyntax)operation.Syntax).Target;
					break;
				default:
					ExceptionUtilities.UnexpectedValue(operation.Syntax.Kind);
					return;
				}
				if (!Nodes.Contains(operation.Syntax))
				{
					Nodes.Add(item);
				}
			}
		}

		private void AddIfInstanceIsFromWithStatement(IOperation operation, IOperation instance)
		{
			if (instance == withStatement.Value && !Nodes.Contains(operation.Syntax))
			{
				Nodes.Add(operation.Syntax);
			}
		}
	}

	private class ExplicitWithConverterCodeAction : CodeAction.DocumentChangeAction
	{
		public override CodeActionKind Kind => CodeActionKind.QuickFix;

		public override bool SupportsFixAll { get; }

		public override string? FixAllSingleInstanceTitle => Title;

		public override string? FixAllTitle => string.Empty;

		public ExplicitWithConverterCodeAction(string title, Func<CancellationToken, Task<Document>> createChangedDocument, string equivalenceKey, bool generateFixAll)
			: base(title, createChangedDocument, equivalenceKey)
		{
			SupportsFixAll = generateFixAll;
		}
	}

	internal class ExplicitWithConverterFixAllProvider : DocumentBasedFixAllByDiagnosticsProvider
	{
		private static readonly ImmutableArray<FixAllScope> supportedFixAllScopes = new FixAllScope[3]
		{
			FixAllScope.Document,
			FixAllScope.Project,
			FixAllScope.Workspace
		}.ToImmutableArray();

		public static ExplicitWithConverterFixAllProvider Instance { get; } = new ExplicitWithConverterFixAllProvider();


		public ExplicitWithConverterFixAllProvider()
			: base(supportedFixAllScopes)
		{
		}

		public override string? GetOverrideFixAllTitle(FixAllScope scope)
		{
			return string.Format(CultureInfo.CurrentCulture, WorkspacesResources.ConvertAllExplicitWith, scope.ToDisplayString().ToLower());
		}

		public override IEnumerable<string> GetSupportedFixAllDiagnosticIds(CodeFixProvider originalCodeFixProvider)
		{
			return base.GetSupportedFixAllDiagnosticIds(originalCodeFixProvider);
		}

		protected override async Task<Document?> FixAllAsync(FixAllContext fixAllContext, Document document, ImmutableArray<Diagnostic> diagnostics)
		{
			if (diagnostics.IsEmpty)
			{
				return document;
			}
			CancellationToken cancellationToken = fixAllContext.CancellationToken;
			ImmutableHashSet<IWithStatement> immutableHashSet = await GetWithStatementNodesToFix(document, diagnostics, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
			if (immutableHashSet.IsEmpty)
			{
				return document;
			}
			return await UpdateAll(document, immutableHashSet, cancellationToken);
		}

		private static async Task<ImmutableHashSet<IWithStatement>> GetWithStatementNodesToFix(Document document, ImmutableArray<Diagnostic> diagnostics, CancellationToken cancellationToken)
		{
			SyntaxNode syntaxRoot = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
			using PooledHashSet<IWithStatement> withStatementsToFix = PooledHashSet<IWithStatement>.GetInstance();
			ImmutableArray<Diagnostic>.Enumerator enumerator = diagnostics.GetEnumerator();
			while (enumerator.MoveNext())
			{
				Diagnostic current = enumerator.Current;
				cancellationToken.ThrowIfCancellationRequested();
				WithStatementSyntax withStatementSyntax = TryGetWithStatementSyntaxForNode(syntaxRoot.FindNode(current.Location.SourceSpan));
				if (withStatementSyntax != null)
				{
					IWithStatement withStatement = await TryGetWithStatementForSyntaxNode(withStatementSyntax, document, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
					if (withStatement != null)
					{
						withStatementsToFix.Add(withStatement);
					}
				}
			}
			return withStatementsToFix.ToImmutableHashSet();
		}

		private static async Task<Document> UpdateAll(Document document, IEnumerable<IWithStatement> withStatementsToFix, CancellationToken cancellationToken)
		{
			SyntaxNode root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
			PooledDictionary<SyntaxNode, Func<SyntaxNode, SyntaxNode>> nodeMapping = PooledDictionary<SyntaxNode, Func<SyntaxNode, SyntaxNode>>.GetInstance();
			try
			{
				foreach (IWithStatement item in withStatementsToFix)
				{
					CollectSyntaxNodesToReplace(item, nodeMapping);
				}
				root = root.ReplaceNodes(nodeMapping.Keys, (SyntaxNode o, SyntaxNode r) => nodeMapping[o](r));
				return document.WithSyntaxRoot(root);
			}
			finally
			{
				if (nodeMapping != null)
				{
					((IDisposable)nodeMapping).Dispose();
				}
			}
		}

		private static void CollectSyntaxNodesToReplace(IWithStatement withStatement, Dictionary<SyntaxNode, Func<SyntaxNode, SyntaxNode>> nodeMapping)
		{
			WithStatementVisitor withStatementVisitor = new WithStatementVisitor(withStatement);
			CodeExpressionSyntax withId = ((WithStatementSyntax)withStatement.Syntax).WithId;
			withStatementVisitor.Visit(withStatement);
			foreach (SyntaxNode node in withStatementVisitor.Nodes)
			{
				nodeMapping.Add(node, (SyntaxNode n) => ComputeMemberAccessReplacementNode(n, withId));
			}
			if (withStatement.Body.Kind != OperationKind.BlockStatement || !withStatement.Syntax.Parent.IsKind(SyntaxKind.Block, SyntaxKind.RepeatStatement, SyntaxKind.CaseElse))
			{
				nodeMapping.Add(withStatement.Syntax, ComputeWithStatementSyntaxReplacementNode);
				return;
			}
			nodeMapping.Add(withStatement.Syntax, MarkWithStatementNodeForRemoval);
			if (!nodeMapping.ContainsKey(withStatement.Syntax.Parent))
			{
				nodeMapping.Add(withStatement.Syntax.Parent, ComputeParentBlockReplacement);
			}
		}
	}

	private static readonly ErrorCode[] fixableErrors = new ErrorCode[2]
	{
		ErrorCode.WRN_ERR_UseOfExplicitWith,
		ErrorCode.HDN_UseOfExplicitWith
	};

	public override ImmutableArray<string> FixableDiagnosticIds { get; } = fixableErrors.Select((ErrorCode t) => MessageProvider.Instance.GetIdForErrorCode((int)t)).ToImmutableArray();


	public override async Task RegisterCodeFixesAsync(CodeFixContext context)
	{
		Document document = context.Document;
		TextSpan span = context.Span;
		CancellationToken cancellationToken = context.CancellationToken;
		SyntaxNode node = (await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false)).FindNode(span);
		WithStatementSyntax withStatementSyntax = TryGetWithStatementSyntaxForNode(node);
		if (withStatementSyntax != null && await TryGetWithStatementForSyntaxNode(withStatementSyntax, document, cancellationToken).ConfigureAwait(continueOnCapturedContext: false) != null)
		{
			context.RegisterFixes(ImmutableArray.Create((CodeAction)CreateCodeAction(node, document, generateFixAll: true)), context.Diagnostics);
		}
	}

	public override FixAllProvider? GetFixAllProvider()
	{
		return ExplicitWithConverterFixAllProvider.Instance;
	}

	private static WithStatementSyntax? TryGetWithStatementSyntaxForNode(SyntaxNode node)
	{
		if (node.Parent.Kind != SyntaxKind.WithStatement)
		{
			return null;
		}
		return (WithStatementSyntax)node.Parent;
	}

	private static async Task<IWithStatement?> TryGetWithStatementForSyntaxNode(WithStatementSyntax withStatementSyntax, Document document, CancellationToken cancellationToken)
	{
		SemanticModel semanticModel = await document.GetSemanticModelForNodeAsync(withStatementSyntax, cancellationToken);
		if (semanticModel == null)
		{
			return null;
		}
		if (!(semanticModel.GetOperation(withStatementSyntax) is IWithStatement withStatement) || withStatement.Value.Kind == OperationKind.InvalidExpression)
		{
			return null;
		}
		return withStatement;
	}

	private ExplicitWithConverterCodeAction CreateCodeAction(SyntaxNode nodeToFix, Document document, bool generateFixAll)
	{
		Document document2 = document;
		SyntaxNode nodeToFix2 = nodeToFix;
		return new ExplicitWithConverterCodeAction(string.Format(CultureInfo.CurrentCulture, WorkspacesResources.ConvertExplicitWith), (CancellationToken c) => Update(document2, (WithStatementSyntax)nodeToFix2.Parent, c), nodeToFix2.ToString(), generateFixAll);
	}

	private static async Task<Document> Update(Document document, WithStatementSyntax withStatementSyntax, CancellationToken cancellationToken)
	{
		SyntaxNode root2 = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		IWithStatement withStatement = (IWithStatement)(await document.GetSemanticModelForNodeAsync(withStatementSyntax, cancellationToken)).GetOperation(withStatementSyntax);
		WithStatementVisitor withStatementVisitor = new WithStatementVisitor(withStatement);
		withStatementVisitor.Visit(withStatement);
		root2 = root2.TrackNodes(withStatementSyntax);
		foreach (SyntaxNode item in withStatementVisitor.Nodes.OrderByDescending((SyntaxNode n) => n.SpanStart))
		{
			SyntaxNode syntaxNode = root2.FindNode(item.Span);
			root2 = root2.ReplaceNode(syntaxNode, ComputeMemberAccessReplacementNode(syntaxNode, withStatementSyntax.WithId));
		}
		withStatementSyntax = root2.GetCurrentNode(withStatementSyntax);
		SyntaxNode parent = withStatementSyntax.Parent;
		if (withStatement.Body.Kind != OperationKind.BlockStatement || !parent.IsKind(SyntaxKind.Block, SyntaxKind.RepeatStatement, SyntaxKind.CaseElse))
		{
			root2 = root2.ReplaceNode(withStatementSyntax, ComputeWithStatementSyntaxReplacementNode(withStatementSyntax));
		}
		else
		{
			SyntaxNode newNode = ComputeParentBlockReplacement(parent.ReplaceNode(withStatementSyntax, MarkWithStatementNodeForRemoval(withStatementSyntax)));
			root2 = root2.ReplaceNode(parent, newNode);
		}
		document = document.WithSyntaxRoot(root2);
		return document;
	}

	private static SyntaxNode ComputeMemberAccessReplacementNode(SyntaxNode node, CodeExpressionSyntax withId)
	{
		return SyntaxFactory.MemberAccessExpression(withId.WithoutTrivia(), node.ToString()).WithTriviaFrom(node);
	}

	private static SyntaxNode ComputeWithStatementSyntaxReplacementNode(SyntaxNode node)
	{
		IEnumerable<SyntaxTrivia> first = ((WithStatementSyntax)node).GetLeadingTrivia();
		IEnumerable<SyntaxTrivia> second = ExtractWithStatementInnerTrivia((WithStatementSyntax)node);
		return ((WithStatementSyntax)node).Statement.WithPrependedLeadingTrivia(first.Concat(second)).WithAdditionalAnnotations(Formatter.Annotation);
	}

	private static SyntaxNode MarkWithStatementNodeForRemoval(SyntaxNode node)
	{
		return node.WithAdditionalAnnotations(new SyntaxAnnotation(AnnotationKind.RemovedExplicitWith));
	}

	private static SyntaxNode ComputeParentBlockReplacement(SyntaxNode oldBlockNode)
	{
		SyntaxList<StatementSyntax> syntaxList = oldBlockNode.Kind switch
		{
			SyntaxKind.Block => ((BlockSyntax)oldBlockNode).Statements, 
			SyntaxKind.RepeatStatement => ((RepeatStatementSyntax)oldBlockNode).Statements, 
			SyntaxKind.CaseElse => ((CaseElseSyntax)oldBlockNode).ElseStatements, 
			_ => throw ExceptionUtilities.UnexpectedValue(oldBlockNode.Kind), 
		};
		using PooledList<StatementSyntax> pooledList = PooledList<StatementSyntax>.GetInstance();
		SyntaxList<StatementSyntax>.Enumerator enumerator = syntaxList.GetEnumerator();
		while (enumerator.MoveNext())
		{
			StatementSyntax current = enumerator.Current;
			if (!current.IsKind(SyntaxKind.WithStatement) || !current.HasAnnotations(AnnotationKind.RemovedExplicitWith))
			{
				pooledList.Add(current);
				continue;
			}
			WithStatementSyntax withStatementSyntax = (WithStatementSyntax)current;
			BlockSyntax blockSyntax = (BlockSyntax)withStatementSyntax.Statement;
			SyntaxList<StatementSyntax> statements = blockSyntax.Statements;
			SyntaxTriviaListBuilder syntaxTriviaListBuilder = SyntaxTriviaListBuilder.Create();
			syntaxTriviaListBuilder.Add(withStatementSyntax.GetLeadingTrivia());
			syntaxTriviaListBuilder.Add(ExtractWithStatementInnerTrivia(withStatementSyntax, TextSpan.FromBounds(withStatementSyntax.WithKeywordToken.SpanStart, ((BlockSyntax)withStatementSyntax.Statement).BeginKeywordToken.FullSpan.End)));
			if (statements.IsEmpty())
			{
				int end = (FilterTrailingWhitespace(withStatementSyntax.GetTrailingTrivia()).Any() ? blockSyntax.FullSpan.End : blockSyntax.Span.End);
				syntaxTriviaListBuilder.Add(blockSyntax.DescendantTrivia(TextSpan.FromBounds(blockSyntax.BeginKeywordToken.FullSpan.End, end), (SyntaxNode c) => true).ToArray());
				if (pooledList.Any())
				{
					pooledList[pooledList.Count - 1] = pooledList[pooledList.Count - 1].WithAppendedTrailingTrivia(syntaxTriviaListBuilder.ToList()).WithAdditionalAnnotations(Formatter.Annotation);
				}
				else
				{
					oldBlockNode = WithAppendedCarriedOverTrivia(oldBlockNode, syntaxTriviaListBuilder.ToList()).WithAdditionalAnnotations(Formatter.Annotation);
				}
				continue;
			}
			for (int i = 0; i < statements.Count; i++)
			{
				StatementSyntax statementSyntax = statements[i];
				if (i == 0)
				{
					statementSyntax = statementSyntax.WithPrependedLeadingTrivia(syntaxTriviaListBuilder.ToList());
				}
				if (i == statements.Count - 1)
				{
					IEnumerable<SyntaxTrivia> enumerable = FilterTrailingWhitespace(((BlockSyntax)withStatementSyntax.Statement).EndKeywordToken.LeadingTrivia);
					IEnumerable<SyntaxTrivia> enumerable2;
					if (!enumerable.Any())
					{
						enumerable2 = FilterTrailingWhitespace(statementSyntax.GetTrailingTrivia());
					}
					else
					{
						IEnumerable<SyntaxTrivia> enumerable3 = statementSyntax.GetTrailingTrivia();
						enumerable2 = enumerable3;
					}
					IEnumerable<SyntaxTrivia> first = enumerable2;
					statementSyntax = statementSyntax.WithTrailingTrivia(first.Concat(enumerable).Concat(withStatementSyntax.GetTrailingTrivia()));
				}
				pooledList.Add(statementSyntax.WithAdditionalAnnotations(Formatter.Annotation));
			}
		}
		return oldBlockNode.Kind switch
		{
			SyntaxKind.Block => ((BlockSyntax)oldBlockNode).WithStatements(ToSyntaxList(pooledList)), 
			SyntaxKind.RepeatStatement => ((RepeatStatementSyntax)oldBlockNode).WithStatements(ToSyntaxList(pooledList)), 
			SyntaxKind.CaseElse => ((CaseElseSyntax)oldBlockNode).WithElseStatements(ToSyntaxList(pooledList)), 
			_ => throw ExceptionUtilities.UnexpectedValue(oldBlockNode.Kind), 
		};
	}

	private static SyntaxList<TNode> ToSyntaxList<TNode>(IEnumerable<TNode> items) where TNode : SyntaxNode
	{
		SyntaxListBuilder syntaxListBuilder = new SyntaxListBuilder(items.Count());
		SyntaxNode[] items2 = items.ToArray();
		syntaxListBuilder.AddRange(items2);
		return syntaxListBuilder.ToList();
	}

	private static SyntaxNode WithAppendedCarriedOverTrivia(SyntaxNode parentNode, SyntaxTriviaList carryOverTrivia)
	{
		return parentNode.Kind switch
		{
			SyntaxKind.Block => ((BlockSyntax)parentNode).WithBeginKeywordToken(Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.SyntaxTokenExtensions.WithAppendedTrailingTrivia(((BlockSyntax)parentNode).BeginKeywordToken, carryOverTrivia)), 
			SyntaxKind.RepeatStatement => ((RepeatStatementSyntax)parentNode).WithRepeatKeywordToken(Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.SyntaxTokenExtensions.WithAppendedTrailingTrivia(((RepeatStatementSyntax)parentNode).RepeatKeywordToken, carryOverTrivia)), 
			SyntaxKind.CaseElse => ((CaseElseSyntax)parentNode).WithElseKeywordToken(Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.SyntaxTokenExtensions.WithAppendedTrailingTrivia(((CaseElseSyntax)parentNode).ElseKeywordToken, carryOverTrivia)), 
			_ => throw ExceptionUtilities.UnexpectedValue(parentNode.Kind), 
		};
	}

	private static IEnumerable<SyntaxTrivia> FilterTrailingWhitespace(IEnumerable<SyntaxTrivia> trivia)
	{
		List<SyntaxTrivia> list = trivia.ToList();
		int num = list.Count;
		int num2 = num - 1;
		while (num2 >= 0 && (list[num2].Kind == SyntaxKind.EndOfLineTrivia || list[num2].Kind == SyntaxKind.WhiteSpaceTrivia))
		{
			num--;
			num2--;
		}
		if (num == 0)
		{
			return Array.Empty<SyntaxTrivia>();
		}
		list.RemoveRange(num, list.Count - num);
		return list;
	}

	private static SyntaxTriviaList ExtractWithStatementInnerTrivia(WithStatementSyntax node, TextSpan? triviaSpan = null)
	{
		bool flag = false;
		SyntaxTriviaListBuilder syntaxTriviaListBuilder = SyntaxTriviaListBuilder.Create();
		foreach (SyntaxTrivia item in node.DescendantTrivia(triviaSpan ?? TextSpan.FromBounds(node.WithKeywordToken.SpanStart, node.DoKeywordToken.FullSpan.End), (SyntaxNode c) => true))
		{
			if (!item.IsKind(SyntaxKind.WhiteSpaceTrivia))
			{
				syntaxTriviaListBuilder.Add(item);
				if (!flag && !item.IsKind(SyntaxKind.EndOfLineTrivia))
				{
					flag = true;
				}
			}
		}
		if (!flag)
		{
			return SyntaxTriviaList.Empty;
		}
		return syntaxTriviaListBuilder.ToList();
	}
}
