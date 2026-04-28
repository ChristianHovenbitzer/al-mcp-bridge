using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces;

public class LinePositionSpanTextChange
{
	public string NewText { get; set; }

	public int StartLine { get; set; }

	public int StartColumn { get; set; }

	public int EndLine { get; set; }

	public int EndColumn { get; set; }

	public static async Task<IEnumerable<LinePositionSpanTextChange>> Convert(Document document, IEnumerable<TextChange> changes)
	{
		SourceText text = await document.GetTextAsync();
		return changes.OrderByDescending((TextChange change) => change.Span).Select(delegate(TextChange change)
		{
			TextSpan span = change.Span;
			string newText = change.NewText;
			string text2 = string.Empty;
			string text3 = string.Empty;
			if (newText.Length > 0)
			{
				if (span.Start > 0 && newText[0] == '\n' && text[span.Start - 1] == '\r')
				{
					span = TextSpan.FromBounds(span.Start - 1, span.End);
					text2 = "\r";
				}
				if (span.End < text.Length - 1 && newText[newText.Length - 1] == '\r' && text[span.End] == '\n')
				{
					span = TextSpan.FromBounds(span.Start, span.End + 1);
					text3 = "\n";
				}
			}
			LinePositionSpan linePositionSpan = text.Lines.GetLinePositionSpan(span);
			return new LinePositionSpanTextChange
			{
				NewText = text2 + newText + text3,
				StartLine = linePositionSpan.Start.Line + 1,
				StartColumn = linePositionSpan.Start.Character + 1,
				EndLine = linePositionSpan.End.Line + 1,
				EndColumn = linePositionSpan.End.Character + 1
			};
		});
	}

	public override bool Equals(object obj)
	{
		if (!(obj is LinePositionSpanTextChange linePositionSpanTextChange))
		{
			return false;
		}
		if (NewText == linePositionSpanTextChange.NewText && StartLine == linePositionSpanTextChange.StartLine && StartColumn == linePositionSpanTextChange.StartColumn && EndLine == linePositionSpanTextChange.EndLine)
		{
			return EndColumn == linePositionSpanTextChange.EndColumn;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return NewText.GetHashCode() * (53 + StartLine) * (59 + StartColumn) * (61 + EndLine) * (67 + EndColumn);
	}
}
