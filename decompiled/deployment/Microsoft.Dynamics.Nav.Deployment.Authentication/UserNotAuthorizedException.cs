using System;

namespace Microsoft.Dynamics.Nav.Deployment.Authentication;

public class UserNotAuthorizedException : Exception
{
	public UserNotAuthorizedException()
	{
	}

	public UserNotAuthorizedException(Exception inner)
		: base(string.Empty, inner)
	{
	}
}
