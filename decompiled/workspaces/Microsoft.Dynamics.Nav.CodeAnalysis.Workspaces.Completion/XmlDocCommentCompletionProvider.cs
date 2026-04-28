using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.CodeAnalysis.Collections;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Options;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Completion;

internal class XmlDocCommentCompletionProvider : AbstractDocCommentCompletionProvider<DocumentationCommentTriviaSyntax>
{
	private static readonly CompletionItemRules s_defaultRules = CompletionItemRules.Create(AbstractDocCommentCompletionProvider<DocumentationCommentTriviaSyntax>.FilterRules, ImmutableArray.Create(CharacterSetModificationRule.Create(CharacterSetModificationKind.Add, '>', '\t')), EnterKeyRule.Never);

	public XmlDocCommentCompletionProvider()
		: base(s_defaultRules)
	{
	}

	internal override bool IsInsertionTrigger(SourceText text, int characterPosition, OptionSet options)
	{
		char c = text[characterPosition];
		if (c != '<' && c != '"' && c != '/')
		{
			return CompletionUtilities.IsTriggerAfterSpaceOrStartOfWordCharacter(text, characterPosition, options);
		}
		return true;
	}

	protected override async Task<IEnumerable<CompletionItem>?> GetItemsWorkerAsync(Document document, int position, CompletionTrigger trigger, CancellationToken cancellationToken)
	{
		_ = 1;
		try
		{
			SyntaxTree syntaxTree = await document.GetSyntaxTreeAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
			SyntaxToken token = syntaxTree.FindTokenOnLeftOfPosition(position, cancellationToken, includeSkipped: true, includeDirectives: false, includeDocumentationComments: true);
			DocumentationCommentTriviaSyntax parentTrivia = Microsoft.Dynamics.Nav.CodeAnalysis.Syntax.SyntaxTokenExtensions.GetAncestor<DocumentationCommentTriviaSyntax>(token);
			if (parentTrivia == null)
			{
				token = syntaxTree.FindTokenOrEndToken(position, cancellationToken);
				parentTrivia = Microsoft.Dynamics.Nav.CodeAnalysis.Syntax.SyntaxTokenExtensions.GetAncestor<DocumentationCommentTriviaSyntax>(token);
			}
			if (parentTrivia == null)
			{
				return null;
			}
			SyntaxToken attachedToken = parentTrivia.ParentTrivia.Token;
			if (attachedToken.Kind == SyntaxKind.None)
			{
				return null;
			}
			SemanticModel semanticModel = await document.GetSemanticModelForNodeAsync(attachedToken.Parent, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
			ISymbol declaredSymbol = GetDeclaredSymbol(attachedToken, semanticModel, cancellationToken);
			if (declaredSymbol != null && token.Kind == SyntaxKind.XmlTextLiteralNewLineToken && trigger.Character == '/')
			{
				SyntaxReference? declaringSyntaxReference = declaredSymbol.DeclaringSyntaxReference;
				if (declaringSyntaxReference != null && declaringSyntaxReference.GetSyntax().GetFirstToken() == attachedToken && token.LeadingTrivia.Last().Kind == SyntaxKind.DocumentationCommentExteriorTrivia)
				{
					return CreateDocumentationCommentSnippet(declaredSymbol);
				}
			}
			if (IsAttributeNameContext(token, position, out string elementName, out ISet<string> attributeNames))
			{
				return GetAttributeItems(elementName, attributeNames);
			}
			if (trigger.Kind == CompletionTriggerKind.Insertion && trigger.Character == ' ')
			{
				return null;
			}
			if (IsAttributeValueContext(token, out elementName, out string attributeName))
			{
				return GetAttributeValueItems(declaredSymbol, elementName, attributeName);
			}
			if (trigger.Kind == CompletionTriggerKind.Insertion && trigger.Character != '<')
			{
				return null;
			}
			List<CompletionItem> list = new List<CompletionItem>();
			if (token.Parent.Kind == SyntaxKind.XmlEmptyElement || token.Parent.Kind == SyntaxKind.XmlText || (token.Parent.IsKind(SyntaxKind.XmlElementEndTag) && token.IsKind(SyntaxKind.GreaterThanToken)) || (token.Parent.IsKind(SyntaxKind.XmlName) && token.Parent.IsParentKind(SyntaxKind.XmlEmptyElement)))
			{
				if (token.Parent.Parent.Kind == SyntaxKind.XmlElement || token.Parent.Parent.IsParentKind(SyntaxKind.XmlElement))
				{
					bool includeKeywords = !token.IsKind(SyntaxKind.LessThanToken) && (!token.Parent.IsKind(SyntaxKind.XmlName) || token.HasLeadingTrivia);
					list.AddRange(GetNestedItems(declaredSymbol, includeKeywords, trigger.Character));
				}
				if (token.Parent.Parent is XmlElementSyntax xmlElementSyntax)
				{
					AddXmlElementItems(list, xmlElementSyntax.StartTag, trigger.Character);
				}
				if (token.Parent.IsParentKind(SyntaxKind.XmlEmptyElement) && token.Parent.Parent.Parent is XmlElementSyntax xmlElementSyntax2)
				{
					AddXmlElementItems(list, xmlElementSyntax2.StartTag, trigger.Character);
				}
				if (token.Parent.Parent is DocumentationCommentTriviaSyntax || (token.Parent.Parent.IsKind(SyntaxKind.XmlEmptyElement) && token.Parent.Parent.Parent is DocumentationCommentTriviaSyntax))
				{
					list.AddRange(GetTopLevelItems(declaredSymbol, parentTrivia, trigger.Character));
				}
			}
			if (token.Parent is XmlElementStartTagSyntax xmlElementStartTagSyntax && token == xmlElementStartTagSyntax.GreaterThanToken)
			{
				AddXmlElementItems(list, xmlElementStartTagSyntax, trigger.Character);
			}
			list.AddRange(GetAlwaysVisibleItems(trigger.Character));
			return list;
		}
		catch (Exception exception) when (FatalError.ReportWithoutCrashUnlessCanceled(exception))
		{
			return SpecializedCollections.EmptyEnumerable<CompletionItem>();
		}
	}

	private static ISymbol? GetDeclaredSymbol(SyntaxToken attachedToken, SemanticModel semanticModel, CancellationToken cancellationToken)
	{
		MethodDeclarationSyntax ancestor = Microsoft.Dynamics.Nav.CodeAnalysis.Syntax.SyntaxTokenExtensions.GetAncestor<MethodDeclarationSyntax>(attachedToken);
		if (ancestor != null)
		{
			return semanticModel.GetDeclaredSymbol(ancestor, cancellationToken);
		}
		FieldSyntax ancestor2 = Microsoft.Dynamics.Nav.CodeAnalysis.Syntax.SyntaxTokenExtensions.GetAncestor<FieldSyntax>(attachedToken);
		if (ancestor2 != null)
		{
			return semanticModel.GetDeclaredSymbol(ancestor2, cancellationToken);
		}
		ControlBaseSyntax ancestor3 = Microsoft.Dynamics.Nav.CodeAnalysis.Syntax.SyntaxTokenExtensions.GetAncestor<ControlBaseSyntax>(attachedToken);
		if (ancestor3 != null)
		{
			return semanticModel.GetDeclaredSymbol(ancestor3, cancellationToken);
		}
		QueryColumnSyntax ancestor4 = Microsoft.Dynamics.Nav.CodeAnalysis.Syntax.SyntaxTokenExtensions.GetAncestor<QueryColumnSyntax>(attachedToken);
		if (ancestor4 != null)
		{
			return semanticModel.GetDeclaredSymbol(ancestor4, cancellationToken);
		}
		QueryFilterSyntax ancestor5 = Microsoft.Dynamics.Nav.CodeAnalysis.Syntax.SyntaxTokenExtensions.GetAncestor<QueryFilterSyntax>(attachedToken);
		if (ancestor5 != null)
		{
			return semanticModel.GetDeclaredSymbol(ancestor5, cancellationToken);
		}
		VariableDeclarationBaseSyntax ancestor6 = Microsoft.Dynamics.Nav.CodeAnalysis.Syntax.SyntaxTokenExtensions.GetAncestor<VariableDeclarationBaseSyntax>(attachedToken);
		if (ancestor6 != null)
		{
			return semanticModel.GetDeclaredSymbol(ancestor6, cancellationToken);
		}
		EnumValueSyntax ancestor7 = Microsoft.Dynamics.Nav.CodeAnalysis.Syntax.SyntaxTokenExtensions.GetAncestor<EnumValueSyntax>(attachedToken);
		if (ancestor7 != null)
		{
			return semanticModel.GetDeclaredSymbol(ancestor7, cancellationToken);
		}
		ObjectSyntax ancestor8 = Microsoft.Dynamics.Nav.CodeAnalysis.Syntax.SyntaxTokenExtensions.GetAncestor<ObjectSyntax>(attachedToken);
		if (ancestor8 != null)
		{
			return semanticModel.GetDeclaredSymbol(ancestor8, cancellationToken);
		}
		return null;
	}

	private IEnumerable<CompletionItem> CreateDocumentationCommentSnippet(ISymbol declaredSymbol)
	{
		PooledStringBuilder instance = PooledStringBuilder.GetInstance();
		PooledStringBuilder instance2 = PooledStringBuilder.GetInstance();
		instance.Builder.Append("/// <summary>\n///\n/// </summary>");
		instance2.Builder.Append(" <summary>\n/// $1\n/// </summary>");
		int num = 2;
		ImmutableArray<IParameterSymbol>.Enumerator enumerator = declaredSymbol.GetParameters().GetEnumerator();
		while (enumerator.MoveNext())
		{
			string name = enumerator.Current.Name;
			StringBuilder builder = instance.Builder;
			StringBuilder stringBuilder = builder;
			StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(28, 1, builder);
			handler.AppendLiteral("\n/// <param name=\"");
			handler.AppendFormatted(name);
			handler.AppendLiteral("\"></param>");
			stringBuilder.Append(ref handler);
			builder = instance2.Builder;
			StringBuilder stringBuilder2 = builder;
			handler = new StringBuilder.AppendInterpolatedStringHandler(29, 2, builder);
			handler.AppendLiteral("\n/// <param name=\"");
			handler.AppendFormatted(name);
			handler.AppendLiteral("\">$");
			handler.AppendFormatted(num);
			handler.AppendLiteral("</param>");
			stringBuilder2.Append(ref handler);
			num++;
		}
		if (declaredSymbol.GetReturnValue() != null)
		{
			instance.Builder.Append("\n/// <returns></returns>");
			StringBuilder builder = instance2.Builder;
			StringBuilder stringBuilder3 = builder;
			StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(25, 1, builder);
			handler.AppendLiteral("\n/// <returns>$");
			handler.AppendFormatted(num);
			handler.AppendLiteral("</returns>");
			stringBuilder3.Append(ref handler);
		}
		CompletionItem[] array = new CompletionItem[1];
		string descriptionText = instance.ToStringAndFree();
		string insertionText = instance2.ToStringAndFree();
		array[0] = CreateItem("Documentation Comment", Glyph.Snippet, default(TextSpan), null, descriptionText, insertionText, isSnippet: true);
		return array;
	}

	private void AddXmlElementItems(List<CompletionItem> items, XmlElementStartTagSyntax startTag, char triggerChar)
	{
		switch (startTag.Name.LocalName.ValueText)
		{
		case "list":
			items.AddRange(GetListItems(triggerChar));
			break;
		case "listheader":
			items.AddRange(GetListHeaderItems(triggerChar));
			break;
		case "item":
			items.AddRange(GetItemTagItems(triggerChar));
			break;
		}
	}

	private bool IsAttributeNameContext(SyntaxToken token, int position, out string? elementName, out ISet<string> attributeNames)
	{
		elementName = null;
		if (token.IsKind(SyntaxKind.XmlTextLiteralToken) && string.IsNullOrWhiteSpace(token.Text))
		{
			token = token.GetPreviousToken();
		}
		token = token.GetPreviousTokenIfTouchingWord(position);
		SyntaxList<XmlAttributeSyntax> syntaxList = default(SyntaxList<XmlAttributeSyntax>);
		if (token.IsKind(SyntaxKind.IdentifierToken) && token.Parent.IsKind(SyntaxKind.XmlName))
		{
			(elementName, syntaxList) = GetElementNameAndAttributes(token.Parent.Parent);
		}
		else if (token.Parent.IsKind(SyntaxKind.XmlNameAttribute) || token.Parent.IsKind(SyntaxKind.XmlTextAttribute))
		{
			XmlAttributeSyntax xmlAttributeSyntax = (XmlAttributeSyntax)token.Parent;
			if (token == xmlAttributeSyntax.EndQuoteToken)
			{
				(elementName, syntaxList) = GetElementNameAndAttributes(xmlAttributeSyntax.Parent);
			}
		}
		attributeNames = syntaxList.Select(GetAttributeName).ToSet();
		return elementName != null;
	}

	private (string? name, SyntaxList<XmlAttributeSyntax> attributes) GetElementNameAndAttributes(SyntaxNode node)
	{
		XmlNameSyntax xmlNameSyntax;
		SyntaxList<XmlAttributeSyntax> item;
		if (!(node is XmlEmptyElementSyntax xmlEmptyElementSyntax))
		{
			if (node is XmlElementSyntax xmlElementSyntax)
			{
				return GetElementNameAndAttributes(xmlElementSyntax.StartTag);
			}
			if (node is XmlElementStartTagSyntax xmlElementStartTagSyntax)
			{
				xmlNameSyntax = xmlElementStartTagSyntax.Name;
				item = xmlElementStartTagSyntax.Attributes;
			}
			else
			{
				xmlNameSyntax = null;
				item = default(SyntaxList<XmlAttributeSyntax>);
			}
		}
		else
		{
			xmlNameSyntax = xmlEmptyElementSyntax.Name;
			item = xmlEmptyElementSyntax.Attributes;
		}
		return (name: xmlNameSyntax?.LocalName.ValueText, attributes: item);
	}

	private bool IsAttributeValueContext(SyntaxToken token, out string? tagName, out string? attributeName)
	{
		XmlAttributeSyntax xmlAttributeSyntax = null;
		if (token.Parent.IsKind(SyntaxKind.IdentifierName) && token.Parent.IsParentKind(SyntaxKind.XmlNameAttribute))
		{
			xmlAttributeSyntax = (XmlNameAttributeSyntax)token.Parent.Parent;
		}
		else if (token.IsKind(SyntaxKind.XmlTextLiteralToken) && token.Parent.IsKind(SyntaxKind.XmlTextAttribute))
		{
			xmlAttributeSyntax = (XmlTextAttributeSyntax)token.Parent;
		}
		else if (token.Parent.IsKind(SyntaxKind.XmlNameAttribute) || token.Parent.IsKind(SyntaxKind.XmlTextAttribute))
		{
			xmlAttributeSyntax = (XmlAttributeSyntax)token.Parent;
			if (token != xmlAttributeSyntax.StartQuoteToken)
			{
				xmlAttributeSyntax = null;
			}
		}
		if (xmlAttributeSyntax != null)
		{
			attributeName = xmlAttributeSyntax.Name.LocalName.ValueText;
			XmlEmptyElementSyntax ancestor = xmlAttributeSyntax.GetAncestor<XmlEmptyElementSyntax>();
			if (ancestor != null)
			{
				tagName = ancestor.Name.LocalName.Text;
				return true;
			}
			XmlElementStartTagSyntax ancestor2 = Microsoft.Dynamics.Nav.CodeAnalysis.Syntax.SyntaxTokenExtensions.GetAncestor<XmlElementStartTagSyntax>(token);
			if (ancestor2 != null)
			{
				tagName = ancestor2.Name.LocalName.Text;
				return true;
			}
		}
		attributeName = null;
		tagName = null;
		return false;
	}

	protected override IEnumerable<string> GetKeywordNames()
	{
		yield return SyntaxKind.TrueKeyword.GetText();
		yield return SyntaxKind.FalseKeyword.GetText();
	}

	protected override IEnumerable<string> GetExistingTopLevelElementNames(DocumentationCommentTriviaSyntax syntax)
	{
		return syntax.Content.Select(GetElementName).WhereNotNull();
	}

	protected override IEnumerable<string> GetExistingTopLevelAttributeValues(DocumentationCommentTriviaSyntax syntax, string elementName, string attributeName)
	{
		string attributeName2 = attributeName;
		IEnumerable<string> enumerable = SpecializedCollections.EmptyEnumerable<string>();
		SyntaxList<XmlNodeSyntax>.Enumerator enumerator = syntax.Content.GetEnumerator();
		while (enumerator.MoveNext())
		{
			XmlNodeSyntax current = enumerator.Current;
			var (text, syntaxList) = GetElementNameAndAttributes(current);
			if (text == elementName)
			{
				enumerable = enumerable.Concat<string>(syntaxList.Where((XmlAttributeSyntax attribute) => GetAttributeName(attribute) == attributeName2).Select(GetAttributeValue));
			}
		}
		return enumerable;
	}

	private string? GetElementName(XmlNodeSyntax node)
	{
		return GetElementNameAndAttributes(node).name;
	}

	private string GetAttributeName(XmlAttributeSyntax attribute)
	{
		return attribute.Name.LocalName.ValueText;
	}

	private string? GetAttributeValue(XmlAttributeSyntax attribute)
	{
		if (!(attribute is XmlTextAttributeSyntax xmlTextAttributeSyntax))
		{
			if (attribute is XmlNameAttributeSyntax xmlNameAttributeSyntax)
			{
				return xmlNameAttributeSyntax.Identifier.Identifier.ValueText;
			}
			return null;
		}
		return xmlTextAttributeSyntax.TextTokens.GetValueText();
	}

	protected static CompletionItem CreateItem(string displayText, Glyph glyph, TextSpan span = default(TextSpan), string? documentationText = null, string? descriptionText = null, string? insertionText = null, bool isSnippet = false)
	{
		string insertionText2 = insertionText ?? displayText;
		Glyph? glyph2 = glyph;
		return CommonCompletionItem.Create(displayText, span, glyph2, descriptionText, documentationText, null, null, null, insertionText2, null, preselect: false, showsWarningIcon: false, shouldFormatOnCommit: false, isArgumentName: false, isSnippet);
	}
}
