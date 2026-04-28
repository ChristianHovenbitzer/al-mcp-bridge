using System;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces;

public sealed class TextAndVersion
{
	public SourceText Text { get; }

	public VersionStamp Version { get; }

	public string FilePath { get; }

	private TextAndVersion(SourceText text, VersionStamp version, string filePath)
	{
		Text = text;
		Version = version;
		FilePath = filePath ?? string.Empty;
	}

	public static TextAndVersion Create(SourceText text, VersionStamp version, string filePath = null)
	{
		if (text == null)
		{
			throw new ArgumentNullException("text");
		}
		return new TextAndVersion(text, version, filePath);
	}
}
