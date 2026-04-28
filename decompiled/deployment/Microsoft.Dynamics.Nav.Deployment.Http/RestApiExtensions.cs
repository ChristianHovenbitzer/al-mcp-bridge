using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.CodeAnalysis;
using Microsoft.Dynamics.Nav.Deployment.Authentication;
using Microsoft.Dynamics.Nav.Deployment.Telemetry;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Dynamics.Nav.Deployment.Http;

internal static class RestApiExtensions
{
	private static readonly Dictionary<string, string> RequestErrorContext = new Dictionary<string, string> { { "request_error", "true" } };

	public static bool IsJson(this HttpResponseMessage message)
	{
		return string.Equals(message.Content.Headers.ContentType?.MediaType, "application/json", StringComparison.OrdinalIgnoreCase);
	}

	public static async Task<T> TryReadAsAsync<T>(this HttpResponseMessage message)
	{
		if (!message.IsJson())
		{
			return default(T);
		}
		string value = await message.Content.ReadAsStringAsync().ConfigureAwait(continueOnCapturedContext: false);
		if (message.IsServerSideError())
		{
			LocalMachineLogger.LogError(value.MarkAsInternal(), RequestErrorContext);
		}
		try
		{
			return JsonConvert.DeserializeObject<T>(value);
		}
		catch (Exception ex)
		{
			LocalMachineLogger.LogException(ex);
			return default(T);
		}
	}

	public static bool IsServerSideError(this HttpResponseMessage message)
	{
		if (message.StatusCode < HttpStatusCode.InternalServerError)
		{
			return message.StatusCode == HttpStatusCode.UnprocessableEntity;
		}
		return true;
	}

	public static HttpResponseMessage Clone(this HttpResponseMessage message, HttpContent content)
	{
		HttpResponseMessage httpResponseMessage = new HttpResponseMessage(message.StatusCode)
		{
			Content = content,
			RequestMessage = message.RequestMessage
		};
		foreach (KeyValuePair<string, IEnumerable<string>> header in message.Headers)
		{
			httpResponseMessage.Headers.TryAddWithoutValidation(header.Key, header.Value);
		}
		httpResponseMessage.Content.Headers.Clear();
		foreach (KeyValuePair<string, IEnumerable<string>> header2 in message.Content.Headers)
		{
			httpResponseMessage.Content.Headers.TryAddWithoutValidation(header2.Key, header2.Value);
		}
		return httpResponseMessage;
	}

	public static async Task<HttpResponseMessage> LogIfResponseIsNotOkOrUnauthorized(this HttpResponseMessage response, Uri resourceUri, IEmitLogger logger)
	{
		HttpResponseMessage returnMessage = response;
		string reason = response.ReasonPhrase;
		if (response.StatusCode == HttpStatusCode.Unauthorized)
		{
			throw new UserNotAuthorizedException();
		}
		if (!response.IsSuccessStatusCode)
		{
			try
			{
				if (response.IsJson())
				{
					string text = await response.Content.ReadAsStringAsync().ConfigureAwait(continueOnCapturedContext: false);
					JObject jObject = JObject.Parse(text);
					if (jObject != null && jObject.TryGetValue("Message", StringComparison.OrdinalIgnoreCase, out JToken value) && value != null)
					{
						reason = value.ToString();
					}
					returnMessage = response.Clone(new StringContent(text));
				}
			}
			catch (JsonReaderException)
			{
			}
			catch (ObjectDisposedException)
			{
			}
			logger.Info(DeploymentResources.HttpError, resourceUri.PathAndQuery, response.StatusCode, reason);
		}
		return returnMessage;
	}
}
