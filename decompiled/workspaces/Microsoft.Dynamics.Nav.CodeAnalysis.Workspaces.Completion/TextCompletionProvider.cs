using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Log;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Completion;

internal class TextCompletionProvider : CompletionProvider
{
	private static readonly TextRecommender[] Recommenders = new TextRecommender[25]
	{
		new MemberNameRecommender(),
		new FieldgroupNameRecommender(),
		new AreaNameRecommender(),
		new SystemActionNameRecommender(),
		new TriggersRecommender(),
		new TypesRecommender(),
		new KeywordsRecommender(),
		new AttributeNameRecommender(),
		new SystemPartNameRecommender(),
		new ApplicationObjectIdRecommender(),
		new TableFieldIdRecommender(),
		new EnumValueIdRecommender(),
		new BooleanParameterRecommender(),
		new BooleanAttributeArgumentRecommender(),
		new EventSubscriberProcedureParametersRecommender(),
		new EventSubscriberArgumentRecommender(),
		new PropertyExpressionOptionAccessRecommender(),
		new EventTriggersRecommender(),
		new LabelPropertyNameRecommender(),
		new FilterExpressionRecommender(),
		new SortingKeyNameRecommender(),
		new InherentPermissionsAttributeRecommender(),
		new RequiredPermissionsAttributeRecommender(),
		new ExternalBusinessEventCategoryRecommender(),
		new ConfigurationDialogSnippetRecommender()
	};

	public override async Task ProvideCompletionsAsync(CompletionContext context, AbstractSyntaxContext memberSyntaxContext)
	{
		CancellationToken cancellationToken = context.CancellationToken;
		cancellationToken.ThrowIfCancellationRequested();
		using (Logger.LogBlock(FunctionId.Completion_SnippetCompletionProvider_ProvideCompletionAsync, cancellationToken))
		{
			TextRecommender[] recommenders = Recommenders;
			for (int i = 0; i < recommenders.Length; i++)
			{
				IEnumerable<CompletionItem> enumerable = await recommenders[i].RecommendTextAsync(context, memberSyntaxContext as MemberSyntaxContext, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
				if (enumerable.Any())
				{
					context.AddItems(enumerable);
				}
			}
		}
	}
}
