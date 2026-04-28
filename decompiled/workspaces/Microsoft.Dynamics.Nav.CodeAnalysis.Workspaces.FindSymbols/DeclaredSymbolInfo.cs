using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.FindSymbols;

internal struct DeclaredSymbolInfo
{
	public string Name { get; }

	public string ContainerDisplayName { get; }

	public string FullyQualifiedContainerName { get; }

	public DeclaredSymbolInfoKind Kind { get; }

	public TextSpan Span { get; }

	public ushort ParameterCount { get; }

	public ushort TypeParameterCount { get; }

	public ImmutableArray<string> InheritanceNames { get; }

	public DeclaredSymbolInfo(string name, string containerDisplayName, string fullyQualifiedContainerName, DeclaredSymbolInfoKind kind, TextSpan span, ImmutableArray<string> inheritanceNames, ushort parameterCount = 0, ushort typeParameterCount = 0)
	{
		this = default(DeclaredSymbolInfo);
		Name = name;
		ContainerDisplayName = containerDisplayName;
		FullyQualifiedContainerName = fullyQualifiedContainerName;
		Kind = kind;
		Span = span;
		ParameterCount = parameterCount;
		TypeParameterCount = typeParameterCount;
		InheritanceNames = inheritanceNames;
	}

	public async Task<ISymbol> ResolveAsync(Document document, CancellationToken cancellationToken)
	{
		return Resolve(await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false), cancellationToken);
	}

	public ISymbol Resolve(SemanticModel semanticModel, CancellationToken cancellationToken)
	{
		SyntaxNode declaration = semanticModel.SyntaxTree.GetRoot(cancellationToken).FindNode(Span);
		return semanticModel.GetDeclaredSymbol(declaration, cancellationToken);
	}
}
