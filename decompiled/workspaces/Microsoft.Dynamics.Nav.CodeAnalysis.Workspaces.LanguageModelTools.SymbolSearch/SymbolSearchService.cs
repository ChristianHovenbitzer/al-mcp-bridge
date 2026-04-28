using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.LanguageModelTools.SymbolSearch;

public sealed class SymbolSearchService : IDisposable
{
	private sealed class ProjectSymbolSnapshot
	{
		public VersionStamp Version { get; }

		public ImmutableArray<SymbolDescriptor> Symbols { get; }

		public ProjectSymbolSnapshot(VersionStamp version, ImmutableArray<SymbolDescriptor> symbols)
		{
			Version = version;
			Symbols = symbols;
		}
	}

	private sealed class SymbolSearchFilterSet
	{
		private readonly HashSet<SymbolKind> objectKinds;

		private readonly HashSet<SymbolKind> memberKinds;

		private readonly HashSet<string> accessModifiers;

		private readonly HashSet<string> obsoleteStates;

		private readonly string? namespaceFilter;

		private readonly string? objectNameFilter;

		private readonly string query;

		private readonly bool hasQuery;

		private readonly SymbolMatchMode matchMode;

		private readonly bool includeMembers;

		private readonly bool membersOnly;

		public bool IncludeProjectSymbols { get; }

		public bool IncludeDependencySymbols { get; }

		public int Limit { get; }

		private SymbolSearchFilterSet(HashSet<SymbolKind> objectKinds, HashSet<SymbolKind> memberKinds, HashSet<string> accessModifiers, HashSet<string> obsoleteStates, string? namespaceFilter, string? objectNameFilter, string query, SymbolMatchMode matchMode, bool includeProjectSymbols, bool includeDependencySymbols, int limit, bool includeMembers, bool membersOnly)
		{
			this.objectKinds = objectKinds;
			this.memberKinds = memberKinds;
			this.accessModifiers = accessModifiers;
			this.obsoleteStates = obsoleteStates;
			this.namespaceFilter = namespaceFilter;
			this.objectNameFilter = objectNameFilter;
			this.query = query;
			hasQuery = !string.IsNullOrEmpty(query);
			this.matchMode = matchMode;
			IncludeProjectSymbols = includeProjectSymbols;
			IncludeDependencySymbols = includeDependencySymbols;
			Limit = limit;
			this.includeMembers = includeMembers;
			this.membersOnly = membersOnly;
		}

		public static SymbolSearchFilterSet Create(SymbolSearchParameters parameters, int defaultLimit)
		{
			SymbolSearchFilters filters = parameters.Filters;
			HashSet<SymbolKind> hashSet = ParseSymbolKinds(filters?.Kinds);
			HashSet<SymbolKind> hashSet2 = ParseSymbolKinds(filters?.MemberKinds);
			HashSet<string> hashSet3 = ParseStringSet(filters?.Access);
			HashSet<string> hashSet4 = ParseStringSet(filters?.ObsoleteState);
			string text = (string.IsNullOrWhiteSpace(filters?.Namespace) ? null : filters.Namespace.Trim());
			string value = (string.IsNullOrWhiteSpace(filters?.ObjectName) ? null : filters.ObjectName.Trim());
			string text2 = parameters.Query?.Trim() ?? string.Empty;
			if (text2 == "*")
			{
				text2 = string.Empty;
			}
			SymbolMatchMode symbolMatchMode = ParseMatchMode(filters?.Match);
			(bool includeProject, bool includeDependencies) tuple = ParseScope(filters?.Scope);
			bool item = tuple.includeProject;
			bool item2 = tuple.includeDependencies;
			int? num = filters?.Limit;
			int limit = ((num.HasValue && num.GetValueOrDefault() > 0) ? Math.Min(filters.Limit.Value, 1000) : defaultLimit);
			bool flag = hashSet2.Count > 0 || !string.IsNullOrEmpty(value);
			bool flag2 = flag && hashSet.Count == 0;
			return new SymbolSearchFilterSet(hashSet, hashSet2, hashSet3, hashSet4, text, value, text2, symbolMatchMode, item, item2, limit, flag, flag2);
		}

