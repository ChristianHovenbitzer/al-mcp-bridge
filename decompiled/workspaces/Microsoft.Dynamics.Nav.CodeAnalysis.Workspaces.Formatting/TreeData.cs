using System.Collections.Generic;
using System.Text;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Formatting;

internal abstract class TreeData
{
	private class Debug : NodeAndText
	{
		private readonly TreeData debugNodeData;

		public Debug(SyntaxNode root, SourceText text)
			: base(root, text)
		{
			debugNodeData = new Node(root);
		}

		public override string GetTextBetween(SyntaxToken token1, SyntaxToken token2)
		{
			string textBetween = base.GetTextBetween(token1, token2);
			Contract.ThrowIfFalse(textBetween == debugNodeData.GetTextBetween(token1, token2));
			return textBetween;
		}
	}

	private class Node : TreeData
	{
		public Node(SyntaxNode root)
			: base(root)
		{
			Contract.ThrowIfFalse(root.GetFirstToken(includeZeroWidth: true).Kind != SyntaxKind.None);
		}

		public override int GetOriginalColumn(int tabSize, SyntaxToken token)
		{
			Contract.ThrowIfTrue(token.Kind == SyntaxKind.None);
			SyntaxToken tokenWithLineBreaks = GetTokenWithLineBreaks(token);
			string lastLineText = GetTextBetween(tokenWithLineBreaks, token).GetLastLineText();
			return lastLineText.GetColumnFromLineOffset(lastLineText.Length, tabSize);
		}

		public override string GetTextBetween(SyntaxToken token1, SyntaxToken token2)
		{
			StringBuilder builder = StringBuilderPool.Allocate();
			CommonFormattingHelpers.AppendTextBetween(token1, token2, builder);
			return StringBuilderPool.ReturnAndFree(builder);
		}

		private SyntaxToken GetTokenWithLineBreaks(SyntaxToken token)
		{
			SyntaxToken previousToken = token.GetPreviousToken(includeZeroWidth: true);
			while (previousToken.Kind != 0)
			{
				if (previousToken.ToFullString().IndexOf('\n') >= 0)
				{
					return previousToken;
				}
				previousToken = previousToken.GetPreviousToken(includeZeroWidth: true);
			}
			return default(SyntaxToken);
		}
	}

	private class NodeAndText : TreeData
	{
		private readonly SourceText text;

		public NodeAndText(SyntaxNode root, SourceText text)
			: base(root)
		{
			Contract.ThrowIfNull(text);
			this.text = text;
		}

		public override int GetOriginalColumn(int tabSize, SyntaxToken token)
		{
			Contract.ThrowIfTrue(token.Kind == SyntaxKind.None);
			TextLine lineFromPosition = text.Lines.GetLineFromPosition(token.SpanStart);
			return lineFromPosition.GetColumnFromLineOffset(token.SpanStart - lineFromPosition.Start, tabSize);
		}

		public override string GetTextBetween(SyntaxToken token1, SyntaxToken token2)
		{
			if (token1.Kind == SyntaxKind.None)
			{
				return text.ToString(TextSpan.FromBounds(token2.FullSpan.Start, token2.SpanStart));
			}
			if (token2.Kind == SyntaxKind.None)
			{
				return text.ToString(TextSpan.FromBounds(token1.Span.End, token1.FullSpan.End));
			}
			return text.ToString(TextSpan.FromBounds(token1.Span.End, token2.SpanStart));
		}
	}

	private class StructuredTrivia : TreeData
	{
		private readonly int initialColumn;

		private readonly TreeData treeData;

		private readonly SyntaxTrivia trivia;

		public StructuredTrivia(SyntaxTrivia trivia, int initialColumn)
			: base(trivia.GetStructure())
		{
			Contract.ThrowIfFalse(trivia.HasStructure);
			this.trivia = trivia;
			SyntaxNode structure = trivia.GetStructure();
			SourceText text = GetText();
			this.initialColumn = initialColumn;
			treeData = ((text == null) ? ((TreeData)new Node(structure)) : ((TreeData)new NodeAndText(structure, text)));
		}

		public override string GetTextBetween(SyntaxToken token1, SyntaxToken token2)
		{
			return treeData.GetTextBetween(token1, token2);
		}

		public override int GetOriginalColumn(int tabSize, SyntaxToken token)
		{
			if (treeData is NodeAndText)
			{
				return treeData.GetOriginalColumn(tabSize, token);
			}
			return trivia.ToFullString().Substring(0, token.SpanStart - trivia.FullSpan.Start).GetTextColumn(tabSize, initialColumn);
		}

		private SourceText GetText()
		{
			SyntaxNode structure = trivia.GetStructure();
			if (structure.SyntaxTree != null && structure.SyntaxTree.GetText() != null)
			{
				return structure.SyntaxTree.GetText();
			}
			SyntaxNode parent = trivia.Token.Parent;
			if (parent != null && parent.SyntaxTree != null && parent.SyntaxTree.GetText() != null)
			{
				return parent.SyntaxTree.GetText();
			}
			return null;
		}
	}

	private readonly SyntaxToken firstToken;

	private readonly SyntaxToken lastToken;

	public SyntaxNode Root { get; }

	public int StartPosition => Root.FullSpan.Start;

	public int EndPosition => Root.FullSpan.End;

	public TreeData(SyntaxNode root)
	{
		Contract.ThrowIfNull(root);
		Root = root;
		firstToken = Root.GetFirstToken(includeZeroWidth: true);
		lastToken = Root.GetLastToken(includeZeroWidth: true);
	}

	public static TreeData Create(SyntaxNode root)
	{
		if (root.SyntaxTree == null || !root.SyntaxTree.TryGetText(out SourceText text))
		{
			return new Node(root);
		}
		return new NodeAndText(root, text);
	}

	public static TreeData Create(SyntaxTrivia trivia, int initialColumn)
	{
		return new StructuredTrivia(trivia, initialColumn);
	}

	public abstract string GetTextBetween(SyntaxToken token1, SyntaxToken token2);

	public abstract int GetOriginalColumn(int tabSize, SyntaxToken token);

	public bool IsFirstToken(SyntaxToken token)
	{
		return firstToken == token;
	}

	public bool IsLastToken(SyntaxToken token)
	{
		return lastToken == token;
	}

	public IEnumerable<SyntaxToken> GetApplicableTokens(TextSpan textSpan)
	{
		return Root.DescendantTokens(textSpan);
	}
}
