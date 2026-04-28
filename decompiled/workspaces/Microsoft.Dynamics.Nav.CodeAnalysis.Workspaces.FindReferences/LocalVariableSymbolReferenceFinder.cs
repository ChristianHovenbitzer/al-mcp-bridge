namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.FindReferences;

internal class LocalVariableSymbolReferenceFinder : AbstractMethodScopedReferenceFinder<IVariableSymbol>
{
	protected override bool CanFind(IVariableSymbol symbol)
	{
		return symbol.Kind == SymbolKind.LocalVariable;
	}
}
