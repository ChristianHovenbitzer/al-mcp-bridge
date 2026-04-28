namespace Microsoft.Dynamics.Nav.Deployment;

public class PublishOptions : ServerConnectionConfiguration
{
	public virtual string? Directory { get; set; }

	public virtual string? PackageFileName { get; set; }

	public virtual bool NoCache { get; set; }

	public virtual SchemaUpdateMode SchemaUpdateMode { get; set; }

	public virtual bool IsRad { get; set; }

	public virtual DependencyPublishingOption DependencyPublishingOption { get; set; }

	public virtual bool ForceUpgrade { get; set; }

	public virtual bool DisableInstallDebugging { get; set; }
}
