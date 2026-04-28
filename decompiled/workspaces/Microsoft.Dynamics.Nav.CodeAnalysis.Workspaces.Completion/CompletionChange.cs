using Microsoft.Dynamics.Nav.CodeAnalysis.Text;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Completion;

public sealed class CompletionChange
{
	public TextChange TextChange { get; }

	public int? NewPosition { get; }

	public bool IncludesCommitCharacter { get; }

	private CompletionChange(TextChange textChange, int? newPosition, bool includesCommitCharacter)
	{
		TextChange = textChange;
		NewPosition = newPosition;
		IncludesCommitCharacter = includesCommitCharacter;
	}

	public static CompletionChange Create(TextChange textChange, int? newPosition = null, bool includesCommitCharacter = false)
	{
		return new CompletionChange(textChange, newPosition, includesCommitCharacter);
	}

	public CompletionChange WithTextChange(TextChange textChange)
	{
		return new CompletionChange(textChange, NewPosition, IncludesCommitCharacter);
	}

	public CompletionChange WithNewPosition(int? newPosition)
	{
		return new CompletionChange(TextChange, newPosition, IncludesCommitCharacter);
	}

	public CompletionChange WithIncludesCommitCharacter(bool includesCommitCharacter)
	{
		return new CompletionChange(TextChange, NewPosition, includesCommitCharacter);
	}
}
