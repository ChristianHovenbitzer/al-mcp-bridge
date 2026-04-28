using System.Threading;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces;

internal static class SemanticModelExtensions
{
	internal static ITypeSymbol GetEnclosingNamedType(this SemanticModel semanticModel, int position, CancellationToken cancellationToken)
	{
		return semanticModel.GetEnclosingSymbol<ITypeSymbol>(position, cancellationToken);
	}

	internal static TSymbol GetEnclosingSymbol<TSymbol>(this SemanticModel semanticModel, int position, CancellationToken cancellationToken) where TSymbol : ISymbol
	{
		for (ISymbol symbol = semanticModel.GetEnclosingSymbol(position, cancellationToken); symbol != null; symbol = symbol.ContainingSymbol)
		{
			if (symbol is TSymbol)
			{
				return (TSymbol)symbol;
			}
		}
		return default(TSymbol);
	}
}
