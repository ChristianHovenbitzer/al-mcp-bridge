using System;
using System.Globalization;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;
using Newtonsoft.Json;

namespace Microsoft.Dynamics.Nav.Deployment.ApiClients;

internal class ServerInfo
{
	[JsonConverter(typeof(VersionConverter))]
	public Version? RuntimeVersion { get; set; }

	[JsonConverter(typeof(VersionConverter))]
	public Version? WebApiVersion { get; set; }

	[JsonConverter(typeof(VersionConverter))]
	public Version? DebuggerVersion { get; set; }

	[JsonProperty]
	public ServerInfoKind Kind { get; set; }

	[JsonProperty]
	public Uri? WebEndpoint { get; set; }

	[JsonProperty]
	public ConnectionOptions? ConnectionOptions { get; set; }

	[JsonIgnore]
	public string WebApiVersionAsString => WebApiVersion?.ToMajorMinorString() ?? string.Empty;

	[JsonIgnore]
	public string DebuggerVersionAsString => DebuggerVersion?.ToMajorMinorString() ?? string.Empty;

	[JsonIgnore]
	public string RuntimeVersionAsString => RuntimeVersion?.ToMajorMinorString() ?? string.Empty;

	public bool Supports(DevApiFeature feature)
	{
		Version version = feature.RequiredVersion();
		return WebApiVersion >= version;
	}

	public bool Supports(SnapshotApiFeature feature)
	{
		Version version = feature.RequiredVersion();
		return WebApiVersion >= version;
	}

	public override string ToString()
	{
		return string.Format(CultureInfo.InvariantCulture, DeploymentResources.ServerInfoFormat, RuntimeVersionAsString, WebApiVersionAsString, DebuggerVersionAsString);
	}

	public Uri? GetWebEndpointUrl()
	{
		if (ConnectionOptions == null)
		{
			return null;
		}
		if (ConnectionOptions.IsOnPremise())
		{
			if (WebEndpoint == null || !ConnectionOptions.UsePublicURLFromServer)
			{
				if (ConnectionOptions.Server == null)
				{
					return WebEndpoint;
				}
				WebEndpoint = BuildWebEndpointUriFromServer();
			}
			else if (!WebEndpoint.IsAbsoluteUri)
			{
				if (ConnectionOptions.IsOnPremiseWithAAD())
				{
					WebEndpoint = new Uri(string.Format(CultureInfo.InvariantCulture, "https://{0}", WebEndpoint.OriginalString));
				}
				else if (ConnectionOptions.Server != null)
				{
					WebEndpoint = new Uri(ConnectionOptions.Server);
				}
				else
				{
					WebEndpoint = new Uri(string.Format(CultureInfo.InvariantCulture, "http://{0}", WebEndpoint.OriginalString));
				}
			}
			return WebEndpoint;
		}
		return CloudTenant.FindFixedWebClientUri(ConnectionOptions.Environment, ConnectionOptions.Tenant, ConnectionOptions.ApplicationFamily, ConnectionOptions.EnvironmentName, ConnectionOptions.DeploymentId);
	}

	private Uri BuildWebEndpointUriFromServer()
	{
		if (WebEndpoint != null)
		{
			Uri uri = new Uri(ConnectionOptions.Server);
			return new UriBuilder(WebEndpoint)
			{
				Host = uri.Host,
				Scheme = uri.Scheme,
				Port = uri.Port
			}.Uri;
		}
		return new Uri(ConnectionOptions.Server);
	}
}
