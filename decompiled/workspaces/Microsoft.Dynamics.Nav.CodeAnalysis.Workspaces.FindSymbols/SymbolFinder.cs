using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.FindReferences;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Host;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.LanguageServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Log;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Shared.Extensions;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.FindSymbols;

internal static class SymbolFinder
{
	private class FindMemberSymbol : BoundTreeWalkerWithStackGuard
	{
		private readonly ITypeSymbol containingSymbol;

		public Symbol? Found { get; private set; }

		public FindMemberSymbol(ITypeSymbol containingSymbol)
		{
			this.containingSymbol = containingSymbol;
		}

		public override BoundNode VisitFieldAccess(BoundFieldAccess node)
		{
			if (containingSymbol == node.FieldSymbol.GetBaseApplicationObjectSymbol())
			{
				Found = node.FieldSymbol;
				return null;
			}
			return base.VisitFieldAccess(node);
		}
	}

	public static async Task<ISymbol> FindSymbolAtPositionAsync(Document document, int position, CancellationToken cancellationToken = default(CancellationToken))
	{
		return await FindSymbolAtPositionAsync(await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false), position, document.Project.Solution.Workspace, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
	}

	public static async Task<ISymbol> FindSymbolAtPositionAsync(SemanticModel semanticModel, int position, Workspace workspace, CancellationToken cancellationToken = default(CancellationToken))
	{
		return (await GetSemanticInfoAtPositionAsync(semanticModel, position, workspace, cancellationToken).ConfigureAwait(continueOnCapturedContext: false)).GetAnySymbol(includeType: false);
	}

	internal static async Task<TokenSemanticInfo> GetSemanticInfoAtPositionAsync(SemanticModel semanticModel, int position, Workspace workspace, CancellationToken cancellationToken)
	{
		SyntaxTree syntaxTree = semanticModel.SyntaxTree;
		ISyntaxFactsService service = workspace.Services.GetLanguageServices("AL").GetService<ISyntaxFactsService>();
		SyntaxToken syntaxToken = await Microsoft.Dynamics.Nav.CodeAnalysis.Syntax.SyntaxTreeExtensions.GetTouchingTokenAsync(syntaxTree, position, service.IsBindableToken, cancellationToken, findInsideTrivia: true).ConfigureAwait(continueOnCapturedContext: false);
		if (syntaxToken != default(SyntaxToken) && syntaxToken.Span.IntersectsWith(position))
		{
			return GetSemanticInfo(semanticModel, syntaxToken, workspace, cancellationToken);
		}
		return TokenSemanticInfo.Empty;
	}

	public static TokenSemanticInfo GetSemanticInfo(SemanticModel semanticModel, SyntaxToken token, Workspace workspace, CancellationToken cancellationToken)
	{
		AbstractHostLanguageServices languageServices = workspace.Services.GetLanguageServices("AL");
		ISyntaxFactsService service = languageServices.GetService<ISyntaxFactsService>();
		if (!service.IsBindableToken(token))
		{
			return TokenSemanticInfo.Empty;
		}
		ISemanticFactsService service2 = languageServices.GetService<ISemanticFactsService>();
		return GetSemanticInfo(semanticModel, service2, service, token, cancellationToken);
	}

	private static TokenSemanticInfo GetSemanticInfo(SemanticModel semanticModel, ISemanticFactsService semanticFacts, ISyntaxFactsService syntaxFacts, SyntaxToken token, CancellationToken cancellationToken)
	{
		SyntaxNode bindableParent = syntaxFacts.GetBindableParent(token);
		ITypeSymbol type = semanticModel.GetTypeInfo(bindableParent, cancellationToken).Type;
		ISymbol declaredSymbol = semanticFacts.GetDeclaredSymbol(semanticModel, token, cancellationToken);
		ImmutableArray<ISymbol> allSymbols = (from s in semanticModel.GetSymbolInfo(bindableParent, cancellationToken).GetBestOrAllSymbols()
			where !s.Equals(declaredSymbol)
			select s).ToImmutableArray();
		allSymbols = AdjustContainingSymbolForBuiltInMethods(semanticModel, bindableParent, declaredSymbol, allSymbols, cancellationToken);
		return new TokenSemanticInfo(declaredSymbol, allSymbols, type);
	}

