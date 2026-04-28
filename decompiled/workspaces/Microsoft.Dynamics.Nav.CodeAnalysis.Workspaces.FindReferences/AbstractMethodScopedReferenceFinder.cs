namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.FindReferences;

internal abstract class AbstractMethodScopedReferenceFinder<TSymbol> : AbstractMemberScopedReferenceFinder<TSymbol, IMethodSymbol> where TSymbol : ISymbol
{
}
