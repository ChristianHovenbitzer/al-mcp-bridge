namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Formatting;

internal struct LineColumnDelta
{
	public static LineColumnDelta Default = new LineColumnDelta(0, 0, whitespaceOnly: true, forceUpdate: false);

	public int Lines { get; private set; }

	public int Spaces { get; private set; }

	public bool WhitespaceOnly { get; private set; }

	public bool ForceUpdate { get; private set; }

	public LineColumnDelta(int lines, int spaces)
	{
		this = default(LineColumnDelta);
		Lines = lines;
		Spaces = spaces;
		WhitespaceOnly = true;
		ForceUpdate = false;
	}

	public LineColumnDelta(int lines, int spaces, bool whitespaceOnly)
		: this(lines, spaces)
	{
		WhitespaceOnly = whitespaceOnly;
		ForceUpdate = false;
	}

	public LineColumnDelta(int lines, int spaces, bool whitespaceOnly, bool forceUpdate)
		: this(lines, spaces, whitespaceOnly)
	{
		ForceUpdate = forceUpdate;
	}

	internal LineColumnDelta With(LineColumnDelta delta)
	{
		LineColumnDelta result;
		if (delta.Lines <= 0)
		{
			result = default(LineColumnDelta);
			result.Lines = Lines;
			result.Spaces = Spaces + delta.Spaces;
			result.WhitespaceOnly = WhitespaceOnly && delta.WhitespaceOnly;
			result.ForceUpdate = ForceUpdate || delta.ForceUpdate;
			return result;
		}
		result = default(LineColumnDelta);
		result.Lines = Lines + delta.Lines;
		result.Spaces = delta.Spaces;
		result.WhitespaceOnly = delta.WhitespaceOnly;
		result.ForceUpdate = ForceUpdate || delta.ForceUpdate || Spaces > 0;
		return result;
	}
}
