using System.Collections.Generic;
using System.Threading;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Options;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Formatting;

internal abstract class TriviaDataWithList : TriviaData
{
	public TriviaDataWithList(OptionSet optionSet, string language)
		: base(optionSet, language)
	{
	}

	public abstract List<SyntaxTrivia> GetTriviaList(CancellationToken cancellationToken);
}
