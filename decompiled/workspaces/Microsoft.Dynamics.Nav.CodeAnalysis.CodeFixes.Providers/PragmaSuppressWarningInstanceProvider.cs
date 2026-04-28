using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.Dynamics.Nav.CodeAnalysis.CodeActions.Mef;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.CodeFixes.Providers;

[CodeFixProvider("PragmaSuppressWarningInstanceProvider")]
public class PragmaSuppressWarningInstanceProvider : PragmaSuppressWarningInstanceCodeAction_Base
{
	private static readonly HashSet<ErrorCode> warningsToIgnore = new HashSet<ErrorCode>
	{
		ErrorCode.WRN_EnumIdentifierTooLong,
		ErrorCode.WRN_SortingFieldShouldBePartOfKey
	};

	private static readonly ImmutableHashSet<string> compilerWarningIds = GetWarningIds();

	public override ImmutableHashSet<string> warningIds => compilerWarningIds;

	public override ImmutableArray<string> FixableDiagnosticIds => compilerWarningIds.ToImmutableArray();

	internal static ImmutableHashSet<string> GetWarningIds()
	{
		return (from ErrorCode x in Enum.GetValues(typeof(ErrorCode))
			where ErrorFacts.IsWarning(x) && !ErrorFacts.IsWarningFutureError(x) && !warningsToIgnore.Contains(x)
			select MessageProvider.Instance.GetIdForErrorCode((int)x)).ToImmutableHashSet();
	}
}
