using System.Collections.Generic;
using System.Net.Http.Headers;
using Microsoft.Dynamics.Nav.CodeAnalysis.Packaging;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.Deployment.Publishing;

internal sealed class PublishResult
{
	public static readonly PublishResult Failure = new PublishResult(success: false, null, null, null, null, 0L);

	public ClientConnectionInfo ClientConnectionInfo { get; }

	public bool Success { get; }

	public long FileSizeBytes { get; }

	public IList<ProjectModelDefinition> PublishedProjectReferences { get; set; }

	public IList<ProjectModelDefinition> PublishedProjectsThatThisProjectDependOn { get; set; }

	public PublishResult(bool success, string tenantId = null, AuthenticationHeaderValue authenticationHeader = null, IList<ProjectModelDefinition> publishedProjectReferences = null, IList<ProjectModelDefinition> publishedProjectsThatThisProjectDependOn = null, long fileSizeBytes = 0L)
	{
		Success = success;
		FileSizeBytes = fileSizeBytes;
		ClientConnectionInfo = new ClientConnectionInfo(tenantId, authenticationHeader);
		PublishedProjectReferences = publishedProjectReferences ?? SpecializedCollections.EmptyList<ProjectModelDefinition>();
		PublishedProjectsThatThisProjectDependOn = publishedProjectsThatThisProjectDependOn ?? SpecializedCollections.EmptyList<ProjectModelDefinition>();
	}
}
