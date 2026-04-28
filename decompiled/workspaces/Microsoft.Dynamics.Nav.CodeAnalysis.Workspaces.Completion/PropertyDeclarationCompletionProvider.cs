using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Log;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Options;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Completion;

internal class PropertyDeclarationCompletionProvider : SymbolCompletionProvider
{
	private static readonly ImmutableArray<string> Tags = ImmutableArray.Create("Property");

	internal override bool IsDebuggerConsoleProvider => false;

	public override async Task ProvideCompletionsAsync(CompletionContext context, AbstractSyntaxContext memberSyntaxContext)
	{
		Document document = context.Document;
		CancellationToken cancellationToken = context.CancellationToken;
		using (Logger.LogBlock(FunctionId.Completion_PropertyDeclarationProvider_ProvideCompletionsAsync, cancellationToken))
		{
			foreach (PropertyTypeInfo item in await RecommendPropertiesAsync(document, memberSyntaxContext as MemberSyntaxContext, context.Options, cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
			{
				context.AddItem(CreateItem(item));
			}
		}
	}

	protected virtual Task<IEnumerable<PropertyTypeInfo>> RecommendPropertiesAsync(Document document, MemberSyntaxContext context, OptionSet options, CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();
		if (!context.General.HasFlag(GeneralContexts.PropertyDeclaration))
		{
			return Task.FromResult(SpecializedCollections.EmptyEnumerable<PropertyTypeInfo>());
		}
		SyntaxNode parent = context.LeftToken.Parent;
		if (parent.Kind == SyntaxKind.PropertyName)
		{
			parent = parent.Parent.Parent.Parent;
		}
		if (parent.Kind == SyntaxKind.Property)
		{
			parent = parent.Parent.Parent;
		}
		IEnumerable<PropertyTypeInfo> availableProperies = GetAvailableProperies(context, parent, cancellationToken);
		availableProperies = FilterModifiableProperties(context, parent, availableProperies);
		availableProperies = FilterAlreadyDeclaredProperties(context, parent, availableProperies, cancellationToken);
		availableProperies = FilterDependentProperties(parent, availableProperies);
		availableProperies = FilterPageCustomizationProperties(context, parent, availableProperies, cancellationToken);
		availableProperies = FilterBlockedProperties(parent, availableProperies, cancellationToken);
		return Task.FromResult(availableProperies);
	}

	private static IEnumerable<PropertyTypeInfo> GetAvailableProperies(MemberSyntaxContext context, SyntaxNode node, CancellationToken cancellationToken)
	{
		if (node.Kind.IsModifyChange())
		{
			ChangeModifySymbol changeModifySymbol = context.SemanticModel.GetDeclaredSymbol(node, cancellationToken) as ChangeModifySymbol;
			if (changeModifySymbol?.Target != null)
			{
				return changeModifySymbol.Target.PropertyTypeInfos.Values;
			}
		}
		return node.GetAllDefinedPropertyNames();
	}

	private static IEnumerable<PropertyTypeInfo> FilterAlreadyDeclaredProperties(MemberSyntaxContext context, SyntaxNode anchorNode, IEnumerable<PropertyTypeInfo> properties, CancellationToken cancellationToken)
	{
		ImmutableHashSet<string> declaredProperties = (from s in context.SemanticModel.LookupSymbols(anchorNode, LookupOptions.OnlyPropertyDeclarations, null, null, cancellationToken)
			where !s.IsSynthesized
			select s.Name).ToImmutableHashSet(SemanticFacts.NameEqualityComparer);
		return properties.Where((PropertyTypeInfo p) => !declaredProperties.Contains(p.Name));
	}

	private static IEnumerable<PropertyTypeInfo> FilterDependentProperties(SyntaxNode anchorNode, IEnumerable<PropertyTypeInfo> properties)
	{
		foreach (PropertyTypeInfo property in properties)
		{
			bool flag = false;
			ImmutableArray<DependentProperty>.Enumerator enumerator2 = property.DependentProperties.GetEnumerator();
			while (enumerator2.MoveNext())
			{
				DependentProperty current2 = enumerator2.Current;
				PropertyValueSyntax propertyValue = anchorNode.GetPropertyValue(current2.Name);
				if (propertyValue != null && current2.Values.Count > 0)
				{
					string propertyValueText = propertyValue.GetPropertyValueText();
					if (!current2.Values.Contains(propertyValueText))
					{
						flag = true;
						break;
					}
				}
			}
			if (!flag)
			{
				yield return property;
			}
		}
	}

	private static IEnumerable<PropertyTypeInfo> FilterModifiableProperties(MemberSyntaxContext context, SyntaxNode anchorNode, IEnumerable<PropertyTypeInfo> properties)
	{
		if (anchorNode.Kind.IsApplicationObjectExtensionSyntax() || anchorNode.Kind.IsModifyChange())
		{
			bool flag = context.Page.HasFlag(PageContexts.View);
			if (!flag && context.DeclaringObject.IsKind(SyntaxKind.TableExtensionObject, SyntaxKind.PageExtensionObject, SyntaxKind.ReportExtensionObject))
			{
				properties = properties.Where((PropertyTypeInfo p) => p.Modification != ModificationKind.None);
			}
			else if (flag || context.DeclaringObject.IsKind(SyntaxKind.PageCustomizationObject))
			{
				properties = properties.Where((PropertyTypeInfo p) => p.Modification.HasFlag(ModificationKind.Customizations));
			}
		}
		return properties;
	}

	private static IEnumerable<PropertyTypeInfo> FilterPageCustomizationProperties(MemberSyntaxContext context, SyntaxNode anchorNode, IEnumerable<PropertyTypeInfo> properties, CancellationToken cancellationToken)
	{
		if (context.DeclaringObject.Kind == SyntaxKind.PageCustomizationObject && (context.Page.HasFlag(PageContexts.Control) || context.Page.HasFlag(PageContexts.ControlGroup)))
		{
			IControlSymbol controlSymbol = context.SemanticModel.GetDeclaredSymbol(anchorNode, cancellationToken) as IControlSymbol;
			return properties.Where((PropertyTypeInfo p) => controlSymbol == null || SemanticFacts.IsPropertyAllowedInPageCustomization(controlSymbol, p.Kind));
		}
		return properties;
	}

	private static IEnumerable<PropertyTypeInfo> FilterBlockedProperties(SyntaxNode anchorNode, IEnumerable<PropertyTypeInfo> properties, CancellationToken cancellationToken)
	{
		SyntaxNode syntaxNode = anchorNode.GetFirstParent(SyntaxKind.PageActionArea) ?? anchorNode.GetFirstParent(SyntaxKind.ActionAddChange);
		if (syntaxNode == null)
		{
			return properties;
		}
		string value = syntaxNode.GetNameStringValue();
		if (syntaxNode.Kind == SyntaxKind.ActionAddChange)
		{
			value = ((ActionAddChangeSyntax)syntaxNode).Anchor.GetIdentifierOrLiteralValue();
		}
		return SyntaxFacts.GetActionAreaKind(value) switch
		{
			ActionAreaKind.Prompting => properties.Where((PropertyTypeInfo p) => PageMemberSymbolHelpers.IsPropertyAllowedOnPromptingActions(p.Kind)), 
			ActionAreaKind.PromptGuide => properties.Where((PropertyTypeInfo p) => PromptDialogPageValidationHelper.IsPropertyAllowedOnPromptGuideActions(p.Kind)), 
			_ => properties, 
		};
	}

	private static CompletionItem CreateItem(PropertyTypeInfo property)
	{
		string text = CreateDisplayParts(property.Name).ToDisplayString();
		string documentation = property.GetDocumentation();
		string name = property.Name;
		string documentation2 = documentation;
		string detailText = text;
		Glyph? glyph = Glyph.Property;
		ImmutableArray<string> tags = Tags;
		return CommonCompletionItem.Create(name, default(TextSpan), glyph, null, documentation2, detailText, null, null, null, null, preselect: false, showsWarningIcon: false, shouldFormatOnCommit: false, isArgumentName: false, isSnippet: false, isMarkdownDocs: true, isDeprecated: false, null, tags);
	}

	private static ImmutableArray<SymbolDisplayPart> CreateDisplayParts(string property)
	{
		List<SymbolDisplayPart> list = new List<SymbolDisplayPart>();
		list.AddText(string.Format(CultureInfo.CurrentCulture, WorkspacesResources.Property, property));
		return list.ToImmutableArray();
	}
}
