using System.Collections.Immutable;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Completion;

internal static class GlyphTags
{
	public static ImmutableArray<string> GetTags(Glyph glyph)
	{
		return ImmutableArray.Create(glyph.ToString());
	}
}
