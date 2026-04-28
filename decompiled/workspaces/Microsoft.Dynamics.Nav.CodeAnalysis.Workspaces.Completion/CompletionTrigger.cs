namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Completion;

public struct CompletionTrigger
{
	public static readonly CompletionTrigger Default = new CompletionTrigger(CompletionTriggerKind.Invoke);

	public CompletionTriggerKind Kind { get; }

	public char Character { get; }

	internal CompletionTrigger(CompletionTriggerKind kind, char character = '\0')
	{
		this = default(CompletionTrigger);
		Kind = kind;
		Character = character;
	}

	public static CompletionTrigger CreateInsertionTrigger(char insertedCharacter)
	{
		return new CompletionTrigger(CompletionTriggerKind.Insertion, insertedCharacter);
	}

	public static CompletionTrigger CreateDeletionTrigger(char deletedCharacter)
	{
		return new CompletionTrigger(CompletionTriggerKind.Deletion, deletedCharacter);
	}

	public static CompletionTrigger CreateDebuggerConsoleCompletion()
	{
		return new CompletionTrigger(CompletionTriggerKind.DebuggerConsole);
	}

	public static bool operator ==(CompletionTrigger completionTrigger1, CompletionTrigger completionTrigger2)
	{
		if (completionTrigger1.Character == completionTrigger2.Character)
		{
			return completionTrigger1.Kind == completionTrigger2.Kind;
		}
		return false;
	}

	public static bool operator !=(CompletionTrigger completionTrigger1, CompletionTrigger completionTrigger2)
	{
		return !(completionTrigger1 == completionTrigger2);
	}

	public override bool Equals(object obj)
	{
		if (obj == null || GetType() != obj.GetType())
		{
			return false;
		}
		CompletionTrigger completionTrigger = (CompletionTrigger)obj;
		return this == completionTrigger;
	}

	public override int GetHashCode()
	{
		return Character.GetHashCode() ^ Kind.GetHashCode();
	}
}
