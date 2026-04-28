using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Host;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.LanguageServices;

internal abstract class AbstractSymbolDisplayService : ISymbolDisplayService, ILanguageService
{
	protected abstract class AbstractSymbolDescriptionBuilder
	{
		private readonly ISymbolDisplayService displayService;

		private readonly SemanticModel semanticModel;

		private readonly int position;

		private readonly Dictionary<SymbolDescriptionGroups, IList<SymbolDisplayPart>> groupMap = new Dictionary<SymbolDescriptionGroups, IList<SymbolDisplayPart>>();

		protected readonly Workspace Workspace;

		protected readonly CancellationToken CancellationToken;

		protected abstract SymbolDisplayFormat MinimallyQualifiedFormat { get; }

		protected AbstractSymbolDescriptionBuilder(ISymbolDisplayService displayService, SemanticModel semanticModel, int position, Workspace workspace, CancellationToken cancellationToken)
		{
			this.displayService = displayService;
			Workspace = workspace;
			CancellationToken = cancellationToken;
			this.semanticModel = semanticModel;
			this.position = position;
		}

		public async Task<ImmutableArray<SymbolDisplayPart>> BuildDescriptionAsync(ImmutableArray<ISymbol> symbolGroup, SymbolDescriptionGroups groups)
		{
			Contract.ThrowIfFalse(symbolGroup.Length > 0);
			await AddPartsAsync(symbolGroup).ConfigureAwait(continueOnCapturedContext: false);
			return BuildDescription(groups);
		}

		public async Task<IDictionary<SymbolDescriptionGroups, ImmutableArray<SymbolDisplayPart>>> BuildDescriptionSectionsAsync(ImmutableArray<ISymbol> symbolGroup)
		{
			Contract.ThrowIfFalse(symbolGroup.Length > 0);
			await AddPartsAsync(symbolGroup).ConfigureAwait(continueOnCapturedContext: false);
			return BuildDescriptionSections();
		}

		protected abstract void AddDeprecatedPrefix();

		protected void AddToGroup(SymbolDescriptionGroups group, params SymbolDisplayPart[] partsArray)
		{
			AddToGroup(group, new IEnumerable<SymbolDisplayPart>[1] { partsArray });
		}

		protected void AddToGroup(SymbolDescriptionGroups group, params IEnumerable<SymbolDisplayPart>[] partsArray)
		{
			List<SymbolDisplayPart> list = partsArray.Flatten().ToList();
			if (list.Count > 0)
			{
				if (!groupMap.TryGetValue(group, out IList<SymbolDisplayPart> value))
				{
					value = new List<SymbolDisplayPart>();
					groupMap.Add(group, value);
				}
				value.AddRange(list);
			}
		}

		protected IEnumerable<SymbolDisplayPart> LineBreak(int count = 1)
		{
			for (int i = 0; i < count; i++)
			{
				yield return new SymbolDisplayPart(SymbolDisplayPartKind.LineBreak, null, Environment.NewLine);
			}
		}

		protected IEnumerable<SymbolDisplayPart> PlainText(string text)
		{
			return Part(SymbolDisplayPartKind.Text, text);
		}

		protected IEnumerable<SymbolDisplayPart> Punctuation(string text)
		{
			return Part(SymbolDisplayPartKind.Punctuation, text);
		}

		protected IEnumerable<SymbolDisplayPart> Space(int count = 1)
		{
			yield return new SymbolDisplayPart(SymbolDisplayPartKind.Space, null, new string(' ', count));
		}

		protected SemanticModel GetSemanticModel(SyntaxTree tree)
		{
			if (this.semanticModel.SyntaxTree == tree)
			{
				return this.semanticModel;
			}
			SemanticModel semanticModel = this.semanticModel;
			if (semanticModel.Compilation.ContainsSyntaxTree(tree))
			{
				return semanticModel.Compilation.GetSemanticModel(tree);
			}
			return null;
		}

