using System;
using System.Collections.Generic;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces;

public sealed class DocumentInfo
{
	public DocumentId Id { get; }

	public string Name { get; }

	public IReadOnlyList<string> Folders { get; }

	public string FilePath { get; }

	public TextLoader TextLoader { get; }

	public bool IsGenerated { get; }

	private DocumentInfo(DocumentId id, string name, IEnumerable<string> folders, TextLoader loader, string filePath, bool isGenerated)
	{
		if (id == null)
		{
			throw new ArgumentNullException("id");
		}
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		Id = id;
		Name = name;
		Folders = folders.ToImmutableReadOnlyListOrEmpty();
		TextLoader = loader;
		FilePath = filePath;
		IsGenerated = isGenerated;
	}

	public static DocumentInfo Create(DocumentId id, string name, IEnumerable<string> folders = null, TextLoader loader = null, string filePath = null, bool isGenerated = false)
	{
		return new DocumentInfo(id, name, folders, loader, filePath, isGenerated);
	}

	private DocumentInfo With(DocumentId id = null, string name = null, IEnumerable<string> folders = null, Optional<TextLoader> loader = default(Optional<TextLoader>), Optional<string> filePath = default(Optional<string>))
	{
		DocumentId documentId = id ?? Id;
		string text = name ?? Name;
		IEnumerable<string> enumerable = folders ?? Folders;
		TextLoader textLoader = (loader.HasValue ? loader.Value : TextLoader);
		string text2 = (filePath.HasValue ? filePath.Value : FilePath);
		if (documentId == Id && text == Name && enumerable == Folders && textLoader == TextLoader && text2 == FilePath)
		{
			return this;
		}
		return new DocumentInfo(documentId, text, enumerable, textLoader, text2, IsGenerated);
	}

	public DocumentInfo WithId(DocumentId id)
	{
		return With(id);
	}

	public DocumentInfo WithName(string name)
	{
		return With(null, name);
	}

	public DocumentInfo WithFolders(IEnumerable<string> folders)
	{
		return With(null, null, folders.ToImmutableReadOnlyListOrEmpty());
	}

	public DocumentInfo WithTextLoader(TextLoader loader)
	{
		return With(null, null, null, loader);
	}

	public DocumentInfo WithFilePath(string filePath)
	{
		Optional<string> filePath2 = filePath;
		return With(null, null, null, default(Optional<TextLoader>), filePath2);
	}
}
