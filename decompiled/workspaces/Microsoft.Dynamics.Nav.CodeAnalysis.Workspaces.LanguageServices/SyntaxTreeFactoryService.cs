using System;
using System.IO;
using System.Text;
using System.Threading;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Host;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.LanguageServices;

internal class SyntaxTreeFactoryService : AbstractSyntaxTreeFactoryService
{
	internal SyntaxTreeFactoryService(HostLanguageServices languageServices)
		: base(languageServices)
	{
	}

	public override SyntaxTree CreateSyntaxTree(string fileName, Encoding encoding, SyntaxNode root, ParseOptions parseOptions)
	{
		return SyntaxFactory.SyntaxTree(root, fileName, encoding, parseOptions);
	}

	public override SyntaxTree ParseSyntaxTree(string fileName, SourceText text, CancellationToken cancellationToken, ParseOptions parseOptions)
	{
		return SyntaxFactory.ParseSyntaxTree(text, fileName, parseOptions, cancellationToken);
	}

	public override SyntaxNode DeserializeNodeFrom(Stream stream, CancellationToken cancellationToken)
	{
		return SyntaxNode.DeserializeFrom(stream, cancellationToken);
	}

	public override bool CanCreateRecoverableTree(SyntaxNode root)
	{
		CompilationUnitSyntax compilationUnitSyntax = root as CompilationUnitSyntax;
		if (base.CanCreateRecoverableTree(root))
		{
			return compilationUnitSyntax != null;
		}
		return false;
	}

	public override SyntaxTree CreateRecoverableTree(ProjectId cacheKey, string filePath, ValueSource<TextAndVersion> text, Encoding encoding, SyntaxNode root)
	{
		throw new NotImplementedException();
	}
}
