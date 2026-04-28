using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Editing;

public class SyntaxEditor
{
	private abstract class Change
	{
		internal readonly SyntaxNode Node;

		public Change(SyntaxNode node)
		{
			Node = node;
		}

		public abstract SyntaxNode Apply(SyntaxNode root);
	}

	private class NoChange : Change
	{
		public NoChange(SyntaxNode node)
			: base(node)
		{
		}

		public override SyntaxNode Apply(SyntaxNode root)
		{
			return root;
		}
	}

	private class RemoveChange : Change
	{
		private readonly SyntaxRemoveOptions options;

		public RemoveChange(SyntaxNode node, SyntaxRemoveOptions options)
			: base(node)
		{
			this.options = options;
		}

		public override SyntaxNode Apply(SyntaxNode root)
		{
			return root.RemoveNode(root.GetCurrentNode(Node), options);
		}
	}

	private class ReplaceChange : Change
	{
		private readonly Func<SyntaxNode, SyntaxNode> modifier;

		private readonly SyntaxEditor editor;

		public ReplaceChange(SyntaxNode node, Func<SyntaxNode, SyntaxNode> modifier, SyntaxEditor editor)
			: base(node)
		{
			this.modifier = modifier;
			this.editor = editor;
		}

		public override SyntaxNode Apply(SyntaxNode root)
		{
			SyntaxNode currentNode = root.GetCurrentNode(Node);
			SyntaxNode node = modifier(currentNode);
			node = editor.ApplyTrackingToNewNode(node);
			return root.ReplaceNode(currentNode, node);
		}
	}

	private class ReplaceWithCollectionChange : Change
	{
		private readonly Func<SyntaxNode, IEnumerable<SyntaxNode>> modifier;

		private readonly SyntaxEditor editor;

		public ReplaceWithCollectionChange(SyntaxNode node, Func<SyntaxNode, IEnumerable<SyntaxNode>> modifier, SyntaxEditor editor)
			: base(node)
		{
			this.modifier = modifier;
			this.editor = editor;
		}

		public override SyntaxNode Apply(SyntaxNode root)
		{
			SyntaxNode currentNode = root.GetCurrentNode(Node);
			List<SyntaxNode> list = modifier(currentNode).ToList();
			for (int i = 0; i < list.Count; i++)
			{
				list[i] = editor.ApplyTrackingToNewNode(list[i]);
			}
			return root.ReplaceNode(currentNode, list);
		}
	}

	private class ReplaceChange<TArgument> : Change
	{
		private readonly Func<SyntaxNode, TArgument, SyntaxNode> modifier;

		private readonly TArgument argument;

		private readonly SyntaxEditor editor;

		public ReplaceChange(SyntaxNode node, Func<SyntaxNode, TArgument, SyntaxNode> modifier, TArgument argument, SyntaxEditor editor)
			: base(node)
		{
			this.modifier = modifier;
			this.argument = argument;
			this.editor = editor;
		}

		public override SyntaxNode Apply(SyntaxNode root)
		{
			SyntaxNode currentNode = root.GetCurrentNode(Node);
			SyntaxNode node = modifier(currentNode, argument);
			node = editor.ApplyTrackingToNewNode(node);
			return root.ReplaceNode(currentNode, node);
		}
	}

	private class InsertChange : Change
	{
		private readonly List<SyntaxNode> newNodes;

		private readonly bool isBefore;

		public InsertChange(SyntaxNode node, IEnumerable<SyntaxNode> newNodes, bool isBefore)
			: base(node)
		{
			this.newNodes = newNodes.ToList();
			this.isBefore = isBefore;
		}

		public override SyntaxNode Apply(SyntaxNode root)
		{
			if (isBefore)
			{
				return root.InsertNodesBefore(root.GetCurrentNode(Node), newNodes);
			}
			return root.InsertNodesAfter(root.GetCurrentNode(Node), newNodes);
		}
	}

	public static SyntaxRemoveOptions DefaultRemoveOptions = SyntaxRemoveOptions.AddElasticMarker;

	private readonly List<Change> changes;

	private bool allowEditsOnLazilyCreatedTrackedNewNodes;

	private HashSet<SyntaxNode> lazyTrackedNewNodesOpt;

	public SyntaxNode OriginalRoot { get; }

	public SyntaxEditor(SyntaxNode root)
	{
		OriginalRoot = root ?? throw new ArgumentNullException("root");
		changes = new List<Change>();
	}

	private SyntaxNode ApplyTrackingToNewNode(SyntaxNode node)
	{
		if (node == null)
		{
			return null;
		}
		if (lazyTrackedNewNodesOpt == null)
		{
			lazyTrackedNewNodesOpt = new HashSet<SyntaxNode>();
		}
		foreach (SyntaxNode item in node.DescendantNodesAndSelf())
		{
			lazyTrackedNewNodesOpt.Add(item);
		}
		return node.TrackNodes(node.DescendantNodesAndSelf());
	}

