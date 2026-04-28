using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Host;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.LanguageServices;

internal class SemanticModelWorkspaceServiceFactory
{
	private class SemanticModelService : ISemanticModelService, IWorkspaceService, IDisposable
	{
		private class CompilationSet
		{
			private const int RebuildThreshold = 3;

			public readonly VersionStamp Version;

			public readonly ValueSource<Compilation> Compilation;

			public readonly ImmutableDictionary<DocumentId, SyntaxTree> Trees;

			public static async Task<CompilationSet> CreateAsync(Project project, CompilationSet oldCompilationSet, CancellationToken cancellationToken)
			{
				Compilation compilation = await project.GetCompilationAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
				return new CompilationSet(await project.GetDependentSemanticVersionAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false), map: GetTreeMap(project, compilation, oldCompilationSet, cancellationToken), compilation: GetCompilation(project, compilation));
			}

			private CompilationSet(VersionStamp version, ValueSource<Compilation> compilation, ImmutableDictionary<DocumentId, SyntaxTree> map)
			{
				Version = version;
				Compilation = compilation;
				Trees = map;
			}

			private static ImmutableDictionary<DocumentId, SyntaxTree> GetTreeMap(Project project, Compilation compilation, CompilationSet oldCompilationSet, CancellationToken cancellationToken)
			{
				int num = compilation.SyntaxTrees.Count();
				if (oldCompilationSet == null || Math.Abs(oldCompilationSet.Trees.Count - num) > 3)
				{
					return ImmutableDictionary.CreateRange(GetNewTreeMap(project, compilation));
				}
				ImmutableDictionary<DocumentId, SyntaxTree> immutableDictionary = AddOrUpdateNewTreeToOldMap(project, compilation, oldCompilationSet, cancellationToken);
				if (immutableDictionary.Count == num && oldCompilationSet.Trees.Count <= num)
				{
					return immutableDictionary;
				}
				return RemoveOldTreeFromMap(compilation, oldCompilationSet.Trees, immutableDictionary, cancellationToken);
			}

			private static ImmutableDictionary<DocumentId, SyntaxTree> RemoveOldTreeFromMap(Compilation newCompilation, ImmutableDictionary<DocumentId, SyntaxTree> oldMap, ImmutableDictionary<DocumentId, SyntaxTree> map, CancellationToken cancellationToken)
			{
				foreach (KeyValuePair<DocumentId, SyntaxTree> item in oldMap)
				{
					cancellationToken.ThrowIfCancellationRequested();
					if (!newCompilation.ContainsSyntaxTree(item.Value))
					{
						DocumentId key = item.Key;
						if (map.TryGetValue(key, out SyntaxTree value) && value == item.Value)
						{
							map = map.Remove(key);
						}
					}
				}
				return map;
			}

			private static ImmutableDictionary<DocumentId, SyntaxTree> AddOrUpdateNewTreeToOldMap(Project newProject, Compilation newCompilation, CompilationSet oldSet, CancellationToken cancellationToken)
			{
				if (!oldSet.Compilation.TryGetValue(out Compilation value))
				{
					return ImmutableDictionary.CreateRange(GetNewTreeMap(newProject, newCompilation));
				}
				ImmutableDictionary<DocumentId, SyntaxTree> immutableDictionary = oldSet.Trees;
				ImmutableArray<SyntaxTree>.Enumerator enumerator = newCompilation.SyntaxTrees.GetEnumerator();
				while (enumerator.MoveNext())
				{
					SyntaxTree current = enumerator.Current;
					cancellationToken.ThrowIfCancellationRequested();
					if (!value.ContainsSyntaxTree(current))
					{
						DocumentId documentId = newProject.GetDocumentId(current);
						if (!(documentId == null))
						{
							immutableDictionary = immutableDictionary.SetItem(documentId, current);
						}
					}
				}
				return immutableDictionary;
			}

			private static IEnumerable<KeyValuePair<DocumentId, SyntaxTree>> GetNewTreeMap(Project project, Compilation compilation)
			{
				ImmutableArray<SyntaxTree>.Enumerator enumerator = compilation.SyntaxTrees.GetEnumerator();
				while (enumerator.MoveNext())
				{
					SyntaxTree current = enumerator.Current;
					DocumentId documentId = project.GetDocumentId(current);
					if (documentId != null)
					{
						yield return new KeyValuePair<DocumentId, SyntaxTree>(documentId, current);
					}
				}
			}

