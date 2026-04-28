using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Utilities;

internal class AnnotationTable<TAnnotation> where TAnnotation : class
{
	private int globalId;

	private readonly Dictionary<TAnnotation, SyntaxAnnotation> realAnnotationMap = new Dictionary<TAnnotation, SyntaxAnnotation>();

	private readonly Dictionary<object, TAnnotation> annotationMap = new Dictionary<object, TAnnotation>();

	private readonly AnnotationKind annotationKind;

	public AnnotationTable(AnnotationKind annotationKind)
	{
		this.annotationKind = annotationKind;
	}

	private IEnumerable<SyntaxAnnotation> GetOrCreateRealAnnotations(TAnnotation[] annotations)
	{
		foreach (TAnnotation annotation in annotations)
		{
			yield return GetOrCreateRealAnnotation(annotation);
		}
	}

	private SyntaxAnnotation GetOrCreateRealAnnotation(TAnnotation annotation)
	{
		if (!realAnnotationMap.TryGetValue(annotation, out SyntaxAnnotation value))
		{
			string text = Interlocked.Increment(ref globalId).ToString();
			value = new SyntaxAnnotation(annotationKind, text);
			annotationMap.Add(text, annotation);
			realAnnotationMap.Add(annotation, value);
		}
		return value;
	}

	private IEnumerable<SyntaxAnnotation> GetRealAnnotations(TAnnotation[] annotations)
	{
		foreach (TAnnotation annotation in annotations)
		{
			SyntaxAnnotation realAnnotation = GetRealAnnotation(annotation);
			if (realAnnotation != null)
			{
				yield return realAnnotation;
			}
		}
	}

	private SyntaxAnnotation GetRealAnnotation(TAnnotation annotation)
	{
		realAnnotationMap.TryGetValue(annotation, out SyntaxAnnotation value);
		return value;
	}

	public TSyntaxNode WithAdditionalAnnotations<TSyntaxNode>(TSyntaxNode node, params TAnnotation[] annotations) where TSyntaxNode : SyntaxNode
	{
		return node.WithAdditionalAnnotations(GetOrCreateRealAnnotations(annotations).ToArray());
	}

	public SyntaxToken WithAdditionalAnnotations(SyntaxToken token, params TAnnotation[] annotations)
	{
		return token.WithAdditionalAnnotations(GetOrCreateRealAnnotations(annotations).ToArray());
	}

	public SyntaxTrivia WithAdditionalAnnotations(SyntaxTrivia trivia, params TAnnotation[] annotations)
	{
		return trivia.WithAdditionalAnnotations(GetOrCreateRealAnnotations(annotations).ToArray());
	}

	public SyntaxNodeOrToken WithAdditionalAnnotations(SyntaxNodeOrToken nodeOrToken, params TAnnotation[] annotations)
	{
		return nodeOrToken.WithAdditionalAnnotations(GetOrCreateRealAnnotations(annotations).ToArray());
	}

	public TSyntaxNode WithoutAnnotations<TSyntaxNode>(TSyntaxNode node, params TAnnotation[] annotations) where TSyntaxNode : SyntaxNode
	{
		return node.WithoutAnnotations(GetRealAnnotations(annotations).ToArray());
	}

	public SyntaxToken WithoutAnnotations(SyntaxToken token, params TAnnotation[] annotations)
	{
		return token.WithoutAnnotations(GetRealAnnotations(annotations).ToArray());
	}

	public SyntaxTrivia WithoutAnnotations(SyntaxTrivia trivia, params TAnnotation[] annotations)
	{
		return trivia.WithoutAnnotations(GetRealAnnotations(annotations).ToArray());
	}

	public SyntaxNodeOrToken WithoutAnnotations(SyntaxNodeOrToken nodeOrToken, params TAnnotation[] annotations)
	{
		return nodeOrToken.WithoutAnnotations(GetRealAnnotations(annotations).ToArray());
	}

	private IEnumerable<TAnnotation> GetAnnotations(IEnumerable<SyntaxAnnotation> realAnnotations)
	{
		foreach (SyntaxAnnotation realAnnotation in realAnnotations)
		{
			if (annotationMap.TryGetValue(realAnnotation.Data, out var value))
			{
				yield return value;
			}
		}
	}

	public IEnumerable<TAnnotation> GetAnnotations(SyntaxNode node)
	{
		return GetAnnotations(node.GetAnnotations(annotationKind));
	}

	public IEnumerable<TAnnotation> GetAnnotations(SyntaxToken token)
	{
		return GetAnnotations(token.GetAnnotations(annotationKind));
	}

	public IEnumerable<TAnnotation> GetAnnotations(SyntaxTrivia trivia)
	{
		return GetAnnotations(trivia.GetAnnotations(annotationKind));
	}

	public IEnumerable<TAnnotation> GetAnnotations(SyntaxNodeOrToken nodeOrToken)
	{
		return GetAnnotations(nodeOrToken.GetAnnotations(annotationKind));
	}

	public IEnumerable<TSpecificAnnotation> GetAnnotations<TSpecificAnnotation>(SyntaxNode node) where TSpecificAnnotation : TAnnotation
	{
		return GetAnnotations(node).OfType<TSpecificAnnotation>();
	}

