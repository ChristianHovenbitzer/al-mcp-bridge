namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces;

internal interface ITextVersionable
{
	bool TryGetTextVersion(out VersionStamp version);
}
