using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Resources;
using System.Runtime.CompilerServices;
using System.Runtime.Versioning;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.Connections.Client;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Dynamics.Nav.CodeAnalysis;
using Microsoft.Dynamics.Nav.CodeAnalysis.CommandLine;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.SymbolReference;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces;
using Microsoft.Dynamics.Nav.Deployment;
using Microsoft.Dynamics.Nav.Deployment.ApiClients;
using Microsoft.Dynamics.Nav.Deployment.Authentication;
using Microsoft.Dynamics.Nav.Deployment.Http;
using Microsoft.Dynamics.Nav.Deployment.Publishing;
using Microsoft.Dynamics.Nav.Deployment.ReferenceDownloader;
using Microsoft.Dynamics.Nav.LanguageModelTools.Build;
using Microsoft.Dynamics.Nav.LanguageModelTools.ErrorHandling;
using Microsoft.Dynamics.Nav.LanguageModelTools.ServerConnection;
using Microsoft.Dynamics.Nav.LanguageModelTools.SignalR;
using Microsoft.Dynamics.Nav.LanguageModelTools.Workflow;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Serialization;

[assembly: CompilationRelaxations(8)]
[assembly: RuntimeCompatibility(WrapNonExceptionThrows = true)]
[assembly: Debuggable(DebuggableAttribute.DebuggingModes.IgnoreSymbolStoreSequencePoints)]
[assembly: TargetFramework(".NETCoreApp,Version=v8.0", FrameworkDisplayName = ".NET 8.0")]
[assembly: AssemblyCompany("Microsoft")]
[assembly: AssemblyConfiguration("Release")]
[assembly: AssemblyCopyright("¸ Microsoft Corporation. All rights reserved.")]
[assembly: AssemblyFileVersion("17.0.34.45391")]
[assembly: AssemblyInformationalVersion("17.0.34.45391+89ddc161d3e4421fa7ecef442abf29ca6e6ebfba")]
[assembly: AssemblyProduct("Microsoft.Dynamics.Nav.LanguageModelTools")]
[assembly: AssemblyTitle("Microsoft.Dynamics.Nav.LanguageModelTools")]
[assembly: InternalsVisibleTo("Test.almcp, PublicKey=0024000004800000940000000602000000240000525341310004000001000100b5fc90e7027f67871e773a8fde8938c81dd402ba65b9201d60593e96c492651e889cc13f1415ebb53fac1131ae0bd333c5ee6021672d9718ea31a8aebd0da0072f25d87dba6fc90ffd598ed4da35e44c398c454307e8e33b8426143daec9f596836f97c8f74750e5975c64e2189f45def46b2a2b1247adc3652bf5c308055da9")]
[assembly: AssemblyVersion("17.0.34.45391")]
[module: RefSafetyRules(11)]
namespace Microsoft.Dynamics.Nav.LanguageModelTools
{
	public static class AnalyzerReferenceResolver
	{
		public static (string Path, bool RequiresAnalyzersCommon) ReplaceWellKnownAnalyzerVariable(string analyzerPath)
		{
			string pathToAnalyzerFolder = CompilerPathUtilities.GetPathToAnalyzerFolder();
			string text = analyzerPath.ToLowerInvariant();
			switch (text)
			{
			case "${codecop}":
				return (Path: Path.Combine(pathToAnalyzerFolder, "Microsoft.Dynamics.Nav.CodeCop.dll"), RequiresAnalyzersCommon: true);
			case "${appsourcecop}":
				return (Path: Path.Combine(pathToAnalyzerFolder, "Microsoft.Dynamics.Nav.AppSourceCop.dll"), RequiresAnalyzersCommon: true);
			case "${pertenantextensioncop}":
				return (Path: Path.Combine(pathToAnalyzerFolder, "Microsoft.Dynamics.Nav.PerTenantExtensionCop.dll"), RequiresAnalyzersCommon: true);
			case "${uicop}":
				return (Path: Path.Combine(pathToAnalyzerFolder, "Microsoft.Dynamics.Nav.UICop.dll"), RequiresAnalyzersCommon: true);
			default:
			{
				if (text.IndexOf("${analyzerfolder}", StringComparison.Ordinal) != 0)
				{
					return (Path: analyzerPath, RequiresAnalyzersCommon: false);
				}
				string path = analyzerPath.Substring("${analyzerfolder}".Length).TrimStart(PathUtilities.DirectorySeparatorChar);
				return (Path: Path.Combine(pathToAnalyzerFolder, path), RequiresAnalyzersCommon: false);
			}
			}
		}

		public static ImmutableArray<AnalyzerReference> Resolve(string[] codeAnalyzers, string? projectPath)
		{
			ArrayBuilder<AnalyzerReference> instance = ArrayBuilder<AnalyzerReference>.GetInstance();
			DefaultAnalyzerAssemblyLoader assemblyLoader = new DefaultAnalyzerAssemblyLoader();
			bool flag = false;
			for (int i = 0; i < codeAnalyzers.Length; i++)
			{
				var (text, flag2) = ReplaceWellKnownAnalyzerVariable(codeAnalyzers[i]);
				if (!Path.IsPathRooted(text) && !string.IsNullOrEmpty(projectPath))
				{
					text = Path.GetFullPath(Path.Combine(projectPath, text));
				}
				flag = flag || flag2;
				if (File.Exists(text))
				{
					instance.Add(new AnalyzerFileReference(text, assemblyLoader));
				}
				else
				{
					instance.Add(new UnresolvedAnalyzerReference(text, string.Format(CultureInfo.InvariantCulture, "Analyzer not found: {0}", text)));
				}
			}
			if (flag)
			{
				string text2 = Path.Combine(CompilerPathUtilities.GetPathToAnalyzerFolder(), "Microsoft.Dynamics.Nav.Analyzers.Common.dll");
				if (File.Exists(text2))
				{
					instance.Add(new AnalyzerFileReference(text2, assemblyLoader));
				}
			}
			return instance.ToImmutableAndFree();
		}
	}
	internal static class ProjectHelper
	{
		public static Project? FindProject(Solution solution, string? projectPath)
		{
			string projectPath2 = projectPath;
			if (string.IsNullOrEmpty(projectPath2))
			{
				return solution.Projects.FirstOrDefault();
			}
			return solution.Projects.FirstOrDefault((Project p) => string.Equals(p.ProjectFolder, projectPath2, StringComparison.OrdinalIgnoreCase));
		}
	}
	public sealed class ToolErrorDetails
	{
		public string Code { get; set; } = string.Empty;


		public string Description { get; set; } = string.Empty;


		public IReadOnlyList<string> PossibleCauses { get; set; } = Array.Empty<string>();


		public IReadOnlyList<string> SuggestedActions { get; set; } = Array.Empty<string>();


		public IReadOnlyList<string> Alternatives { get; set; } = Array.Empty<string>();


		public IReadOnlyList<string> MissingPrerequisites { get; set; } = Array.Empty<string>();


		public IReadOnlyList<string> DiagnosticHints { get; set; } = Array.Empty<string>();


		public bool Retryable { get; set; }
	}
	public sealed class ToolResponse
	{
		public bool Succeeded { get; set; }

		public string? Message { get; set; }

		public object? Data { get; set; }

		public IReadOnlyList<string> NextSteps { get; set; } = Array.Empty<string>();


		public ToolErrorDetails? ErrorDetails { get; set; }

		public IReadOnlyList<string> Warnings { get; set; } = Array.Empty<string>();

	}
}
namespace Microsoft.Dynamics.Nav.LanguageModelTools.Workflow
{
	public sealed class WorkflowChain
	{
		public string CurrentTool { get; init; } = string.Empty;


		public IReadOnlyList<WorkflowStep> NextSteps { get; init; } = Array.Empty<WorkflowStep>();


		public IReadOnlyList<string> Suggestions { get; init; } = Array.Empty<string>();


		public string? WorkflowContext { get; init; }
	}
	public enum WorkflowCondition
	{
		Always,
		OnSuccess,
		OnFailure
	}
	public static class WorkflowEngine
	{
		public static class ToolNames
		{
			public const string Build = "al_build";

			public const string Compile = "al_compile";

			public const string GetDiagnostics = "al_getdiagnostics";

			public const string DownloadSymbols = "al_downloadsymbols";

			public const string Publish = "al_publish";

			public const string SetBreakpoint = "al_setbreakpoint";

			public const string Debug = "al_debug";

			public const string SnapshotDebugging = "al_snapshotdebugging";

			public const string SymbolSearch = "al_symbolsearch";

			public const string GetDependencies = "al_getdependencies";
		}

		public static class CommonSteps
		{
			public static readonly WorkflowStep BuildPackage = new WorkflowStep
			{
				Tool = "al_build",
				DisplayName = "Build Package",
				Reason = "Compile code to verify no errors and create deployment package",
				Priority = 10,
				Condition = WorkflowCondition.OnSuccess
			};

			public static readonly WorkflowStep RebuildPackage = new WorkflowStep
			{
				Tool = "al_build",
				DisplayName = "Rebuild Package",
				Reason = "Recompile after applying fixes to verify errors are resolved",
				Priority = 9,
				Condition = WorkflowCondition.Always
			};

			public static readonly WorkflowStep BuildAllProjects = new WorkflowStep
			{
				Tool = "al_build",
				DisplayName = "Build All Projects",
				Reason = "Compile all workspace projects including dependency tree",
				Priority = 10,
				Condition = WorkflowCondition.OnSuccess,
				Parameters = new Dictionary<string, object> { ["scope"] = "all" }
			};

			public static readonly WorkflowStep GetDiagnosticsErrors = new WorkflowStep
			{
				Tool = "al_getdiagnostics",
				DisplayName = "Get Errors",
				Reason = "Retrieve detailed error locations and messages",
				Priority = 10,
				Condition = WorkflowCondition.OnFailure,
				Parameters = new Dictionary<string, object> { ["severities"] = new string[1] { "error" } }
			};

			public static readonly WorkflowStep GetDiagnosticsWarnings = new WorkflowStep
			{
				Tool = "al_getdiagnostics",
				DisplayName = "Review Warnings",
				Reason = "Review warnings to improve code quality",
				Priority = 9,
				Condition = WorkflowCondition.OnSuccess,
				Parameters = new Dictionary<string, object> { ["severities"] = new string[1] { "warning" } }
			};

			public static readonly WorkflowStep DownloadSymbols = new WorkflowStep
			{
				Tool = "al_downloadsymbols",
				DisplayName = "Download Symbols",
				Reason = "Download symbols if errors indicate missing references",
				Priority = 9,
				Condition = WorkflowCondition.OnFailure
			};

			public static readonly WorkflowStep DownloadSymbolsHighPriority = new WorkflowStep
			{
				Tool = "al_downloadsymbols",
				DisplayName = "Download Symbols",
				Reason = "Download missing symbols to resolve compilation errors",
				Priority = 10,
				Condition = WorkflowCondition.OnFailure
			};

			public static readonly WorkflowStep Publish = new WorkflowStep
			{
				Tool = "al_publish",
				DisplayName = "Publish",
				Reason = "Deploy extension to Business Central server",
				Priority = 9,
				Condition = WorkflowCondition.OnSuccess
			};

			public static readonly WorkflowStep PublishWithDebug = new WorkflowStep
			{
				Tool = "al_publish",
				DisplayName = "Publish & Debug",
				Reason = "Deploy and start debugging session",
				Priority = 8,
				Condition = WorkflowCondition.OnSuccess,
				Parameters = new Dictionary<string, object> { ["debug"] = true }
			};

			public static readonly WorkflowStep SetBreakpoint = new WorkflowStep
			{
				Tool = "al_setbreakpoint",
				DisplayName = "Set Breakpoint",
				Reason = "Set debugging breakpoints in AL code",
				Priority = 10,
				Condition = WorkflowCondition.Always
			};

			public static readonly WorkflowStep AttachDebugger = new WorkflowStep
			{
				Tool = "al_debug",
				DisplayName = "Attach Debugger",
				Reason = "Start debugging session to step through code",
				Priority = 9,
				Condition = WorkflowCondition.OnSuccess
			};

			public static readonly WorkflowStep SnapshotInitialize = new WorkflowStep
			{
				Tool = "al_snapshotdebugging",
				DisplayName = "Start Snapshot Debug",
				Reason = "Initialize snapshot debugging for production troubleshooting",
				Priority = 10,
				Condition = WorkflowCondition.OnSuccess,
				Parameters = new Dictionary<string, object> { ["action"] = "initialize" }
			};

			public static readonly WorkflowStep SnapshotFinish = new WorkflowStep
			{
				Tool = "al_snapshotdebugging",
				DisplayName = "Finish Snapshot",
				Reason = "Complete snapshot capture after issue is reproduced",
				Priority = 10,
				Condition = WorkflowCondition.OnSuccess,
				Parameters = new Dictionary<string, object> { ["action"] = "finish" }
			};

			public static readonly WorkflowStep SnapshotView = new WorkflowStep
			{
				Tool = "al_snapshotdebugging",
				DisplayName = "View Snapshots",
				Reason = "Open snapshot viewer to analyze captured data",
				Priority = 10,
				Condition = WorkflowCondition.OnSuccess,
				Parameters = new Dictionary<string, object> { ["action"] = "view" }
			};

			public static readonly WorkflowStep SymbolSearch = new WorkflowStep
			{
				Tool = "al_symbolsearch",
				DisplayName = "Search Symbols",
				Reason = "Search for correct symbol names if references are unresolved",
				Priority = 8,
				Condition = WorkflowCondition.OnFailure
			};
		}

		public static class CommonSuggestions
		{
			public const string AddressHighPriority = "Address high-priority issues first";

			public const string TestInDevEnv = "Test your changes in a development environment";

			public const string TestInTargetEnv = "Test in target environment";

			public const string RunUnitTests = "Consider running unit tests if available";

			public const string VerifyFunctionality = "Verify all functionality works as expected";

			public const string CheckSymbolErrors = "Check for any symbol-related errors in the Problems panel";

			public const string ReviewCompilationWarnings = "Review compilation warnings in the Problems panel";

			public const string VerifyDependencies = "Verify all dependencies are resolved";

			public const string EnsureDependenciesAvailable = "Ensure all dependencies are available";

			public const string VerifyPublishConfig = "Ensure launch configuration has 'request': 'publish' and valid 'environmentType'";

			public const string VerifyDebugConfig = "Ensure launch configuration has 'request': 'launch' or 'attach' for debugging";

			public const string VerifySnapshotConfig = "Ensure launch configuration has 'request': 'snapshotInitialize' for production debugging";

