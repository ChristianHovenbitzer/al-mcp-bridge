using System.Collections.Generic;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces;

public struct ProjectChanges
{
	public ProjectId ProjectId => NewProject.Id;

	public Project OldProject { get; }

	public Project NewProject { get; }

	internal ProjectChanges(Project newProject, Project oldProject)
	{
		NewProject = newProject;
		OldProject = oldProject;
	}

	public IEnumerable<ProjectReference> GetAddedProjectReferences()
	{
		HashSet<ProjectReference> oldRefs = new HashSet<ProjectReference>(OldProject.ProjectReferences);
		foreach (ProjectReference projectReference in NewProject.ProjectReferences)
		{
			if (!oldRefs.Contains(projectReference))
			{
				yield return projectReference;
			}
		}
	}

	public IEnumerable<ProjectReference> GetRemovedProjectReferences()
	{
		HashSet<ProjectReference> newRefs = new HashSet<ProjectReference>(NewProject.ProjectReferences);
		foreach (ProjectReference projectReference in OldProject.ProjectReferences)
		{
			if (!newRefs.Contains(projectReference))
			{
				yield return projectReference;
			}
		}
	}

	public IEnumerable<SymbolReferenceSpecification> GetAddedSymbolReferences()
	{
		HashSet<SymbolReferenceSpecification> oldMetadata = new HashSet<SymbolReferenceSpecification>(OldProject.SymbolReferences);
		foreach (SymbolReferenceSpecification symbolReference in NewProject.SymbolReferences)
		{
			if (!oldMetadata.Contains(symbolReference))
			{
				yield return symbolReference;
			}
		}
	}

	public IEnumerable<SymbolReferenceSpecification> GetRemovedSymbolReferences()
	{
		HashSet<SymbolReferenceSpecification> newMetadata = new HashSet<SymbolReferenceSpecification>(NewProject.SymbolReferences);
		foreach (SymbolReferenceSpecification symbolReference in OldProject.SymbolReferences)
		{
			if (!newMetadata.Contains(symbolReference))
			{
				yield return symbolReference;
			}
		}
	}

	public IEnumerable<AnalyzerReference> GetAddedAnalyzerReferences()
	{
		HashSet<AnalyzerReference> oldAnalyzerReferences = new HashSet<AnalyzerReference>(OldProject.AnalyzerReferences);
		foreach (AnalyzerReference analyzerReference in NewProject.AnalyzerReferences)
		{
			if (!oldAnalyzerReferences.Contains(analyzerReference))
			{
				yield return analyzerReference;
			}
		}
	}

	public IEnumerable<AnalyzerReference> GetRemovedAnalyzerReferences()
	{
		HashSet<AnalyzerReference> newAnalyzerReferences = new HashSet<AnalyzerReference>(NewProject.AnalyzerReferences);
		foreach (AnalyzerReference analyzerReference in OldProject.AnalyzerReferences)
		{
			if (!newAnalyzerReferences.Contains(analyzerReference))
			{
				yield return analyzerReference;
			}
		}
	}

	public IEnumerable<DocumentId> GetAddedDocuments()
	{
		foreach (DocumentId documentId in NewProject.DocumentIds)
		{
			if (!OldProject.ContainsDocument(documentId))
			{
				yield return documentId;
			}
		}
	}

	public IEnumerable<DocumentId> GetAddedAdditionalDocuments()
	{
		foreach (DocumentId additionalDocumentId in NewProject.AdditionalDocumentIds)
		{
			if (!OldProject.ContainsAdditionalDocument(additionalDocumentId))
			{
				yield return additionalDocumentId;
			}
		}
	}

	public IEnumerable<DocumentId> GetChangedDocuments()
	{
		foreach (DocumentId documentId in NewProject.DocumentIds)
		{
			DocumentState documentState = NewProject.GetDocumentState(documentId);
			DocumentState documentState2 = OldProject.GetDocumentState(documentId);
			if (documentState2 != null && documentState != documentState2)
			{
				yield return documentId;
			}
		}
	}

	public IEnumerable<DocumentId> GetChangedAdditionalDocuments()
	{
		foreach (DocumentId additionalDocumentId in NewProject.AdditionalDocumentIds)
		{
			TextDocumentState additionalDocumentState = NewProject.GetAdditionalDocumentState(additionalDocumentId);
			TextDocumentState additionalDocumentState2 = OldProject.GetAdditionalDocumentState(additionalDocumentId);
			if (additionalDocumentState2 != null && additionalDocumentState != additionalDocumentState2)
			{
				yield return additionalDocumentId;
			}
		}
	}

	public IEnumerable<DocumentId> GetRemovedDocuments()
	{
		foreach (DocumentId documentId in OldProject.DocumentIds)
		{
			if (!NewProject.ContainsDocument(documentId))
			{
				yield return documentId;
			}
		}
	}

	public IEnumerable<DocumentId> GetRemovedAdditionalDocuments()
	{
		foreach (DocumentId additionalDocumentId in OldProject.AdditionalDocumentIds)
		{
			if (!NewProject.ContainsAdditionalDocument(additionalDocumentId))
			{
				yield return additionalDocumentId;
			}
		}
	}
}
