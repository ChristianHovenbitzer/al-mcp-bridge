using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Microsoft.Dynamics.Nav.Deployment;

internal static class UriHelper
{
	public static bool TryParseAbsoluteUri(string url, out Uri? uri)
	{
		uri = null;
		if (string.IsNullOrEmpty(url))
		{
			return false;
		}
		if (!Uri.TryCreate(url, UriKind.Absolute, out uri))
		{
			return false;
		}
		if (!string.Equals(uri.Scheme, Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase) && !string.Equals(uri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase))
		{
			return false;
		}
		return true;
	}

	public static bool TryCreateBaseServerAddress(string scheme, string host, int port, out Uri uri)
	{
		if (!Uri.TryCreate(scheme + Uri.SchemeDelimiter + host + ":" + port.ToString(CultureInfo.InvariantCulture) + "/", UriKind.Absolute, out uri))
		{
			return false;
		}
		return true;
	}

	public static string CreateQueryString(IDictionary<string, string> nameValueCollection, bool prependQuestionMark = true)
	{
		if (nameValueCollection == null || nameValueCollection.Count == 0)
		{
			return string.Empty;
		}
		IEnumerable<string> values = nameValueCollection.Select<KeyValuePair<string, string>, string>((KeyValuePair<string, string> kv) => FormattableString.Invariant($"{Uri.EscapeDataString(kv.Key)}={Uri.EscapeDataString(kv.Value)}"));
		string text = string.Join("&", values);
		if (!prependQuestionMark)
		{
			return text;
		}
		return "?" + text;
	}

	internal static int GetPort(this Uri serverUri, int? connectionOptionsPort)
	{
		if (connectionOptionsPort.HasValue)
		{
			return connectionOptionsPort.Value;
		}
		if (!serverUri.IsDefaultPort)
		{
			return serverUri.Port;
		}
		return 7049;
	}

	internal static Uri? CreateBaseClientUri(ConnectionOptions connectionOptions, IEmitLogger logger)
	{
		if (string.IsNullOrEmpty(connectionOptions.Server) || !TryParseAbsoluteUri(connectionOptions.Server, out Uri uri))
		{
			logger.Error(DeploymentResources.MalformedAbsoluteUriError, connectionOptions.Server ?? string.Empty);
			return null;
		}
		if (uri == null || string.IsNullOrEmpty(uri.Scheme) || !TryCreateBaseServerAddress(uri.Scheme, uri.Host, uri.GetPort(connectionOptions.Port), out Uri uri2))
		{
			logger.Error(DeploymentResources.MalformedServerAddressError, connectionOptions.Server);
			return null;
		}
		return new Uri(uri2, Uri.EscapeDataString(connectionOptions.ServerInstance) + "/");
	}
}