		public bool IsSatisfied(int currentCount)
		{
			return currentCount >= Limit;
		}

		public bool Match(SymbolDescriptor descriptor)
		{
			if (descriptor.IsTopLevel)
			{
				if (membersOnly)
				{
					return false;
				}
				if (objectKinds.Count > 0 && !objectKinds.Contains(descriptor.SymbolKind))
				{
					return false;
				}
			}
			else
			{
				if (!includeMembers)
				{
					return false;
				}
				if (memberKinds.Count > 0 && !memberKinds.Contains(descriptor.SymbolKind))
				{
					return false;
				}
			}
			if (!string.IsNullOrEmpty(namespaceFilter) && !StringComparer.OrdinalIgnoreCase.Equals(namespaceFilter, descriptor.Namespace))
			{
				return false;
			}
			if (!string.IsNullOrEmpty(objectNameFilter) && !StringComparer.OrdinalIgnoreCase.Equals(objectNameFilter, descriptor.ContainerObjectName ?? descriptor.Name))
			{
				return false;
			}
			if (accessModifiers.Count > 0 && !accessModifiers.Contains(descriptor.Accessibility.ToString()))
			{
				return false;
			}
			if (obsoleteStates.Count > 0 && !MatchObsoleteState(descriptor))
			{
				return false;
			}
			if (hasQuery && !MatchQuery(descriptor))
			{
				return false;
			}
			return true;
		}

		private bool MatchObsoleteState(SymbolDescriptor descriptor)
		{
			using (HashSet<string>.Enumerator enumerator = obsoleteStates.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					switch (enumerator.Current)
					{
					case "PENDING":
						if (descriptor.IsObsoletePending)
						{
							return true;
						}
						break;
					case "OBSOLETE":
						if (descriptor.IsObsoletePending || descriptor.IsObsoleteRemoved)
						{
							return true;
						}
						break;
					case "REMOVED":
						if (descriptor.IsObsoleteRemoved)
						{
							return true;
						}
						break;
					case "PENDINGMOVE":
						if (descriptor.IsObsoletePendingMove)
						{
							return true;
						}
						break;
					case "MOVED":
						if (descriptor.IsObsoleteMoved)
						{
							return true;
						}
						break;
					}
				}
			}
			return false;
		}

		private bool MatchQuery(SymbolDescriptor descriptor)
		{
			StringComparison comparisonType = StringComparison.OrdinalIgnoreCase;
			bool flag = descriptor.Name.Contains(query, comparisonType) || (!string.IsNullOrEmpty(descriptor.FullName) && descriptor.FullName.Contains(query, comparisonType)) || (!string.IsNullOrEmpty(descriptor.ContainerObjectName) && descriptor.ContainerObjectName.Contains(query, comparisonType));
			bool flag2 = !string.IsNullOrEmpty(descriptor.DocSummary) && descriptor.DocSummary.IndexOf(query, comparisonType) >= 0;
			return matchMode switch
			{
				SymbolMatchMode.Name => flag, 
				SymbolMatchMode.Documentation => flag2, 
				SymbolMatchMode.All => flag || flag2, 
				_ => flag, 
			};
		}

		private static HashSet<SymbolKind> ParseSymbolKinds(IEnumerable<string>? values)
		{
			HashSet<SymbolKind> hashSet = new HashSet<SymbolKind>();
			if (values == null)
			{
				return hashSet;
			}
			foreach (string value in values)
			{
				SymbolKind result;
				if (SymbolKindExtensions.TryGetEnumObjectTypeSymbolKind(value, out var symbolKind))
				{
					hashSet.Add(symbolKind);
				}
				else if (Enum.TryParse<SymbolKind>(value, ignoreCase: true, out result))
				{
					hashSet.Add(result);
				}
			}
			return hashSet;
		}

		private static HashSet<string> ParseStringSet(IEnumerable<string>? values)
		{
			HashSet<string> hashSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
			if (values == null)
			{
				return hashSet;
			}
			foreach (string value in values)
			{
				if (!string.IsNullOrWhiteSpace(value))
				{
					hashSet.Add(value.Trim());
				}
			}
			return hashSet;
		}

