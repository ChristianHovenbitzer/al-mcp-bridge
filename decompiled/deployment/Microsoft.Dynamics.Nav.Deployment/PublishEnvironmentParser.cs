using System;

namespace Microsoft.Dynamics.Nav.Deployment;

public static class PublishEnvironmentParser
{
	public static PublishEnvironment Parse(string s)
	{
		PublishEnvironment result = PublishEnvironment.Production;
		if (!string.IsNullOrEmpty(s))
		{
			Enum.TryParse<PublishEnvironment>(s, ignoreCase: true, out result);
		}
		return result;
	}
}
