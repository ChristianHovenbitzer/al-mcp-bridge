using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces;

public class ProjectDependencyGraph
{
	private readonly ImmutableArray<ProjectId> projectIds;

	private readonly ImmutableDictionary<ProjectId, ImmutableHashSet<ProjectId>> referencesMap;

	private readonly NonReentrantLock dataLock = new NonReentrantLock();

	private ImmutableDictionary<ProjectId, ImmutableHashSet<ProjectId>> lazyReverseReferencesMap;

	private ImmutableArray<ProjectId> lazyTopologicallySortedProjects;

	private ImmutableArray<IEnumerable<ProjectId>> lazyDependencySets;

	private ImmutableDictionary<ProjectId, ImmutableHashSet<ProjectId>> transitiveReferencesMap = ImmutableDictionary<ProjectId, ImmutableHashSet<ProjectId>>.Empty;

	private ImmutableDictionary<ProjectId, ImmutableHashSet<ProjectId>> reverseTransitiveReferencesMap = ImmutableDictionary<ProjectId, ImmutableHashSet<ProjectId>>.Empty;

	internal static readonly ProjectDependencyGraph Empty = new ProjectDependencyGraph(ImmutableArray.Create<ProjectId>(), ImmutableDictionary<ProjectId, ImmutableHashSet<ProjectId>>.Empty);

	internal ProjectDependencyGraph(ImmutableArray<ProjectId> projectIds, ImmutableDictionary<ProjectId, ImmutableHashSet<ProjectId>> referencesMap)
	{
		this.projectIds = projectIds;
		this.referencesMap = referencesMap;
	}

	public IImmutableSet<ProjectId> GetProjectsThatThisProjectDirectlyDependsOn(ProjectId projectId)
	{
		if (projectId == null)
		{
			throw new ArgumentNullException("projectId");
		}
		if (referencesMap.TryGetValue(projectId, out ImmutableHashSet<ProjectId> value))
		{
			return value;
		}
		return ImmutableHashSet<ProjectId>.Empty;
	}

	public IList<ProjectId> GetProjectsThatThisProjectDirectlyDependsOnTopologicallySorted(ProjectId projectId)
	{
		IEnumerable<ProjectId> projectsThatThisProjectDirectlyDependsOn = GetProjectsThatThisProjectDirectlyDependsOn(projectId);
		return SortDependenciesTopologically(projectsThatThisProjectDirectlyDependsOn);
	}

	public IImmutableSet<ProjectId> GetProjectsThatDirectlyDependOnThisProject(ProjectId projectId)
	{
		if (projectId == null)
		{
			throw new ArgumentNullException("projectId");
		}
		if (lazyReverseReferencesMap == null)
		{
			using (dataLock.DisposableWait())
			{
				return GetProjectsThatDirectlyDependOnThisProject_NoLock(projectId);
			}
		}
		return GetProjectsThatDirectlyDependOnThisProject_NoLock(projectId);
	}

	public IList<ProjectId> GetProjectsThatDirectlyDependOnThisProjectTopologicallySorted(ProjectId projectId)
	{
		IImmutableSet<ProjectId> projectsThatDirectlyDependOnThisProject = GetProjectsThatDirectlyDependOnThisProject(projectId);
		return SortDependenciesTopologically(projectsThatDirectlyDependOnThisProject);
	}

	private ImmutableHashSet<ProjectId> GetProjectsThatDirectlyDependOnThisProject_NoLock(ProjectId projectId)
	{
		if (lazyReverseReferencesMap == null)
		{
			lazyReverseReferencesMap = ComputeReverseReferencesMap();
		}
		if (lazyReverseReferencesMap.TryGetValue(projectId, out ImmutableHashSet<ProjectId> value))
		{
			return value;
		}
		return ImmutableHashSet<ProjectId>.Empty;
	}

	private ImmutableDictionary<ProjectId, ImmutableHashSet<ProjectId>> ComputeReverseReferencesMap()
	{
		Dictionary<ProjectId, HashSet<ProjectId>> dictionary = new Dictionary<ProjectId, HashSet<ProjectId>>();
		foreach (KeyValuePair<ProjectId, ImmutableHashSet<ProjectId>> item in referencesMap)
		{
			foreach (ProjectId item2 in item.Value)
			{
				if (!dictionary.TryGetValue(item2, out var value))
				{
					value = new HashSet<ProjectId>();
					dictionary.Add(item2, value);
				}
				value.Add(item.Key);
			}
		}
		return dictionary.Select((KeyValuePair<ProjectId, HashSet<ProjectId>> kvp) => new KeyValuePair<ProjectId, ImmutableHashSet<ProjectId>>(kvp.Key, kvp.Value.ToImmutableHashSet())).ToImmutableDictionary();
	}

