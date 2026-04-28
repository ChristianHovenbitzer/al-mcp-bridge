using System;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.Deployment.ApiClients;

internal static class SnapshotApiFeatureExtensions
{
	public static Version RequiredVersion(this SnapshotApiFeature feature)
	{
		return feature switch
		{
			SnapshotApiFeature.DownloadSymbols => SnapshotApiVersions.Version1_0, 
			SnapshotApiFeature.NamePublisherChanging => SnapshotApiVersions.Version2_0, 
			SnapshotApiFeature.SampleProfiling => SnapshotApiVersions.Version3_0, 
			_ => throw ExceptionUtilities.UnexpectedValue(feature), 
		};
	}
}
