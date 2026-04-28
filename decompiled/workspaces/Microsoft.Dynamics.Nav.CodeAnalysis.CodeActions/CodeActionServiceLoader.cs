using System;
using System.Collections.Generic;
using System.Composition;
using System.Composition.Hosting;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Dynamics.Nav.CodeAnalysis.CodeActions.Mef;
using Microsoft.Dynamics.Nav.CodeAnalysis.CodeFixes;
using Microsoft.Dynamics.Nav.CodeAnalysis.CodeRefactoring;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.CodeActions;

internal class CodeActionServiceLoader
{
	private readonly string[] defaultAssemblyNames = new string[1] { "Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces" };

	private readonly string[] analyzerAssemblyNames = new string[3] { "Microsoft.Dynamics.Nav.CodeCop", "Microsoft.Dynamics.Nav.UICop", "Microsoft.Dynamics.Nav.Analyzers.Common" };

	private readonly string analyzersFolderPath;

	private readonly CompositionContext compositionContext;

	public CodeActionServiceLoader()
	{
		HashSet<Assembly> hashSet = new HashSet<Assembly>();
		analyzersFolderPath = CompilerPathUtilities.GetPathToAnalyzerFolder();
		LoadDefaultAssemblies(hashSet);
		LoadAnalyzerAssemblies(hashSet);
		LoadOtherAnalyzerAssemblies(hashSet);
		ContainerConfiguration containerConfiguration = new ContainerConfiguration().WithAssemblies(hashSet);
		compositionContext = containerConfiguration.CreateContainer();
	}

	public CodeActionService CreateCodeActionService()
	{
		CodeFixService codeFixService = new CodeFixService(TryGetExports<CodeFixProvider, CodeActionProviderMetadata>()?.Select((Lazy<CodeFixProvider, CodeActionProviderMetadata> export) => export.Value));
		CodeRefactoringService codeRefactoringService = new CodeRefactoringService(TryGetExports<CodeRefactoringProvider, CodeActionProviderMetadata>()?.Select((Lazy<CodeRefactoringProvider, CodeActionProviderMetadata> export) => export.Value));
		return new ALCodeActionService(codeFixService, codeRefactoringService);
	}

	private void LoadDefaultAssemblies(HashSet<Assembly> builder)
	{
		LoadAssemblies(defaultAssemblyNames, builder, TryAssemblyLoad);
	}

	private void LoadAnalyzerAssemblies(HashSet<Assembly> builder)
	{
		LoadAssemblies(analyzerAssemblyNames, builder, TryAssemblyLoadFrom);
	}

	private void LoadOtherAnalyzerAssemblies(HashSet<Assembly> builder)
	{
		IReadOnlyList<string> assemblyNames = (from t in Directory.GetFiles(analyzersFolderPath, "*.dll")
			select Path.GetFileNameWithoutExtension(t) into t
			where !t.StartsWith("Microsoft.") && !t.StartsWith("System.")
			select t).ToImmutableReadOnlyListOrEmpty();
		LoadAssemblies(assemblyNames, builder, TryAssemblyLoadFrom);
	}

	private void LoadAssemblies(IEnumerable<string> assemblyNames, HashSet<Assembly> builder, Func<string, Assembly?> load)
	{
		foreach (string assemblyName in assemblyNames)
		{
			Assembly assembly = load(assemblyName);
			if (assembly != null)
			{
				builder.Add(assembly);
			}
		}
	}

	private Assembly TryAssemblyLoad(string assemblySimpleName)
	{
		AssemblyName name = GetType().GetTypeInfo().Assembly.GetName();
		_ = name.Name;
		Version version = name.Version;
		string text = name.GetPublicKeyToken().Aggregate(string.Empty, (string s, byte b) => s + b.ToString("x2", CultureInfo.InvariantCulture));
		if (string.IsNullOrEmpty(text))
		{
			text = "null";
		}
		AssemblyName assemblyRef = new AssemblyName(string.Format(CultureInfo.InvariantCulture, "{0}, Version={1}, Culture=neutral, PublicKeyToken={2}", assemblySimpleName, version, text));
		try
		{
			return Assembly.Load(assemblyRef);
		}
		catch (Exception)
		{
			return null;
		}
	}

	private Assembly? TryAssemblyLoadFrom(string assemblySimpleName)
	{
		string text = Path.Combine(analyzersFolderPath, assemblySimpleName) + ".dll";
		if (!File.Exists(text))
		{
			return null;
		}
		try
		{
			return Assembly.LoadFrom(text);
		}
		catch (Exception)
		{
			return null;
		}
	}

	private IEnumerable<Lazy<TExtension, TMetadata>> TryGetExports<TExtension, TMetadata>()
	{
		try
		{
			return compositionContext.GetExports<Lazy<TExtension, TMetadata>>();
		}
		catch (Exception)
		{
			return null;
		}
	}
}
