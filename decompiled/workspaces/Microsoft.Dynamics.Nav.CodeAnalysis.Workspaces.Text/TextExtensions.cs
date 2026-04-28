using System.Collections.Generic;
using System.Linq;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Text;

internal static class TextExtensions
{
	public static IEnumerable<Document> GetRelatedDocumentsWithChanges(this SourceText text)
	{
		if (Workspace.TryGetWorkspace(text.Container, out Workspace workspace))
		{
			IEnumerable<DocumentId> relatedDocumentIds = workspace.GetRelatedDocumentIds(text.Container);
			Solution sol = workspace.CurrentSolution.WithDocumentText(relatedDocumentIds, text, PreservationMode.PreserveIdentity);
			return from id in relatedDocumentIds
				select sol.GetDocument(id) into d
				where d != null
				select d;
		}
		return SpecializedCollections.EmptyEnumerable<Document>();
	}

	public static Document GetOpenDocumentInCurrentContextWithChanges(this SourceText text)
	{
		if (Workspace.TryGetWorkspace(text.Container, out Workspace workspace))
		{
			DocumentId documentIdInCurrentContext = workspace.GetDocumentIdInCurrentContext(text.Container);
			if (documentIdInCurrentContext == null || !workspace.CurrentSolution.ContainsDocument(documentIdInCurrentContext))
			{
				return null;
			}
			return workspace.CurrentSolution.WithDocumentText(documentIdInCurrentContext, text, PreservationMode.PreserveIdentity).GetDocument(documentIdInCurrentContext);
		}
		return null;
	}

	public static IEnumerable<Document> GetRelatedDocuments(this SourceTextContainer container)
	{
		if (Workspace.TryGetWorkspace(container, out Workspace workspace))
		{
			Solution sol = workspace.CurrentSolution;
			return from id in workspace.GetRelatedDocumentIds(container)
				select sol.GetDocument(id) into d
				where d != null
				select d;
		}
		return SpecializedCollections.EmptyEnumerable<Document>();
	}

	public static Document GetOpenDocumentInCurrentContext(this SourceTextContainer container)
	{
		if (Workspace.TryGetWorkspace(container, out Workspace workspace))
		{
			DocumentId documentIdInCurrentContext = workspace.GetDocumentIdInCurrentContext(container);
			return workspace.CurrentSolution.GetDocument(documentIdInCurrentContext);
		}
		return null;
	}
}
