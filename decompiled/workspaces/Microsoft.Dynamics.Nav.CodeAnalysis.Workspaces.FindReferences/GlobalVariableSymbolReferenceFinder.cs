namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.FindReferences;

internal class GlobalVariableSymbolReferenceFinder : AbstractMemberScopedReferenceFinder<IVariableSymbol, IApplicationObjectTypeSymbol>
{
	protected override bool CanFind(IVariableSymbol symbol)
	{
		return symbol.Kind == SymbolKind.GlobalVariable;
	}
}
