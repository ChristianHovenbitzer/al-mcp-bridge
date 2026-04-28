using System.Collections.Generic;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.SignatureHelp;

internal class SignatureHelpResult<T>
{
	public IEnumerable<T> Signatures { get; }

	public int ActiveSignature { get; }

	public int ActiveParameter { get; }

	public SignatureHelpResult(IEnumerable<T> signatures, int activeSignature, int activeParameter)
	{
		Signatures = signatures;
		ActiveSignature = activeSignature;
		ActiveParameter = activeParameter;
	}
}
