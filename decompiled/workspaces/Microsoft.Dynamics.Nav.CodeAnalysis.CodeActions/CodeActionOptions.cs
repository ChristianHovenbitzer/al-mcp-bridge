using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.CodeActions;

[DataContract]
public readonly struct CodeActionOptions
{
	public static readonly CodeActionOptions Default;

	public bool IsBlocking { get; }

	private CodeActionOptions(bool blocking)
	{
		IsBlocking = blocking;
	}

	public CodeActionOptions WithBlocking(bool blocking)
	{
		return new CodeActionOptions(blocking);
	}
}