	public IImmutableSet<ProjectId> GetProjectsThatThisProjectTransitivelyDependsOn(ProjectId projectId)
	{
		if (projectId == null)
		{
			throw new ArgumentNullException("projectId");
		}
		if (transitiveReferencesMap.TryGetValue(projectId, out ImmutableHashSet<ProjectId> value))
		{
			return value;
		}
		using (dataLock.DisposableWait())
		{
			return GetProjectsThatThisProjectTransitivelyDependsOn_NoLock(projectId);
		}
	}

	public IList<ProjectId> GetProjectsThatThisProjectTransitivelyDependsOnTopologicallySorted(ProjectId projectId)
	{
		IImmutableSet<ProjectId> projectsThatThisProjectTransitivelyDependsOn = GetProjectsThatThisProjectTransitivelyDependsOn(projectId);
		return SortDependenciesTopologically(projectsThatThisProjectTransitivelyDependsOn);
	}

	internal IList<ProjectId> SortDependenciesTopologically(IEnumerable<ProjectId> dependencies)
	{
		PooledList<ProjectId> instance = PooledList<ProjectId>.GetInstance();
		try
		{
			foreach (ProjectId topologicallySortedProject in GetTopologicallySortedProjects())
			{
				if (dependencies.Contains(topologicallySortedProject))
				{
					instance.Add(topologicallySortedProject);
				}
			}
			return instance.ToList();
		}
		finally
		{
			instance.Free();
		}
	}

	private ImmutableHashSet<ProjectId> GetProjectsThatThisProjectTransitivelyDependsOn_NoLock(ProjectId projectId)
	{
		if (!transitiveReferencesMap.TryGetValue(projectId, out ImmutableHashSet<ProjectId> value))
		{
			using PooledObject<HashSet<ProjectId>> pooledObject = SharedPools.Default<HashSet<ProjectId>>().GetPooledObject();
			HashSet<ProjectId> @object = pooledObject.Object;
			ComputeTransitiveReferences(projectId, @object);
			value = @object.ToImmutableHashSet();
			transitiveReferencesMap = transitiveReferencesMap.Add(projectId, value);
		}
		return value;
	}

	private void ComputeTransitiveReferences(ProjectId project, HashSet<ProjectId> result)
	{
		foreach (ProjectId item in GetProjectsThatThisProjectDirectlyDependsOn(project))
		{
			if (result.Add(item))
			{
				ComputeTransitiveReferences(item, result);
			}
		}
	}

	public IEnumerable<ProjectId> GetProjectsThatTransitivelyDependOnThisProject(ProjectId projectId)
	{
		if (projectId == null)
		{
			throw new ArgumentNullException("projectId");
		}
		if (reverseTransitiveReferencesMap.TryGetValue(projectId, out ImmutableHashSet<ProjectId> value))
		{
			return value;
		}
		using (dataLock.DisposableWait())
		{
			return GetProjectsThatTransitivelyDependOnThisProject_NoLock(projectId);
		}
	}

	public IList<ProjectId> GetProjectsThatTransitivelyDependOnThisProjectTopologicallySorted(ProjectId projectId)
	{
		IEnumerable<ProjectId> projectsThatTransitivelyDependOnThisProject = GetProjectsThatTransitivelyDependOnThisProject(projectId);
		return SortDependenciesTopologically(projectsThatTransitivelyDependOnThisProject);
	}

	private ImmutableHashSet<ProjectId> GetProjectsThatTransitivelyDependOnThisProject_NoLock(ProjectId projectId)
	{
		if (!reverseTransitiveReferencesMap.TryGetValue(projectId, out ImmutableHashSet<ProjectId> value))
		{
			using PooledObject<HashSet<ProjectId>> pooledObject = SharedPools.Default<HashSet<ProjectId>>().GetPooledObject();
			HashSet<ProjectId> @object = pooledObject.Object;
			ComputeReverseTransitiveReferences(projectId, @object);
			value = @object.ToImmutableHashSet();
			reverseTransitiveReferencesMap = reverseTransitiveReferencesMap.Add(projectId, value);
		}
		return value;
	}

	private void ComputeReverseTransitiveReferences(ProjectId project, HashSet<ProjectId> results)
	{
		foreach (ProjectId item in GetProjectsThatDirectlyDependOnThisProject_NoLock(project))
		{
			if (results.Add(item))
			{
				ComputeReverseTransitiveReferences(item, results);
			}
		}
	}

