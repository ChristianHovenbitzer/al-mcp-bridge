using System;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Utilities;

internal sealed class SimpleTaskQueue
{
	private readonly TaskScheduler taskScheduler;

	private readonly object gate = new object();

	private int taskCount;

	public Task LastScheduledTask { get; private set; }

	public SimpleTaskQueue(TaskScheduler taskScheduler)
	{
		this.taskScheduler = taskScheduler;
		taskCount = 0;
		LastScheduledTask = SpecializedTasks.EmptyTask;
	}

	private TTask ScheduleTaskWorker<TTask>(Func<int, TTask> taskCreator) where TTask : Task
	{
		lock (gate)
		{
			taskCount++;
			int arg = ((taskCount % 100 == 0) ? 1 : 0);
			return (TTask)(LastScheduledTask = taskCreator(arg));
		}
	}

	public Task ScheduleTask(Action taskAction, CancellationToken cancellationToken = default(CancellationToken))
	{
		Action taskAction2 = taskAction;
		return ScheduleTaskWorker((int delay) => LastScheduledTask.ContinueWithAfterDelay(taskAction2, cancellationToken, delay, TaskContinuationOptions.None, taskScheduler));
	}

	public Task<T> ScheduleTask<T>(Func<T> taskFunc, CancellationToken cancellationToken = default(CancellationToken))
	{
		Func<T> taskFunc2 = taskFunc;
		return ScheduleTaskWorker((int delay) => LastScheduledTask.ContinueWithAfterDelay((Task t) => taskFunc2(), cancellationToken, delay, TaskContinuationOptions.None, taskScheduler));
	}

	public Task ScheduleTask(Func<Task> taskFuncAsync, CancellationToken cancellationToken = default(CancellationToken))
	{
		Func<Task> taskFuncAsync2 = taskFuncAsync;
		return ScheduleTaskWorker((int delay) => LastScheduledTask.ContinueWithAfterDelayFromAsync((Task t) => taskFuncAsync2(), cancellationToken, delay, TaskContinuationOptions.None, taskScheduler));
	}

	public Task<T> ScheduleTask<T>(Func<Task<T>> taskFuncAsync, CancellationToken cancellationToken = default(CancellationToken))
	{
		Func<Task<T>> taskFuncAsync2 = taskFuncAsync;
		return ScheduleTaskWorker((int delay) => LastScheduledTask.ContinueWithAfterDelayFromAsync((Task t) => taskFuncAsync2(), cancellationToken, delay, TaskContinuationOptions.None, taskScheduler));
	}
}