			private static ValueSource<Compilation> GetCompilation(Project project, Compilation compilation)
			{
				IProjectCacheHostService service = project.Solution.Workspace.Services.GetService<IProjectCacheHostService>();
				if (service != null && project.Solution.BranchId == project.Solution.Workspace.PrimaryBranchId)
				{
					return new WeakConstantValueSource<Compilation>(service.CacheObjectIfCachingEnabledForKey(project.Id, project, compilation));
				}
				return new ConstantValueSource<Compilation>(compilation);
			}

			[Conditional("DEBUG")]
			private static void ValidateTreeMap(ImmutableDictionary<DocumentId, SyntaxTree> actual, Project project, Compilation compilation)
			{
				ImmutableDictionary.CreateRange(GetNewTreeMap(project, compilation));
			}
		}

		private static readonly ConditionalWeakTable<Workspace, ConditionalWeakTable<BranchId, Dictionary<ProjectId, CompilationSet>>> map = new ConditionalWeakTable<Workspace, ConditionalWeakTable<BranchId, Dictionary<ProjectId, CompilationSet>>>();

		private static readonly ConditionalWeakTable<Compilation, ConditionalWeakTable<SyntaxNode, WeakReference<SemanticModel>>> semanticModelMap = new ConditionalWeakTable<Compilation, ConditionalWeakTable<SyntaxNode, WeakReference<SemanticModel>>>();

