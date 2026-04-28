using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Options;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Completion;

internal class SymbolCompletionProvider : AbstractSymbolCompletionProvider
{
	internal override bool IsDebuggerConsoleProvider => true;

	protected override Task<IEnumerable<ISymbol>> GetSymbolsWorker(AbstractSyntaxContext context, OptionSet options, CancellationToken cancellationToken)
	{
		return Recommender.GetRecommendedSymbolsAtPositionAsync(context, options, cancellationToken);
	}

	protected sealed override string GetDisplayText(ISymbol symbol, AbstractSyntaxContext context, char ch)
	{
		return GetDisplayText(symbol, context);
	}

	protected virtual string GetDisplayText(ISymbol symbol, AbstractSyntaxContext context)
	{
		return symbol.ToDisplayString(SymbolDisplayFormat.SymbolCompletionFormat);
	}

	internal override bool IsInsertionTrigger(SourceText text, int characterPosition, OptionSet options)
	{
		return CompletionUtilities.IsTriggerCharacter(text, characterPosition, options);
	}

	protected override async Task<bool> IsSemanticTriggerCharacterAsync(Document document, int characterPosition, CancellationToken cancellationToken)
	{
		bool? flag = await IsTriggerOnDotAsync(document, characterPosition, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		if (flag.HasValue)
		{
			return flag.Value;
		}
		return true;
	}

	private async Task<bool?> IsTriggerOnDotAsync(Document document, int characterPosition, CancellationToken cancellationToken)
	{
		if ((await document.GetTextAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false))[characterPosition] != '.')
		{
			return null;
		}
		SyntaxToken syntaxToken = (await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false)).FindToken(characterPosition);
		if (syntaxToken.Kind == SyntaxKind.DotToken)
		{
			syntaxToken = syntaxToken.GetPreviousToken();
		}
		return !syntaxToken.Kind.IsNumericLiteral();
	}

	protected override (string displayText, string insertText, bool isSnippet, string obsoletionText, string? descriptionText, int hashCode) GetDisplayAndInsertionText(ISymbol symbol, AbstractSyntaxContext context)
	{
		bool num = context.EnclosingBinder.IsSymbolInScope(symbol);
		string displayText = GetDisplayText(symbol, context);
		bool isSnippet;
		string insertionText = GetInsertionText(symbol, context, out isSnippet);
		string text = symbol.ToDisplayString(SymbolDisplayFormat.QualifiedNameOnlyFormat);
		string text2 = ((!num) ? text : displayText);
		string obsoleteInformationMessage = ObsoleteSymbolHelper.GetObsoleteInformationMessage(symbol);
		return new ValueTuple<string, string, bool, string, string, int>(item6: symbol.Kind.IsApplicationObject() ? text.GetHashCode() : ((symbol.Kind != SymbolKind.Method) ? Hash.Combine(displayText?.GetHashCode() ?? 0, insertionText?.GetHashCode() ?? 0, isSnippet.GetHashCode(), obsoleteInformationMessage?.GetHashCode() ?? 0, text2?.GetHashCode() ?? 0) : GetMethodHashCode(displayText, symbol)), item1: displayText, item2: insertionText, item3: isSnippet, item4: obsoleteInformationMessage, item5: text2);
	}

	private static int GetMethodHashCode(string displayText, ISymbol symbol)
	{
		return Hash.Combine(displayText.GetHashCode(), symbol.ContainingSymbol?.GetHashCode() ?? 0);
	}

	protected override CompletionItemRules GetCompletionItemRules(IReadOnlyList<ISymbol> symbols, AbstractSyntaxContext context)
	{
		return CompletionItemRules.Default;
	}

	private string GetInsertionText(ISymbol symbol, AbstractSyntaxContext context, out bool isSnippet)
	{
		isSnippet = false;
		string text = symbol.Name.QuoteIdentifierIfNeeded();
		if (!symbol.IsKind(SymbolKind.Method))
		{
			if (symbol.IsKind(SymbolKind.Key))
			{
				return symbol.ToDisplayString(SymbolDisplayFormat.NameOnlyFormat);
			}
			return text;
		}
		MethodSymbol methodSymbol = (MethodSymbol)symbol;
		MethodKind methodKind = methodSymbol.MethodKind;
		if (methodKind != MethodKind.BuiltInMethod)
		{
			if (methodKind == MethodKind.Property)
			{
				goto IL_0058;
			}
		}
		else if (((BuiltInMethodTypeSymbol)methodSymbol).IsProperty)
		{
			goto IL_0058;
		}
		MemberSyntaxContext obj = context as MemberSyntaxContext;
		if (obj != null && obj.General == GeneralContexts.AttributeArgumentList)
		{
			return text;
		}
		bool flag = methodSymbol.ReturnType.NavTypeKind == NavTypeKind.None;
		if (methodSymbol.ParameterCount == 0)
		{
			if (!flag)
			{
				return text + "()";
			}
			return text + "();";
		}
		isSnippet = true;
		if (!flag)
		{
			return text + "($0)";
		}
		return text + "($0);";
		IL_0058:
		return text;
	}
}
