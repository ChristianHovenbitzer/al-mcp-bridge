using System.IO;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces;

public class DocumentationProviderFactory : IDocumentationProviderFactory
{
	public static IDocumentationProviderFactory Instance { get; } = new DocumentationProviderFactory();


	public DocumentationProvider CreateFromContent(Stream stream)
	{
		return XmlDocumentationProvider.CreateFromStream(stream);
	}

	public DocumentationProvider CreateFromContent(byte[] bytes)
	{
		return XmlDocumentationProvider.CreateFromBytes(bytes);
	}

	public DocumentationProvider CreateFromFile(string filePath)
	{
		return XmlDocumentationProvider.CreateFromFile(filePath);
	}
}
