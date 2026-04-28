using System.CommandLine;
using System.Net;
using System.Threading.Tasks;

namespace Microsoft.Dynamics.Nav.Tool;

internal static class Program
{
	private static async Task<int> Main(string[] args)
	{
		ServicePointManager.CheckCertificateRevocationList = true;
		return new RootCommand("Microsoft AL CLI tools")
		{
			CompileCommand.Instance,
			WorkspaceCommand.Instance,
			GetPackageManifestCommand.Instance,
			CreateSymbolPackageCommand.Instance,
			GetLatestSupportedRuntimeVersionCommand.Instance,
			IsSymbolOnlyCommand.Instance,
			IsRuntimePackageCommand.Instance,
			LaunchMcpServerCommand.Instance,
			PublishAppCommand.Instance,
			AuthCommand.Instance
		}.Parse(args).Invoke();
	}
}
Latest version is '10.0.0.8330' (yours is '8.2.0.7535-95108c96')
