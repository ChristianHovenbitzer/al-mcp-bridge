using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Host;

internal class WorkspaceTaskSchedulerFactory : IWorkspaceTaskSchedulerFactory, IWorkspaceService
{
	private class WorkspaceTaskScheduler : IWorkspaceTaskScheduler
	{
		private readonly WorkspaceTaskSchedulerFactory factory;

		private readonly TaskScheduler taskScheduler;

		public WorkspaceTaskScheduler(WorkspaceTaskSchedulerFactory factory, TaskScheduler taskScheduler)
		{
			this.factory = factory;
			this.taskScheduler = taskScheduler;
		}

		private TTask ScheduleTaskWorker<TTask>(string taskName, Func<TTask> taskCreator) where TTask : Task
		{
			taskName = taskName ?? (GetType().Name + ".ScheduleTask");
			object asyncToken = factory.BeginAsyncOperation(taskName);
			TTask val = taskCreator();
			factory.CompleteAsyncOperation(asyncToken, val);
			return val;
		}

		public Task ScheduleTask(Action taskAction, string taskName, CancellationToken cancellationToken)
		{
			Action taskAction2 = taskAction;
			return ScheduleTaskWorker(taskName, () => Task.Factory.SafeStartNew(taskAction2, cancellationToken, taskScheduler));
		}

		public Task<T> ScheduleTask<T>(Func<T> taskFunc, string taskName, CancellationToken cancellationToken)
		{
			Func<T> taskFunc2 = taskFunc;
			return ScheduleTaskWorker(taskName, () => Task.Factory.SafeStartNew(taskFunc2, cancellationToken, taskScheduler));
		}

		public Task ScheduleTask(Func<Task> taskFunc, string taskName, CancellationToken cancellationToken = default(CancellationToken))
		{
			Func<Task> taskFunc2 = taskFunc;
			return ScheduleTaskWorker(taskName, () => Task.Factory.SafeStartNewFromAsync(taskFunc2, cancellationToken, taskScheduler));
		}

		public Task<T> ScheduleTask<T>(Func<Task<T>> taskFunc, string taskName, CancellationToken cancellationToken = default(CancellationToken))
		{
			Func<Task<T>> taskFunc2 = taskFunc;
			return ScheduleTaskWorker(taskName, () => Task.Factory.SafeStartNewFromAsync(taskFunc2, cancellationToken, taskScheduler));
		}
	}

	internal sealed class WorkspaceTaskQueue : IWorkspaceTaskScheduler
	{
		private readonly WorkspaceTaskSchedulerFactory factory;

		private readonly SimpleTaskQueue queue;

		public WorkspaceTaskQueue(WorkspaceTaskSchedulerFactory factory, TaskScheduler taskScheduler)
		{
			this.factory = factory;
			queue = new SimpleTaskQueue(taskScheduler);
		}

		public T3 ScheduleTask<T1, T2, T3>(Func<T1, T2, T3> taskScheduler, string taskName, T1 arg1, T2 arg2) where T3 : Task
		{
			taskName = taskName ?? (GetType().Name + ".Task");
			object asyncToken = factory.BeginAsyncOperation(taskName);
			T3 val = taskScheduler(arg1, arg2);
			factory.CompleteAsyncOperation(asyncToken, val);
			return val;
		}

		public Task ScheduleTask(Action taskAction, string taskName, CancellationToken cancellationToken)
		{
			return ScheduleTask((Action t, CancellationToken c) => queue.ScheduleTask(t, c), taskName, taskAction, cancellationToken);
		}

		public Task<T> ScheduleTask<T>(Func<T> taskFunc, string taskName, CancellationToken cancellationToken)
		{
			return ScheduleTask((Func<T> t, CancellationToken c) => queue.ScheduleTask(t, c), taskName, taskFunc, cancellationToken);
		}

		public Task ScheduleTask(Func<Task> taskFunc, string taskName, CancellationToken cancellationToken = default(CancellationToken))
		{
			return ScheduleTask((Func<Task> t, CancellationToken c) => queue.ScheduleTask(t, c), taskName, taskFunc, cancellationToken);
		}

		public Task<T> ScheduleTask<T>(Func<Task<T>> taskFunc, string taskName, CancellationToken cancellationToken = default(CancellationToken))
		{
			return ScheduleTask((Func<Task<T>> t, CancellationToken c) => queue.ScheduleTask(t, c), taskName, taskFunc, cancellationToken);
		}
	}

	public virtual IWorkspaceTaskScheduler CreateTaskScheduler(TaskScheduler taskScheduler = null)
	{
		if (taskScheduler == null)
		{
			taskScheduler = ((SynchronizationContext.Current != null) ? TaskScheduler.FromCurrentSynchronizationContext() : TaskScheduler.Default);
		}
		return new WorkspaceTaskScheduler(this, taskScheduler);
	}

	public virtual IWorkspaceTaskScheduler CreateTaskQueue(TaskScheduler taskScheduler = null)
	{
		if (taskScheduler == null)
		{
			taskScheduler = ((SynchronizationContext.Current != null) ? TaskScheduler.FromCurrentSynchronizationContext() : TaskScheduler.Default);
		}
		return new WorkspaceTaskQueue(this, taskScheduler);
	}

	protected virtual object BeginAsyncOperation(string taskName)
	{
		return null;
	}

	protected virtual void CompleteAsyncOperation(object asyncToken, Task task)
	{
	}
}
