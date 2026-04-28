using System;
using System.Globalization;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Deployment.ApiClients;

[Serializable]
public class ExtensionTooNewException : IncompatibleExtensionException
{
	internal ExtensionTooNewException(ExtensionInfo extensionInfo, ServerInfo serverInfo)
		: this(string.Format(CultureInfo.InvariantCulture, DeploymentResources.ExtensionTooNewError, serverInfo.ToString(), extensionInfo.ToString()))
	{
	}

	public ExtensionTooNewException()
	{
	}

	public ExtensionTooNewException(string message)
		: base(message)
	{
	}

	public ExtensionTooNewException(string message, Exception inner)
		: base(message, inner)
	{
	}

	protected ExtensionTooNewException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
