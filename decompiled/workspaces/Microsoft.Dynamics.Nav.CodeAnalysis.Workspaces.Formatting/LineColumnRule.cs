namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Formatting;

internal struct LineColumnRule
{
	internal enum SpaceOperations
	{
		Preserve,
		Force
	}

	internal enum LineOperations
	{
		Preserve,
		Force
	}

	internal enum IndentationOperations
	{
		Absolute,
		Default,
		Given,
		Follow,
		Preserve
	}

	public SpaceOperations SpaceOperation { get; private set; }

	public LineOperations LineOperation { get; private set; }

	public IndentationOperations IndentationOperation { get; private set; }

	public int Lines { get; private set; }

	public int Spaces { get; private set; }

	public int Indentation { get; private set; }

	public LineColumnRule With(int? lines = null, int? spaces = null, int? indentation = null, LineOperations? lineOperation = null, SpaceOperations? spaceOperation = null, IndentationOperations? indentationOperation = null)
	{
		LineColumnRule result = default(LineColumnRule);
		result.SpaceOperation = ((!spaceOperation.HasValue) ? SpaceOperation : spaceOperation.Value);
		result.LineOperation = ((!lineOperation.HasValue) ? LineOperation : lineOperation.Value);
		result.IndentationOperation = ((!indentationOperation.HasValue) ? IndentationOperation : indentationOperation.Value);
		result.Lines = ((!lines.HasValue) ? Lines : lines.Value);
		result.Spaces = ((!spaces.HasValue) ? Spaces : spaces.Value);
		result.Indentation = ((!indentation.HasValue) ? Indentation : indentation.Value);
		return result;
	}

	public static LineColumnRule Preserve()
	{
		LineColumnRule result = default(LineColumnRule);
		result.SpaceOperation = SpaceOperations.Preserve;
		result.LineOperation = LineOperations.Preserve;
		result.IndentationOperation = IndentationOperations.Preserve;
		result.Lines = 0;
		result.Spaces = 0;
		result.Indentation = 0;
		return result;
	}

	public static LineColumnRule PreserveWithGivenSpaces(int spaces)
	{
		LineColumnRule result = default(LineColumnRule);
		result.SpaceOperation = SpaceOperations.Preserve;
		result.LineOperation = LineOperations.Preserve;
		result.IndentationOperation = IndentationOperations.Given;
		result.Lines = 0;
		result.Spaces = spaces;
		result.Indentation = 0;
		return result;
	}

	public static LineColumnRule PreserveLinesWithDefaultIndentation(int lines)
	{
		LineColumnRule result = default(LineColumnRule);
		result.SpaceOperation = SpaceOperations.Preserve;
		result.LineOperation = LineOperations.Preserve;
		result.IndentationOperation = IndentationOperations.Default;
		result.Lines = lines;
		result.Spaces = 0;
		result.Indentation = -1;
		return result;
	}

	public static LineColumnRule PreserveLinesWithGivenIndentation(int lines)
	{
		LineColumnRule result = default(LineColumnRule);
		result.SpaceOperation = SpaceOperations.Preserve;
		result.LineOperation = LineOperations.Preserve;
		result.IndentationOperation = IndentationOperations.Given;
		result.Lines = lines;
		result.Spaces = 0;
		result.Indentation = -1;
		return result;
	}

	public static LineColumnRule PreserveLinesWithAbsoluteIndentation(int lines, int indentation)
	{
		LineColumnRule result = default(LineColumnRule);
		result.SpaceOperation = SpaceOperations.Preserve;
		result.LineOperation = LineOperations.Preserve;
		result.IndentationOperation = IndentationOperations.Absolute;
		result.Lines = lines;
		result.Spaces = 0;
		result.Indentation = indentation;
		return result;
	}

	public static LineColumnRule PreserveLinesWithFollowingPrecedingIndentation()
	{
		LineColumnRule result = default(LineColumnRule);
		result.SpaceOperation = SpaceOperations.Preserve;
		result.LineOperation = LineOperations.Preserve;
		result.IndentationOperation = IndentationOperations.Follow;
		result.Lines = -1;
		result.Spaces = 0;
		result.Indentation = -1;
		return result;
	}

	public static LineColumnRule ForceSpaces(int spaces)
	{
		LineColumnRule result = default(LineColumnRule);
		result.SpaceOperation = SpaceOperations.Force;
		result.LineOperation = LineOperations.Preserve;
		result.IndentationOperation = IndentationOperations.Preserve;
		result.Lines = 0;
		result.Spaces = spaces;
		result.Indentation = 0;
		return result;
	}

	public static LineColumnRule PreserveSpacesOrUseDefaultIndentation(int spaces)
	{
		LineColumnRule result = default(LineColumnRule);
		result.SpaceOperation = SpaceOperations.Preserve;
		result.LineOperation = LineOperations.Preserve;
		result.IndentationOperation = IndentationOperations.Default;
		result.Lines = 0;
		result.Spaces = spaces;
		result.Indentation = -1;
		return result;
	}

	public static LineColumnRule ForceSpacesOrUseDefaultIndentation(int spaces)
	{
		LineColumnRule result = default(LineColumnRule);
		result.SpaceOperation = SpaceOperations.Force;
		result.LineOperation = LineOperations.Preserve;
		result.IndentationOperation = IndentationOperations.Default;
		result.Lines = 0;
		result.Spaces = spaces;
		result.Indentation = -1;
		return result;
	}

	public static LineColumnRule ForceSpacesOrUseAbsoluteIndentation(int spacesOrIndentation)
	{
		LineColumnRule result = default(LineColumnRule);
		result.SpaceOperation = SpaceOperations.Force;
		result.LineOperation = LineOperations.Preserve;
		result.IndentationOperation = IndentationOperations.Absolute;
		result.Lines = 0;
		result.Spaces = spacesOrIndentation;
		result.Indentation = spacesOrIndentation;
		return result;
	}
}