	public IEnumerable<TSpecificAnnotation> GetAnnotations<TSpecificAnnotation>(SyntaxToken token) where TSpecificAnnotation : TAnnotation
	{
		return GetAnnotations(token).OfType<TSpecificAnnotation>();
	}

	public IEnumerable<TSpecificAnnotation> GetAnnotations<TSpecificAnnotation>(SyntaxTrivia trivia) where TSpecificAnnotation : TAnnotation
	{
		return GetAnnotations(trivia).OfType<TSpecificAnnotation>();
	}

	public IEnumerable<TSpecificAnnotation> GetAnnotations<TSpecificAnnotation>(SyntaxNodeOrToken nodeOrToken) where TSpecificAnnotation : TAnnotation
	{
		return GetAnnotations(nodeOrToken).OfType<TSpecificAnnotation>();
	}

	public bool HasAnnotations(SyntaxNode node)
	{
		return node.HasAnnotations(annotationKind);
	}

	public bool HasAnnotations(SyntaxToken token)
	{
		return token.HasAnnotations(annotationKind);
	}

	public bool HasAnnotations(SyntaxTrivia trivia)
	{
		return trivia.HasAnnotations(annotationKind);
	}

	public bool HasAnnotations(SyntaxNodeOrToken nodeOrToken)
	{
		return nodeOrToken.HasAnnotations(annotationKind);
	}

	public bool HasAnnotations<TSpecificAnnotation>(SyntaxNode node) where TSpecificAnnotation : TAnnotation
	{
		return GetAnnotations(node).OfType<TSpecificAnnotation>().Any();
	}

	public bool HasAnnotations<TSpecificAnnotation>(SyntaxToken token) where TSpecificAnnotation : TAnnotation
	{
		return GetAnnotations(token).OfType<TSpecificAnnotation>().Any();
	}

	public bool HasAnnotations<TSpecificAnnotation>(SyntaxTrivia trivia) where TSpecificAnnotation : TAnnotation
	{
		return GetAnnotations(trivia).OfType<TSpecificAnnotation>().Any();
	}

	public bool HasAnnotations<TSpecificAnnotation>(SyntaxNodeOrToken nodeOrToken) where TSpecificAnnotation : TAnnotation
	{
		return GetAnnotations(nodeOrToken).OfType<TSpecificAnnotation>().Any();
	}

	public bool HasAnnotation(SyntaxNode node, TAnnotation annotation)
	{
		return node.HasAnnotation(GetRealAnnotation(annotation));
	}

	public bool HasAnnotation(SyntaxToken token, TAnnotation annotation)
	{
		return token.HasAnnotation(GetRealAnnotation(annotation));
	}

	public bool HasAnnotation(SyntaxTrivia trivia, TAnnotation annotation)
	{
		return trivia.HasAnnotation(GetRealAnnotation(annotation));
	}

	public bool HasAnnotation(SyntaxNodeOrToken nodeOrToken, TAnnotation annotation)
	{
		return nodeOrToken.HasAnnotation(GetRealAnnotation(annotation));
	}

	public IEnumerable<SyntaxNodeOrToken> GetAnnotatedNodesAndTokens(SyntaxNode node)
	{
		return node.GetAnnotatedNodesAndTokens(annotationKind);
	}

	public IEnumerable<SyntaxNode> GetAnnotatedNodes(SyntaxNode node)
	{
		return from nt in node.GetAnnotatedNodesAndTokens(annotationKind)
			where nt.IsNode
			select nt.AsNode();
	}

	public IEnumerable<SyntaxToken> GetAnnotatedTokens(SyntaxNode node)
	{
		return from nt in node.GetAnnotatedNodesAndTokens(annotationKind)
			where nt.IsToken
			select nt.AsToken();
	}

	public IEnumerable<SyntaxTrivia> GetAnnotatedTrivia(SyntaxNode node)
	{
		return node.GetAnnotatedTrivia(annotationKind);
	}

	public IEnumerable<SyntaxNodeOrToken> GetAnnotatedNodesAndTokens<TSpecificAnnotation>(SyntaxNode node) where TSpecificAnnotation : TAnnotation
	{
		return from nt in node.GetAnnotatedNodesAndTokens(annotationKind)
			where this.HasAnnotations<TSpecificAnnotation>(nt)
			select nt;
	}

	public IEnumerable<SyntaxNode> GetAnnotatedNodes<TSpecificAnnotation>(SyntaxNode node) where TSpecificAnnotation : TAnnotation
	{
		return from nt in node.GetAnnotatedNodesAndTokens(annotationKind)
			where nt.IsNode && this.HasAnnotations<TSpecificAnnotation>(nt)
			select nt.AsNode();
	}

	public IEnumerable<SyntaxToken> GetAnnotatedTokens<TSpecificAnnotation>(SyntaxNode node) where TSpecificAnnotation : TAnnotation
	{
		return from nt in node.GetAnnotatedNodesAndTokens(annotationKind)
			where nt.IsToken && this.HasAnnotations<TSpecificAnnotation>(nt)
			select nt.AsToken();
	}

	public IEnumerable<SyntaxTrivia> GetAnnotatedTrivia<TSpecificAnnotation>(SyntaxNode node) where TSpecificAnnotation : TAnnotation
	{
		return from tr in node.GetAnnotatedTrivia(annotationKind)
			where this.HasAnnotations<TSpecificAnnotation>(tr)
			select tr;
	}
}
