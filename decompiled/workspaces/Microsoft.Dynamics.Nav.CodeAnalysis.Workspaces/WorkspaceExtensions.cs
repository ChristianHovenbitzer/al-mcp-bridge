using System.Collections.Generic;
using System.Threading;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces;

public static class WorkspaceExtensions
{
	internal static void ApplyDocumentChanges(this Workspace workspace, Document newDocument, CancellationToken cancellationToken)
	{
		Solution currentSolution = workspace.CurrentSolution;
		Document document = currentSolution.GetDocument(newDocument.Id);
		Solution newSolution = UpdateDocument(textChanges: newDocument.GetTextChangesAsync(document, cancellationToken).WaitAndGetResult(cancellationToken), solution: currentSolution, id: newDocument.Id, cancellationToken: cancellationToken);
		workspace.TryApplyChanges(newSolution);
	}

	internal static void ApplyTextChanges(this Workspace workspace, DocumentId id, IEnumerable<TextChange> textChanges, CancellationToken cancellationToken)
	{
		Solution newSolution = workspace.CurrentSolution.UpdateDocument(id, textChanges, cancellationToken);
		workspace.TryApplyChanges(newSolution);
	}

	public static void ApplyTextChanges(this Workspace workspace, DocumentId id, TextChange textChange, CancellationToken cancellationToken)
	{
		workspace.ApplyTextChanges(id, SpecializedCollections.SingletonEnumerable(textChange), cancellationToken);
	}

	internal static Solution UpdateDocument(this Solution solution, DocumentId id, IEnumerable<TextChange> textChanges, CancellationToken cancellationToken)
	{
		SourceText text = solution.GetDocument(id).GetTextAsync(cancellationToken).WaitAndGetResult(cancellationToken)
			.WithChanges(textChanges);
		return solution.WithDocumentText(id, text, PreservationMode.PreserveIdentity);
	}
}