	private IEnumerable<SyntaxNode> ApplyTrackingToNewNodes(IEnumerable<SyntaxNode> nodes)
	{
		foreach (SyntaxNode node in nodes)
		{
			yield return ApplyTrackingToNewNode(node);
		}
	}

	public SyntaxNode GetChangedRoot()
	{
		IEnumerable<SyntaxNode> nodes = (from c in changes
			where OriginalRoot.Contains(c.Node)
			select c.Node).Distinct();
		SyntaxNode syntaxNode = OriginalRoot.TrackNodes(nodes);
		foreach (Change change in changes)
		{
			syntaxNode = change.Apply(syntaxNode);
		}
		return syntaxNode;
	}

	public void TrackNode(SyntaxNode node)
	{
		CheckNodeInOriginalTreeOrTracked(node);
		changes.Add(new NoChange(node));
	}

	public void RemoveNode(SyntaxNode node)
	{
		RemoveNode(node, DefaultRemoveOptions);
	}

	public void RemoveNode(SyntaxNode node, SyntaxRemoveOptions options)
	{
		CheckNodeInOriginalTreeOrTracked(node);
		changes.Add(new RemoveChange(node, options));
	}

	public void ReplaceNode(SyntaxNode node, Func<SyntaxNode, SyntaxNode> computeReplacement)
	{
		CheckNodeInOriginalTreeOrTracked(node);
		if (computeReplacement == null)
		{
			throw new ArgumentNullException("computeReplacement");
		}
		allowEditsOnLazilyCreatedTrackedNewNodes = true;
		changes.Add(new ReplaceChange(node, computeReplacement, this));
	}

	internal void ReplaceNode(SyntaxNode node, Func<SyntaxNode, IEnumerable<SyntaxNode>> computeReplacement)
	{
		CheckNodeInOriginalTreeOrTracked(node);
		if (computeReplacement == null)
		{
			throw new ArgumentNullException("computeReplacement");
		}
		allowEditsOnLazilyCreatedTrackedNewNodes = true;
		changes.Add(new ReplaceWithCollectionChange(node, computeReplacement, this));
	}

	internal void ReplaceNode<TArgument>(SyntaxNode node, Func<SyntaxNode, TArgument, SyntaxNode> computeReplacement, TArgument argument)
	{
		CheckNodeInOriginalTreeOrTracked(node);
		if (computeReplacement == null)
		{
			throw new ArgumentNullException("computeReplacement");
		}
		allowEditsOnLazilyCreatedTrackedNewNodes = true;
		changes.Add(new ReplaceChange<TArgument>(node, computeReplacement, argument, this));
	}

	public void ReplaceNode(SyntaxNode node, SyntaxNode newNode)
	{
		SyntaxNode newNode2 = newNode;
		CheckNodeInOriginalTreeOrTracked(node);
		if (node != newNode2)
		{
			newNode2 = ApplyTrackingToNewNode(newNode2);
			changes.Add(new ReplaceChange(node, (SyntaxNode n) => newNode2, this));
		}
	}

	public void InsertBefore(SyntaxNode node, IEnumerable<SyntaxNode> newNodes)
	{
		CheckNodeInOriginalTreeOrTracked(node);
		if (newNodes == null)
		{
			throw new ArgumentNullException("newNodes");
		}
		newNodes = ApplyTrackingToNewNodes(newNodes);
		changes.Add(new InsertChange(node, newNodes, isBefore: true));
	}

	public void InsertBefore(SyntaxNode node, SyntaxNode newNode)
	{
		InsertBefore(node, new SyntaxNode[1] { newNode });
	}

	public void InsertAfter(SyntaxNode node, IEnumerable<SyntaxNode> newNodes)
	{
		CheckNodeInOriginalTreeOrTracked(node);
		if (newNodes == null)
		{
			throw new ArgumentNullException("newNodes");
		}
		newNodes = ApplyTrackingToNewNodes(newNodes);
		changes.Add(new InsertChange(node, newNodes, isBefore: false));
	}

	public void InsertAfter(SyntaxNode node, SyntaxNode newNode)
	{
		InsertAfter(node, new SyntaxNode[1] { newNode });
	}

	private void CheckNodeInOriginalTreeOrTracked(SyntaxNode node)
	{
		if (node == null)
		{
			throw new ArgumentNullException("node");
		}
		if (!OriginalRoot.Contains(node) && !allowEditsOnLazilyCreatedTrackedNewNodes)
		{
			HashSet<SyntaxNode> hashSet = lazyTrackedNewNodesOpt;
			if (hashSet == null || !hashSet.Contains(node))
			{
				throw new ArgumentException("Node is not in tree", "node");
			}
		}
	}
}