			public const string VerifySymbolsConfig = "Ensure valid launch configuration for downloading symbols";

			public const string MonitorDeployment = "Monitor deployment logs";

			public static string CheckProblemsPanel => "Check the Problems panel for compilation errors and diagnostics";

			public static string VerifyAppJson => "Verify app.json has correct dependencies and version";
		}

		private static readonly Dictionary<string, WorkflowChain> StandardDevelopment = new Dictionary<string, WorkflowChain>
		{
			["al_downloadsymbols"] = new WorkflowChain
			{
				CurrentTool = "al_downloadsymbols",
				NextSteps = new WorkflowStep[2]
				{
					CommonSteps.BuildPackage,
					CommonSteps.BuildAllProjects
				},
				Suggestions = new string[3] { "Ensure valid launch configuration for downloading symbols", "Verify all dependencies are resolved", "Check for any symbol-related errors in the Problems panel" },
				WorkflowContext = "Symbol Download  Build  Quality Check"
			},
			["al_build"] = new WorkflowChain
			{
				CurrentTool = "al_build",
				NextSteps = new WorkflowStep[4]
				{
					CommonSteps.Publish,
					CommonSteps.PublishWithDebug,
					CommonSteps.GetDiagnosticsWarnings,
					CommonSteps.DownloadSymbols
				},
				Suggestions = new string[3] { "Review compilation warnings in the Problems panel", "Test your changes in a development environment", "Consider running unit tests if available" },
				WorkflowContext = "Build  Deploy or Debug"
			},
			["al_getdiagnostics"] = new WorkflowChain
			{
				CurrentTool = "al_getdiagnostics",
				NextSteps = new WorkflowStep[2]
				{
					CommonSteps.RebuildPackage,
					CommonSteps.SymbolSearch
				},
				Suggestions = new string[2]
				{
					CommonSuggestions.CheckProblemsPanel,
					"Address high-priority issues first"
				},
				WorkflowContext = "Review Diagnostics  Fix  Rebuild"
			},
			["al_publish"] = new WorkflowChain
			{
				CurrentTool = "al_publish",
				NextSteps = new WorkflowStep[2]
				{
					CommonSteps.AttachDebugger,
					CommonSteps.SetBreakpoint
				},
				Suggestions = new string[4] { "Ensure launch configuration has 'request': 'publish' and valid 'environmentType'", "Monitor deployment logs", "Test in target environment", "Verify all functionality works as expected" },
				WorkflowContext = "Deploy  Test  Debug"
			},
			["al_getdependencies"] = new WorkflowChain
			{
				CurrentTool = "al_getdependencies",
				NextSteps = new WorkflowStep[3]
				{
					CommonSteps.SymbolSearch,
					CommonSteps.DownloadSymbols,
					CommonSteps.BuildPackage
				},
				Suggestions = new string[2] { "Verify all dependencies are resolved", "Ensure all dependencies are available" },
				WorkflowContext = "Dependencies  Explore Symbols  Build"
			},
			["al_symbolsearch"] = new WorkflowChain
			{
				CurrentTool = "al_symbolsearch",
				NextSteps = new WorkflowStep[2]
				{
					CommonSteps.BuildPackage,
					CommonSteps.GetDiagnosticsErrors
				},
				Suggestions = new string[1] { "Verify all dependencies are resolved" },
				WorkflowContext = "Symbol Search  Build  Verify"
			}
		};

		private static readonly Dictionary<string, WorkflowChain> Debugging = new Dictionary<string, WorkflowChain>
		{
			["al_setbreakpoint"] = new WorkflowChain
			{
				CurrentTool = "al_setbreakpoint",
				NextSteps = new WorkflowStep[2]
				{
					CommonSteps.PublishWithDebug,
					CommonSteps.AttachDebugger
				},
				Suggestions = new string[1] { "Ensure launch configuration has 'request': 'launch' or 'attach' for debugging" },
				WorkflowContext = "Set Breakpoints  Deploy  Debug"
			},
			["al_debug"] = new WorkflowChain
			{
				CurrentTool = "al_debug",
				NextSteps = Array.Empty<WorkflowStep>(),
				Suggestions = new string[3] { "Ensure launch configuration has 'request': 'launch' or 'attach' for debugging", "Inspect variables and call stack during debugging", "Analyze runtime behavior to identify issues" },
				WorkflowContext = "Debugging session active"
			}
		};

		private static readonly Dictionary<string, WorkflowChain> SnapshotDebugging = new Dictionary<string, WorkflowChain>
		{
			["al_snapshotdebugging_init"] = new WorkflowChain
			{
				CurrentTool = "al_snapshotdebugging",
				NextSteps = new WorkflowStep[2]
				{
					CommonSteps.SetBreakpoint,
					CommonSteps.SnapshotFinish
				},
				Suggestions = new string[3] { "Ensure launch configuration has 'request': 'snapshotInitialize' for production debugging", "Set breakpoints in code you want to analyze", "Instruct user to reproduce the issue in Business Central" },
				WorkflowContext = "Snapshot Initialized  Set Breakpoints  Capture"
			},
			["al_snapshotdebugging_finish"] = new WorkflowChain
			{
				CurrentTool = "al_snapshotdebugging",
				NextSteps = new WorkflowStep[1] { CommonSteps.SnapshotView },
				Suggestions = new string[2] { "Snapshot captured successfully", "Ready to analyze captured data" },
				WorkflowContext = "Snapshot Captured  View & Analyze"
			},
			["al_snapshotdebugging_view"] = new WorkflowChain
			{
				CurrentTool = "al_snapshotdebugging",
				NextSteps = Array.Empty<WorkflowStep>(),
				Suggestions = new string[2] { "Analyze snapshot data to understand runtime behavior", "Review variable values at breakpoint locations" },
				WorkflowContext = "Analyzing Snapshot Data"
			}
		};

		private static readonly Dictionary<string, WorkflowChain> Troubleshooting = new Dictionary<string, WorkflowChain>
		{
			["al_build_failed"] = new WorkflowChain
			{
				CurrentTool = "al_build",
				NextSteps = new WorkflowStep[3]
				{
					CommonSteps.GetDiagnosticsErrors,
					CommonSteps.DownloadSymbolsHighPriority,
					CommonSteps.SymbolSearch
				},
				Suggestions = new string[3]
				{
					CommonSuggestions.CheckProblemsPanel,
					CommonSuggestions.VerifyAppJson,
					"Ensure all dependencies are available"
				},
				WorkflowContext = "Build Failed  Get Errors  Fix  Rebuild"
			},
			["al_publish_failed"] = new WorkflowChain
			{
				CurrentTool = "al_publish",
				NextSteps = new WorkflowStep[1] { CommonSteps.BuildPackage },
				Suggestions = new string[3] { "Ensure launch configuration has 'request': 'publish' and valid 'environmentType'", "Check if .app file exists", "Verify server connectivity and authentication" },
				WorkflowContext = "Publish Failed  Build  Retry Publish"
			}
		};

		public static WorkflowChain? GetChain(string toolName, bool success = true, string? context = null)
		{
			if (!success)
			{
				string key = toolName + "_failed";
				if (Troubleshooting.TryGetValue(key, out WorkflowChain value))
				{
					return FilterByCondition(value, success);
				}
			}
			if (toolName == "al_snapshotdebugging" && !string.IsNullOrEmpty(context))
			{
				string key2 = "al_snapshotdebugging_" + context;
				if (SnapshotDebugging.TryGetValue(key2, out WorkflowChain value2))
				{
					return FilterByCondition(value2, success);
				}
			}
			if (StandardDevelopment.TryGetValue(toolName, out WorkflowChain value3))
			{
				return FilterByCondition(value3, success);
			}
			if (Debugging.TryGetValue(toolName, out WorkflowChain value4))
			{
				return FilterByCondition(value4, success);
			}
			return null;
		}

		public static IReadOnlyList<string> GetNextStepStrings(string toolName, bool success = true, string? context = null)
		{
			WorkflowChain chain = GetChain(toolName, success, context);
			if (chain == null)
			{
				return Array.Empty<string>();
			}
			return (from s in chain.NextSteps
				orderby s.Priority descending
				select s.ToAgentString()).ToArray();
		}

		public static IReadOnlyList<string> GetSuggestions(string toolName, bool success = true, string? context = null)
		{
			return GetChain(toolName, success, context)?.Suggestions ?? Array.Empty<string>();
		}

		public static string? GetWorkflowContext(string toolName, bool success = true, string? context = null)
		{
			return GetChain(toolName, success, context)?.WorkflowContext;
		}

		private static WorkflowChain FilterByCondition(WorkflowChain chain, bool success)
		{
			WorkflowStep[] nextSteps = (from step in chain.NextSteps
				where step.Condition switch
				{
					WorkflowCondition.Always => true, 
					WorkflowCondition.OnSuccess => success, 
					WorkflowCondition.OnFailure => !success, 
					_ => true, 
				}
				select step into s
				orderby s.Priority descending
				select s).ToArray();
			return new WorkflowChain
			{
				CurrentTool = chain.CurrentTool,
				NextSteps = nextSteps,
				Suggestions = chain.Suggestions,
				WorkflowContext = chain.WorkflowContext
			};
		}
	}
	public sealed class WorkflowStep
	{
		public string Tool { get; init; } = string.Empty;


		public string DisplayName { get; init; } = string.Empty;


		public string Reason { get; init; } = string.Empty;


		public int Priority { get; init; } = 5;


		[JsonConverter(typeof(JsonStringEnumConverter))]
		public WorkflowCondition Condition { get; init; }

		public Dictionary<string, object>? Parameters { get; init; }

		public string ToAgentString()
		{
			string value = FormatParameters();
			return $"[Priority:{Priority}] {Tool}{value}: {Reason}";
		}

		private string FormatParameters()
		{
			if (Parameters == null || Parameters.Count == 0)
			{
				return string.Empty;
			}
			List<string> list = new List<string>();
			foreach (KeyValuePair<string, object> parameter in Parameters)
			{
				list.Add(parameter.Key + "=" + FormatValue(parameter.Value));
			}
			return " " + string.Join(" ", list);
		}

		private static string FormatValue(object value)
		{
			if (!(value is string text))
			{
				if (value is bool flag)
				{
					return flag.ToString().ToLowerInvariant();
				}
				return value?.ToString() ?? "null";
			}
			return "'" + text + "'";
		}
	}
}
namespace Microsoft.Dynamics.Nav.LanguageModelTools.TestRunning
{
	public class CodeCoverageForTest
	{
		public int MethodId { get; init; }

		public int ApplicationObjectId { get; init; }

		public Guid OwningApp { get; init; }

		public DateTime Timestamp { get; set; }

		public List<CodeCoverageProcedure> CoveredProcedures { get; set; } = new List<CodeCoverageProcedure>();


		public override bool Equals(object obj)
		{
			if (obj is CodeCoverageForTest codeCoverageForTest)
			{
				if (MethodId == codeCoverageForTest.MethodId && ApplicationObjectId == codeCoverageForTest.ApplicationObjectId)
				{
					return OwningApp == codeCoverageForTest.OwningApp;
				}
				return false;
			}
			return false;
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(MethodId, ApplicationObjectId, OwningApp);
		}
	}
	public class CodeCoverageInformation
	{
		public List<CodeCoverageForTest> Tests { get; set; } = new List<CodeCoverageForTest>();


		public Dictionary<TestIdentifier, TestResultStatus> TestStatus { get; set; } = new Dictionary<TestIdentifier, TestResultStatus>();

	}
	public struct TestIdentifier : IEquatable<TestIdentifier>
	{
		public int CodeunitId { get; set; }

		public string MethodName { get; set; }

		public TestIdentifier(int codeunitId, string methodName)
		{
			CodeunitId = codeunitId;
			MethodName = methodName;
		}

		public bool Equals(TestIdentifier other)
		{
			if (CodeunitId == other.CodeunitId)
			{
				return string.Equals(MethodName, other.MethodName, StringComparison.Ordinal);
			}
			return false;
		}

		public override bool Equals(object obj)
		{
			if (obj is TestIdentifier other)
			{
				return Equals(other);
			}
			return false;
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(CodeunitId, MethodName);
		}
	}
	public class CodeCoverageProcedure
	{
		public int MethodId { get; init; }

		public int ObjectId { get; init; }

		public int ObjectType { get; init; }
	}
	public enum CoverageMode
	{
		None,
		Line,
		Procedure
	}
	public class HubBasedTestRunnerService : HubBasedService
	{
		private static class HubServerConstantNames
		{
			internal const string TestRunnerHubName = "/TestRunnerHub";

			internal const string AuthenticationHeader = "Authentication";

			internal const string AuthorizationHeader = "Authorization";

			internal const string Tenant = "tenant";

			internal const string DeploymentId = "deploymentId";

			internal const string RunTests = "RunTests";

			internal const string StopTestExecution = "StopTestExecution";

			internal const string Initialize = "Initialize";
		}

		private static class HubClientConstantNames
		{
			internal const string TestStarted = "TestStarted";

			internal const string TestCompleted = "TestCompleted";

			internal const string TestRunCompleted = "TestRunCompleted";

			internal const string RuntimeInitialized = "RuntimeInitialized";
		}

		private TestPlan? testPlan;

		private int currentTestIndex = -1;

		private int isFinalized;

		public bool IsConnectionOpen => CanHandleRequest();

		public event Action<int, string>? TestStarted;

		public event Action<int, string, TestResultStatus, string, long>? TestCompleted;

		public event Action<CodeCoverageInformation>? RunCompleted;

		public event Action? RunFinished;

		public HubBasedTestRunnerService(IEmitLogger logger)
		{
			base.logger = logger;
		}