		protected IEnumerable<SymbolDisplayPart> ToMinimalDisplayParts(ISymbol symbol, SymbolDisplayFormat format = null)
		{
			format = format ?? MinimallyQualifiedFormat;
			return displayService.ToMinimalDisplayParts(semanticModel, position, symbol, format);
		}

		protected IEnumerable<SymbolDisplayPart> ToDisplayParts(ISymbol symbol, SymbolDisplayFormat format = null)
		{
			return displayService.ToDisplayParts(symbol, format);
		}

		private IEnumerable<SymbolDisplayPart> Part(SymbolDisplayPartKind kind, ISymbol symbol, string text)
		{
			yield return new SymbolDisplayPart(kind, symbol, text);
		}

		private IEnumerable<SymbolDisplayPart> Part(SymbolDisplayPartKind kind, string text)
		{
			return Part(kind, null, text);
		}

		private async Task AddPartsAsync(ImmutableArray<ISymbol> symbols)
		{
			await AddDescriptionPartAsync(symbols[0]).ConfigureAwait(continueOnCapturedContext: false);
		}

		private Task AddDescriptionPartAsync(ISymbol symbol)
		{
			switch (symbol.Kind)
			{
			case SymbolKind.Method:
				AddDescriptionForMethod((IMethodSymbol)symbol);
				break;
			case SymbolKind.Parameter:
				AddDescriptionForParameter((IParameterSymbol)symbol);
				break;
			case SymbolKind.Field:
				AddDescriptionForField((FieldSymbol)symbol);
				break;
			case SymbolKind.Property:
				AddDescriptionForProperty((IPropertySymbol)symbol);
				break;
			case SymbolKind.GlobalVariable:
			case SymbolKind.LocalVariable:
				AddDescriptionForVariable((SourceVariableSymbol)symbol);
				break;
			default:
				AddDescriptionForArbitrarySymbol(symbol);
				break;
			}
			return Task.FromResult(result: true);
		}

		private ImmutableArray<SymbolDisplayPart> BuildDescription(SymbolDescriptionGroups groups)
		{
			List<SymbolDisplayPart> list = new List<SymbolDisplayPart>();
			foreach (SymbolDescriptionGroups item in groupMap.Keys.OrderBy((SymbolDescriptionGroups g1, SymbolDescriptionGroups g2) => g1 - g2))
			{
				if ((groups & item) != 0)
				{
					if (!list.IsEmpty())
					{
						int precedingNewLineCount = GetPrecedingNewLineCount(item);
						list.AddRange(LineBreak(precedingNewLineCount));
					}
					IList<SymbolDisplayPart> collection = groupMap[item];
					list.AddRange(collection);
				}
			}
			return list.ToImmutableArray();
		}

		private static int GetPrecedingNewLineCount(SymbolDescriptionGroups group)
		{
			return group switch
			{
				SymbolDescriptionGroups.MainDescription => 0, 
				SymbolDescriptionGroups.Documentation => 1, 
				_ => Contract.FailWithReturn<int>("unknown part kind"), 
			};
		}

		private IDictionary<SymbolDescriptionGroups, ImmutableArray<SymbolDisplayPart>> BuildDescriptionSections()
		{
			return groupMap.ToDictionary<KeyValuePair<SymbolDescriptionGroups, IList<SymbolDisplayPart>>, SymbolDescriptionGroups, ImmutableArray<SymbolDisplayPart>>((KeyValuePair<SymbolDescriptionGroups, IList<SymbolDisplayPart>> kvp) => kvp.Key, (KeyValuePair<SymbolDescriptionGroups, IList<SymbolDisplayPart>> kvp) => kvp.Value.ToImmutableArrayOrEmpty());
		}

		private void AddDescriptionForVariable(VariableSymbol symbol)
		{
			AddDescriptionForArbitrarySymbol(symbol, SymbolDisplayFormat.ShortFormat);
		}

		private void AddDescriptionForParameter(IParameterSymbol symbol)
		{
			AddDescriptionForArbitrarySymbol(symbol, SymbolDisplayFormat.ShortFormat);
		}

