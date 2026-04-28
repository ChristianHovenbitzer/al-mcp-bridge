using System;
using System.Composition;
using Microsoft.Dynamics.Nav.CodeAnalysis.CodeRefactoring;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.CodeActions.Mef;

[MetadataAttribute]
[AttributeUsage(AttributeTargets.Class)]
public sealed class CodeRefactoringProviderAttribute : ExportAttribute
{
	public string ProviderName { get; }

	public CodeRefactoringProviderAttribute(string providerName)
		: base(typeof(CodeRefactoringProvider))
	{
		ProviderName = providerName;
	}
}
