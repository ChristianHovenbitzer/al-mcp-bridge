using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Completion;

internal sealed class MemberNameRecommender : TextRecommender
{
	private const string NameAndSourceSeparator = ";";

	protected internal override async Task<IEnumerable<CompletionItem>> RecommendTextAsync(CompletionContext context, MemberSyntaxContext syntaxContext, CancellationToken cancellationToken)
	{
		if (!syntaxContext.General.HasFlag(GeneralContexts.MemberName))
		{
			return await base.RecommendTextAsync(context, syntaxContext, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		}
		SyntaxNode syntaxNode = GoToParentIfIdentifier(syntaxContext.LeftToken.Parent);
		bool hasToHaveClsCompliantName = syntaxNode.Kind.NameMustBeClsCompliant();
		IEnumerable<ISymbol> first = syntaxContext.SemanticModel.LookupSymbols(syntaxContext.LeftToken.SpanStart, LookupOptions.Default, null, null, SymbolKind.Undefined, cancellationToken);
		first = first.Concat(GetTableSymbolsIfExpected(syntaxContext, syntaxNode));
		IEnumerable<ISymbol> enumerable2;
		if (!syntaxContext.General.HasFlag(GeneralContexts.AnyExpression))
		{
			IEnumerable<ISymbol> enumerable = first;
			enumerable2 = enumerable;
		}
		else
		{
			enumerable2 = first.Where(SemanticFacts.IsValidForExpression);
		}
		first = enumerable2;
		bool isSourceMissing;
		TextSpan spanForReplacement = GetSpanForReplacement(syntaxNode, syntaxContext.LeftToken, out isSourceMissing);
		first = first.Where(IsRelevantSymbol);
		IEnumerable<CompletionItem> enumerable3 = first.Select((ISymbol s) => CreateItem(s, spanForReplacement, hasToHaveClsCompliantName, isSourceMissing));
		Binder enclosingBinder = syntaxContext.SemanticModel.GetEnclosingBinder(syntaxContext.LeftToken.SpanStart);
		if (enclosingBinder != null && !enclosingBinder.AreImplicitWithEnabled(syntaxContext.LeftToken))
		{
			foreach (VariableSymbol v in first.Where((ISymbol s) => s.Kind == SymbolKind.GlobalVariable && ((VariableSymbol)s).IsSynthesized && ((SynthesizedGlobalVariableSymbol)s).HasImplicitWith).Cast<VariableSymbol>())
			{
				first = enclosingBinder.GetMemberSymbolsFromType(v.Type);
				IEnumerable<ISymbol> enumerable4;
				if (!syntaxContext.General.HasFlag(GeneralContexts.AnyExpression))
				{
					IEnumerable<ISymbol> enumerable = first;
					enumerable4 = enumerable;
				}
				else
				{
					enumerable4 = first.Where(SemanticFacts.IsValidForExpression);
				}
				first = enumerable4;
				first = first.Where(IsRelevantSymbol);
				enumerable3 = enumerable3.Concat(first.Select((ISymbol s) => CreateItem(s, spanForReplacement, hasToHaveClsCompliantName, isSourceMissing, v.Name + ".")));
			}
		}
		return enumerable3;
	}

	private static bool IsRelevantSymbol(ISymbol s)
	{
		if (string.IsNullOrEmpty(GetName(s)))
		{
			return false;
		}
		if (!s.IsKind(SymbolKind.Field, SymbolKind.Table, SymbolKind.GlobalVariable, SymbolKind.LocalVariable))
		{
			if (s.IsKind(SymbolKind.Method))
			{
				return ((MethodSymbol)s).MethodKind == MethodKind.Method;
			}
			return false;
		}
		return true;
	}

	private static IEnumerable<ISymbol> GetTableSymbolsIfExpected(MemberSyntaxContext context, SyntaxNode parentOfInterest)
	{
		SyntaxKind kind = parentOfInterest.Kind;
		if (kind == SyntaxKind.ReportDataItem || kind == SyntaxKind.XmlPortTableElement || kind == SyntaxKind.QueryDataItem)
		{
			return context.SemanticModel.Compilation.GetApplicationObjectTypeSymbolsAcrossModules(SymbolKind.Table, accessibleOnly: true);
		}
		return Enumerable.Empty<ISymbol>();
	}

	private static string? GetName(ISymbol s)
	{
		return s.Kind switch
		{
			SymbolKind.ReportDataItem => ((ReportDataItemSymbol)s).RelatedTable?.Name, 
			SymbolKind.XmlPortNode => ((SourceXmlPortNodeSymbol)s).Type?.Name, 
			SymbolKind.QueryDataItem => ((SourceQueryDataItemSymbol)s).RelatedTable?.Name, 
			_ => s.Name, 
		};
	}

	private static SyntaxNode GoToParentIfIdentifier(SyntaxNode node)
	{
		if (node.IsKind(SyntaxKind.IdentifierName))
		{
			return node.Parent;
		}
		return node;
	}

	private static CompletionItem CreateItem(ISymbol symbol, TextSpan span, bool nameMustBeClsCompliant, bool isSourceMissing, string? sourcePrefix = null)
	{
		string name = GetName(symbol);
		string value = (nameMustBeClsCompliant ? FixNameIfNotClsCompliant(name) : name);
		value = value.QuoteIdentifierIfNeeded();
		if (sourcePrefix == null)
		{
			sourcePrefix = string.Empty;
		}
		string text = (isSourceMissing ? (value + ";" + sourcePrefix + name.QuoteIdentifierIfNeeded()) : value);
		Glyph? glyph = symbol.GetGlyph();
		string insertionText = text;
		string obsoleteInformationMessage = ObsoleteSymbolHelper.GetObsoleteInformationMessage(symbol);
		return CommonCompletionItem.Create(name, span, glyph, null, null, null, null, null, insertionText, obsoleteInformationMessage);
	}

	private static string FixNameIfNotClsCompliant(string name)
	{
		if (name.IsValidClsIdentifier())
		{
			return name;
		}
		return Microsoft.Dynamics.Nav.CodeAnalysis.Utilities.StringExtensions.GetValidClsIdentifierByUsingUnderscores(name);
	}

	private static TextSpan GetSpanForReplacement(SyntaxNode parentOfInterest, SyntaxToken leftToken, out bool isSourceMissing)
	{
		SyntaxToken openParenthesisToken;
		SyntaxToken semicolonToken;
		SyntaxNode syntaxNode;
		SyntaxToken closeParenthesisToken;
		switch (parentOfInterest.Kind)
		{
		case SyntaxKind.PageField:
		{
			PageFieldSyntax obj7 = (PageFieldSyntax)parentOfInterest;
			openParenthesisToken = obj7.OpenParenthesisToken;
			semicolonToken = obj7.SemicolonToken;
			syntaxNode = obj7.Expression;
			closeParenthesisToken = obj7.CloseParenthesisToken;
			break;
		}
		case SyntaxKind.XmlPortTableElement:
		{
			XmlPortTableElementSyntax obj6 = (XmlPortTableElementSyntax)parentOfInterest;
			openParenthesisToken = obj6.OpenParenthesisToken;
			semicolonToken = obj6.SemicolonToken;
			syntaxNode = obj6.SourceTable;
			closeParenthesisToken = obj6.CloseParenthesisToken;
			break;
		}
		case SyntaxKind.ReportDataItem:
		{
			ReportDataItemSyntax obj5 = (ReportDataItemSyntax)parentOfInterest;
			openParenthesisToken = obj5.OpenParenthesisToken;
			semicolonToken = obj5.SemicolonToken;
			syntaxNode = obj5.DataItemTable;
			closeParenthesisToken = obj5.CloseParenthesisToken;
			break;
		}
		case SyntaxKind.ReportColumn:
		{
			ReportColumnSyntax obj4 = (ReportColumnSyntax)parentOfInterest;
			openParenthesisToken = obj4.OpenParenthesisToken;
			semicolonToken = obj4.SemicolonToken;
			syntaxNode = obj4.SourceExpression;
			closeParenthesisToken = obj4.CloseParenthesisToken;
			break;
		}
		case SyntaxKind.QueryDataItem:
		{
			QueryDataItemSyntax obj3 = (QueryDataItemSyntax)parentOfInterest;
			openParenthesisToken = obj3.OpenParenthesisToken;
			semicolonToken = obj3.SemicolonToken;
			syntaxNode = obj3.DataItemTable;
			closeParenthesisToken = obj3.CloseParenthesisToken;
			break;
		}
		case SyntaxKind.QueryColumn:
		{
			QueryColumnSyntax obj2 = (QueryColumnSyntax)parentOfInterest;
			openParenthesisToken = obj2.OpenParenthesisToken;
			semicolonToken = obj2.SemicolonToken;
			syntaxNode = obj2.RelatedField;
			closeParenthesisToken = obj2.CloseParenthesisToken;
			break;
		}
		case SyntaxKind.QueryFilter:
		{
			QueryFilterSyntax obj = (QueryFilterSyntax)parentOfInterest;
			openParenthesisToken = obj.OpenParenthesisToken;
			semicolonToken = obj.SemicolonToken;
			syntaxNode = obj.RelatedField;
			closeParenthesisToken = obj.CloseParenthesisToken;
			break;
		}
		default:
			isSourceMissing = false;
			return default(TextSpan);
		}
		if (closeParenthesisToken.IsMissing || closeParenthesisToken.IsKind(SyntaxKind.None))
		{
			if (semicolonToken.IsMissing || semicolonToken.IsKind(SyntaxKind.None))
			{
				isSourceMissing = true;
				return GetSpanFromPosToPos(openParenthesisToken.SpanEnd, leftToken.SpanEnd);
			}
			isSourceMissing = true;
			return GetSpanFromPosToPos(openParenthesisToken.SpanEnd, semicolonToken.SpanEnd);
		}
		if (semicolonToken.IsMissing || semicolonToken.IsKind(SyntaxKind.None))
		{
			isSourceMissing = true;
			return GetSpanFromPosToPos(openParenthesisToken.SpanEnd, leftToken.SpanEnd);
		}
		isSourceMissing = syntaxNode == null || syntaxNode.IsMissing || syntaxNode.IsKind(SyntaxKind.None);
		return GetSpanFromPosToPos(openParenthesisToken.SpanEnd, isSourceMissing ? semicolonToken.SpanEnd : semicolonToken.SpanStart);
	}

	private static TextSpan GetSpanFromPosToPos(int start, int end)
	{
		return new TextSpan(start, end - start);
	}
}
