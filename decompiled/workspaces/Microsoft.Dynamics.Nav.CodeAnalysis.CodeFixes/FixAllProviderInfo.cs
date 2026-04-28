using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.Dynamics.Nav.CodeAnalysis.CodeRefactoring;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.CodeFixes;

internal abstract class FixAllProviderInfo
{
	private class CodeFixerFixAllProviderInfo : FixAllProviderInfo
	{
		private readonly IEnumerable<string> supportedDiagnosticIds;

		public CodeFixerFixAllProviderInfo(FixAllProvider fixAllProvider, IEnumerable<string> supportedDiagnosticIds, ImmutableArray<FixAllScope> supportedScopes)
			: base(fixAllProvider, supportedScopes)
		{
			this.supportedDiagnosticIds = supportedDiagnosticIds;
		}

		public override bool CanBeFixed(Diagnostic diagnostic)
		{
			return supportedDiagnosticIds.Contains(diagnostic.Id);
		}
	}

	private class CodeRefactoringFixAllProviderInfo : FixAllProviderInfo
	{
		public CodeRefactoringFixAllProviderInfo(FixAllProvider fixAllProvider, ImmutableArray<FixAllScope> supportedScopes)
			: base(fixAllProvider, supportedScopes)
		{
		}

		public override bool CanBeFixed(Diagnostic diagnostic)
		{
			throw ExceptionUtilities.Unreachable;
		}
	}

	public readonly FixAllProvider FixAllProvider;

	public readonly ImmutableArray<FixAllScope> SupportedScopes;

	private FixAllProviderInfo(FixAllProvider fixAllProvider, ImmutableArray<FixAllScope> supportedScopes)
	{
		FixAllProvider = fixAllProvider;
		SupportedScopes = supportedScopes;
	}

	public static FixAllProviderInfo? Create(object provider)
	{
		if (provider is CodeFixProvider provider2)
		{
			return CreateWithCodeFixer(provider2);
		}
		if (provider is CodeRefactoringProvider provider3)
		{
			return CreateWithCodeRefactoring(provider3);
		}
		return null;
	}

	private static FixAllProviderInfo? CreateWithCodeFixer(CodeFixProvider provider)
	{
		FixAllProvider fixAllProvider = provider.GetFixAllProvider();
		if (fixAllProvider == null)
		{
			return null;
		}
		IEnumerable<string> supportedFixAllDiagnosticIds = fixAllProvider.GetSupportedFixAllDiagnosticIds(provider);
		if (supportedFixAllDiagnosticIds == null || supportedFixAllDiagnosticIds.IsEmpty())
		{
			return null;
		}
		ImmutableArray<FixAllScope> supportedScopes = fixAllProvider.GetSupportedFixAllScopes().ToImmutableArrayOrEmpty();
		if (supportedScopes.IsEmpty)
		{
			return null;
		}
		return new CodeFixerFixAllProviderInfo(fixAllProvider, supportedFixAllDiagnosticIds, supportedScopes);
	}

	private static FixAllProviderInfo? CreateWithCodeRefactoring(CodeRefactoringProvider provider)
	{
		FixAllProvider fixAllProvider = provider.GetFixAllProvider();
		if (fixAllProvider == null)
		{
			return null;
		}
		ImmutableArray<FixAllScope> supportedScopes = fixAllProvider.GetSupportedFixAllScopes().ToImmutableArrayOrEmpty();
		if (supportedScopes.IsEmpty)
		{
			return null;
		}
		return new CodeRefactoringFixAllProviderInfo(fixAllProvider, supportedScopes);
	}

	public abstract bool CanBeFixed(Diagnostic diagnostic);
}