		private void AddDescriptionForField(FieldSymbol symbol)
		{
			AddDescriptionForArbitrarySymbol(symbol, SymbolDisplayFormat.ShortFormat);
		}

		private void AddDescriptionForMethod(IMethodSymbol symbol)
		{
			AddDescriptionForArbitrarySymbol(symbol, SymbolDisplayFormat.SignatureFormat);
		}

		protected virtual void AddDescriptionForProperty(IPropertySymbol symbol)
		{
			AddDescriptionForArbitrarySymbol(symbol, SymbolDisplayFormat.ShortFormat);
		}

		private void AddDescriptionForArbitrarySymbol(ISymbol symbol, SymbolDisplayFormat format = null)
		{
			AddToGroup(SymbolDescriptionGroups.MainDescription, ToMinimalDisplayParts(symbol, format));
		}
	}

	public abstract ImmutableArray<SymbolDisplayPart> ToDisplayParts(ISymbol symbol, SymbolDisplayFormat format = null);

	public abstract ImmutableArray<SymbolDisplayPart> ToMinimalDisplayParts(SemanticModel semanticModel, int position, ISymbol symbol, SymbolDisplayFormat format);

	protected abstract AbstractSymbolDescriptionBuilder CreateDescriptionBuilder(Workspace workspace, SemanticModel semanticModel, int position, CancellationToken cancellationToken);

	public string ToDisplayString(ISymbol symbol, SymbolDisplayFormat format = null)
	{
		return ToDisplayParts(symbol, format).ToDisplayString();
	}

	public string ToMinimalDisplayString(SemanticModel semanticModel, int position, ISymbol symbol, SymbolDisplayFormat format = null)
	{
		return ToMinimalDisplayParts(semanticModel, position, symbol, format).ToDisplayString();
	}

	public Task<string> ToDescriptionStringAsync(Workspace workspace, SemanticModel semanticModel, int position, ISymbol symbol, SymbolDescriptionGroups groups, CancellationToken cancellationToken)
	{
		return ToDescriptionStringAsync(workspace, semanticModel, position, ImmutableArray.Create(symbol), groups, cancellationToken);
	}

	public async Task<string> ToDescriptionStringAsync(Workspace workspace, SemanticModel semanticModel, int position, ImmutableArray<ISymbol> symbols, SymbolDescriptionGroups groups, CancellationToken cancellationToken)
	{
		return (await ToDescriptionPartsAsync(workspace, semanticModel, position, symbols, groups, cancellationToken).ConfigureAwait(continueOnCapturedContext: false)).ToDisplayString();
	}

	public async Task<ImmutableArray<SymbolDisplayPart>> ToDescriptionPartsAsync(Workspace workspace, SemanticModel semanticModel, int position, ImmutableArray<ISymbol> symbols, SymbolDescriptionGroups groups, CancellationToken cancellationToken)
	{
		if (symbols.Length == 0)
		{
			return ImmutableArray.Create<SymbolDisplayPart>();
		}
		return await CreateDescriptionBuilder(workspace, semanticModel, position, cancellationToken).BuildDescriptionAsync(symbols, groups).ConfigureAwait(continueOnCapturedContext: false);
	}

	public async Task<IDictionary<SymbolDescriptionGroups, ImmutableArray<SymbolDisplayPart>>> ToDescriptionGroupsAsync(Workspace workspace, SemanticModel semanticModel, int position, ImmutableArray<ISymbol> symbols, CancellationToken cancellationToken)
	{
		if (symbols.Length == 0)
		{
			return SpecializedCollections.EmptyDictionary<SymbolDescriptionGroups, ImmutableArray<SymbolDisplayPart>>();
		}
		return await CreateDescriptionBuilder(workspace, semanticModel, position, cancellationToken).BuildDescriptionSectionsAsync(symbols).ConfigureAwait(continueOnCapturedContext: false);
	}
}
