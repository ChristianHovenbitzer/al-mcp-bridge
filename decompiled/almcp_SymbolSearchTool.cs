using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Dynamics.BusinessCentral.ALMcp.Services;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.LanguageModelTools.SymbolSearch;
using Microsoft.Extensions.DependencyInjection;
using ModelContextProtocol.Server;

namespace Microsoft.Dynamics.BusinessCentral.ALMcp.Tools;

[McpServerToolType]
public class SymbolSearchTool
{
	private const string ToolName = "al_symbolsearch";

	[McpServerTool(Name = "al_symbolsearch")]
	[Description("Searches AL symbols (tables, codeunits, pages, fields, methods, etc.) across project and dependencies. Parameters: query (required) - text to match in names/docs, '*' matches all. filters.kinds=['Table','Codeunit','Page','Report','Enum','Interface'] for object types. filters.memberKinds=['Field','Method','Key','Action','Trigger'] for member types. filters.objectName to search within specific object (e.g., 'Customer'). filters.namespace restricts to namespace. filters.access=['Public','Internal']. filters.obsoleteState=['No','Pending','Removed']. filters.match='name'|'doc'|'all'. filters.scope='project'|'dependencies'|'all'. filters.limit (max 200). Returns: Array of symbols with name, kind, container, signature, docSummary, path. Search patterns: (1) Find objects by name: query='Customer', kinds=['Table'] - returns matching tables. (2) List all members of an object: query='*', objectName='Customer', memberKinds=['Field'] - lists ALL fields in Customer table. (3) Search within an object: query='Balance', objectName='Customer', memberKinds=['Field'] - finds fields containing 'Balance' in Customer table. Note: objectName filters members inside a named object; use query with kinds to find the object itself. Use cases: Discover APIs, find fields, explore dependencies without source.")]
	public static async Task<SymbolSearchResult> SearchAsync(McpServer server, SymbolSearchParameters? parameters = null, CancellationToken cancellationToken = default(CancellationToken))
	{
		SymbolSearchParameters parameters2 = parameters;
		if (server == null)
		{
			throw new ArgumentNullException("server");
		}
		ISymbolSearchToolService service = server.Services?.GetService<ISymbolSearchToolService>();
		if (service == null)
		{
			throw new InvalidOperationException("Symbol search service is not available.");
		}
		McpTelemetryService telemetry = server.Services?.GetService<McpTelemetryService>();
		return await telemetry.TrackIfAvailableAsync("al_symbolsearch", () => service.SearchAsync(parameters2 ?? new SymbolSearchParameters(), cancellationToken)).ConfigureAwait(continueOnCapturedContext: false);
	}
}
Latest version is '10.0.0.8330' (yours is '8.2.0.7535-95108c96')
