using System.IO;
using System.Reflection;
using System.Xml;
using System.Xml.Xsl;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Documentation;

public static class XmlDocCommentToMarkdownTransformer
{
	private static readonly XslCompiledTransform transform;

	static XmlDocCommentToMarkdownTransformer()
	{
		Assembly assembly = typeof(XmlDocCommentToMarkdownTransformer).Assembly;
		string name = typeof(WorkspacesResources).Namespace + ".Documentation.XmlDocCommentToMarkdown.xslt";
		using Stream input = assembly.GetManifestResourceStream(name);
		using XmlReader stylesheet = XmlReader.Create(input);
		XsltSettings @default = XsltSettings.Default;
		transform = new XslCompiledTransform();
		transform.Load(stylesheet, @default, new XmlUrlResolver());
	}

	public static string Transform(string xmlComment)
	{
		using XmlReader input = XmlReader.Create(new StringReader(xmlComment));
		using StringWriter stringWriter = new StringWriter();
		transform.Transform(input, null, stringWriter);
		return stringWriter.ToString();
	}
}
