using System;
using System.Globalization;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Deployment.ApiClients;

[Serializable]
public class ExtensionTooOldException : IncompatibleExtensionException
{
	internal ExtensionTooOldException(ExtensionInfo extensionInfo, ServerInfo serverInfo)
		: this(string.Format(CultureInfo.InvariantCulture, DeploymentResources.ExtensionTooOldError, serverInfo.ToString(), extensionInfo.ToString()))
	{
	}

	public ExtensionTooOldException()
	{
	}

	public ExtensionTooOldException(string message)
		: base(message)
	{
	}

	public ExtensionTooOldException(string message, Exception inner)
		: base(message, inner)
	{
	}

	protected ExtensionTooOldException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
