using System;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.Deployment.ApiClients;

internal static class DevApiFeatureExtensions
{
	public static Version RequiredVersion(this DevApiFeature feature)
	{
		return feature switch
		{
			DevApiFeature.GetSourceCode => DevApiVersions.Version2_0, 
			DevApiFeature.Rad => DevApiVersions.Version3_0, 
			DevApiFeature.ProjectReferencePublishing => DevApiVersions.Version4_0, 
			DevApiFeature.NamePublisherChanging => DevApiVersions.Version5_0, 
			DevApiFeature.NetCoreSignalR => DevApiVersions.Version6_0, 
			DevApiFeature.TestRunning => DevApiVersions.Version7_0, 
			_ => throw ExceptionUtilities.UnexpectedValue(feature), 
		};
	}
}