	internal static bool OriginalSymbolsMatch(ISymbol searchSymbol, ISymbol? symbolToMatch, SyntaxNode syntaxNode, SemanticModel semanticModel, CancellationToken cancellationToken)
	{
		if (symbolToMatch == null)
		{
			return false;
		}
		if (searchSymbol == symbolToMatch)
		{
			return true;
		}
		if (IsReferenceInUserControl(searchSymbol, symbolToMatch) || IsReferenceInUserControl(symbolToMatch, searchSymbol))
		{
			return true;
		}
		if (IsTriggerReferenceForTrigger(searchSymbol, symbolToMatch))
		{
			return true;
		}
		if (IsBuiltInMethodReferenceForTrigger(searchSymbol, symbolToMatch, syntaxNode, semanticModel, cancellationToken))
		{
			return true;
		}
		if (IsBuiltInMethodReferenceForBuiltInMethod(searchSymbol, symbolToMatch, syntaxNode, semanticModel, cancellationToken))
		{
			return true;
		}
		if (IsTriggerOrTriggerEventReferenceForBuiltInMethod(searchSymbol, symbolToMatch))
		{
			return true;
		}
		return false;
	}

	private static bool IsReferenceInUserControl(ISymbol declaration, ISymbol method)
	{
		if (!declaration.ContainingSymbol.IsKind(SymbolKind.ControlAddIn) || !method.IsKind(SymbolKind.Method) || !method.ContainingSymbol.IsKind(SymbolKind.Control))
		{
			return false;
		}
		ControlSymbol controlSymbol = (ControlSymbol)method.ContainingSymbol;
		if (controlSymbol.ControlKind != ControlKind.UserControl)
		{
			return false;
		}
		return declaration.ContainingSymbol.Equals(controlSymbol.RelatedControlAddInSymbol);
	}

	private static bool IsTriggerReferenceForTrigger(ISymbol triggerMethod, ISymbol otherTrigger)
	{
		if (!ValidateMethodKinds(triggerMethod, MethodKind.Trigger, otherTrigger, MethodKind.Trigger, out MethodSymbol searchMethod, out MethodSymbol methodToMatch))
		{
			return false;
		}
		if (!TriggerReferenceHelpers.GetRelatedTriggerSymbolNames(searchMethod).Contains(methodToMatch.Name) && !SemanticFacts.IsSameName(triggerMethod.Name, methodToMatch.Name))
		{
			return false;
		}
		ISymbol adjustedContainingSymbol = TriggerReferenceHelpers.GetAdjustedContainingSymbol(searchMethod);
		if (adjustedContainingSymbol == null)
		{
			return false;
		}
		ISymbol adjustedContainingSymbol2 = TriggerReferenceHelpers.GetAdjustedContainingSymbol(methodToMatch);
		if (adjustedContainingSymbol2 != null)
		{
			return adjustedContainingSymbol == adjustedContainingSymbol2;
		}
		return false;
	}

	private static bool IsBuiltInMethodReferenceForBuiltInMethod(ISymbol builtInMethod, ISymbol otherBuiltInMethod, SyntaxNode syntaxNode, SemanticModel semanticModel, CancellationToken cancellationToken)
	{
		if (!ValidateMethodKinds(builtInMethod, MethodKind.BuiltInMethod, otherBuiltInMethod, MethodKind.BuiltInMethod, out MethodSymbol searchMethod, out MethodSymbol methodToMatch))
		{
			return false;
		}
		ISymbol adjustedContainingSymbol = TriggerReferenceHelpers.GetAdjustedContainingSymbol(searchMethod);
		if (adjustedContainingSymbol == null || adjustedContainingSymbol.Kind == SymbolKind.Class)
		{
			return false;
		}
		if (!BuiltInMethodReferenceHelper.GetRelatedBuiltInMethodNames(searchMethod).Contains(methodToMatch.Name) && !SemanticFacts.IsSameName(searchMethod.Name, methodToMatch.Name))
		{
			return false;
		}
		Symbol containingSymbolFromSyntaxNode = GetContainingSymbolFromSyntaxNode(syntaxNode, semanticModel, (BuiltInMethodTypeSymbol)methodToMatch, cancellationToken);
		if (containingSymbolFromSyntaxNode == null)
		{
			return false;
		}
		return containingSymbolFromSyntaxNode == adjustedContainingSymbol;
	}

