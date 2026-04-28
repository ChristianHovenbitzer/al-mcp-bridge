using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.CodeAnalysis.Collections;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Completion;

internal class EventSubscriberProcedureParametersRecommender : TextRecommender
{
	private const string SpaceToken = " ";

	private const string SenderVariableName = "sender";

	private const string ObjectSenderString = "Object Sender";

	private const string EventParameterString = "Event Parameter";

	private const string GlobalVariableString = "Global Variable";

	private const string TriggerParameterString = "Trigger Parameter";

	protected internal override async Task<IEnumerable<CompletionItem>> RecommendTextAsync(CompletionContext context, MemberSyntaxContext syntaxContext, CancellationToken cancellationToken)
	{
		SyntaxToken leftToken = syntaxContext.LeftToken;
		if (!IsLeftTokenEligibleForRecommender(leftToken))
		{
			return await base.RecommendTextAsync(context, syntaxContext, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		}
		string eventSubscriberAttribute = AttributeKind.EventSubscriber.ToString();
		MethodDeclarationSyntax methodDeclarationSyntax = (MethodDeclarationSyntax)leftToken.GetAncestor((SyntaxNode a) => a.IsKind(SyntaxKind.MethodDeclaration));
		int num;
		if (methodDeclarationSyntax == null)
		{
			num = 1;
		}
		else
		{
			_ = methodDeclarationSyntax.Attributes;
			num = 0;
		}
		if (num != 0 || !methodDeclarationSyntax.Attributes.Any((MemberAttributeSyntax x) => string.Equals(x.Name.ToString(), eventSubscriberAttribute, StringComparison.OrdinalIgnoreCase)))
		{
			return await base.RecommendTextAsync(context, syntaxContext, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		}
		MemberAttributeSyntax memberAttributeSyntax = methodDeclarationSyntax.Attributes.First((MemberAttributeSyntax x) => string.Equals(x.Name.ToString(), eventSubscriberAttribute, StringComparison.OrdinalIgnoreCase));
		if (memberAttributeSyntax == null || !(memberAttributeSyntax.ArgumentList?.Arguments).HasValue || memberAttributeSyntax.ArgumentList.Arguments.Count < 6)
		{
			return await base.RecommendTextAsync(context, syntaxContext, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		}
		MethodSymbol methodSymbol = (MethodSymbol)syntaxContext.SemanticModel.GetDeclaredSymbol(methodDeclarationSyntax);
		if (methodSymbol == null)
		{
			return await base.RecommendTextAsync(context, syntaxContext, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		}
		AttributeSymbol attributeSymbol = methodSymbol.Attributes.First(delegate(AttributeSymbol x)
		{
			AttributeTypeInfo attributeInfo = x.AttributeInfo;
			return attributeInfo != null && attributeInfo.Kind == AttributeKind.EventSubscriber;
		});
		ImmutableArray<ParameterSymbol> parameters = methodSymbol.Parameters;
		AttributeArgumentSymbol attributeArgumentSymbol = attributeSymbol.Arguments[0];
		SourceAttributeArgumentSymbol sourceAttributeArgumentSymbol = (SourceAttributeArgumentSymbol)attributeSymbol.Arguments[1];
		if (!Enum.TryParse<SymbolKind>(attributeArgumentSymbol.ValueText, out var result))
		{
			return await base.RecommendTextAsync(context, syntaxContext, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		}
		if (sourceAttributeArgumentSymbol.ValueAsInteger.HasValue)
		{
			int value = sourceAttributeArgumentSymbol.ValueAsInteger.Value;
			string valueText = attributeSymbol.Arguments[2].ValueText;
			List<CompletionItem> list = TryGetTriggerEventParameters(syntaxContext, result, value, valueText, parameters);
			if (!list.Any())
			{
				list = TryGetEventParameters(syntaxContext, result, value, valueText, parameters);
			}
			return FilterAndAdjustCompletionItemSpans(list, leftToken);
		}
		return await base.RecommendTextAsync(context, syntaxContext, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
	}

	private static bool IsLeftTokenEligibleForRecommender(SyntaxToken syntaxToken)
	{
		switch (syntaxToken.Kind)
		{
		case SyntaxKind.SemicolonToken:
		case SyntaxKind.OpenParenToken:
			return syntaxToken.ParentIsKind(SyntaxKind.ParameterList);
		case SyntaxKind.VarKeyword:
			return syntaxToken.ParentIsKind(SyntaxKind.Parameter);
		case SyntaxKind.IdentifierToken:
			return syntaxToken.Parent.ParentIsKind(SyntaxKind.Parameter);
		default:
			return false;
		}
	}

	private static List<CompletionItem> TryGetTriggerEventParameters(MemberSyntaxContext syntaxContext, SymbolKind objectType, int objectId, string eventName, ImmutableArray<ParameterSymbol> declaredParameters)
	{
		ImmutableArray<ISymbolWithId> objectSymbolsByIdAcrossModules = syntaxContext.SemanticModel.Compilation.GetObjectSymbolsByIdAcrossModules(objectType, objectId);
		if (objectSymbolsByIdAcrossModules.Length == 1 && objectSymbolsByIdAcrossModules[0].IsApplicationObjectSymbolWithTriggerEvents())
		{
			BuiltInTriggerEventSymbol builtInTriggerEventSymbol = ((ISymbolWithTriggerEvents)objectSymbolsByIdAcrossModules[0]).GetTriggerEvent(eventName) as BuiltInTriggerEventSymbol;
			if (builtInTriggerEventSymbol != null)
			{
				return GetTriggerParameterSuggestions(builtInTriggerEventSymbol, declaredParameters);
			}
		}
		return new List<CompletionItem>();
	}

	private static List<CompletionItem> TryGetEventParameters(MemberSyntaxContext syntaxContext, SymbolKind objectType, int objectId, string eventName, ImmutableArray<ParameterSymbol> declaredParameters)
	{
		List<CompletionItem> result = new List<CompletionItem>();
		ImmutableArray<ISymbolWithId> objectSymbolsByIdAcrossModules = syntaxContext.SemanticModel.Compilation.GetObjectSymbolsByIdAcrossModules(objectType, objectId);
		if (objectSymbolsByIdAcrossModules.Length == 1)
		{
			ApplicationObjectTypeSymbol applicationObjectTypeSymbol = (ApplicationObjectTypeSymbol)objectSymbolsByIdAcrossModules[0];
			IEnumerable<ISymbol> source = applicationObjectTypeSymbol.QueryAllEvents(eventName, syntaxContext.SemanticModel.Compilation);
			if (source.Count() == 1)
			{
				MethodSymbol procedure = (MethodSymbol)source.First();
				result = TryGetSuggestions(applicationObjectTypeSymbol, procedure, AttributeCategory.Business, declaredParameters);
				if (result.Count() > 0)
				{
					return result;
				}
				return TryGetSuggestions(applicationObjectTypeSymbol, procedure, AttributeCategory.Integration, declaredParameters);
			}
		}
		return result;
	}

	private static List<CompletionItem> TryGetSuggestions(ApplicationObjectTypeSymbol publisher, MethodSymbol procedure, AttributeCategory kind, ImmutableArray<ParameterSymbol> declaredParameters)
	{
		bool tryIncludeGlobals = kind == AttributeCategory.Integration;
		return GetParametersVariablesAndSenderSuggestions(publisher, procedure, kind, declaredParameters, tryIncludeGlobals);
	}

	private static List<CompletionItem> GetParametersVariablesAndSenderSuggestions(ApplicationObjectTypeSymbol publisher, MethodSymbol procedure, AttributeCategory kind, ImmutableArray<ParameterSymbol> declaredParameters, bool tryIncludeGlobals = false)
	{
		List<CompletionItem> list = new List<CompletionItem>();
		AttributeSymbol attributeSymbol = procedure.Attributes.FirstOrDefault(delegate(AttributeSymbol a)
		{
			AttributeTypeInfo attributeInfo = a.AttributeInfo;
			return attributeInfo != null && attributeInfo.Category == kind;
		});
		if (attributeSymbol == null)
		{
			return new List<CompletionItem>();
		}
		List<CompletionItem> suggestions = GetSuggestions(procedure.Parameters, CreateParameterItem, declaredParameters);
		list.AddRange(suggestions);
		if (attributeSymbol.Arguments.Length > 0)
		{
			if (attributeSymbol.Arguments[0].ValueAsBoolean.GetValueOrDefault())
			{
				CompletionItem senderSuggestion = GetSenderSuggestion(publisher, declaredParameters);
				if (senderSuggestion != null)
				{
					list.Add(senderSuggestion);
				}
			}
			if (tryIncludeGlobals && attributeSymbol.Arguments.Length > 1 && attributeSymbol.Arguments[1].ValueAsBoolean.GetValueOrDefault())
			{
				List<CompletionItem> suggestions2 = GetSuggestions(publisher.GlobalVariables, CreateVariableItem, declaredParameters);
				list.AddRange(suggestions2);
			}
		}
		return list;
	}

	private static List<CompletionItem> GetSuggestions<T>(IEnumerable<T> list, Func<T, CompletionItem> call, ImmutableArray<ParameterSymbol> declaredParameters) where T : ISymbolWithType
	{
		List<CompletionItem> list2 = new List<CompletionItem>();
		foreach (T member in list)
		{
			if (declaredParameters.All((ParameterSymbol x) => !SemanticFacts.IsSameName(x.Name, member.Name)))
			{
				list2.Add(call(member));
			}
		}
		return list2;
	}

	private static List<CompletionItem> GetTriggerParameterSuggestions(BuiltInTriggerEventSymbol triggerEvent, ImmutableArray<ParameterSymbol> declaredParameters)
	{
		List<CompletionItem> list = new List<CompletionItem>();
		ImmutableArray<ParameterSymbol>.Enumerator enumerator = triggerEvent.Parameters.GetEnumerator();
		while (enumerator.MoveNext())
		{
			ParameterSymbol param = enumerator.Current;
			if (declaredParameters.All((ParameterSymbol x) => !SemanticFacts.IsSameName(x.Name, param.Name) || x.Type != param.Type || x.IsVar != param.IsVar) && ((!SemanticFacts.IsSameName(param.Name, "Rec") && !SemanticFacts.IsSameName(param.Name, "xRec")) || !(param.Type == NavCorLib.NoneType)))
			{
				list.Add(CreateParameterItem(triggerEvent, param));
			}
		}
		return list;
	}

	private static CompletionItem GetSenderSuggestion(ApplicationObjectTypeSymbol publisher, ImmutableArray<ParameterSymbol> declaredParameters)
	{
		if (declaredParameters.Any((ParameterSymbol x) => SemanticFacts.IsSameName(x.Name, "sender")))
		{
			return null;
		}
		PooledStringBuilder instance = PooledStringBuilder.GetInstance();
		StringBuilder builder = instance.Builder;
		builder.Append("sender");
		builder.Append(SyntaxKind.ColonToken.GetText());
		builder.Append(" ");
		if (publisher.Kind == SymbolKind.Table)
		{
			builder.Append(NavTypeKind.Record.ToString());
		}
		else
		{
			builder.Append(publisher.Kind);
		}
		builder.Append(" ");
		builder.Append(publisher.Name.QuoteIdentifierIfNeeded());
		string text = instance.ToStringAndFree();
		return CreateItem(text, text, "Object Sender", null, ObsoleteSymbolHelper.GetObsoleteInformationMessage(publisher));
	}

	private static CompletionItem CreateVariableItem(VariableSymbol variable)
	{
		PooledStringBuilder instance = PooledStringBuilder.GetInstance();
		StringBuilder builder = instance.Builder;
		builder.Append(variable.Name);
		builder.Append(SyntaxKind.ColonToken.GetText());
		builder.Append(" ");
		if (!variable.Type.IsGenericType())
		{
			if (variable.Type.TypeCategory == TypeCategoryKind.ApplicationObject)
			{
				builder.Append(variable.Type.NavTypeKind);
				builder.Append(" ");
			}
			builder.Append(variable.Type.Name.QuoteIdentifierIfNeeded());
			if (variable.Type.HasLength)
			{
				builder.Append(SyntaxKind.OpenBracketToken.GetText());
				builder.Append(variable.Type.Length);
				builder.Append(SyntaxKind.CloseBracketToken.GetText());
			}
		}
		else
		{
			builder.Append(variable.Type.OriginalDefinition);
		}
		string text = instance.ToStringAndFree();
		return CreateItem(text, text, "Global Variable", null, ObsoleteSymbolHelper.GetObsoleteInformationMessage(variable));
	}

	private static CompletionItem CreateParameterItem(ParameterSymbol parameter)
	{
		string text = parameter.ToDisplayString(SymbolDisplayFormat.MethodCompletionFormat);
		return CreateItem(text, text, "Event Parameter");
	}

	private static CompletionItem CreateParameterItem(BuiltInTriggerEventSymbol triggerEvent, ParameterSymbol parameter)
	{
		string text = parameter.ToDisplayString(SymbolDisplayFormat.MethodCompletionFormat);
		string documentation = parameter.GetDocumentation(triggerEvent);
		string obsoleteInformationMessage = ObsoleteSymbolHelper.GetObsoleteInformationMessage(parameter.Type);
		documentation = documentation?.GetDocumentationText(obsoleteInformationMessage, isMarkdownDocs: true);
		return CreateItem(text, text, "Trigger Parameter", documentation, obsoleteInformationMessage);
	}

	private static CompletionItem CreateItem(string displayText, string insertionText, string detailText, string documentation = null, string obsoleteInformation = null)
	{
		string documentation2 = documentation ?? string.Empty;
		Glyph? glyph = Glyph.Parameter;
		return CommonCompletionItem.Create(displayText, default(TextSpan), glyph, null, documentation2, detailText, null, null, insertionText, obsoleteInformation, preselect: false, showsWarningIcon: false, shouldFormatOnCommit: false, isArgumentName: false, isSnippet: false, isMarkdownDocs: true);
	}

	private static IEnumerable<CompletionItem> FilterAndAdjustCompletionItemSpans(List<CompletionItem> items, SyntaxToken token)
	{
		ParameterSyntax parameterSyntax = null;
		switch (token.Kind)
		{
		case SyntaxKind.IdentifierToken:
			parameterSyntax = token.Parent.Parent as ParameterSyntax;
			break;
		case SyntaxKind.VarKeyword:
			parameterSyntax = token.Parent as ParameterSyntax;
			break;
		}
		if (parameterSyntax != null)
		{
			string text = parameterSyntax.GetText().ToString();
			ArrayBuilder<CompletionItem> instance = ArrayBuilder<CompletionItem>.GetInstance();
			try
			{
				foreach (CompletionItem item in items)
				{
					if (item.InsertionText.StartsWith(text, StringComparison.OrdinalIgnoreCase))
					{
						instance.Add(item.WithSpan(new TextSpan(parameterSyntax.Span.Start, text.Length)));
					}
				}
				return instance.ToArray();
			}
			finally
			{
				instance.Free();
			}
		}
		return items;
	}
}
