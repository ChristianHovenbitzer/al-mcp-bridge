namespace Microsoft.Dynamics.Nav.CodeAnalysis.CodeActions;

public class CodeActionKind
{
	public static readonly CodeActionKind QuickFix = new CodeActionKind("quickfix");

	public static readonly CodeActionKind Refactor = new CodeActionKind("refactor");

	public static readonly CodeActionKind Empty = new CodeActionKind(string.Empty);

	private readonly string kind;

	private CodeActionKind(string kind)
	{
		this.kind = kind;
	}

	public override string ToString()
	{
		return kind;
	}

	public override bool Equals(object obj)
	{
		if (obj is CodeActionKind codeActionKind)
		{
			return codeActionKind.kind == kind;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return 29 * (31 + kind.GetHashCode());
	}
}
