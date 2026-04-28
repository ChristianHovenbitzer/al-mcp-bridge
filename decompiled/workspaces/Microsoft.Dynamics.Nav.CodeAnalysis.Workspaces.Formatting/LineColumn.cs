namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Formatting;

internal struct LineColumn
{
	public static LineColumn Default = new LineColumn
	{
		Line = 0,
		Column = 0,
		WhitespaceOnly = true
	};

	public int Line { get; private set; }

	public int Column { get; private set; }

	public bool WhitespaceOnly { get; private set; }

	public LineColumn(int line, int column, bool whitespaceOnly)
	{
		this = default(LineColumn);
		Line = line;
		Column = column;
		WhitespaceOnly = whitespaceOnly;
	}

	public LineColumn With(LineColumnDelta delta)
	{
		LineColumn result;
		if (delta.Lines <= 0)
		{
			result = default(LineColumn);
			result.Line = Line;
			result.Column = Column + delta.Spaces;
			result.WhitespaceOnly = WhitespaceOnly && delta.WhitespaceOnly;
			return result;
		}
		result = default(LineColumn);
		result.Line = Line + delta.Lines;
		result.Column = delta.Spaces;
		result.WhitespaceOnly = delta.WhitespaceOnly;
		return result;
	}
}