		public async Task SetupAndRunTests(ConnectionOptions connectionOptions, string startupCompany, TestPlan testPlan, string debuggingContext, CoverageMode coverageMode, CancellationToken cancellationToken)
		{
			await OpenConnectionAsync((connectionOptions.IsOnPremise() ? FindOnPremiseConfig(connectionOptions, logger) : FindCloudConfig(connectionOptions, logger)).Endpoint, connectionOptions).ConfigureAwait(continueOnCapturedContext: false);
			await Initialize(startupCompany, debuggingContext, coverageMode, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
			await RunTests(testPlan, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		}

		public async Task OpenConnectionAsync(string url, ConnectionOptions connectionOptions)
		{
			if (hubConnection != null)
			{
				await hubConnection.DisposeAsync().ConfigureAwait(continueOnCapturedContext: false);
			}
			HubConnectionOptions hubOptions = new HubConnectionOptions
			{
				Url = url
			};
			if (!string.IsNullOrEmpty(connectionOptions.Tenant))
			{
				hubOptions.QueryParameters.Add("tenant", connectionOptions.Tenant);
			}
			if (!string.IsNullOrEmpty(connectionOptions.DeploymentId))
			{
				hubOptions.QueryParameters.Add("deploymentId", connectionOptions.DeploymentId);
			}
			ServerInfo serverInfo = await ServerRegistry.DevInstance.GetServerInfo(connectionOptions, logger).ConfigureAwait(continueOnCapturedContext: false);
			if (serverInfo == null || !serverInfo.Supports(DevApiFeature.TestRunning))
			{
				logger?.Error("Server does not support test running.");
				await FinalizeTestRun().ConfigureAwait(continueOnCapturedContext: false);
				return;
			}
			ClientConnectionInfo clientConnectionInfo = await new AppsApiClient(connectionOptions, logger).GetClientConnectionInfo().ConfigureAwait(continueOnCapturedContext: false);
			if (clientConnectionInfo.AuthenticationHeader == null)
			{
				hubOptions.Credentials = CredentialCache.DefaultCredentials;
			}
			else
			{
				string value = clientConnectionInfo.AuthenticationHeader.ToString();
				hubOptions.QueryParameters.Add("Authentication", value);
				hubOptions.Headers.Add("Authorization", value);
			}
			await SetupConnection(hubOptions, "/TestRunnerHub").ConfigureAwait(continueOnCapturedContext: false);
		}

		public async Task RunTests(TestPlan testPlan, CancellationToken cancellationToken)
		{
			Interlocked.Exchange(ref isFinalized, 0);
			if (CanHandleRequest())
			{
				this.testPlan = testPlan;
				currentTestIndex = 0;
				await RunNextTest(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
			}
			else
			{
				await FinalizeTestRun().ConfigureAwait(continueOnCapturedContext: false);
			}
		}

		public async Task Initialize(string companyName, string debuggingContext, CoverageMode coverageMode, CancellationToken cancellationToken)
		{
			if (CanHandleRequest())
			{
				await hubConnection.InvokeAsync("Initialize", companyName, debuggingContext, coverageMode, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
			}
		}

		public async Task StopTestRunAsync(CancellationToken cancellationToken)
		{
			if (CanHandleRequest())
			{
				await hubConnection.InvokeAsync("StopTestExecution", cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
				await FinalizeTestRun().ConfigureAwait(continueOnCapturedContext: false);
				testPlan = null;
				currentTestIndex = -1;
			}
		}

		protected override void RegisterHubCallbacks()
		{
			if (hubConnection != null)
			{
				hubConnection.On("TestStarted", (Action<int, string>)OnTestStarted);
				hubConnection.On("TestCompleted", (Action<int, string, TestResultStatus, string, long>)OnTestCompleted);
				hubConnection.On("TestRunCompleted", (Action<CodeCoverageInformation>)OnTestRunCompleted);
				hubConnection.On("RuntimeInitialized", (Action)OnRuntimeInitialized);
				base.RegisterHubCallbacks();
				hubConnection.Closed += OnHubClosed;
			}
		}

		public override void HubConnected()
		{
			logger?.Info("Test hub connected.");
		}

		protected override async Task TerminateSession()
		{
			if (CanHandleRequest())
			{
				await hubConnection.InvokeAsync("StopTestExecution").ConfigureAwait(continueOnCapturedContext: false);
			}
		}

		private void OnTestStarted(int codeunitId, string methodName)
		{
			this.TestStarted?.Invoke(codeunitId, methodName);
		}

		private void OnTestCompleted(int codeunitId, string methodName, TestResultStatus status, string output, long duration)
		{
			this.TestCompleted?.Invoke(codeunitId, methodName, status, output, duration);
		}

		private void OnTestRunCompleted(CodeCoverageInformation codeCoverageData)
		{
			this.RunCompleted?.Invoke(codeCoverageData);
			RunNextTest(CancellationToken.None).ConfigureAwait(continueOnCapturedContext: false);
		}

		private async Task OnHubClosed(Exception? _)
		{
			if (testPlan != null)
			{
				await FinalizeTestRun().ConfigureAwait(continueOnCapturedContext: false);
			}
		}

		private void OnRuntimeInitialized()
		{
		}

		private async Task RunNextTest(CancellationToken cancellationToken)
		{
			try
			{
				if (cancellationToken.IsCancellationRequested || testPlan == null || currentTestIndex >= testPlan.CodeunitTests.Count)
				{
					await FinalizeTestRun().ConfigureAwait(continueOnCapturedContext: false);
					return;
				}
				CodeunitTestGroup codeunitTestGroup = testPlan.CodeunitTests[currentTestIndex];
				currentTestIndex++;
				await RunTestInternal(codeunitTestGroup.CodeunitId, new List<string>(codeunitTestGroup.TestMethods)).ConfigureAwait(continueOnCapturedContext: false);
			}
			catch (Exception ex)
			{
				logger?.Exception(ex);
				await FinalizeTestRun().ConfigureAwait(continueOnCapturedContext: false);
			}
		}

		private async Task RunTestInternal(int codeunitId, List<string> testMethods)
		{
			if (CanHandleRequest())
			{
				await hubConnection.InvokeCoreAsync("RunTests", new object[2]
				{
					codeunitId,
					testMethods.ToArray()
				}).ConfigureAwait(continueOnCapturedContext: false);
			}
		}

		private async Task FinalizeTestRun()
		{
			if (Interlocked.CompareExchange(ref isFinalized, 1, 0) == 0)
			{
				testPlan = null;
				this.RunFinished?.Invoke();
				await CloseConnectionAsync().ConfigureAwait(continueOnCapturedContext: false);
			}
		}

		public void SendMessage(string message)
		{
			logger?.Info(message);
		}

		private static (string InternalTenantId, string Endpoint, string DeploymentId) FindOnPremiseConfig(ConnectionOptions options, IEmitLogger logger)
		{
			Uri uri = OnPremiseHttpClientFactory.Instance.Value.CreateBaseClientUri(options, logger);
			return (InternalTenantId: options.Tenant, Endpoint: uri.AbsoluteUri + "dev", DeploymentId: null);
		}

		private static (string InternalTenantId, string Endpoint, string DeploymentId) FindCloudConfig(ConnectionOptions options, IEmitLogger logger)
		{
			Uri uri = CloudHttpClientFactory.Instance.Value.CreateBaseClientUri(options, logger);
			return (InternalTenantId: options.Tenant, Endpoint: uri.AbsoluteUri + "dev", DeploymentId: options.DeploymentId);
		}
	}
	public class TestMethodResult
	{
		public int CodeunitId { get; }

		public string MethodName { get; }

		public TestResultStatus Status { get; }

		public string Output { get; }

		public long DurationMs { get; }

		public TestMethodResult(int codeunitId, string methodName, TestResultStatus status, string output, long durationMs)
		{
			CodeunitId = codeunitId;
			MethodName = methodName;
			Status = status;
			Output = output;
			DurationMs = durationMs;
		}
	}
	public class TestPlan
	{
		public IReadOnlyList<CodeunitTestGroup> CodeunitTests { get; }

		public TestPlan(IEnumerable<CodeunitTestGroup> codeunitTests)
		{
			CodeunitTests = codeunitTests?.ToList() ?? new List<CodeunitTestGroup>();
		}
	}
	public class CodeunitTestGroup
	{
		public int CodeunitId { get; }

		public IReadOnlyList<string> TestMethods { get; }

		public CodeunitTestGroup(int codeunitId, IEnumerable<string> testMethods)
		{
			CodeunitId = codeunitId;
			TestMethods = testMethods?.ToList() ?? new List<string>();
		}
	}
	public enum TestResultStatus
	{
		Passed,
		Failed,
		Skipped
	}
	public sealed class TestRunParameters : ServerConnectionParameters
	{
		[Description("The name of the company to use when running tests (e.g., 'CRONUS International Ltd.').")]
		public string? Company { get; set; }

		[Description("The test codeunit ID to run (e.g., 50100).")]
		public int? CodeunitId { get; set; }

		[Description("Optional list of test method names to run within the codeunit. Runs all methods if not specified.")]
		public string[]? TestMethods { get; set; }
	}
	public class TestRunService
	{
		private const string ToolName = "al_run_tests";

		private readonly IEmitLogger logger;

		public TestRunService(IEmitLogger logger)
		{
			this.logger = logger ?? throw new ArgumentNullException("logger");
		}

		public virtual async Task<ToolResponse> RunTestsAsync(TestRunParameters parameters, CancellationToken cancellationToken = default(CancellationToken))
		{
			if (parameters == null)
			{
				return ResponseEnricher.CreateErrorResponse("al_run_tests", ToolErrorCode.InvalidProject, ToolErrorResources.ParametersCannotBeNull);
			}
			if (!parameters.CodeunitId.HasValue)
			{
				return ResponseEnricher.CreateErrorResponse("al_run_tests", ToolErrorCode.TestRunFailed, ToolErrorResources.CodeunitIdRequired);
			}
			try
			{
				ConnectionOptionsBuilder.MergeFromLaunchJson(parameters, logger);
				string missingConfigMessage = ConnectionOptionsBuilder.GetMissingConfigMessage(parameters);
				if (missingConfigMessage != null)
				{
					return ResponseEnricher.CreateErrorResponse("al_run_tests", ToolErrorCode.ConnectionFailed, missingConfigMessage);
				}
				ConnectionOptions connectionOptions = ConnectionOptionsBuilder.Build(parameters, logger);
				TestPlan testPlan = BuildTestPlan(parameters.CodeunitId.Value, parameters.TestMethods);
				List<TestMethodResult> results = new List<TestMethodResult>();
				TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
				using HubBasedTestRunnerService runner = new HubBasedTestRunnerService(logger);
				runner.TestCompleted += delegate(int codeunitId, string methodName, TestResultStatus status, string output, long duration)
				{
					results.Add(new TestMethodResult(codeunitId, methodName, status, output, duration));
				};
				runner.RunFinished += delegate
				{
					tcs.TrySetResult(result: true);
				};
				using (cancellationToken.Register(delegate
				{
					tcs.TrySetCanceled(cancellationToken);
				}))
				{
					await runner.SetupAndRunTests(connectionOptions, parameters.Company ?? string.Empty, testPlan, string.Empty, CoverageMode.None, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
					await tcs.Task.ConfigureAwait(continueOnCapturedContext: false);
					return FormatResponse(results);
				}
			}
			catch (OperationCanceledException)
			{
				return ResponseEnricher.CreateErrorResponse("al_run_tests", ToolErrorCode.TestRunFailed, ToolErrorResources.TestRunCancelled);
			}
			catch (Exception ex2)
			{
				logger.Exception(ex2);
				return ResponseEnricher.CreateErrorResponse("al_run_tests", ToolErrorCode.UnknownError, ex2.Message);
			}
		}

		private static TestPlan BuildTestPlan(int codeunitId, string[]? testMethods)
		{
			return new TestPlan(new CodeunitTestGroup[1]
			{
				new CodeunitTestGroup(codeunitId, testMethods ?? Array.Empty<string>())
			});
		}

		private static ToolResponse FormatResponse(List<TestMethodResult> results)
		{
			if (results.Count == 0)
			{
				return ResponseEnricher.CreateSuccessResponse("al_run_tests", "Test run completed. No test results were returned.");
			}
			int num = 0;
			int num2 = 0;
			int num3 = 0;
			StringBuilder stringBuilder = new StringBuilder();
			using (List<TestMethodResult>.Enumerator enumerator = results.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					switch (enumerator.Current.Status)
					{
					case TestResultStatus.Passed:
						num++;
						break;
					case TestResultStatus.Failed:
						num2++;
						break;
					case TestResultStatus.Skipped:
						num3++;
						break;
					}
				}
			}
			StringBuilder stringBuilder2 = stringBuilder;
			StringBuilder stringBuilder3 = stringBuilder2;
			StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(47, 3, stringBuilder2);
			handler.AppendLiteral("Test run completed: ");
			handler.AppendFormatted(num);
			handler.AppendLiteral(" passed, ");
			handler.AppendFormatted(num2);
			handler.AppendLiteral(" failed, ");
			handler.AppendFormatted(num3);
			handler.AppendLiteral(" skipped.");
			stringBuilder3.AppendLine(ref handler);
			stringBuilder.AppendLine();
			stringBuilder.AppendLine("Results:");
			foreach (TestMethodResult result in results)
			{
				string value = result.Status switch
				{
					TestResultStatus.Passed => "PASS", 
					TestResultStatus.Failed => "FAIL", 
					_ => "SKIP", 
				};
				stringBuilder2 = stringBuilder;
				StringBuilder stringBuilder4 = stringBuilder2;
				handler = new StringBuilder.AppendInterpolatedStringHandler(8, 3, stringBuilder2);
				handler.AppendLiteral("  ");
				handler.AppendFormatted(value);
				handler.AppendLiteral(" ");
				handler.AppendFormatted(result.MethodName);
				handler.AppendLiteral(" (");
				handler.AppendFormatted(result.DurationMs);
				handler.AppendLiteral("ms)");
				stringBuilder4.AppendLine(ref handler);
				if (result.Status == TestResultStatus.Failed && !string.IsNullOrWhiteSpace(result.Output))
				{
					stringBuilder2 = stringBuilder;
					StringBuilder stringBuilder5 = stringBuilder2;
					handler = new StringBuilder.AppendInterpolatedStringHandler(7, 1, stringBuilder2);
					handler.AppendLiteral("       ");
					handler.AppendFormatted(result.Output.Trim());
					stringBuilder5.AppendLine(ref handler);
				}
			}
			string text = stringBuilder.ToString().TrimEnd();
			if (num2 == 0)
			{
				return ResponseEnricher.CreateSuccessResponse("al_run_tests", text);
			}
			return ResponseEnricher.CreateErrorResponse("al_run_tests", ToolErrorCode.TestRunFailed, text);
		}
	}
}
namespace Microsoft.Dynamics.Nav.LanguageModelTools.SignalR
{
	internal class AspNetCoreSignalRFactory : ISignalRFactory
	{
		public HubConnection CreateHubConnection(HubConnectionOptions connectionOptions)
		{
			HubConnectionOptions connectionOptions2 = connectionOptions;
			IHubConnectionBuilder hubConnectionBuilder = ((IHubConnectionBuilder)new HubConnectionBuilder().AddNewtonsoftJsonProtocol(delegate(NewtonsoftJsonHubProtocolOptions options)
			{
				options.PayloadSerializerSettings.ContractResolver = new DefaultContractResolver();
			})).WithUrl(connectionOptions2.FullUrl, (Action<HttpConnectionOptions>)delegate(HttpConnectionOptions options)
			{
				options.Cookies = connectionOptions2.CookieContainer;
				options.Headers = connectionOptions2.Headers;
				options.Credentials = connectionOptions2.Credentials;
			});
			if (connectionOptions2.LogStream != null)
			{
				hubConnectionBuilder.ConfigureLogging(delegate(ILoggingBuilder loggingBuilder)
				{
					loggingBuilder.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Debug);
					loggingBuilder.AddProvider(new AspNetCoreSignalRLoggingProvider(connectionOptions2.LogStream));
				});
			}
			return hubConnectionBuilder.Build();
		}
	}
	internal class AspNetCoreSignalRLogger : ILogger
	{
		private readonly object lockObject = new object();

		private readonly StringWriter? outStream;

		public AspNetCoreSignalRLogger(StringWriter? outStream)
		{
			this.outStream = outStream;
		}

		IDisposable ILogger.BeginScope<TState>(TState state)
		{
			return null;
		}

		bool ILogger.IsEnabled(Microsoft.Extensions.Logging.LogLevel logLevel)
		{
			return true;
		}

		void ILogger.Log<TState>(Microsoft.Extensions.Logging.LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
		{
			if (outStream == null)
			{
				return;
			}
			try
			{
				string value = FormattableString.Invariant($"[{eventId.Name}] [{logLevel.ToString()}] - {formatter(state, exception)}");
				lock (lockObject)
				{
					outStream.WriteLine(value);
				}
			}
			catch (ObjectDisposedException)
			{
			}
		}
	}
	internal class AspNetCoreSignalRLoggingProvider : ILoggerProvider, IDisposable
	{
		private readonly AspNetCoreSignalRLogger logger;

		public AspNetCoreSignalRLoggingProvider(StringWriter? outStream)
		{
			logger = new AspNetCoreSignalRLogger(outStream);
		}

		ILogger ILoggerProvider.CreateLogger(string categoryName)
		{
			return logger;
		}

		void IDisposable.Dispose()
		{
		}
	}
	public abstract class HubBasedService : IDisposable
	{
		private static class HubClientConstantNames
		{
			internal const string HubConnected = "HubConnected";

			internal const string LogServerMessage = "LogServerMessage";

			internal const string LogServerInfoMessage = "LogServerInfoMessage";
		}

		public static readonly int DefaultHubConnectionTimeout = 30000;

		private readonly Dictionary<string, string> connectionContext = new Dictionary<string, string>();

		private static readonly Regex bearerTokenFilter = new Regex("Bearer.*$", RegexOptions.Multiline | RegexOptions.Compiled);

		protected IEmitLogger? logger;

		protected HubConnection? hubConnection;

		protected CookieContainer? cookieContainer;

		protected StringWriter? hubTraces;

		private bool isDisposed;

		public IDictionary<string, string> ConnectionContext => connectionContext;

		public bool CanHandleRequest()
		{
			if (hubConnection != null)
			{
				return hubConnection.State == HubConnectionState.Connected;
			}
			return false;
		}

		protected async Task SetupConnection(HubConnectionOptions hubOptions, string hubName)
		{
			hubTraces?.Dispose();
			hubTraces = new StringWriter(CultureInfo.InvariantCulture);
			hubOptions.LogStream = hubTraces;
			cookieContainer = new CookieContainer();
			hubOptions.CookieContainer = cookieContainer;
			hubOptions.HubName = hubName;
			hubConnection = new AspNetCoreSignalRFactory().CreateHubConnection(hubOptions);
			hubConnection.HandshakeTimeout = TimeSpan.FromMilliseconds(DefaultHubConnectionTimeout);
			RegisterHubCallbacks();
			try
			{
				await hubConnection.StartAsync().ConfigureAwait(continueOnCapturedContext: false);
			}
			catch (Exception ex)
			{
				logger?.Error("Failed to establish SignalR hub connection due to: " + ex.Message);
				LocalMachineLogger.LogError("Failed to establish SignalR hub connection with context: " + ex.Message);
				throw;
			}
			logger?.Info("SignalR hub connection established with context [" + hubConnection.ConnectionId + "]");
			SetConnectionContext(hubOptions.Url);
		}

		protected virtual void SetConnectionContext(string url)
		{
			if (hubConnection == null)
			{
				return;
			}
			connectionContext["connectioncontext"] = hubConnection.ConnectionId;
			string leftPart = new Uri(url).GetLeftPart(UriPartial.Authority);
			if (cookieContainer != null)
			{
				Cookie cookie = cookieContainer.GetCookies(new Uri(leftPart))?["ApplicationGatewayAffinity"];
				if (cookie != null)
				{
					connectionContext[cookie.Name] = cookie.Value;
				}
			}
		}

		private async Task OnConnectionError(Exception? ex)
		{
			if (ex != null)
			{
				logger?.Error("Connection closed with error: {0}", ex.Message);
				await TerminateSession();
			}
		}

		protected abstract Task TerminateSession();

		protected void CloseHubTraces()
		{
			if (hubTraces != null)
			{
				LocalMachineLogger.LogVerbose(bearerTokenFilter.Replace(hubTraces.ToString(), "[bearer token]"));
				hubTraces.Dispose();
				hubTraces = null;
			}
		}

		protected virtual void RegisterHubCallbacks()
		{
			if (hubConnection != null)
			{
				hubConnection.On("HubConnected", (Action)HubConnected);
				hubConnection.On("LogServerInfoMessage", (Action<string>)LogServerInfoMessage);
				hubConnection.On("LogServerMessage", (Action<string>)LogServerMessage);
				hubConnection.Closed += OnConnectionError;
			}
		}

		public abstract void HubConnected();

		public virtual void LogServerInfoMessage(string message)
		{
			if (!string.IsNullOrEmpty(message))
			{
				logger?.Info(message);
			}
		}

		public virtual void LogServerMessage(string message)
		{
			if (!string.IsNullOrEmpty(message))
			{
				logger?.Error(message);
			}
		}

		public virtual async Task CloseConnectionAsync()
		{
			if (CanHandleRequest())
			{
				await TerminateSession().ConfigureAwait(continueOnCapturedContext: false);
			}
			if (hubConnection != null)
			{
				hubConnection.Closed -= OnConnectionError;
				await hubConnection.DisposeAsync().ConfigureAwait(continueOnCapturedContext: false);
				hubConnection = null;
				CloseHubTraces();
			}
		}

		public virtual void Dispose()
		{
			if (!isDisposed)
			{
				try
				{
					isDisposed = true;
					TerminateSession().ConfigureAwait(continueOnCapturedContext: false).GetAwaiter().GetResult();
					CloseConnectionAsync().ConfigureAwait(continueOnCapturedContext: false).GetAwaiter().GetResult();
				}
				catch (Exception ex)
				{
					logger?.Exception(ex);
				}
			}
		}
	}
	public class HubConnectionOptions
	{
		internal const string DefaultSignalREndpoint = "/signalr";

		public string Url { get; set; }

		public bool UseDefaultUrl { get; set; }

		public CookieContainer CookieContainer { get; set; }

		public ICredentials Credentials { get; set; }

		public Dictionary<string, string> QueryParameters { get; }

		public Dictionary<string, string> Headers { get; }

		public StringWriter? LogStream { get; set; }

		public string HubName { get; set; }

		public string FullUrl => FormattableString.Invariant(FormattableStringFactory.Create("{0}{1}{2}?{3}", Url, HubName, UseDefaultUrl ? "/signalr" : string.Empty, QueryParameterString));

		public string QueryParameterString
		{
			get
			{
				if (QueryParameters.Count == 0)
				{
					return string.Empty;
				}
				string text = "";
				foreach (KeyValuePair<string, string> queryParameter in QueryParameters)
				{
					text += FormattableString.Invariant($"&{Uri.EscapeDataString(queryParameter.Key)}={Uri.EscapeDataString(queryParameter.Value)}");
				}
				return text;
			}
		}

		public HubConnectionOptions()
		{
			QueryParameters = new Dictionary<string, string>();
			Headers = new Dictionary<string, string>();
			UseDefaultUrl = false;
		}
	}
	internal interface ISignalRFactory
	{
		HubConnection CreateHubConnection(HubConnectionOptions connectionOptions);
	}
}
namespace Microsoft.Dynamics.Nav.LanguageModelTools.ServerConnection
{
	public static class ConnectionOptionsBuilder
	{
		public static ConnectionOptions Build(ServerConnectionParameters parameters, IEmitLogger? logger = null)
		{
			if (parameters == null)
			{
				throw new ArgumentNullException("parameters");
			}
			ServerConnectionConfiguration serverConnectionConfiguration = new ServerConnectionConfiguration
			{
				Server = parameters.Server,
				ServerInstance = parameters.ServerInstance,
				Port = parameters.Port,
				Tenant = parameters.Tenant,
				ApplicationFamily = parameters.ApplicationFamily,
				EnvironmentName = parameters.EnvironmentName,
				Environment = parameters.Environment,
				UseInteractiveLogin = parameters.UseInteractiveLogin,
				UseModernTieAuthUrl = parameters.UseModernTieAuthUrl
			};
			if (!string.IsNullOrEmpty(parameters.EnvironmentType) && Enum.TryParse<EnvironmentType>(parameters.EnvironmentType, ignoreCase: true, out var result))
			{
				serverConnectionConfiguration.EnvironmentType = result;
			}
			if (!string.IsNullOrEmpty(parameters.Authentication))
			{
				if (Enum.TryParse<AuthenticationMethod>(parameters.Authentication, ignoreCase: true, out var result2))
				{
					serverConnectionConfiguration.Authentication = result2;
				}
			}
			else
			{
				serverConnectionConfiguration.Authentication = AuthenticationMethod.AAD;
			}
			logger?.Info($"Built ConnectionOptions: auth={serverConnectionConfiguration.Authentication}, server={serverConnectionConfiguration.Server ?? "null"}, instance={serverConnectionConfiguration.ServerInstance ?? "null"}, env={serverConnectionConfiguration.EnvironmentName ?? "null"}");
			return serverConnectionConfiguration.CreateConnectionOptions(parameters.Environment);
		}

		public static LaunchConfiguration? MergeFromLaunchJson(ServerConnectionParameters parameters, IEmitLogger? logger = null)
		{
			if (parameters == null)
			{
				throw new ArgumentNullException("parameters");
			}
			if (string.IsNullOrEmpty(parameters.ProjectPath))
			{
				return null;
			}
			LaunchConfiguration firstAlConfiguration = LaunchConfigurationReader.GetFirstAlConfiguration(parameters.ProjectPath);
			if (firstAlConfiguration == null)
			{
				logger?.Info("No AL launch configuration found in launch.json");
				return null;
			}
			logger?.Info($"Using settings from launch.json: auth={firstAlConfiguration.Authentication ?? "null"}, server={firstAlConfiguration.Server ?? "null"}, instance={firstAlConfiguration.ServerInstance ?? "null"}");
			bool num = parameters.Environment != 0 || !string.IsNullOrEmpty(parameters.EnvironmentName) || string.Equals(parameters.EnvironmentType, "Sandbox", StringComparison.OrdinalIgnoreCase) || string.Equals(parameters.EnvironmentType, "Production", StringComparison.OrdinalIgnoreCase);
			bool flag = !string.IsNullOrEmpty(parameters.Server) || !string.IsNullOrEmpty(parameters.ServerInstance) || parameters.Port.HasValue || string.Equals(parameters.EnvironmentType, "OnPrem", StringComparison.OrdinalIgnoreCase);
			if (num && !flag)
			{
				string server = parameters.Server;
				string serverInstance = parameters.ServerInstance;
				int? port = parameters.Port;
				string authentication = parameters.Authentication;
				LaunchConfigurationReader.MergeIntoParameters(parameters, firstAlConfiguration);
				parameters.Server = server;
				parameters.ServerInstance = serverInstance;
				parameters.Port = port;
				parameters.Authentication = authentication;
				logger?.Info("launch.json merge: skipped on-prem settings for cloud operation");
			}
			else
			{
				LaunchConfigurationReader.MergeIntoParameters(parameters, firstAlConfiguration);
			}
			logger?.Info($"After merge: auth={parameters.Authentication ?? "null"}, server={parameters.Server ?? "null"}, instance={parameters.ServerInstance ?? "null"}");
			return firstAlConfiguration;
		}

		public static ConnectionOptions BuildWithLaunchJson(ServerConnectionParameters parameters, IEmitLogger? logger = null)
		{
			MergeFromLaunchJson(parameters, logger);
			return Build(parameters, logger);
		}

		public static string? GetMissingConfigMessage(ServerConnectionParameters parameters)
		{
			if (parameters == null)
			{
				throw new ArgumentNullException("parameters");
			}
			if (string.IsNullOrEmpty(parameters.Server) && string.IsNullOrEmpty(parameters.EnvironmentName) && string.IsNullOrEmpty(parameters.Tenant))
			{
				return "No server connection is configured. Provide environmentName and tenant for cloud, or serverUrl and serverInstance for on-premise, or add a configuration to launch.json.";
			}
			return null;
		}
	}
	public class LaunchConfiguration
	{
		[JsonPropertyName("type")]
		public string? Type { get; set; }

		[JsonPropertyName("name")]
		public string? Name { get; set; }

		[JsonPropertyName("request")]
		public string? Request { get; set; }

		[JsonPropertyName("server")]
		public string? Server { get; set; }

		[JsonPropertyName("serverInstance")]
		public string? ServerInstance { get; set; }

		[JsonPropertyName("port")]
		public int? Port { get; set; }

		[JsonPropertyName("tenant")]
		public string? Tenant { get; set; }

		[JsonPropertyName("authentication")]
		public string? Authentication { get; set; }

		[JsonPropertyName("environmentType")]
		public string? EnvironmentType { get; set; }

		[JsonPropertyName("environmentName")]
		public string? EnvironmentName { get; set; }

		[JsonPropertyName("schemaUpdateMode")]
		public string? SchemaUpdateMode { get; set; }
	}
	public class LaunchConfigurationDocument
	{
		[JsonPropertyName("version")]
		public string? Version { get; set; }

		[JsonPropertyName("configurations")]
		public LaunchConfiguration[]? Configurations { get; set; }
	}
	public static class LaunchConfigurationReader
	{
		private const string VsCodeFolder = ".vscode";

		private const string LaunchJsonFile = "launch.json";

		public static LaunchConfigurationDocument? ReadLaunchConfiguration(string projectPath)
		{
			if (string.IsNullOrEmpty(projectPath))
			{
				return null;
			}
			string path = Path.Combine(projectPath, ".vscode", "launch.json");
			if (!File.Exists(path))
			{
				return null;
			}
			try
			{
				string json = File.ReadAllText(path);
				JsonSerializerOptions options = new JsonSerializerOptions
				{
					PropertyNameCaseInsensitive = true,
					ReadCommentHandling = JsonCommentHandling.Skip,
					AllowTrailingCommas = true
				};
				return JsonSerializer.Deserialize<LaunchConfigurationDocument>(json, options);
			}
			catch (Exception)
			{
				return null;
			}
		}

		public static LaunchConfiguration? GetFirstAlConfiguration(string projectPath, string? configurationName = null)
		{
			string configurationName2 = configurationName;
			LaunchConfigurationDocument launchConfigurationDocument = ReadLaunchConfiguration(projectPath);
			if (launchConfigurationDocument?.Configurations == null || launchConfigurationDocument.Configurations.Length == 0)
			{
				return null;
			}
			IEnumerable<LaunchConfiguration> source = launchConfigurationDocument.Configurations.Where((LaunchConfiguration c) => string.Equals(c.Type, "al", StringComparison.OrdinalIgnoreCase) && string.Equals(c.Request, "launch", StringComparison.OrdinalIgnoreCase));
			if (!string.IsNullOrEmpty(configurationName2))
			{
				return source.FirstOrDefault((LaunchConfiguration c) => string.Equals(c.Name, configurationName2, StringComparison.OrdinalIgnoreCase));
			}
			return source.FirstOrDefault();
		}

		public static void MergeIntoParameters(ServerConnectionParameters parameters, LaunchConfiguration launchConfig)
		{
			if (parameters != null && launchConfig != null)
			{
				if (string.IsNullOrEmpty(parameters.Server))
				{
					parameters.Server = launchConfig.Server;
				}
				if (string.IsNullOrEmpty(parameters.ServerInstance))
				{
					parameters.ServerInstance = launchConfig.ServerInstance;
				}
				if (!parameters.Port.HasValue && launchConfig.Port.HasValue)
				{
					parameters.Port = launchConfig.Port;
				}
				if (string.IsNullOrEmpty(parameters.Tenant))
				{
					parameters.Tenant = launchConfig.Tenant;
				}
				if (string.IsNullOrEmpty(parameters.Authentication))
				{
					parameters.Authentication = launchConfig.Authentication;
				}
				if (string.IsNullOrEmpty(parameters.EnvironmentType))
				{
					parameters.EnvironmentType = launchConfig.EnvironmentType;
				}
				if (string.IsNullOrEmpty(parameters.EnvironmentName))
				{
					parameters.EnvironmentName = launchConfig.EnvironmentName;
				}
			}
		}
	}
	public class ServerConnectionParameters
	{
		[Description("Optional AL project folder path. Used to read connection settings from launch.json.")]
		public string? ProjectPath { get; set; }

		[Description("Publish environment for endpoint selection: Production (default), Tie, ServicesTie.")]
		public PublishEnvironment Environment { get; set; }

		[Description("Server URL for on-premise deployment (e.g., 'http://localhost').")]
		public string? Server { get; set; }

		[Description("Server instance name for on-premise deployment (e.g., 'BC').")]
		public string? ServerInstance { get; set; }

		[Description("Port number for on-premise development service.")]
		public int? Port { get; set; }

		[Description("Environment name for cloud deployment (e.g., 'sandbox', 'production').")]
		public string? EnvironmentName { get; set; }

		[Description("Environment type: 'OnPrem', 'Sandbox', or 'Production'.")]
		public string? EnvironmentType { get; set; }

		[Description("Authentication method: 'AAD' (Azure AD/Entra ID), 'Windows', or 'UserPassword'.")]
		public string? Authentication { get; set; }

		[Description("Tenant ID for multi-tenant environments.")]
		public string? Tenant { get; set; }

		[Description("Application family for the cloud server.")]
		public string? ApplicationFamily { get; set; }

		[Description("Set to true to force re-authentication (bypass cached tokens).")]
		public bool NoCache { get; set; }

		[Description("Use interactive browser login when authentication is required. If false, device code flow is used.")]
		public bool UseInteractiveLogin { get; set; } = true;


		[Description("Use the modern Tie/ServicesTie Entra ID authority URL (login.microsoftonline-ppe.com) instead of legacy (login.windows-ppe.net).")]
		public bool UseModernTieAuthUrl { get; set; }
	}
}
namespace Microsoft.Dynamics.Nav.LanguageModelTools.Publish
{
	public sealed class PublishParameters : ServerConnectionParameters
	{
		[Description("Optional path to the .app file to publish. If not specified, uses the built package from the project's output folder.")]
		public string? AppPath { get; set; }

		[Description("Schema update mode: 'Synchronize' (default), 'ForceSync', or 'Recreate'.")]
		public string? SchemaUpdateMode { get; set; }

		[Description("Set to true to force upgrade without requiring version change.")]
		public bool ForceUpgrade { get; set; }

		[Description("Skip the build step and publish the existing .app package from the project's output folder.")]
		public bool SkipBuild { get; set; }

		[Description("Include full dependency chain in publish. Set to true to build and publish all transitive dependencies.")]
		public bool BuildDependencies { get; set; }
	}
	public class PublishService
	{
		private const string ToolName = "al_publish";

		private readonly IEmitLogger logger;

		private readonly BuildWorkspaceDelegate? buildWorkspace;

		public PublishService(IEmitLogger logger, BuildWorkspaceDelegate? buildWorkspace = null)
		{
			this.logger = logger ?? throw new ArgumentNullException("logger");
			this.buildWorkspace = buildWorkspace;
		}

		public async Task<ToolResponse> PublishAsync(PublishParameters parameters, CancellationToken cancellationToken = default(CancellationToken))
		{
			if (parameters == null)
			{
				return ResponseEnricher.CreateErrorResponse("al_publish", ToolErrorCode.InvalidProject, ToolErrorResources.ParametersCannotBeNull);
			}
			try
			{
				LaunchConfiguration launchConfiguration = ConnectionOptionsBuilder.MergeFromLaunchJson(parameters, logger);
				if (launchConfiguration != null)
				{
					MergePublishSpecificParameters(parameters, launchConfiguration);
				}
				if (!string.IsNullOrEmpty(parameters.AppPath))
				{
					return await PublishAppFileAsync(parameters, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
				}
				if (parameters.SkipBuild)
				{
					return await PublishExistingPackageAsync(parameters, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
				}
				return await BuildAndPublishProjectAsync(parameters, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
			}
			catch (OperationCanceledException)
			{
				return ResponseEnricher.CreateErrorResponse("al_publish", ToolErrorCode.PublishFailed, ToolErrorResources.PublishCancelled);
			}
			catch (UserNotAuthenticatedException)
			{
				string text = parameters.Authentication ?? "Unknown";
				return ResponseEnricher.CreateErrorResponse("al_publish", ToolErrorCode.AuthenticationFailed, "Authentication required for method '" + text + "'. Run 'altool auth login' (interactive) and retry.");
			}
			catch (Exception ex3)
			{
				logger.Exception(ex3);
				return ResponseEnricher.CreateErrorResponse("al_publish", ToolErrorCode.UnknownError, ex3.Message);
			}
		}

		private async Task<ToolResponse> PublishAppFileAsync(PublishParameters parameters, CancellationToken cancellationToken)
		{
			string fullPath = Path.GetFullPath(parameters.AppPath);
			if (!File.Exists(fullPath))
			{
				return ResponseEnricher.CreateErrorResponse("al_publish", ToolErrorCode.PackageCreationFailed, "Package file not found: " + fullPath);
			}
			PublishOptions options = CreatePublishOptions(parameters, fullPath);
			return await PublishPackageAsync(options, fullPath, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		}

		private async Task<ToolResponse> BuildAndPublishProjectAsync(PublishParameters parameters, CancellationToken cancellationToken)
		{
			if (buildWorkspace == null)
			{
				return ResponseEnricher.CreateErrorResponse("al_publish", ToolErrorCode.PackageCreationFailed, ToolErrorResources.BuildDelegateUnavailable);
			}
			string projectPath = parameters.ProjectPath;
			if (string.IsNullOrEmpty(projectPath))
			{
				return ResponseEnricher.CreateErrorResponse("al_publish", ToolErrorCode.InvalidProject, ToolErrorResources.ProjectPathRequired);
			}
			BuildParameters parameters2 = new BuildParameters
			{
				ProjectPath = projectPath,
				Scope = (parameters.BuildDependencies ? BuildScope.All : BuildScope.Current)
			};
			var (flag, immutableArray) = await buildWorkspace(parameters2, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
			if (!flag || immutableArray.IsDefaultOrEmpty)
			{
				return ResponseEnricher.CreateErrorResponse("al_publish", ToolErrorCode.CompilationFailed, ToolErrorResources.BuildFailed);
			}
			string appPath = immutableArray[immutableArray.Length - 1].Item2;
			if (string.IsNullOrEmpty(appPath) || !File.Exists(appPath))
			{
				return ResponseEnricher.CreateErrorResponse("al_publish", ToolErrorCode.PackageCreationFailed, ToolErrorResources.OutputPackageNotFound);
			}
			PublishOptions options = CreatePublishOptions(parameters, appPath);
			string depsLabel = (parameters.BuildDependencies ? " (with dependencies)" : "");
			ToolResponse toolResponse = await PublishPackageAsync(options, appPath, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
			if (toolResponse.Succeeded)
			{
				toolResponse.Message = "Package published (full" + depsLabel + "): " + appPath;
			}
			return toolResponse;
		}

		private async Task<ToolResponse> PublishExistingPackageAsync(PublishParameters parameters, CancellationToken cancellationToken)
		{
			string projectPath = parameters.ProjectPath;
			if (string.IsNullOrEmpty(projectPath) || !Directory.Exists(projectPath))
			{
				return ResponseEnricher.CreateErrorResponse("al_publish", ToolErrorCode.InvalidProject, ToolErrorResources.ProjectPathRequired);
			}
			string appPath = FindMostRecentAppFile(projectPath);
			if (string.IsNullOrEmpty(appPath))
			{
				return ResponseEnricher.CreateErrorResponse("al_publish", ToolErrorCode.PackageCreationFailed, ToolErrorResources.SkipBuildNoPackageFound);
			}
			PublishOptions options = CreatePublishOptions(parameters, appPath);
			ToolResponse toolResponse = await PublishPackageAsync(options, appPath, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
			if (toolResponse.Succeeded)
			{
				toolResponse.Message = "Existing package published (skip build): " + appPath;
			}
			return toolResponse;
		}

		internal static string? FindMostRecentAppFile(string projectDir)
		{
			string[] obj = new string[2]
			{
				projectDir,
				Path.Combine(projectDir, "output")
			};
			FileInfo fileInfo = null;
			string[] array = obj;
			foreach (string path in array)
			{
				if (!Directory.Exists(path))
				{
					continue;
				}
				foreach (string item in Directory.EnumerateFiles(path, "*.app"))
				{
					FileInfo fileInfo2 = new FileInfo(item);
					if (fileInfo == null || fileInfo2.LastWriteTimeUtc > fileInfo.LastWriteTimeUtc)
					{
						fileInfo = fileInfo2;
					}
				}
			}
			return fileInfo?.FullName;
		}

		private async Task<ToolResponse> PublishPackageAsync(PublishOptions options, string appPath, CancellationToken cancellationToken)
		{
			logger.Info($"Publishing {appPath} (auth={options.Authentication})");
			PublishResult publishResult = await new Publisher(logger).Publish(options, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
			return publishResult.Success ? ResponseEnricher.CreateSuccessResponse("al_publish", $"Package published: {appPath} ({publishResult.FileSizeBytes} bytes)") : ResponseEnricher.CreateErrorResponse("al_publish", ToolErrorCode.PublishFailed, ToolErrorResources.PublishOperationFailed);
		}

		private static PublishOptions CreatePublishOptions(PublishParameters parameters, string appPath)
		{
			PublishOptions publishOptions = new PublishOptions
			{
				Directory = (string.IsNullOrEmpty(appPath) ? null : Path.GetDirectoryName(appPath)),
				PackageFileName = (string.IsNullOrEmpty(appPath) ? null : Path.GetFileName(appPath)),
				NoCache = parameters.NoCache,
				ForceUpgrade = parameters.ForceUpgrade,
				Server = parameters.Server,
				ServerInstance = parameters.ServerInstance,
				Port = parameters.Port,
				Tenant = parameters.Tenant,
				ApplicationFamily = parameters.ApplicationFamily,
				EnvironmentName = parameters.EnvironmentName,
				Environment = parameters.Environment,
				DependencyPublishingOption = DependencyPublishingOption.Ignore,
				UseInteractiveLogin = parameters.UseInteractiveLogin,
				UseModernTieAuthUrl = parameters.UseModernTieAuthUrl
			};
			if (!string.IsNullOrEmpty(parameters.EnvironmentType) && Enum.TryParse<EnvironmentType>(parameters.EnvironmentType, ignoreCase: true, out var result))
			{
				publishOptions.EnvironmentType = result;
			}
			if (!string.IsNullOrEmpty(parameters.Authentication) && Enum.TryParse<AuthenticationMethod>(parameters.Authentication, ignoreCase: true, out var result2))
			{
				publishOptions.Authentication = result2;
			}
			else
			{
				publishOptions.Authentication = AuthenticationMethod.AAD;
			}
			if (!string.IsNullOrEmpty(parameters.SchemaUpdateMode) && Enum.TryParse<SchemaUpdateMode>(parameters.SchemaUpdateMode, ignoreCase: true, out var result3))
			{
				publishOptions.SchemaUpdateMode = result3;
			}
			else
			{
				publishOptions.SchemaUpdateMode = SchemaUpdateMode.Synchronize;
			}
			return publishOptions;
		}

		private static void MergePublishSpecificParameters(PublishParameters parameters, LaunchConfiguration launchConfig)
		{
			if (parameters != null && launchConfig != null && string.IsNullOrEmpty(parameters.SchemaUpdateMode))
			{
				parameters.SchemaUpdateMode = launchConfig.SchemaUpdateMode;
			}
		}
	}
}
namespace Microsoft.Dynamics.Nav.LanguageModelTools.ErrorHandling
{
	public sealed class ErrorTemplate
	{
		public string Description { get; init; } = string.Empty;


		public IReadOnlyList<string> PossibleCauses { get; init; } = Array.Empty<string>();


		public IReadOnlyList<string> SuggestedActions { get; init; } = Array.Empty<string>();


		public IReadOnlyList<string> Alternatives { get; init; } = Array.Empty<string>();


		public IReadOnlyList<string> MissingPrerequisites { get; init; } = Array.Empty<string>();


		public IReadOnlyList<string> DiagnosticHints { get; init; } = Array.Empty<string>();


		public bool Retryable { get; init; }
	}
	public static class ErrorTemplateRegistry
	{
		public static class DiagnosticHints
		{
			public const string CheckAlExtensionLoaded = "Check if AL Language extension is properly loaded";

			public const string CheckAlExtensionActivated = "Check that AL Language extension is installed and enabled";

			public const string CheckExtensionOutput = "Check the AL Language extension output panel for details";

			public const string CheckProblemsPanel = "Check the Problems panel for compilation errors and diagnostics";

			public const string CheckBuildOutput = "Check build errors and diagnostics in the Output panel";

			public const string EnsureAlSourceFilesSyntax = "Ensure all AL source files have correct syntax";

			public const string ReviewWorkspaceProjects = "Review all projects in the workspace for syntax errors";

			public const string EnsureDependencyProjectsConfigured = "Ensure all dependency projects are properly configured";

			public const string CheckProjectsSymbolsDownloaded = "Check that all projects have required symbols downloaded";

			public const string CheckLaunchJson = "Check launch.json for correct server connection settings";

			public const string CheckServerAccessible = "Verify if the server is accessible";

			public const string CheckServerUrl = "Check server URL in launch.json configuration";

			public const string CheckNetwork = "Check network connectivity to the server";

			public const string CheckFirewall = "Check if firewall is blocking connection";

			public const string VerifyCredentials = "Verify authentication credentials are valid";

			public const string VerifyAuthMethod = "Verify authentication method in launch.json (Windows/AAD/UserPassword)";

			public const string VerifyAppJson = "Verify app.json has correct dependencies and version";

			public const string EnsureWorkspaceOpen = "Ensure a workspace folder is open in VS Code";

			public const string CheckFilePermissions = "Verify you have write permissions in the workspace folder";

			public const string VerifyPlatformInAppJson = "Verify app.json has platform and application versions specified";
		}

		public static class Alternatives
		{
			public const string RestartVSCode = "Restart VS Code";

			public const string ReloadWindow = "Reload VS Code window";

			public const string CheckExtensionsPanel = "Check if AL extension is activated in the Extensions panel";

			public const string VerifyServerUrl = "Verify server URL in launch.json";

			public const string VerifyServerPort = "Verify server URL and port in launch.json";

			public const string CheckAuthMethod = "Check authentication method in launch.json (Windows/AAD/UserPassword)";

			public const string CheckAuthConfig = "Check authentication method configuration in launch.json";

			public const string DownloadSymbolsFirst = "Try al_downloadsymbols tool to download symbols first";

			public const string BuildPackageFirst = "Try al_build tool to build the package first";

			public const string BuildIndividualProjects = "Try building individual projects with al_build to isolate issues";

			public const string CheckAppJson = "Review app.json for correct configuration";

			public const string VerifyAllAppJsonFiles = "Verify all app.json files in the workspace are valid";

			public const string CleanRebuild = "Delete .alpackages folder and rebuild";

			public const string CheckDependencyReferences = "Check that dependency references between projects are correct";

			public const string OpenFolderFirst = "Create a folder and open it in VS Code";

			public const string UseFileOpenFolder = "Use File > Open Folder to open a workspace";
		}

		public static class PossibleCauses
		{
			public const string ExtensionInternalError = "Extension internal error";

			public const string ExtensionNotActivated = "AL Language extension not activated";

			public const string ExtensionNotLoaded = "AL Language extension not properly loaded";

			public const string SyntaxErrors = "Syntax errors in AL code";

			public const string CompilationErrors = "Compilation errors";

			public const string MissingSymbols = "Missing or incompatible symbol references";

			public const string MissingDependencies = "Missing dependencies";

			public const string CompilerVersionMismatch = "Compiler version mismatch";

			public const string InvalidAppJson = "Invalid app.json configuration";

			public const string MissingPlatformInAppJson = "Missing platform or application in app.json";

			public const string InvalidLaunchJson = "Server connection settings incorrect in launch.json";

			public const string CircularDependencies = "Circular dependencies between projects";

			public const string IncorrectDependencyVersions = "Incorrect dependency versions specified in app.json";

			public const string ProjectsNotConfigured = "Projects not properly configured in workspace";

			public const string MissingBCSymbols = "Missing Business Central system symbols for one or more projects";

			public const string ServerConnectionFailed = "Server connection settings incorrect";

			public const string ServerNotAccessible = "Business Central server not accessible";

			public const string ServerNotRunning = "Server not accessible or not running";

			public const string AuthenticationFailed = "Authentication failed";

			public const string NetworkIssues = "Network connectivity issues";

			public const string FirewallBlocking = "Firewall blocking outbound connections";

			public const string ServerCertIssues = "Server certificate issues (HTTPS)";

			public const string ServerVersionIncompatible = "Server version incompatible with AL extension version";

			public const string NoWorkspaceFolder = "No workspace folder is open";

			public const string WorkspaceReadOnly = "Workspace folder is read-only";

			public const string InsufficientPermissions = "Insufficient file system permissions";

			public const string PackageBuildFailed = "Package build failed before publish";

			public const string VersionConflict = "Version conflict with existing extension on server";

			public const string NoAppFile = "No .app file found";
		}

		private static readonly ImmutableDictionary<ToolErrorCode, ErrorTemplate> Templates;

		static ErrorTemplateRegistry()
		{
			ImmutableDictionary<ToolErrorCode, ErrorTemplate>.Builder builder = ImmutableDictionary.CreateBuilder<ToolErrorCode, ErrorTemplate>();
			builder[ToolErrorCode.NoWorkspace] = new ErrorTemplate
			{
				Description = ToolErrorResources.NoWorkspace,
				PossibleCauses = new string[3] { "No workspace folder is open", "The current folder does not contain AL source files", "The workspace configuration is corrupted" },
				SuggestedActions = new string[3] { "Open a folder containing an AL project (with app.json file)", "Create a new AL project", "Check that the current workspace contains AL source files" },
				MissingPrerequisites = new string[1] { "AL project workspace" },
				Retryable = false
			};
			builder[ToolErrorCode.InvalidProject] = new ErrorTemplate
			{
				Description = ToolErrorResources.InvalidProject,
				PossibleCauses = new string[3] { "Invalid app.json configuration", "Invalid project structure", "Unsupported project version" },
				SuggestedActions = new string[3] { "Check if app.json exists and is valid", "Create a new AL project", "Verify the project structure matches AL conventions" },
				Retryable = false
			};
			builder[ToolErrorCode.MissingAppJson] = new ErrorTemplate
			{
				Description = ToolErrorResources.MissingAppJson,
				PossibleCauses = new string[3] { "This is not an AL project folder", "The app.json file was deleted or moved", "Working in the wrong directory" },
				SuggestedActions = new string[3] { "Navigate to the correct AL project folder", "Create a new AL project", "Check if app.json exists in a parent or subdirectory" },
				MissingPrerequisites = new string[1] { "app.json file" },
				Retryable = false
			};
			builder[ToolErrorCode.MissingLaunchJson] = new ErrorTemplate
			{
				Description = ToolErrorResources.MissingLaunchJson,
				PossibleCauses = new string[3] { "Project not configured for debugging", "Launch configuration was deleted", "Missing .vscode folder" },
				SuggestedActions = new string[3] { "Create launch.json configuration", "Use AL: Go! command to set up project", "Check .vscode folder exists" },
				Retryable = false
			};
			builder[ToolErrorCode.ConnectionFailed] = new ErrorTemplate
			{
				Description = ToolErrorResources.ConnectionFailed,
				PossibleCauses = new string[3] { "Server not accessible or not running", "Network connectivity issues", "Server connection settings incorrect in launch.json" },
				SuggestedActions = new string[3] { "Verify server is running and accessible", "Check network connectivity", "Review launch.json server settings" },
				DiagnosticHints = new string[2] { "Check launch.json for correct server connection settings", "Check network connectivity to the server" },
				Retryable = true
			};
			builder[ToolErrorCode.ServerUnavailable] = new ErrorTemplate
			{
				Description = ToolErrorResources.ServerUnavailable,
				PossibleCauses = new string[3] { "Server not accessible or not running", "Server maintenance in progress", "Network connectivity issues" },
				SuggestedActions = new string[3] { "Wait and retry later", "Check server status", "Contact server administrator" },
				Retryable = true
			};
			builder[ToolErrorCode.AuthenticationFailed] = new ErrorTemplate
			{
				Description = ToolErrorResources.AuthenticationFailed,
				PossibleCauses = new string[3] { "Invalid credentials", "Expired authentication token", "Insufficient file system permissions" },
				SuggestedActions = new string[3] { "Check username and password", "Re-authenticate with server", "Verify user permissions" },
				DiagnosticHints = new string[2] { "Verify authentication credentials are valid", "Verify authentication method in launch.json (Windows/AAD/UserPassword)" },
				Retryable = true
			};
			builder[ToolErrorCode.CompilationFailed] = new ErrorTemplate
			{
				Description = ToolErrorResources.CompilationFailed,
				PossibleCauses = new string[4] { "Syntax errors in AL code", "Missing dependencies", "Incompatible API versions", "Missing or incompatible symbol references" },
				SuggestedActions = new string[4] { "Check the Problems panel in VS Code for specific compilation errors", "Download symbols using al_downloadsymbols tool if missing", "Review and fix syntax errors in AL code", "Check app.json dependencies are correct" },
				Alternatives = new string[2] { "Try al_downloadsymbols tool to download symbols first", "Check dependencies with al_getpackagedependencies" },
				DiagnosticHints = new string[2] { "Check the Problems panel for compilation errors and diagnostics", "Check the AL Language extension output panel for details" },
				Retryable = true
			};
			builder[ToolErrorCode.MissingDependencies] = new ErrorTemplate
			{
				Description = ToolErrorResources.MissingDependencies,
				PossibleCauses = new string[3] { "Dependencies not specified in app.json", "Symbols not downloaded", "Incorrect dependency versions" },
				SuggestedActions = new string[3] { "Download symbols using al_downloadsymbols", "Verify app.json has correct dependencies and version", "Verify dependency versions are compatible" },
				Alternatives = new string[1] { "Try al_downloadsymbols tool to download symbols first" },
				Retryable = true
			};
			builder[ToolErrorCode.SymbolDownloadFailed] = new ErrorTemplate
			{
				Description = ToolErrorResources.SymbolDownloadFailed,
				PossibleCauses = new string[4] { "Network connectivity issues", "Server connection settings incorrect in launch.json", "Authentication failed", "Business Central server not accessible" },
				SuggestedActions = new string[4] { "Check internet connection and try again", "Verify server configuration in launch.json", "Check authentication credentials", "Try downloading symbols manually from VS Code command palette" },
				DiagnosticHints = new string[2] { "Check the AL Language extension output panel for details", "Check launch.json for correct server connection settings" },
				Retryable = true
			};
			builder[ToolErrorCode.PublishFailed] = new ErrorTemplate
			{
				Description = ToolErrorResources.PublishFailed,
				PossibleCauses = new string[4] { "Compilation errors", "Server connection settings incorrect", "Insufficient file system permissions", "Version conflict with existing extension on server" },
				SuggestedActions = new string[4] { "Fix compilation errors first using al_build", "Check server connection", "Verify publish permissions", "Try uninstalling existing version first" },
				Alternatives = new string[2] { "Try al_build tool to build the package first", "Try incremental publish" },
				DiagnosticHints = new string[2] { "Check the Problems panel for compilation errors and diagnostics", "Check the AL Language extension output panel for details" },
				Retryable = true
			};
			builder[ToolErrorCode.PackageCreationFailed] = new ErrorTemplate
			{
				Description = ToolErrorResources.PackageCreationFailed,
				PossibleCauses = new string[3] { "Compilation errors", "Missing dependencies", "Insufficient file system permissions" },
				SuggestedActions = new string[3] { "Fix compilation errors", "Download missing symbols", "Check file system permissions" },
				DiagnosticHints = new string[2] { "Check the Problems panel for compilation errors and diagnostics", "Verify you have write permissions in the workspace folder" },
				Retryable = true
			};
			builder[ToolErrorCode.TestRunFailed] = new ErrorTemplate
			{
				Description = ToolErrorResources.TestRunFailed,
				PossibleCauses = new string[3] { "One or more test methods failed", "Test run was cancelled", "Server connection settings incorrect" },
				SuggestedActions = new string[3] { "Review the failed test output for details", "Fix the failing tests and retry", "Check server connection and re-run" },
				DiagnosticHints = new string[1] { "Check the test output for assertion failures or runtime errors" },
				Retryable = true
			};
			builder[ToolErrorCode.CommandExecutionFailed] = new ErrorTemplate
			{
				Description = ToolErrorResources.CommandExecutionFailed,
				PossibleCauses = new string[3] { "Extension internal error", "Command not available", "Prerequisites not met" },
				SuggestedActions = new string[3] { "Try the operation again", "Restart VS Code", "Check AL extension output for details" },
				DiagnosticHints = new string[1] { "Check the AL Language extension output panel for details" },
				Retryable = true
			};
			builder[ToolErrorCode.InsufficientPermissions] = new ErrorTemplate
			{
				Description = ToolErrorResources.InsufficientPermissions,
				PossibleCauses = new string[3] { "Insufficient file system permissions", "Workspace folder is read-only", "User account lacks required permissions" },
				SuggestedActions = new string[3] { "Run VS Code as administrator", "Check file/folder permissions", "Contact system administrator" },
				DiagnosticHints = new string[1] { "Verify you have write permissions in the workspace folder" },
				Retryable = false
			};
			builder[ToolErrorCode.UnknownError] = new ErrorTemplate
			{
				Description = ToolErrorResources.UnknownError,
				PossibleCauses = new string[2] { "Extension internal error", "Unexpected system state" },
				SuggestedActions = new string[3] { "Try the operation again", "Restart VS Code", "Check AL extension output for details" },
				DiagnosticHints = new string[1] { "Check the AL Language extension output panel for details" },
				Retryable = true
			};
			Templates = builder.ToImmutable();
		}

		public static ErrorTemplate? GetTemplate(ToolErrorCode errorCode)
		{
			if (!Templates.TryGetValue(errorCode, out ErrorTemplate value))
			{
				return null;
			}
			return value;
		}

		public static IReadOnlyDictionary<ToolErrorCode, ErrorTemplate> GetAllTemplates()
		{
			return Templates;
		}
	}
	public static class ResponseEnricher
	{
		public static ToolResponse CreateSuccessResponse(string toolName, string message, object? data = null, IReadOnlyList<string>? warnings = null)
		{
			return new ToolResponse
			{
				Succeeded = true,
				Message = message,
				Data = data,
				NextSteps = WorkflowEngine.GetNextStepStrings(toolName),
				Warnings = (warnings ?? Array.Empty<string>())
			};
		}

		public static ToolResponse CreateErrorResponse(string toolName, ToolErrorCode errorCode, string? originalError = null, ErrorDetailOverrides? customOverrides = null)
		{
			return CreateErrorResponseCore(toolName, errorCode, originalError, null, customOverrides);
		}

		public static ToolResponse CreateErrorResponse(string toolName, ToolErrorCode errorCode, string? originalError, object? data, ErrorDetailOverrides? customOverrides = null)
		{
			return CreateErrorResponseCore(toolName, errorCode, originalError, data, customOverrides);
		}

		private static ToolResponse CreateErrorResponseCore(string toolName, ToolErrorCode errorCode, string? originalError, object? data, ErrorDetailOverrides? customOverrides)
		{
			ErrorTemplate template = ErrorTemplateRegistry.GetTemplate(errorCode);
			ToolErrorDetails errorDetails = new ToolErrorDetails
			{
				Code = errorCode.ToString(),
				Description = (customOverrides?.Description ?? template?.Description ?? ToolErrorResources.GenericErrorOccurred),
				PossibleCauses = MergeArrays(template?.PossibleCauses, customOverrides?.AdditionalPossibleCauses),
				SuggestedActions = MergeArrays(template?.SuggestedActions, customOverrides?.AdditionalSuggestedActions),
				Alternatives = MergeArrays(template?.Alternatives, customOverrides?.AdditionalAlternatives),
				MissingPrerequisites = MergeArrays(template?.MissingPrerequisites, customOverrides?.AdditionalMissingPrerequisites),
				DiagnosticHints = MergeArrays(template?.DiagnosticHints, customOverrides?.AdditionalDiagnosticHints),
				Retryable = (customOverrides?.Retryable ?? template?.Retryable ?? true)
			};
			return new ToolResponse
			{
				Succeeded = false,
				Message = (originalError ?? template?.Description ?? ToolErrorResources.GenericErrorOccurred),
				Data = data,
				NextSteps = WorkflowEngine.GetNextStepStrings(toolName, success: false),
				ErrorDetails = errorDetails
			};
		}

		private static IReadOnlyList<string> MergeArrays(IReadOnlyList<string>? baseArray, IReadOnlyList<string>? additionalArray)
		{
			if (baseArray == null && additionalArray == null)
			{
				return Array.Empty<string>();
			}
			if (baseArray == null)
			{
				return additionalArray;
			}
			if (additionalArray == null || additionalArray.Count == 0)
			{
				return baseArray;
			}
			return baseArray.Concat(additionalArray).ToArray();
		}
	}
	public sealed class ErrorDetailOverrides
	{
		public string? Description { get; set; }

		public IReadOnlyList<string>? AdditionalPossibleCauses { get; set; }

		public IReadOnlyList<string>? AdditionalSuggestedActions { get; set; }

		public IReadOnlyList<string>? AdditionalAlternatives { get; set; }

		public IReadOnlyList<string>? AdditionalMissingPrerequisites { get; set; }

		public IReadOnlyList<string>? AdditionalDiagnosticHints { get; set; }

		public bool? Retryable { get; set; }
	}
	public enum ToolErrorCode
	{
		NoWorkspace,
		InvalidProject,
		MissingAppJson,
		MissingLaunchJson,
		ConnectionFailed,
		ServerUnavailable,
		AuthenticationFailed,
		CompilationFailed,
		MissingDependencies,
		SymbolDownloadFailed,
		PublishFailed,
		PackageCreationFailed,
		TestRunFailed,
		CommandExecutionFailed,
		InsufficientPermissions,
		UnknownError
	}
	[GeneratedCode("System.Resources.Tools.StronglyTypedResourceBuilder", "18.0.0.0")]
	[DebuggerNonUserCode]
	[CompilerGenerated]
	internal class ToolErrorResources
	{
		private static ResourceManager resourceMan;

		private static CultureInfo resourceCulture;

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		internal static ResourceManager ResourceManager
		{
			get
			{
				if (resourceMan == null)
				{
					resourceMan = new ResourceManager("Microsoft.Dynamics.Nav.LanguageModelTools.ErrorHandling.ToolErrorResources", typeof(ToolErrorResources).Assembly);
				}
				return resourceMan;
			}
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		internal static CultureInfo Culture
		{
			get
			{
				return resourceCulture;
			}
			set
			{
				resourceCulture = value;
			}
		}

		internal static string AllDependenciesAvailable => ResourceManager.GetString("AllDependenciesAvailable", resourceCulture);

		internal static string AllSymbolsInCache => ResourceManager.GetString("AllSymbolsInCache", resourceCulture);

		internal static string AuthenticationFailed => ResourceManager.GetString("AuthenticationFailed", resourceCulture);

		internal static string AuthenticationRequiredRunLogin => ResourceManager.GetString("AuthenticationRequiredRunLogin", resourceCulture);

		internal static string BuildCancelled => ResourceManager.GetString("BuildCancelled", resourceCulture);

		internal static string BuildDelegateUnavailable => ResourceManager.GetString("BuildDelegateUnavailable", resourceCulture);

		internal static string BuildFailed => ResourceManager.GetString("BuildFailed", resourceCulture);

		internal static string BuildSucceeded => ResourceManager.GetString("BuildSucceeded", resourceCulture);

		internal static string CommandExecutionFailed => ResourceManager.GetString("CommandExecutionFailed", resourceCulture);

		internal static string CompilationFailed => ResourceManager.GetString("CompilationFailed", resourceCulture);

		internal static string ConnectionFailed => ResourceManager.GetString("ConnectionFailed", resourceCulture);

		internal static string GenericErrorOccurred => ResourceManager.GetString("GenericErrorOccurred", resourceCulture);

		internal static string InsufficientPermissions => ResourceManager.GetString("InsufficientPermissions", resourceCulture);

		internal static string InvalidProject => ResourceManager.GetString("InvalidProject", resourceCulture);

		internal static string MissingAppJson => ResourceManager.GetString("MissingAppJson", resourceCulture);

		internal static string MissingDependencies => ResourceManager.GetString("MissingDependencies", resourceCulture);

		internal static string MissingLaunchJson => ResourceManager.GetString("MissingLaunchJson", resourceCulture);

		internal static string NoProjectFound => ResourceManager.GetString("NoProjectFound", resourceCulture);

		internal static string NoSolutionLoaded => ResourceManager.GetString("NoSolutionLoaded", resourceCulture);

		internal static string NoWorkspace => ResourceManager.GetString("NoWorkspace", resourceCulture);

		internal static string OutputPackageNotFound => ResourceManager.GetString("OutputPackageNotFound", resourceCulture);

		internal static string PackageCreationFailed => ResourceManager.GetString("PackageCreationFailed", resourceCulture);

		internal static string PackagingCancelled => ResourceManager.GetString("PackagingCancelled", resourceCulture);

		internal static string PackagingFailed => ResourceManager.GetString("PackagingFailed", resourceCulture);

		internal static string PackagingNoOutputPath => ResourceManager.GetString("PackagingNoOutputPath", resourceCulture);

		internal static string ParametersCannotBeNull => ResourceManager.GetString("ParametersCannotBeNull", resourceCulture);

		internal static string CodeunitIdRequired => ResourceManager.GetString("CodeunitIdRequired", resourceCulture);

		internal static string TestRunCancelled => ResourceManager.GetString("TestRunCancelled", resourceCulture);

		internal static string ProjectPathRequired => ResourceManager.GetString("ProjectPathRequired", resourceCulture);

		internal static string PublishCancelled => ResourceManager.GetString("PublishCancelled", resourceCulture);

		internal static string PublishFailed => ResourceManager.GetString("PublishFailed", resourceCulture);

		internal static string PublishOperationFailed => ResourceManager.GetString("PublishOperationFailed", resourceCulture);

		internal static string SkipBuildNoPackageFound => ResourceManager.GetString("SkipBuildNoPackageFound", resourceCulture);

		internal static string TestRunFailed => ResourceManager.GetString("TestRunFailed", resourceCulture);

		internal static string ServerUnavailable => ResourceManager.GetString("ServerUnavailable", resourceCulture);

		internal static string SymbolDownloadCancelled => ResourceManager.GetString("SymbolDownloadCancelled", resourceCulture);

		internal static string SymbolDownloadFailed => ResourceManager.GetString("SymbolDownloadFailed", resourceCulture);

		internal static string UnknownError => ResourceManager.GetString("UnknownError", resourceCulture);

		internal ToolErrorResources()
		{
		}
	}
}
namespace Microsoft.Dynamics.Nav.LanguageModelTools.DownloadSymbols
{
	public sealed class DownloadSymbolsParameters : ServerConnectionParameters
	{
		[Description("Set to true to force re-download all symbols even if they exist in cache.")]
		public bool Force { get; set; }

		[Description("Set to true to download symbols from global sources only (AppSource, Microsoft). No server connection required.")]
		public bool GlobalSourcesOnly { get; set; }

		[Description("Symbols country/region for NuGet downloads (e.g. 'w1', 'us', 'dk'). Default: 'w1'.")]
		public string? SymbolsCountryRegion { get; set; }

		[Description("Custom NuGet feed URLs to search before built-in Microsoft feeds.")]
		public IReadOnlyList<string>? NugetFeeds { get; set; }

		[Description("Set to true to skip built-in Microsoft feeds and only search custom feeds.")]
		public bool UseOnlyCustomFeeds { get; set; }
	}
	public sealed class DownloadSymbolsResult
	{
		public int DownloadedCount { get; set; }

		public int TotalReferences { get; set; }

		public int RequestedCount { get; set; }

		public string? CachePath { get; set; }

		public string? Source { get; set; }
	}
	public delegate void RefreshWorkspaceDelegate(ImmutableArray<SymbolReferenceSpecification> downloadedReferences);
	public sealed class DownloadSymbolsService
	{
		private const string ToolName = "al_downloadsymbols";

		private readonly Workspace workspace;

		private readonly IEmitLogger logger;

		private readonly RefreshWorkspaceDelegate? refreshWorkspace;

		public DownloadSymbolsService(Workspace workspace, IEmitLogger logger, RefreshWorkspaceDelegate? refreshWorkspace = null)
		{
			this.workspace = workspace ?? throw new ArgumentNullException("workspace");
			this.logger = logger ?? throw new ArgumentNullException("logger");
			this.refreshWorkspace = refreshWorkspace;
		}

		public async Task<ToolResponse> DownloadSymbolsAsync(DownloadSymbolsParameters? parameters, CancellationToken cancellationToken = default(CancellationToken))
		{
			if (parameters == null)
			{
				parameters = new DownloadSymbolsParameters();
			}
			try
			{
				Solution currentSolution = workspace.CurrentSolution;
				if (currentSolution == null)
				{
					return ResponseEnricher.CreateErrorResponse("al_downloadsymbols", ToolErrorCode.NoWorkspace, ToolErrorResources.NoSolutionLoaded);
				}
				Project project = currentSolution.Projects.FirstOrDefault();
				if (project == null)
				{
					return ResponseEnricher.CreateErrorResponse("al_downloadsymbols", ToolErrorCode.InvalidProject, ToolErrorResources.NoProjectFound);
				}
				IEnumerable<string> packageCachePaths = project.PackageCachePaths;
				string cacheDirectory = Path.Combine(project.ProjectFolder ?? string.Empty, packageCachePaths.FirstOrDefault() ?? ".alpackages");
				logger.Info("Using package cache path: " + cacheDirectory);
				ProjectManifest projectManifest = ProjectLoader.LoadFromFile(project.FilePath);
				if (projectManifest == null)
				{
					return ResponseEnricher.CreateErrorResponse("al_downloadsymbols", ToolErrorCode.MissingAppJson, "Project manifest not found or invalid: " + project.FilePath);
				}
				IList<SymbolReferenceSpecification> symbolReferences = GetSymbolReferencesFromManifest(projectManifest);
				if (symbolReferences.Count == 0)
				{
					return ResponseEnricher.CreateSuccessResponse("al_downloadsymbols", ToolErrorResources.AllDependenciesAvailable);
				}
				logger.Info($"Found {symbolReferences.Count} symbol references to check.");
				ImmutableArray<SymbolReferenceSpecification> referencesToDownload;
				if (parameters.Force)
				{
					referencesToDownload = symbolReferences.ToImmutableArray();
					logger.Info("Force mode: will download all symbols.");
				}
				else
				{
					new LocalCacheSymbolReferenceAnalyzer(project.PackageCachePaths).FindMissingReferences(symbolReferences, out IEnumerable<SymbolReferenceSpecification> missingReferences);
					referencesToDownload = missingReferences.ToImmutableArray();
					logger.Info($"Found {referencesToDownload.Length} missing symbols to download.");
				}
				if (referencesToDownload.Length == 0)
				{
					return ResponseEnricher.CreateSuccessResponse("al_downloadsymbols", ToolErrorResources.AllSymbolsInCache, new DownloadSymbolsResult
					{
						TotalReferences = symbolReferences.Count,
						CachePath = cacheDirectory
					});
				}
				ImmutableArray<SymbolReferenceSpecification> downloadedReferences;
				string source;
				if (parameters.GlobalSourcesOnly)
				{
					downloadedReferences = await DownloadFromGlobalSourcesAsync(parameters, referencesToDownload, cacheDirectory).ConfigureAwait(continueOnCapturedContext: false);
					source = "NuGet (Global Sources)";
				}
				else
				{
					ConnectionOptions connectionOptions = ConnectionOptionsBuilder.BuildWithLaunchJson(parameters, logger);
					downloadedReferences = await DownloadFromServerAsync(connectionOptions, referencesToDownload, cacheDirectory).ConfigureAwait(continueOnCapturedContext: false);
					source = "Business Central Server";
				}
				bool num = downloadedReferences.Length >= referencesToDownload.Length;
				if (downloadedReferences.Length > 0)
				{
					refreshWorkspace?.Invoke(downloadedReferences);
					logger.Info("Workspace symbol refresh requested.");
				}
				if (num)
				{
					return ResponseEnricher.CreateSuccessResponse("al_downloadsymbols", $"Successfully downloaded {downloadedReferences.Length} symbol package(s).", new DownloadSymbolsResult
					{
						DownloadedCount = downloadedReferences.Length,
						TotalReferences = symbolReferences.Count,
						CachePath = cacheDirectory,
						Source = source
					});
				}
				return ResponseEnricher.CreateErrorResponse("al_downloadsymbols", ToolErrorCode.SymbolDownloadFailed, $"Downloaded {downloadedReferences.Length} of {referencesToDownload.Length} requested symbols. Some symbols may not be available.", new DownloadSymbolsResult
				{
					DownloadedCount = downloadedReferences.Length,
					RequestedCount = referencesToDownload.Length,
					CachePath = cacheDirectory
				});
			}
			catch (OperationCanceledException)
			{
				return ResponseEnricher.CreateErrorResponse("al_downloadsymbols", ToolErrorCode.SymbolDownloadFailed, ToolErrorResources.SymbolDownloadCancelled);
			}
			catch (UserNotAuthenticatedException)
			{
				return ResponseEnricher.CreateErrorResponse("al_downloadsymbols", ToolErrorCode.AuthenticationFailed, ToolErrorResources.AuthenticationRequiredRunLogin);
			}
			catch (Exception ex3)
			{
				logger.Exception(ex3);
				return ResponseEnricher.CreateErrorResponse("al_downloadsymbols", ToolErrorCode.UnknownError, ex3.Message);
			}
		}

		private async Task<ImmutableArray<SymbolReferenceSpecification>> DownloadFromServerAsync(ConnectionOptions connectionOptions, ImmutableArray<SymbolReferenceSpecification> referencesToDownload, string cacheDirectory)
		{
			logger.Info($"Downloading {referencesToDownload.Length} packages from Business Central server...");
			return await ((IPackageDownloader)new NavDevServerPackageDownloader(connectionOptions, logger)).DownloadPackages(referencesToDownload, cacheDirectory).ConfigureAwait(continueOnCapturedContext: false);
		}

		private async Task<ImmutableArray<SymbolReferenceSpecification>> DownloadFromGlobalSourcesAsync(DownloadSymbolsParameters parameters, ImmutableArray<SymbolReferenceSpecification> referencesToDownload, string cacheDirectory)
		{
			logger.Info($"Downloading {referencesToDownload.Length} packages from global NuGet sources...");
			string text = parameters.SymbolsCountryRegion ?? "w1";
			logger.Info("Using symbols country/region: " + text);
			return await new NuGetPackageDownloader(logger, text, parameters.NugetFeeds, parameters.UseOnlyCustomFeeds).DownloadPackages(referencesToDownload, cacheDirectory).ConfigureAwait(continueOnCapturedContext: false);
		}

		private static IList<SymbolReferenceSpecification> GetSymbolReferencesFromManifest(ProjectManifest manifest)
		{
			List<SymbolReferenceSpecification> list = new List<SymbolReferenceSpecification>();
			if (manifest.AppManifest?.PlatformReference != null)
			{
				list.Add(manifest.AppManifest.PlatformReference);
			}
			if (manifest.AppManifest?.AppReference != null)
			{
				list.Add(manifest.AppManifest.AppReference);
			}
			if (manifest.AppManifest?.DependencyReferences != null)
			{
				list.AddRange(manifest.AppManifest.DependencyReferences);
			}
			return list;
		}
	}
}
namespace Microsoft.Dynamics.Nav.LanguageModelTools.Diagnostics
{
	public sealed class DiagnosticInfo
	{
		public string File { get; set; } = string.Empty;


		public DiagnosticRange Range { get; set; } = new DiagnosticRange();


		public string Severity { get; set; } = string.Empty;


		public string Message { get; set; } = string.Empty;


		public string? Source { get; set; }

		public string? Code { get; set; }
	}
	public sealed class DiagnosticRange
	{
		public int StartLine { get; set; }

		public int StartColumn { get; set; }

		public int EndLine { get; set; }

		public int EndColumn { get; set; }
	}
	public sealed class DiagnosticsParameters
	{
		[Description("Diagnostics scope: file path (.al/.dal).")]
		public string? FilePath { get; set; }

		[Description("Diagnostics scope: folder path (recursive).")]
		public string? FolderPath { get; set; }

		[Description("Diagnostics scope: AL project folder path.")]
		public string? ProjectPath { get; set; }

		[Description("Filter by diagnostic severity. Default: all.")]
		public IReadOnlyList<string>? Severities { get; set; }

		[Description("Filter by diagnostic source (matches diagnostic.source). Default: all.")]
		public IReadOnlyList<string>? Areas { get; set; }

		[Description("Max diagnostics returned. Default: 200, max: 500.")]
		public int? Limit { get; set; }
	}
	public sealed class DiagnosticsResult
	{
		public bool Succeeded { get; set; }

		public string? Message { get; set; }

		public IReadOnlyList<DiagnosticInfo> Diagnostics { get; set; } = Array.Empty<DiagnosticInfo>();


		public int ErrorCount { get; set; }

		public bool Truncated { get; set; }
	}
	public sealed class DiagnosticsService : IDisposable
	{
		private const int DefaultLimit = 200;

		private const int MaxLimit = 500;

		private readonly Workspace workspace;

		private bool disposed;

		public DiagnosticsService(Workspace workspace)
		{
			this.workspace = workspace ?? throw new ArgumentNullException("workspace");
		}

		public async Task<DiagnosticsResult> GetDiagnosticsAsync(DiagnosticsParameters parameters, CancellationToken cancellationToken)
		{
			ThrowIfDisposed();
			if (parameters == null)
			{
				parameters = new DiagnosticsParameters();
			}
			Solution currentSolution = workspace.CurrentSolution;
			if (currentSolution == null)
			{
				return CreateEmptyResult();
			}
			int limit = Math.Min(parameters.Limit.GetValueOrDefault(200), 500);
			HashSet<string> severityFilter = CreateSeverityFilter(parameters.Severities);
			HashSet<string> areaFilter = CreateAreaFilter(parameters.Areas);
			List<DiagnosticInfo> results = new List<DiagnosticInfo>(Math.Min(limit, 200));
			int errorCount = 0;
			bool truncated = false;
			foreach (Project project in currentSolution.Projects)
			{
				cancellationToken.ThrowIfCancellationRequested();
				if (!string.IsNullOrEmpty(parameters.ProjectPath) && !string.Equals(project.ProjectFolder, parameters.ProjectPath, StringComparison.OrdinalIgnoreCase))
				{
					continue;
				}
				Compilation compilation = await project.GetCompilationAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
				if (compilation == null)
				{
					continue;
				}
				ImmutableArray<Diagnostic>.Enumerator enumerator2 = compilation.GetDiagnostics(cancellationToken).GetEnumerator();
				while (enumerator2.MoveNext())
				{
					Diagnostic current2 = enumerator2.Current;
					cancellationToken.ThrowIfCancellationRequested();
					string filePath = GetFilePath(current2.Location);
					if (string.IsNullOrEmpty(filePath) || (!string.IsNullOrEmpty(parameters.FilePath) && !string.Equals(filePath, parameters.FilePath, StringComparison.OrdinalIgnoreCase)) || (!string.IsNullOrEmpty(parameters.FolderPath) && !filePath.StartsWith(parameters.FolderPath, StringComparison.OrdinalIgnoreCase)))
					{
						continue;
					}
					string text = MapSeverity(current2.Severity);
					if (severityFilter != null && !severityFilter.Contains(text))
					{
						continue;
					}
					string source = GetSource(current2);
					if (areaFilter == null || (source != null && areaFilter.Contains(source)))
					{
						if (string.Equals(text, "error", StringComparison.Ordinal))
						{
							errorCount++;
						}
						if (results.Count >= limit)
						{
							truncated = true;
						}
						else
						{
							results.Add(CreateDiagnosticInfo(current2, filePath, text, source));
						}
					}
				}
			}
			return new DiagnosticsResult
			{
				Succeeded = true,
				Message = (truncated ? $"Truncated to first {limit} diagnostics." : null),
				Diagnostics = results,
				ErrorCount = errorCount,
				Truncated = truncated
			};
		}

		public void Dispose()
		{
			disposed = true;
		}

		private static DiagnosticsResult CreateEmptyResult()
		{
			return new DiagnosticsResult
			{
				Succeeded = true,
				Diagnostics = Array.Empty<DiagnosticInfo>()
			};
		}

		private static HashSet<string>? CreateSeverityFilter(IReadOnlyList<string>? severities)
		{
			if (severities == null || severities.Count == 0)
			{
				return null;
			}
			return new HashSet<string>(severities.Select((string s) => s.ToLowerInvariant()), StringComparer.OrdinalIgnoreCase);
		}

		private static HashSet<string>? CreateAreaFilter(IReadOnlyList<string>? areas)
		{
			if (areas == null || areas.Count == 0)
			{
				return null;
			}
			return new HashSet<string>(areas, StringComparer.OrdinalIgnoreCase);
		}

		private static string? GetFilePath(Location location)
		{
			if (location == null || location.Kind != LocationKind.SourceFile)
			{
				return null;
			}
			return location.SourceTree?.FilePath;
		}

		private static string MapSeverity(DiagnosticSeverity severity)
		{
			return severity switch
			{
				DiagnosticSeverity.Error => "error", 
				DiagnosticSeverity.Warning => "warning", 
				DiagnosticSeverity.Info => "info", 
				DiagnosticSeverity.Hidden => "hint", 
				_ => "info", 
			};
		}

		private static string? GetSource(Diagnostic diagnostic)
		{
			if (diagnostic.Descriptor?.Category != null)
			{
				return diagnostic.Descriptor.Category;
			}
			string id = diagnostic.Id;
			if (!string.IsNullOrEmpty(id))
			{
				int i;
				for (i = 0; i < id.Length && char.IsLetter(id[i]); i++)
				{
				}
				if (i > 0)
				{
					return id.Substring(0, i);
				}
			}
			return null;
		}

		private static DiagnosticInfo CreateDiagnosticInfo(Diagnostic diagnostic, string filePath, string severity, string? source)
		{
			FileLinePositionSpan lineSpan = diagnostic.Location.GetLineSpan();
			return new DiagnosticInfo
			{
				File = filePath,
				Range = new DiagnosticRange
				{
					StartLine = lineSpan.StartLinePosition.Line + 1,
					StartColumn = lineSpan.StartLinePosition.Character + 1,
					EndLine = lineSpan.EndLinePosition.Line + 1,
					EndColumn = lineSpan.EndLinePosition.Character + 1
				},
				Severity = severity,
				Message = diagnostic.GetMessage(),
				Source = source,
				Code = diagnostic.Id
			};
		}

		private void ThrowIfDisposed()
		{
			if (disposed)
			{
				throw new ObjectDisposedException("DiagnosticsService");
			}
		}
	}
}
namespace Microsoft.Dynamics.Nav.LanguageModelTools.Build
{
	public sealed class BuildDiagnostic
	{
		public string Severity { get; set; } = string.Empty;


		public string Code { get; set; } = string.Empty;


		public string Description { get; set; } = string.Empty;


		public string? Location { get; set; }
	}
	public sealed class BuildParameters
	{
		[Description("Build scope - 'current' for active project only, 'all' for workspace with full dependency tree.")]
		public BuildScope Scope { get; set; }

		[Description("Optional AL project folder path to build. If not specified, builds the default project.")]
		public string? ProjectPath { get; set; }

		[Description("Optional output path for the generated .app file. If not specified, uses the project's default output folder.")]
		public string? OutputPath { get; set; }

		[Description("Set to true to return only error diagnostics (filters out warnings, info, hints).")]
		public bool OnlyErrors { get; set; }

		[Description("Maximum number of diagnostics to return. Default: 100.")]
		public int? MaxDiagnostics { get; set; }

		[Description("Set to true to enable code analysis with analyzers (CodeCop, AppSourceCop, etc.). When not specified, uses the server's startup configuration.")]
		public bool? EnableCodeAnalysis { get; set; }

		[Description("Code analyzers to use. Well-known values: '${CodeCop}', '${AppSourceCop}', '${PerTenantExtensionCop}', '${UICop}'. When not specified, uses the server's startup configuration.")]
		public string[]? CodeAnalyzers { get; set; }
	}
	public enum BuildScope
	{
		[Description("Build only the current project")]
		Current,
		[Description("Build all projects including full dependency tree")]
		All
	}
	public delegate Task<(bool Success, ImmutableArray<(ProjectId Id, string OutputPath)> Projects)> BuildWorkspaceDelegate(BuildParameters? parameters, CancellationToken cancellationToken);
	public class BuildService
	{
		private const string ToolName = "al_build";

		private readonly BuildWorkspaceDelegate buildWorkspace;

		public BuildWorkspaceDelegate BuildDelegate => buildWorkspace;

		public BuildService(BuildWorkspaceDelegate buildWorkspace)
		{
			this.buildWorkspace = buildWorkspace ?? throw new ArgumentNullException("buildWorkspace");
		}

		public virtual async Task<ToolResponse> BuildAsync(BuildParameters? parameters, CancellationToken cancellationToken)
		{
			if (parameters == null)
			{
				parameters = new BuildParameters();
			}
			try
			{
				var (flag, immutableArray) = await buildWorkspace(parameters, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
				if (!flag)
				{
					return ResponseEnricher.CreateErrorResponse("al_build", ToolErrorCode.CompilationFailed, ToolErrorResources.BuildFailed);
				}
				string text = (immutableArray.IsDefaultOrEmpty ? null : immutableArray[immutableArray.Length - 1].Item2);
				return new ToolResponse
				{
					Succeeded = true,
					Message = (string.IsNullOrEmpty(text) ? ToolErrorResources.BuildSucceeded : ("Package generated: " + text)),
					NextSteps = WorkflowEngine.GetNextStepStrings("al_build")
				};
			}
			catch (OperationCanceledException)
			{
				return ResponseEnricher.CreateErrorResponse("al_build", ToolErrorCode.CompilationFailed, ToolErrorResources.BuildCancelled);
			}
			catch (Exception ex2)
			{
				return ResponseEnricher.CreateErrorResponse("al_build", ToolErrorCode.UnknownError, ex2.Message);
			}
		}
	}
}
