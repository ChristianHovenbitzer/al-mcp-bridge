using System.IO;
using System.Text;
using System.Threading;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Host;

internal interface ISyntaxTreeFactoryService : ILanguageService
{
	SyntaxTree CreateSyntaxTree(string filePath, Encoding encoding, SyntaxNode root, ParseOptions parseOptions);

	SyntaxTree ParseSyntaxTree(string filePath, SourceText text, CancellationToken cancellationToken, ParseOptions parseOptions);

	bool CanCreateRecoverableTree(SyntaxNode root);

	SyntaxTree CreateRecoverableTree(ProjectId cacheKey, string filePath, ValueSource<TextAndVersion> text, Encoding encoding, SyntaxNode root);

	SyntaxNode DeserializeNodeFrom(Stream stream, CancellationToken cancellationToken);
}
