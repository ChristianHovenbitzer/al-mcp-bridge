using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.Deployment;

public static class PublishEnvironmentExtensions
{
	public static string DeploymentSuffix(this PublishEnvironment env)
	{
		return env switch
		{
			PublishEnvironment.Production => string.Empty, 
			PublishEnvironment.Tie => "-tie", 
			PublishEnvironment.ServicesTie => "-servicestie", 
			_ => throw ExceptionUtilities.UnexpectedValue(env), 
		};
	}
}