	public IEnumerable<ProjectId> GetTopologicallySortedProjects(CancellationToken cancellationToken = default(CancellationToken))
	{
		if (lazyTopologicallySortedProjects == null)
		{
			using (dataLock.DisposableWait(cancellationToken))
			{
				GetTopologicallySortedProjects_NoLock(cancellationToken);
			}
		}
		return lazyTopologicallySortedProjects;
	}

	private IEnumerable<ProjectId> GetTopologicallySortedProjects_NoLock(CancellationToken cancellationToken)
	{
		if (lazyTopologicallySortedProjects == null)
		{
			using PooledObject<HashSet<ProjectId>> pooledObject = SharedPools.Default<HashSet<ProjectId>>().GetPooledObject();
			using PooledObject<List<ProjectId>> pooledObject2 = SharedPools.Default<List<ProjectId>>().GetPooledObject();
			TopologicalSort(projectIds, pooledObject.Object, pooledObject2.Object, cancellationToken);
			lazyTopologicallySortedProjects = pooledObject2.Object.ToImmutableArray();
		}
		return lazyTopologicallySortedProjects;
	}

	private void TopologicalSort(IEnumerable<ProjectId> projectIdsToSort, HashSet<ProjectId> seenProjects, List<ProjectId> resultList, CancellationToken cancellationToken)
	{
		foreach (ProjectId item in projectIdsToSort)
		{
			cancellationToken.ThrowIfCancellationRequested();
			if (seenProjects.Add(item))
			{
				if (referencesMap.TryGetValue(item, out ImmutableHashSet<ProjectId> value))
				{
					TopologicalSort(value, seenProjects, resultList, cancellationToken);
				}
				resultList.Add(item);
			}
		}
	}

	public IEnumerable<IEnumerable<ProjectId>> GetDependencySets(CancellationToken cancellationToken = default(CancellationToken))
	{
		if (lazyDependencySets == null)
		{
			using (dataLock.DisposableWait(cancellationToken))
			{
				GetDependencySets_NoLock(cancellationToken);
			}
		}
		return lazyDependencySets;
	}

	private IEnumerable<IEnumerable<ProjectId>> GetDependencySets_NoLock(CancellationToken cancellationToken)
	{
		if (lazyDependencySets == null)
		{
			using PooledObject<HashSet<ProjectId>> pooledObject = SharedPools.Default<HashSet<ProjectId>>().GetPooledObject();
			using PooledObject<List<IEnumerable<ProjectId>>> pooledObject2 = SharedPools.Default<List<IEnumerable<ProjectId>>>().GetPooledObject();
			ComputeDependencySets(pooledObject.Object, pooledObject2.Object, cancellationToken);
			lazyDependencySets = pooledObject2.Object.ToImmutableArray();
		}
		return lazyDependencySets;
	}

	private void ComputeDependencySets(HashSet<ProjectId> seenProjects, List<IEnumerable<ProjectId>> results, CancellationToken cancellationToken)
	{
		ImmutableArray<ProjectId>.Enumerator enumerator = projectIds.GetEnumerator();
		while (enumerator.MoveNext())
		{
			ProjectId current = enumerator.Current;
			if (!seenProjects.Add(current))
			{
				continue;
			}
			using PooledObject<HashSet<ProjectId>> pooledObject = SharedPools.Default<HashSet<ProjectId>>().GetPooledObject();
			ComputedDependencySet(current, pooledObject.Object);
			seenProjects.UnionWith(pooledObject.Object);
			using PooledObject<HashSet<ProjectId>> pooledObject2 = SharedPools.Default<HashSet<ProjectId>>().GetPooledObject();
			using PooledObject<List<ProjectId>> pooledObject3 = SharedPools.Default<List<ProjectId>>().GetPooledObject();
			TopologicalSort(pooledObject.Object, pooledObject2.Object, pooledObject3.Object, cancellationToken);
			results.Add(pooledObject3.Object.ToImmutableArrayOrEmpty());
		}
	}

	private void ComputedDependencySet(ProjectId project, HashSet<ProjectId> result)
	{
		if (!result.Add(project))
		{
			return;
		}
		foreach (ProjectId item in GetProjectsThatDirectlyDependOnThisProject_NoLock(project).Concat(GetProjectsThatThisProjectDirectlyDependsOn(project)))
		{
			ComputedDependencySet(item, result);
		}
	}
}
