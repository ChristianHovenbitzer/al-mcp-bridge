using System;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Host;

internal interface IWorkspaceTaskScheduler
{
	Task ScheduleTask(Action taskAction, string taskName, CancellationToken cancellationToken = default(CancellationToken));

	Task<T> ScheduleTask<T>(Func<T> taskFunc, string taskName, CancellationToken cancellationToken = default(CancellationToken));

	Task ScheduleTask(Func<Task> taskFunc, string taskName, CancellationToken cancellationToken = default(CancellationToken));

	Task<T> ScheduleTask<T>(Func<Task<T>> taskFunc, string taskName, CancellationToken cancellationToken = default(CancellationToken));
}
