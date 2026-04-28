using System;

namespace Microsoft.Dynamics.Nav.DebuggerService;

[Flags]
public enum SnapshotDebuggingExecutionContextWrapper
{
	None = 0,
	Debugging = 1,
	Profiling = 2
}
