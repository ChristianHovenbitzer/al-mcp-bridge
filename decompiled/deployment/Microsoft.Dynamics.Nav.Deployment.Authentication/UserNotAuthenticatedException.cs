using System;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Deployment.Authentication;

[Serializable]
public class UserNotAuthenticatedException : Exception
{
	public UserNotAuthenticatedException()
	{
	}

	public UserNotAuthenticatedException(string message)
		: base(message)
	{
	}

	public UserNotAuthenticatedException(string message, Exception inner)
		: base(message, inner)
	{
	}

	protected UserNotAuthenticatedException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
