using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Host;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces;

internal class BackgroundParser : IDisposable
{
	private readonly Workspace workspace;

	private readonly IWorkspaceTaskScheduler taskScheduler;

	private ReaderWriterLockSlim stateLock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);

	private readonly object parseGate = new object();

	private ImmutableDictionary<DocumentId, CancellationTokenSource> workMap = ImmutableDictionary.Create<DocumentId, CancellationTokenSource>();

	private bool disposedValue;

	public bool IsStarted { get; private set; }

	public BackgroundParser(Workspace workspace)
	{
		this.workspace = workspace;
		IWorkspaceTaskSchedulerFactory service = workspace.Services.GetService<IWorkspaceTaskSchedulerFactory>();
		taskScheduler = service.CreateTaskScheduler(TaskScheduler.Default);
		this.workspace.WorkspaceChanged += OnWorkspaceChanged;
		this.workspace.DocumentOpened += OnDocumentOpened;
		this.workspace.DocumentClosed += OnDocumentClosed;
	}

	private void OnDocumentOpened(object sender, DocumentEventArgs args)
	{
		Parse(args.Document);
	}

	private void OnDocumentClosed(object sender, DocumentEventArgs args)
	{
		CancelParse(args.Document.Id);
	}

	private void OnWorkspaceChanged(object sender, WorkspaceChangeEventArgs args)
	{
		switch (args.Kind)
		{
		case WorkspaceChangeKind.SolutionAdded:
		case WorkspaceChangeKind.SolutionRemoved:
		case WorkspaceChangeKind.SolutionCleared:
			CancelAllParses();
			break;
		case WorkspaceChangeKind.DocumentRemoved:
			CancelParse(args.DocumentId);
			break;
		case WorkspaceChangeKind.DocumentChanged:
			ParseIfOpen(args.NewSolution.GetDocument(args.DocumentId));
			break;
		case WorkspaceChangeKind.ProjectChanged:
		{
			Project project = args.OldSolution.GetProject(args.ProjectId);
			Project project2 = args.NewSolution.GetProject(args.ProjectId);
			if (!project.SupportsCompilation || object.Equals(project.ParseOptions, project2.ParseOptions))
			{
				break;
			}
			{
				foreach (Document document in args.NewSolution.GetProject(args.ProjectId).Documents)
				{
					ParseIfOpen(document);
				}
				break;
			}
		}
		}
	}

	public void Start()
	{
		using (stateLock.DisposableRead())
		{
			if (!IsStarted)
			{
				IsStarted = true;
			}
		}
	}

	public void Stop()
	{
		using (stateLock.DisposableWrite())
		{
			if (IsStarted)
			{
				CancelAllParses_NoLock();
				IsStarted = false;
			}
		}
	}

	public void CancelAllParses()
	{
		using (stateLock.DisposableWrite())
		{
			CancelAllParses_NoLock();
		}
	}

	private void CancelAllParses_NoLock()
	{
		stateLock.AssertCanWrite();
		foreach (KeyValuePair<DocumentId, CancellationTokenSource> item in workMap)
		{
			item.Value.Cancel();
		}
		workMap = ImmutableDictionary.Create<DocumentId, CancellationTokenSource>();
	}

	public void CancelParse(DocumentId documentId)
	{
		if (!(documentId != null))
		{
			return;
		}
		using (stateLock.DisposableWrite())
		{
			if (workMap.TryGetValue(documentId, out CancellationTokenSource value))
			{
				value.Cancel();
				workMap = workMap.Remove(documentId);
			}
		}
	}

	public void Parse(Document document)
	{
		if (document == null)
		{
			return;
		}
		lock (parseGate)
		{
			CancelParse(document.Id);
			if (IsStarted)
			{
				ParseDocumentAsync(document);
			}
		}
	}

	private void ParseIfOpen(Document document)
	{
		if (document != null && document.IsOpen())
		{
			Parse(document);
		}
	}

	private void ParseDocumentAsync(Document document)
	{
		Document document2 = document;
		CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
		using (stateLock.DisposableWrite())
		{
			workMap = workMap.Add(document2.Id, cancellationTokenSource);
		}
		CancellationToken cancellationToken = cancellationTokenSource.Token;
		taskScheduler.ScheduleTask(() => document2.GetSyntaxTreeAsync(cancellationToken), "BackgroundParser.ParseDocumentAsync", cancellationToken).SafeContinueWith(delegate
		{
			if (disposedValue)
			{
				return;
			}
			using (stateLock?.DisposableWrite())
			{
				workMap = workMap.Remove(document2.Id);
			}
		}, CancellationToken.None, TaskContinuationOptions.None, TaskScheduler.Default);
	}

	protected virtual void Dispose(bool disposing)
	{
		if (!disposedValue)
		{
			if (disposing)
			{
				IsStarted = false;
				stateLock.Dispose();
				stateLock = null;
			}
			disposedValue = true;
		}
	}

	public void Dispose()
	{
		Dispose(disposing: true);
	}
}
