using System.IO;
using System.Text;
using System.Threading;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Host;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Options;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.LanguageServices;

internal abstract class AbstractSyntaxTreeFactoryService : ISyntaxTreeFactoryService, ILanguageService
{
	internal readonly int MinimumLengthForRecoverableTree;

	private readonly bool hasCachingService;

	internal HostLanguageServices LanguageServices { get; }

	protected AbstractSyntaxTreeFactoryService(HostLanguageServices languageServices)
	{
		LanguageServices = languageServices;
		MinimumLengthForRecoverableTree = languageServices.WorkspaceServices.Workspace.Options?.GetOption(CacheOption.RecoverableTreeLengthThreshold) ?? 4096;
		hasCachingService = languageServices.WorkspaceServices.GetService<IProjectCacheHostService>() != null;
	}

	public abstract SyntaxTree CreateSyntaxTree(string filePath, Encoding encoding, SyntaxNode root, ParseOptions parseOptions);

	public abstract SyntaxTree ParseSyntaxTree(string filePath, SourceText text, CancellationToken cancellationToken, ParseOptions parseOptions);

	public abstract SyntaxTree CreateRecoverableTree(ProjectId cacheKey, string filePath, ValueSource<TextAndVersion> text, Encoding encoding, SyntaxNode root);

	public abstract SyntaxNode DeserializeNodeFrom(Stream stream, CancellationToken cancellationToken);

	public virtual bool CanCreateRecoverableTree(SyntaxNode root)
	{
		if (hasCachingService)
		{
			return root.FullSpan.Length >= MinimumLengthForRecoverableTree;
		}
		return false;
	}

	protected static SyntaxNode RecoverNode(SyntaxTree tree, TextSpan textSpan, int kind)
	{
		for (SyntaxNode syntaxNode = tree.GetRoot().FindToken(textSpan.Start, findInsideTrivia: true).Parent; syntaxNode != null; syntaxNode = ((!(syntaxNode is IStructuredTriviaSyntax { ParentTrivia: { Token: var token } })) ? syntaxNode.Parent : token.Parent))
		{
			if (syntaxNode.Span == textSpan && (int)syntaxNode.Kind == kind)
			{
				return syntaxNode;
			}
		}
		throw ExceptionUtilities.Unreachable;
	}
}
