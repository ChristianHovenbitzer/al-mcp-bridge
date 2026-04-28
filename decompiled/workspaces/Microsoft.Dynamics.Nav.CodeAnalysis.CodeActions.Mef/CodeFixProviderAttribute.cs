using System;
using System.Composition;
using Microsoft.Dynamics.Nav.CodeAnalysis.CodeFixes;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.CodeActions.Mef;

[MetadataAttribute]
[AttributeUsage(AttributeTargets.Class)]
public sealed class CodeFixProviderAttribute : ExportAttribute
{
	public string ProviderName { get; }

	public CodeFixProviderAttribute(string providerName)
		: base(typeof(CodeFixProvider))
	{
		ProviderName = providerName;
	}
}
