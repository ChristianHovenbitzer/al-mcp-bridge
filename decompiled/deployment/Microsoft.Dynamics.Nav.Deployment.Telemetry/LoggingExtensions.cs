namespace Microsoft.Dynamics.Nav.Deployment.Telemetry;

public static class LoggingExtensions
{
	private const string PrivateInformationTagName = "<pi>";

	private const string EndPrivateInformationTagName = "</pi>";

	private const string InternalInformationTagName = "<ii>";

	private const string EndInternalInformationTagName = "</ii>";

	public static string MarkAsPartnerContent(this string value)
	{
		return value.MarkAsInternal();
	}

	public static string MarkAsCustomerContent(this string value)
	{
		return value.MarkAsPrivate();
	}

	public static string MarkAsCustomerIdentifiableInformation(this string value)
	{
		return value.MarkAsInternal();
	}

	public static string MarkAsTenantId(this string value)
	{
		return value.MarkAsInternal();
	}

	public static string MarkAsEndUserIdentifiableInformation(this string value)
	{
		return value.MarkAsPrivate();
	}

	public static string MarkAsPrivate(this string value)
	{
		if (string.IsNullOrEmpty(value))
		{
			return string.Empty;
		}
		return MarkString(value, "<pi>", "</pi>");
	}

	public static string MarkAsInternal(this string value)
	{
		if (string.IsNullOrEmpty(value))
		{
			return string.Empty;
		}
		return MarkString(value, "<ii>", "</ii>");
	}

	private static string MarkString(string plainString, string markupTagName, string endMarkupTagName)
	{
		if (plainString == null)
		{
			return null;
		}
		return markupTagName + plainString + endMarkupTagName;
	}
}
