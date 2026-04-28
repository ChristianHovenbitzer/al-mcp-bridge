using System.Threading;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces;

internal class SemanticDocument : SyntacticDocument
{
	public readonly SemanticModel SemanticModel;

	private SemanticDocument(Document document, SourceText text, SyntaxTree tree, SyntaxNode root, SemanticModel semanticModel)
		: base(document, text, tree, root)
	{
		SemanticModel = semanticModel;
	}

	public new static async Task<SemanticDocument> CreateAsync(Document document, CancellationToken cancellationToken)
	{
		SourceText text = await document.GetTextAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		SyntaxNode root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		SemanticModel semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		return new SemanticDocument(document, text, root.SyntaxTree, root, semanticModel);
	}
}
