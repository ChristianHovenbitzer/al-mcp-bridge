using System.IO;
using System.Text;
using Microsoft.Dynamics.Nav.CodeAnalysis.SymbolReference;
using Newtonsoft.Json;

namespace Microsoft.Dynamics.Nav.Deployment;

internal static class StringSerializerHelper
{
	internal static string SerializePayload<T>(T? data, JsonSerializerSettings? settings = null) where T : class
	{
		if (data == null)
		{
			return string.Empty;
		}
		if (settings == null)
		{
			settings = SymbolReferenceJsonWriter.StandardSerializerSettings;
		}
		StringBuilder stringBuilder = new StringBuilder();
		using (StringWriter textWriter = new StringWriter(stringBuilder))
		{
			JsonSerializer.Create(settings).Serialize(textWriter, data);
		}
		return stringBuilder.ToString();
	}
}
