using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Host;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.LanguageServices;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces;

internal class ReferenceLoaderFactoryService : IReferenceLoaderFactoryService, ILanguageService
{
	public ISymbolReferenceLoader GetSymbolReferenceLoader(Workspace workspace, ProjectId projectId)
	{
		workspace.SymbolReferenceLoader.SetNextLoaderForProject(workspace, projectId, null);
		return new ProjectBasedSymbolReferenceLoader(workspace.SymbolReferenceLoader, workspace, projectId);
	}
}
