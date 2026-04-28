using System;
using Newtonsoft.Json;

namespace Microsoft.Dynamics.Nav.Deployment.Authentication;

public class AuthenticationMethodJsonConverter : JsonConverter
{
	public override bool CanConvert(Type objectType)
	{
		return objectType.Equals(typeof(AuthenticationMethod));
	}

	public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		if (Enum.TryParse<AuthenticationMethod>(reader.Value as string, ignoreCase: true, out var result))
		{
			return result;
		}
		throw new UserSetupException(DeploymentResources.WrongAuthenticationMethod);
	}

	public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
	{
		writer.WriteValue(value.ToString());
	}
}
