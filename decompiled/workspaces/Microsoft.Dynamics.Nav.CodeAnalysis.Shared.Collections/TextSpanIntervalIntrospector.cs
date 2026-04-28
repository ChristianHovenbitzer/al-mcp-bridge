namespace Microsoft.Dynamics.Nav.CodeAnalysis.Shared.Collections;

internal class TextSpanIntervalIntrospector : IIntervalIntrospector<TextSpan>
{
	public static readonly IIntervalIntrospector<TextSpan> Instance = new TextSpanIntervalIntrospector();

	private TextSpanIntervalIntrospector()
	{
	}

	public int GetStart(TextSpan value)
	{
		return value.Start;
	}

	public int GetLength(TextSpan value)
	{
		return value.Length;
	}
}
