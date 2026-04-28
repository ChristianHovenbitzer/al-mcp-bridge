using System.Threading;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Log;

internal interface ILogger
{
	bool IsEnabled(FunctionId functionId);

	void Log(FunctionId functionId, LogMessage logMessage);

	void LogBlockStart(FunctionId functionId, LogMessage logMessage, int uniquePairId, CancellationToken cancellationToken);

	void LogBlockEnd(FunctionId functionId, LogMessage logMessage, int uniquePairId, int delta, CancellationToken cancellationToken);
}