	private static bool IsBuiltInMethodReferenceForTrigger(ISymbol triggerMethod, ISymbol builtInMethod, SyntaxNode syntaxNode, SemanticModel semanticModel, CancellationToken cancellationToken)
	{
		if (!ValidateMethodKinds(triggerMethod, MethodKind.Trigger, builtInMethod, MethodKind.BuiltInMethod, out MethodSymbol searchMethod, out MethodSymbol methodToMatch))
		{
			return false;
		}
		ISymbol adjustedContainingSymbol = TriggerReferenceHelpers.GetAdjustedContainingSymbol(searchMethod);
		if (adjustedContainingSymbol == null)
		{
			return false;
		}
		if (!TriggerReferenceHelpers.GetRelatedBuiltInMethodNames(searchMethod).Contains(methodToMatch.Name))
		{
			return false;
		}
		if (syntaxNode.Parent.Kind == SyntaxKind.MethodDeclaration || syntaxNode.Parent.Kind == SyntaxKind.TriggerDeclaration)
		{
			return false;
		}
		BoundCall boundCall = semanticModel.GetOperation(syntaxNode, cancellationToken) as BoundCall;
		Symbol containingSymbolFromSyntaxNode = GetContainingSymbolFromSyntaxNode(syntaxNode, semanticModel, (BuiltInMethodTypeSymbol)methodToMatch, cancellationToken);
		if (containingSymbolFromSyntaxNode == null)
		{
			return false;
		}
		if (containingSymbolFromSyntaxNode == adjustedContainingSymbol)
		{
			if (boundCall != null)
			{
				return !HasRunTriggerParameterSetToFalse(boundCall);
			}
			return true;
		}
		return false;
	}

	private static bool IsTriggerOrTriggerEventReferenceForBuiltInMethod(ISymbol builtInMethod, ISymbol triggerOrTriggerEventMethod)
	{
		if (!ValidateMethodKinds(builtInMethod, MethodKind.BuiltInMethod, triggerOrTriggerEventMethod, MethodKind.Trigger, out MethodSymbol searchMethod, out MethodSymbol methodToMatch))
		{
			return false;
		}
		if (!BuiltInMethodReferenceHelper.GetRelatedTriggerAndTriggerEventSymbolNames(searchMethod).Contains(methodToMatch.Name))
		{
			return false;
		}
		ISymbol adjustedContainingSymbol = TriggerReferenceHelpers.GetAdjustedContainingSymbol(searchMethod);
		if (adjustedContainingSymbol == null || adjustedContainingSymbol.Kind == SymbolKind.Class)
		{
			return false;
		}
		ISymbol adjustedContainingSymbol2 = TriggerReferenceHelpers.GetAdjustedContainingSymbol(methodToMatch);
		if (adjustedContainingSymbol2 != null)
		{
			return adjustedContainingSymbol == adjustedContainingSymbol2;
		}
		return false;
	}

	private static bool ValidateMethodKinds(ISymbol searchSymbol, MethodKind searchSymbolKind, ISymbol symbolToMatch, MethodKind symbolToMatchKind, out MethodSymbol searchMethod, out MethodSymbol methodToMatch)
	{
		searchMethod = null;
		methodToMatch = null;
		if (searchSymbol.Kind != SymbolKind.Method || symbolToMatch.Kind != SymbolKind.Method)
		{
			return false;
		}
		searchMethod = (MethodSymbol)searchSymbol;
		if (searchMethod.MethodKind != searchSymbolKind)
		{
			return false;
		}
		methodToMatch = (MethodSymbol)symbolToMatch;
		return methodToMatch.MethodKind == symbolToMatchKind;
	}

