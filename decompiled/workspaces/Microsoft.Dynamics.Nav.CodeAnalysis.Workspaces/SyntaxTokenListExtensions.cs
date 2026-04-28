using Microsoft.Dynamics.Nav.CodeAnalysis.Collections;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces;

internal static class SyntaxTokenListExtensions
{
	internal static string GetValueText(this SyntaxTokenList tokens)
	{
		switch (tokens.Count)
		{
		case 0:
			return string.Empty;
		case 1:
			return tokens[0].ValueText;
		default:
		{
			PooledStringBuilder instance = PooledStringBuilder.GetInstance();
			SyntaxTokenList.Enumerator enumerator = tokens.GetEnumerator();
			while (enumerator.MoveNext())
			{
				SyntaxToken current = enumerator.Current;
				instance.Builder.Append(current.ValueText);
			}
			return instance.ToStringAndFree();
		}
		}
	}
}
