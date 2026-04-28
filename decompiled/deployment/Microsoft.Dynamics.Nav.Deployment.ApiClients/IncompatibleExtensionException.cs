using System;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Deployment.ApiClients;

[Serializable]
public abstract class IncompatibleExtensionException : Exception
{
	protected IncompatibleExtensionException()
	{
	}

	protected IncompatibleExtensionException(string message)
		: base(message)
	{
	}

	protected IncompatibleExtensionException(string message, Exception inner)
		: base(message, inner)
	{
	}

	protected IncompatibleExtensionException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
