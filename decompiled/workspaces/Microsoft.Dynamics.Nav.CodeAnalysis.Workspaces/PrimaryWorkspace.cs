using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces;

internal static class PrimaryWorkspace
{
	private static readonly ReaderWriterLockSlim registryGate = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);

	private static Workspace primaryWorkspace;

	private static readonly List<TaskCompletionSource<Workspace>> primaryWorkspaceTaskSourceList = new List<TaskCompletionSource<Workspace>>();

	public static Workspace Workspace
	{
		get
		{
			using (registryGate.DisposableRead())
			{
				return primaryWorkspace;
			}
		}
	}

	public static void Register(Workspace workspace)
	{
		if (workspace == null)
		{
			throw new ArgumentNullException("workspace");
		}
		using (registryGate.DisposableWrite())
		{
			primaryWorkspace = workspace;
			foreach (TaskCompletionSource<Workspace> primaryWorkspaceTaskSource in primaryWorkspaceTaskSourceList)
			{
				try
				{
					primaryWorkspaceTaskSource.TrySetResult(workspace);
				}
				catch
				{
				}
			}
			primaryWorkspaceTaskSourceList.Clear();
		}
	}

	public static Task<Workspace> GetWorkspaceAsync(CancellationToken cancellationToken = default(CancellationToken))
	{
		using (registryGate.DisposableWrite())
		{
			if (primaryWorkspace != null)
			{
				return Task.FromResult(primaryWorkspace);
			}
			TaskCompletionSource<Workspace> taskSource = new TaskCompletionSource<Workspace>();
			if (cancellationToken.CanBeCanceled)
			{
				try
				{
					CancellationTokenRegistration registration = cancellationToken.Register(delegate
					{
						taskSource.TrySetCanceled();
					});
					taskSource.Task.ContinueWith(delegate
					{
						registration.Dispose();
					}, CancellationToken.None, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
				}
				catch
				{
				}
			}
			primaryWorkspaceTaskSourceList.Add(taskSource);
			return taskSource.Task;
		}
	}
}