		private static SymbolMatchMode ParseMatchMode(string? match)
		{
			string text = match?.ToLowerInvariant();
			if (!(text == "doc"))
			{
				if (text == "all")
				{
					return SymbolMatchMode.All;
				}
				return SymbolMatchMode.Name;
			}
			return SymbolMatchMode.Documentation;
		}

		private static (bool includeProject, bool includeDependencies) ParseScope(string? scope)
		{
			string text = scope?.ToLowerInvariant();
			if (!(text == "project"))
			{
				if (text == "dependencies")
				{
					return (includeProject: false, includeDependencies: true);
				}
				return (includeProject: true, includeDependencies: true);
			}
			return (includeProject: true, includeDependencies: false);
		}
	}

	private sealed class SymbolDescriptor
	{
		public string Id { get; }

		public string Name { get; }

		public string FullName { get; }

		public string KindDisplay { get; }

		public SymbolKind SymbolKind { get; }

		public string? Namespace { get; }

		public string? ContainerName { get; }

		public string? ContainerObjectName { get; }

		public string? Signature { get; }

		public string? DocSummary { get; }

		public string? Path { get; }

		public SymbolSearchScope Scope { get; }

		public bool IsTopLevel { get; }

		public Accessibility Accessibility { get; }

		public bool IsObsoletePending { get; }

		public bool IsObsoleteRemoved { get; }

		public bool IsObsoleteMoved { get; }

		public bool IsObsoletePendingMove { get; }

		public SymbolDescriptor(string id, string name, string fullName, string kindDisplay, SymbolKind symbolKind, string? namespaceName, string? containerName, string? containerObjectName, string? signature, string? docSummary, string? path, SymbolSearchScope scope, bool isTopLevel, Accessibility accessibility, bool isObsoletePending, bool isObsoleteRemoved, bool isObsoleteMoved, bool isObsoletePendingMove)
		{
			Id = id;
			Name = name;
			FullName = fullName;
			KindDisplay = kindDisplay;
			SymbolKind = symbolKind;
			Namespace = namespaceName;
			ContainerName = containerName;
			ContainerObjectName = containerObjectName;
			Signature = signature;
			DocSummary = docSummary;
			Path = path;
			Scope = scope;
			IsTopLevel = isTopLevel;
			Accessibility = accessibility;
			IsObsoletePending = isObsoletePending;
			IsObsoleteRemoved = isObsoleteRemoved;
			IsObsoleteMoved = isObsoleteMoved;
			IsObsoletePendingMove = isObsoletePendingMove;
		}

		public SymbolInfo ToSymbolInfo()
		{
			return new SymbolInfo
			{
				Id = Id,
				Name = Name,
				FullName = FullName,
				Kind = KindDisplay,
				Namespace = Namespace,
				ContainerName = (ContainerObjectName ?? ContainerName),
				Signature = Signature,
				DocSummary = DocSummary,
				Path = Path
			};
		}

		public static SymbolDescriptor Create(Symbol symbol, Symbol? containerSymbol, SymbolSearchScope scope, bool isTopLevel, CancellationToken cancellationToken)
		{
			string @namespace = GetNamespace(symbol);
			string fullName = (string.IsNullOrEmpty(@namespace) ? symbol.Name : (@namespace + '.' + symbol.Name));
			string docSummary = symbol.GetDocumentationComment(cancellationToken)?.Trim();
			string signature = symbol.ToDisplayString(SymbolDisplayFormat.SignatureFormat);
			string path = TryGetFilePath(symbol);
			return new SymbolDescriptor(SymbolId.CreateId(symbol), symbol.Name, fullName, symbol.Kind.ToString(), symbol.Kind, @namespace, symbol.ContainingSymbol?.Name, containerSymbol?.Name, signature, docSummary, path, scope, isTopLevel, symbol.DeclaredAccessibility, symbol.IsObsoletePending, symbol.IsObsoleteRemoved, symbol.IsObsoleteMoved, symbol.IsObsoletePendingMove);
		}

		private static string? GetNamespace(Symbol symbol)
		{
			NamespaceSymbol containingNamespace = symbol.ContainingNamespace;
			if (containingNamespace == null || containingNamespace.IsGlobalNamespace)
			{
				return null;
			}
			return containingNamespace.QualifiedName ?? containingNamespace.ToDisplayString();
		}

		private static string? TryGetFilePath(Symbol symbol)
		{
			Location location = symbol.Location;
			if (location != null && location.IsInSource && !string.IsNullOrEmpty(location.SourceTree?.FilePath))
			{
				return location.SourceTree.FilePath;
			}
			return null;
		}
	}

	private enum SymbolMatchMode
	{
		Name,
		Documentation,
		All
	}

	private enum SymbolSearchScope
	{
		Project,
		Dependencies
	}

	private const int DefaultLimit = 200;

	private const string NoCompilationAvailableMessage = "No compilation available.";

	private const string TruncatedResultsMessageFormat = "Truncated to first {0} results.";

	private const string FoundSymbolsMessageFormat = "Found {0} symbols.";

	private readonly Workspace workspace;

	private readonly ConcurrentDictionary<ProjectId, ProjectSymbolSnapshot> projectCache = new ConcurrentDictionary<ProjectId, ProjectSymbolSnapshot>();

	private readonly ConcurrentDictionary<string, ImmutableArray<SymbolDescriptor>> dependencyCache = new ConcurrentDictionary<string, ImmutableArray<SymbolDescriptor>>(StringComparer.OrdinalIgnoreCase);

	private bool disposed;

	public SymbolSearchService(Workspace workspace)
	{
		this.workspace = workspace ?? throw new ArgumentNullException("workspace");
	}

	public async Task<SymbolSearchResult> SearchAsync(ProjectId projectId, SymbolSearchParameters parameters, CancellationToken cancellationToken)
	{
		ThrowIfDisposed();
		ArgumentNullException.ThrowIfNull(projectId, "projectId");
		ArgumentNullException.ThrowIfNull(parameters, "parameters");
		Solution solution = workspace.CurrentSolution ?? throw new InvalidOperationException("Workspace does not have an active solution.");
		Project project = solution.GetProject(projectId) ?? throw new ArgumentException("Project not found.", "projectId");
		SymbolSearchFilterSet filterSet = SymbolSearchFilterSet.Create(parameters, 200);
		Compilation compilation = await project.GetCompilationAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		if (compilation == null)
		{
			return new SymbolSearchResult
			{
				Succeeded = true,
				Message = "No compilation available.",
				Symbols = Array.Empty<SymbolInfo>()
			};
		}
		List<SymbolInfo> results = new List<SymbolInfo>(Math.Min(filterSet.Limit, 200));
		bool truncated = false;
		if (filterSet.IncludeProjectSymbols)
		{
			AppendMatches(await GetProjectDescriptorsAsync(project, compilation, cancellationToken).ConfigureAwait(continueOnCapturedContext: false), filterSet, results, ref truncated);
		}
		if (!truncated && filterSet.IncludeDependencySymbols)
		{
			AppendMatches(await GetDependencyDescriptorsAsync(compilation, cancellationToken).ConfigureAwait(continueOnCapturedContext: false), filterSet, results, ref truncated);
		}
		return new SymbolSearchResult
		{
			Succeeded = true,
			Message = (truncated ? $"Truncated to first {filterSet.Limit} results." : $"Found {results.Count} symbols."),
			Symbols = results
		};
	}

	public void Dispose()
	{
		if (!disposed)
		{
			projectCache.Clear();
			dependencyCache.Clear();
			disposed = true;
		}
	}

	private static void AppendMatches(ImmutableArray<SymbolDescriptor> descriptors, SymbolSearchFilterSet filterSet, List<SymbolInfo> results, ref bool truncated)
	{
		ImmutableArray<SymbolDescriptor>.Enumerator enumerator = descriptors.GetEnumerator();
		while (enumerator.MoveNext())
		{
			SymbolDescriptor current = enumerator.Current;
			if (filterSet.Match(current))
			{
				results.Add(current.ToSymbolInfo());
				if (filterSet.IsSatisfied(results.Count))
				{
					truncated = true;
					break;
				}
			}
		}
	}

	private async Task<ImmutableArray<SymbolDescriptor>> GetProjectDescriptorsAsync(Project project, Compilation compilation, CancellationToken cancellationToken)
	{
		if (projectCache.TryGetValue(project.Id, out ProjectSymbolSnapshot value) && value.Version == project.Version)
		{
			return value.Symbols;
		}
		ImmutableArray<SymbolDescriptor> immutableArray = BuildDescriptors(compilation.CompiledModule.GetDeclaredObjectSymbols(), SymbolSearchScope.Project, cancellationToken);
		value = new ProjectSymbolSnapshot(project.Version, immutableArray);
		projectCache[project.Id] = value;
		return immutableArray;
	}

	private Task<ImmutableArray<SymbolDescriptor>> GetDependencyDescriptorsAsync(Compilation compilation, CancellationToken cancellationToken)
	{
		ImmutableArray<IModuleSymbol> referenceModules = compilation.CompiledModule.ReferenceModules;
		if (referenceModules.IsDefaultOrEmpty)
		{
			return Task.FromResult(ImmutableArray<SymbolDescriptor>.Empty);
		}
		ImmutableArray<SymbolDescriptor>.Builder builder = ImmutableArray.CreateBuilder<SymbolDescriptor>();
		ImmutableArray<IModuleSymbol>.Enumerator enumerator = referenceModules.GetEnumerator();
		while (enumerator.MoveNext())
		{
			IModuleSymbol current = enumerator.Current;
			cancellationToken.ThrowIfCancellationRequested();
			if (current != null)
			{
				string key = BuildDependencyCacheKey(current);
				if (!dependencyCache.TryGetValue(key, out ImmutableArray<SymbolDescriptor> value))
				{
					value = BuildDescriptors(current.GetObjectSymbols(), SymbolSearchScope.Dependencies, cancellationToken);
					dependencyCache[key] = value;
				}
				builder.AddRange(value);
			}
		}
		return Task.FromResult(builder.ToImmutable());
	}

	private static string BuildDependencyCacheKey(IModuleSymbol module)
	{
		return module.AppId.ToString("D") + ':' + module.Version;
	}

	private static ImmutableArray<SymbolDescriptor> BuildDescriptors(IEnumerable<ISymbol> topLevelSymbols, SymbolSearchScope scope, CancellationToken cancellationToken)
	{
		ImmutableArray<SymbolDescriptor>.Builder builder = ImmutableArray.CreateBuilder<SymbolDescriptor>();
		foreach (ISymbol topLevelSymbol in topLevelSymbols)
		{
			cancellationToken.ThrowIfCancellationRequested();
			if (topLevelSymbol is Symbol symbol)
			{
				builder.Add(SymbolDescriptor.Create(symbol, null, scope, isTopLevel: true, cancellationToken));
				if (topLevelSymbol is ContainerSymbol container)
				{
					AppendMembers(builder, container, symbol, scope, cancellationToken);
				}
			}
		}
		return builder.ToImmutable();
	}

	private static void AppendMembers(ImmutableArray<SymbolDescriptor>.Builder builder, ContainerSymbol container, Symbol rootContainer, SymbolSearchScope scope, CancellationToken cancellationToken)
	{
		ImmutableArray<Symbol>.Enumerator enumerator = container.GetMembers().GetEnumerator();
		while (enumerator.MoveNext())
		{
			Symbol current = enumerator.Current;
			cancellationToken.ThrowIfCancellationRequested();
			if (current.Kind != SymbolKind.Namespace)
			{
				builder.Add(SymbolDescriptor.Create(current, rootContainer, scope, isTopLevel: false, cancellationToken));
				if (current is ContainerSymbol container2)
				{
					AppendMembers(builder, container2, rootContainer, scope, cancellationToken);
				}
			}
		}
	}

	private void ThrowIfDisposed()
	{
		if (disposed)
		{
			throw new ObjectDisposedException("SymbolSearchService");
		}
	}
}
