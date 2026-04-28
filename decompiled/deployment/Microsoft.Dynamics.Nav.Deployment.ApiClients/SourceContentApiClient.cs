using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Threading.Tasks;

namespace Microsoft.Dynamics.Nav.Deployment.ApiClients;

internal class SourceContentApiClient : ApiClient
{
	public SourceContentApiClient(ConnectionOptions options, IEmitLogger logger)
		: base(options, logger)
	{
	}

	public async Task<string> GetSource(int objectType, int objectId)
	{
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		dictionary.Add("type", objectType.ToString(CultureInfo.InvariantCulture));
		dictionary.Add("id", objectId.ToString(CultureInfo.InvariantCulture));
		AddTenantIfNeeded(dictionary);
		AddDeploymentIdIfNeeded(dictionary);
		string url = "dev/sourcecontent" + UriHelper.CreateQueryString(dictionary);
		HttpResponseMessage obj = await (await CreateHttpClient().ConfigureAwait(continueOnCapturedContext: false)).GetAsync(new Uri(url, UriKind.Relative)).ConfigureAwait(continueOnCapturedContext: false);
		obj.EnsureSuccessStatusCode();
		return await obj.Content.ReadAsStringAsync().ConfigureAwait(continueOnCapturedContext: false);
	}
}
