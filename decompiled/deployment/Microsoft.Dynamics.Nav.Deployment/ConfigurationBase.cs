using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.Deployment;

public class ConfigurationBase
{
	public string? Name { get; set; }

	public ConfigurationRequestType Request { get; set; }

	internal int CreateConfigurationIdentifier()
	{
		int hashCode = Request.GetHashCode();
		if (Name == null)
		{
			return hashCode;
		}
		return Hash.Combine(Name.GetHashCode(), hashCode);
	}
}
