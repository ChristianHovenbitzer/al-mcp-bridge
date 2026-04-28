using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Log;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Completion;

internal class PropertyValueCompletionProvider : SymbolCompletionProvider
{
	private static readonly ImmutableArray<string> Tags = ImmutableArray.Create("PropertyValue");

	internal override bool IsDebuggerConsoleProvider => false;

	public override async Task ProvideCompletionsAsync(CompletionContext context, AbstractSyntaxContext memberSyntaxContext)
	{
		Document document = context.Document;
		CancellationToken cancellationToken = context.CancellationToken;
		using (Logger.LogBlock(FunctionId.Completion_PropertyValueProvider_ProvideCompletionsAsync, cancellationToken))
		{
			PropertyValuesRecommender propertyValuesRecommender = new BooleanValuesRecommender((MemberSyntaxContext)memberSyntaxContext);
			IEnumerable<PropertyValueRecommendation> obj = await RecommendPropertyValuesAsync(document, propertyValuesRecommender, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
			context.IsExclusive = propertyValuesRecommender.IsExclusive;
			Microsoft.Dynamics.Nav.CodeAnalysis.Syntax.SyntaxTokenExtensions.GetAncestor<PropertySyntax>(memberSyntaxContext.TargetToken);
			foreach (PropertyValueRecommendation item in obj)
			{
				context.AddItem(CreateItem(item));
			}
		}
	}

	protected virtual async Task<IEnumerable<PropertyValueRecommendation>> RecommendPropertyValuesAsync(Document document, PropertyValuesRecommender propertyValuesRecommender, CancellationToken cancellationToken)
	{
		if (!propertyValuesRecommender.Context.General.HasFlag(GeneralContexts.PropertyValue))
		{
			return ImmutableArray<PropertyValueRecommendation>.Empty;
		}
		return await propertyValuesRecommender.RecommendPropertyValuesAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
	}

	private static CompletionItem CreateItem(PropertyValueRecommendation recommendation)
	{
		string descriptionValue;
		string obsoleteInformation;
		string name;
		string filterText;
		if (recommendation.Symbol != null)
		{
			string displayText = recommendation.Symbol.ToDisplayString(SymbolDisplayFormat.SymbolCompletionFormat);
			string text = recommendation.InsertionText ?? recommendation.DisplayText.QuoteIdentifierIfNeeded();
			filterText = GetFilterText(recommendation.Symbol, recommendation.DisplayText);
			name = recommendation.Symbol.Name;
			object symbols = ImmutableArray.Create(recommendation.Symbol);
			string sortText = name;
			string insertionText = text;
			obsoleteInformation = recommendation.ObsoleteInformation;
			descriptionValue = recommendation.DescriptionValue;
			bool addUsingStatementWhenCompleting = recommendation.AddUsingStatementWhenCompleting;
			return SymbolCompletionItem.Create(displayText, default(TextSpan), (IReadOnlyList<ISymbol>)symbols, -1, -1, sortText, insertionText, null, filterText, preselect: false, null, isArgumentName: false, null, default(ImmutableArray<string>), null, isSnippet: false, shouldSerializeItem: false, obsoleteInformation, descriptionValue, addUsingStatementWhenCompleting);
		}
		string displayText2 = recommendation.DisplayText;
		descriptionValue = recommendation.DetailText;
		obsoleteInformation = recommendation.InsertionText ?? recommendation.DisplayText.QuoteIdentifierIfNeeded();
		name = recommendation.DescriptionValue;
		filterText = recommendation.ObsoleteInformation;
		Glyph? glyph = ((recommendation.Glyph == Glyph.None) ? Glyph.Property : recommendation.Glyph);
		return CommonCompletionItem.Create(tags: Tags, isMarkdownDocs: recommendation.IsMarkdownDocs, isDeprecated: recommendation.IsDeprecated, documentation: recommendation.DocumentationText, displayText: displayText2, span: default(TextSpan), glyph: glyph, descriptionText: name, detailText: descriptionValue, sortText: null, filterText: null, insertionText: obsoleteInformation, obsoleteInformation: filterText);
	}

	protected static string GetFilterText(ISymbol symbol, string displayText)
	{
		return ((displayText == symbol.Name || (displayText.Length > 0 && displayText[0] == '@')) ? displayText : symbol.Name).QuoteIdentifier();
	}
}
