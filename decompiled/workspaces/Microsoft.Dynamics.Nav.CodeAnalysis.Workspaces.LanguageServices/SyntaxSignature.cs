using System.Collections.Generic;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.LanguageServices;

internal class SyntaxSignature
{
	public SyntaxKind KeywordKind { get; }

	public IReadOnlyList<SyntaxNodeSlotDefinition> Parameters { get; }

	public IReadOnlyList<SyntaxKind> Separators { get; }

	public SyntaxNodeDefinition Definition { get; }

	public SyntaxSignature(SyntaxNodeDefinition definition, SyntaxKind keywordKind, IReadOnlyList<SyntaxNodeSlotDefinition> parameters, IReadOnlyList<SyntaxKind> separators)
	{
		DebugAssertHelper.Assert(separators.Count == parameters.Count - 1 || (separators.Count == 0 && parameters.Count == 0), "Wrong number of separators");
		KeywordKind = keywordKind;
		Parameters = parameters;
		Separators = separators;
		Definition = definition;
	}
}
