using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Host;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Log;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces;

internal class BackgroundCompiler : IDisposable
{
	private Workspace workspace;

	private readonly IWorkspaceTaskScheduler compilationScheduler;

	private Compilation[] mostRecentCompilations;

	private readonly object buildGate = new object();

	private CancellationTokenSource cancellationSource;

	public BackgroundCompiler(Workspace workspace)
	{
		this.workspace = workspace;
		IWorkspaceTaskSchedulerFactory service = workspace.Services.GetService<IWorkspaceTaskSchedulerFactory>();
		compilationScheduler = service.CreateTaskScheduler(TaskScheduler.Default);
		cancellationSource = new CancellationTokenSource();
		this.workspace.WorkspaceChanged += OnWorkspaceChanged;
		this.workspace.DocumentOpened += OnDocumentOpened;
		this.workspace.DocumentClosed += OnDocumentClosed;
	}

	public void Dispose()
	{
		if (workspace != null)
		{
			CancelBuild(releasePreviousCompilations: true);
			workspace.DocumentClosed -= OnDocumentClosed;
			workspace.DocumentOpened -= OnDocumentOpened;
			workspace.WorkspaceChanged -= OnWorkspaceChanged;
			workspace = null;
		}
		if (cancellationSource != null)
		{
			cancellationSource.Dispose();
			cancellationSource = null;
		}
	}

	private void OnDocumentOpened(object sender, DocumentEventArgs args)
	{
		Rebuild(args.Document.Project.Solution, args.Document.Project.Id);
	}

	private void OnDocumentClosed(object sender, DocumentEventArgs args)
	{
		Rebuild(args.Document.Project.Solution, args.Document.Project.Id);
	}

	private void OnWorkspaceChanged(object sender, WorkspaceChangeEventArgs args)
	{
		switch (args.Kind)
		{
		case WorkspaceChangeKind.SolutionAdded:
		case WorkspaceChangeKind.SolutionRemoved:
		case WorkspaceChangeKind.SolutionCleared:
			CancelBuild(releasePreviousCompilations: true);
			break;
		case WorkspaceChangeKind.SolutionChanged:
		case WorkspaceChangeKind.ProjectRemoved:
			Rebuild(args.NewSolution);
			break;
		default:
			Rebuild(args.NewSolution, args.ProjectId);
			break;
		}
	}

	private void Rebuild(Solution solution, ProjectId initialProject = null)
	{
		lock (buildGate)
		{
			CancelBuild(releasePreviousCompilations: false);
			ISet<ProjectId> set = (from d in workspace.GetOpenDocumentIds()
				select d.ProjectId).ToSet();
			if (set.Count > 0)
			{
				BuildCompilationsAsync(solution, initialProject, set);
			}
		}
	}

	private void CancelBuild(bool releasePreviousCompilations)
	{
		lock (buildGate)
		{
			cancellationSource.Cancel();
			cancellationSource = new CancellationTokenSource();
			if (releasePreviousCompilations)
			{
				mostRecentCompilations = null;
			}
		}
	}

	private void BuildCompilationsAsync(Solution solution, ProjectId initialProject, ISet<ProjectId> allProjects)
	{
		Solution solution2 = solution;
		ProjectId initialProject2 = initialProject;
		ISet<ProjectId> allProjects2 = allProjects;
		CancellationToken cancellationToken = cancellationSource.Token;
		compilationScheduler.ScheduleTask(() => BuildCompilationsAsync(solution2, initialProject2, allProjects2, cancellationToken), "BackgroundCompiler.BuildCompilationsAsync", cancellationToken);
	}

	private Task BuildCompilationsAsync(Solution solution, ProjectId initialProject, ISet<ProjectId> projectsToBuild, CancellationToken cancellationToken)
	{
		ProjectId initialProject2 = initialProject;
		List<ProjectId> list = new List<ProjectId>();
		if (initialProject2 != null)
		{
			list.Add(initialProject2);
		}
		list.AddRange(projectsToBuild.Where((ProjectId p) => p != initialProject2));
		IDisposable logger = Logger.LogBlock(FunctionId.BackgroundCompiler_BuildCompilationsAsync, cancellationToken);
		return Task.WhenAll((from p in list.Select(solution.GetProject)
			where p != null
			select p.GetCompilationAsync(cancellationToken).AsTask()).ToArray()).SafeContinueWith<Compilation[]>(delegate(Task<Compilation[]> t)
		{
			logger.Dispose();
			if (t.Status == TaskStatus.RanToCompletion)
			{
				lock (buildGate)
				{
					if (!cancellationToken.IsCancellationRequested)
					{
						mostRecentCompilations = t.Result;
					}
				}
			}
		}, CancellationToken.None, TaskContinuationOptions.None, TaskScheduler.Default);
	}
}
