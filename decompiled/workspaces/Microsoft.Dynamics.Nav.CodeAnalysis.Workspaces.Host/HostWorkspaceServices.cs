using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Options;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Host;

public class HostWorkspaceServices : AbstractHostWorkspaceServices
{
	private ImmutableDictionary<Type, IWorkspaceService> serviceMap = ImmutableDictionary<Type, IWorkspaceService>.Empty;

	private readonly Lazy<HostLanguageServices> hostLanguageService;

	public override AbstractHostServices HostServices { get; }

	public override Workspace Workspace { get; }

	public virtual IPersistentStorageService PersistentStorage => GetRequiredService<IPersistentStorageService>();

	public virtual ITemporaryStorageService TemporaryStorage => GetRequiredService<ITemporaryStorageService>();

	internal virtual ITextFactoryService TextFactory => GetRequiredService<ITextFactoryService>();

	public virtual IEnumerable<string> SupportedLanguages => SpecializedCollections.EmptyEnumerable<string>();

	internal HostWorkspaceServices(AbstractHostServices hostServices, Workspace workspace)
	{
		HostServices = hostServices;
		Workspace = workspace;
		hostLanguageService = new Lazy<HostLanguageServices>(() => new HostLanguageServices(this));
	}

	public override TWorkspaceService GetRequiredService<TWorkspaceService>()
	{
		return GetService<TWorkspaceService>() ?? throw new InvalidOperationException(WorkspacesResources.WorkspaceServicesUnavailable);
	}

	public virtual bool IsSupported(string languageName)
	{
		return false;
	}

	public override AbstractHostLanguageServices GetLanguageServices(string languageName)
	{
		if (languageName == "AL")
		{
			return hostLanguageService.Value;
		}
		throw new NotSupportedException(string.Format(CultureInfo.CurrentUICulture, WorkspacesResources.UnsupportedLanguage, languageName));
	}

	public override TWorkspaceService GetService<TWorkspaceService>()
	{
		if (TryGetService(typeof(TWorkspaceService), out IWorkspaceService service))
		{
			return (TWorkspaceService)service;
		}
		return default(TWorkspaceService);
	}

	private bool TryGetService(Type serviceType, out IWorkspaceService service)
	{
		Type serviceType2 = serviceType;
		if (!serviceMap.TryGetValue(serviceType2, out service))
		{
			service = ImmutableInterlocked.GetOrAdd<Type, IWorkspaceService>(ref serviceMap, serviceType2, delegate
			{
				if (serviceType2 == typeof(IWorkspaceTaskSchedulerFactory))
				{
					return new WorkspaceTaskSchedulerFactory();
				}
				if (serviceType2 == typeof(ITemporaryStorageService))
				{
					return new TrivialTemporaryStorageService();
				}
				if (serviceType2 == typeof(IOptionService))
				{
					return OptionServiceFactory.Create();
				}
				return (serviceType2 == typeof(IDocumentTextDifferencingService)) ? new DefaultDocumentTextDifferencingService() : null;
			});
		}
		return service != null;
	}
}
