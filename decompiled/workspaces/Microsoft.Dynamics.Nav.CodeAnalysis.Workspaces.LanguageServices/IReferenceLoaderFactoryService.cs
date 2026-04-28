using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Host;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.LanguageServices;

internal interface IReferenceLoaderFactoryService : ILanguageService
{
	ISymbolReferenceLoader GetSymbolReferenceLoader(Workspace workspace, ProjectId projectId);
}
