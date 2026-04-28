using System.Threading.Tasks;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Host;

internal interface IWorkspaceTaskSchedulerFactory : IWorkspaceService
{
	IWorkspaceTaskScheduler CreateTaskScheduler(TaskScheduler taskScheduler = null);

	IWorkspaceTaskScheduler CreateTaskQueue(TaskScheduler taskScheduler = null);
}
