using System;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Deployment;

public class UserSetupException : Exception
{
	public UserSetupException()
	{
	}

	public UserSetupException(string message)
		: base(message)
	{
	}

	public UserSetupException(string message, Exception inner)
		: base(message, inner)
	{
	}

	protected UserSetupException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
