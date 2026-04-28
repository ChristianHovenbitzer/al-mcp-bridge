using System;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces;

internal sealed class TreeAndVersion
{
	public SyntaxTree Tree { get; }

	public VersionStamp Version { get; }

	private TreeAndVersion(SyntaxTree tree, VersionStamp version)
	{
		Tree = tree;
		Version = version;
	}

	public static TreeAndVersion Create(SyntaxTree tree, VersionStamp version)
	{
		if (tree == null)
		{
			throw new ArgumentNullException("tree");
		}
		return new TreeAndVersion(tree, version);
	}
}
