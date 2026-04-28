using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Formatting.Rules;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Formatting;

internal class NodeOperations
{
	public static NodeOperations Empty = new NodeOperations();

	public Task<List<IndentBlockOperation>> IndentBlockOperationTask { get; }

	public Task<List<SuppressOperation>> SuppressOperationTask { get; }

	public Task<List<AlignTokensOperation>> AlignmentOperationTask { get; }

	public Task<List<AnchorIndentationOperation>> AnchorIndentationOperationsTask { get; }

	public NodeOperations(Task<List<IndentBlockOperation>> indentBlockOperationTask, Task<List<SuppressOperation>> suppressOperationTask, Task<List<AnchorIndentationOperation>> anchorIndentationOperationsTask, Task<List<AlignTokensOperation>> alignmentOperationTask)
	{
		IndentBlockOperationTask = indentBlockOperationTask;
		SuppressOperationTask = suppressOperationTask;
		AlignmentOperationTask = alignmentOperationTask;
		AnchorIndentationOperationsTask = anchorIndentationOperationsTask;
	}

	private NodeOperations()
	{
		IndentBlockOperationTask = Task.FromResult(new List<IndentBlockOperation>());
		SuppressOperationTask = Task.FromResult(new List<SuppressOperation>());
		AlignmentOperationTask = Task.FromResult(new List<AlignTokensOperation>());
		AnchorIndentationOperationsTask = Task.FromResult(new List<AnchorIndentationOperation>());
	}
}
