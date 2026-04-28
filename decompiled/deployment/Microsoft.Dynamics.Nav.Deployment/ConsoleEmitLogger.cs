using System;

namespace Microsoft.Dynamics.Nav.Deployment;

internal class ConsoleEmitLogger : EmitLogger
{
	protected override void Send(string message)
	{
		Console.WriteLine(message);
	}
}
