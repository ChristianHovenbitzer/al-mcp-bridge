using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Xml;
using System.Xml.Linq;
using Microsoft.Dynamics.Nav.AL.Common;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces;

public abstract class XmlDocumentationProvider : DocumentationProvider
{
	private sealed class ContentBasedXmlDocumentationProvider : XmlDocumentationProvider
	{
		private readonly byte[] xmlDocCommentBytes;

		public ContentBasedXmlDocumentationProvider(byte[] xmlDocCommentBytes)
		{
			this.xmlDocCommentBytes = xmlDocCommentBytes ?? throw new ArgumentNullException("xmlDocCommentBytes");
		}

		protected override Stream GetSourceStream(CancellationToken cancellationToken)
		{
			return SerializableBytes.CreateReadableStream(xmlDocCommentBytes);
		}

		public override bool Equals(object obj)
		{
			if (obj is ContentBasedXmlDocumentationProvider other)
			{
				return EqualsHelper(other);
			}
			return false;
		}

		private bool EqualsHelper(ContentBasedXmlDocumentationProvider other)
		{
			if (this == other || xmlDocCommentBytes == other.xmlDocCommentBytes)
			{
				return true;
			}
			if (xmlDocCommentBytes.Length != other.xmlDocCommentBytes.Length)
			{
				return false;
			}
			for (int i = 0; i < xmlDocCommentBytes.Length; i++)
			{
				if (xmlDocCommentBytes[i] != other.xmlDocCommentBytes[i])
				{
					return false;
				}
			}
			return true;
		}

		public override int GetHashCode()
		{
			return Hash.CombineValues(xmlDocCommentBytes);
		}
	}

	private sealed class StreamContentBasedXmlDocumentationProvider : XmlDocumentationProvider
	{
		private readonly Stream xmlDocCommentStream;

		public StreamContentBasedXmlDocumentationProvider(Stream xmlDocCommentStream)
		{
			this.xmlDocCommentStream = xmlDocCommentStream ?? throw new ArgumentNullException("xmlDocCommentStream");
		}

		protected override Stream GetSourceStream(CancellationToken cancellationToken)
		{
			if (xmlDocCommentStream is SerializableBytes.PooledStream)
			{
				return xmlDocCommentStream;
			}
			return SerializableBytes.CreateReadableStreamAsync(xmlDocCommentStream, cancellationToken).GetAwaiter().GetResult();
		}

		public override bool Equals(object obj)
		{
			if (obj is StreamContentBasedXmlDocumentationProvider other)
			{
				return EqualsHelper(other);
			}
			return false;
		}

		private bool EqualsHelper(StreamContentBasedXmlDocumentationProvider other)
		{
			if (this == other || xmlDocCommentStream == other.xmlDocCommentStream)
			{
				return true;
			}
			return false;
		}

		public override int GetHashCode()
		{
			return Hash.CombineValues(xmlDocCommentStream);
		}
	}

	private sealed class FileBasedXmlDocumentationProvider : XmlDocumentationProvider
	{
		private readonly string filePath;

		public FileBasedXmlDocumentationProvider(string filePath)
		{
			this.filePath = filePath ?? throw new ArgumentNullException("filePath");
			DebugAssertHelper.Assert(PathUtilities.IsAbsolute(filePath));
		}

		protected override Stream GetSourceStream(CancellationToken cancellationToken)
		{
			return new FileStream(filePath, FileMode.Open, FileAccess.Read);
		}

		public override bool Equals(object obj)
		{
			if (obj is FileBasedXmlDocumentationProvider fileBasedXmlDocumentationProvider)
			{
				return filePath == fileBasedXmlDocumentationProvider.filePath;
			}
			return false;
		}

		public override int GetHashCode()
		{
			return filePath.GetHashCode();
		}
	}

	private sealed class NullXmlDocumentationProvider : XmlDocumentationProvider
	{
		protected internal override string GetDocumentationForSymbol(string documentationMemberID, CultureInfo preferredCulture, CancellationToken cancellationToken = default(CancellationToken))
		{
			return "";
		}

		protected override Stream GetSourceStream(CancellationToken cancellationToken)
		{
			return new MemoryStream();
		}

		public override bool Equals(object obj)
		{
			return this == obj;
		}

		public override int GetHashCode()
		{
			return 0;
		}
	}

	private readonly NonReentrantLock gate = new NonReentrantLock();

	private Dictionary<string, string> docComments;

	private static XmlDocumentationProvider DefaultXmlDocumentationProvider { get; } = new NullXmlDocumentationProvider();


	protected abstract Stream GetSourceStream(CancellationToken cancellationToken);

	public static XmlDocumentationProvider CreateFromBytes(byte[] xmlDocCommentBytes)
	{
		return new ContentBasedXmlDocumentationProvider(xmlDocCommentBytes);
	}

	public static XmlDocumentationProvider CreateFromStream(Stream xmlDocCommentStream)
	{
		return new StreamContentBasedXmlDocumentationProvider(xmlDocCommentStream);
	}

	public static XmlDocumentationProvider CreateFromFile(string xmlDocCommentFilePath)
	{
		if (!File.Exists(xmlDocCommentFilePath))
		{
			return DefaultXmlDocumentationProvider;
		}
		return new FileBasedXmlDocumentationProvider(xmlDocCommentFilePath);
	}

	private XDocument GetXDocument(CancellationToken cancellationToken)
	{
		using Stream stream = GetSourceStream(cancellationToken);
		return Microsoft.Dynamics.Nav.AL.Common.XmlUtilities.GetSafeXDocument(stream);
	}

	protected internal override string GetDocumentationForSymbol(string? documentationMemberID, CultureInfo? preferredCulture, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (documentationMemberID == null)
		{
			return string.Empty;
		}
		if (docComments == null)
		{
			using (gate.DisposableWait(cancellationToken))
			{
				try
				{
					docComments = new Dictionary<string, string>();
					foreach (XElement item in GetXDocument(cancellationToken).Descendants("member"))
					{
						if (item.Attribute("name") != null)
						{
							using XmlReader xmlReader = item.CreateReader();
							xmlReader.MoveToContent();
							docComments[item.Attribute("name").Value] = xmlReader.ReadInnerXml();
						}
					}
				}
				catch (Exception)
				{
				}
			}
		}
		Dictionary<string, string> dictionary = docComments;
		if (dictionary == null || !dictionary.TryGetValue(documentationMemberID, out string value))
		{
			return string.Empty;
		}
		return value;
	}
}
