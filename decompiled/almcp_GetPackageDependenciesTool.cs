using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Dynamics.BusinessCentral.ALMcp.Services;
using Microsoft.Dynamics.Nav.CodeAnalysis;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.LanguageModelTools.Dependencies;
using Microsoft.Extensions.DependencyInjection;
using ModelContextProtocol.Server;

namespace Microsoft.Dynamics.BusinessCentral.ALMcp.Tools;

[McpServerToolType]
public class GetPackageDependenciesTool
{
	private const string ToolName = "al_getpackagedependencies";

	[McpServerTool(Name = "al_getpackagedependencies")]
	[Description("Gets the package dependencies defined in an AL project's app.json. Parameters: projectPath specifies the AL project folder (absolute path containing app.json). name optionally filters to a specific dependency by app name. Returns: List of dependencies with name, publisher, appId, and version range. Includes Base Application, System Application, and referenced extensions. Use cases: Understand project dependencies, verify versions, troubleshoot missing symbols. Next steps: Run al_downloadsymbols to download listed dependencies.")]
	public static async Task<GetDependencyResult> GetPackageDependenciesAsync(McpServer server, [Description("Optional module/app name to get dependencies for. If not specified, uses the default project.")] string? name = null, [Description("Optional AL project folder path to get dependencies from.")] string? projectPath = null, CancellationToken cancellationToken = default(CancellationToken))
	{
		string name2 = name;
		string projectPath2 = projectPath;
		if (server == null)
		{
			throw new ArgumentNullException("server");
		}
		McpTelemetryService.TrackToolInvocation("al_getpackagedependencies");
		CompilationService compilationService = server.Services?.GetService<CompilationService>();
		if (compilationService == null)
		{
			return new GetDependencyResult
			{
				Succeeded = false,
				ErrorMessage = "Compilation service is not available."
			};
		}
		McpTelemetryService telemetry = server.Services?.GetService<McpTelemetryService>();
		return await telemetry.TrackIfAvailableAsync("al_getpackagedependencies", () => GetDependenciesFromSolutionAsync(compilationService.Workspace, name2, projectPath2, cancellationToken)).ConfigureAwait(continueOnCapturedContext: false);
	}

	private static async Task<GetDependencyResult> GetDependenciesFromSolutionAsync(Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Workspace workspace, string? name, string? projectPath, CancellationToken cancellationToken)
	{
		string name2 = name;
		Solution currentSolution = workspace.CurrentSolution;
		if (currentSolution == null)
		{
			return new GetDependencyResult
			{
				Succeeded = false,
				ErrorMessage = "No solution loaded in workspace."
			};
		}
		Project project = (string.IsNullOrEmpty(projectPath) ? currentSolution.Projects.FirstOrDefault() : currentSolution.FindProject(projectPath));
		if (project == null)
		{
			return new GetDependencyResult
			{
				Succeeded = false,
				ErrorMessage = (string.IsNullOrEmpty(projectPath) ? "No project found in workspace." : ("Project not found: " + projectPath))
			};
		}
		Compilation compilation = await currentSolution.GetCompilationAsync(project.Id, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		if (compilation == null)
		{
			return new GetDependencyResult
			{
				Succeeded = false,
				ErrorMessage = "Failed to get compilation for project."
			};
		}
		IModuleSymbol moduleSymbol = ((!string.IsNullOrEmpty(name2) && !(name2 == project.Name)) ? compilation.ReferenceManager?.GetLoadedModules().FirstOrDefault((IModuleSymbol m) => m.Name == name2) : compilation.CompiledModule);
		if (moduleSymbol == null)
		{
			return new GetDependencyResult
			{
				Succeeded = false,
				ErrorMessage = (string.IsNullOrEmpty(name2) ? "No module found for project." : ("Module not found: " + name2))
			};
		}
		IEnumerable<IModuleSymbol> enumerable = moduleSymbol.ReferencedModules ?? Array.Empty<IModuleSymbol>();
		List<DependencyInfo> list = new List<DependencyInfo>();
		foreach (IModuleSymbol item in enumerable)
		{
			if (item != null)
			{
				list.Add(new DependencyInfo
				{
					Name = (item.Name ?? string.Empty),
					Publisher = item.Publisher,
					Id = item.AppId.ToString(),
					Version = item.Version?.ToString()
				});
			}
		}
		return new GetDependencyResult
		{
			Succeeded = true,
			ModuleName = moduleSymbol.Name,
			Dependencies = list
		};
	}
}
Latest version is '10.0.0.8330' (yours is '8.2.0.7535-95108c96')
