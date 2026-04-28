using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Net;
using System.Xml;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Documentation;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Utilities;

internal sealed class DocumentationComment
{
	private class CommentBuilder
	{
		private readonly DocumentationComment comment;

		private ImmutableArray<string>.Builder? parameterNamesBuilder;

		public static DocumentationComment Parse(string xml, bool asMarkdown = false)
		{
			try
			{
				return new CommentBuilder(xml, asMarkdown).ParseInternal(xml);
			}
			catch (Exception)
			{
				return new DocumentationComment
				{
					FullXmlFragment = xml,
					HadXmlParseError = true
				};
			}
		}

		private CommentBuilder(string xml, bool isMarkdown)
		{
			comment = new DocumentationComment
			{
				FullXmlFragment = xml,
				IsMarkdown = isMarkdown
			};
		}

		private DocumentationComment ParseInternal(string xml)
		{
			XmlFragmentParser.ParseFragment(xml, ParseCallback, this);
			comment.ParameterNames = ((parameterNamesBuilder == null) ? ImmutableArray<string>.Empty : parameterNamesBuilder.ToImmutable());
			return comment;
		}

		private static void ParseCallback(XmlReader reader, CommentBuilder builder)
		{
			builder.ParseCallback(reader);
		}

		private string TrimEachLineRaw(string text)
		{
			string[] array = text.Split(s_NewLineAsStringArray, StringSplitOptions.None);
			if (array.Length < 2)
			{
				return string.Join(Environment.NewLine, array.Select((string i) => i.Trim()));
			}
			int whitespaceCount = 0;
			for (string text2 = array[^1]; whitespaceCount < text2.Length && text2[whitespaceCount] == ' '; whitespaceCount++)
			{
			}
			return string.Join(Environment.NewLine, array.Select((string l) => TrimStart(l, whitespaceCount)));
		}

		private static string TrimStart(string s, int maxTrim)
		{
			if (string.IsNullOrEmpty(s) || s[0] != ' ')
			{
				return s;
			}
			int i = 1;
			int num;
			for (num = Math.Min(maxTrim, s.Length); i < num && s[i] == ' '; i++)
			{
			}
			return s.Substring(num);
		}

		private string TrimEachLine(string text)
		{
			return string.Join(Environment.NewLine, from i in text.Split(s_NewLineAsStringArray, StringSplitOptions.RemoveEmptyEntries)
				select i.Trim());
		}

		private void ParseCallback(XmlReader reader)
		{
			if (reader.NodeType == XmlNodeType.Element)
			{
				string localName = reader.LocalName;
				if (DocumentationCommentXmlNames.ElementEquals(localName, "example") && comment.ExampleText == null)
				{
					comment.ExampleText = ElementToText(reader);
				}
				else if (DocumentationCommentXmlNames.ElementEquals(localName, "summary") && comment.SummaryText == null)
				{
					comment.SummaryText = ElementToText(reader);
				}
				else if (DocumentationCommentXmlNames.ElementEquals(localName, "returns") && comment.ReturnsText == null)
				{
					comment.ReturnsText = ElementToText(reader);
				}
				else if (DocumentationCommentXmlNames.ElementEquals(localName, "value") && comment.ValueText == null)
				{
					comment.ValueText = ElementToText(reader);
				}
				else if (DocumentationCommentXmlNames.ElementEquals(localName, "remarks") && comment.RemarksText == null)
				{
					comment.RemarksText = ElementToText(reader);
				}
				else if (DocumentationCommentXmlNames.ElementEquals(localName, "param"))
				{
					string attribute = reader.GetAttribute("name");
					string value = ElementToText(reader);
					if (!string.IsNullOrWhiteSpace(attribute) && !comment.parameterTexts.ContainsKey(attribute))
					{
						(parameterNamesBuilder ?? (parameterNamesBuilder = ImmutableArray.CreateBuilder<string>())).Add(attribute);
						comment.parameterTexts.Add(attribute, value);
					}
				}
				else
				{
					reader.Read();
				}
			}
			else
			{
				reader.Read();
			}
		}

		private string? ElementToText(XmlReader reader)
		{
			if (!comment.IsMarkdown)
			{
				return TrimEachLineRaw(WebUtility.HtmlDecode(reader.ReadInnerXml()));
			}
			return TrimEachLine(ToMarkdown(reader.ReadOuterXml()));
		}
	}

	private static readonly string[] s_NewLineAsStringArray = new string[1] { "\n" };

	private static volatile DocumentationComment? cacheLastXmlFragmentParse;

	private readonly Dictionary<string, string> parameterTexts = new Dictionary<string, string>();

	private readonly Dictionary<string, ImmutableArray<string>> exceptionTexts = new Dictionary<string, ImmutableArray<string>>();

	public static readonly DocumentationComment Empty = new DocumentationComment();

	public bool HadXmlParseError { get; private set; }

	public string? FullXmlFragment { get; private set; }

	public string? ExampleText { get; private set; }

	public string? SummaryText { get; private set; }

	public string? ReturnsText { get; private set; }

	public string? ValueText { get; private set; }

	public string? RemarksText { get; private set; }

	public ImmutableArray<string> ParameterNames { get; private set; }

	public ImmutableArray<string> TypeParameterNames { get; private set; }

	public ImmutableArray<string> ExceptionTypes { get; private set; }

	public bool IsMarkdown { get; private set; }

	private DocumentationComment()
	{
		ParameterNames = ImmutableArray<string>.Empty;
		TypeParameterNames = ImmutableArray<string>.Empty;
		ExceptionTypes = ImmutableArray<string>.Empty;
	}

	public static DocumentationComment FromXmlFragmentAsMarkdown(string xml)
	{
		DocumentationComment documentationComment = cacheLastXmlFragmentParse;
		if (documentationComment == null || documentationComment.FullXmlFragment != xml || !documentationComment.IsMarkdown)
		{
			documentationComment = (cacheLastXmlFragmentParse = CommentBuilder.Parse(xml, asMarkdown: true));
		}
		return documentationComment;
	}

	public static DocumentationComment FromXmlFragment(string xml)
	{
		DocumentationComment documentationComment = cacheLastXmlFragmentParse;
		if (documentationComment == null || documentationComment.FullXmlFragment != xml || documentationComment.IsMarkdown)
		{
			documentationComment = (cacheLastXmlFragmentParse = CommentBuilder.Parse(xml));
		}
		return documentationComment;
	}

	private static string ToMarkdown(string xmlComment)
	{
		try
		{
			return XmlDocCommentToMarkdownTransformer.Transform(xmlComment);
		}
		catch
		{
			return xmlComment;
		}
	}

	public string GetParameterText(string parameterName)
	{
		parameterTexts.TryGetValue(parameterName, out string value);
		return value;
	}

	public ImmutableArray<string> GetExceptionTexts(string exceptionName)
	{
		exceptionTexts.TryGetValue(exceptionName, out ImmutableArray<string> value);
		if (value.IsDefault)
		{
			return ImmutableArray.Create<string>();
		}
		return value;
	}
}