		private readonly ReaderWriterLockSlim gate = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);

		private static readonly ConditionalWeakTable<BranchId, Dictionary<ProjectId, CompilationSet>>.CreateValueCallback createVersionMap = (BranchId c) => new Dictionary<ProjectId, CompilationSet>();

		private static readonly ConditionalWeakTable<Compilation, ConditionalWeakTable<SyntaxNode, WeakReference<SemanticModel>>>.CreateValueCallback createNodeMap = (Compilation c) => new ConditionalWeakTable<SyntaxNode, WeakReference<SemanticModel>>();

		public async Task<SemanticModel> GetSemanticModelForNodeAsync(Document document, SyntaxNode node, CancellationToken cancellationToken = default(CancellationToken))
		{
			ISyntaxFactsService syntaxFactsService = document.Project.LanguageServices.GetService<ISyntaxFactsService>();
			if (syntaxFactsService == null || node == null)
			{
				return await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
			}
			if (IsPrimaryBranch(document) && !document.IsOpen())
			{
				return await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
			}
			Dictionary<ProjectId, CompilationSet> versionMap = GetVersionMapFromBranchOrPrimary(document.Project.Solution.Workspace, document.Project.Solution.BranchId);
			ProjectId projectId = document.Project.Id;
			VersionStamp version = await document.Project.GetDependentSemanticVersionAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
			CompilationSet value;
			using (gate.DisposableRead())
			{
				versionMap.TryGetValue(projectId, out value);
			}
			if (value == null)
			{
				await AddVersionCacheAsync(document.Project, version, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
				return await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
			}
			if (version.Equals(value.Version))
			{
				if (!value.Compilation.TryGetValue(out Compilation oldCompilation))
				{
					await AddVersionCacheAsync(document.Project, version, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
					return await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
				}
				if (!value.Trees.TryGetValue(document.Id, out SyntaxTree oldTree))
				{
					return await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
				}
				SyntaxNode syntaxNode = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
				if (syntaxNode.SyntaxTree == oldTree)
				{
					return await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
				}
				SyntaxNode member = syntaxFactsService.GetContainingMemberDeclaration(syntaxNode, node.SpanStart);
				if (!syntaxFactsService.IsMethodLevelMember(member))
				{
					return await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
				}
				SemanticModel cachedSemanticModel = GetCachedSemanticModel(oldCompilation, member);
				if (cachedSemanticModel != null)
				{
					return cachedSemanticModel;
				}
				SyntaxNode syntaxNode2 = syntaxFactsService.GetMethodLevelMember(memberId: syntaxFactsService.GetMethodLevelMemberId(syntaxNode, member), root: await oldTree.GetRootAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false));
				if (syntaxNode2 == null)
				{
					return await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
				}
				if (!TryGetSpeculativeSemanticModel(oldCompilation.GetSemanticModel(oldTree), syntaxNode2, member, out SemanticModel speculativeModel))
				{
					return await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
				}
				Contract.ThrowIfNull(speculativeModel);
				return CacheSemanticModel(oldCompilation, member, speculativeModel);
			}
			await UpdateVersionCacheAsync(document.Project, version, value, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
			return await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		}

		private static bool TryGetSpeculativeSemanticModel(SemanticModel oldSemanticModel, SyntaxNode oldNode, SyntaxNode newNode, out SemanticModel speculativeModel)
		{
			MethodOrTriggerDeclarationSyntax methodOrTriggerDeclarationSyntax = oldNode as MethodOrTriggerDeclarationSyntax;
			MethodOrTriggerDeclarationSyntax methodOrTriggerDeclarationSyntax2 = newNode as MethodOrTriggerDeclarationSyntax;
			if (methodOrTriggerDeclarationSyntax == null || methodOrTriggerDeclarationSyntax2 == null || methodOrTriggerDeclarationSyntax.Body == null)
			{
				speculativeModel = null;
				return false;
			}
			SemanticModel speculativeModel2;
			bool result = oldSemanticModel.TryGetSpeculativeSemanticModelForMethodBody(methodOrTriggerDeclarationSyntax.Body.BeginKeywordToken.Span.End, methodOrTriggerDeclarationSyntax2, out speculativeModel2);
			speculativeModel = speculativeModel2;
			return result;
		}

		private static bool IsPrimaryBranch(Document document)
		{
			return document.Project.Solution.BranchId == document.Project.Solution.Workspace.PrimaryBranchId;
		}

		private Task AddVersionCacheAsync(Project project, VersionStamp version, CancellationToken cancellationToken)
		{
			return UpdateVersionCacheAsync(project, version, null, cancellationToken);
		}

		private async Task UpdateVersionCacheAsync(Project project, VersionStamp version, CompilationSet primarySet, CancellationToken cancellationToken)
		{
			Dictionary<ProjectId, CompilationSet> versionMap = GetVersionMapFromBranch(project.Solution.Workspace, project.Solution.BranchId);
			if (AlreadyHasLatestCompilationSet(versionMap, project.Id, version, out CompilationSet compilationSet) && compilationSet.Compilation.TryGetValue(out Compilation _))
			{
				return;
			}
			CompilationSet value2 = await CompilationSet.CreateAsync(project, compilationSet ?? primarySet, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
			using (gate.DisposableWrite())
			{
				if (!versionMap.TryGetValue(project.Id, out compilationSet) || version != compilationSet.Version)
				{
					versionMap[project.Id] = value2;
				}
			}
		}

		private bool AlreadyHasLatestCompilationSet(Dictionary<ProjectId, CompilationSet> versionMap, ProjectId projectId, VersionStamp version, out CompilationSet compilationSet)
		{
			using (gate.DisposableRead())
			{
				return versionMap.TryGetValue(projectId, out compilationSet) && version == compilationSet.Version;
			}
		}

		private static SemanticModel GetCachedSemanticModel(ConditionalWeakTable<SyntaxNode, WeakReference<SemanticModel>> nodeMap, SyntaxNode newMember)
		{
			if (!nodeMap.TryGetValue(newMember, out WeakReference<SemanticModel> value) || !value.TryGetTarget(out var target))
			{
				return null;
			}
			return target;
		}

		private static SemanticModel GetCachedSemanticModel(Compilation oldCompilation, SyntaxNode newMember)
		{
			return GetCachedSemanticModel(semanticModelMap.GetValue(oldCompilation, createNodeMap), newMember);
		}

		private static SemanticModel CacheSemanticModel(Compilation oldCompilation, SyntaxNode newMember, SemanticModel speculativeSemanticModel)
		{
			ConditionalWeakTable<SyntaxNode, WeakReference<SemanticModel>> value = semanticModelMap.GetValue(oldCompilation, createNodeMap);
			SemanticModel cachedSemanticModel = GetCachedSemanticModel(value, newMember);
			if (cachedSemanticModel != null)
			{
				return cachedSemanticModel;
			}
			WeakReference<SemanticModel> weakReference = new WeakReference<SemanticModel>(speculativeSemanticModel);
			WeakReference<SemanticModel> value2 = value.GetValue(newMember, (SyntaxNode c) => weakReference);
			if (value2.TryGetTarget(out var target))
			{
				return target;
			}
			value2.SetTarget(speculativeSemanticModel);
			return speculativeSemanticModel;
		}

		private Dictionary<ProjectId, CompilationSet> GetVersionMapFromBranchOrPrimary(Workspace workspace, BranchId branchId)
		{
			ConditionalWeakTable<BranchId, Dictionary<ProjectId, CompilationSet>> branchMap = GetBranchMap(workspace);
			if (branchMap.TryGetValue(branchId, out var value))
			{
				return value;
			}
			if (branchMap.TryGetValue(workspace.PrimaryBranchId, out value))
			{
				return value;
			}
			return branchMap.GetValue(branchId, createVersionMap);
		}

		private Dictionary<ProjectId, CompilationSet> GetVersionMapFromBranch(Workspace workspace, BranchId branchId)
		{
			return GetBranchMap(workspace).GetValue(branchId, createVersionMap);
		}

		private ConditionalWeakTable<BranchId, Dictionary<ProjectId, CompilationSet>> GetBranchMap(Workspace workspace)
		{
			if (!map.TryGetValue(workspace, out ConditionalWeakTable<BranchId, Dictionary<ProjectId, CompilationSet>> value))
			{
				ConditionalWeakTable<BranchId, Dictionary<ProjectId, CompilationSet>> newBranchMap = new ConditionalWeakTable<BranchId, Dictionary<ProjectId, CompilationSet>>();
				value = map.GetValue(workspace, (Workspace c) => newBranchMap);
				if (value == newBranchMap)
				{
					workspace.DocumentClosed += OnDocumentClosed;
					workspace.WorkspaceChanged += OnWorkspaceChanged;
				}
			}
			return value;
		}

		private void OnDocumentClosed(object sender, DocumentEventArgs e)
		{
			ClearVersionMap(e.Document.Project.Solution.Workspace, e.Document.Id);
		}

		private void OnWorkspaceChanged(object sender, WorkspaceChangeEventArgs e)
		{
			switch (e.Kind)
			{
			case WorkspaceChangeKind.SolutionChanged:
			case WorkspaceChangeKind.SolutionAdded:
			case WorkspaceChangeKind.SolutionRemoved:
			case WorkspaceChangeKind.SolutionCleared:
			case WorkspaceChangeKind.SolutionReloaded:
				ClearVersionMap(e.NewSolution.Workspace, e.NewSolution.ProjectIds);
				break;
			case WorkspaceChangeKind.ProjectAdded:
			case WorkspaceChangeKind.ProjectRemoved:
			case WorkspaceChangeKind.ProjectChanged:
			case WorkspaceChangeKind.ProjectReloaded:
				ClearVersionMap(e.NewSolution.Workspace, e.ProjectId);
				break;
			case WorkspaceChangeKind.DocumentRemoved:
				ClearVersionMap(e.NewSolution.Workspace, e.DocumentId);
				break;
			default:
				Contract.Fail("Unknown event");
				break;
			case WorkspaceChangeKind.DocumentAdded:
			case WorkspaceChangeKind.DocumentReloaded:
			case WorkspaceChangeKind.DocumentChanged:
			case WorkspaceChangeKind.AdditionalDocumentAdded:
			case WorkspaceChangeKind.AdditionalDocumentRemoved:
			case WorkspaceChangeKind.AdditionalDocumentReloaded:
			case WorkspaceChangeKind.AdditionalDocumentChanged:
			case WorkspaceChangeKind.ActiveDocumentChanged:
				break;
			}
		}

		private void ClearVersionMap(Workspace workspace, DocumentId documentId)
		{
			if (workspace.GetOpenDocumentIds(documentId.ProjectId).Any())
			{
				return;
			}
			Dictionary<ProjectId, CompilationSet> versionMapFromBranch = GetVersionMapFromBranch(workspace, workspace.PrimaryBranchId);
			using (gate.DisposableWrite())
			{
				versionMapFromBranch.Remove(documentId.ProjectId);
			}
		}

		private void ClearVersionMap(Workspace workspace, ProjectId projectId)
		{
			Dictionary<ProjectId, CompilationSet> versionMapFromBranch = GetVersionMapFromBranch(workspace, workspace.PrimaryBranchId);
			using (gate.DisposableWrite())
			{
				versionMapFromBranch.Remove(projectId);
			}
		}

		private void ClearVersionMap(Workspace workspace, IReadOnlyList<ProjectId> projectIds)
		{
			Dictionary<ProjectId, CompilationSet> versionMapFromBranch = GetVersionMapFromBranch(workspace, workspace.PrimaryBranchId);
			using (gate.DisposableWrite())
			{
				using PooledObject<HashSet<ProjectId>> pooledObject = SharedPools.Default<HashSet<ProjectId>>().GetPooledObject();
				HashSet<ProjectId> @object = pooledObject.Object;
				@object.UnionWith(versionMapFromBranch.Keys);
				@object.ExceptWith(projectIds);
				foreach (ProjectId item in @object)
				{
					versionMapFromBranch.Remove(item);
				}
			}
		}

		public void Dispose()
		{
			gate.Dispose();
		}
	}

	public static IWorkspaceService CreateService()
	{
		return new SemanticModelService();
	}
}
