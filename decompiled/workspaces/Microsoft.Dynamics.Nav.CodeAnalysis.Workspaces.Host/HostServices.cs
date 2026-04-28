using System.Threading;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Host;

public class HostServices : AbstractHostServices
{
	private static HostServices defaultHost;

	public static HostServices DefaultHost
	{
		get
		{
			if (defaultHost == null)
			{
				HostServices value = Create();
				Interlocked.CompareExchange(ref defaultHost, value, null);
			}
			return defaultHost;
		}
	}

	public static HostServices Create()
	{
		return new HostServices();
	}

	private HostServices()
	{
	}

	protected internal override AbstractHostWorkspaceServices CreateWorkspaceServices(Workspace workspace)
	{
		return new HostWorkspaceServices(this, workspace);
	}
}