	private static ImmutableArray<ISymbol> AdjustContainingSymbolForBuiltInMethods(SemanticModel semanticModel, SyntaxNode bindableParent, ISymbol declaredSymbol, ImmutableArray<ISymbol> allSymbols, CancellationToken cancellationToken)
	{
		if (declaredSymbol == null && allSymbols.Length == 1 && allSymbols[0].Kind == SymbolKind.Method && ((MethodSymbol)allSymbols[0]).MethodKind == MethodKind.BuiltInMethod)
		{
			BuiltInMethodTypeSymbol builtInMethodTypeSymbol = (BuiltInMethodTypeSymbol)allSymbols[0];
			Symbol containingSymbolFromSyntaxNode = GetContainingSymbolFromSyntaxNode(bindableParent, semanticModel, builtInMethodTypeSymbol, cancellationToken);
			if (containingSymbolFromSyntaxNode != null && containingSymbolFromSyntaxNode.Kind != SymbolKind.Class)
			{
				return ImmutableArray.Create((ISymbol)new SynthesizedObjectSpecificBuiltInMethodSymbol(containingSymbolFromSyntaxNode, builtInMethodTypeSymbol));
			}
		}
		return allSymbols;
	}

	private static Symbol? GetContainingSymbolFromSyntaxNode(SyntaxNode syntaxNode, SemanticModel semanticModel, BuiltInMethodTypeSymbol symbol, CancellationToken cancellationToken)
	{
		if (symbol.ContainingSymbol.Kind == SymbolKind.Class && ((LanguageClassTypeSymbol)symbol.ContainingSymbol).ImplicitMemberAccess)
		{
			return symbol.ContainingSymbol;
		}
		Symbol callReceiverOptTypeSymbol = GetCallReceiverOptTypeSymbol(semanticModel.GetOperation(syntaxNode, cancellationToken) as BoundCall);
		if (callReceiverOptTypeSymbol != null)
		{
			return callReceiverOptTypeSymbol;
		}
		SyntaxNode syntaxNode2 = syntaxNode.GetFirstParent(SyntaxKind.Block)?.Parent;
		if (syntaxNode2 != null)
		{
			IApplicationObjectTypeSymbol applicationObjectTypeSymbol = semanticModel.GetDeclaredSymbol(syntaxNode2, cancellationToken)?.GetContainingApplicationObjectTypeSymbol();
			if (applicationObjectTypeSymbol != null && applicationObjectTypeSymbol.Kind.IsExtensionOrCustomizationObject())
			{
				applicationObjectTypeSymbol = ((ApplicationObjectExtensionTypeSymbol)applicationObjectTypeSymbol).Target;
			}
			return applicationObjectTypeSymbol as Symbol;
		}
		return null;
	}

	private static Symbol? GetCallReceiverOptTypeSymbol(BoundCall? boundCall)
	{
		if (boundCall?.ReceiverOpt == null)
		{
			return null;
		}
		if (TryGetTestSymbolUnderlyingSymbol(boundCall.ReceiverOpt, out Symbol testReferenceSymbol))
		{
			return testReferenceSymbol;
		}
		TypeSymbol typeSymbol = boundCall.ReceiverOpt.MemberType?.OriginalDefinition;
		if (typeSymbol != null)
		{
			int parameterIndex = GetParameterIndex(boundCall.Method, (ParameterSymbol x) => x is BuiltInParameterSymbol builtInParameterSymbol && builtInParameterSymbol.ContainingTypeResolver);
			if (parameterIndex != -1 && parameterIndex < boundCall.Arguments.Length)
			{
				return GetMemberReferenceInCallArguments(boundCall.Arguments[parameterIndex], typeSymbol);
			}
		}
		return typeSymbol;
	}

