using System.Threading;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces;

internal class SyntacticDocument
{
	public readonly Document Document;

	public readonly SourceText Text;

	public readonly SyntaxTree SyntaxTree;

	public readonly SyntaxNode Root;

	public Project Project => Document.Project;

	protected SyntacticDocument(Document document, SourceText text, SyntaxTree tree, SyntaxNode root)
	{
		Document = document;
		Text = text;
		SyntaxTree = tree;
		Root = root;
	}

	public static async Task<SyntacticDocument> CreateAsync(Document document, CancellationToken cancellationToken)
	{
		SourceText text = await document.GetTextAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		SyntaxNode syntaxNode = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		return new SyntacticDocument(document, text, syntaxNode.SyntaxTree, syntaxNode);
	}
}
