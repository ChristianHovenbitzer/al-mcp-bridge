using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Versioning;
using Microsoft.Dynamics.Nav.CodeAnalysis;
using Microsoft.Dynamics.Nav.CodeAnalysis.Telemetry;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Log;
using Microsoft.Dynamics.Nav.EditorServices.Protocol;
using Microsoft.Dynamics.Nav.EditorServices.Protocol.Channel;
using Microsoft.Dynamics.Nav.EditorServices.Protocol.LanguageServer;
using Microsoft.Dynamics.Nav.EditorServices.Protocol.MessageProtocol;

[assembly: CompilationRelaxations(8)]
[assembly: RuntimeCompatibility(WrapNonExceptionThrows = true)]
[assembly: Debuggable(DebuggableAttribute.DebuggingModes.IgnoreSymbolStoreSequencePoints)]
[assembly: TargetFramework(".NETCoreApp,Version=v8.0", FrameworkDisplayName = ".NET 8.0")]
[assembly: AssemblyCompany("Microsoft")]
[assembly: AssemblyConfiguration("Release")]
[assembly: AssemblyCopyright("¸ Microsoft Corporation. All rights reserved.")]
[assembly: AssemblyFileVersion("17.0.34.45391")]
[assembly: AssemblyInformationalVersion("17.0.34.45391+89ddc161d3e4421fa7ecef442abf29ca6e6ebfba")]
[assembly: AssemblyProduct("Microsoft.Dynamics.Nav.EditorServices.Host")]
[assembly: AssemblyTitle("Microsoft.Dynamics.Nav.EditorServices.Host")]
[assembly: AssemblyVersion("17.0.34.45391")]
[module: RefSafetyRules(11)]
namespace Prod.EditorServices.Host;

public class Program
{
	private const string WaitForDebuggerArgument = "/waitForDebugger";

	private const string StartDebuggingArgument = "/startDebugging";

	private const string LogPathArgument = "/logPath:";

	private const string LogLevelArgument = "/logLevel:";

	private const string EnvArgument = "/env:";

	private const string BrowserArgument = "/browser:";

	private const string IncognitoArgument = "/incognito:";

	private const string ProjectRootArgument = "/projectRoot:";

	private const string InputPipeHandleArgument = "/in:";

	private const string OutputPipeHandleArgument = "/out:";

	private const string DisableTelemetryArgument = "/disableTelemetry";

	private const string TelemetryLevelArgument = "/telemetryLevel:";

	private const string SessionIdArgument = "/sessionId:";

	private const string delayAfterLastDocumentChangeArgument = "/delayAfterLastDocumentChange:";

	private const string delayAfterLastProjectChangeArgument = "/delayAfterLastProjectChange:";

	private const string InlayHintsParameterNamesArgument = "/inlayHintsParameterNames:";

	private const string inlayHintsFunctionReturnTypesArgument = "/inlayHintsFunctionReturnTypes:";

	private const string semanticFoldingArgument = "/semanticFolding:";

	private const string extendGoToSymbolInWorkspaceArgument = "/extendGoToSymbolInWorkspace:";

	private const string extendGoToSymbolInWorkspaceResultLimitArgument = "/extendGoToSymbolInWorkspaceResultLimit:";

	private const string extendGoToSymbolInWorkspaceIncludeSymbolFilesArgument = "/extendGoToSymbolInWorkspaceIncludeSymbolFiles:";

	private const string testCoverageCachePathArgument = "/testCoverageCachePath:";

	private const char argumentSplitter = ',';