	private static bool TryGetTestSymbolUnderlyingSymbol(BoundExpression receiverOpt, out Symbol? testReferenceSymbol)
	{
		switch (receiverOpt.Kind)
		{
		case BoundKind.TestActionAccess:
			testReferenceSymbol = ((BoundTestActionAccess)receiverOpt).ActionSymbol.ActionSymbol;
			return true;
		case BoundKind.TestFieldAccess:
			testReferenceSymbol = ((BoundTestFieldAccess)receiverOpt).FieldSymbol.ControlSymbol;
			return true;
		case BoundKind.TestPartAccess:
			testReferenceSymbol = ((BoundTestPartAccess)receiverOpt).PartSymbol.ControlSymbol;
			return true;
		default:
			testReferenceSymbol = null;
			return false;
		}
	}

	private static bool HasRunTriggerParameterSetToFalse(BoundCall boundCall)
	{
		int parameterIndex = GetParameterIndex(boundCall.Method, (ParameterSymbol x) => SemanticFacts.IsSameName(x.Name, "RunTrigger"));
		if (parameterIndex != -1 && parameterIndex < boundCall.Arguments.Length)
		{
			ConstantValue? constantValue = boundCall.Arguments[parameterIndex].ConstantValue;
			if ((object)constantValue == null)
			{
				return false;
			}
			return !constantValue.BooleanValue;
		}
		return false;
	}

	private static int GetParameterIndex(MethodSymbol method, Func<ParameterSymbol, bool> filter)
	{
		for (int i = 0; i < method.ParameterCount; i++)
		{
			if (filter(method.Parameters[i]))
			{
				return i;
			}
		}
		return -1;
	}

	private static Symbol? GetMemberReferenceInCallArguments(BoundExpression boundExpression, ITypeSymbol containingSymbol)
	{
		FindMemberSymbol findMemberSymbol = new FindMemberSymbol(containingSymbol);
		findMemberSymbol.Visit(boundExpression);
		return findMemberSymbol.Found;
	}

	public static async Task<IEnumerable<ReferencedSymbol>> FindReferencesAsync(ISymbol symbol, Solution solution, CancellationToken cancellationToken = default(CancellationToken))
	{
		StreamingProgressCollector progressCollector = new StreamingProgressCollector(StreamingFindReferencesProgress.Instance);
		await FindReferencesAsync(SymbolAndProjectId.Create(symbol, null), solution, progressCollector, null, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		return progressCollector.GetReferencedSymbols();
	}

	public static async Task<IEnumerable<ReferencedSymbol>> FindReferencesAsync(ISymbol symbol, Solution solution, IFindReferencesProgress progress, IImmutableSet<Document> documents, CancellationToken cancellationToken = default(CancellationToken))
	{
		StreamingProgressCollector streamingProgress = new StreamingProgressCollector(new StreamingFindReferencesProgressAdapter(progress));
		await FindReferencesAsync(SymbolAndProjectId.Create(symbol, null), solution, streamingProgress, documents, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		return streamingProgress.GetReferencedSymbols();
	}

	internal static async Task FindReferencesAsync(SymbolAndProjectId symbolAndProjectId, Solution solution, IStreamingFindReferencesProgress progress, IImmutableSet<Document> documents, CancellationToken cancellationToken)
	{
		using (Logger.LogBlock(FunctionId.FindReference, cancellationToken))
		{
			await FindReferencesInCurrentProcessAsync(symbolAndProjectId, solution, progress, documents, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		}
	}

	internal static Task FindReferencesInCurrentProcessAsync(SymbolAndProjectId symbolAndProjectId, Solution solution, IStreamingFindReferencesProgress progress, IImmutableSet<Document> documents, CancellationToken cancellationToken)
	{
		ImmutableArray<IReferenceFinder> defaultReferenceFinders = ReferenceFinders.DefaultReferenceFinders;
		progress = progress ?? StreamingFindReferencesProgress.Instance;
		return new FindReferencesSearchEngine(solution, documents, defaultReferenceFinders, progress, cancellationToken).FindReferencesAsync(symbolAndProjectId);
	}
}
