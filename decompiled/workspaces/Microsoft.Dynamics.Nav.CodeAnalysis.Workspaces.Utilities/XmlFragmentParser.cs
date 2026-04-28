using System;
using System.IO;
using System.Xml;
using Microsoft.Dynamics.Nav.AL.Common;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Utilities;

internal sealed class XmlFragmentParser
{
	private sealed class Reader : TextReader
	{
		private string text;

		private int position;

		private static readonly string s_rootElementName = "_" + Guid.NewGuid().ToString("N");

		internal static readonly string CurrentElementName = "_" + Guid.NewGuid().ToString("N");

		private static readonly string s_rootStart = "<" + s_rootElementName + ">";

		private static readonly string s_currentStart = "<" + CurrentElementName + ">";

		private static readonly string s_currentEnd = "</" + CurrentElementName + ">";

		public void Reset()
		{
			text = null;
			position = 0;
		}

		public void SetText(string newText)
		{
			text = newText;
			if (position > 0)
			{
				position = s_rootStart.Length;
			}
		}

		public override int Read(char[] buffer, int index, int count)
		{
			if (count == 0)
			{
				return 0;
			}
			int num = count;
			position += EncodeAndAdvance(s_rootStart, position, buffer, ref index, ref count);
			position += EncodeAndAdvance(s_currentStart, position - s_rootStart.Length, buffer, ref index, ref count);
			position += EncodeAndAdvance(text, position - s_rootStart.Length - s_currentStart.Length, buffer, ref index, ref count);
			position += EncodeAndAdvance(s_currentEnd, position - s_rootStart.Length - s_currentStart.Length - text.Length, buffer, ref index, ref count);
			if (num == count)
			{
				buffer[index++] = ' ';
				count--;
			}
			return num - count;
		}

		private static int EncodeAndAdvance(string src, int srcIndex, char[] dest, ref int destIndex, ref int destCount)
		{
			if (destCount == 0 || srcIndex < 0 || srcIndex >= src.Length)
			{
				return 0;
			}
			int num = Math.Min(src.Length - srcIndex, destCount);
			src.CopyTo(srcIndex, dest, destIndex, num);
			destIndex += num;
			destCount -= num;
			return num;
		}

		public override int Read()
		{
			throw ExceptionUtilities.Unreachable;
		}

		public override int Peek()
		{
			throw ExceptionUtilities.Unreachable;
		}
	}

	private XmlReader xmlReader;

	private readonly Reader textReader = new Reader();

	private static readonly ObjectPool<XmlFragmentParser> s_pool = SharedPools.Default<XmlFragmentParser>();

	private bool BeforeStart => xmlReader.Depth < 2;

	private bool ReachedEnd
	{
		get
		{
			if (xmlReader.Depth == 1 && xmlReader.NodeType == XmlNodeType.EndElement)
			{
				return xmlReader.LocalName == Reader.CurrentElementName;
			}
			return false;
		}
	}

	public static void ParseFragment<TArg>(string xmlFragment, Action<XmlReader, TArg> callback, TArg arg)
	{
		XmlFragmentParser xmlFragmentParser = s_pool.Allocate();
		try
		{
			xmlFragmentParser.ParseInternal(xmlFragment, callback, arg);
		}
		finally
		{
			s_pool.Free(xmlFragmentParser);
		}
	}

	private void ParseInternal<TArg>(string text, Action<XmlReader, TArg> callback, TArg arg)
	{
		textReader.SetText(text);
		if (xmlReader == null)
		{
			xmlReader = XmlReader.Create(textReader, Microsoft.Dynamics.Nav.AL.Common.XmlUtilities.SafeXmlReaderSettings);
		}
		try
		{
			while (!ReachedEnd)
			{
				if (BeforeStart)
				{
					xmlReader.Read();
				}
				else
				{
					callback(xmlReader, arg);
				}
			}
			xmlReader.ReadEndElement();
		}
		catch
		{
			xmlReader.Dispose();
			xmlReader = null;
			textReader.Reset();
			throw;
		}
	}
}