	public static void Main(string[] args)
	{
		AppContext.SetSwitch("System.Net.Http.UsePortInSpn", isEnabled: true);
		LogLevel result = LogLevel.Normal;
		string logPath = null;
		EnvironmentInfo environmentInfo = null;
		BrowserInfo browserInfo = null;
		string projectRoot = null;
		bool flag = false;
		bool enableTelemetry = true;
		string sessionId = null;
		TimeSpan? delayAfterLastDocumentChange = null;
		TimeSpan? delayAfterLastProjectChange = null;
		TelemetryLevel result2 = TelemetryLevel.All;
		bool result3 = false;
		bool result4 = false;
		bool result5 = false;
		bool result6 = false;
		bool result7 = false;
		int result8 = 0;
		string testCoverageCachePath = null;
		ProtocolConfiguration protocolConfiguration = new ProtocolConfiguration();
		if (args != null)
		{
			IList<string> list = args.ToList();
			if (args.Length == 1)
			{
				list = args[0].Split(new char[1] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
			}
			foreach (string item in list)
			{
				if (item.StartsWith("/waitForDebugger", StringComparison.OrdinalIgnoreCase))
				{
					Debugger.Launch();
				}
				if (item.StartsWith("/startDebugging", StringComparison.OrdinalIgnoreCase))
				{
					flag = true;
				}
				else if (item.StartsWith("/logPath:", StringComparison.OrdinalIgnoreCase))
				{
					logPath = item.Substring("/logPath:".Length).Trim('"');
				}
				else if (item.StartsWith("/logLevel:", StringComparison.OrdinalIgnoreCase))
				{
					Enum.TryParse<LogLevel>(item.Substring("/logLevel:".Length), ignoreCase: true, out result);
				}
				else if (item.StartsWith("/env:", StringComparison.OrdinalIgnoreCase))
				{
					environmentInfo = environmentInfo ?? new EnvironmentInfo();
					environmentInfo.Env = item.Substring("/env:".Length);
				}
				else if (item.StartsWith("/projectRoot:", StringComparison.OrdinalIgnoreCase))
				{
					projectRoot = item.Substring("/projectRoot:".Length);
				}
				else if (item.StartsWith("/in:", StringComparison.OrdinalIgnoreCase))
				{
					protocolConfiguration.InputPipeHandle = item.Substring("/in:".Length);
				}
				else if (item.StartsWith("/out:", StringComparison.OrdinalIgnoreCase))
				{
					protocolConfiguration.OutputPipeHandle = item.Substring("/out:".Length);
				}
				else if (item.StartsWith("/disableTelemetry", StringComparison.OrdinalIgnoreCase))
				{
					enableTelemetry = false;
				}
				else if (item.StartsWith("/sessionId:", StringComparison.OrdinalIgnoreCase))
				{
					sessionId = item.Substring("/sessionId:".Length);
				}
				else if (item.StartsWith("/logLevel:", StringComparison.OrdinalIgnoreCase))
				{
					Enum.TryParse<LogLevel>(item.Substring("/logLevel:".Length), ignoreCase: true, out result);
				}
				else if (item.StartsWith("/telemetryLevel:", StringComparison.OrdinalIgnoreCase))
				{
					Enum.TryParse<TelemetryLevel>(item.Substring("/telemetryLevel:".Length), ignoreCase: true, out result2);
				}
				else if (item.StartsWith("/browser:", StringComparison.OrdinalIgnoreCase))
				{
					browserInfo = browserInfo ?? new BrowserInfo();
					Enum.TryParse<Browser>(item.Substring("/browser:".Length), ignoreCase: true, out var result9);
					browserInfo.Browser = result9;
				}
				else if (item.StartsWith("/incognito:", StringComparison.OrdinalIgnoreCase))
				{
					browserInfo = browserInfo ?? new BrowserInfo();
					bool.TryParse(item.Substring("/incognito:".Length), out var result10);
					browserInfo.Incognito = result10;
				}
				else if (item.StartsWith("/delayAfterLastDocumentChange:", StringComparison.OrdinalIgnoreCase))
				{
					if (int.TryParse(item.Substring("/delayAfterLastDocumentChange:".Length), out var result11))
					{
						delayAfterLastDocumentChange = TimeSpan.FromMilliseconds(result11);
					}
				}
				else if (item.StartsWith("/delayAfterLastProjectChange:", StringComparison.OrdinalIgnoreCase))
				{
					if (int.TryParse(item.Substring("/delayAfterLastProjectChange:".Length), out var result12))
					{
						delayAfterLastProjectChange = TimeSpan.FromMilliseconds(result12);
					}
				}
				else if (item.StartsWith("/inlayHintsParameterNames:", StringComparison.OrdinalIgnoreCase))
				{
					if (!bool.TryParse(item.Substring("/inlayHintsParameterNames:".Length), out result3))
					{
						result3 = false;
					}
				}
				else if (item.StartsWith("/inlayHintsFunctionReturnTypes:", StringComparison.OrdinalIgnoreCase))
				{
					if (!bool.TryParse(item.Substring("/inlayHintsFunctionReturnTypes:".Length), out result4))
					{
						result4 = false;
					}
				}
				else if (item.StartsWith("/semanticFolding:", StringComparison.OrdinalIgnoreCase))
				{
					if (!bool.TryParse(item.Substring("/semanticFolding:".Length), out result5))
					{
						result5 = false;
					}
				}
				else if (item.StartsWith("/extendGoToSymbolInWorkspace:", StringComparison.OrdinalIgnoreCase))
				{
					if (!bool.TryParse(item.Substring("/extendGoToSymbolInWorkspace:".Length), out result6))
					{
						result6 = false;
					}
				}
				else if (item.StartsWith("/extendGoToSymbolInWorkspaceResultLimit:", StringComparison.OrdinalIgnoreCase))
				{
					if (!int.TryParse(item.Substring("/extendGoToSymbolInWorkspaceResultLimit:".Length), out result8))
					{
						result8 = 0;
					}
				}
				else if (item.StartsWith("/extendGoToSymbolInWorkspaceIncludeSymbolFiles:", StringComparison.OrdinalIgnoreCase))
				{
					if (!bool.TryParse(item.Substring("/extendGoToSymbolInWorkspaceIncludeSymbolFiles:".Length), out result7))
					{
						result7 = false;
					}
				}
				else if (item.StartsWith("/testCoverageCachePath:", StringComparison.OrdinalIgnoreCase))
				{
					testCoverageCachePath = item.Substring("/testCoverageCachePath:".Length).Trim('"');
				}
			}
		}
		ExternalTelemetryLogger.EnableTelemetry = enableTelemetry;
		ExternalTelemetryLogger.TelemetryLevel = result2;
		SetupEnvironment(result, logPath, flag);
		LanguageServerOptions.Init(result, logPath, environmentInfo, projectRoot, browserInfo);
		ExternalTelemetryLogger.InitializeNewSessionAsync(sessionId).GetAwaiter().GetResult();
		ProtocolEndpoint protocolEndpoint = new ProtocolEndpoint();
		MessageProtocolType messageProtocolType = (flag ? MessageProtocolType.DebugAdapter : MessageProtocolType.LanguageServer);
		ChannelBase channel = ChannelFactory.CreateChannel(protocolConfiguration);
		InlayHintOptions inlayHintOptions = new InlayHintOptions
		{
			ParameterNamesEnabled = result3,
			ReturnTypesEnabled = result4
		};
		ExtendGoToSymbolInWorkspaceOptions extendGoToSymbolInWorkspace = new ExtendGoToSymbolInWorkspaceOptions
		{
			Enabled = result6,
			IncludeSymbolFiles = result7,
			ResultLimit = result8
		};
		DiagnosticServiceOptions diagnosticServiceOptions = new DiagnosticServiceOptions(delayAfterLastDocumentChange, delayAfterLastProjectChange);
		ExtensionOptions extensionOptions = new ExtensionOptions
		{
			DiagnosticServiceOptions = diagnosticServiceOptions,
			InlayHintOptions = inlayHintOptions,
			FoldingEnabled = result5,
			ExtendGoToSymbolInWorkspace = extendGoToSymbolInWorkspace,
			TestCoverageCachePath = testCoverageCachePath
		};
		protocolEndpoint.Start(channel, messageProtocolType, extensionOptions).Wait();
		LocalMachineLogger.LogNormal("Editor Services Host started!");
		protocolEndpoint.WaitForExit();
		ExternalTelemetryLogger.Dispose();
		LocalMachineLogger.LogNormal("Editor Services Host exited normally.");
		ServicePointManager.MaxServicePointIdleTime = 10000;
	}

	private static void SetupEnvironment(LogLevel level, string logPath, bool isDebuggerSession)
	{
		ServicePointManager.CheckCertificateRevocationList = true;
		AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
		string logFilePath = logPath ?? (isDebuggerSession ? "DebuggerServices.log" : "EditorServices.log");
		if (level == LogLevel.Debug)
		{
			level = LogLevel.Verbose;
		}
		LocalMachineLogger.SetFileLogger(new FileLogWriter(logFilePath, deleteExisting: false));
		LocalMachineLogger.SetLogLevel(level);
		string location = Assembly.GetExecutingAssembly().Location;
		LocalMachineLogger.LogNormal(FormattableString.Invariant($"Editor Services Host v{FileVersionInfo.GetVersionInfo(location).FileVersion} starting (pid {Process.GetCurrentProcess().Id})..."));
	}

	private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
	{
		LocalMachineLogger.LogException((Exception)e.ExceptionObject);
	}
}
