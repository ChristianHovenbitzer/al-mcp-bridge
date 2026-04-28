using System;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Log;

internal sealed class EmptyLogBlock : IDisposable
{
	public static readonly EmptyLogBlock Instance = new EmptyLogBlock();

	public void Dispose()
	{
	}
}
