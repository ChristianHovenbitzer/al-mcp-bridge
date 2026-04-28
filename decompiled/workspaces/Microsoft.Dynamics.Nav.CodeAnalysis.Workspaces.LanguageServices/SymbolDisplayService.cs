using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.LanguageServices;

internal class SymbolDisplayService : AbstractSymbolDisplayService
{
	protected class SymbolDescriptionBuilder : AbstractSymbolDescriptionBuilder
	{
		private static readonly SymbolDisplayFormat minimallyQualifiedFormat = new SymbolDisplayFormat(SymbolDisplayTypeQualificationStyle.NameOnly, SymbolDisplayKindOptions.None, SymbolDisplayApplicationObjectOptions.None, SymbolDisplayMethodOptions.IncludeName, SymbolDisplayParameterOptions.IncludeType | SymbolDisplayParameterOptions.IncludeName);

		protected override SymbolDisplayFormat MinimallyQualifiedFormat => minimallyQualifiedFormat;

		public SymbolDescriptionBuilder(ISymbolDisplayService displayService, SemanticModel semanticModel, int position, Workspace workspace, CancellationToken cancellationToken)
			: base(displayService, semanticModel, position, workspace, cancellationToken)
		{
		}

		protected override void AddDeprecatedPrefix()
		{
			AddToGroup(SymbolDescriptionGroups.MainDescription, Punctuation("["), PlainText(WorkspacesResources.Deprecated), Punctuation("]"), Space());
		}

		protected override void AddDescriptionForProperty(IPropertySymbol symbol)
		{
			IEnumerable<SymbolDisplayPart> enumerable = ToMinimalDisplayParts(symbol).SkipWhile((SymbolDisplayPart p) => p.Symbol == null);
			AddToGroup(SymbolDescriptionGroups.MainDescription, enumerable);
		}
	}

	public override ImmutableArray<SymbolDisplayPart> ToDisplayParts(ISymbol symbol, SymbolDisplayFormat format = null)
	{
		return symbol.ToDisplayParts(format);
	}

	public override ImmutableArray<SymbolDisplayPart> ToMinimalDisplayParts(SemanticModel semanticModel, int position, ISymbol symbol, SymbolDisplayFormat format)
	{
		return symbol.ToMinimalDisplayParts(semanticModel, position, format);
	}

	protected override AbstractSymbolDescriptionBuilder CreateDescriptionBuilder(Workspace workspace, SemanticModel semanticModel, int position, CancellationToken cancellationToken)
	{
		return new SymbolDescriptionBuilder(this, semanticModel, position, workspace, cancellationToken);
	}
}
