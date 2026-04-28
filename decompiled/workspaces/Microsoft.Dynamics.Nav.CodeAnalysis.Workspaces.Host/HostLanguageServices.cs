using System;
using System.Collections.Immutable;
using Microsoft.Dynamics.Nav.CodeAnalysis.CodeActions;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Classification;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Completion;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.FindReferences;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.LanguageServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.SignatureHelp;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Host;

public class HostLanguageServices : AbstractHostLanguageServices
{
	private ImmutableDictionary<Type, ILanguageService> serviceMap = ImmutableDictionary<Type, ILanguageService>.Empty;

	public HostWorkspaceServices WorkspaceServices { get; }

	internal virtual ICompilationFactoryService CompilationFactory => GetService<ICompilationFactoryService>();

	internal HostLanguageServices(HostWorkspaceServices workspaceServices)
	{
		WorkspaceServices = workspaceServices;
	}

	public override TLanguageService GetService<TLanguageService>()
	{
		if (TryGetService(typeof(TLanguageService), out ILanguageService service))
		{
			return (TLanguageService)service;
		}
		return default(TLanguageService);
	}

	private bool TryGetService(Type serviceType, out ILanguageService service)
	{
		Type serviceType2 = serviceType;
		if (!serviceMap.TryGetValue(serviceType2, out service))
		{
			service = ImmutableInterlocked.GetOrAdd<Type, ILanguageService>(ref serviceMap, serviceType2, delegate(Type svctype)
			{
				if (svctype == typeof(ICompilationFactoryService))
				{
					return new CompilationFactoryService();
				}
				if (svctype == typeof(ISyntaxTreeFactoryService))
				{
					return new SyntaxTreeFactoryService(this);
				}
				if (svctype == typeof(IClassificationService))
				{
					return new ClassificationService();
				}
				if (svctype == typeof(IRecommendationService))
				{
					return new RecommendationService();
				}
				if (svctype == typeof(CompletionService))
				{
					return new ALCompletionService(WorkspaceServices.Workspace);
				}
				if (svctype == typeof(CodeActionService))
				{
					return CodeActionService.Create();
				}
				if (svctype == typeof(IReferenceLoaderFactoryService))
				{
					return new ReferenceLoaderFactoryService();
				}
				if (svctype == typeof(ISymbolUsageLoaderFactoryService))
				{
					return new SymbolUsageLoaderFactoryService();
				}
				if (svctype == typeof(IDotNetResolverFactoryService))
				{
					return new DotNetResolverFactoryService();
				}
				if (serviceType2 == typeof(ISymbolDisplayService))
				{
					return new SymbolDisplayService();
				}
				if (serviceType2 == typeof(IFindReferencesService))
				{
					return new FindReferencesService();
				}
				if (serviceType2 == typeof(ISignatureHelpService))
				{
					return new SignatureHelpService();
				}
				if (serviceType2 == typeof(ISyntaxFactsService))
				{
					return SyntaxFactsService.Instance;
				}
				if (serviceType2 == typeof(ISemanticFactsService))
				{
					return SemanticFactsService.Instance;
				}
				if (serviceType2 == typeof(ISyntaxFormattingService))
				{
					return SyntaxFormattingService.Instance;
				}
				return (serviceType2 == typeof(IFormattingService)) ? FormattingService.Instance : null;
			});
		}
		return service != null;
	}

	public TLanguageService GetRequiredService<TLanguageService>() where TLanguageService : ILanguageService
	{
		return GetService<TLanguageService>() ?? throw new InvalidOperationException(WorkspacesResources.WorkspaceServicesUnavailable);
	}
}
